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
    public class LoginRepository : ILogin
    {
        PostgreSQLConnection _connection;
        UserRepository _userRepository;
        public LoginRepository(PostgreSQLConnection connection)
        {
            _connection = connection;
            _userRepository = new UserRepository(_connection);
        }
        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);

        //        public async Task<UserDto> GetLogin(string user, string contraseña)
        //        {
        //            await using var conn = DbConnection();
        //            await conn.OpenAsync();

        //            await using var cmd = new NpgsqlCommand("SELECT * FROM Funcion_ObtenerLogin(@p_username, @p_contrasena);", conn);
        //            cmd.Parameters.AddWithValue("p_username", user);
        //            cmd.Parameters.AddWithValue("p_contrasena", contraseña);

        //            await using var reader = await cmd.ExecuteReaderAsync();

        //            if (await reader.ReadAsync())
        //            {
        //                return new UserDto
        //                {
        ////                    IdLogin = reader.GetInt64(reader.GetOrdinal("id_login")),
        //                    Username = reader.GetString(reader.GetOrdinal("username")),
        //                    Contrasena = reader.GetString(reader.GetOrdinal("contrasena")),
        //                    Token = reader.IsDBNull(reader.GetOrdinal("token")) ? null : reader.GetString(reader.GetOrdinal("token")),
        //                    IdUsuario = reader.GetInt32(reader.GetOrdinal("id_usuario")),
        //                    Nombres = reader.GetString(reader.GetOrdinal("nombres")),
        //                    Apellidos = reader.GetString(reader.GetOrdinal("apellidos")),
        //                    //Correo = reader.GetString(reader.GetOrdinal("correo")),
        //                    NoCel = reader.GetString(reader.GetOrdinal("no_cel")),
        //                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        //                };
        //            }

        //            return null; // No se encontró el usuario
        //        }

        public async Task<LoginResponse> Login(LoginDto loginDto)
        {

            UserDto? targetUser = await FindUserByusername(loginDto.user);

            if (targetUser == null) throw new HttpResponseException(StatusCodes.Status401Unauthorized);

            bool passwordIsValid = BC.EnhancedVerify(loginDto.contraseña, targetUser.Contrasena);

            if (!passwordIsValid) throw new HttpResponseException(StatusCodes.Status401Unauthorized);

            string[] tokens = GenerateTokens(
              Convert.ToString(targetUser.IdUsuario)
            );

            await UpdateRT(targetUser.IdUsuario, tokens[1]);

            return new LoginResponse
            {
                User = targetUser,
                Role = targetUser.rol,
                accessToken = tokens[0],
                refreshToken = tokens[1]
            };
        }

        /// <summary>
        /// Elimina el refresh token (lo setea a null) del registro del usuario con la Id dada
        /// </summary>
        /// <param name="userId">Id del usuario al que se le "cerrara la sesion"</param>
        /// <returns></returns>
        public async Task<bool> LogOut(int userId)
        {
            string sqlQuery = "select * from fun_auth_clearRT(p_userId := @userId);";

            using (NpgsqlConnection database = DbConnection())
            {
                await database.OpenAsync();
                await database.QueryAsync(
                  sqlQuery,
                  param: new
                  {
                      userId
                  }
                );
                await database.CloseAsync();
            }
            return true;
        }
//falta esta funcion
        private async Task<UserDto?> FindUserByusername(string username)
        {
            string sqlQuery = "select * from view_usuario where Username = @username;";

            UserDto? targetUser = (await _userRepository.UserQueryAsync(sqlQuery, new { username })).FirstOrDefault();
            return targetUser;
        }

        /// <summary>
        /// Busca un usuario en la base de datos en base a un di dado
        /// </summary>
        /// <param name="userId">id del usuario a buscar</param>
        /// <returns></returns>
        private async Task<UserDto?> FindUserById(int userId)
        {
            string sqlQuery = "select * from view_usuario where idUsuario = @userId;";

            UserDto? targetUser = (await _userRepository.UserQueryAsync(sqlQuery, new { userId })).FirstOrDefault();
            return targetUser;
        }

        /// <summary>
        /// Genera un token que guarda el id del usuario
        /// </summary>
        /// <param name="userId">Id del usuario que guardara el token</param>
        /// <returns>Token con el token del usuario como payload</returns>
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

        /// <summary>
        /// Hashea el refreshToken y lo inserta en el campo "refreshToken" del usuario con la id dada
        /// </summary>
        /// <param name="userId">Id del usuario al que se le seteara / actualizara el refreshToken</param>
        /// <param name="refreshToken">RefreshToken que sera seteado despues de hashearlo</param>
        /// <returns></returns>
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

        /// <summary>
        /// Obtiene los claims del refreshToken, en caso de que no haya claims se retornara null. 
        /// En caso de que existan los claims 
        /// necesarios se extraeran los id de usaurio y cendi para generar nuevos tokens 
        /// y setearlos en base de datos
        /// </summary>
        /// <param name="refreshTokenDto">string del refreshToken</param>
        /// <returns></returns>
        public async Task<RefreshTokenResponse> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            List<Claim> claims = GetClaimsFromToken(refreshTokenDto.RefreshToken);
            if (claims.Count == 0) return null;

            string userId = claims.FirstOrDefault(claim => claim.Type == "UserId").Value;
            string cendiId = claims.FirstOrDefault(claim => claim.Type == "CendiId").Value;

            if (userId == null || cendiId == null) return null;

            string[] tokens = GenerateTokens(userId);

            UserDto user = await FindUserById(Convert.ToInt32(userId));

            if (user.refreshToken == null) throw new HttpResponseException(StatusCodes.Status401Unauthorized);

            bool rtMatches = BC.EnhancedVerify(refreshTokenDto.RefreshToken, user.refreshToken);

            if (!rtMatches)
            {
                await LogOut(user.IdUsuario);
                throw new HttpResponseException(StatusCodes.Status401Unauthorized);
            }

            await UpdateRT(Convert.ToInt32(userId), tokens[1]);

            return new RefreshTokenResponse
            {
                AccessToken = tokens[0],
                RefreshToken = tokens[1],
            };
        }

        /// <summary>
        /// Valida el refreshToken (incluye la validacion de la expiracion) y si este es valido retorna
        /// una lista de los claims del token, si no, retorna una lista vacia
        /// </summary>
        /// <param name="refreshToken">string del refreshtoken</param>
        /// <returns></returns>
        private List<Claim> GetClaimsFromToken(string refreshToken)
        {
            var rtKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("RT_SECRET"));
            SymmetricSecurityKey rtSigningKey = new(rtKey);

            TokenValidationParameters validationParameters = new()
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = rtSigningKey,
                ValidateLifetime = true,
                LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) => {
                    return expires.HasValue && expires > DateTime.UtcNow;
                }
            };

            try
            {
                ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validationParameters, out _);
                return principal.Claims.ToList();
            }
            catch (SecurityTokenValidationException error)
            {
                return [];
            }

        }
       public async Task<string> RegistroTemporal(RegisterTemp registerTemp)
       {
            if (registerTemp is null)
                throw new HttpResponseException(StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(registerTemp.username) || string.IsNullOrWhiteSpace(registerTemp.contrasena))
                throw new HttpResponseException(StatusCodes.Status400BadRequest);

            var hashedPassword = BC.EnhancedHashPassword(registerTemp.contrasena);

            string sql = @"
                SELECT Funcion_RegistrarUsuario(
                    p_nombre := @Nombre,
                    p_apellidos := @Apellidos,
                    p_username := @Username,
                    p_contrasena := @Contrasena,
                    p_correo := @Correo,
                    p_idrol := @IdRol,
                    p_num_cel := @NumCel
                );";

            using var db = DbConnection();
            await db.OpenAsync();

            try
            {
                var result = await db.QueryFirstOrDefaultAsync<int>(sql, new
                {
                    Nombre = registerTemp.nombre,
                    Apellidos = registerTemp.apellidos,
                    Username = registerTemp.username,
                    Contrasena = hashedPassword,
                    Correo = registerTemp.correo,
                    IdRol = registerTemp.idrol,
                    NumCel = registerTemp.num_cel
                });

                return result > 0 ? "registro exitoso" : "error en el registro";
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
            {
                // Este código SQLState significa violación de unique constraint
                return $"El username '{registerTemp.username}' ya existe";
            }
       }

    }
}