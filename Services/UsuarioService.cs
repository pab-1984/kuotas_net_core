// Kuotasmig.Core/Services/UsuarioService.cs

using Kuotasmig.Core.Models; // Mantenemos la referencia al modelo por si acaso

namespace Kuotasmig.Core.Services
{
    /// <summary>
    /// Este servicio se mantiene para compatibilidad, pero su lógica de base de datos
    /// ha sido reemplazada por ASP.NET Core Identity. Las operaciones reales de usuario
    /// ahora ocurren en las páginas de /Account (Login, Register, etc.).
    /// </summary>
    public class UsuarioService
    {
        // El constructor ya no necesita ninguna dependencia.
        public UsuarioService()
        {
        }

        /// <summary>
        /// La lógica de búsqueda ahora la maneja SignInManager en Login.cshtml.cs
        /// </summary>
        public Usuario? Buscar(string usuarioParam, string contraseñaParam)
        {
            return null;
        }

        /// <summary>
        /// La lógica para agregar usuarios ahora la maneja UserManager en Register.cshtml.cs
        /// </summary>
        public int Agregar(Usuario nuevoUsuario)
        {
            return 0;
        }

        /// <summary>
        /// La modificación de contraseñas se debe manejar con las páginas de Identity.
        /// </summary>
        public int ModificarContraseña(string usuarioParam, string nuevaContraseñaParam)
        {
            return 0;
        }

        /// <summary>
        /// La eliminación de usuarios se debe manejar desde una página de administración de Identity.
        /// </summary>
        public int Eliminar(string usuarioParam)
        {
            return 0;
        }
    }
}