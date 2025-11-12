using IPAndSiteBlockerAPI;

class Program
{
    static void Main()
    {
        // Настройка whitelist
        BlockChecker.Whitelist.Add("google.com");
        BlockChecker.Whitelist.Add("192.168.1.1");

        // Примеры сообщений
        string[] messages = {
            "Check out this site: http://example.com",
            "My IP is 192.168.1.100",
            "Visit google.com for more info",
            "This is a safe message"
        };

        foreach (var msg in messages)
        {
            bool blocked = BlockChecker.IsBlocked(msg);
            Console.WriteLine($"Message: '{msg}' - Blocked: {blocked}");
        }

        // Пример очистки имени
        string playerName = "Player with http://bad.com in name";
        string cleanedName = BlockChecker.CleanName(playerName);
        Console.WriteLine($"Original name: '{playerName}' - Cleaned: '{cleanedName}'");
    }
}