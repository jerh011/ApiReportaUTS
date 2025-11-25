namespace ReportaUTS.Model
{
    public class RegistrarReporteModel
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Privacidad { get; set; }
        public int EdificioId { get; set; }
        public int CategoriaId { get; set; }
        public int UsuarioId { get; set; }
        public int EstadoId { get; set; }
        public string Imagen { get; set; }
    }
}
