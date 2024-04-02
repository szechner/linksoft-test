using SocialNetworkAnalyzer.App.Abstractions;
using SocialNetworkAnalyzer.App.Events;

namespace SocialNetworkAnalyzer.App.DataSet.CreateDataSet;

/// <summary>
/// Command to create a new data set 
/// </summary>
public sealed record CreateDataSetCommand(string Name, string TmpFilePath) : ICommand<DataSetCreatedEvent>;