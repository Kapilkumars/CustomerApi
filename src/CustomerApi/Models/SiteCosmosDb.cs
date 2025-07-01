using Customer.Metis.Common.Space;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Attributes;

namespace CustomerCustomerApi.Models;

[PartitionKeyPath("/id")]
public class SiteCosmosDb : Item, ISiteCosmosDb
{
    public string Name { get; set; }
    public List<SiteCosmosDb> ChildSpaces { get; set; }
    public GeometryBase? Geometry { get; set; }
    public int? ActiveScene { get; set; }
    public object? Schedule { get; set; }
    public string? Organization { get; set; }
    public string? Address { get; set; }
    public string? BuildingType { get; set; }
    public string? Type { get; set; }
    public int? Index { get; set; }
    public string? ExchangeName { get; set; }
    public string? ExchangeId { get; set; }
}
