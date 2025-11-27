using ReportaUTS.Dtos;

namespace ReportaUTS.Interfaces
{
    public interface IEdificio
    {
        public Task<List<EdificioDto>> GetEdificio();

    }
}
