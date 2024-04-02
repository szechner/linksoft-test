using System.Collections.Immutable;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.App.Abstractions;
using SocialNetworkAnalyzer.App.Events;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Core.Guards;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.Repositories;

namespace SocialNetworkAnalyzer.App.DataSet.CreateDataSet;

/// <summary>
/// Logic to handle the <see cref="CreateDataSetCommand"/>
/// </summary>
public sealed class CreateDataSetCommandHandler(IDataSetsRepository dataSetsRepository, IUsersRepository usersRepository, IRelationshipsRepository relationshipsRepository, ITransactionManager transactionManager, IPublisher mediator, IDataSetStatisticsRepository datasetStatisticsRepository, ILogger<CreateDataSetCommandHandler> logger)
    : ICommandRequestHandler<CreateDataSetCommand, DataSetCreatedEvent>
{
    private const char separator = ' ';

    public async Task<DataSetCreatedEvent> Handle(CreateDataSetCommand request, CancellationToken cancellationToken)
    {
        using var __ = Measurement.ElapsedTime(this);
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogDebug("Handling CreateDataSetCommand");

        var command = Guard.Require.ArgumentNotNull(request);

        var validator = new CreateDataSetCommandValidator();

        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var dataToImport = await ProcessFile(request.TmpFilePath, cancellationToken);

        if (dataToImport.Length == 0)
        {
            logger.LogWarning("File is empty");
            throw new ValidationException(new[]
            {
                new ValidationFailure("File", "File is empty")
            });
        }

        var dataSet = await dataSetsRepository.CreateDataSet(request.Name, cancellationToken);

        _ = await datasetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var users = dataToImport.SelectMany(x => new[] { x.UserId1, x.UserId2 }).Distinct().ToImmutableArray();

        await usersRepository.Add(users, cancellationToken);

        await relationshipsRepository.Add(dataToImport, dataSet.Id, cancellationToken);

        transactionManager.Commit();

        var @event = new DataSetCreatedEvent(dataSet.Id);

        _ = mediator.Publish(@event, CancellationToken.None);

        logger.LogInformation("Created data set with id {DataSetId}", dataSet.Id);

        return @event;
    }

    private static async Task<ImmutableArray<(int UserId1, int UserId2)>> ProcessFile(string tmpFilePath, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(tmpFilePath);

        var lines = new HashSet<(int UserId1, int UserId2)>();

        var failedLines = new List<string>();

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (userId1, userId2, error) = ParseLine(line.Trim());
            if (error)
            {
                failedLines.Add($"Line {lines.Count + 1} is not in the correct format");
            }

            lines.Add((userId1, userId2));
        }

        if (failedLines.Count > 0)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("File", string.Join(Environment.NewLine, failedLines), "Expected two integers separated by a space.")
            });
        }

        return lines.ToImmutableArray();
    }

    private static (int UserId1, int UserId2, bool error) ParseLine(string line)
    {
        var parts = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2 || !int.TryParse(parts[0], out var userId1) || !int.TryParse(parts[1], out var userId2))
        {
            return (0, 0, true);
        }

        return (userId1, userId2, false);
    }
}