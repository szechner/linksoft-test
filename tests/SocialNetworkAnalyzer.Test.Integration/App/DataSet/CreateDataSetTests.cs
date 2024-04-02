using FakeItEasy;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.App.DataSet.CreateDataSet;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;
using SocialNetworkAnalyzer.Data.Repositories;
using SocialNetworkAnalyzer.Test.Integration.EntityFramework.Extensions;

namespace SocialNetworkAnalyzer.Test.Integration.App.DataSet;

[TestFixture]
public class CreateDataSetTests
{
    private ServiceProvider serviceProvider = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var services = RepositoryTestsExtensions.CreateServiceCollection(nameof(CreateDataSetTests).ToLower());

        serviceProvider = services.BuildServiceProvider();
        StaticLogger.Initialize(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.DataSet), serviceProvider, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.DataSetStatistics), serviceProvider, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.Relationship), serviceProvider, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.User), serviceProvider, CancellationToken.None);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        var context = serviceProvider.GetRequiredService<SocialMappingContext>();
        context.Database.EnsureDeleted();
        serviceProvider.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Create_DataSet_With_File(CancellationToken cancellationToken)
    {
        var tmpFileName = CreateImportFile();
        var scope = serviceProvider.CreateScope();

        var dataSetRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CreateDataSetCommandHandler>>();

        var mediatr = A.Fake<IPublisher>();

        var handler = new CreateDataSetCommandHandler(dataSetRepository, usersRepository, relationshipsRepository, transactionManager, mediatr, dataSetStatisticsRepository, logger);

        var result = await handler.Handle(new CreateDataSetCommand(nameof(Create_DataSet_With_File), tmpFileName), cancellationToken);

        result.Should().NotBeNull();
        
        File.Delete(tmpFileName);
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Create_DataSet_Validation_With_Empty_File(CancellationToken cancellationToken)
    {
        var tmpFileName = CreateImportFile(0);
        var scope = serviceProvider.CreateScope();

        var dataSetRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CreateDataSetCommandHandler>>();

        var mediatr = A.Fake<IPublisher>();

        var handler = new CreateDataSetCommandHandler(dataSetRepository, usersRepository, relationshipsRepository, transactionManager, mediatr, dataSetStatisticsRepository, logger);

        Func<Task> result = () => handler.Handle(new CreateDataSetCommand(nameof(Create_DataSet_With_File), tmpFileName), cancellationToken);

        await result.Should().ThrowAsync<ValidationException>().Where(e =>
            e.Message.Contains("File is empty") &&
            e.Errors.Any(er => er.PropertyName == "File")
        );
        
        File.Delete(tmpFileName);
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Create_DataSet_Validation_With_Wrong_File(CancellationToken cancellationToken)
    {
        var tmpFileName = CreateImportFile(10, "-");
        var scope = serviceProvider.CreateScope();

        var dataSetRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CreateDataSetCommandHandler>>();

        var mediatr = A.Fake<IPublisher>();

        var handler = new CreateDataSetCommandHandler(dataSetRepository, usersRepository, relationshipsRepository, transactionManager, mediatr, dataSetStatisticsRepository, logger);

        Func<Task> result = () => handler.Handle(new CreateDataSetCommand(nameof(Create_DataSet_With_File), tmpFileName), cancellationToken);

        await result.Should().ThrowAsync<ValidationException>().Where(e =>
            e.Message.Contains("is not in the correct format") &&
            e.Errors.Any(er => er.PropertyName == "File")
        );
        
        File.Delete(tmpFileName);
    }

    private string CreateImportFile(int rows = 100, string separator = " ")
    {
        var buffer = new List<(int UserId1, int UserId2)>();

        for (var i = 0; i < rows; i++)
        {
            var userId1 = Random.Shared.Next(10, 100);
            var userId2 = Random.Shared.Next(10, 100);

            if (userId1 == userId2)
            {
                userId2++;
            }

            buffer.Add((userId1, userId2));
        }

        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));

        using var tmpFile = File.CreateText(fileName);
        foreach (var (userId1, userId2) in buffer)
        {
            tmpFile.WriteLine($"{userId1}{separator}{userId2}");
        }

        return fileName;
    }
}