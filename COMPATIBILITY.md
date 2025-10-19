# Compatibility Guide / Руководство по совместимости

## Version Management / Управление версиями

This plugin is designed to be compatible with new versions of CounterStrikeSharp.API automatically.  
Этот плагин разработан для автоматической совместимости с новыми версиями CounterStrikeSharp.API.

### How It Works / Как это работает

The project uses **floating version** (`Version="*"`) for CounterStrikeSharp.API, which means:  
Проект использует **плавающую версию** (`Version="*"`) для CounterStrikeSharp.API, что означает:

- ✅ Automatically uses the latest available version / Автоматически использует последнюю доступную версию
- ✅ No manual version updates needed / Не требуется ручное обновление версий
- ✅ Compatible with minor and patch updates / Совместим с минорными и патч-обновлениями

### Dependencies / Зависимости

| Package | Version | Notes |
|---------|---------|-------|
| CounterStrikeSharp.API | * (latest) | Core API - floating version |
| Newtonsoft.Json | 13.* | JSON serialization - major version locked |
| Microsoft.Extensions.Localization | 8.* | Localization - major version locked |

## API Usage / Использование API

This plugin uses the following CounterStrikeSharp API features:  
Этот плагин использует следующие функции CounterStrikeSharp API:

### Core Classes / Основные классы
- `BasePlugin` - Main plugin base class / Основной базовый класс плагина
- `BasePluginConfig` - Configuration base class / Базовый класс конфигурации
- `CCSPlayerController` - Player controller / Контроллер игрока
- `CommandInfo` - Command information / Информация о командах

### Modules / Модули
- `CounterStrikeSharp.API.Modules.Commands` - Command listeners / Слушатели команд
- `CounterStrikeSharp.API.Modules.Utils` - Utilities (ChatColors) / Утилиты (цвета чата)
- `CounterStrikeSharp.API.Modules.Admin` - Admin permissions / Админ-права
- `CounterStrikeSharp.API.Modules.Memory` - Memory operations / Операции с памятью

### Key API Methods / Ключевые методы API
- `AddCommandListener()` - Chat command interception / Перехват команд чата
- `GameEventHandler` attribute - Event handling / Обработка событий
- `Server.NextFrame()` - Delayed execution / Отложенное выполнение
- `NativeAPI.IssueServerCommand()` - Server commands / Команды сервера
- `Utilities.SetStateChanged()` - State updates / Обновление состояния
- `AdminManager.PlayerHasPermissions()` - Permission checks / Проверка прав

## Testing New Versions / Тестирование новых версий

When a new CounterStrikeSharp.API version is released, test these features:  
При выходе новой версии CounterStrikeSharp.API протестируйте эти функции:

### 1. Chat Blocking / Блокировка чата
- [ ] Send a message with URL in public chat / Отправить сообщение с URL в публичный чат
- [ ] Send a message with URL in team chat / Отправить сообщение с URL в командный чат
- [ ] Send a message with IP address / Отправить сообщение с IP-адресом
- [ ] Send a whitelisted URL (should work) / Отправить URL из белого списка (должно работать)

### 2. Name Checking / Проверка имён
- [ ] Connect with name containing URL / Подключиться с именем содержащим URL
- [ ] Change name to include IP address / Сменить имя на содержащее IP-адрес
- [ ] Join during warmup / Зайти во время разминки
- [ ] Join during active round / Зайти во время активного раунда
- [ ] Change team with blocked name / Сменить команду с заблокированным именем

### 3. Admin Features / Функции админа
- [ ] Admin with @css/generic can send URLs / Админ с @css/generic может отправлять URL
- [ ] Admin immunity works correctly / Иммунитет админа работает корректно
- [ ] Non-admin gets blocked / Не-админ получает блокировку

### 4. Configuration / Конфигурация
- [ ] Config auto-generates on first load / Конфиг авто-создаётся при первой загрузке
- [ ] Config updates work / Обновление конфига работает
- [ ] All config options apply correctly / Все опции конфига применяются корректно

### 5. Logging / Логирование
- [ ] Main log file creates correctly / Основной лог-файл создаётся корректно
- [ ] Blocked domains log works / Лог заблокированных доменов работает
- [ ] No errors in console / Нет ошибок в консоли

## Breaking Changes to Watch For / На что обратить внимание

If the plugin stops working after an API update, check:  
Если плагин перестал работать после обновления API, проверьте:

### Common Issues / Частые проблемы

1. **Event Names Changed / Изменились имена событий**
   - Look for: `EventRoundStart`, `EventPlayerConnectFull`, etc.
   - Solution: Update event names in `[GameEventHandler]` methods

2. **ChatColors Moved / Перемещены ChatColors**
   - Look for: `ChatColors.DarkRed`, `ChatColors.White`, etc.
   - Solution: Update import namespace or color access

3. **AdminManager API Changed / Изменился API AdminManager**
   - Look for: `AdminManager.PlayerHasPermissions()`
   - Solution: Check new permission check method

4. **NativeAPI Changes / Изменения NativeAPI**
   - Look for: `NativeAPI.IssueServerCommand()`
   - Solution: Use new server command method

5. **State Change API / API изменения состояния**
   - Look for: `Utilities.SetStateChanged()`
   - Solution: Check new state update method

## Troubleshooting / Решение проблем

### Plugin Won't Load / Плагин не загружается
```bash
# Check CounterStrikeSharp version
# Проверьте версию CounterStrikeSharp
cat addons/counterstrikesharp/counterstrikesharp.json

# Rebuild plugin
# Пересоберите плагин
dotnet build -c Release
```

### Compilation Errors / Ошибки компиляции
```bash
# Clean and rebuild
# Очистите и пересоберите
dotnet clean
dotnet restore
dotnet build -c Release
```

### Runtime Errors / Ошибки времени выполнения
- Check console for error messages / Проверьте консоль на сообщения об ошибках
- Check log files in `csgo/addons/counterstrikesharp/logs/` / Проверьте лог-файлы
- Verify all dependencies are installed / Убедитесь что все зависимости установлены

## Future-Proofing Best Practices / Лучшие практики для будущего

This plugin follows these practices for better compatibility:  
Этот плагин следует этим практикам для лучшей совместимости:

1. ✅ **Defensive Coding** - Try-catch blocks prevent crashes / Блоки try-catch предотвращают краши
2. ✅ **Null Checks** - Always validate objects before use / Всегда проверяйте объекты перед использованием
3. ✅ **Error Handling** - Graceful degradation on errors / Плавная деградация при ошибках
4. ✅ **Async Operations** - Non-blocking I/O operations / Неблокирующие операции ввода-вывода
5. ✅ **Caching** - Minimize repeated API calls / Минимизация повторных вызовов API
6. ✅ **Standard Patterns** - Follow CounterStrikeSharp conventions / Следование конвенциям CounterStrikeSharp

### Defensive Mechanisms / Защитные механизмы

The plugin implements multiple layers of protection:  
Плагин реализует множественные уровни защиты:

#### 1. Safe API Wrappers / Безопасные обертки API
```csharp
// Example: Safe admin permission check
private bool HasAdminImmunity(CCSPlayerController player)
{
    try
    {
        return AdminManager.PlayerHasPermissions(player, "@css/generic");
    }
    catch (Exception ex)
    {
        // If API changed, log and continue safely
        LogMessageAsync($"Warning: Admin check failed: {ex.Message}");
        return false;
    }
}
```

#### 2. Protected Event Handlers / Защищенные обработчики событий
All game event handlers are wrapped in try-catch blocks to prevent crashes:  
Все обработчики игровых событий обернуты в блоки try-catch для предотвращения крашей:

- OnRoundStart
- OnRoundFreezeEnd
- OnPlayerConnectFull
- OnPlayerSpawn
- OnPlayerTeam
- OnPlayerChangeName

#### 3. Multiple Fallback Mechanisms / Множественные fallback механизмы
```csharp
// Example: Player identification with fallbacks
private string GetPlayerIdentifier(CCSPlayerController player)
{
    try
    {
        return player.SteamID.ToString();  // Try SteamID first
    }
    catch
    {
        try
        {
            return $"User{player.UserId}";  // Fallback to UserId
        }
        catch
        {
            return "UnknownPlayer";  // Final fallback
        }
    }
}
```

#### 4. API Version Logging / Логирование версии API
On plugin load, the API version is logged for troubleshooting:  
При загрузке плагина версия API логируется для диагностики:
```
[2025-10-19 20:45:10] IPAndSiteBlocker v0.2.3 loading...
[2025-10-19 20:45:10] CounterStrikeSharp API: 1.0.123
[2025-10-19 20:45:10] Chat listeners registered successfully.
[2025-10-19 20:45:10] IPAndSiteBlocker loaded successfully!
```

#### 5. Critical Method Protection / Защита критичных методов
Methods that interact directly with the game state are protected:  
Методы, которые напрямую взаимодействуют с состоянием игры, защищены:

- **RenamePlayer** - Protected with multiple try-catch layers / Защищен множественными слоями try-catch
- **CheckAndHandlePlayerName** - Safe validation and handling / Безопасная валидация и обработка
- **OnPlayerChat** - Protected chat message sending / Защищенная отправка сообщений в чат

These mechanisms ensure the plugin continues to function even if:  
Эти механизмы гарантируют что плагин продолжит работать даже если:
- ✅ API methods are renamed / Методы API переименованы
- ✅ Method signatures change / Сигнатуры методов изменены
- ✅ Properties are moved or removed / Свойства перемещены или удалены
- ✅ Event structures change / Структуры событий изменены
- ✅ Admin system is updated / Система админов обновлена

## Getting Help / Получение помощи

If you encounter compatibility issues:  
Если вы столкнулись с проблемами совместимости:

1. Check CounterStrikeSharp [GitHub Issues](https://github.com/roflmuffin/CounterStrikeSharp/issues)
2. Review [CounterStrikeSharp Documentation](https://docs.cssharp.dev/)
3. Check [API Changes/Breaking Changes](https://github.com/roflmuffin/CounterStrikeSharp/releases) in release notes
4. Join CounterStrikeSharp Discord for community support

## Version History / История версий

| Plugin Version | Min CSS API Version | Notes |
|----------------|---------------------|-------|
| 0.2.2 | Any | Current version - floating API version |
| 0.2.x | Any | Compatible with all CSS versions |
| 0.1.x | Any | Initial release |

