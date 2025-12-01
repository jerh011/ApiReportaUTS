using Dapper;
using Npgsql;
using ReportaUTS.Conexion;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ReportaUTS.Repository
{
    public class ReportesRepository : IReportes
    {
        private readonly PostgreSQLConnection _connection;
        private readonly IWebHostEnvironment _env;
        private const int MaxImageBytes = 6 * 1024 * 1024; // límite antes de procesar (6 MB)
        private const int ResizeMaxWidth = 1024;           // ancho máximo al redimensionar
        private const int JpegQuality = 75;                // calidad JPEG resultante

        public ReportesRepository(PostgreSQLConnection connection, IWebHostEnvironment env)
        {
            _connection = connection;
            _env = env;
        }

        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);

        public async Task<string> InsertarReport(InsertarReporteDto dto)
        {
            await using var conn = DbConnection();
            await conn.OpenAsync();

            // Si hay imagen, guardarla y devolver ruta
            string rutaImagen = null;
            if (!string.IsNullOrEmpty(dto.Imagen))
            {
                rutaImagen = GuardarImagenBase64YDevolverRuta(dto.Imagen);
            }

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
                p_imagen = rutaImagen
            };

            var result = await conn.QuerySingleAsync<string>(sql, parametros);

            return result;
        }

        // --- helper: decodifica, (opcionalmente) comprime/redimensiona y guarda archivo ---
        private string GuardarImagenBase64YDevolverRuta(string dataImage)
        {
            if (string.IsNullOrWhiteSpace(dataImage))
                throw new ArgumentException("La imagen enviada está vacía.");

            // Extraer base64 si viene con data URI
            var base64Data = dataImage.Contains(",") ? dataImage.Split(',', 2)[1] : dataImage;
            base64Data = base64Data.Trim();

            // Convertir a bytes
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64Data);
            }
            catch
            {
                throw new ArgumentException("La imagen no es un Base64 válido.");
            }

            // Validar tamaño (opcional)
            if (bytes.Length > MaxImageBytes)
            {
                // Si quieres, aquí podrías procesar/redimensionar. Por ahora, lanzamos excepción.
                // Alternativamente podrías redimensionar con ImageSharp antes de guardar.
                throw new ArgumentException($"La imagen excede el tamaño máximo permitido de {MaxImageBytes} bytes.");
            }

            // Carpeta wwwroot/imagenes (usa WebRootPath proporcionado por IWebHostEnvironment)
            var carpeta = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), "imagenes");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            // Generar nombre único y guardar (aquí usamos .png por simplicidad)
            var nombreArchivo = $"{Guid.NewGuid()}.png";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            File.WriteAllBytes(rutaCompleta, bytes);

            // Devolver ruta relativa para guardar en DB (empieza con '/')
            return $"/imagenes/{nombreArchivo}";
        }

        // --- Métodos existentes sin cambios funcionales ---
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
                                 privacidad_texto as PrivacidadTexto,
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

        public async Task<List<ReportePorUsuarioWhitImagenDto>> ReportePorUsuarioWhitImagen(int usuarioId)
        {
            await using var conn = DbConnection();
            await conn.OpenAsync();

            const string sql = @"SELECT 
                             reporte_id,
                             titulo as Titulo,
                             categoria as Categoria,
                             estado as Estado,
                             privacidad_texto as PrivacidadTexto,
                             fecha_formateada as FechaFormateada,
                             descripcion as descripcion,
                             imagen as ImagenUrl
                         FROM funcion_ReportePorUsuarioWhitImagen(@p_usuario_id);";  // <- nombre de función correcto

            var parametros = new
            {
                p_usuario_id = usuarioId
            };

            var result = await conn.QueryAsync<ReportePorUsuarioWhitImagenDto>(sql, parametros);

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
