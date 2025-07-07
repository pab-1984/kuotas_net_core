// Kuotasmig.Core/Models/Usuario.cs
namespace Kuotasmig.Core.Models
{
    public class Usuario
    {
        // Aunque la clase original tenía un ID implícito, es buena práctica definirlo.
        // Si tu tabla no tiene un ID y USUARIO es la clave primaria, ajústalo.
        public int ID { get; set; } // Asumiendo que tienes un ID, si no, quitar o adaptar.
        public string USUARIO { get; set; } = string.Empty;
        public string CONTRASEÑA { get; set; } = string.Empty; // ¡RECORDATORIO: Debe ser un hash!
        public string TIPO { get; set; } = string.Empty;
        public string NOMBRE { get; set; } = string.Empty;
    }
}