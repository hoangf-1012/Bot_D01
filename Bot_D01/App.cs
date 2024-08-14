using System.Text.Json;
using Bot_D01;
using Bot_D01.Schedule;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;

namespace Bot_D01
{
    public class App
    {
        public static async Task run()
        {
            await startconfig();
            //await App.run();

            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "MTIyNTA0NTUyODE2OTk0MzE0MA.GsqSOs.NVqBzaQpfoYC7wU4BGvsp8613zWBNCWZm_DrV4",
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.GuildMessages | DiscordIntents.MessageContents
            });

            var slashCommands = discord.UseSlashCommands();


            var bot = new Bot(discord, slashCommands);

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        public static async Task startconfig()
        {
            Console.WriteLine("Start connfiguration");
            // Tạo đối tượng dictionary
            var timestart = new Dictionary<int, string>
            {
                { 1, "06:45" },
                { 2, "07:40" },
                { 3, "08:40" },
                { 4, "09:40" },
                { 5, "10:40" },
                { 6, "13:00" },
                { 7, "13:55" },
                { 8, "14:55" },
                { 9, "15:55" },
                { 10, "16:50" }
            };

            var timeend = new Dictionary<int, string>
            {
                { 1, "07:35" },
                { 2, "08:30" },
                { 3, "09:30" },
                { 4, "10:30" },
                { 5, "11:30" },
                { 6, "13:50" },
                { 7, "14:45" },
                { 8, "15:45" },
                { 9, "16:45" },
                { 10, "17:40" }
            };

            var jsonObject = new
            {
                timestart = timestart,
                timeend = timeend
            };

            string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData");
            string jsonFilePath = Path.Combine(folderPath, "config.json");


            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            await File.WriteAllTextAsync(jsonFilePath, jsonString);


            await Schedule.Utilities.saveTokenAsync("user" , new LoginInfor("name","password","token"));

            Console.WriteLine($"File Config saved in '{jsonFilePath}'");
            Console.WriteLine("Bot running start");
        }
    }
}
