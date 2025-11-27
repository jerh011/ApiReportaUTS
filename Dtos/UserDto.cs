using ReportaUTS.Model;
using System.Text.Json.Serialization;

namespace ReportaUTS.Dtos
{
    public class UserDto
    {
        //public long IdLogin { get; set; }
        public string Username { get; set; }
        public string Contrasena { get; set; }
        public int IdUsuario { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        //public string Correo { get; set; } 
        public string NoCel { get; set; }
        public string CreatedAt { get; set; }
        public Role rol { get; set; }


      
    }
}
