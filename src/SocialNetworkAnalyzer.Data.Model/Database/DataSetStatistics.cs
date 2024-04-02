using SocialNetworkAnalyzer.Data.Abstraction;

namespace SocialNetworkAnalyzer.Data.Model.Database;

/// <summary>
/// Statistics for a DataSet
/// </summary>
public class DataSetStatistics : IDbModel
{
    public DataSet DataSet { get; init; } = null!;
    public required int DataSetId { get; init; }
    public int? NodesCount { get; set; }
    public int? RelationshipsCount { get; set; }
    public double? AvgRelationsCount { get; set; }
    public double? AvgGroupRelationsCount { get; set; }

    public string? Error { get; set; }

    public required DataSetStatisticsState State { get; set; } = DataSetStatisticsState.Pending;

}

public enum DataSetStatisticsState
{
    Pending,
    Calculated,
    Error
}