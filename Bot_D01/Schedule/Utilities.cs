
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AngleSharp;
using AngleSharp.Html.Parser;


namespace Bot_D01.Schedule
{
    public class Utilities
    {
        public static async Task<string> getSessionStringAsync(string action)
        {
            var url = "http://220.231.119.171/kcntt/" + action; // Thay thế bằng URL thực tế của bạn

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false // Tắt tự động chuyển hướng để không bị thay đổi phản hồi
            };

            using (var client = new HttpClient(handler))
            {
                try
                {
                    var response = await client.GetAsync(url);

                    return string.Join(", ", response.Headers.GetValues("Location"));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Lỗi: {e.Message}");
                    return "";
                }
            }
        }

        public static string computeMd5Hash(string input)
        {
            // Tạo đối tượng MD5
            using (MD5 md5 = MD5.Create())
            {
                // Chuyển đổi chuỗi đầu vào thành mảng byte
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                // Tính toán mã băm MD5
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Chuyển đổi mảng byte thành chuỗi hex
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static async Task saveTokenAsync(string userId, LoginInfor userTokenInfo)
        {
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AppData/Users");
            string filePath = Path.Combine(directoryPath, "listUserInfor.json");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Đọc file JSON nếu đã tồn tại
            Dictionary<string, LoginInfor> tokenDict = new();
            if (File.Exists(filePath))
            {
                var jsonString = await File.ReadAllTextAsync(filePath);
                tokenDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, LoginInfor>>(jsonString) ?? new Dictionary<string, LoginInfor>();
            }

            // Cập nhật dữ liệu với token mới
            tokenDict[userId] = userTokenInfo;

            // Ghi lại dữ liệu vào file JSON
            var updatedJsonString = System.Text.Json.JsonSerializer.Serialize(tokenDict, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(filePath, updatedJsonString);
        }

        public static async Task saveScheduleAsync(string userId, ScheduleResult schedule)
        {

            string jsonString = JsonConvert.SerializeObject(schedule, Formatting.Indented);

            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @$"AppData/Users");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string jsonFilePath = Path.Combine(folderPath, $"{userId}.json");

            File.WriteAllText(jsonFilePath, jsonString);
        }
        public static string getPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @$"AppData/Users");
        }

        public static async Task<ScheduleResult> GetSchedule(string user)
        {
            string jsonFilePath = Path.Combine(getPath(), $"{user}.json");
            
            if(!File.Exists(jsonFilePath))
            {

                string path = Path.Combine(getPath(), "listUserInfor.json");
                string jsonString = File.ReadAllText(path);

                JObject jsonObject = JObject.Parse(jsonString);

                if (jsonObject.ContainsKey($"{user}"))
                {
                    JObject userObject = (JObject)jsonObject[$"{user}"]!;

                    LoginInfor loginInfo = userObject.ToObject<LoginInfor>()!;


                    var filepath = await DataCrawler.Crawl(loginInfo);

                    var s = await ScheduleProcessor.ProcessFileAsync(filepath);

                    await saveScheduleAsync($"{user}", s);

                    await RemoveExcelFile(Path.Combine(getPath(), $"{s.StudentId}.xls"));

                    ScheduleResult? scheduleResult = JsonConvert.DeserializeObject<ScheduleResult>(jsonString);
                    return scheduleResult!;
                }
                else
                {

                    return new ScheduleResult
                    {
                        Task = "readFile",
                        Success = false,
                        Error = "noInfo"
                    };
                }
            }
            else
            {
                string jsonString = File.ReadAllText(jsonFilePath);
                ScheduleResult? scheduleResult = JsonConvert.DeserializeObject<ScheduleResult>(jsonString);
                return scheduleResult!;
            }

        }
        public static async Task RemoveExcelFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                try
                {
                    await Task.Run(() => File.Delete(filepath));
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error deleting file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("File does not exist.");
            }
        }

        public static async Task<ViewState> GetViewState(LoginInfor info)
        {
            using var client = new HttpClient();

            var sessionString = await getSessionStringAsync("Reports/Form/StudentTimeTable.aspx");

            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Language", "en,vi;q=0.9,vi-VN;q=0.8,fr-FR;q=0.7,fr;q=0.6,en-US;q=0.5");
            client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Referer", $"http://220.231.119.171{sessionString}");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");

            var cookieContainer = new System.Net.CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer
            };
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"http://220.231.119.171{sessionString}");
            requestMessage.Headers.Add("Cookie", $"SignIn={info.token}");

            using var response = await client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {

                var content = await response.Content.ReadAsStringAsync();

                var config = Configuration.Default;
                var context = BrowsingContext.New(config);
                var parser = context.GetService<IHtmlParser>();
                var document = await parser.ParseDocumentAsync(content);

                var values = new Dictionary<string, string>();

                var ids = new[]
                {
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__LASTFOCUS",
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION",
                    "hidDiscountFactor",
                    "hidReduceTuitionType",
                    "hidXetHeSoHocPhiTheoDoiTuong",
                    "hidTuitionFactorMode",
                    "hidLoaiUuTienHeSoHocPhi",
                    "hidSecondFieldId",
                    "hidSecondAyId",
                    "hidSecondFacultyId",
                    "hidSecondAdminClassId",
                    "hidSecondFieldLevel",
                    "hidXetHeSoHocPhiDoiTuongTheoNganh",
                    "hidUnitPriceDetail",
                    "hidFacultyId",
                    "hidFieldLevel",
                    "hidPrintLocationByCode",
                    "hidUnitPriceKP",
                    "btnView",
                    "hidStudentId",
                    "hidAcademicYearId",
                    "hidFieldId",
                    "hidSemester",
                    "hidTerm",
                    "hidShowTeacher",
                    "hidUnitPrice",
                    "hidTuitionCalculating",
                    "hidTrainingSystemId",
                    "hidAdminClassId",
                    "hidTuitionStudentType",
                    "hidStudentTypeId",
                    "hidUntuitionStudentTypeId",
                    "hidUniversityCode",
                    "txtTuNgay",
                    "txtDenNgay",
                    "hidTuanBatDauHK2",
                    "hidSoTietSang",
                    "hidSoTietChieu",
                    "hidSoTietToi"
                };

                foreach (var id in ids)
                {
                    var element = document.GetElementById(id);
                    if (element != null)
                    {
                        var value = element.GetAttribute("value") ?? "";
                        values[id] = value;
                    }
                    else
                    {
                        values[id] = ""; // Nếu không tìm thấy id, để giá trị trống
                    }
                }

                var encodedContent = new FormUrlEncodedContent(values);
                var encodedString = await encodedContent.ReadAsStringAsync();
                var result = new ViewState();
                result.viewState = encodedString;
                result.location = sessionString;

                return result;
            }
            return null;

        }
        public class ViewState
        {
            public string viewState {  get; set; } = string.Empty;
            public string location { get; set; } = string.Empty;
        }
    }
}
