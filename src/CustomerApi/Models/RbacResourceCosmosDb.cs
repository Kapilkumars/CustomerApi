using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Attributes;

namespace CustomerCustomerApi.Models
{
    [PartitionKeyPath("/id")]
    public class RbacResourceCosmosDb : Item
    {
        public string Description { get; set; }
        public string ResourceName { get; set; }
        public bool IsDeleted { get; set; }

        public void Update(string description)
        {
            Description = description;
        }

        public void ToggleIsDeleted()
        {
            if (IsDeleted)
                return;
            IsDeleted = !IsDeleted;
        }
    }
}
