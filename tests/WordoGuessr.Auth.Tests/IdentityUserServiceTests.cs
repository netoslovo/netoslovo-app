using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using FakeItEasy;
using Shouldly;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.App.Services;
using WordoGuessr.Auth.Domain;
using WordoGuessr.Auth.Infra.Database;
using WordoGuessr.Auth.Infra.Identity;
using WordoGuessr.Common.Domain.ValueObjects;
using Xunit;

namespace WordoGuessr.Auth.Tests;

public sealed class IdentityUserServiceTests : IClassFixture<AuthDbContextFixture>
{
    private const string OccupiedUserName = "Player_123456";
    private const int ExpectedShortPoolCreateAttempts = 2;
    private const int ExpectedExtendedPoolCreateAttempts = 3;
    private static readonly DateTimeOffset _currentTime = new DateTimeOffset(2026, 8, 15, 10, 0, 0, TimeSpan.Zero);

    private readonly AuthDbContextFixture _fixture;
    private readonly ITestOutputHelper _output;

    public IdentityUserServiceTests(AuthDbContextFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task CreateConfirmedEmailUser_ShouldCreateConfirmedUser()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await ClearUsers(ct);

        const string userName = "Player_654321";
        var generator = A.Fake<IUserNameGenerator>();
        A.CallTo(() => generator.Generate()).Returns(userName);
        var targetUserId = Guid.NewGuid();
        var email = EmailAddress.Create("new@wordoguessr.test");

        var validator = CreateSuccessfulUserValidator();
        await using var serviceProvider = CreateServiceProvider(validator);
        await using var scope = serviceProvider.CreateAsyncScope();
        var userService = CreateUserService(scope, generator);

        // ACT
        var result = await userService.CreateConfirmedEmailUser(targetUserId, email);

        // ASSERT
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(targetUserId);
        result.Value.UserName.ShouldBe(userName);
        result.Value.Email.ShouldBe(email.Value);
        result.Value.EmailConfirmed.ShouldBeTrue();
        result.Value.CreatedAt.ShouldBe(_currentTime);
        A.CallTo(() => generator.Generate()).MustHaveHappenedOnceExactly();

        await using var assertDbContext = _fixture.CreateDbContext();
        var persistedUser = await assertDbContext.Users.SingleAsync(user => user.Id == targetUserId, ct);
        persistedUser.UserName.ShouldBe(userName);
        persistedUser.EmailConfirmed.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateConfirmedEmailUser_ShouldRetryAfterIdentityDuplicateUserName()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await ClearUsers(ct);

        const string duplicateUserName = "Player_111111";
        const string availableUserName = "Player_222222";
        var generator = A.Fake<IUserNameGenerator>();
        A.CallTo(() => generator.Generate())
            .ReturnsNextFromSequence(duplicateUserName, availableUserName);

        var describer = new IdentityErrorDescriber();
        var validator = A.Fake<IUserValidator<ApplicationUser>>();
        A.CallTo(() => validator.ValidateAsync(
                A<UserManager<ApplicationUser>>._,
                A<ApplicationUser>._))
            .ReturnsNextFromSequence(
                IdentityResult.Failed(describer.DuplicateUserName(duplicateUserName)),
                IdentityResult.Success);

        await using var serviceProvider = CreateServiceProvider(validator);
        await using var scope = serviceProvider.CreateAsyncScope();
        var userService = CreateUserService(scope, generator);

        // ACT
        var result = await userService.CreateConfirmedEmailUser(
            Guid.NewGuid(),
            EmailAddress.Create("retry@wordoguessr.test"));

        // ASSERT
        result.IsSuccess.ShouldBeTrue();
        result.Value.UserName.ShouldBe(availableUserName);
        A.CallTo(() => generator.Generate())
            .MustHaveHappenedANumberOfTimesMatching(callCount => callCount == 2);
        A.CallTo(() => generator.GenerateFromExtendedPool())
            .MustNotHaveHappened();
        A.CallTo(() => validator.ValidateAsync(
                A<UserManager<ApplicationUser>>._,
                A<ApplicationUser>._))
            .MustHaveHappenedANumberOfTimesMatching(callCount => callCount == 2);
    }

    [Fact]
    public async Task CreateConfirmedEmailUser_ShouldNotRetryAfterDatabaseDuplicateEmail()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await ClearUsers(ct);

        const string existingEmail = "existing@wordoguessr.test";
        await SeedUser(OccupiedUserName, existingEmail, ct);

        var generator = A.Fake<IUserNameGenerator>();
        A.CallTo(() => generator.Generate()).Returns("Player_333333");
        var targetUserId = Guid.NewGuid();

        var validator = CreateSuccessfulUserValidator();
        await using var serviceProvider = CreateServiceProvider(validator);
        await using var scope = serviceProvider.CreateAsyncScope();
        var userService = CreateUserService(scope, generator);

        // ACT
        var result = await userService.CreateConfirmedEmailUser(
            targetUserId,
            EmailAddress.Create(existingEmail));

        // ASSERT
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(UserErrorCode.DuplicateEmail);
        A.CallTo(() => generator.Generate()).MustHaveHappenedOnceExactly();

        await using var assertDbContext = _fixture.CreateDbContext();
        var userWasCreated = await assertDbContext.Users.AnyAsync(user => user.Id == targetUserId, ct);
        userWasCreated.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateConfirmedEmailUser_ShouldPreserveUnknownIdentityError()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await ClearUsers(ct);

        const string errorCode = "UnexpectedIdentityError";
        var generator = A.Fake<IUserNameGenerator>();
        A.CallTo(() => generator.Generate()).Returns("Player_444444");

        var validator = A.Fake<IUserValidator<ApplicationUser>>();
        A.CallTo(() => validator.ValidateAsync(
                A<UserManager<ApplicationUser>>._,
                A<ApplicationUser>._))
            .Returns(IdentityResult.Failed(new IdentityError { Code = errorCode }));

        await using var serviceProvider = CreateServiceProvider(validator);
        await using var scope = serviceProvider.CreateAsyncScope();
        var userService = CreateUserService(scope, generator);

        // ACT
        var exception = await Should.ThrowAsync<UserServiceException>(async () =>
            await userService.CreateConfirmedEmailUser(
                Guid.NewGuid(),
                EmailAddress.Create("unknown-error@wordoguessr.test")));

        // ASSERT
        exception.Errors.ShouldHaveSingleItem().Code.ShouldBe(errorCode);
        A.CallTo(() => generator.Generate()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateConfirmedEmailUser_ShouldThrowWhenUserNameAttemptsAreExhausted()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await ClearUsers(ct);
        await SeedUser(OccupiedUserName, "occupied@wordoguessr.test", ct);

        var generator = A.Fake<IUserNameGenerator>();
        A.CallTo(() => generator.Generate()).Returns(OccupiedUserName);
        A.CallTo(() => generator.GenerateFromExtendedPool()).Returns(OccupiedUserName);
        var targetUserId = Guid.NewGuid();

        var validator = CreateSuccessfulUserValidator();
        await using var serviceProvider = CreateServiceProvider(validator);
        await using var scope = serviceProvider.CreateAsyncScope();
        var userService = CreateUserService(scope, generator);

        // ACT
        var exception = await Should.ThrowAsync<UserServiceException>(async () =>
            await userService.CreateConfirmedEmailUser(
                targetUserId,
                EmailAddress.Create("new@wordoguessr.test")));

        // ASSERT
        A.CallTo(() => generator.Generate())
            .MustHaveHappenedANumberOfTimesMatching(callCount => callCount == ExpectedShortPoolCreateAttempts);
        A.CallTo(() => generator.GenerateFromExtendedPool())
            .MustHaveHappenedANumberOfTimesMatching(callCount => callCount == ExpectedExtendedPoolCreateAttempts);
        exception.Errors.ShouldHaveSingleItem().Code.ShouldBe(
            UserErrorCode.UserNameGenerationAttemptsExhausted.ToString());

        await using var assertDbContext = _fixture.CreateDbContext();
        var userWasCreated = await assertDbContext.Users.AnyAsync(user => user.Id == targetUserId, ct);
        userWasCreated.ShouldBeFalse();
    }

    private ServiceProvider CreateServiceProvider(IUserValidator<ApplicationUser> validator)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => _fixture.CreateDbContext(_output));
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AuthDbContext>();
        services.RemoveAll<IUserValidator<ApplicationUser>>();
        services.AddSingleton(validator);

        return services.BuildServiceProvider();
    }

    private static IUserValidator<ApplicationUser> CreateSuccessfulUserValidator()
    {
        var validator = A.Fake<IUserValidator<ApplicationUser>>();
        A.CallTo(() => validator.ValidateAsync(
                A<UserManager<ApplicationUser>>._,
                A<ApplicationUser>._))
            .Returns(IdentityResult.Success);

        return validator;
    }

    private static IdentityUserService CreateUserService(
        AsyncServiceScope scope,
        IUserNameGenerator generator)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var timeProvider = new FakeTimeProvider();
        timeProvider.SetUtcNow(_currentTime);

        return new IdentityUserService(userManager, timeProvider, generator);
    }

    private async Task ClearUsers(CancellationToken ct)
    {
        await using var dbContext = _fixture.CreateDbContext();
        await dbContext.Users.ExecuteDeleteAsync(ct);
    }

    private async Task SeedUser(string userName, string email, CancellationToken ct)
    {
        await using var dbContext = _fixture.CreateDbContext();
        var user = new ApplicationUser(Guid.NewGuid(), email, userName, _currentTime)
        {
            NormalizedUserName = userName.ToUpperInvariant(),
            NormalizedEmail = email.ToUpperInvariant()
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);
    }

}
