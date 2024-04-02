using SocialNetworkAnalyzer.Data.Abstraction;

namespace SocialNetworkAnalyzer.Data.Model.Database;

/// <summary>
/// Relationship DB model
/// </summary>
public class Relationship : IDbModel
{
    public required int UserId1 { get; init; }
    
    public required User User1 { get; init; }

    public required int UserId2 { get; init; }

    public required User User2 { get; init; }
    
    public required int DataSetId { get; init; }
    
    public required DataSet DataSet { get; init; }
}