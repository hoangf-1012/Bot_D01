
using System.Net;
using System.Net.Http.Headers;


namespace Bot_D01.Schedule
{
    public class CookieTaker
    {
        public static async Task<LoginInfor> GetCookie(string studentid, string password, string location)
        {
            var cookieContainer = new CookieContainer();


            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer
            };


            using var httpClient = new HttpClient(handler);

            var url = "http://220.231.119.171" + location;


            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/avif", 0.8));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp", 0.8));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/apng", 0.8));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/signed-exchange", 0.7));
            httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.Zero };
            httpClient.DefaultRequestHeaders.Referrer = new Uri("http://220.231.119.171" + location);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Proxy-Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36 Edg/127.0.0.0");

            var content = new StringContent("__EVENTTARGET=&__EVENTARGUMENT=&__LASTFOCUS=" +
                "&__VIEWSTATE=mu3ZxY89LvcVlhPRKq%2Bgj4bqjoi1oFAGiTHdlEe6MKQnkrhYBN2JJmVoHeaLbNQKLTb1c25oxnsT5TtBuyE%2BZ2ZcZgCC4mOHQ1grv6UJaAQZmAo3%2BMKINkKMfDUjVQnBcxK33zTcwTkZ2O%2Bd0dVJNz%2B3g9UVtOFc9JNL3ZrmQXXFTdvud18Hzutm0QBERBnvBQYeJatDSqO6JzjyE920KDTHoIQBmKJDdgygbglFMhmkqWBdPlDAQ9XDHC%2BTqn89XTh2q3HCQkysf%2FfM%2BTynHv%2FOD00HoCQQGlo1tEhAnXnH97IyDXpn73%2FvnP8siNgV2R9WWBkwgERpGNlyi%2BXBkvQIFtZWXJJrfCadupw9rKLX2dgOikVWgYYjXkLTEd4VKAqS0hGjB%2FkgJ7HXgpU9oUvHZIebv7Ap0lOVWcMmkCMcHpm1w0ap6X%2BuvkCL9FKTQ0dRKogVfVKXik83jtWhtdp5yLtQbeWDUDDw2sNvP3lZz3Xb0uTLZwATA6E%2FAwIisdqvG4NhWSgM%2BNa%2Fj5EAkMo4x8L2jL%2Bl5bqWBRkglCjaEPFJCnCccNXbWWCABhnBj6TraiAig9rcK06mc4ccyhtNZkq9iVqQAkf%2FmnU54VWLwmviSGifl97QnnxSvNDJF6xgkc0ACvx8bz%2BCYWX5maYrsqqV6v%2BpYUCH%2FqvfyhOUzWFMiRY66ato%2BNetUzIxpzIEE4oyENShzrdxJYy%2F6z2X935ecvtuRkip9Vk1xe3skPcdGa2nV6SOX%2BtVdbBLpN80p40BzFXJv3NpAMi4McbPjVF2zjaDacodjsOQf52VdIGlSv8ppCepseyMs5jO9z13AUuTDpH0fi4e9Aj2gTOfMwxzyxZnboNWX9YokKAapYm9U9zfuT0h8o5Hh2WLKhRSyBJUpQbhijbsTvTwHnT7m3w8e18KvXrfkM4fIZRq0T9IJxdcc%2BfvAz5yjaY7RAUwFaHYVDosBh9uQ%2FHJlJbUbJthWwjB6qPrTUUElsjG%2BPGpbeQg3vOkRkHYq5%2FFGCo%2BltDdKBO588KwrTGaFOP8bZvBSxo5CXb%2Bw%2Fllqu03ifJiUXQewzgIrsitlmYBYj8uDGmaUlRPcwT5ysxSmD%2FL8%2Faq%2FJdXOwqFgQuIEBDslK6fP78EKi%2F1dr5HIFRJK5BiYvBJUlRZB%2FV7CYWOi5CE9djZrQveR2iSsuM%2BiA8OY3CV4fmz4Rwubp0wRhITfzaX0rWDWhNaFV3ORFU3E1c5xt2%2F1cml0MrodmL4pWPjjXtb2hZKWOsmPFWJOXnrfdi8mhNSiQnvazQ0DKj2EmQWiRnwwcjaFgsz9RCulLHjwpZ8srIsTCvA3uStIZW9bPsMlSYnchcs6%2F7m9JSvo6w8mXLK%2FC0lh6JB%2FgcDHopWryQPxhegKcp4dDifYvMXkd%2Bb3FCkFwUmkOTin7UTHcjYa3xTSBQC490gs1xsKvm%2FJiSp8cxKO0y7fAI%2FvyGXwLuB2%2FOrbpBz2nMJ8lPCDz6ZdxftfK1Dp%2FYkDpalBGFnbMsLiHQzl%2B%2B%2FhkdF8jawEAeqEpRNyE2RFaQEMWv1Z%2FixVpDhKMvElqhz8jaiooNsooYG77r1Tb5Nalr34N28Cbqz6hW%2BA4hoXtE2TvzzJoNqwjxZ0wD%2B5wuBDj6lzd7e41yFmbw7dTw8qfllsym3TwPRC3HItNxQRajGz7FlDNOYpFJCOmZarkVBVbkLifhb%2Fu6mtfL7H4S1itK%2FwFBxmsxUYgaVMXd9dB%2FtxbiFfz3Njy5ejdiwCoyPv0%2F670Q%3D" +
                "&__VIEWSTATEGENERATOR=92FB0661" +
                "&__EVENTVALIDATION=LissgS7p4FiqAgodQ%2B1e5qpdgVS30Ba5GVfTmuNXomy5uJBOpZbBiJsPb4%2FUyOk0tY2jS%2Fy4%2FDeiMOvIayE0nTK62YAOXGXUGplbHeJvvF7PPGHiUiWOyKXbmubJVQyQRjuXy3YIkjld5sEUN6Xe%2FYP%2Bkybwg2U5ykKcRFj7zv3WNxDCI6IRVdXct5sKrFoaT4Y%2BssY%2FeS1Az7821KmV4pciaFkhkISedfP8a2w5SD%2FDDGElZdFg0KRgmwl72baiw7yu3178qjsMQzlgXGiSqUWcrMvD3Hh5II%2FTnhxmajSUsJ3JVqVQNMTV8MnjTz%2B55sRYaZZY7NPoyXbVopF4%2B%2FdJwGQ8NXMl7MNbHN4gQV3lr%2BOwHiU2UFVW7MdOg4twDbYkhLx1XkhCo04CRltTVw%3D%3D" +
                "&PageHeader1%24drpNgonNgu=010527EFBEB84BCA8919321CFD5C3A34&PageHeader1%24hidisNotify=0&PageHeader1%24hidValueNotify=.&txtUserName=" +
                $"{studentid}&txtPassword={password}&btnSubmit=%C4%90%C4%83ng+nh%E1%BA%ADp&hidUserId=&hidUserFullName=&hidTrainingSystemId=",
                System.Text.Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var response = await httpClient.PostAsync(url, content);

            var cookies = cookieContainer.GetCookies(new Uri(url));

            foreach (Cookie cookie in cookies)
            {
                var infor = new LoginInfor(studentid, password, cookie.Value);
                return infor;
            }
            return new LoginInfor("", "","");
        }
    }
}


