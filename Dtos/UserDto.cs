using ReportaUTS.Model;
using System.Text.Json.Serialization;

namespace ReportaUTS.Dtos
{
    public class UserDto
    {
        //public long IdLogin { get; set; }
        public string Username { get; set; }
        public string Contrasena { get; set; }
        public string? accessToken { get; set; }
        public int IdUsuario { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        //public string Correo { get; set; } 
        public string NoCel { get; set; }
        public DateTime CreatedAt { get; set; }
        public Role rol { get; set; }

        [JsonPropertyName("refreshToken")]
        public string refreshToken { get; set; }

        //[JsonPropertyName("userToken")]
        //public string token { get; set; } // renombrado solo en JSON para evitar colisión
    }
}
