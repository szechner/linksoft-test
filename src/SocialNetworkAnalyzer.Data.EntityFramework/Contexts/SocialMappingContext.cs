using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.Data.EntityFramework.Base;
using SocialNetworkAnalyzer.Data.Model.Database;

namespace SocialNetworkAnalyzer.Data.EntityFramework.Contexts;

/// <summary>
/// EntityFramework <see cref="DbContext"/> for SocialMapping database 
/// </summary>
public class SocialMappingContext(DbContextOptions<SocialMappingContext> options, ILogger<SocialMappingContext> logger, IDbSchemaProvider dbSchemaProvider) : DbContextBase<SocialMappingContext>(options, logger, dbSchemaProvider)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dataSetEntity = modelBuilder.Entity<DataSet>();
        dataSetEntity.HasKey(p => p.Id);
        dataSetEntity.Property(p => p.Name).IsRequired().HasMaxLength(100);
        dataSetEntity.Property(p => p.Created).HasDefaultValueSql("now()");
        dataSetEntity.HasMany(p => p.Relationships).WithOne().HasForeignKey(p => p.DataSetId);

        var relationshipEntity = modelBuilder.Entity<Relationship>();
        relationshipEntity.HasKey(p => new { p.UserId1, p.UserId2, p.DataSetId });
        relationshipEntity.HasOne(p => p.DataSet).WithMany(p => p.Relationships).HasForeignKey(p => p.DataSetId);
        relationshipEntity.HasOne(p => p.User1).WithMany().HasForeignKey(p => p.UserId1);
        relationshipEntity.HasOne(p => p.User2).WithMany().HasForeignKey(p => p.UserId2);

        var userEntity = modelBuilder.Entity<User>();
        userEntity.HasIndex(p => p.Id);
        userEntity.Property(p => p.Id).ValueGeneratedNever();
        userEntity.HasMany(p => p.Relationships1).WithOne(p => p.User1).HasForeignKey(p => p.UserId1);
        userEntity.HasMany(p => p.Relationships2).WithOne(p => p.User2).HasForeignKey(p => p.UserId2);
        userEntity.Ignore(p => p.Friends);

        var datasetStatisticsEntity = modelBuilder.Entity<DataSetStatistics>();
        datasetStatisticsEntity.HasKey(p => p.DataSetId);
        datasetStatisticsEntity.HasOne(p => p.DataSet).WithOne().HasForeignKey<DataSetStatistics>(p => p.DataSetId);
        datasetStatisticsEntity.Property(p => p.NodesCount).IsRequired(false);
        datasetStatisticsEntity.Property(p => p.RelationshipsCount).IsRequired(false);
        datasetStatisticsEntity.Property(p => p.AvgRelationsCount).IsRequired(false);
        datasetStatisticsEntity.Property(p => p.AvgGroupRelationsCount).IsRequired(false);
        datasetStatisticsEntity.Property(p => p.Error).IsRequired(false);
        datasetStatisticsEntity.Property(p => p.State).HasDefaultValue(DataSetStatisticsState.Pending);
        
        Logger.LogInformation("{DbContext} Model created",nameof(SocialMappingContext));
    }
}