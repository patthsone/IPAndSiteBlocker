# IPAndSiteBlocker
A plugin that blocks websites and IP addresses (name + chat), with a configurable whitelist for allowed sites and IPs.
Плагин, блокирующий веб-сайты и IP-адреса (имя + чат), с настраиваемым белым списком разрешенных сайтов и IP-адресов.

🌍 **Multi-Language Support / Поддержка нескольких языков**: English, Russian, Ukrainian / Английский, русский, украинский

## Compatibility / Совместимость
✅ **CounterStrikeSharp API**: Compatible with **ANY** version (automatic) / Совместим с **ЛЮБОЙ** версией (автоматически)  
✅ **Auto-Updates**: Uses floating version to automatically work with new API releases / Использует плавающую версию для автоматической работы с новыми релизами API  
✅ **.NET 8.0**: Requires .NET 8.0 runtime / Требует .NET 8.0  
✅ **Platform**: x64 (64-bit) only / Только x64 (64-битная)  
✅ **Future-Proof**: Protected with defensive coding and fallback mechanisms / Защищён защитным кодированием и fallback механизмами

### 📚 Documentation / Документация

🚀 **[Quick Start Guide](QUICK_START.md)** - 5-minute setup guide for beginners (**NEW USERS START HERE**)  
🚀 **[Быстрый старт](QUICK_START.md)** - 5-минутное руководство для новичков (**НОВЫЕ ПОЛЬЗОВАТЕЛИ НАЧНИТЕ ЗДЕСЬ**)

⚡ **[Version Compatibility Summary](VERSION_COMPATIBILITY.md)** - Quick overview of version compatibility  
⚡ **[Краткое резюме совместимости](VERSION_COMPATIBILITY.md)** - Быстрый обзор совместимости версий

📖 **[Full Compatibility Guide](COMPATIBILITY.md)** - Detailed information about version compatibility and troubleshooting  
📖 **[Полное руководство по совместимости](COMPATIBILITY.md)** - Подробная информация о совместимости версий и решении проблем

🔄 **[Upgrade Guide](UPGRADE_GUIDE.md)** - Step-by-step instructions for updating from older versions  
🔄 **[Руководство по обновлению](UPGRADE_GUIDE.md)** - Пошаговые инструкции для обновления со старых версий

📝 **[Changelog](CHANGELOG.md)** - Version history and release notes  
📝 **[История изменений](CHANGELOG.md)** - История версий и примечания к релизам

## Installation / Установка

### Requirements / Требования
- Counter-Strike 2 Server / Сервер Counter-Strike 2
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) installed / CounterStrikeSharp установлен
- .NET 8.0 SDK (for building) / .NET 8.0 SDK (для сборки)

### Building from Source / Сборка из исходников
```bash
# Clone the repository / Клонируйте репозиторий
git clone https://github.com/patthsone/IPAndSiteBlocker.git
cd IPAndSiteBlocker

# Build the plugin / Соберите плагин
dotnet build -c Release

# The compiled DLL will be in bin/Release/net8.0/
# Скомпилированный DLL будет в bin/Release/net8.0/
```

### Installation Steps / Шаги установки
1. Copy `IPAndSiteBlocker.dll` to `csgo/addons/counterstrikesharp/plugins/IPAndSiteBlocker/`  
   Скопируйте `IPAndSiteBlocker.dll` в `csgo/addons/counterstrikesharp/plugins/IPAndSiteBlocker/`

2. Restart the server or use `css_plugins reload` command  
   Перезапустите сервер или используйте команду `css_plugins reload`

3. Configure the plugin by editing the auto-generated config file  
   Настройте плагин отредактировав автоматически созданный конфиг-файл

### Updating / Обновление
The plugin uses floating versions, so rebuilding with `dotnet build` will automatically use the latest compatible API version.  
Плагин использует плавающие версии, поэтому пересборка через `dotnet build` автоматически использует последнюю совместимую версию API.

## Features / Возможности
✅ **Multi-Language Support / Поддержка нескольких языков**: English, Russian, Ukrainian with automatic fallback / Английский, русский, украинский с автоматическим fallback
✅ **Strict Blocking Mode / Строгий режим блокировки**: Blocks ALL links and IPs except whitelisted ones / Блокирует ВСЕ ссылки и IP кроме белого списка
✅ **Auto-Logging Blocked Domains / Автоматическое логирование**: Automatically records all blocked domains to a file / Автоматически записывает все заблокированные домены в файл
✅ **Enhanced Name Checking / Улучшенная проверка имён**: Checks names at multiple events (spawn, team change, round start, etc.) / Проверяет имена при всех событиях (спавн, смена команды, старт раунда и т.д.)
✅ **Optimized Performance / Оптимизированная производительность**: Cached blocking results for faster processing / Кэширование результатов для быстрой обработки
✅ **Asynchronous Logging / Асинхронное логирование**: Non-blocking logging system for high-traffic servers / Неблокирующая система логирования
✅ **Universal Chat Handling / Универсальная обработка чата**: Single method handles both public and team chat / Один метод обрабатывает оба чата
✅ **Smart Domain Detection / Умное определение доменов**: Blocks naked domains (site.io, domain.xyz) without protocols / Блокирует домены без протоколов
✅ **Safe Config Updates / Безопасное обновление конфига**: Error handling prevents crashes during config updates / Обработка ошибок предотвращает краши
✅ **Admin Immunity / Иммунитет админов**: Admins with proper permissions can bypass blocking / Админы с правами могут обходить блокировку

## API for Developers / API для разработчиков

The plugin includes a standalone API that other developers can use to integrate IP and site blocking functionality into their own CounterStrikeSharp plugins or other .NET applications.

Плагин включает в себя автономный API, который другие разработчики могут использовать для интеграции функциональности блокировки IP и сайтов в свои собственные плагины CounterStrikeSharp или другие .NET приложения.

### Using the API / Использование API

#### Option 1: Reference the DLL / Вариант 1: Ссылка на DLL
1. Download `IPAndSiteBlockerAPI.dll` from the `API/bin/Release/net8.0/` folder
   Скачайте `IPAndSiteBlockerAPI.dll` из папки `API/bin/Release/net8.0/`

2. Add reference to your project / Добавьте ссылку в ваш проект
   ```bash
   dotnet add reference path/to/IPAndSiteBlockerAPI.dll
   ```

3. Use in your code / Используйте в коде
   ```csharp
   using IPAndSiteBlockerAPI;

   // Configure whitelist / Настройте белый список
   BlockChecker.Whitelist.Add("google.com");
   BlockChecker.Whitelist.Add("192.168.1.1");

   // Check if message is blocked / Проверьте, заблокировано ли сообщение
   bool isBlocked = BlockChecker.IsBlocked("Check this site: http://example.com");

   // Clean player name / Очистите имя игрока
   string cleanName = BlockChecker.CleanName("Player with badsite.net");
   ```

#### Option 2: Copy the source code / Вариант 2: Скопируйте исходный код
Copy `IPAndSiteBlockerAPI.cs` to your project and use it directly.
Скопируйте `IPAndSiteBlockerAPI.cs` в ваш проект и используйте напрямую.

### API Methods / Методы API

- **IsBlocked(string message)**: Returns true if the message contains blocked content / Возвращает true, если сообщение содержит заблокированный контент
- **CleanName(string name)**: Removes blocked domains/IPs from player names / Удаляет заблокированные домены/IP из имён игроков
- **IsWhitelisted(string message)**: Checks if content is in the whitelist / Проверяет, находится ли контент в белом списке

### Building the API / Сборка API
```bash
cd API
dotnet build -c Release
```

## Config / Конфигурация
The configuration file will be automatically generated in `csgo/addons/counterstrikesharp/configs/plugins/IPAndSiteBlocker/IPAndSiteBlocker.json`  
Конфигурационный файл автоматически создаётся в `csgo/addons/counterstrikesharp/configs/plugins/IPAndSiteBlocker/IPAndSiteBlocker.json`
```json
{
    "language": "en",
    "whitelist": [
        "yoursite.com",
        "192.168.1.1"
    ],
    "block_message": "{darkred}Blocked: Sending IP addresses or websites is not allowed.",
    "name_action": 1,
    "rename_message": "{darkred}Your name contains a blocked IP address or website. It will be renamed.",
    "admin_immunity": 1,
    "log_path": "addons/counterstrikesharp/logs/ip_site_blocker.log",
    "blocked_domains_log": "addons/counterstrikesharp/logs/blocked_domains.log",
    "auto_log_blocked": true,
    "ConfigVersion": 3
}
```

### Configuration Options / Опции конфигурации
- **language** / Язык: Plugin language ("en", "ru", "ua") - defaults to "en" if not set / Язык плагина ("en", "ru", "ua") - по умолчанию "en" если не указано
- **whitelist** / Белый список: Array of allowed domains/IPs that won't be blocked / Массив разрешённых доменов/IP, которые не будут блокироваться
- **block_message** / Сообщение о блокировке: Message shown when a message is blocked / Сообщение при блокировке сообщения
- **name_action** / Действие с именем: 0 = kick player / кикнуть игрока, 1 = rename player / переименовать игрока
- **rename_message** / Сообщение о переименовании: Message shown when player is renamed / Сообщение при переименовании игрока
- **admin_immunity** / Иммунитет админов: 0 = disabled / отключено, 1 = admins with @css/generic permission are immune / админы с правами @css/generic имеют иммунитет
- **log_path** / Путь к логу: Path to main log file (relative to csgo directory) / Путь к основному файлу логов (относительно csgo директории)
- **blocked_domains_log** / Лог заблокированных доменов: Path to blocked domains log file / Путь к файлу логов заблокированных доменов
- **auto_log_blocked** / Авто-логирование: true = automatically log all blocked domains / автоматически логировать все заблокированные домены, false = disabled / отключено

## How It Works / Как это работает

### Strict Blocking Mode / Строгий режим блокировки
The plugin now blocks **ALL** links, domains, and IP addresses by default, except those explicitly listed in the whitelist.  
Плагин теперь блокирует **ВСЕ** ссылки, домены и IP-адреса по умолчанию, кроме тех, которые явно указаны в белом списке.

**Example / Пример:**
- Whitelist / Белый список: `["yoursite.com", "192.168.1.1"]`
- ✅ Allowed / Разрешено: "yoursite.com", "192.168.1.1"
- ❌ Blocked / Заблокировано: Any other domain or IP / Любой другой домен или IP

### Auto-Logging Blocked Domains / Автоматическое логирование
All blocked attempts are automatically logged to `blocked_domains.log` with timestamp and type.  
Все заблокированные попытки автоматически записываются в `blocked_domains.log` с меткой времени и типом.

**Log Format / Формат лога:**
```
[2025-10-19 15:30:45] [URL] https://example.com
[2025-10-19 15:31:12] [IP] 192.168.1.100
[2025-10-19 15:32:05] [Domain] badsite.net
[2025-10-19 15:33:20] [NakedDomain] somesite.io
```

You can review this log file to see what domains/IPs players are attempting to share, and add legitimate ones to your whitelist.  
Вы можете просмотреть этот файл лога, чтобы увидеть, какие домены/IP игроки пытаются отправить, и добавить легитимные в белый список.

### Enhanced Name Checking / Улучшенная проверка имён
Player names are now checked at multiple events to ensure consistency:  
Имена игроков теперь проверяются при множественных событиях для обеспечения постоянства:
- ✅ Player Connect / Подключение игрока
- ✅ Player Spawn / Спавн игрока
- ✅ Team Change / Смена команды
- ✅ Round Start / Начало раунда
- ✅ Freeze Time End / Конец разминки
- ✅ Name Change / Смена имени

This fixes the issue where links would appear/disappear when joining during warmup or rounds.
Это исправляет проблему, когда ссылки появлялись/исчезали при заходе во время разминки или раунда.

### Language Support / Поддержка языков
The plugin supports multiple languages with automatic fallback to English if the selected language file is missing.
Плагин поддерживает несколько языков с автоматическим fallback на английский, если файл выбранного языка отсутствует.

**Supported Languages / Поддерживаемые языки:**
- **en** (English) - Default / По умолчанию
- **ru** (Русский)
- **ua** (Українська)

**Example configurations / Примеры конфигураций:**
```json
{"language": "ru"}  // Russian / Русский
{"language": "ua"}  // Ukrainian / Украинский
{"language": "en"}  // English / Английский
```

## Available colors / Доступные цвета
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
