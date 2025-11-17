using Npgsql;
using ReportaUTS.Conexion;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using System.Threading.Tasks;

namespace ReportaUTS.Repository
{
    public class CategoriaRepository : ICategoria
    {
        PostgreSQLConnection _connection;

        public CategoriaRepository(PostgreSQLConnection connection)
        {
            _connection = connection;
        }

        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);

        public async Task<CategoriaDto> GetCategoria()
        {
            using var conn = DbConnection();
            await conn.OpenAsync();

            // Llamamos exactamente a la función que proporcionaste
            using var cmd = new NpgsqlCommand("SELECT * FROM funcion_cantegorias();", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            CategoriaDto categoria = null;

            if (await reader.ReadAsync())
            {
                categoria = new CategoriaDto
                {
                    IdCategoria = reader.GetInt32(reader.GetOrdinal("idcategorias")), // nombre de columna según tu función
                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                    Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion"))
                                  ? null
                                  : reader.GetString(reader.GetOrdinal("descripcion"))
                };
            }

            return categoria;
        }
    }
}
