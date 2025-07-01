using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Attributes;

namespace CustomerCustomerApi.Models
{
    [PartitionKeyPath("/id")]
    public class ProductCosmosDb : Item
    {
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string Region { get; set; }
        public string Culture { get; set; }
        public List<Skus> Skus { get; set; }

        public void Update(string productName, string productDescription, string region, string culture)
        {
            ProductName = productName;
            ProductDescription = productDescription;
            Region = region;
            Culture = culture;
        }
    }

    public class Skus
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public List<ModuleCosmosDb> Modules { get; set; }
    }
}
