using SocialNetworkAnalyzer.Data.Abstraction;

namespace SocialNetworkAnalyzer.Data.Model.Database;

/// <summary>
/// User DB model
/// </summary>
public class User : IDbModel
{
    public required int Id { get; init; }

    public List<Relationship> Relationships1 { get; init; } = [];

    public List<Relationship> Relationships2 { get; init; } = [];

    public List<User> Friends => Relationships1.Select(p => p.User2).Concat(Relationships2.Select(t => t.User1)).DistinctBy(t => t.Id).ToList();
}