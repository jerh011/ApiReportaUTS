using ReportaUTS.Dtos;

namespace ReportaUTS.Interfaces
{
    public interface IReportes
    {
        public Task<string> InsertarReport(InsertarReporteDto dto);
        public Task<List<ReporteVotosDto>> ObtenerReportesPorVotos(string dto);
        public Task<List<ReportePorUsuarioDto>> ReportePorUsuario(int usuarioId);
        public Task<int> ContarVotosPorReporte(int usuarioId);
        public Task<List<ReportePorUsuarioWhitImagenDto>> ReportePorUsuarioWhitImagen(int usuarioId);
    }
}
