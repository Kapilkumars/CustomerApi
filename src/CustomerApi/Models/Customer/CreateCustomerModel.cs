namespace CustomerCustomerApi.Models.Customer
{
    public class CreateCustomerModel
    {
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public AddressModel CustomerAddress { get; set; }
        public string Type { get; set; }
    }
}
