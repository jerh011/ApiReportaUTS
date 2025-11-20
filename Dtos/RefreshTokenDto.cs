using System.ComponentModel.DataAnnotations;

namespace ReportaUTS.Dtos
{
    public class RefreshTokenDto
    {

        [Required(ErrorMessage = "El refreshToken no ha sido proporcionado.")]
        public string RefreshToken { get; set; }
    }
}
