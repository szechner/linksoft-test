using FluentValidation;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.App.Abstractions;
using SocialNetworkAnalyzer.App.Abstractions.Base;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Data.Repositories;

namespace SocialNetworkAnalyzer.App.DataSet.GetDataSets;

/// <summary>
/// Logic to handle the <see cref="GetDataSetStatisticsQuery"/> 
/// </summary>
public sealed class GetDataSetStatisticsQueryHandler(IDataSetStatisticsRepository dataSetStatisticsRepository, ILogger<GetDataSetStatisticsQueryHandler> logger) : IPagedQueryRequestHandler<GetDataSetStatisticsQuery, GetDataSetStatisticsQueryResultModel>
{
    public async Task<IPagedQueryResult<GetDataSetStatisticsQueryResultModel>> Handle(GetDataSetStatisticsQuery query, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);

        cancellationToken.ThrowIfCancellationRequested();

        logger.LogDebug("Handling GetDataSetStatisticsQuery");

        var validator = new GetDataSetStatisticsQueryValidator();
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var totalCount = await dataSetStatisticsRepository.CountRows(cancellationToken);

        if (totalCount == 0)
        {
            logger.LogInformation("No data sets statistics found at page {Page} with page size {PageSize}", query.Page, query.PageSize);
            return PagedQueryResult.Empty<GetDataSetStatisticsQueryResultModel>(query.PageSize);
        }

        var data = await dataSetStatisticsRepository.GetDataSetStatistics(query.PageSize, query.PageSize * query.Page, cancellationToken);

        var model = data.Select(t => new GetDataSetStatisticsQueryResultModel(
            t.DataSetId,
            t.DataSet.Name,
            t.NodesCount,
            t.RelationshipsCount,
            t.AvgRelationsCount,
            t.AvgGroupRelationsCount,
            t.Error,
            t.State
        )).ToList();

        logger.LogInformation("Returning {Count} data sets statistics at page {Page} with page size {PageSize}", model.Count, query.PageSize, query.PageSize);
        return PagedQueryResult.Create(model, totalCount, query.Page, query.PageSize);
    }
}