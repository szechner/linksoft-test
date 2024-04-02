using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkAnalyzer.App.DataSet.CreateDataSet;
using SocialNetworkAnalyzer.App.Events;
using SocialNetworkAnalyzer.App.WebApi.Core;
using SocialNetworkAnalyzer.App.WebApi.Validators;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using IFormFile = Microsoft.AspNetCore.Http.IFormFile;

namespace SocialNetworkAnalyzer.App.WebApi.Endpoints;

/// <summary>
/// Endpoint to create a new data set
/// <para>Resource: /datasets</para>
/// </summary>
public class CreateDataSet : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("datasets", async ([FromForm] IFormFile file, [FromForm] string dataSetName, [FromServices] ISender sender, CancellationToken cancellationToken) =>
            {
                using var _ = Measurement.ElapsedTime(this);
                var validator = new TextFileValidator();
                await validator.ValidateAndThrowAsync(file, cancellationToken);
                
                var tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));

                await using (var tempFile = File.Create(tempFileName))
                {
                    await file.CopyToAsync(tempFile, cancellationToken);
                }

                var command = new CreateDataSetCommand(dataSetName, tempFileName);

                var result = await sender.Send(command, cancellationToken);
                return result;
            })
            .DisableAntiforgery()
            .Produces<ProblemDetails>()
            .Produces<DataSetCreatedEvent>();
    }
}