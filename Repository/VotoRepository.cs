using Npgsql;
using ReportaUTS.Conexion;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using System.Threading.Tasks;

namespace ReportaUTS.Repository
{
    public class VotoRepository : IVotos
    {
        private readonly PostgreSQLConnection _connection;

        public VotoRepository(PostgreSQLConnection connection)
        {
            _connection = connection;
        }

        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);

        public async Task<string> InsertarVoto(VotoDto insertVoto)
        {
            if (insertVoto == null)
                throw new ArgumentNullException(nameof(insertVoto));

            await using var conn = DbConnection();
            await conn.OpenAsync();

            const string sql = @"SELECT funcion_insertarvoto(
                                    @p_usuario_id,
                                    @p_reporte_id
                                 );";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("p_usuario_id", insertVoto.idUsuario);
            cmd.Parameters.AddWithValue("p_reporte_id", insertVoto.idReporte);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "No se recibió respuesta.";
        }
    }
}
