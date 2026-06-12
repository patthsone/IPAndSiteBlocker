<<<<<<< Updated upstream
![Total Downloads](https://img.shields.io/github/downloads/patthsone/IPAndSiteBlocker/total?style=flat&label=Total%20Downloads&labelColor=rgba(0%2C%2070%2C%20114%2C%201)&color=rgba(255%2C%20255%2C%20255%2C%201)) 
![Latest Release](https://img.shields.io/github/v/release/patthsone/IPAndSiteBlocker?style=flat&label=Latest%20Release&labelColor=rgba(0%2C%2070%2C%20114%2C%201)&color=rgba(255%2C%20255%2C%20255%2C%201)) 

[Discord сервер](https://discord.gg/VmJzFBD6wf)
## IPAndSiteBlocker
Blocks IP addresses and websites in **chat** and **player names** for Counter-Strike 2 servers running CounterStrikeSharp.  
Блокирует IP‑адреса и сайты в **чате** и **никах** игроков на сервере Counter‑Strike 2 (CounterStrikeSharp).
=======
# IPAndSiteBlocker
>>>>>>> Stashed changes

Release-версия плагина для CounterStrikeSharp: блокирует IP-адреса, ссылки и домены в чате и никах игроков.

## Требования

- Counter-Strike 2 server
- CounterStrikeSharp
- .NET 8 SDK для сборки
- `CounterStrikeSharp.API.dll` из установленного CounterStrikeSharp

## Сборка

Самый правильный вариант — собирать против DLL из твоего установленного CounterStrikeSharp.

### Вариант 1

Положи `CounterStrikeSharp.API.dll` рядом с `compile.bat`, затем запусти:

```bat
compile.bat
```

### Вариант 2

Укажи путь вручную:

```bat
set CSSHARP_API=C:\server\csgo\addons\counterstrikesharp\api\CounterStrikeSharp.API.dll
compile.bat
```

## Установка

После сборки скопируй папку:

```text
compile/addons/counterstrikesharp/plugins/IPAndSiteBlocker
```

в:

```text
csgo/addons/counterstrikesharp/plugins/IPAndSiteBlocker
```

Перезагрузи сервер или выполни:

```text
css_plugins reload IPAndSiteBlocker
```

## Конфиг

Конфиг создаётся автоматически:

```text
csgo/addons/counterstrikesharp/configs/plugins/IPAndSiteBlocker/IPAndSiteBlocker.json
```

Основные параметры:

```json
{
  "language": "ru",
  "whitelist": [
    "luxecs2.ru",
    "t.me/luxecs2",
    "discord.gg",
    "steamcommunity.com"
  ],
  "block_message": "{darkred}Запрещено отправлять IP-адреса и ссылки на сторонние сайты.",
  "name_action": 1,
  "rename_message": "{darkred}Ваш ник содержит запрещённый сайт или IP. Ник был изменён.",
  "admin_immunity": true,
  "admin_permission": "@css/generic",
  "log_path": "addons/counterstrikesharp/logs/ip_site_blocker.log",
  "blocked_domains_log": "addons/counterstrikesharp/logs/blocked_domains.log",
  "auto_log_blocked": true,
  "check_names_on_events": true,
  "ConfigVersion": 4
}
```

`name_action`:

- `0` — кикать игрока за запрещённый ник
- `1` — переименовывать игрока

## domains.cfg

Файл создаётся автоматически:

```text
csgo/addons/counterstrikesharp/configs/plugins/IPAndSiteBlocker/domains.cfg
```

Формат:

```text
*.com
*.ru
*.gg

"DefaultName" "Player"
```

После изменения `domains.cfg` перезагрузи плагин.

## Что исправлено в release

- Проект собирается через локальный `CounterStrikeSharp.API.dll`, а не через NuGet.
- Namespace, class name, AssemblyName и папки приведены к `IPAndSiteBlocker`.
- Убран старый мусор из `compile.bat` от GameTimeBonus.
- Whitelist стал безопаснее: проверяет точный домен или поддомен, а не простой `Contains`.
- Добавлен `admin_permission` вместо жёстко зашитого `@css/generic`.
- Логи пишутся через очередь без `async void`.
- Добавлен `ConfigVersion` 4.
- Добавлены `ru/en/ua` переводы.
