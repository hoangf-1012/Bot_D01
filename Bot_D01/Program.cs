using Bot_D01;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;

class Program
{
    private static async Task Main(string[] args)
    {
        await App.startconfig();
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
}
