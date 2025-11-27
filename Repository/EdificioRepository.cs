using Dapper;
using Npgsql;
using ReportaUTS.Conexion;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using System.Threading.Tasks;

namespace ReportaUTS.Repository
{
    public class EdificioRepository : IEdificio
    {
        private readonly PostgreSQLConnection _connection;

        public EdificioRepository(PostgreSQLConnection connection)
        {
            _connection = connection;
        }

        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);

        public async Task<List<EdificioDto>> GetEdificio()
        {
            using var conn = DbConnection();
            await conn.OpenAsync();

            // Usar QueryAsync para obtener múltiples registros
            var edificios = await conn.QueryAsync<EdificioDto>(
               @"SELECT id_edificio, nombre, descripcion
         FROM ""Edificio"";"
            );

            return edificios.ToList();
        }

    }
}
