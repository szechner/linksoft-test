using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.App.Abstractions;
using SocialNetworkAnalyzer.App.Events;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.Repositories;

namespace SocialNetworkAnalyzer.App.DataSetStatistics.EventHandlers.DataSetCreated;

/// <summary>
/// <see cref="DataSetCreatedEvent"/> handler to update the average relations count in the data set statistics
/// </summary>
public sealed class UpdateAvgRelationsCountEventHandler(IServiceScopeFactory serviceScopeFactory) : IEventHandler<DataSetCreatedEvent>
{
    public async Task Handle(DataSetCreatedEvent dataSetCreatedEvent, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<UpdateAvgRelationsCountEventHandler>>();

        var dataSetStatisticsRepository = serviceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        var relationshipRepository = serviceProvider.GetRequiredService<IRelationshipsRepository>();
        var transactionManager = serviceProvider.GetRequiredService<ITransactionManager>();

        try
        {
            var avgRelationsCount = await relationshipRepository.GetAvgRelationsCount(dataSetCreatedEvent.DataSetId, cancellationToken);
            await dataSetStatisticsRepository.UpdateAvgRelationsCount(dataSetCreatedEvent.DataSetId, avgRelationsCount, cancellationToken);
        }
        catch (Exception e)
        {
            transactionManager.Rollback();
            await dataSetStatisticsRepository.SetError(dataSetCreatedEvent.DataSetId, e.ToString(), cancellationToken);
        }
        finally
        {
            transactionManager.Commit();
        }

        logger.LogInformation("DataSetStatistics Id={DataSetId} AvgRelationsCount updated", dataSetCreatedEvent.DataSetId);
    }
}