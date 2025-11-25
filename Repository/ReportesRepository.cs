using Dapper;
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

            var parametros = new
            {
                p_titulo = dto.Titulo,
                p_descripcion = dto.Descripcion,
                p_privacidad = dto.Privacidad,
                p_edificio_id = dto.EdificioId,
                p_categoria_id = dto.CategoriaId,
                p_usuario_id = dto.UsuarioId,
                p_estado_id = dto.EstadoId,
                p_imagen = dto.Imagen
            };

            var result = await conn.QuerySingleAsync<string>(sql, parametros);

            return result;
        }

        public async Task<List<ReporteVotosDto>> ObtenerReportesPorVotos(string orden)
        {
            await using var conn = DbConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT 
                    id_reporte AS IdReporte,
                    titulo    AS Titulo,
                    total_votos AS TotalVotos
                FROM funcion_reportesporvotos(@p_orden);";

            var parametros = new
            {
                p_orden = orden ?? "DESC"
            };

            var result = await conn.QueryAsync<ReporteVotosDto>(sql, parametros);

            return result.ToList();
        }



        public async Task<List<ReportePorUsuarioDto>> ReportePorUsuario(int usuarioId)
        {
            await using var conn = DbConnection();
            await conn.OpenAsync();

            const string sql = @"SELECT 
                                 titulo as Titulo,
                                 categoria as Categoria,
                                 estado as Estado,
                                 fecha_formateada as FechaFormateada,
                                 edificio_descripcion as EdificioDescripcion
                                 FROM funcion_reporteporusuario(@p_usuario_id);";

            var parametros = new
            {
                p_usuario_id = usuarioId
            };

            var result = await conn.QueryAsync<ReportePorUsuarioDto>(sql, parametros);

            return result.ToList();
        }

        public async Task<int> ContarVotosPorReporte(int reporteId)
        {
            await using var conn = DbConnection();
            await conn.OpenAsync();

            const string sql = @"SELECT funcion_contarvotosporreporte(@p_reporte_id);";

            var parametros = new
            {
                p_reporte_id = reporteId
            };

            var result = await conn.ExecuteScalarAsync<int?>(sql, parametros);

            return result ?? 0;
        }

    }
}