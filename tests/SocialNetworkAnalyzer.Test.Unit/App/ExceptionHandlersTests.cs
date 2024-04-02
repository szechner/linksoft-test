using FakeItEasy;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SocialNetworkAnalyzer.App.ExceptionHandlers;
using SocialNetworkAnalyzer.Data.Abstraction;

namespace SocialNetworkAnalyzer.Test.Unit.App;

[TestFixture]
public class ExceptionHandlersTests
{
    [Test]
    [CancelAfter(90_000)]
    public async Task BadRequestExceptionHandler_Try_Handle()
    {
        var httpContext = new DefaultHttpContext();

        var services = new ServiceCollection();

        var transactionManager = new Fake<ITransactionManager>();
        transactionManager.CallsTo(tm => tm.Rollback()).DoesNothing();
        
        services.AddScoped<ITransactionManager>(sp => transactionManager.FakedObject);
        
        httpContext.RequestServices = services.BuildServiceProvider();
        
        var exception = new Exception("Test");
        
        var exceptionHandler = new BadRequestExceptionHandler();

        var handled = await exceptionHandler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var validationException = new ValidationException("Test", new []
        {
            new ValidationFailure("test","test")
        });
        handled = await exceptionHandler.TryHandleAsync(httpContext, validationException, CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        httpContext.Response.ContentType.Should().Be("application/problem+json");
    }
    
    [Test]
    [CancelAfter(90_000)]
    public async Task GlobalExceptionHandler_Try_Handle()
    {
        var httpContext = new DefaultHttpContext();

        var services = new ServiceCollection();

        var transactionManager = new Fake<ITransactionManager>();
        transactionManager.CallsTo(tm => tm.Rollback()).DoesNothing();
        
        services.AddScoped<ITransactionManager>(sp => transactionManager.FakedObject);
        
        httpContext.RequestServices = services.BuildServiceProvider();
        
        var exception = new Exception("Test");
        
        var exceptionHandler = new GlobalExceptionHandler();

        var validationException = new ValidationException("Test");
        var handled = await exceptionHandler.TryHandleAsync(httpContext, validationException, CancellationToken.None);

        handled.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        handled = await exceptionHandler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        httpContext.Response.ContentType.Should().Be("application/problem+json");
        
        
    }
}