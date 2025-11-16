# IPAndSiteBlockerAPI

Бібліотека .NET для блокування повідомлень, що містять URL, IP-адреси та домени, з підтримкою білого списку. Розроблена для використання в плагінах CounterStrikeSharp.

## Можливості

- Виявляє та блокує повідомлення з URL, IP та доменами
- Підтримує білий список дозволених сайтів/адрес
- Очищає імена гравців, видаляючи заблокований контент
- Сумісна з dependency injection в плагінах CounterStrikeSharp

## Встановлення

Додайте IPAndSiteBlockerAPI.dll до посилань вашого проекту або використовуйте NuGet, якщо доступний.

## Використання

### Базове використання

```csharp
using IPAndSiteBlockerAPI;

// Створення екземпляра з білим списком
var whitelist = new List<string> { "google.com", "192.168.1.1" };
var blockChecker = new BlockChecker(whitelist);

// Перевірка, чи заблоковане повідомлення
bool isBlocked = blockChecker.IsBlocked("Подивіться http://example.com");

// Очищення імені гравця
string cleanName = blockChecker.CleanName("Гравець з http://bad.com");
```

### У плагіні CounterStrikeSharp

Зареєструйте сервіс у методі `Load` вашого плагіна:

```csharp
public override void Load(bool hotReload)
{
    // Реєстрація BlockChecker як singleton
    Services.RegisterSingleton<IBlockChecker>(() => new BlockChecker(new List<string> { "allowed.com" }));
}
```

Потім ін'єктуйте його у ваші класи:

```csharp
public class MyPlugin : BasePlugin
{
    private readonly IBlockChecker _blockChecker;

    public MyPlugin(IBlockChecker blockChecker)
    {
        _blockChecker = blockChecker;
    }

    // Використовуйте _blockChecker у ваших методах
}
```

## Довідник API

### Інтерфейс IBlockChecker

- `List<string> Whitelist { get; set; }` - Список елементів білого списку
- `bool IsBlocked(string message)` - Повертає true, якщо повідомлення містить заблокований контент
- `string CleanName(string name)` - Видаляє заблокований контент з імені
- `bool IsWhitelisted(string message)` - Перевіряє, чи знаходиться повідомлення у білому списку

### Клас BlockChecker

Реалізує `IBlockChecker`. Конструктор приймає опціональний `IEnumerable<string>` для початкового білого списку.

## Примітки

- Шаблони Regex попередньо скомпільовані для продуктивності
- Перевіряє поширені розширення доменів
- Зіставлення без урахування регістру