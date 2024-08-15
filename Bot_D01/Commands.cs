
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus;
using Bot_D01.Schedule;
using DSharpPlus.ModalCommands;
using Newtonsoft.Json.Linq;
namespace Bot_D01
{
    public class Commands : ApplicationCommandModule
    {
        [SlashCommand("test", "Lệnh mẫu /test")]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("Hello, World!");
        }


        [SlashCommand("menu", "Danh sách tùy chọn")]
        public async Task MenuCommand(InteractionContext ctx)
        {
            var user = ctx.User;


            var embed = new DiscordEmbedBuilder
            {
                Title = $"Hi! {user.Username}!",
                Description = "Chọn một tùy chọn từ các nút bên dưới.\n||'/help' để xem danh sách lệnh.\n",
                Color = DiscordColor.Blurple
            };

            var messageBuilder = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Secondary, "view_today_schedule", "Lịch hôm nay"),
                    new DiscordButtonComponent(ButtonStyle.Success, "view_tomorrow_schedule", "Lịch ngày mai"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "view_week_schedule", "Lịch tuần"),
                });

            await ctx.CreateResponseAsync(messageBuilder);
        }

        [SlashCommand("reset", "Tải lại dữ liệu")]
        public async Task ResetCommand(InteractionContext ctx)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var user = ctx.User;
                    string path = Path.Combine(Schedule.Utilities.getPath(), "listUserInfor.json");
                    string jsonString = File.ReadAllText(path);
                    JObject jsonObject = JObject.Parse(jsonString);

                    if (jsonObject.ContainsKey($"{user.Username}"))
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = $"Hi! {user.Username}",
                            Description = "Dữ liệu của bạn đã được làm mới.",
                            Color = DiscordColor.Green,
                        };

                        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                            .AddEmbed(embed));

                        JObject userObject = (JObject)jsonObject[$"{user.Username}"]!;
                        LoginInfor loginInfo = userObject.ToObject<LoginInfor>()!;
                        var filepath = await DataCrawler.Crawl(loginInfo);
                        var s = await ScheduleProcessor.ProcessFileAsync(filepath);
                        await Schedule.Utilities.saveScheduleAsync($"{user.Username}", s);

                        Console.WriteLine("Đã làm mới dữ liệu");
                    }
                    else
                    {
                        var modal = ModalBuilder.Create("login_modal")
                            .WithTitle("Đăng nhập DKTC")
                            .AddComponents(new TextInputComponent("Mã sinh viên", "studentid", "DTC123"))
                            .AddComponents(new TextInputComponent("Mật khẩu", "password", "..."))
                            .AddComponents(new TextInputComponent("Gì cũng được", "", "Cứ gửi đi không sao đâu😜!", "", false));

                        await ctx.Channel.SendMessageAsync("Bạn cần đăng nhập trước khi tải lại dữ liệu.");
                        await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    }
                }
                catch (Exception ex)
                {
                    await ctx.Channel.SendMessageAsync($"Đã xảy ra lỗi: {ex.Message}");
                }
            });
        }



        [SlashCommand("help", "Xem danh sách các lệnh")]
        public async Task HelpCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Danh Sách Các Lệnh",
                Description = "Dưới đây là danh sách các lệnh mà bạn có thể sử dụng.",
                Color = DiscordColor.Blurple,
            };

            embed.AddField("/menu", "Hiển thị một menu với các tùy chọn và nút bấm.");
            embed.AddField("/accountlink", "Liên kết tài khoản DKTC.");
            embed.AddField("/tomorow", "Xem lịch ngày mai.");
            embed.AddField("/today", "Xem lịch hôm nay.");
            embed.AddField("/reset", "tải lại dữ liệu lịch của bạn.");
            embed.AddField("/test", "Hello, World!");

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .AddEmbed(embed));
        }


         
        [SlashCommand("accountlink", "Liên kết tài khoản dktc")]
        public async Task LoginCommand(InteractionContext ctx )
        {

            DiscordInteractionResponseBuilder modal = ModalBuilder.Create("login_modal")
                                .WithTitle("Dăng nhập DKTC")
                                .AddComponents(new TextInputComponent("Mã sinh viên", "studentid", "DTC123"))
                                .AddComponents(new TextInputComponent("Mật khẩu", "password", "..."))
                                .AddComponents(new TextInputComponent("Gì cũng được", "", "Cứ gửi đi không sao đâu😜!", "", false));
            await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }

        [SlashCommand("tomorow", "Xem lịch ngày mai")]
        public async Task TomorowCommand(InteractionContext ctx)
        {
            var user = ctx.User;

            var scheduleResult = await Schedule.Utilities.GetSchedule(user.Username);

            if (!scheduleResult.Success & scheduleResult.Error == "noInfo")
            {
                DiscordInteractionResponseBuilder modal = ModalBuilder.Create("login_modal")
                .WithTitle("Dăng nhập DKTC")
                .AddComponents(new TextInputComponent("Mã sinh viên", "studentid", "DTC123"))
                .AddComponents(new TextInputComponent("Mật khẩu", "password", "..."))
                .AddComponents(new TextInputComponent("Gì cũng được", "", "Cứ gửi đi không sao đâu😜!", "", false));
                await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
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
                    builder.Author.IconUrl = user.AvatarUrl;
                    builder.Author.Name = $"Hi! {user.Username}";
                    builder.Author.Url = user.AvatarUrl;

                    foreach (var lesson in entry.Lessons)
                    {
                        builder.AddField($"{lesson.SubjectName}", $"{lesson.timeStart} - {lesson.timeEnd} | {lesson.Address} | {lesson.teacher}", true);
                    }
                    var embed = builder.Build();

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
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

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed));
                }
            }
            else
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Hi {user.Username}!",
                    Description = "Không có dữ liệu lịch",
                    Color = DiscordColor.Orange
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .AddEmbed(errorEmbed));
            }
        }


        [SlashCommand("today", "Xem lịch hôm nay")]
        public async Task TodayCommand(InteractionContext ctx)
        {
            var user = ctx.User;

            var scheduleResult = await Schedule.Utilities.GetSchedule(user.Username);

            if (!scheduleResult.Success & scheduleResult.Error == "noInfo")
            {
                DiscordInteractionResponseBuilder modal = ModalBuilder.Create("login_modal")
                .WithTitle("Dăng nhập DKTC")
                .AddComponents(new TextInputComponent("Mã sinh viên", "studentid", "DTC123"))
                .AddComponents(new TextInputComponent("Mật khẩu", "password", "..."))
                .AddComponents(new TextInputComponent("Gì cũng được", "", "Cứ gửi đi không sao đâu😜!", "", false));
                await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
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
                    builder.Author.IconUrl = user.AvatarUrl;
                    builder.Author.Name = $"Hi! {user.Username}";
                    builder.Author.Url = user.AvatarUrl;

                    foreach (var lesson in entry.Lessons)
                    {
                        builder.AddField($"{lesson.SubjectName}", $"{lesson.timeStart} - {lesson.timeEnd} | {lesson.Address} | {lesson.teacher}", true);
                    }
                    var embed = builder.Build();

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
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

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed));
                }
            }
            else
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Hi {user.Username}!",
                    Description = "Không có dữ liệu lịch",
                    Color = DiscordColor.Orange
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .AddEmbed(errorEmbed));
            }
        }
    }
}
