using FluentAssertions;
using FluentValidation.TestHelper;
using HEVEQ.Application.Features.Auth.Commad.Login;
using Xunit;

namespace HEVEQ.Tests.Application.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailIsEmpty()
    {
        // Arrange
        var command = new LoginCommand("", "Password123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.email)
            .WithErrorMessage("Email is Required");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new LoginCommand("invalid-email", "Password123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.email)
            .WithErrorMessage("this is not a correct email format");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenPasswordIsEmpty()
    {
        // Arrange
        var command = new LoginCommand("test@heveq.com", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.password)
            .WithErrorMessage("Password is Required");
    }

    [Fact]
    public void Validator_ShouldNotHaveAnyErrors_WhenCommandIsValid()
    {
        // Arrange
        var command = new LoginCommand("test@heveq.com", "Password123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
