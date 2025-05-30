// Kuotasmig.Core/Services/CalculadoraService.cs
using Kuotasmig.Core.Data;
using Kuotasmig.Core.Models;
using Microsoft.Data.SqlClient;
using System;

namespace Kuotasmig.Core.Services
{
    public class CalculadoraService
    {
        private readonly BDService _bdService;

        public CalculadoraService(BDService bdService)
        {
            _bdService = bdService ?? throw new ArgumentNullException(nameof(bdService));
        }

        public CalculadoraEntry? Buscar() // Asumiendo que devuelve el último o único registro
        {
            CalculadoraEntry entry = null;
            using (SqlConnection conexion = _bdService.ObtenerConexion())
            {
                conexion.Open();
                // Ajusta esta consulta según tu lógica (ej. TOP 1 ORDER BY ID DESC si hay ID)
                string query = "SELECT TOP 1 TEXTO FROM CALCULADORA";
                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entry = new CalculadoraEntry
                            {
                                TEXTO = reader.GetString(0)
                                // Asignar ID si existe
                            };
                        }
                    }
                }
            }
            return entry;
        }

        public int Agregar(CalculadoraEntry nuevaEntrada)
        {
            int ret = 0;
            using (SqlConnection conn = _bdService.ObtenerConexion())
            {
                conn.Open();
                string query = "INSERT INTO CALCULADORA (TEXTO) VALUES (@texto)";
                using (SqlCommand comando = new SqlCommand(query, conn))
                {
                    comando.Parameters.AddWithValue("@texto", nuevaEntrada.TEXTO);
                    ret = comando.ExecuteNonQuery();
                }
            }
            return ret;
        }

        // La función original de modificar actualizaba todas las filas.
        // Esto es peligroso. Asumo que quieres modificar un TEXTO específico,
        // o idealmente, un registro por ID.
        // Este ejemplo actualiza TODOS los registros, igual que el original.
        public int ModificarTodo(string nuevoTexto)
        {
            int ret = 0;
            using (SqlConnection conn = _bdService.ObtenerConexion())
            {
                conn.Open();
                string query = "UPDATE CALCULADORA SET TEXTO = @texto";
                using (SqlCommand comando = new SqlCommand(query, conn))
                {
                    comando.Parameters.AddWithValue("@texto", nuevoTexto);
                    ret = comando.ExecuteNonQuery();
                }
            }
            return ret;
        }

        // La función original de eliminar usaba "WHERE USUARIO =". Asumo que es un error
        // y que quisiste eliminar por TEXTO o un ID.
        // Este ejemplo elimina por TEXTO.
        public int EliminarPorTexto(string textoParam)
        {
            int ret = 0;
            using (SqlConnection conn = _bdService.ObtenerConexion())
            {
                conn.Open();
                string query = "DELETE FROM CALCULADORA WHERE TEXTO = @texto";
                using (SqlCommand comando = new SqlCommand(query, conn))
                {
                    comando.Parameters.AddWithValue("@texto", textoParam);
                    ret = comando.ExecuteNonQuery();
                }
            }
            return ret;
        }
    }
}