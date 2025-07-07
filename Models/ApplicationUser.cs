using Microsoft.AspNetCore.Identity;

namespace Kuotasmig.Core.Models
{
    // Puedes añadir propiedades personalizadas a tus usuarios aquí si lo necesitas.
    // Por ejemplo, para mantener la compatibilidad con tu tabla original `USUARIO`,
    // podríamos añadir las propiedades NOMBRE y TIPO.
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string? Nombre { get; set; }

        [PersonalData]
        public string? Tipo { get; set; }
    }
}