namespace CustomerCustomerApi.Models.Product
{
    public class ProductModel
    {
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string Region { get; set; }
        public string Culture { get; set; }
        public List<SkusModel> Skus { get; set; }
    }
}
