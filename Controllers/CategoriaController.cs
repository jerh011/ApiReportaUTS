using Microsoft.AspNetCore.Mvc;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using ReportaUTS.Repository;

namespace ReportaUTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoria _categoriaRepository;

        public CategoriaController(ICategoria authRepository) => _categoriaRepository = authRepository;
        [HttpGet]
        public async Task<IActionResult> GetCategoria()
        {
            CategoriaDto responseData = await _categoriaRepository.GetCategoria();
            return Ok(responseData);
        }
    }
}
