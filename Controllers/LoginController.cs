using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using ReportaUTS.Model;
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
            //logindto.user = "jerh";
            //logindto.contraseña = "123";
            var holaa =await _loginRepository.Login(logindto);
            return Ok(holaa);
        }
        [HttpPost("Registro-Temporal")]
        public async Task<IActionResult> RegistroTemporal([FromBody] RegisterTemp registerTemp)
        {
           
            string result = await _loginRepository.RegistroTemporal(registerTemp);
            return Ok(result);
        }

    }
}
