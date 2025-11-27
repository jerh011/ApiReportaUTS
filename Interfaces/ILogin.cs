using ReportaUTS.Dtos;
using ReportaUTS.Model;

namespace ReportaUTS.Interfaces
{
    public interface ILogin
    {
        public Task<LoginResponse> Login(LoginDto loginDto);
        public Task<bool> LogOut(int userId);
        public Task<RefreshTokenResponse> RefreshToken(RefreshTokenDto refreshTokenDto);
        public Task<string> RegistroTemporal(RegisterTemp registerTemp);


    }
}
