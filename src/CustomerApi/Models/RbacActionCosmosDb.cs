using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Attributes;

namespace CustomerCustomerApi.Models
{
    [PartitionKeyPath("/id")]
    public class RbacActionCosmosDb : Item
    {
        public string Category { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }

        public void Update(string description, string action)
        {
            Action = action;
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
