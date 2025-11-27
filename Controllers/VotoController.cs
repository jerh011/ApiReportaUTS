using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;

namespace ReportaUTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotoController : Controller
    {
        private readonly IVotos _votoRepository;
        public VotoController(IVotos votoRepository) => _votoRepository = votoRepository;
        [HttpPost("")]
        public async Task<IActionResult> InsertarReportes([FromBody] VotoDto dtos)
        {

            string responseData = await _votoRepository.InsertarVoto(dtos);
            return Ok(responseData);
        }
    }
}
