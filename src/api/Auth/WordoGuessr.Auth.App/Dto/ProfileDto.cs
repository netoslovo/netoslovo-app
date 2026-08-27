namespace WordoGuessr.Auth.App.Dto;

public sealed record ProfileDto(
    string Email,
    string UserName,
    DateTimeOffset? UserNameChangedAt,
    DateTimeOffset CreatedAt
);