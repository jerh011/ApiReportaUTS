namespace ReportaUTS.Conexion
{
    public class PostgreSQLConnection
    {
        public PostgreSQLConnection(string connectionString) => ConnectionString = connectionString;
        public string ConnectionString { get; set; }
    }

    // Posibles errores custom que pueden lanzar las funciones de postgresql
    public static class DB_ERRORS
    {
        public const string UNAUTHORIZED = "P0401";
        public const string BAD_REQUEST = "P0400";
        public const string CONFLICT = "P0409";
    }
}
