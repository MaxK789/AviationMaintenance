namespace Aviation.WebApi.GraphQL.Payloads;

public class UserError
{
    public string Message { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
