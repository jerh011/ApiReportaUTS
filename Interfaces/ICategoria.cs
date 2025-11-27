using ReportaUTS.Dtos;

namespace ReportaUTS.Interfaces
{
    public interface ICategoria
    {
        public Task<List<CategoriaDto>> GetCategoria();
    }
}
