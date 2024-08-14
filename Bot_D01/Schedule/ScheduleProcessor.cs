using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ExcelDataReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot_D01.Schedule
{
    public class ScheduleProcessor
    {
        private static List<ScheduleEntry> schedule = new List<ScheduleEntry>();

        public static async Task<ScheduleResult> ProcessFileAsync(string filePath)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                    var dataSet = reader.AsDataSet();

                    var table = dataSet.Tables[0];

                    // Lấy dữ liệu từ ô cụ thể
                    var idName = FormatName(table.Rows[5][2].ToString());
                    var studentId = idName["studenId"];
                    var name = idName["Name"];
                    var major = table.Rows[6][2].ToString();
                    var course = table.Rows[7][2].ToString();

                    var columnB = table.AsEnumerable().Select(row => row[1].ToString()).ToList();

                    string pattern = @"Tuần \d+ \(\d{2}/\d{2}/\d{4} đến \d{2}/\d{2}/\d{4}\)";
                    var regex = new Regex(pattern);

                    string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AppData\config.json");

                    string jsonContent = File.ReadAllText(jsonFilePath);

                    var jsonObject = JObject.Parse(jsonContent);

                    for (int i = 0; i < columnB.Count; i++)
                    {
                        var cellValueB = columnB[i];
                        if (regex.IsMatch(cellValueB))
                        {

                            var predate = ParseDate(cellValueB.ToString()).AddDays(-2);

                            bool foundValueInD = false;

                            int j = i + 1;
                            while(j < table.Rows.Count)
                            {
                                var cellValueD = table.Rows[j][3]?.ToString();
                                if (string.IsNullOrWhiteSpace(cellValueD))
                                {
                                    break;
                                }

                                var scheduleEntry = new ScheduleEntry
                                {
                                    Date = predate.AddDays(int.Parse(cellValueD))
                                };

                                scheduleEntry.Lessons = new List<Lesson>();
                                
                                while (j < table.Rows.Count)
                                {
                                    var value = table.Rows[j][3]?.ToString();
                                    
                                    if (value != cellValueD)
                                    {
                                        j--;
                                        break;
                                    }


                                    var lesson = new Lesson();
                                    var timevaliue = table.Rows[j][4]?.ToString();
                                    var timeFormat = FormatTime(timevaliue);

                                    lesson.timeStart = jsonObject["timestart"]?[timeFormat[1].ToString()]?.ToString()!;
                                    lesson.timeEnd = jsonObject["timeend"]?[timeFormat[2].ToString()]?.ToString()!;
                                    lesson.teacher = table.Rows[j][2]?.ToString()!;
                                    lesson.SubjectName = table.Rows[j][1]?.ToString()!;
                                    lesson.Address = table.Rows[j][5].ToString()!;

                                    scheduleEntry.Lessons.Add(lesson);
                                    j++;
                                }

                                schedule.Add(scheduleEntry);
                                foundValueInD = true;
                                j++;
                            }

                            if (!foundValueInD)
                            {
                                Console.WriteLine($"Không tìm thấy giá trị trong cột D từ hàng {i + 1}.");
                            }
                        }
                    }

                    return new ScheduleResult
                    {
                        Task = "readFile",
                        Success = true,
                        StudentId = studentId,
                        Name = name,
                        Course = course,
                        Major = major,
                        Schedule = schedule
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return new ScheduleResult
                {
                    Task = "readFile",
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private static Dictionary<string, string> FormatName(string input)
        {
            string[] parts = input.Split(new string[] { " - " }, StringSplitOptions.None);

            if (parts.Length == 2)
            {
                string part1 = parts[0].Trim();
                string part2 = parts[1].Trim();

                return new Dictionary<string, string>
                {
                    { "studenId", part1 },
                    { "Name", part2 }
                };
            }
            else
            {
                return new Dictionary<string, string>
                {
                    { "studenId", "" },
                    { "Name", "" }
                };
            }
        }

        private static Dictionary<int, int> FormatTime(string input)
        {
            // Biểu thức chính quy để tách các số nguyên
            string pattern = @"(\d+)\s*-->\s*(\d+)";

            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);

            if (match.Success)
            {
                // Lấy hai số nguyên từ các nhóm khớp
                int number1 = int.Parse(match.Groups[1].Value);
                int number2 = int.Parse(match.Groups[2].Value);

                // Tạo và trả về từ điển chứa các số nguyên
                return new Dictionary<int, int>
                {
                    { 1, number1 },
                    { 2, number2 }
                };
            }
            else
            {
                // Trả về null nếu không tìm thấy các số nguyên
                return null;
            }
        }
        private static DateTime ParseDate(string dateStr)
        {
            string pattern = @"\((\d{2}/\d{2}/\d{4})";

            Regex regex = new Regex(pattern);
            Match match = regex.Match(dateStr);

            string startDateStr = match.Groups[1].Value;

            DateTime startDate = DateTime.ParseExact(startDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            return startDate;
        }

        //private static (int Row, string Col) GetAddressCell(string address)
        //{
        //    var row = int.Parse(new string(address.Where(char.IsDigit).ToArray()));
        //    var col = new string(address.Where(char.IsLetter).ToArray());
        //    return (row, col);
        //}

        //private static int FormatDay(int day)
        //{
        //    return day - 1;
        //}

        //private static List<DateTime> FindDatesWithDay(DateTime startDate, DateTime endDate, int dayOfWeek)
        //{
        //    var dates = new List<DateTime>();
        //    for (var date = startDate; date <= endDate; date = date.AddDays(1))
        //    {
        //        if (FormatDay((int)date.DayOfWeek) == dayOfWeek)
        //        {
        //            dates.Add(date);
        //        }
        //    }
        //    return dates;
        //}

        //private static int FindIndexDate(DateTime date)
        //{
        //    return schedule.FindIndex(entry => entry.Date == date.Ticks);
        //}

        //private static void AddToSchedule(DateTime startDate, DateTime endDate, int dayOfWeek, string lesson, string subjectName, string address)
        //{
        //    var dateList = FindDatesWithDay(startDate, endDate, dayOfWeek);
        //    foreach (var date in dateList)
        //    {
        //        var foundIndex = FindIndexDate(date);
        //        if (foundIndex == -1)
        //        {
        //            schedule.Add(new ScheduleEntry
        //            {
        //                Date = date.Ticks,
        //                Lessons = new List<Lesson> { new Lesson { LessonInfo = lesson, SubjectName = subjectName, Address = address } }
        //            });
        //        }
        //        else
        //        {
        //            schedule[foundIndex].Lessons.Add(new Lesson { LessonInfo = lesson, SubjectName = subjectName, Address = address });
        //        }
        //    }
        //}

        //private static List<string> LessonArray(string lesson)
        //{
        //    var lessonArray = new List<string> { "1,2,3", "4,5,6", "7,8,9", "10,11,12", "13,14,15,16" };
        //    return lessonArray.Where(lessonArrayItem => lesson.Contains(lessonArrayItem)).ToList();
        //}

        //private static void GetInfoDetail(long startDateTicks, long endDateTicks, string subjectName, string str)
        //{
        //    var startDate = new DateTime(startDateTicks);
        //    var endDate = new DateTime(endDateTicks);
        //    var details = str.Split('\n');

        //    foreach (var detail in details)
        //    {
        //        if (!string.IsNullOrWhiteSpace(detail))
        //        {
        //            var parts = detail.Split("tại");
        //            var address = parts[1];
        //            var dayAndLesson = parts[0].Split("tiết");
        //            var lesson = dayAndLesson[1].Replace(" ", "");
        //            var day = dayAndLesson[0].Replace(" ", "").Replace("Thứ", "");
        //            var lessonArray = LessonArray(lesson);

        //            foreach (var lessonItem in lessonArray)
        //            {
        //                AddToSchedule(startDate, endDate, int.Parse(day), lessonItem, subjectName, address);
        //            }
        //        }
        //    }
        //}
    }
}
