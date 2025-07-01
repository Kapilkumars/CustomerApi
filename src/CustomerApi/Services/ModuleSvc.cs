using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Module;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;

namespace CustomerCustomerApi.Services
{
    public class ModuleSvc : IModuleSvc
    {
        private readonly IMapper _mapper;
        private readonly IRepository<ModuleCosmosDb> _moduleCosmosRepository;

        public ModuleSvc(IMapper mapper, IRepository<ModuleCosmosDb> moduleCosmosRepository)
        {
            _mapper = mapper;
            _moduleCosmosRepository = moduleCosmosRepository;
        }
        public async Task<ModuleResponse> CreateModuleAsync(ModuleModel module, CancellationToken cancellationToken)
        {
            try
            {
                var moduleItem = await _moduleCosmosRepository.CreateAsync(_mapper.Map<ModuleCosmosDb>(module), cancellationToken);
                return _mapper.Map<ModuleResponse>(moduleItem);
            }
            catch (CosmosException ex)
            {
                throw new ServiceException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (Exception ex)
            {
                throw new ServiceException("Not cosmos related exception, see inner exception!", ex);
            }
        }

        public async Task<List<ModuleResponse>> GetAllModulesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var moduleItems = await _moduleCosmosRepository.GetAsync(x => true, cancellationToken);
                return _mapper.Map<List<ModuleResponse>>(moduleItems);
            }
            catch (CosmosException ex)
            {
                throw new ServiceException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (Exception ex)
            {
                throw new ServiceException("Not cosmos related exception, see inner exception!", ex);
            }
        }

        public async Task<ModuleResponse> GetModuleAsync(string moduleId, CancellationToken cancellationToken)
        {
            try
            {
                var moduleItem = await _moduleCosmosRepository.GetAsync(id:moduleId, cancellationToken: cancellationToken);
                return _mapper.Map<ModuleResponse>(moduleItem);
            }
            catch (CosmosException ex)
            {
                throw new ServiceException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (Exception ex)
            {
                throw new ServiceException("Not cosmos related exception, see inner exception!", ex);
            }
        }

        public async Task<ModuleResponse> UpdateModuleAsync(string moduleId, ModuleModel module, CancellationToken cancellationToken)
        {
            try
            {
                var moduleItem = await _moduleCosmosRepository.GetAsync(id: moduleId, cancellationToken: cancellationToken);

                moduleItem.Update(module.Name, module.Description, module.Cost, module.IsSubscription);
                var updatedItem = await _moduleCosmosRepository.UpdateAsync(moduleItem, cancellationToken: cancellationToken);

                return _mapper.Map<ModuleResponse>(updatedItem);
            }
            catch (CosmosException ex)
            {
                throw new ServiceException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (Exception ex)
            {
                throw new ServiceException("Not cosmos related exception, see inner exception!", ex);
            }
        }

        public async Task RemoveModuleAsync(string moduleId, CancellationToken cancellationToken)
        {
            try
            {
                var moduleItem = await _moduleCosmosRepository.GetAsync(id: moduleId, cancellationToken: cancellationToken);
                if (moduleItem is null)
                    throw new NotFoundExeption($"The module does not exist.");

                moduleItem.ToggleIsDeleted();
                await _moduleCosmosRepository.UpdateAsync(moduleItem, cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                throw new ServiceException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (ServiceException ex)
            {
                throw new ServiceException("Error during deleting module, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new ServiceException("Not cosmos related exception, see inner exception!", ex);
            }
        }  
    }
}
