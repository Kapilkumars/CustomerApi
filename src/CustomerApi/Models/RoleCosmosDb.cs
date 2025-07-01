using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Attributes;

namespace CustomerCustomerApi.Models;

[PartitionKeyPath("/id")]
public class RoleCosmosDb : Item
{
    public RoleProperties Properties { get; set; }
    public RolePermision Permissions { get; set; }
    public string Name { get; set; }

    public void Update(RoleProperties properties)
    {
        Properties = properties;
    }
}

public class RoleProperties
{
    public string Type { get; set; }
    public string Description { get; set; }
    public string DisplayName { get; set; }
}

public class RolePermision
{
    public List<ActionInfo> UiActions { get; set; }
    public List<ActionInfo> DataActions { get; set; }
}

public class ActionInfo
{
    public RbacResourceCosmosDb Resource { get; set; }
    public List<RbacActionCosmosDb> Actions { get; set; }
}