using IPAndSiteBlockerAPI;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var whitelist = new List<string> { "google.com", "192.168.1.1" };
        var blockChecker = new BlockChecker(whitelist);

        string[] messages = {
            "Check out this site: http://example.com",
            "My IP is 192.168.1.100",
            "Visit google.com for more info",
            "This is a safe message"
        };

        foreach (var msg in messages)
        {
            bool blocked = blockChecker.IsBlocked(msg);
            Console.WriteLine($"Message: '{msg}' - Blocked: {blocked}");
        }

        string playerName = "Player with http://bad.com in name";
        string cleanedName = blockChecker.CleanName(playerName);
        Console.WriteLine($"Original name: '{playerName}' - Cleaned: '{cleanedName}'");
    }
}