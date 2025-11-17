using ReportaUTS.Dtos;

namespace ReportaUTS.Interfaces
{
    public interface IVotos
    {
        public Task<string> InsertarVoto(VotoDto insertVoto);
    }
}