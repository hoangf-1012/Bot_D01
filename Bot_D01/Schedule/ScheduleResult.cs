
namespace Bot_D01.Schedule
{
    public class ScheduleResult
    {
        public string Task { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public List<ScheduleEntry>? Schedule { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
