using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    private DiscordSocketClient _client;
    private const string ConfigFolder = "Configs";
    private const string ConfigFile = "Configs/TOKEN.yml";
    private const string ApiUrl = "http://localhost:9199/"; // Адрес API плагина

    static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        // Загружаем токен из файла
        string token = SetupToken();
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Токен не найден в TOKEN.yml. Укажите токен и перезапустите бота.");
            return;
        }

        _client = new DiscordSocketClient();
        _client.Log += LogAsync;
        _client.SlashCommandExecuted += SlashCommandHandlerAsync;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        Console.WriteLine("Бот запущен. Нажмите Enter для завершения.");
        await Task.Delay(-1);
    }

    private string SetupToken()
    {
        // Создаём папку Configs, если её нет
        if (!Directory.Exists(ConfigFolder))
        {
            Directory.CreateDirectory(ConfigFolder);
            Console.WriteLine($"Создана папка {ConfigFolder}.");
        }

        // Создаём файл TOKEN.yml, если его нет
        if (!File.Exists(ConfigFile))
        {
            File.WriteAllText(ConfigFile, "TOKEN = 'PUT_YOUR_TOKEN'");
            Console.WriteLine($"Создан файл {ConfigFile}. Укажите токен и перезапустите бота.");
            return null;
        }

        // Читаем токен из файла
        string[] lines = File.ReadAllLines(ConfigFile);
        foreach (string line in lines)
        {
            if (line.StartsWith("TOKEN"))
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    return parts[1].Trim().Trim('\''); // Убираем пробелы и кавычки
                }
            }
        }

        Console.WriteLine("Не удалось найти токен в файле TOKEN.yml. Укажите токен и перезапустите бота.");
        return null;
    }

    private async Task SlashCommandHandlerAsync(SocketSlashCommand command)
    {
        if (command.CommandName == "sendcommand")
        {
            string cmd = command.Data.Options.First().Value.ToString();
            await command.RespondAsync($"Выполняю команду: {cmd}...");

            string result = await SendCommandToPluginAsync(cmd);
            await command.FollowupAsync(result);
        }
    }

    private async Task<string> SendCommandToPluginAsync(string cmd)
    {
        HttpClient client = new HttpClient();
        try
        {
            var content = new StringContent(cmd, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(ApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"Ошибка: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            return $"Ошибка соединения с плагином: {ex.Message}";
        }
        finally
        {
            client.Dispose();
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }
}
