
namespace Bot_D01.Schedule
{
    public class LoginInfor
    {
        public LoginInfor(string userName, string password, string token)
        {
            this.userName = userName;
            this.password = password;
            this.token = token;
        }
        public string userName { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
    }
}
