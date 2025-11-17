using Npgsql;
using ReportaUTS.Conexion;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using System.Threading.Tasks;
namespace ReportaUTS.Repository
{
    public class ReportesRepository: IReportes
    {
        PostgreSQLConnection _connection;
        public ReportesRepository(PostgreSQLConnection connection)=>_connection = connection;
        
        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);
        public async Task<string> InsertarReport(InsertarReporteDto dto)
        {
            await using var conn = DbConnection();
            await conn.OpenAsync();

            const string sql = @"SELECT funcion_insertarreporte(
                                    @p_titulo,
                                    @p_descripcion,
                                    @p_privacidad,
                                    @p_edificio_id,
                                    @p_categoria_id,
                                    @p_usuario_id,
                                    @p_estado_id,
                                    @p_imagen
                                 );";

            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("p_titulo", dto.Titulo);
            cmd.Parameters.AddWithValue("p_descripcion", (object?)dto.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_privacidad", dto.Privacidad);
            cmd.Parameters.AddWithValue("p_edificio_id", dto.EdificioId);
            cmd.Parameters.AddWithValue("p_categoria_id", dto.CategoriaId);
            cmd.Parameters.AddWithValue("p_usuario_id", dto.UsuarioId);
            cmd.Parameters.AddWithValue("p_estado_id", dto.EstadoId);
            cmd.Parameters.AddWithValue("p_imagen", (object?)dto.Imagen ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString() ?? "No se recibió respuesta.";
        }
        public async Task<List<ReporteVotosDto>> ObtenerReportesPorVotos(string orden)
        {
            var reportes = new List<ReporteVotosDto>();

            await using var conn = DbConnection();
            await conn.OpenAsync();

            const string sql = "SELECT * FROM funcion_reportesporvotos(@p_orden);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("p_orden", orden ?? "DESC");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reportes.Add(new ReporteVotosDto
                {
                    IdReporte = reader.GetInt32(0),
                    Titulo = reader.GetString(1),
                    TotalVotos = reader.GetInt32(2)
                });
            }

            return reportes;
        }

        public async Task<List<ReportePorUsuarioDto>> ReportePorUsuario(int usuarioId)
        {
            var reportes = new List<ReportePorUsuarioDto>();

            await using var conn = DbConnection();
            await conn.OpenAsync();

            const string sql = "SELECT * FROM funcion_reporteporusuario(@p_usuario_id);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("p_usuario_id", usuarioId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reportes.Add(new ReportePorUsuarioDto
                {
                    Titulo = reader.GetString(reader.GetOrdinal("titulo")),
                    Categoria = reader.GetString(reader.GetOrdinal("categoria")),
                    Estado = reader.GetString(reader.GetOrdinal("estado")),
                    FechaFormateada = reader.GetString(reader.GetOrdinal("fecha_formateada")),
                    EdificioDescripcion = reader.IsDBNull(reader.GetOrdinal("edificio_descripcion"))
                                           ? null
                                           : reader.GetString(reader.GetOrdinal("edificio_descripcion"))
                });
            }

            return reportes;
        }
        public async Task<int> ContarVotosPorReporte(int reporteId)
        {
            await using var conn = DbConnection();
            await conn.OpenAsync();

            const string sql = @"SELECT funcion_contarvotosporreporte(@p_reporte_id);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("p_reporte_id", reporteId);

            var result = await cmd.ExecuteScalarAsync();

            return result != null ? (int)result : 0;
        }
    }
}