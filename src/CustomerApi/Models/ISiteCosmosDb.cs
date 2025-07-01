using Customer.Metis.Common.Space;
using CustomerCustomerApi.Models;

public interface ISiteCosmosDb
{
    string Name { get; set; }
    int? ActiveScene { get; set; }
    List<SiteCosmosDb> ChildSpaces { get; set; }
    GeometryBase? Geometry { get; set; }
    object? Schedule { get; set; }
}
