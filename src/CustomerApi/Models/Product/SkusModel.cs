namespace CustomerCustomerApi.Models.Product;

public class SkusModel
{
    public string Sku { get; set; }
    public string Name { get; set; }
    public List<string> ModuleIds { get; set; }
}
