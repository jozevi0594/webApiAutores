using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage ="El Campo {0} es Requerido")]
        [StringLength( maximumLength: 120, ErrorMessage = "El campo {0} no debe tener mas {1} caracteres.")]
        [MinLength(2,ErrorMessage ="El campo {0} debe tener al menos {1} caracteres.")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        public List<AutorLibro> AutoresLibros { get; set; }

    }
}
