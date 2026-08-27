namespace WordoGuessr.API.BuildingBlocks.ProblemDetails;

public static class ProblemDetailsMapping
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails Create(int statusCode, string title, string detail, string code)
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problemDetails.Extensions["code"] = code;
        return problemDetails;
    }
}
