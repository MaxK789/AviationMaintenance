namespace Aviation.WebApi.GraphQL.Payloads;

public class UserError
{
    public required string Message { get; init; }
    public required string Code { get; init; }
}
