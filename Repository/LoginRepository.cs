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
namespace ReportaUTS.Repository
{
    public class LoginRepository : ILogin
    {
        PostgreSQLConnection _connection;
        public LoginRepository(PostgreSQLConnection connection)
        {
            _connection = connection;
        }
        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);

        public async Task<UserDto> GetLogin(string user, string contraseña)
        {
            await using var conn = DbConnection();
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT * FROM Funcion_ObtenerLogin(@p_username, @p_contrasena);", conn);
            cmd.Parameters.AddWithValue("p_username", user);
            cmd.Parameters.AddWithValue("p_contrasena", contraseña);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserDto
                {
                    IdLogin = reader.GetInt64(reader.GetOrdinal("id_login")),
                    Username = reader.GetString(reader.GetOrdinal("username")),
                    Contrasena = reader.GetString(reader.GetOrdinal("contrasena")),
                    Token = reader.IsDBNull(reader.GetOrdinal("token")) ? null : reader.GetString(reader.GetOrdinal("token")),
                    RefrechToken = reader.IsDBNull(reader.GetOrdinal("refrechtoken")) ? null : reader.GetString(reader.GetOrdinal("refrechtoken")),
                    IdUsuario = reader.GetInt32(reader.GetOrdinal("id_usuario")),
                    Nombres = reader.GetString(reader.GetOrdinal("nombres")),
                    Apellidos = reader.GetString(reader.GetOrdinal("apellidos")),
                    Correo = reader.GetString(reader.GetOrdinal("correo")),
                    NoCel = reader.GetString(reader.GetOrdinal("no_cel")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                };
            }

            return null; // No se encontró el usuario
        }

        public async Task<LoginResponse> Login(LoginDto loginDto)
        {

            UserDto? targetUser = await FindUserByEmail(loginDto.user);

            if (targetUser == null) throw new HttpResponseException(StatusCodes.Status401Unauthorized);

            bool passwordIsValid = BC.EnhancedVerify(loginDto.contraseña, targetUser.Contrasena);

            if (!passwordIsValid) throw new HttpResponseException(StatusCodes.Status401Unauthorized);

            string[] tokens = GenerateTokens(
              Convert.ToString(targetUser.IdUsuario)
            );

            await UpdateRT(targetUser.IdUsuario, tokens[1]);

            return new LoginResponse
            {
                User = targetUser.,
                Role = targetUser.,
                accessToken = tokens[0],
                refreshToken = tokens[1]
            };
        }
        private async Task<UserDto?> FindUserByEmail(string user)
        {
            string sqlQuery = "select * from view_usuario where emailUsuario = @email";

            UserDto? targetUser = (await _connection.UserQueryAsync(sqlQuery, new { user })).FirstOrDefault();
            return targetUser;
        }
        private async Task UpdateRT(int userId, string refreshToken)
        {
            string sqlQuery = "select * from fun_auth_updateRT(p_userId := @userId, p_refreshtoken := @refreshToken);";

            refreshToken = BC.EnhancedHashPassword(refreshToken);

            using (NpgsqlConnection database = DbConnection())
            {
                await database.OpenAsync();
                await database.QueryAsync(
                  sqlQuery,
                  param: new
                  {
                      userId,
                      refreshToken
                  }
                );
                await database.CloseAsync();
            }
        }

        private string[] GenerateTokens(string userId)
        {
            //string userId = Convert.ToString(user.IdUsuario);
            //string cendiId = Convert.ToString(user.Cendi.IdCendi);

            JwtSecurityTokenHandler tokenHandler = new();
            var jwtKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET"));
            var rtKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("RT_SECRET"));
            SymmetricSecurityKey jwtSigningKey = new(jwtKey);
            SymmetricSecurityKey rtSigningKey = new(rtKey);

            SecurityTokenDescriptor jwtDescriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim("UserId", userId),
                }),
                SigningCredentials = new SigningCredentials(jwtSigningKey, SecurityAlgorithms.HmacSha256Signature),
                Expires = DateTime.UtcNow.AddHours(1)
            };

            SecurityTokenDescriptor rtDescriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim("UserId", userId),
                }),
                SigningCredentials = new SigningCredentials(rtSigningKey, SecurityAlgorithms.HmacSha256Signature),
                Expires = DateTime.UtcNow.AddDays(1)
            };

            SecurityToken jwtToken = tokenHandler.CreateToken(jwtDescriptor);
            SecurityToken rtToken = tokenHandler.CreateToken(rtDescriptor);
            string jwt = tokenHandler.WriteToken(jwtToken);
            string rt = tokenHandler.WriteToken(rtToken);
            return [jwt, rt];
        }
    }
}