using ReportaUTS.Dtos;

namespace ReportaUTS.Model
{
    public class LoginResponse
    {
        public UserDto User { get; set; }
        public Role Role { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
