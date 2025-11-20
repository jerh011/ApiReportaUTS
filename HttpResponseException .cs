namespace ReportaUTS
{
    public class HttpResponseException:Exception
    {

        public int StatusCode { get; set; }
        new public dynamic? Data { get; set; } = null;
        new public string? Message { get; set; } = null;

        public HttpResponseException(int statusCode) => StatusCode = statusCode;

        public HttpResponseException(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }

        public HttpResponseException(int statusCode, dynamic data)
        {
            StatusCode = statusCode;
            Data = data;
        }

        public HttpResponseException(int statusCode, string message, dynamic data)
        {
            StatusCode = statusCode;
            Data = data;
            Message = message;
        }
    }
}