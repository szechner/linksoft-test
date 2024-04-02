using System.Text.Json.Serialization;
using SocialNetworkAnalyzer.App.Abstractions;

namespace SocialNetworkAnalyzer.App.Events;

/// <summary>
/// Event that is raised when a new data set is created 
/// </summary>
public sealed record DataSetCreatedEvent : IEvent
{
    [JsonConstructor]
    public DataSetCreatedEvent()
    {
    }

    public DataSetCreatedEvent(int dataSeId)
    {
        DataSetId = dataSeId;
    }
    
    [JsonPropertyName("dataSetId")]
    public int DataSetId { get; set; }
}