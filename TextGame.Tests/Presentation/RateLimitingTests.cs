using System.Reflection;
using Microsoft.AspNetCore.RateLimiting;
using TextGame.Presentation.Controllers;

namespace TextGame.Tests.Presentation;

/// <summary>
/// notes/backend-requests.md п. 12: лимит остаётся на входе и регистрации, но выход из-под него
/// выведен — иначе игрок, упёршийся в 429, не может разлогиниться.
/// </summary>
public class RateLimitingTests
{
    [Fact]
    public void LogIn_IsRateLimited()
    {
        // Arrange
        MethodInfo login = typeof(AuthController).GetMethod(nameof(AuthController.LogInAsync))!;

        // Act
        var attribute = login.GetCustomAttribute<EnableRateLimitingAttribute>(inherit: true);

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void LogOut_IsNotRateLimited()
    {
        // Arrange
        MethodInfo logout = typeof(AuthController).GetMethod(nameof(AuthController.LogOut))!;

        // Act
        var attribute = logout.GetCustomAttribute<EnableRateLimitingAttribute>(inherit: true);

        // Assert
        attribute.Should().BeNull();
    }

    [Fact]
    public void AuthController_HasNoClassLevelRateLimit()
    {
        // Arrange
        var attributes = typeof(AuthController).GetCustomAttributes<EnableRateLimitingAttribute>(inherit: true);

        // Assert
        attributes.Should().BeEmpty();
    }

    [Fact]
    public void Register_IsRateLimited()
    {
        // Arrange
        MethodInfo register = typeof(UsersController).GetMethod(nameof(UsersController.RegisterAsync))!;

        // Act
        var classAttribute = typeof(UsersController).GetCustomAttribute<EnableRateLimitingAttribute>(inherit: true);
        var methodAttribute = register.GetCustomAttribute<EnableRateLimitingAttribute>(inherit: true);

        // Assert
        // UsersController по плану 1.2.0 не трогаем: лимит остаётся на классе.
        classAttribute.Should().NotBeNull();
        methodAttribute.Should().BeNull();
    }
}
