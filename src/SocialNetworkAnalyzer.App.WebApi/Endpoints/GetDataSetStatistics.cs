using MediatR;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkAnalyzer.App.Abstractions.Base;
using SocialNetworkAnalyzer.App.DataSet.GetDataSets;
using SocialNetworkAnalyzer.App.WebApi.Core;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;

namespace SocialNetworkAnalyzer.App.WebApi.Endpoints;

/// <summary>
/// Endpoint to get data set statistics
/// <para>Resource: /datasets</para>
/// </summary>
public class GetDataSetStatistics : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("datasets", async ([FromQuery] int? page, [FromQuery] int? pageSize, [FromServices] ISender sender, CancellationToken cancellationToken) =>
            {
                using var _ = Measurement.ElapsedTime(this);

                var query = new GetDataSetStatisticsQuery(page ?? 0, pageSize ?? 10);
                var result = await sender.Send(query, cancellationToken);
                return result;
            })
            .Produces<ProblemDetails>()
            .Produces<PagedQueryResult<GetDataSetStatisticsQueryResultModel>>();
    }
}