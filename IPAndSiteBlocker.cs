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
using System.Globalization;

namespace SiteAndIPBlocker;

public class Localization
{
    private readonly Dictionary<string, string> _translations = new();
    private static readonly string ModuleDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

    public Localization(string language)
    {
        LoadTranslations(language);
    }

    private void LoadTranslations(string language)
    {
        try
        {
            string translationsPath = Path.Combine(ModuleDirectory, "translations", $"{language}.json");
            if (File.Exists(translationsPath))
            {
                string json = File.ReadAllText(translationsPath);
                var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (translations != null)
                {
                    _translations.Clear();
                    foreach (var kvp in translations)
                    {
                        _translations[kvp.Key] = kvp.Value;
                    }
                }
            }
            else
            {
                // Fallback to English if language file doesn't exist
                LoadTranslations("en");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IPAndSiteBlocker] Error loading translations for {language}: {ex.Message}");
            // Fallback to English
            LoadTranslations("en");
        }
    }

    public string Get(string key, params object[] args)
    {
        if (_translations.TryGetValue(key, out string? value))
        {
            return args.Length > 0 ? string.Format(value, args) : value;
        }
        return $"[{key}]"; // Return key in brackets if translation not found
    }
}

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

    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 3;
}

public class SiteAndIPBlocker : BasePlugin, IPluginConfig<SiteAndIPBlockerConfig>
{
    public override string ModuleName => "IPAndSiteBlocker";
    public override string ModuleVersion => "0.2.4";
    public override string ModuleAuthor => "PattHs and Luxecs2.ru";
    public override string ModuleDescription => "Блокировка сайтов и IP-адресов в чате + имена игроков. (Future-proof: Compatible with all CounterStrikeSharp.API versions)";

    public SiteAndIPBlockerConfig Config { get; set; } = null!;
    private Localization? _localization;

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
    
    // Cached paths to avoid accessing Server/Config from non-main threads
    private string? _cachedLogPath;
    private string? _cachedBlockedDomainsLogPath;

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
        _localization = new Localization(config.Language);
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

            LogMessageAsync(_localization?.Get("config_updated", newCfgVersion) ?? $"Config updated for V{newCfgVersion}.");
        }
        catch (Exception ex)
        {
            LogMessageAsync(_localization?.Get("error_updating_config", ex.Message) ?? $"Error updating config: {ex.Message}");
        }
    }

    public override void Load(bool hotReload)
    {
        try
        {
            // Cache paths in main thread to avoid thread-safety issues
            _cachedLogPath = Path.Combine(Server.GameDirectory, "csgo", Config.LogPath);
            _cachedBlockedDomainsLogPath = Path.Combine(Server.GameDirectory, "csgo", Config.BlockedDomainsLog);
            
            // Log plugin and API version information for troubleshooting
            LogMessageAsync($"IPAndSiteBlocker v{ModuleVersion} loading...");
            LogMessageAsync($"CounterStrikeSharp API: {GetApiVersion()}");

            // Universal chat handling - single method for both say and say_team
            // Wrapped in try-catch to handle potential API changes
            try
            {
                AddCommandListener("say", OnPlayerChat);
                AddCommandListener("say_team", OnPlayerChat);
                LogMessageAsync(_localization?.Get("chat_listeners_registered") ?? "Chat listeners registered successfully.");
            }
            catch (Exception ex)
            {
                LogMessageAsync(_localization?.Get("warning_chat_listeners", ex.Message) ?? $"Warning: Failed to register chat listeners: {ex.Message}");
                // Continue loading even if chat listeners fail
            }

            // Start async logging
            _ = Task.Run(ProcessLogQueueAsync, _logCancellationTokenSource.Token);

            LogMessageAsync(_localization?.Get("plugin_loaded") ?? "IPAndSiteBlocker loaded successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IPAndSiteBlocker] {_localization?.Get("critical_error_load", ex.Message) ?? $"Critical error during load: {ex.Message}"}");
            throw; // Re-throw critical errors
        }
    }
    
    // Get API version for logging and compatibility checking
    private string GetApiVersion()
    {
        try
        {
            var apiAssembly = typeof(BasePlugin).Assembly;
            var version = apiAssembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
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

        // Admin immunity check using safe wrapper
        if (HasAdminImmunity(player))
            return HookResult.Continue;

        if (IsBlockedCached(chatMessage))
        {
            try
            {
                player.PrintToChat(ReplaceColorPlaceholders(_localization?.Get("block_message") ?? Config.BlockMessage));
            }
            catch (Exception ex)
            {
                LogMessageAsync(_localization?.Get("warning_chat_message", ex.Message) ?? $"Warning: Failed to send chat message: {ex.Message}");
            }

            LogMessageAsync(_localization?.Get("blocked_message_log", GetPlayerIdentifier(player), chatMessage) ?? $"Blocked message from {GetPlayerIdentifier(player)}: {chatMessage}");
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    // Multiple event hooks for reliable name checking
    // Wrapped in try-catch to handle potential API changes
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        try
        {
            // Check all connected players when round starts
            var players = Utilities.GetPlayers();
            if (players != null)
            {
                foreach (var player in players)
                {
                    try
                    {
                        if (player != null && player.IsValid && !player.IsBot)
                        {
                            CheckAndHandlePlayerName(player);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessageAsync(_localization?.Get("error_checking_player", "OnRoundStart", ex.Message) ?? $"Error checking player in OnRoundStart: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessageAsync(_localization?.Get("error_event", "OnRoundStart", ex.Message) ?? $"Error in OnRoundStart event: {ex.Message}");
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        try
        {
            // Check all players when freeze time ends
            var players = Utilities.GetPlayers();
            if (players != null)
            {
                foreach (var player in players)
                {
                    try
                    {
                        if (player != null && player.IsValid && !player.IsBot)
                        {
                            CheckAndHandlePlayerName(player);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessageAsync($"Error checking player in OnRoundFreezeEnd: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessageAsync(_localization?.Get("error_event", "OnRoundFreezeEnd", ex.Message) ?? $"Error in OnRoundFreezeEnd event: {ex.Message}");
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        try
        {
            var player = @event.Userid;
            if (player != null)
            {
                // Delay check slightly to ensure player is fully loaded
                Server.NextFrame(() =>
                {
                    try
                    {
                        CheckAndHandlePlayerName(player);
                    }
                    catch (Exception ex)
                    {
                        LogMessageAsync(_localization?.Get("error_delayed_name_check", ex.Message) ?? $"Error in delayed name check: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            LogMessageAsync(_localization?.Get("error_event", "OnPlayerConnectFull", ex.Message) ?? $"Error in OnPlayerConnectFull event: {ex.Message}");
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        try
        {
            // Check player name on every spawn
            var player = @event.Userid;
            if (player != null)
            {
                CheckAndHandlePlayerName(player);
            }
        }
        catch (Exception ex)
        {
            LogMessageAsync(_localization?.Get("error_event", "OnPlayerSpawn", ex.Message) ?? $"Error in OnPlayerSpawn event: {ex.Message}");
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        try
        {
            // Check player name when changing teams
            var player = @event.Userid;
            if (player != null)
            {
                CheckAndHandlePlayerName(player);
            }
        }
        catch (Exception ex)
        {
            LogMessageAsync(_localization?.Get("error_event", "OnPlayerTeam", ex.Message) ?? $"Error in OnPlayerTeam event: {ex.Message}");
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerChangeName(EventPlayerChangename @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot) 
            return HookResult.Continue;

        // Admin immunity check using safe wrapper
        if (HasAdminImmunity(player))
            return HookResult.Continue;
        
        string newName = @event.Newname;
        
        if (IsBlockedCached(newName))
        {
            if (Config.NameAction == 0)
            {
                try
                {
                    NativeAPI.IssueServerCommand($"kickid {player.UserId}");
                    LogMessageAsync(_localization?.Get("kicked_player", GetPlayerIdentifier(player), newName) ?? $"Kicked player {GetPlayerIdentifier(player)} for banned name: {newName}");
                }
                catch (Exception ex)
                {
                    LogMessageAsync(_localization?.Get("error_kicking_player", ex.Message) ?? $"Error kicking player: {ex.Message}");
                }
                return HookResult.Handled;
            }
            else if (Config.NameAction == 1)
            {
                try
                {
                    Server.NextFrame(() => RenamePlayer(player));
                }
                catch (Exception ex)
                {
                    LogMessageAsync(_localization?.Get("error_scheduling_rename", ex.Message) ?? $"Error scheduling rename: {ex.Message}");
                }
            }
        }
        
        return HookResult.Continue;
    }

    // Universal name checking and handling
    private void CheckAndHandlePlayerName(CCSPlayerController? player)
    {
        if (player == null || !player.IsValid || player.IsBot) 
            return;

        // Admin immunity check using safe wrapper
        if (HasAdminImmunity(player))
            return;
        
        string playerName = player.PlayerName;
        
        if (IsBlockedCached(playerName))
        {
            if (Config.NameAction == 0)
            {
                try
                {
                    NativeAPI.IssueServerCommand($"kickid {player.UserId}");
                    LogMessageAsync(_localization?.Get("kicked_player", GetPlayerIdentifier(player), playerName) ?? $"Kicked player {GetPlayerIdentifier(player)} for banned name: {playerName}");
                }
                catch (Exception ex)
                {
                    LogMessageAsync(_localization?.Get("error_kicking_player", ex.Message) ?? $"Error kicking player: {ex.Message}");
                }
            }
            else if (Config.NameAction == 1)
            {
                RenamePlayer(player);
            }
        }
    }

    private void RenamePlayer(CCSPlayerController player)
    {
        try
        {
            if (player == null || !player.IsValid)
                return;
                
            string originalName = player.PlayerName;
            string cleanedName = CleanName(originalName);
            
            // Ensure the new name is different and not empty
            if (string.IsNullOrEmpty(cleanedName) || cleanedName == originalName)
            {
                cleanedName = "Player" + Random.Shared.Next(1000, 9999);
            }
            
            // Try to rename the player - this API might change between versions
            try
            {
                player.PlayerName = cleanedName;
                
                // SetStateChanged might not exist in all versions, wrap in try-catch
                try
                {
                    Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
                }
                catch (Exception ex)
                {
                    LogMessageAsync(_localization?.Get("warning_setstatechanged", ex.Message) ?? $"Warning: SetStateChanged failed (API change?): {ex.Message}");
                    // Continue anyway - the name change might still work
                }
                
                // PrintToChat might also change
                try
                {
                    player.PrintToChat(ReplaceColorPlaceholders(_localization?.Get("rename_message") ?? Config.RenameMessage));
                }
                catch
                {
                    // Silently fail if chat message doesn't work
                }

                LogMessageAsync(_localization?.Get("renamed_player", GetPlayerIdentifier(player), originalName, cleanedName) ?? $"Renamed player {GetPlayerIdentifier(player)} from '{originalName}' to '{cleanedName}'");
            }
            catch (Exception ex)
            {
                LogMessageAsync(_localization?.Get("error_renaming_player", ex.Message) ?? $"Error renaming player: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            LogMessageAsync(_localization?.Get("critical_error_rename", ex.Message) ?? $"Critical error in RenamePlayer: {ex.Message}");
        }
    }

    private void LogBlockedDomain(string blockedContent, string type)
    {
        if (!Config.AutoLogBlocked || string.IsNullOrEmpty(_cachedBlockedDomainsLogPath))
            return;

        try
        {
            // Use cached path to avoid accessing Server/Config from non-main thread
            var logPath = _cachedBlockedDomainsLogPath;
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

    // Safe admin permission check - handles API changes gracefully
    private bool HasAdminImmunity(CCSPlayerController player)
    {
        if (Config.AdminImmunity != 1)
            return false;
            
        if (player == null || !player.IsValid)
            return false;
        
        try
        {
            // AdminManager API might change between versions
            return AdminManager.PlayerHasPermissions(player, "@css/generic");
        }
        catch (Exception ex)
        {
            // If AdminManager API changed, log and default to no immunity
            LogMessageAsync(_localization?.Get("warning_admin_permission", ex.Message) ?? $"Warning: Admin permission check failed (API change?): {ex.Message}");
            return false;
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
            try
            {
                return $"User{player.UserId}";
            }
            catch
            {
                return "UnknownPlayer";
            }
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
                
                if (_logQueue.Count > 0 && !string.IsNullOrEmpty(_cachedLogPath))
                {
                    // Use cached path to avoid accessing Server/Config from non-main thread
                    var logPath = _cachedLogPath;
                    var logDir = Path.GetDirectoryName(logPath);
                    
                    if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
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