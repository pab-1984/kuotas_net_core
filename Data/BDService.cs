// Kuotasmig.Core/Data/BDService.cs
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace Kuotasmig.Core.Data
{
    public class BDService
    {
        private readonly string _connectionString;

        public BDService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("P2015ConnectionString");
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("La cadena de conexión 'P2015ConnectionString' no se encontró.");
            }
        }

        // Cambiar tipo de retorno
        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(_connectionString); // <--- CAMBIO AQUÍ
        }
    }
}