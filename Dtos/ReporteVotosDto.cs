namespace ReportaUTS.Dtos
{
    public class ReporteVotosDto
    {
        public int IdReporte { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int TotalVotos { get; set; }
    }
}
