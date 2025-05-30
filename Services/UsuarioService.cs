// Kuotasmig.Core/Services/UsuarioService.cs
using Kuotasmig.Core.Data;
using Kuotasmig.Core.Models;
using Microsoft.Data.SqlClient;
using System;

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
            Usuario u = null;
            // ADVERTENCIA: Esta comparación de contraseña es insegura.
            // Se debe comparar el hash de la contraseña proporcionada con el hash almacenado.
            // Esto es solo para ilustrar la migración de la lógica de BD.

            using (SqlConnection conexion = _bdService.ObtenerConexion())
            {
                conexion.Open();
                string query = "SELECT USUARIO, CONTRASEÑA, TIPO, NOMBRE FROM USUARIO WHERE USUARIO = @usuario AND CONTRASEÑA = @contraseña";
                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@usuario", usuarioParam);
                    comando.Parameters.AddWithValue("@contraseña", contraseñaParam); // DEBE SER HASH

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            u = new Usuario // Usando la clase modelo
                            {
                                USUARIO = reader.GetString(0),
                                CONTRASEÑA = reader.GetString(1), // No almacenar/devolver contraseña en texto plano
                                TIPO = reader.GetString(2),
                                NOMBRE = reader.GetString(3)
                                // Asignar ID si existe en la tabla y en el modelo
                            };
                        }
                    }
                }
            }
            return u;
        }

        public int Agregar(Usuario nuevoUsuario)
        {
            int ret = 0;
            using (SqlConnection conn = _bdService.ObtenerConexion())
            {
                conn.Open();
                string query = "INSERT INTO USUARIO (USUARIO, CONTRASEÑA, NOMBRE, TIPO) VALUES (@usuario, @contraseña, @nombre, @tipo)";
                using (SqlCommand comando = new SqlCommand(query, conn))
                {
                    comando.Parameters.AddWithValue("@usuario", nuevoUsuario.USUARIO);
                    comando.Parameters.AddWithValue("@contraseña", nuevoUsuario.CONTRASEÑA); // ¡HASHEAR ANTES DE GUARDAR!
                    comando.Parameters.AddWithValue("@nombre", nuevoUsuario.NOMBRE);
                    comando.Parameters.AddWithValue("@tipo", nuevoUsuario.TIPO);
                    ret = comando.ExecuteNonQuery();
                }
            }
            return ret;
        }

        public int ModificarContraseña(string usuarioParam, string nuevaContraseñaParam)
        {
            int ret = 0;
            using (SqlConnection conn = _bdService.ObtenerConexion())
            {
                conn.Open();
                string query = "UPDATE USUARIO SET CONTRASEÑA = @nuevaContraseña WHERE USUARIO = @usuario";
                using (SqlCommand comando = new SqlCommand(query, conn))
                {
                    comando.Parameters.AddWithValue("@nuevaContraseña", nuevaContraseñaParam); // ¡HASHEAR!
                    comando.Parameters.AddWithValue("@usuario", usuarioParam);
                    ret = comando.ExecuteNonQuery();
                }
            }
            return ret;
        }

        public int Eliminar(string usuarioParam)
        {
            int ret = 0;
            using (SqlConnection conn = _bdService.ObtenerConexion())
            {
                conn.Open();
                string query = "DELETE FROM USUARIO WHERE USUARIO = @usuario";
                using (SqlCommand comando = new SqlCommand(query, conn))
                {
                    comando.Parameters.AddWithValue("@usuario", usuarioParam);
                    ret = comando.ExecuteNonQuery();
                }
            }
            return ret;
        }
    }
}