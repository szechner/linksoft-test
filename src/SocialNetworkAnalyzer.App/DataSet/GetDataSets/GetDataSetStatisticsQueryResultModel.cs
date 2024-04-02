using System.Text.Json.Serialization;
using SocialNetworkAnalyzer.App.Abstractions;
using SocialNetworkAnalyzer.Data.Model.Database;

namespace SocialNetworkAnalyzer.App.DataSet.GetDataSets;

/// <summary>
/// Output model for data set statistics
/// </summary>
public sealed record GetDataSetStatisticsQueryResultModel : IPagedQueryResultModel
{
    /// <summary>
    /// Output model for data set statistics
    /// </summary>
    [JsonConstructor]
    public GetDataSetStatisticsQueryResultModel(int dataSetId,
        string name,
        int? nodesCount,
        int? relationshipsCount,
        double? avgRelationsCount,
        double? avgGroupRelationsCount,
        string? error,
        DataSetStatisticsState state)
    {
        this.DataSetId = dataSetId;
        this.Name = name;
        this.NodesCount = nodesCount;
        this.RelationshipsCount = relationshipsCount;
        this.AvgRelationsCount = avgRelationsCount;
        this.AvgGroupRelationsCount = avgGroupRelationsCount;
        this.Error = error;
        this.State = state;
    }

    [JsonPropertyName("dataSetId")]
    public int DataSetId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("nodesCount")]
    public int? NodesCount { get; set; }
    
    [JsonPropertyName("relationshipsCount")]
    public int? RelationshipsCount { get; set; }
    
    [JsonPropertyName("avgRelationsCount")]
    public double? AvgRelationsCount { get; set; }
    
    [JsonPropertyName("avgGroupRelationsCount")]
    public double? AvgGroupRelationsCount { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("state")]
    public DataSetStatisticsState State { get; set; }

    [JsonPropertyName("stateString")]
    public string StateString => State.ToString("G");
}