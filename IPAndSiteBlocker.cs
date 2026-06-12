using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace IPAndSiteBlocker;

public sealed class IPAndSiteBlockerConfig : BasePluginConfig
{
    [JsonPropertyName("language")]
    public string Language { get; set; } = "ru";

    [JsonPropertyName("whitelist")]
    public List<string> Whitelist { get; set; } = new()
    {
        "luxecs2.ru",
        "t.me/luxecs2",
        "discord.gg",
        "steamcommunity.com"
    };

    [JsonPropertyName("block_message")]
    public string BlockMessage { get; set; } = "{darkred}Запрещено отправлять IP-адреса и ссылки на сторонние сайты.";

    [JsonPropertyName("name_action")]
    public int NameAction { get; set; } = 1;

    [JsonPropertyName("rename_message")]
    public string RenameMessage { get; set; } = "{darkred}Ваш ник содержит запрещённый сайт или IP. Ник был изменён.";

    [JsonPropertyName("admin_immunity")]
    public bool AdminImmunity { get; set; } = true;

    [JsonPropertyName("admin_permission")]
    public string AdminPermission { get; set; } = "@css/generic";

    [JsonPropertyName("log_path")]
    public string LogPath { get; set; } = "addons/counterstrikesharp/logs/ip_site_blocker.log";

    [JsonPropertyName("blocked_domains_log")]
    public string BlockedDomainsLog { get; set; } = "addons/counterstrikesharp/logs/blocked_domains.log";

    [JsonPropertyName("auto_log_blocked")]
    public bool AutoLogBlocked { get; set; } = true;

    [JsonPropertyName("check_names_on_events")]
    public bool CheckNamesOnEvents { get; set; } = true;

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 4;
}

internal sealed class PluginLocalization
{
    private readonly Dictionary<string, string> _phrases = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _baseDirectory;

    public PluginLocalization(string baseDirectory, string language)
    {
        _baseDirectory = baseDirectory;
        Load(language);
    }

    public string Get(string key, params object[] args)
    {
        if (!_phrases.TryGetValue(key, out var value))
            value = key;

        if (args.Length == 0)
            return value;

        try
        {
            return string.Format(value, args);
        }
        catch
        {
            return value;
        }
    }

    private void Load(string language)
    {
        _phrases.Clear();
        LoadBuiltInRu();

        var lang = string.IsNullOrWhiteSpace(language) ? "ru" : language.Trim().ToLowerInvariant();
        var path = Path.Combine(_baseDirectory, "translations", lang + ".json");

        if (!File.Exists(path) && lang != "ru")
            path = Path.Combine(_baseDirectory, "translations", "ru.json");

        if (!File.Exists(path))
            return;

        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (loaded == null)
                return;

            foreach (var item in loaded)
                _phrases[item.Key] = item.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IPAndSiteBlocker] Translation load error: {ex.Message}");
        }
    }

    private void LoadBuiltInRu()
    {
        _phrases["block_message"] = "{darkred}Запрещено отправлять IP-адреса и ссылки на сторонние сайты.";
        _phrases["rename_message"] = "{darkred}Ваш ник содержит запрещённый сайт или IP. Ник был изменён.";
        _phrases["config_updated"] = "Конфиг обновлён до версии {0}.";
        _phrases["loaded"] = "Плагин загружен.";
        _phrases["unloaded"] = "Плагин выгружен.";
        _phrases["chat_registered"] = "Слушатели чата зарегистрированы.";
        _phrases["domains_loaded"] = "domains.cfg загружен. Зон: {0}. DefaultName: {1}";
        _phrases["blocked_chat"] = "Заблокировано сообщение от {0}: {1}";
        _phrases["blocked_name"] = "Заблокирован ник {0}: {1}";
        _phrases["renamed"] = "Игрок {0} переименован: {1} -> {2}";
        _phrases["kicked"] = "Игрок {0} кикнут за запрещённый ник: {1}";
        _phrases["error"] = "Ошибка: {0}";
    }
}

internal readonly record struct BlockMatch(string Value, string Type);

[MinimumApiVersion(90)]
public sealed class IPAndSiteBlocker : BasePlugin, IPluginConfig<IPAndSiteBlockerConfig>
{
    public override string ModuleName => "IPAndSiteBlocker";
    public override string ModuleVersion => "2.5.8";
    public override string ModuleAuthor => "PattHs / LUXECS2.RU";
    public override string ModuleDescription => "Blocks websites and IP addresses in chat and player names.";

    public IPAndSiteBlockerConfig Config { get; set; } = new();

    private const int MaxCacheSize = 4096;
    private const string DefaultPlayerName = "Player";

    private static readonly string ModuleDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
    private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "IPAndSiteBlocker";

    private static readonly Regex UrlRegex = new(@"\b(?:(?:https?|ftp)://|www\.)[^\s<>""']+", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex IpRegex = new(@"(?<!\d)(?:\d{1,3}\.){3}\d{1,3}(?!\d)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex DomainRegex = new(@"(?<![\w.-])(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,63}(?![\w.-])", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ConcurrentDictionary<string, bool> _blockedCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<string> _logQueue = new();
    private readonly CancellationTokenSource _logCts = new();
    private readonly object _domainsLock = new();

    private PluginLocalization _localization = null!;
    private Timer? _cacheTimer;
    private Task? _logTask;
    private Regex? _tldDomainRegex;
    private string[] _tldSuffixes = Array.Empty<string>();
    private string _defaultName = DefaultPlayerName;
    private string _logPath = string.Empty;
    private string _blockedLogPath = string.Empty;
    private bool _unloaded;

    private static readonly Dictionary<string, char> ColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["{default}"] = ChatColors.Default,
        ["{white}"] = ChatColors.White,
        ["{darkred}"] = ChatColors.DarkRed,
        ["{green}"] = ChatColors.Green,
        ["{lightyellow}"] = ChatColors.LightYellow,
        ["{lightblue}"] = ChatColors.LightBlue,
        ["{olive}"] = ChatColors.Olive,
        ["{lime}"] = ChatColors.Lime,
        ["{red}"] = ChatColors.Red,
        ["{lightpurple}"] = ChatColors.LightPurple,
        ["{purple}"] = ChatColors.Purple,
        ["{grey}"] = ChatColors.Grey,
        ["{yellow}"] = ChatColors.Yellow,
        ["{gold}"] = ChatColors.Gold,
        ["{silver}"] = ChatColors.Silver,
        ["{blue}"] = ChatColors.Blue,
        ["{darkblue}"] = ChatColors.DarkBlue,
        ["{bluegrey}"] = ChatColors.BlueGrey,
        ["{magenta}"] = ChatColors.Magenta,
        ["{lightred}"] = ChatColors.LightRed,
        ["{orange}"] = ChatColors.Orange
    };

    private static readonly string DefaultDomainsCfg = """
*.pw
*.r
*.com
*.net
*.org
*.ru
*.ua
*.kz
*.by
*.info
*.biz
*.io
*.gg
*.tv
*.site
*.xyz
*.online
*.pro
*.club
*.vip
*.me
*.cc
*.cn
*.de
*.uk
*.pl
*.fr
*.eu
*.us
*.top
*.fun
*.shop
*.su
*.ro
*.cz
*.sk
*.hu
*.bg
*.rs
*.hr
*.si
*.lt
*.lv
*.ee
*.fi
*.se
*.no
*.dk
*.nl
*.be
*.ch
*.at
*.it
*.es
*.pt
*.gr
*.tr

"DefaultName" "Player"
""";

    public void OnConfigParsed(IPAndSiteBlockerConfig config)
    {
        Config = config;
        _localization = new PluginLocalization(ModuleDirectory, Config.Language);
        SaveConfigIfNeeded();
    }

    public override void Load(bool hotReload)
    {
        _unloaded = false;
        _logPath = ToGamePath(Config.LogPath);
        _blockedLogPath = ToGamePath(Config.BlockedDomainsLog);

        LoadDomainsConfig();

        AddCommandListener("say", OnPlayerChat);
        AddCommandListener("say_team", OnPlayerChat);

        _logTask = Task.Run(ProcessLogQueueAsync);
        _cacheTimer = new Timer(_ => _blockedCache.Clear(), null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

        Log($"IPAndSiteBlocker v{ModuleVersion} loaded. API: {GetApiVersion()}");
        Log(_localization.Get("chat_registered"));
    }

    public override void Unload(bool hotReload)
    {
        _unloaded = true;
        _cacheTimer?.Dispose();
        _logCts.Cancel();
        FlushLogs();
        _logCts.Dispose();
        Console.WriteLine("[IPAndSiteBlocker] Unloaded.");
    }

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (Config.CheckNamesOnEvents && @event.Userid != null)
            Server.NextFrame(() => CheckAndHandlePlayerName(@event.Userid));

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (Config.CheckNamesOnEvents)
            CheckAndHandlePlayerName(@event.Userid);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerChangeName(EventPlayerChangename @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!IsRealPlayer(player) || HasAdminImmunity(player!))
            return HookResult.Continue;

        var newName = @event.Newname ?? string.Empty;
        if (!IsBlockedCached(newName))
            return HookResult.Continue;

        LogBlockedDomain(newName, "Name");
        HandleBadName(player!, newName);
        return Config.NameAction == 0 ? HookResult.Handled : HookResult.Continue;
    }

    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo command)
    {
        if (!IsRealPlayer(player) || HasAdminImmunity(player!))
            return HookResult.Continue;

        var text = command.GetArg(1);
        if (string.IsNullOrWhiteSpace(text))
            return HookResult.Continue;

        var match = FindBlocked(text);
        if (match == null)
            return HookResult.Continue;

        LogBlockedDomain(match.Value.Value, match.Value.Type);
        Log(_localization.Get("blocked_chat", GetPlayerIdentifier(player!), text));
        player!.PrintToChat(Colorize(Config.BlockMessage));
        return HookResult.Handled;
    }

    private void CheckAndHandlePlayerName(CCSPlayerController? player)
    {
        if (!IsRealPlayer(player) || HasAdminImmunity(player!))
            return;

        var name = player!.PlayerName ?? string.Empty;
        if (!IsBlockedCached(name))
            return;

        LogBlockedDomain(name, "Name");
        HandleBadName(player, name);
    }

    private void HandleBadName(CCSPlayerController player, string badName)
    {
        if (Config.NameAction == 0)
        {
            KickPlayer(player, badName);
            return;
        }

        RenamePlayer(player, badName);
    }

    private void KickPlayer(CCSPlayerController player, string badName)
    {
        try
        {
            NativeAPI.IssueServerCommand($"kickid {player.UserId} \"Blocked name\"");
            Log(_localization.Get("kicked", GetPlayerIdentifier(player), badName));
        }
        catch (Exception ex)
        {
            Log(_localization.Get("error", ex.Message));
        }
    }

    private void RenamePlayer(CCSPlayerController player, string badName)
    {
        try
        {
            var cleanName = CleanName(badName);
            if (string.IsNullOrWhiteSpace(cleanName) || string.Equals(cleanName, badName, StringComparison.Ordinal))
                cleanName = GetFallbackName();

            player.PlayerName = cleanName;
            TrySetNameStateChanged(player);
            player.PrintToChat(Colorize(Config.RenameMessage));
            Log(_localization.Get("renamed", GetPlayerIdentifier(player), badName, cleanName));
        }
        catch (Exception ex)
        {
            Log(_localization.Get("error", ex.Message));
        }
    }

    private static void TrySetNameStateChanged(CCSPlayerController player)
    {
        try
        {
            Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
        }
        catch
        {
        }
    }

    private bool IsBlockedCached(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (_blockedCache.TryGetValue(text, out var cached))
            return cached;

        var blocked = FindBlocked(text) != null;
        if (_blockedCache.Count >= MaxCacheSize)
            _blockedCache.Clear();

        _blockedCache.TryAdd(text, blocked);
        return blocked;
    }

    private BlockMatch? FindBlocked(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        foreach (Match match in UrlRegex.Matches(text))
        {
            var value = TrimMatch(match.Value);
            if (!IsWhitelisted(value))
                return new BlockMatch(value, "URL");
        }

        foreach (Match match in IpRegex.Matches(text))
        {
            var value = match.Value;
            if (IsValidIPv4(value) && !IsWhitelisted(value))
                return new BlockMatch(value, "IP");
        }

        foreach (Match match in DomainRegex.Matches(text))
        {
            var value = TrimMatch(match.Value);
            if (!IsWhitelisted(value))
                return new BlockMatch(value, "Domain");
        }

        Regex? tldRegex;
        lock (_domainsLock)
            tldRegex = _tldDomainRegex;

        if (tldRegex == null)
            return null;

        foreach (Match match in tldRegex.Matches(text))
        {
            var value = TrimMatch(match.Value);
            if (!IsWhitelisted(value))
                return new BlockMatch(value, "TLD");
        }

        return null;
    }

    private string CleanName(string name)
    {
        var result = name;
        result = UrlRegex.Replace(result, string.Empty);
        result = IpRegex.Replace(result, string.Empty);
        result = DomainRegex.Replace(result, string.Empty);

        Regex? tldRegex;
        lock (_domainsLock)
            tldRegex = _tldDomainRegex;

        if (tldRegex != null)
            result = tldRegex.Replace(result, string.Empty);

        result = Regex.Replace(result, @"\s+", " ").Trim();
        return result.Length >= 2 ? result : GetFallbackName();
    }

    private bool IsWhitelisted(string value)
    {
        if (Config.Whitelist.Count == 0 || string.IsNullOrWhiteSpace(value))
            return false;

        var normalizedValue = NormalizeWhitelistValue(value);
        if (string.IsNullOrWhiteSpace(normalizedValue))
            return false;

        foreach (var entry in Config.Whitelist)
        {
            var normalizedEntry = NormalizeWhitelistValue(entry);
            if (string.IsNullOrWhiteSpace(normalizedEntry))
                continue;

            if (IsIp(normalizedValue) || IsIp(normalizedEntry))
            {
                if (string.Equals(normalizedValue, normalizedEntry, StringComparison.OrdinalIgnoreCase))
                    return true;

                continue;
            }

            if (string.Equals(normalizedValue, normalizedEntry, StringComparison.OrdinalIgnoreCase))
                return true;

            if (normalizedValue.EndsWith("." + normalizedEntry, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string NormalizeWhitelistValue(string value)
    {
        var result = TrimMatch(value).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(result))
            return string.Empty;

        if (result.StartsWith("*."))
            result = result[2..];

        if (result.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            result.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            result.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
        {
            if (Uri.TryCreate(result, UriKind.Absolute, out var uri))
                result = uri.Host;
        }
        else if (result.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
        {
            result = result[4..];
        }
        else if (result.Contains('/'))
        {
            result = result.Split('/')[0];
        }

        if (result.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            result = result[4..];

        return result.Trim('.');
    }

    private static string TrimMatch(string value)
    {
        return value.Trim().TrimEnd('.', ',', ';', ':', '!', '?', ')', ']', '}', '\'', '"');
    }

    private static bool IsIp(string value)
    {
        return IPAddress.TryParse(value, out _);
    }

    private static bool IsValidIPv4(string value)
    {
        if (!IPAddress.TryParse(value, out var address))
            return false;

        return address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    private void LoadDomainsConfig()
    {
        try
        {
            var configDir = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", AssemblyName);
            Directory.CreateDirectory(configDir);

            var path = Path.Combine(configDir, "domains.cfg");
            if (!File.Exists(path))
                File.WriteAllText(path, DefaultDomainsCfg);

            ParseDomains(File.ReadAllLines(path), out var suffixes, out var defaultName);
            var regex = BuildTldRegex(suffixes);

            lock (_domainsLock)
            {
                _tldSuffixes = suffixes;
                _tldDomainRegex = regex;
                _defaultName = SanitizeFallbackName(defaultName, regex);
            }

            Log(_localization.Get("domains_loaded", suffixes.Length, _defaultName));
        }
        catch (Exception ex)
        {
            Log(_localization.Get("error", "domains.cfg: " + ex.Message));
            lock (_domainsLock)
            {
                _tldSuffixes = Array.Empty<string>();
                _tldDomainRegex = null;
                _defaultName = DefaultPlayerName;
            }
        }
    }

    private static void ParseDomains(IEnumerable<string> lines, out string[] suffixes, out string defaultName)
    {
        var result = new List<string>();
        defaultName = DefaultPlayerName;

        foreach (var rawLine in lines)
        {
            var line = RemoveComments(rawLine).Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var kv = Regex.Match(line, "^\\s*\"(?<key>[^\"]+)\"\\s+\"(?<value>[^\"]*)\"", RegexOptions.CultureInvariant);
            if (kv.Success)
            {
                if (kv.Groups["key"].Value.Equals("DefaultName", StringComparison.OrdinalIgnoreCase))
                    defaultName = kv.Groups["value"].Value;

                continue;
            }

            var token = line.Trim().Trim('*').Trim();
            if (token.StartsWith("*.", StringComparison.Ordinal))
                token = token[1..];

            if (!token.StartsWith('.'))
                token = "." + token.TrimStart('.');

            token = token.ToLowerInvariant();
            if (Regex.IsMatch(token, @"^\.[a-z0-9.-]{1,63}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                result.Add(token);
        }

        suffixes = result.Distinct(StringComparer.OrdinalIgnoreCase).OrderByDescending(x => x.Length).ToArray();
    }

    private static string RemoveComments(string line)
    {
        if (string.IsNullOrEmpty(line))
            return string.Empty;

        var trimmed = line.TrimStart();
        if (trimmed.StartsWith('#') || trimmed.StartsWith(';'))
            return string.Empty;

        var inQuotes = false;
        for (var i = 0; i < line.Length - 1; i++)
        {
            if (line[i] == '"')
                inQuotes = !inQuotes;

            if (!inQuotes && line[i] == '/' && line[i + 1] == '/')
                return line[..i];
        }

        return line;
    }

    private static Regex? BuildTldRegex(string[] suffixes)
    {
        if (suffixes.Length == 0)
            return null;

        var pattern = string.Join("|", suffixes.Select(Regex.Escape));
        return new Regex($@"(?<![\w.-])[a-z0-9][a-z0-9-]{{0,61}}(?:{pattern})(?![\w.-])", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    private static string SanitizeFallbackName(string value, Regex? tldRegex)
    {
        var result = string.IsNullOrWhiteSpace(value) ? DefaultPlayerName : value.Trim();
        result = UrlRegex.Replace(result, string.Empty);
        result = IpRegex.Replace(result, string.Empty);
        result = DomainRegex.Replace(result, string.Empty);
        if (tldRegex != null)
            result = tldRegex.Replace(result, string.Empty);

        result = Regex.Replace(result, @"\s+", " ").Trim();
        return result.Length >= 2 ? result : DefaultPlayerName;
    }

    private string GetFallbackName()
    {
        lock (_domainsLock)
            return string.IsNullOrWhiteSpace(_defaultName) ? DefaultPlayerName : _defaultName;
    }

    private bool HasAdminImmunity(CCSPlayerController player)
    {
        if (!Config.AdminImmunity)
            return false;

        if (string.IsNullOrWhiteSpace(Config.AdminPermission))
            return false;

        try
        {
            return AdminManager.PlayerHasPermissions(player, Config.AdminPermission);
        }
        catch (Exception ex)
        {
            Log(_localization.Get("error", "Admin immunity: " + ex.Message));
            return false;
        }
    }

    private static bool IsRealPlayer(CCSPlayerController? player)
    {
        return player != null && player.IsValid && !player.IsBot;
    }

    private static string Colorize(string message)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        var result = message;
        foreach (var color in ColorMap)
            result = result.Replace(color.Key, color.Value.ToString(), StringComparison.OrdinalIgnoreCase);

        return result.Length > 0 && result[0] != ' ' ? " " + result : result;
    }

    private void LogBlockedDomain(string value, string type)
    {
        if (!Config.AutoLogBlocked || string.IsNullOrWhiteSpace(_blockedLogPath))
            return;

        EnqueueFileLine(_blockedLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {value}");
    }

    private void Log(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        Console.WriteLine("[IPAndSiteBlocker] " + message);
        EnqueueFileLine(_logPath, line);
    }

    private void EnqueueFileLine(string path, string line)
    {
        if (_unloaded || string.IsNullOrWhiteSpace(path))
            return;

        _logQueue.Enqueue(path + "\t" + line);
    }

    private async Task ProcessLogQueueAsync()
    {
        while (!_logCts.IsCancellationRequested)
        {
            try
            {
                FlushLogs();
                await Task.Delay(1000, _logCts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
            }
        }

        FlushLogs();
    }

    private void FlushLogs()
    {
        if (_logQueue.IsEmpty)
            return;

        var groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        while (_logQueue.TryDequeue(out var item))
        {
            var index = item.IndexOf('\t');
            if (index <= 0)
                continue;

            var path = item[..index];
            var line = item[(index + 1)..];

            if (!groups.TryGetValue(path, out var list))
            {
                list = new List<string>();
                groups[path] = list;
            }

            list.Add(line);
        }

        foreach (var group in groups)
        {
            try
            {
                var dir = Path.GetDirectoryName(group.Key);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                File.AppendAllLines(group.Key, group.Value);
            }
            catch
            {
            }
        }
    }

    private void SaveConfigIfNeeded()
    {
        try
        {
            var fresh = new IPAndSiteBlockerConfig();
            if (Config.Version >= fresh.Version)
                return;

            Config.Version = fresh.Version;
            var path = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", AssemblyName, AssemblyName + ".json");
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true }));
            Log(_localization.Get("config_updated", fresh.Version));
        }
        catch (Exception ex)
        {
            Console.WriteLine("[IPAndSiteBlocker] Config update error: " + ex.Message);
        }
    }

    private static string ToGamePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        if (Path.IsPathRooted(path))
            return path;

        return Path.Combine(Server.GameDirectory, "csgo", path.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string GetPlayerIdentifier(CCSPlayerController player)
    {
        try
        {
            return $"{player.PlayerName} [{player.SteamID}]";
        }
        catch
        {
            return "UnknownPlayer";
        }
    }

    private static string GetApiVersion()
    {
        try
        {
            return typeof(BasePlugin).Assembly.GetName().Version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
