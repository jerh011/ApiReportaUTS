using Microsoft.AspNetCore.Mvc;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;

namespace ReportaUTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EdificioController:Controller
    {
        private readonly IEdificio _edificioRepository;
        public EdificioController(IEdificio edificioRepository)=> _edificioRepository = edificioRepository;
        [HttpGet]
        public async Task<IActionResult> GetEdificio() {
            List<EdificioDto> responseData=await _edificioRepository.GetEdificio();
            return Ok(responseData);
        }
    }
}
