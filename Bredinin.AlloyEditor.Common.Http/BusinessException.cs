using System.Net;

namespace Bredinin.AlloyEditor.Common.Http;

public class BusinessException : Exception
{
    public HttpStatusCode StatusCode { get; }
    
    public BusinessException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message) 
    {
        StatusCode = statusCode;
    }
    
    public BusinessException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}