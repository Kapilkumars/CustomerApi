using CustomerCustomerApi.Models.Module;

namespace CustomerCustomerApi.Models.Product
{
    public class ProductResponse
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string Region { get; set; }
        public string Culture { get; set; }
        public List<SkusResponse> Skus { get; set; }
    }
    public class SkusResponse
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public List<ModuleResponse> Modules { get; set; }
    }
}
