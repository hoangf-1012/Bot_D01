using DSharpPlus.EventArgs;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using Bot_D01.Schedule;
using System.Globalization;


namespace Bot_D01
{
    public class Bot
    {
        private readonly DiscordClient _discord;
        private readonly SlashCommandsExtension _slashCommands;

        public Bot(DiscordClient discord, SlashCommandsExtension slashCommands)
        {
            _discord = discord;
            _discord.MessageCreated += OnMessageCreated;
            _discord.ComponentInteractionCreated += OnComponentInteractionCreated;
            _discord.ModalSubmitted += OnModalSubmitted;
            _slashCommands = slashCommands;
            _slashCommands.RegisterCommands<Commands>();
        }

        private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Message.Author.IsBot)
            {
                return;
            }
            if (e.Message.Content == "!hello")
            {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                builder.Color = new DiscordColor(169, 216, 255);
                builder.AddField("Hãy nhớ", "“Con gà đứng ở lối đi, vì sao? Để tìm con gà khác, chắc chắn là như thế!”");
                builder.ImageUrl = e.Message.Author.AvatarUrl;
                builder.WithThumbnail(e.Message.Author.AvatarUrl);
                builder.Title = "Meow";
                builder.Url = e.Message.Author.AvatarUrl;
                builder.Description = "Hello, World!";
                builder.Author = new DiscordEmbedBuilder.EmbedAuthor();
                builder.Author.IconUrl = e.Message.Author.AvatarUrl;
                builder.Author.Name = e.Message.Author.Username;
                builder.Author.Url = e.Message.Author.AvatarUrl;
                builder.Footer = new DiscordEmbedBuilder.EmbedFooter();
                builder.Footer.IconUrl = e.Message.Author.AvatarUrl;
                builder.Footer.Text = "Have a good day!";

                var embed = builder.Build();
                await e.Message.RespondAsync(embed);
            }

        }



        private static async Task OnModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
        {
            try
            {
                if (e.Interaction.Data.CustomId == ">login_modal")
                {
                    var studentId = e.Values["studentid"];
                    var password = e.Values["password"];

                    var passwordMd5 = Schedule.Utilities.computeMd5Hash(password);

                    var sessionString = await Schedule.Utilities.getSessionStringAsync("login.aspx");
                    var result = await CookieTaker.GetCookie(studentId, passwordMd5, sessionString);

                    var user = e.Interaction.User;

                    if (result.userName != "")
                    {
                        await Schedule.Utilities.saveTokenAsync(user.Username, result);

                        var embed = new DiscordEmbedBuilder
                        {
                            Title = $"Hi {user.Username}!",
                            Description = "Bạn đã đăng nhập thành công vào dktc!",
                            Color = DiscordColor.Green
                        };

                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .AddEmbed(embed));
                    }
                    else
                    {
                        var errorEmbed = new DiscordEmbedBuilder
                        {
                            Title = $"Hi {user.Username}!",
                            Description = "Đăng nhập thất bại vào dktc!",
                            Color = DiscordColor.Red
                        };

                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .AddEmbed(errorEmbed));
                    }
                }
            }
            catch (Exception ex)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "Lỗi",
                    Description = $"Đã xảy ra lỗi: {ex.Message}. Vui lòng thử lại.",
                    Color = DiscordColor.Orange
                };

                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .AddEmbed(errorEmbed));
            }
        }

        private static async Task OnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            if (e.Id == "view_tomorrow_schedule")
            {
                var user = e.Interaction.User;

                var scheduleResult = await Schedule.Utilities.GetSchedule(user.Username);

                if (!scheduleResult.Success && scheduleResult.Error == "noInfo")
                { 
                    DiscordInteractionResponseBuilder modal = ModalBuilder.Create("login_modal")
                    .WithTitle("Dăng nhập DKTC")
                    .AddComponents(new TextInputComponent("Mã sinh viên", "studentid", "DTC123"))
                    .AddComponents(new TextInputComponent("Mật khẩu", "password", "..."))
                    .AddComponents(new TextInputComponent("Gì cũng được", "", "Cứ gửi đi không sao đâu😜!", "", false));
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                }

                DateTime searchDate = (DateTime.Now).AddDays(1);

               

                if (scheduleResult.Schedule != null)
                {
                    var entry = scheduleResult.Schedule.Find(s => s.Date.Date == searchDate.Date);
                    if (entry != null)
                    {
                        DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                        builder.Color = DiscordColor.Yellow;
                        builder.Title = $"Ngày mai bạn có {entry.Lessons!.Count} lịch";
                        builder.Description = "lịch học";
                        builder.Author = new DiscordEmbedBuilder.EmbedAuthor();
                        builder.Author.IconUrl = e.Message.Author.AvatarUrl;
                        builder.Author.Name = $"Hi! {user.Username}";
                        builder.Author.Url = e.Message.Author.AvatarUrl;

                        foreach (var lesson in entry.Lessons)
                        {
                            builder.AddField($"{lesson.SubjectName}", $"{lesson.timeStart} - {lesson.timeEnd} | {lesson.Address} | {lesson.teacher}", true);

                        }
                        var embed = builder.Build();

                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .AddEmbed(embed));
                    }
                    else
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = $"Hi {user.Username}!",
                            Description = "Mai bạn rảnh!",
                            Color = DiscordColor.Green
                        };

                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .AddEmbed(embed));
                    }
                }
                else
                {
                    var errorEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Hi {user.Username}!",
                        Description = "Không có dữ liệu lịch, thử '/tomorow' hoặc '/reset' để lải lại lịch của bạn",
                        Color = DiscordColor.Orange
                    };

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(errorEmbed));
                }
            }

            if (e.Id == "view_week_schedule")
            {
                var user = e.Interaction.User;

                var scheduleResult = await Schedule.Utilities.GetSchedule(user.Username);

                if (!scheduleResult.Success && scheduleResult.Error == "noInfo")
                {
                    var modal = ModalBuilder.Create("login_modal")
                        .WithTitle("Đăng nhập DKTC")
                        .AddComponents(new TextInputComponent("Mã sinh viên", "studentid", "DTC123"))
                        .AddComponents(new TextInputComponent("Mật khẩu", "password", "..."))
                        .AddComponents(new TextInputComponent("Gì cũng được", "", "Cứ gửi đi không sao đâu😜!", "", false));

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    return;
                }

                if (scheduleResult.Schedule != null)
                {
                    DateTime today = DateTime.UtcNow.Date;
                    DayOfWeek currentDayOfWeek = today.DayOfWeek;

                    int daysSinceMonday = (int)currentDayOfWeek - (int)DayOfWeek.Monday;
                    if (daysSinceMonday < 0)
                    {
                        daysSinceMonday += 7;
                    }

                    DateTime monday = today.AddDays(-daysSinceMonday);
                    DateTime[] weekDays = new DateTime[7];
                    for (int i = 0; i < 7; i++)
                    {
                        weekDays[i] = monday.AddDays(i);
                    }


                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Lịch học tuần này của {user.Username}",
                    };
                    embed.Author = new DiscordEmbedBuilder.EmbedAuthor();
                    embed.Author.IconUrl = user.AvatarUrl;
                    embed.Author.Name =$"Hi! {user.Username}";
                    embed.Author.Url = user.AvatarUrl;
                    embed.Color = new DiscordColor(169, 216, 255);

                    foreach (var searchDate in weekDays)
                    {
                        var entry = scheduleResult.Schedule.Find(s => s.Date.Date == searchDate.Date);
                        CultureInfo vietnamCulture = new CultureInfo("vi-VN");
                        string thisDate = searchDate.ToString("dddd, dd MMMM yyyy", vietnamCulture);

                        if (searchDate.Date == today.Date)
                        {
                            thisDate = "Hôm nay";
                        }

                        // Nổi bật ngày với nền và in đậm
                        var fieldTitle = searchDate.Date == today.Date
                            ? $"**📅 {thisDate}**" // Nổi bật ngày hôm nay với emoji và in đậm
                            : $"**{thisDate}**"; // In đậm các ngày khác

                        if (entry != null)
                        {


                            embed.AddField(fieldTitle,
                                string.Join("\n\n", entry.Lessons.Select(lesson =>
                                    $"{lesson.SubjectName.Split('(')[0].Trim()} | {lesson.timeStart} - {lesson.timeEnd} | {lesson.Address}\n")));
                        }
                        else
                        {
                            embed.AddField(fieldTitle,
                                "Bạn rảnh");
                        }
                    }

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed.Build()));
                }
                else
                {
                    var errorEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Hi {user.Username}!",
                        Description = "Không có dữ liệu lịch. Thử '/tomorrow' hoặc '/reset' để lặp lại lịch của bạn",
                        Color = DiscordColor.Orange
                    };

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(errorEmbed));
                }
            }

            if (e.Id == "view_today_schedule")
            {
                var user = e.Interaction.User;

                var scheduleResult = await Schedule.Utilities.GetSchedule(user.Username);

                if (!scheduleResult.Success && scheduleResult.Error == "noInfo")
                {
                    DiscordInteractionResponseBuilder modal = ModalBuilder.Create("login_modal")
                    .WithTitle("Dăng nhập DKTC")
                    .AddComponents(new TextInputComponent("Mã sinh viên", "studentid", "DTC123"))
                    .AddComponents(new TextInputComponent("Mật khẩu", "password", "..."))
                    .AddComponents(new TextInputComponent("Gì cũng được", "", "Cứ gửi đi không sao đâu😜!", "", false));
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                }

                DateTime searchDate = DateTime.Now;



                if (scheduleResult.Schedule != null)
                {
                    var entry = scheduleResult.Schedule.Find(s => s.Date.Date == searchDate.Date);
                    if (entry != null)
                    {
                        DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                        builder.Color = DiscordColor.Yellow;
                        builder.Title = $"Hôm nay bạn có {entry.Lessons!.Count} lịch";
                        builder.Description = "lịch học";
                        builder.Author = new DiscordEmbedBuilder.EmbedAuthor();
                        builder.Author.IconUrl = e.Message.Author.AvatarUrl;
                        builder.Author.Name = $"Hi! {user.Username}";
                        builder.Author.Url = e.Message.Author.AvatarUrl;

                        foreach (var lesson in entry.Lessons)
                        {
                            builder.AddField($"{lesson.SubjectName}", $"{lesson.timeStart} - {lesson.timeEnd} | {lesson.Address} | {lesson.teacher}", true);

                        }
                        var embed = builder.Build();

                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .AddEmbed(embed));
                    }
                    else
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = $"Hi {user.Username}!",
                            Description = "Hôm nay bạn rảnh!",
                            Color = DiscordColor.Green
                        };

                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .AddEmbed(embed));
                    }
                }
                else
                {
                    var errorEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Hi {user.Username}!",
                        Description = "Không có dữ liệu lịch, thử '/today' hoặc '/reset' để lải lại lịch của bạn",
                        Color = DiscordColor.Orange
                    };

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(errorEmbed));
                }
            }
        }
    }
}
