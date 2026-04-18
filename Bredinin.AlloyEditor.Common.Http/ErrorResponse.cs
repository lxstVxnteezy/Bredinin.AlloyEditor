namespace Bredinin.AlloyEditor.Common.Http;

public record ErrorResponse(int StatusCode,string Message, string? Detail = null);
