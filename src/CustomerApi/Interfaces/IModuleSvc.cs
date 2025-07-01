using CustomerCustomerApi.Models.Module;

namespace CustomerCustomerApi.Interfaces
{
    public interface IModuleSvc
    {
        Task<List<ModuleResponse>> GetAllModulesAsync(CancellationToken cancellationToken);
        Task<ModuleResponse> GetModuleAsync(string moduleId, CancellationToken cancellationToken);
        Task<ModuleResponse> CreateModuleAsync(ModuleModel module, CancellationToken cancellationToken);
        Task<ModuleResponse> UpdateModuleAsync(string moduleId, ModuleModel module, CancellationToken cancellationToken);
        Task RemoveModuleAsync(string moduleId, CancellationToken cancellationToken);
    }
}
