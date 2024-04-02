using SocialNetworkAnalyzer.App;
using SocialNetworkAnalyzer.App.ExceptionHandlers;
using SocialNetworkAnalyzer.App.WebApi.Core;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Core.Guards;
using SocialNetworkAnalyzer.Data.EntityFramework.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLogging();
builder.Services.AddEntityFramework(Guard.Require.ArgumentNotNull(builder.Configuration.GetConnectionString("LocalPostgresConnection"), message: "Connection string not found"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
{
    cfg.NotificationPublisher = new ParallelNotificationPublisher();
    cfg.NotificationPublisherType = typeof(ParallelNotificationPublisher);
    cfg.RegisterServicesFromAssembly(AppAssembly.Assembly);
});

builder.Services.AddExceptionHandlers();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

StaticLogger.Initialize(app.Services);

app.MapEndpoints();

app.UseExceptionHandler();

app.Run();

public partial class Program
{
}