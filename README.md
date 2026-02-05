## IPAndSiteBlocker
Blocks IP addresses and websites in **chat** and **player names** for Counter-Strike 2 servers running CounterStrikeSharp.  
Блокирует IP‑адреса и сайты в **чате** и **никах** игроков на сервере Counter‑Strike 2 (CounterStrikeSharp).

- **Languages / Языки**: `en`, `ru`, `ua` (auto-fallback to English / авто‑fallback на английский)
- **.NET**: 8.0
- **Platform / Платформа**: x64

## Features / Возможности
- **Chat blocking / Блокировка чата**: blocks URLs, domains and IPs (unless whitelisted) / блокирует ссылки, домены и IP (если не в whitelist)
- **Name filtering / Фильтрация ников**: kick or rename when name contains a site/IP / кик или переименование если в нике сайт/IP
- **Whitelist / Белый список**: allow specific domains/IPs / разрешает конкретные домены/IP
- **Admin immunity / Иммунитет админов**: optional bypass for admins with `@css/generic` / опциональный обход для админов `@css/generic`
- **Logging / Логи**: main log + separate blocked-domains log / основной лог + отдельный лог заблокированного
- **domains.cfg (v3)**: your requested `*.tld` format + `DefaultName` / формат `*.tld` + `DefaultName`

## Installation / Установка
### Requirements / Требования
- Counter-Strike 2 Server / Сервер CS2
- CounterStrikeSharp installed / Установлен CounterStrikeSharp ([repo](https://github.com/roflmuffin/CounterStrikeSharp))
- .NET 8.0 runtime (to run) / .NET 8.0 runtime (для запуска)

### Install steps / Шаги установки
1. Copy `IPAndSiteBlocker.dll` to:  
   `csgo/addons/counterstrikesharp/plugins/IPAndSiteBlocker/`
2. Restart server or reload plugin:  
   `css_plugins reload IPAndSiteBlocker`
3. After first start the plugin will generate configs (see below) / После первого запуска плагин создаст конфиги (см. ниже)

## Building / Сборка
### Build from source / Сборка из исходников
```bash
dotnet publish -c Release
```
Or run `compile.bat` on Windows / Или запустите `compile.bat` на Windows.

Output (default) / Результат (по умолчанию): `bin/Release/net8.0/publish/`

## Configuration / Конфигурация
The plugin uses **two** config files:  
Плагин использует **два** конфиг‑файла:

### IPAndSiteBlocker.json (main) / IPAndSiteBlocker.json (основной)
- **Path / Путь**:  
  `csgo/addons/counterstrikesharp/configs/plugins/IPAndSiteBlocker/IPAndSiteBlocker.json`
- **ConfigVersion**: `3`

Example / Пример:
```json
{
  "language": "ru",
  "whitelist": [
    "yoursite.com",
    "192.168.1.1"
  ],
  "block_message": "{darkred}Заблокировано: Отправка IP-адресов или сайтов запрещена.",
  "name_action": 1,
  "rename_message": "{darkred}Ваше имя содержит заблокированный IP-адрес или сайт. Оно будет переименовано.",
  "admin_immunity": 1,
  "log_path": "addons/counterstrikesharp/logs/ip_site_blocker.log",
  "blocked_domains_log": "addons/counterstrikesharp/logs/blocked_domains.log",
  "auto_log_blocked": true,
  "ConfigVersion": 3
}
```

Options / Опции:
- **language**: `en` / `ru` / `ua`
- **whitelist**: list of allowed domains/IPs / список разрешённых доменов/IP
- **block_message**: message shown when chat is blocked / сообщение при блокировке чата
- **name_action**: `0` = kick / кик, `1` = rename / переименовать
- **rename_message**: message shown when player is renamed / сообщение при переименовании
- **admin_immunity**: `0` off / выкл, `1` on / вкл (permission: `@css/generic`)
- **log_path**: main log (relative to `csgo/`) / основной лог (относительно `csgo/`)
- **blocked_domains_log**: blocked domains log (relative to `csgo/`) / лог заблокированного (относительно `csgo/`)
- **auto_log_blocked**: `true/false`

### domains.cfg (v3 extra) / domains.cfg (доп. v3)
This file is in the **format you requested** (not JSON). The plugin reads it on plugin load.  
Это файл **в вашем формате** (не JSON). Плагин читает его при загрузке плагина.

- **Path / Путь**:  
  `csgo/addons/counterstrikesharp/configs/plugins/IPAndSiteBlocker/domains.cfg`
- If the file is missing, it will be created automatically / Если файла нет — будет создан автоматически

**Format / Формат**
- One entry per line / 1 запись на строку
- TLD masks (any of these forms are accepted): `*.com`, `.com`, `com`  
  Маски доменных зон (принимаются все эти формы): `*.com`, `.com`, `com`
- Default name line / Строка дефолтного имени:
  - `"DefaultName" "Player"`

Example / Пример:
```
*.pw
*.r
*.com
*.net
*.org
*.ru
*.ua

"DefaultName" "Player" - имя на которое будет меняться при срабатывание плагина.
```

Apply changes / Применение изменений:
- After editing `domains.cfg`, reload the plugin or restart the server / После правок `domains.cfg` перезагрузите плагин или сервер

## How whitelist works / Как работает whitelist
Whitelist check is **case-insensitive** and matches if:
Проверка whitelist **без учёта регистра** и срабатывает если:
- message/domain equals the whitelist item / значение равно элементу whitelist
- or contains it as a substring / или содержит его как подстроку

Tip / Совет: use specific domains (e.g. `steamcommunity.com`) to avoid over-whitelisting / используйте точные домены, чтобы не разрешить лишнее.

## Logging / Логи
- **Main log**: `log_path`
- **Blocked domains log**: `blocked_domains_log` (records `[URL]`, `[IP]`, `[Domain]`, `[NakedDomain]`)

## Colors / Цвета в сообщениях
You can use placeholders in `block_message` and `rename_message`:  
Можно использовать плейсхолдеры в `block_message` и `rename_message`:
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
