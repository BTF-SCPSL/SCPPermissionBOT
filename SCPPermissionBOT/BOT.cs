using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    private DiscordSocketClient _client;
    private const string ConfigFolder = "Configs";
    private const string ConfigFile = "Configs/TOKEN.yml";

    static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        string token = SetupToken();
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Токен не найден в TOKEN.yml. Укажите токен и перезапустите бота.");
            return;
        }

        _client = new DiscordSocketClient();
        _client.Log += LogAsync;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        Console.WriteLine("Бот запущен. Нажмите Enter для завершения.");
        await Task.Delay(-1);
    }

    private string SetupToken()
    {
        if (!Directory.Exists(ConfigFolder))
        {
            Directory.CreateDirectory(ConfigFolder);
            Console.WriteLine($"Создана папка {ConfigFolder}.");
        }

        if (!File.Exists(ConfigFile))
        {
            File.WriteAllText(ConfigFile, "TOKEN = 'PUT_YOUR_TOKEN'");
            Console.WriteLine($"Создан файл {ConfigFile}. Укажите токен и перезапустите бота.");
            return null;
        }

        string[] lines = File.ReadAllLines(ConfigFile);
        foreach (string line in lines)
        {
            if (line.StartsWith("TOKEN"))
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    return parts[1].Trim().Trim('\'');
                }
            }
        }

        Console.WriteLine("Не удалось найти токен в файле TOKEN.yml. Укажите токен и перезапустите бота.");
        return null;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }
}