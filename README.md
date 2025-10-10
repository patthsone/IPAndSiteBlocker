# IPAndSiteBlocker
A plugin that blocks websites and IP addresses (name + chat), with a configurable whitelist for allowed sites and IPs.
Плагин, блокирующий веб-сайты и IP-адреса (имя + чат), с настраиваемым белым списком разрешенных сайтов и IP-адресов.

## Features
✅ **Optimized Performance**: Cached blocking results for faster processing  
✅ **Asynchronous Logging**: Non-blocking logging system for high-traffic servers  
✅ **Universal Chat Handling**: Single method handles both public and team chat  
✅ **Smart Domain Detection**: Blocks naked domains (site.io, domain.xyz) without protocols  
✅ **Reliable Name Checking**: Map start hook ensures names are checked when map loads  
✅ **Safe Config Updates**: Error handling prevents crashes during config updates  
✅ **Improved Player Handling**: Guaranteed name changes prevent banned names from leaking  
✅ **Enhanced Logging**: Always displays SteamID correctly, even when temporarily unavailable  

## Config
The configuration file will be automatically generated in `csgo/addons/counterstrikesharp/configs/plugins/IPAndSiteBlocker/IPAndSiteBlocker.json`
```json
{
    "whitelist": [
        "сайт",
        "айпи"
    ],
    "block_message": "{darkred}Blocked: Sending IP addresses or websites is not allowed.",
    "name_action": 1,
    "rename_message": "{darkred}Your name contains a blocked IP address or website. It will be renamed.",
    "admin_immunity": 1,
    "log_path": "addons/counterstrikesharp/logs/ip_site_blocker.log",
    "ConfigVersion": 1
}
```

### Configuration Options
- **whitelist**: Array of allowed domains/IPs that won't be blocked
- **block_message**: Message shown when a message is blocked
- **name_action**: 0 = kick player, 1 = rename player
- **rename_message**: Message shown when player is renamed
- **admin_immunity**: 0 = disabled, 1 = admins with @css/generic permission are immune
- **log_path**: Path to log file (relative to csgo directory)

## Available colors
```
{default}
{white}
{darkred}
{green}
{lightyellow}
{lightblue}
{olive}
{lime}
{red}
{lightpurple}
{purple}
{grey}
{yellow}
{gold}
{silver}
{blue}
{darkblue}
{bluegrey}
{magenta}
{lightred}
{orange}
```
