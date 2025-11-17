using Microsoft.AspNetCore.Mvc;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using ReportaUTS.Repository;

namespace ReportaUTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : Controller
    {
        private readonly ICategoria _categoriaRepository;
        public CategoriaController(ICategoria categoriaRepository) => _categoriaRepository = categoriaRepository;
        [HttpGet]
        public async Task<IActionResult> GetCategoria()
        {
            CategoriaDto responseData = await _categoriaRepository.GetCategoria();
            return Ok(responseData);
        }
    }
}
