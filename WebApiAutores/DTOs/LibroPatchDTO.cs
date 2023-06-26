using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class LibroPatchDTO
    {
        [Required(ErrorMessage = "El Campo {0} es Requerido")]
        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 250, ErrorMessage = "El campo {0} no debe tener mas {1} caracteres.")]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
