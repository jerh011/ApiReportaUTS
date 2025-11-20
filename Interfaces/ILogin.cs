using ReportaUTS.Dtos;

namespace ReportaUTS.Interfaces
{
    public interface ILogin
    {
        public Task<UserDto> GetLogin(string user, string contraseña);
    }
}
