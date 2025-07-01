using CommonModels.Enum;
using System.ComponentModel.DataAnnotations;

namespace CustomerCustomerApi.Models.Rbac;

public class RbacActionModel
{
    [Required]
    public ActionCategory Category { get; set; }
    [Required]
    public string Action { get; set; }
    [Required]
    [StringLength(100)]
    public string Description { get; set; }
}

public class RbacActionResponse
{
    public string Id { get; set; }
    public string Category { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
}
