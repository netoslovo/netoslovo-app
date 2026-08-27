using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Dto;

namespace WordoGuessr.Auth.App.UseCases.User.GetCurrentAuthState;

public sealed record GetCurrentAuthStateQuery : IQuery<AuthMeDto>;
