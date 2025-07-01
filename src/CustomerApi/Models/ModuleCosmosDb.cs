using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Attributes;

namespace CustomerCustomerApi.Models
{
    [PartitionKeyPath("/id")]
    public class ModuleCosmosDb : Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        public bool IsSubscription { get; set; }
        public bool IsDeleted { get; set; }

        public void Update(string name, string description, double cost, bool isSubscription)
        {
            Name = name;
            Description = description;
            Cost = cost;
            IsSubscription = isSubscription;
        }

        public void ToggleIsDeleted()
        {
            if (IsDeleted)
                return;
            IsDeleted = !IsDeleted;
        }
    }
}
