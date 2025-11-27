using System.Text.Json.Serialization;

namespace ReportaUTS.Model
{
    public class Role
    {
        public int IdRol { get; set; }

        public string NombreRol { get; set; }

     
        public string Descripcion { get; set; }

 
        public string Llave { get; set; }

        //[JsonIgnore]
        //public int Nivel { get; set; }

        ////[JsonIgnore]
        //public bool RolActivo { get; set; }
    }
}
