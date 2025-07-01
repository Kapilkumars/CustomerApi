using CustomerCustomerApi.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerCustomerFunctions.Functions.Module
{
    public class UpdateModulesInProducts
    {
        private readonly IRepository<ProductCosmosDb> _productRepository;
        private readonly IRepository<ModuleCosmosDb> _moduleRepository;
        private readonly IRepository<CustomerCosmosDb> _customerRepository;

        public UpdateModulesInProducts(IRepository<ProductCosmosDb> productRepository,
                                       IRepository<ModuleCosmosDb> moduleRepository,
                                       IRepository<CustomerCosmosDb> customerRepository)
        {
            _productRepository = productRepository;
            _moduleRepository = moduleRepository;
            _customerRepository = customerRepository;
        }

        [FunctionName("UpdateModulesInProducts")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "metis-customers",
            collectionName: "modules",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            ILogger log)
        {
            try
            {
                log.LogInformation("UpdateModulesInProducts function started!");
                if (input != null && input.Count > 0)
                {
                    foreach (var item in input)
                    {
                        var moduleItem = JsonConvert.DeserializeObject<ModuleCosmosDb>(item.ToString());
                        if (moduleItem.IsDeleted)
                        {
                            log.LogInformation("Start removing modules with id :" + item.Id);
                            await RemoveModulesFromProductsAsync(moduleItem, log);
                            await RemoveModulesFromCustomerAsync(moduleItem, log);
                        }
                        else
                        {
                            log.LogInformation("Start update modules in products");
                            await UpdateModulesInProductsAsync(moduleItem, log);
                            await UpdateModulesInCustomersAsync(moduleItem, log);
                        }
                        log.LogInformation("First document Id " + moduleItem.Id + " successfully updated");
                    }
                }
            }
            catch (CosmosException ex)
            {
                log.LogInformation(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }
        }

        public async Task UpdateModulesInCustomersAsync(ModuleCosmosDb moduleItem, ILogger logger)
        {
            var customers = await _customerRepository.GetByQueryAsync($"SELECT * FROM c WHERE c.tenants[0].subscriptions[0].entitlemets[0].module.id = '{moduleItem.Id}'");
            if (customers.Any())
            {
                foreach (var customer in customers)
                {
                    foreach (var tenant in customer.Tenants)
                    {
                        foreach (var sub in tenant.Subscriptions)
                        {
                            foreach (var entitlemet in sub.Entitlemets)
                            {
                                entitlemet.Module.Update(moduleItem.Name, moduleItem.Description, moduleItem.Cost, moduleItem.IsSubscription);
                            }
                        }
                    }
                }

                await _customerRepository.UpdateAsync(customers);
                logger.LogInformation("Modified " + customers.Count() + " customer items");
            }
        }

        public async Task RemoveModulesFromCustomerAsync(ModuleCosmosDb moduleItem, ILogger logger)
        {
            var customers = await _customerRepository.GetByQueryAsync($"SELECT * FROM c WHERE c.tenants[0].subscriptions[0].entitlemets[0].module.id = '{moduleItem.Id}'");
            if (customers.Any())
            {
                foreach (var customer in customers)
                {
                    foreach (var tenant in customer.Tenants)
                    {
                        foreach (var sub in tenant.Subscriptions)
                        {
                            foreach (var entitlemet in sub.Entitlemets)
                            {
                                entitlemet.Module = null;
                            }
                        }
                    }
                }

                await _customerRepository.UpdateAsync(customers);
                logger.LogInformation("Modified " + customers.Count() + " customer items");
            }
        }

        public async Task UpdateModulesInProductsAsync(ModuleCosmosDb moduleItem, ILogger logger)
        {
            var products = await _productRepository.GetAsync(x => x.Skus.Any(s => s.Modules.Any(m => m.Id == moduleItem.Id)));

            if (products.Count() > 0)
            {
                foreach (var product in products)
                {
                    foreach (var sku in product.Skus)
                    {
                        foreach (var module in sku.Modules)
                        {
                            if (module.Id == moduleItem.Id)
                            {
                                module.Update(moduleItem.Name, moduleItem.Description, moduleItem.Cost, moduleItem.IsSubscription);
                                logger.LogInformation("Modified module in product with Id " + product.Id);
                            }
                        }
                    }
                }
                await _productRepository.UpdateAsync(products);
            }

            logger.LogInformation("Modified " + products.Count() + " product items");
        }

        public async Task RemoveModulesFromProductsAsync(ModuleCosmosDb moduleItem, ILogger logger)
        {
            await _moduleRepository.DeleteAsync(moduleItem);
            logger.LogInformation("Module with Id " + moduleItem.Id + " successfully deleted");

            var productItems = await _productRepository.GetAsync(x => x.Skus.Any(s => s.Modules.Any(m => m.Id == moduleItem.Id)));
            if (productItems.Count() > 0)
            {
                var products = productItems.ToList();

                for (int i = products.Count - 1; i >= 0; i--)
                {
                    var product = products[i];
                    for (int j = product.Skus.Count - 1; j >= 0; j--)
                    {
                        var sku = product.Skus[j];
                        for (int k = sku.Modules.Count - 1; k >= 0; k--)
                        {
                            var module = sku.Modules[k];
                            if (module.Id == moduleItem.Id)
                            {
                                sku.Modules.RemoveAt(k);
                            }
                        }
                    }
                }
                await _productRepository.UpdateAsync(products);
            }
            logger.LogInformation("Modified " + productItems.Count() + " product items");
        }
    }
}
