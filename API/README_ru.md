# IPAndSiteBlockerAPI

Библиотека .NET для блокировки сообщений, содержащих URL, IP-адреса и домены, с поддержкой белого списка. Разработана для использования в плагинах CounterStrikeSharp.

## Возможности

- Обнаруживает и блокирует сообщения с URL, IP и доменами
- Поддерживает белый список разрешенных сайтов/адресов
- Очищает имена игроков, удаляя заблокированный контент
- Совместима с dependency injection в плагинах CounterStrikeSharp

## Установка

Добавьте IPAndSiteBlockerAPI.dll в ссылки вашего проекта или используйте NuGet, если доступен.

## Использование

### Базовое использование

```csharp
using IPAndSiteBlockerAPI;

// Создание экземпляра с белым списком
var whitelist = new List<string> { "google.com", "192.168.1.1" };
var blockChecker = new BlockChecker(whitelist);

// Проверка, заблокировано ли сообщение
bool isBlocked = blockChecker.IsBlocked("Посмотри http://example.com");

// Очистка имени игрока
string cleanName = blockChecker.CleanName("Игрок с http://bad.com");
```

### В плагине CounterStrikeSharp

Зарегистрируйте сервис в методе `Load` вашего плагина:

```csharp
public override void Load(bool hotReload)
{
    // Регистрация BlockChecker как singleton
    Services.RegisterSingleton<IBlockChecker>(() => new BlockChecker(new List<string> { "allowed.com" }));
}
```

Затем инъектируйте его в ваши классы:

```csharp
public class MyPlugin : BasePlugin
{
    private readonly IBlockChecker _blockChecker;

    public MyPlugin(IBlockChecker blockChecker)
    {
        _blockChecker = blockChecker;
    }

    // Используйте _blockChecker в ваших методах
}
```

## Справочник API

### Интерфейс IBlockChecker

- `List<string> Whitelist { get; set; }` - Список элементов белого списка
- `bool IsBlocked(string message)` - Возвращает true, если сообщение содержит заблокированный контент
- `string CleanName(string name)` - Удаляет заблокированный контент из имени
- `bool IsWhitelisted(string message)` - Проверяет, находится ли сообщение в белом списке

### Класс BlockChecker

Реализует `IBlockChecker`. Конструктор принимает опциональный `IEnumerable<string>` для начального белого списка.

## Примечания

- Шаблоны Regex предварительно скомпилированы для производительности
- Проверяет распространенные расширения доменов
- Сопоставление без учета регистра