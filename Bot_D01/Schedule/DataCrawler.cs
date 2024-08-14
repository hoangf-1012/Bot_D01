
using System.Diagnostics;
using System.Text;

namespace Bot_D01.Schedule
{
    public class DataCrawler
    {
        public static async Task<string> Crawl(LoginInfor infor)
        {
            var viewState = await Utilities.GetViewState(infor);
            var url = "http://220.231.119.171" + viewState.location;


            var other = "&PageHeader1%24drpNgonNgu=010527EFBEB84BCA8919321CFD5C3A34&" +
                "PageHeader1%24hidisNotify=0&" +
                "PageHeader1%24hidValueNotify=0&" +
                "drpSemester=0d6981189e104dff8c950cc3e21991c7&" +
                "drpTerm=1&" +
                "drpType=K&";
            // Tạo đối tượng HttpContent
            var content = new StringContent(
                viewState.viewState + other,
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            // Tạo một HttpClient
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                client.DefaultRequestHeaders.Add("Accept-Language", "en,vi;q=0.9,vi-VN;q=0.8,fr-FR;q=0.7,fr;q=0.6,en-US;q=0.5");
                client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("Cookie", $"SignIn={infor.token}");
                client.DefaultRequestHeaders.Add("Origin", "http://220.231.119.171");
                client.DefaultRequestHeaders.Add("Referer", $"http://220.231.119.171{viewState.location}");
                client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");

                try
                {
                    var response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {

                        var responseBytes = await response.Content.ReadAsByteArrayAsync();

                        var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @$"AppData\Users");
                        var filePath = Path.Combine(folderPath, $"{infor.userName}.xls");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        try
                        {
                            await File.WriteAllBytesAsync(filePath, responseBytes);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error writing file: {ex.Message}");
                            return "";
                        }
                        return filePath;
                    }
                    else
                    {

                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                        Console.WriteLine("loi khi gui yeu cau lay du lieu den dktc");
                        Console.WriteLine(response.StatusCode);
                        return string.Empty;
                    }
  
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Lỗi khi gửi yêu cầu:");
                    Console.WriteLine(e.Message);
                    return string.Empty;
                }
            }
        }
    }
}
