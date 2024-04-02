using SocialNetworkAnalyzer.Data.Abstraction;

namespace SocialNetworkAnalyzer.Data.Model.Database;

/// <summary>
/// DataSet DB model
/// </summary>
public class DataSet : IDbModel
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public DateTime Created { get; init; }

    public List<Relationship> Relationships { get; init; } = [];
}