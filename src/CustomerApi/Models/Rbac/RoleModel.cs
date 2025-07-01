namespace CustomerCustomerApi.Models.Rbac;
public class RoleModel
{
    public string Name { get; set; }
    public Properties Properties { get; set; }
    public Permission Permission { get; set; }
}

public class Properties
{
    public string Type { get; set; }
    public string Description { get; set; }
    public string DisplayName { get; set; }
}
public class Permission
{
    public List<ActionInfo> UiActions { get; set; }
    public List<ActionInfo> DataActions { get; set; }
}

public class ActionInfo
{
    public string ResourceId { get; set; }
    public List<string> ActionIds { get; set; }
}

public class RoleResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Properties Properties { get; set; }
    public PermissionResponse Permissions { get; set; }
}
public class PermissionResponse
{
    public List<ActionInfoResponse> UiActions { get; set; }
    public List<ActionInfoResponse> DataActions { get; set; }
}

public class ActionInfoResponse
{
    public RbacResourceResponse Resource{ get; set; }
    public List<RbacActionResponse> Actions { get; set; }
}