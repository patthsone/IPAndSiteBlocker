# IPAndSiteBlocker
A plugin that blocks websites and IP addresses (name + chat), with a configurable whitelist for allowed sites and IPs.
Плагин, блокирующий веб-сайты и IP-адреса (имя + чат), с настраиваемым белым списком разрешенных сайтов и IP-адресов.

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
    "ConfigVersion": 1
}
```

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
