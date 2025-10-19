using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text.Json;
using System.Collections.Concurrent;

namespace SiteAndIPBlocker;

public class SiteAndIPBlockerConfig : BasePluginConfig
{
    [JsonPropertyName("whitelist")]
    public List<string> Whitelist { get; set; } = new List<string>();

    [JsonPropertyName("block_message")]
    public string BlockMessage { get; set; } = "{darkred}Blocked: Sending IP addresses or websites is not allowed.";
    
    [JsonPropertyName("name_action")]
    public int NameAction { get; set; } = 0;

    [JsonPropertyName("rename_message")]
    public string RenameMessage { get; set; } = "{darkred}Your name contains a blocked IP address or website. It will be renamed.";

    [JsonPropertyName("admin_immunity")]
    public int AdminImmunity { get; set; } = 0;

    [JsonPropertyName("log_path")]
    public string LogPath { get; set; } = "addons/counterstrikesharp/logs/ip_site_blocker.log";

    [JsonPropertyName("blocked_domains_log")]
    public string BlockedDomainsLog { get; set; } = "addons/counterstrikesharp/logs/blocked_domains.log";

    [JsonPropertyName("auto_log_blocked")]
    public bool AutoLogBlocked { get; set; } = true;

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 2;
}

public class SiteAndIPBlocker : BasePlugin, IPluginConfig<SiteAndIPBlockerConfig>
{
    public override string ModuleName => "IPAndSiteBlocker";
    public override string ModuleVersion => "0.2.2";
    public override string ModuleAuthor => "PattHs and Luxecs2.ru";
    public override string ModuleDescription => "Блокировка сайтов и IP-адресов в чате + имена игроков.";

    public SiteAndIPBlockerConfig Config { get; set; } = null!;

    // Optimized regex patterns for better performance
    private static readonly Regex UrlRegex = new(@"\b(?:https?|ftp)://[^\s/$.?#].[^\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex IpRegex = new(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
    private static readonly Regex DomainRegex = new(@"\b(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    // Cache for blocking results to improve performance
    private readonly ConcurrentDictionary<string, bool> _blockingCache = new();
    
    // Async logging queue
    private readonly SemaphoreSlim _logSemaphore = new(1, 1);
    private readonly Queue<string> _logQueue = new();
    private readonly CancellationTokenSource _logCancellationTokenSource = new();

    private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
    private static readonly string CfgPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{AssemblyName}/{AssemblyName}.json";

    private static readonly List<string> CommonDomains = new List<string>
    {
        ".com", ".net", ".org", ".info", ".biz", ".us", ".ru",
        ".online", ".su", ".co", ".io", ".me", ".tv", ".edu",
        ".xyz", ".site", ".tech", ".dev", ".app", ".cloud"
    };

    public void OnConfigParsed(SiteAndIPBlockerConfig config)
    {
        Config = config;
        SafeUpdateConfig(config);
    }

    private void SafeUpdateConfig<T>(T config) where T : BasePluginConfig, new()
    {
        try
        {
            var newCfgVersion = new T().Version;

            if (config.Version == newCfgVersion)
                return;

            config.Version = newCfgVersion;

            var updatedJsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(CfgPath, updatedJsonContent);

            LogMessageAsync($"Config updated for V{newCfgVersion}.");
        }
        catch (Exception ex)
        {
            LogMessageAsync($"Error updating config: {ex.Message}");
        }
    }

    public override void Load(bool hotReload)
    {
        // Universal chat handling - single method for both say and say_team
        AddCommandListener("say", OnPlayerChat);
        AddCommandListener("say_team", OnPlayerChat);
        
        // Start async logging
        _ = Task.Run(ProcessLogQueueAsync, _logCancellationTokenSource.Token);
    }

    public override void Unload(bool hotReload)
    {
        _logCancellationTokenSource.Cancel();
        _logSemaphore.Dispose();
    }

    private static readonly Dictionary<string, char> ColorMap = new Dictionary<string, char>
    {
        { "{default}", ChatColors.Default }, { "{white}", ChatColors.White },
        { "{darkred}", ChatColors.DarkRed }, { "{green}", ChatColors.Green },
        { "{lightyellow}", ChatColors.LightYellow }, { "{lightblue}", ChatColors.LightBlue },
        { "{olive}", ChatColors.Olive }, { "{lime}", ChatColors.Lime },
        { "{red}", ChatColors.Red }, { "{lightpurple}", ChatColors.LightPurple },
        { "{purple}", ChatColors.Purple }, { "{grey}", ChatColors.Grey },
        { "{yellow}", ChatColors.Yellow }, { "{gold}", ChatColors.Gold },
        { "{silver}", ChatColors.Silver }, { "{blue}", ChatColors.Blue },
        { "{darkblue}", ChatColors.DarkBlue }, { "{bluegrey}", ChatColors.BlueGrey },
        { "{magenta}", ChatColors.Magenta }, { "{lightred}", ChatColors.LightRed },
        { "{orange}", ChatColors.Orange }
    };

    private string ReplaceColorPlaceholders(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;
            
        if (message[0] != ' ')
            message = " " + message;
        
        foreach (var colorPlaceholder in ColorMap)
            message = message.Replace(colorPlaceholder.Key, colorPlaceholder.Value.ToString());
        
        return message;
    }

    // Optimized blocking check with caching
    private bool IsBlockedCached(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        // Check cache first
        if (_blockingCache.TryGetValue(message, out bool cachedResult))
            return cachedResult;

        bool isBlocked = IsBlocked(message);
        
        // Cache the result (limit cache size to prevent memory issues)
        if (_blockingCache.Count < 1000)
            _blockingCache.TryAdd(message, isBlocked);
        
        return isBlocked;
    }

    private bool IsBlocked(string message)
    {
        // Check URLs - block ALL URLs except whitelisted
        var urlMatches = UrlRegex.Matches(message);
        foreach (Match match in urlMatches)
        {
            if (!IsWhitelisted(match.Value))
            {
                LogBlockedDomain(match.Value, "URL");
                return true;
            }
        }

        // Check IP addresses - block ALL IPs except whitelisted
        var ipMatches = IpRegex.Matches(message);
        foreach (Match match in ipMatches)
        {
            if (!IsWhitelisted(match.Value))
            {
                LogBlockedDomain(match.Value, "IP");
                return true;
            }
        }

        // Check domains (including naked domains) - block ALL domains except whitelisted
        var domainMatches = DomainRegex.Matches(message);
        foreach (Match match in domainMatches)
        {
            if (!IsWhitelisted(match.Value))
            {
                LogBlockedDomain(match.Value, "Domain");
                return true;
            }
        }

        // Check for naked domains without protocol - block ALL except whitelisted
        foreach (var domain in CommonDomains)
        {
            if (message.IndexOf(domain, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var nakedDomainMatches = Regex.Matches(message, $@"\b\w+{Regex.Escape(domain)}\b", RegexOptions.IgnoreCase);
                foreach (Match match in nakedDomainMatches)
                {
                    if (!IsWhitelisted(match.Value))
                    {
                        LogBlockedDomain(match.Value, "NakedDomain");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private string CleanName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "Player";

        // Remove URLs
        name = UrlRegex.Replace(name, "");
        
        // Remove IP addresses
        name = IpRegex.Replace(name, "");
        
        // Remove domains
        name = DomainRegex.Replace(name, "");
        
        // Remove naked domains
        foreach (var domain in CommonDomains)
            name = Regex.Replace(name, $@"\b\w+{Regex.Escape(domain)}\b", "", RegexOptions.IgnoreCase);

        name = name.Trim();
        
        // Ensure we have a valid name
        if (string.IsNullOrEmpty(name) || name.Length < 2)
            name = "Player" + Random.Shared.Next(1000, 9999);
            
        return name;
    }

    private bool IsWhitelisted(string message)
    {
        if (string.IsNullOrEmpty(message) || Config.Whitelist.Count == 0)
            return false;

        string lowerCaseMessage = message.ToLowerInvariant();

        return Config.Whitelist.Any(whitelistedItem => 
            lowerCaseMessage.Equals(whitelistedItem.ToLowerInvariant()) ||
            lowerCaseMessage.Contains(whitelistedItem.ToLowerInvariant()));
    }

    // Universal chat handling method
    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo message)
    {
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;

        string chatMessage = message.GetArg(1);
        if (string.IsNullOrEmpty(chatMessage))
            return HookResult.Continue;

        // Admin immunity check
        if (Config.AdminImmunity == 1 && AdminManager.PlayerHasPermissions(player, "@css/generic"))
            return HookResult.Continue;

        if (IsBlockedCached(chatMessage))
        {
            player.PrintToChat(ReplaceColorPlaceholders(Config.BlockMessage));
            LogMessageAsync($"Blocked message from {GetPlayerIdentifier(player)}: {chatMessage}");
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    // Multiple event hooks for reliable name checking
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        // Check all connected players when round starts
        var players = Utilities.GetPlayers();
        foreach (var player in players)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CheckAndHandlePlayerName(player);
            }
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        // Check all players when freeze time ends
        var players = Utilities.GetPlayers();
        foreach (var player in players)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CheckAndHandlePlayerName(player);
            }
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        // Delay check slightly to ensure player is fully loaded
        Server.NextFrame(() => CheckAndHandlePlayerName(player));
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        // Check player name on every spawn
        CheckAndHandlePlayerName(@event.Userid);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        // Check player name when changing teams
        CheckAndHandlePlayerName(@event.Userid);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerChangeName(EventPlayerChangename @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot) 
            return HookResult.Continue;

        // Admin immunity check
        if (Config.AdminImmunity == 1 && AdminManager.PlayerHasPermissions(player, "@css/generic"))
            return HookResult.Continue;
        
        string newName = @event.Newname;
        
        if (IsBlockedCached(newName))
        {
            if (Config.NameAction == 0)
            {
                NativeAPI.IssueServerCommand($"kickid {player.UserId}");
                LogMessageAsync($"Kicked player {GetPlayerIdentifier(player)} for banned name: {newName}");
                return HookResult.Handled;
            }
            else if (Config.NameAction == 1)
            {
                Server.NextFrame(() => RenamePlayer(player));
            }
        }
        
        return HookResult.Continue;
    }

    // Universal name checking and handling
    private void CheckAndHandlePlayerName(CCSPlayerController? player)
    {
        if (player == null || !player.IsValid || player.IsBot) 
            return;

        // Admin immunity check
        if (Config.AdminImmunity == 1 && AdminManager.PlayerHasPermissions(player, "@css/generic"))
            return;
        
        string playerName = player.PlayerName;
        
        if (IsBlockedCached(playerName))
        {
            if (Config.NameAction == 0)
            {
                NativeAPI.IssueServerCommand($"kickid {player.UserId}");
                LogMessageAsync($"Kicked player {GetPlayerIdentifier(player)} for banned name: {playerName}");
            }
            else if (Config.NameAction == 1)
            {
                RenamePlayer(player);
            }
        }
    }

    private void RenamePlayer(CCSPlayerController player)
    {
        string originalName = player.PlayerName;
        string cleanedName = CleanName(originalName);
        
        // Ensure the new name is different and not empty
        if (string.IsNullOrEmpty(cleanedName) || cleanedName == originalName)
        {
            cleanedName = "Player" + Random.Shared.Next(1000, 9999);
        }
        
        player.PlayerName = cleanedName;
        Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
        player.PrintToChat(ReplaceColorPlaceholders(Config.RenameMessage));
        
        LogMessageAsync($"Renamed player {GetPlayerIdentifier(player)} from '{originalName}' to '{cleanedName}'");
    }

    private void LogBlockedDomain(string blockedContent, string type)
    {
        if (!Config.AutoLogBlocked)
            return;

        try
        {
            var logPath = Path.Combine(Server.GameDirectory, "csgo", Config.BlockedDomainsLog);
            var logDir = Path.GetDirectoryName(logPath);
            
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {blockedContent}";
            
            // Asynchronously append to log without blocking
            Task.Run(() =>
            {
                try
                {
                    File.AppendAllText(logPath, logEntry + Environment.NewLine);
                }
                catch
                {
                    // Ignore logging errors
                }
            });
        }
        catch
        {
            // Ignore logging errors to prevent crashes
        }
    }

    private string GetPlayerIdentifier(CCSPlayerController player)
    {
        try
        {
            return player.SteamID.ToString();
        }
        catch
        {
            return $"User{player.UserId}";
        }
    }

    // Asynchronous logging
    private async void LogMessageAsync(string message)
    {
        try
        {
            await _logSemaphore.WaitAsync();
            _logQueue.Enqueue($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
        catch
        {
            // Ignore logging errors to prevent crashes
        }
        finally
        {
            _logSemaphore.Release();
        }
    }

    private async Task ProcessLogQueueAsync()
    {
        while (!_logCancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await _logSemaphore.WaitAsync(_logCancellationTokenSource.Token);
                
                if (_logQueue.Count > 0)
                {
                    var logPath = Path.Combine(Server.GameDirectory, "csgo", Config.LogPath);
                    var logDir = Path.GetDirectoryName(logPath);
                    
                    if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logPath))
                        Directory.CreateDirectory(logDir);
                    
                    var logEntries = new List<string>();
                    while (_logQueue.Count > 0)
                    {
                        logEntries.Add(_logQueue.Dequeue());
                    }
                    
                    await File.AppendAllLinesAsync(logPath, logEntries, _logCancellationTokenSource.Token);
                }
            }
            catch
            {
                // Ignore logging errors to prevent crashes
            }
            finally
            {
                _logSemaphore.Release();
            }
            
            await Task.Delay(1000, _logCancellationTokenSource.Token);
        }
    }
}