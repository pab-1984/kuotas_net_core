using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuotasmig.Core.Models
{
    [Table("calculadora")] // Le dice a EF Core que esta clase corresponde a la tabla 'calculadora'
    public class CalculadoraEntry
    {
        [Key] // Marca la propiedad 'ID' como la clave primaria
        [Column("id")]
        public int ID { get; set; }

        [Column("texto")]
        public string? TEXTO { get; set; } // Hacemos la propiedad anulable con '?'
    }
}