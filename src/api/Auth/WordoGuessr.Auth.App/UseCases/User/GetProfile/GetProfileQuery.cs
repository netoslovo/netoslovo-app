using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Dto;

namespace WordoGuessr.Auth.App.UseCases.User.GetProfile;

public sealed record GetProfileQuery(Guid UserId) : IQuery<ProfileDto?>;
