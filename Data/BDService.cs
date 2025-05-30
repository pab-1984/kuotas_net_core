// Kuotasmig.Core/Data/BDService.cs
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace Kuotasmig.Core.Data // Asegúrate que el namespace coincida con tu estructura
{
    public class BDService
    {
        private readonly string _connectionString;

        public BDService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("P2015ConnectionString")!;
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("La cadena de conexión 'P2015ConnectionString' no se encontró o está vacía en la configuración.");
            }
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_connectionString);
        }
    }
}