namespace ReportaUTS.Dtos
{
    public class UserDto
    {
        public long IdLogin { get; set; }
        public string? Token { get; set; }
        public int IdUsuario { get; set; }
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string NoCel { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}