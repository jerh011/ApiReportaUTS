using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using ReportaUTS.Conexion;
using ReportaUTS.Dtos;
using ReportaUTS.Interfaces;
using ReportaUTS.Model;
using BC = BCrypt.Net.BCrypt;
using Dapper;

namespace ReportaUTS.Repository
{
    public class UserRepository
    {
        private PostgreSQLConnection _connectionString;

        public UserRepository(PostgreSQLConnection connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connectionString.ConnectionString);

    

        #region UserQueryAsync
        /// <summary>
        /// Permite ejecutar consultas sql con parametros en ellas.
        /// Mapea los datos regresados (los retornados view_usuarios unicamente) de los usuarios.
        /// </summary>
        /// <param name="sqlQuery">Consulta sql</param>
        /// <param name="parameters">Parametros de la consulta</param>
        /// <returns>IEnumerable (puede ser usado como si fuese una lista) con los datos resultantes de la ejecucion de la consulta sql</returns>
        public async Task<IEnumerable<UserDto>> UserQueryAsync(string sqlQuery, object? parameters = null)
        {

            IEnumerable<UserDto> users = [];

            using NpgsqlConnection database = DbConnection();

            try
            {
                await database.OpenAsync();
                var result = await database.QueryAsync<UserDto, Role, UserDto>(
                  sqlQuery,
                  param: parameters,
                  map: (user, role) => {
                      user.rol = role;
                      return user;
                  },
                  splitOn: "idRol"
                );

                users = result.Distinct();

                await database.CloseAsync();
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == DB_ERRORS.UNAUTHORIZED)
                    throw new HttpResponseException(StatusCodes.Status401Unauthorized, ex.MessageText);

                if (ex.SqlState == DB_ERRORS.CONFLICT)
                    throw new HttpResponseException(StatusCodes.Status409Conflict, ex.MessageText);

                if (ex.SqlState == DB_ERRORS.BAD_REQUEST)
                    throw new HttpResponseException(StatusCodes.Status400BadRequest, ex.MessageText);

                throw new HttpResponseException(StatusCodes.Status500InternalServerError);
            }
            catch (Exception)
            {
                throw new HttpResponseException(StatusCodes.Status500InternalServerError);
            }

            return users;
        }
        #endregion
    }
}