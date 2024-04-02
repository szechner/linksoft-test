using SocialNetworkAnalyzer.App.Abstractions.Base;

namespace SocialNetworkAnalyzer.App.DataSet.GetDataSets;

/// <summary>
/// Query to get data set statistics
/// </summary>
public sealed record GetDataSetStatisticsQuery(int Page, int PageSize) : PagedQuery<GetDataSetStatisticsQueryResultModel>(Page, PageSize);