// Kuotasmig.Core/Services/CalculadoraService.cs
using Kuotasmig.Core.Data;
using Kuotasmig.Core.Models;
// using Microsoft.Data.SqlClient; // Ya no es necesario para este archivo
using System;
using MySql.Data.MySqlClient; // Asegúrate que este using esté presente
using System.Collections.Generic; // Para List<> si devuelves colecciones

namespace Kuotasmig.Core.Services
{
    public class CalculadoraService
    {
        private readonly BDService _bdService;

        public CalculadoraService(BDService bdService)
        {
            _bdService = bdService ?? throw new ArgumentNullException(nameof(bdService));
        }

        // Asumiendo que devuelve el último registro por ID o uno específico
        public CalculadoraEntry? BuscarUltimo() // Cambiado el nombre para ser más específico
        {
            CalculadoraEntry? entry = null; // Hacer anulable
            // En MySQL, para obtener el último registro, usualmente se ordena por ID descendente y se toma el primero.
            // La sintaxis TOP 1 es de SQL Server. En MySQL se usa LIMIT 1.
            string query = "SELECT `id`, `texto` FROM `calculadora` ORDER BY `id` DESC LIMIT 1";

            using (MySqlConnection conexion = _bdService.ObtenerConexion()) // MySqlConnection
            {
                try
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion)) // MySqlCommand
                    {
                        using (MySqlDataReader reader = comando.ExecuteReader()) // MySqlDataReader
                        {
                            if (reader.Read())
                            {
                                entry = new CalculadoraEntry
                                {
                                    ID = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                                    TEXTO = reader.IsDBNull(reader.GetOrdinal("texto")) ? string.Empty : reader.GetString("texto")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en CalculadoraService.BuscarUltimo: {ex.Message}");
                    // Considera relanzar o manejar de otra forma
                }
            }
            return entry;
        }

        // Método para buscar por ID si lo necesitas
        public CalculadoraEntry? BuscarPorId(int id)
        {
            CalculadoraEntry? entry = null;
            string query = "SELECT `id`, `texto` FROM `calculadora` WHERE `id` = @id";

            using (MySqlConnection conexion = _bdService.ObtenerConexion())
            {
                 try
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                entry = new CalculadoraEntry
                                {
                                    ID = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                                    TEXTO = reader.IsDBNull(reader.GetOrdinal("texto")) ? string.Empty : reader.GetString("texto")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en CalculadoraService.BuscarPorId: {ex.Message}");
                }
            }
            return entry;
        }


        public int Agregar(CalculadoraEntry nuevaEntrada)
        {
            int ret = 0;
            // En MySQL, si la columna 'id' es AUTO_INCREMENT, no necesitas insertarla explícitamente.
            // Si quieres obtener el ID generado, hay formas de hacerlo después del INSERT.
            string query = "INSERT INTO `calculadora` (`texto`) VALUES (@texto)";

            using (MySqlConnection conn = _bdService.ObtenerConexion()) // MySqlConnection
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conn)) // MySqlCommand
                    {
                        comando.Parameters.AddWithValue("@texto", nuevaEntrada.TEXTO);
                        ret = comando.ExecuteNonQuery();
                        // Para obtener el último ID insertado en MySQL:
                        // if (ret > 0) {
                        //     nuevaEntrada.ID = (int)comando.LastInsertedId;
                        // }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en CalculadoraService.Agregar: {ex.Message}");
                }
            }
            return ret;
        }

        // Modificar un registro específico por su ID
        public int Modificar(CalculadoraEntry entradaAModificar)
        {
            int ret = 0;
            string query = "UPDATE `calculadora` SET `texto` = @texto WHERE `id` = @id";

            using (MySqlConnection conn = _bdService.ObtenerConexion()) // MySqlConnection
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conn)) // MySqlCommand
                    {
                        comando.Parameters.AddWithValue("@texto", entradaAModificar.TEXTO);
                        comando.Parameters.AddWithValue("@id", entradaAModificar.ID);
                        ret = comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en CalculadoraService.Modificar: {ex.Message}");
                }
            }
            return ret;
        }

        // El método ModificarTodo que actualiza TODAS las filas es peligroso,
        // lo mantendré si es una funcionalidad que realmente necesitas, pero con una advertencia.
        public int ModificarTodo(string nuevoTexto)
        {
            Console.WriteLine("ADVERTENCIA: Se está ejecutando ModificarTodo en CalculadoraService, esto actualizará TODAS las filas.");
            int ret = 0;
            string query = "UPDATE `calculadora` SET `texto` = @texto"; // Sin WHERE, afecta todas las filas

            using (MySqlConnection conn = _bdService.ObtenerConexion())
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conn))
                    {
                        comando.Parameters.AddWithValue("@texto", nuevoTexto);
                        ret = comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en CalculadoraService.ModificarTodo: {ex.Message}");
                }
            }
            return ret;
        }


        // Eliminar por ID es más seguro que por texto si el texto no es único.
        public int EliminarPorId(int id)
        {
            int ret = 0;
            string query = "DELETE FROM `calculadora` WHERE `id` = @id";

            using (MySqlConnection conn = _bdService.ObtenerConexion()) // MySqlConnection
            {
                 try
                {
                    conn.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conn)) // MySqlCommand
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        ret = comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en CalculadoraService.EliminarPorId: {ex.Message}");
                }
            }
            return ret;
        }

        // Mantengo EliminarPorTexto si es una funcionalidad específica que necesitas,
        // pero ten cuidado si la columna 'texto' no tiene un índice UNIQUE.
        public int EliminarPorTexto(string textoParam)
        {
            int ret = 0;
            string query = "DELETE FROM `calculadora` WHERE `texto` = @texto";

            using (MySqlConnection conn = _bdService.ObtenerConexion())
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conn))
                    {
                        comando.Parameters.AddWithValue("@texto", textoParam);
                        ret = comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                     Console.WriteLine($"Error en CalculadoraService.EliminarPorTexto: {ex.Message}");
                }
            }
            return ret;
        }
    }
}