using System.ComponentModel.DataAnnotations;

namespace PruebaMyper.Models
{
    public class Trabajador
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Debe seleccionar un tipo de documento.")]
        public string TipoDocumento { get; set; }

        [Required(ErrorMessage = "Debe ingresar el número de documento.")]
        public string NumeroDocumento { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un sexo.")]
        public string Sexo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un departamento.")]
        public int? IdDepartamento { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una provincia.")]
        public int? IdProvincia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un distrito.")]
        public int? IdDistrito { get; set; }
        public string? NombreDepartamento { get; set; }
        public string? NombreProvincia { get; set; }
        public string? NombreDistrito { get; set; }

    }
}
