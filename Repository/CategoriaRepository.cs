using Dapper;
using Npgsql;
using ReportaUTS.Conexion;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using System.Threading.Tasks;

namespace ReportaUTS.Repository
{
    public class CategoriaRepository : ICategoria
    {
        private readonly PostgreSQLConnection _connection;

        public CategoriaRepository(PostgreSQLConnection connection)
        {
            _connection = connection;
        }

        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);
        public async Task<List<CategoriaDto>> GetCategoria()
        {
            await using var conn = DbConnection();
            await conn.OpenAsync();

            // Ejecuta la función y mapea los resultados a CategoriaDto
            var categorias = await conn.QueryAsync<CategoriaDto>(
                "SELECT * FROM funcion_cantegorias();"
            );

            return categorias.AsList(); // Dapper tiene AsList() para convertir a List<T>
        }


    }
}
