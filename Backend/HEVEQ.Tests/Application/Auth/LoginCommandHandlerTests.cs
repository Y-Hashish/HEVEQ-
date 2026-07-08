using FluentAssertions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Auth.Commad.Login;
using HEVEQ.Application.Features.Auth.DTOs;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Identity;
using HEVEQ.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HEVEQ.Tests.Application.Auth;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        var store = new UserStore<ApplicationUser, IdentityRole<Guid>, ApplicationDbContext, Guid>(_context);
        var optionsAccessor = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        var userValidators = new List<IUserValidator<ApplicationUser>> { new UserValidator<ApplicationUser>() };
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>> { new PasswordValidator<ApplicationUser>() };
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var logger = new Logger<UserManager<ApplicationUser>>(new LoggerFactory());

        _userManager = new UserManager<ApplicationUser>(
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            null,
            logger);

        var roleStore = new RoleStore<IdentityRole<Guid>, ApplicationDbContext, Guid>(_context);
        var roleManager = new RoleManager<IdentityRole<Guid>>(
            roleStore,
            null,
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Logger<RoleManager<IdentityRole<Guid>>>(new LoggerFactory()));

        roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Customer" }).Wait();
        roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Provider" }).Wait();

        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new LoginCommandHandler(_userManager, _jwtServiceMock.Object, _context);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@test.com", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Message.Should().Be("Invalid Email or Password");
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserName = "test@heveq.com",
            Email = "test@heveq.com",
            EmailConfirmed = true
        };
        await _userManager.CreateAsync(user, "CorrectPassword123!");

        var command = new LoginCommand("test@heveq.com", "WrongPassword123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Message.Should().Be("Invalid Email or Password");
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldRequireEmailConfirmation_WhenUserEmailIsNotConfirmedAndRoleIsCustomer()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserName = "customer@heveq.com",
            Email = "customer@heveq.com",
            EmailConfirmed = false,
            FirstName = "John",
            LastName = "Doe"
        };
        await _userManager.CreateAsync(user, "Password123!");
        await _userManager.AddToRoleAsync(user, "Customer");

        var command = new LoginCommand("customer@heveq.com", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.RequiresEmailConfirmation.Should().BeTrue();
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Email confirmation is required before login.");
    }

    [Fact]
    public async Task Handle_ShouldLoginSuccessfully_AndGenerateNewRefreshToken_WhenNoActiveTokenExists()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserName = "valid@heveq.com",
            Email = "valid@heveq.com",
            EmailConfirmed = true,
            FirstName = "Valid",
            LastName = "User"
        };
        await _userManager.CreateAsync(user, "Password123!");
        await _userManager.AddToRoleAsync(user, "Customer");

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("FakeAccessToken");

        var generatedRefreshToken = new RefreshToken
        {
            Token = "FakeRefreshToken",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns(generatedRefreshToken);

        var command = new LoginCommand("valid@heveq.com", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.AccessToken.Should().Be("FakeAccessToken");
        result.RefreshToken.Should().Be("FakeRefreshToken");
        _context.RefreshTokens.Any(rt => rt.Token == "FakeRefreshToken").Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
