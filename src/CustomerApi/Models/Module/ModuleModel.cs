using System.ComponentModel.DataAnnotations;

namespace CustomerCustomerApi.Models.Module;

public class ModuleModel
{
    [Required]
    public string Name { get; set; }
    [Required]
    [StringLength(100)]
    public string Description { get; set; }
    [Required]
    public double Cost { get; set; }
    [Required]
    public bool IsSubscription { get; set; }
}

public class ModuleResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Cost { get; set; }
    public bool IsSubscription { get; set; }
}
