// Kuotasmig.Core/Services/UsuarioService.cs
using Kuotasmig.Core.Data;
using Kuotasmig.Core.Models;
// using Microsoft.Data.SqlClient; // Ya no es necesario para este archivo
using System;
using MySql.Data.MySqlClient; // Asegúrate que este using esté presente

namespace Kuotasmig.Core.Services
{
    public class UsuarioService
    {
        private readonly BDService _bdService;

        public UsuarioService(BDService bdService)
        {
            _bdService = bdService ?? throw new ArgumentNullException(nameof(bdService));
        }

        public Usuario? Buscar(string usuarioParam, string contraseñaParam)
        {
            Usuario? u = null; // Hacer anulable para el resultado
            // La query debería ser compatible con MySQL, revisa los nombres de columna si son sensibles a mayúsculas/minúsculas en tu MySQL
            string query = "SELECT ID, USUARIO, CONTRASEÑA, TIPO, NOMBRE FROM `USUARIO` WHERE `USUARIO` = @usuario AND `CONTRASEÑA` = @contraseñaInput";

            using (MySqlConnection conexion = _bdService.ObtenerConexion())
            {
                try
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioParam);
                        comando.Parameters.AddWithValue("@contraseñaInput", contraseñaParam); // ¡NO USAR EN PRODUCCIÓN ASÍ! Contraseña en texto plano.

                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                u = new Usuario
                                {
                                    // Usar GetOrdinal para robustez o asegurarse que los índices son correctos
                                    ID = reader.IsDBNull(reader.GetOrdinal("ID")) ? 0 : reader.GetInt32("ID"),
                                    USUARIO = reader.IsDBNull(reader.GetOrdinal("USUARIO")) ? string.Empty : reader.GetString("USUARIO"),
                                    CONTRASEÑA = "", // No cargar la contraseña real
                                    TIPO = reader.IsDBNull(reader.GetOrdinal("TIPO")) ? string.Empty : reader.GetString("TIPO"),
                                    NOMBRE = reader.IsDBNull(reader.GetOrdinal("NOMBRE")) ? string.Empty : reader.GetString("NOMBRE")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en UsuarioService.Buscar: {ex.Message}");
                    // Considera relanzar o manejar de otra forma
                }
            }
            return u;
        }

        public int Agregar(Usuario nuevoUsuario)
        {
            int ret = 0;
            string query = "INSERT INTO `USUARIO` (`USUARIO`, `CONTRASEÑA`, `NOMBRE`, `TIPO`) VALUES (@usuario, @contraseña, @nombre, @tipo)";

            using (MySqlConnection conn = _bdService.ObtenerConexion()) // <--- CAMBIO AQUÍ
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conn)) // <--- CAMBIO AQUÍ
                    {
                        comando.Parameters.AddWithValue("@usuario", nuevoUsuario.USUARIO);
                        comando.Parameters.AddWithValue("@contraseña", nuevoUsuario.CONTRASEÑA); // ¡DEBES HASHEAR ESTA CONTRASEÑA ANTES DE GUARDARLA!
                        comando.Parameters.AddWithValue("@nombre", nuevoUsuario.NOMBRE);
                        comando.Parameters.AddWithValue("@tipo", nuevoUsuario.TIPO);
                        ret = comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en UsuarioService.Agregar: {ex.Message}");
                    // Considera relanzar o manejar de otra forma
                }
            }
            return ret;
        }

        public int ModificarContraseña(string usuarioParam, string nuevaContraseñaParam)
        {
            int ret = 0;
            string query = "UPDATE `USUARIO` SET `CONTRASEÑA` = @nuevaContraseña WHERE `USUARIO` = @usuario";

            using (MySqlConnection conn = _bdService.ObtenerConexion()) // <--- CAMBIO AQUÍ
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conn)) // <--- CAMBIO AQUÍ
                    {
                        comando.Parameters.AddWithValue("@nuevaContraseña", nuevaContraseñaParam); // ¡DEBES HASHEAR ESTA CONTRASEÑA!
                        comando.Parameters.AddWithValue("@usuario", usuarioParam);
                        ret = comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en UsuarioService.ModificarContraseña: {ex.Message}");
                    // Considera relanzar o manejar de otra forma
                }
            }
            return ret;
        }

        public int Eliminar(string usuarioParam)
        {
            int ret = 0;
            string query = "DELETE FROM `USUARIO` WHERE `USUARIO` = @usuario";

            using (MySqlConnection conn = _bdService.ObtenerConexion()) // <--- CAMBIO AQUÍ
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conn)) // <--- CAMBIO AQUÍ
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioParam);
                        ret = comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en UsuarioService.Eliminar: {ex.Message}");
                    // Considera relanzar o manejar de otra forma
                }
            }
            return ret;
        }
    }
}