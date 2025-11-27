using Microsoft.AspNetCore.Mvc;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using ReportaUTS.Model;
using ReportaUTS.Repository;

namespace ReportaUTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : Controller
    {
        private readonly IReportes _reportesRepository;
        public ReportesController(IReportes reporteRepository) => _reportesRepository = reporteRepository;
        [HttpPost("RegistrarReporte")]
        public async Task<IActionResult> InsertarReportes([FromBody] InsertarReporteDto dtos)
        {

            string responseData = await _reportesRepository.InsertarReport(dtos);
            return Ok(responseData);
        }

        [HttpGet("ObtenerReportesPorVotos")]
        public async Task<IActionResult> ObtenerReportesPorVotos([FromQuery] string orden)
        {
            var reportes = await _reportesRepository.ObtenerReportesPorVotos(orden);
            return Ok(reportes);
        }


        [HttpGet("ReportePorUsuarioDto")]
        public async Task<IActionResult> ReportePorUsuarioDto([FromQuery] int idUsuario)
        {
            var reportes = await _reportesRepository.ReportePorUsuario(idUsuario);
            return Ok(reportes);
        }

        [HttpGet("ContarVotosPorReporte")]
        public async Task<IActionResult> ContarVotosPorReporte([FromQuery] int idUsuario)
        {
            var reportes = await _reportesRepository.ContarVotosPorReporte(idUsuario);
            return Ok(reportes);
        }
        //[HttpPost]
        //public async Task<IActionResult> RegistrarReporte ([FromBody] RegistrarReporteModel dtos)
        //{

        //    string responseData = await _reportesRepository.InsertarReport(dtos);
        //    return Ok(responseData);
        //}
    }
}
