using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class AutorCreacionDTO
    {
        [Required(ErrorMessage = "El Campo {0} es Requerido")]
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe tener mas {1} caracteres.")]
        [MinLength(2, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres.")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
    }
}
