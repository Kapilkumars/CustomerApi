using System.ComponentModel.DataAnnotations;

namespace CustomerCustomerApi.Models.Rbac;

public class RbacResourceModel
{
    [Required]
    [StringLength(30)]
    public string ResourceName { get; set; }
    [Required]
    [StringLength(100)]
    public string Description { get; set; }
}

public class RbacResourceResponse
{
    public string Id { get; set; }
    public string ResourceName { get; set; }
    public string Description { get; set; }
}
