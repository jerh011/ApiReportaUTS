using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using ReportaUTS.Repository;

namespace ReportaUTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly ILogin _loginRepository;

        public LoginController(ILogin LoginRepository) => _loginRepository = LoginRepository;
       
        [HttpPost]
     
        public async Task <IActionResult> GetLogin([FromBody] LoginDto logindto)
        {
            var holaa =await _loginRepository.GetLogin(logindto.user, logindto.contraseña);
            return Ok(holaa);
        }

    }
}
