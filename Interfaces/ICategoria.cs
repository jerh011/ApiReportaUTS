using ReportaUTS.Dtos;

namespace ReportaUTS.Interfaces
{
    public interface ICategoria
    {
        public Task<CategoriaDto> GetCategoria();
    }
}
