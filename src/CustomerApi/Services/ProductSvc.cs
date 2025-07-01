using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Product;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;

namespace CustomerCustomerApi.Services
{
    public class ProductSvc : IProductSvc
    {
        private readonly IMapper _mapper;
        private readonly IRepository<ProductCosmosDb> _productCosmosRepository;
        private readonly IRepository<ModuleCosmosDb> _moduleCosmosRepository;

        public ProductSvc(IMapper mapper,
                          IRepository<ProductCosmosDb> productCosmosRepository,
                          IRepository<ModuleCosmosDb> moduleCosmosRepository)
        {
            _mapper = mapper;
            _productCosmosRepository = productCosmosRepository;
            _moduleCosmosRepository = moduleCosmosRepository;
        }

        public async Task<ProductResponse> CreateProductAsync(ProductModel product, CancellationToken cancellationToken)
        {
            try
            {
                var productItem = _mapper.Map<ProductCosmosDb>(product);
                productItem.Skus = new List<Skus>();
                foreach (var sku in product.Skus)
                {
                    var modules = new List<ModuleCosmosDb>();
                    foreach (var id in sku.ModuleIds)
                    {
                        var module = await _moduleCosmosRepository.GetAsync(x => x.Id == id, cancellationToken: cancellationToken);
                        if (!module.Any())
                            throw new NotFoundExeption($"Module with id : {id} not exist");
                        modules.Add(module.First());
                    }

                    productItem.Skus.Add(new Skus
                    {
                        Sku = sku.Sku,
                        Name = sku.Name,
                        Modules = modules.ToList(),
                    });
                }

                var productResult = await _productCosmosRepository.CreateAsync(productItem, cancellationToken);

                return _mapper.Map<ProductResponse>(productResult);
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
                throw new ServiceException("Error during creating product, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new ServiceException("Not cosmos related exception, see inner exception!", ex);
            }
        }

        public async Task<List<ProductResponse>> GetAllProductsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var roleItems = await _productCosmosRepository.GetAsync(x => true, cancellationToken);
                return _mapper.Map<List<ProductResponse>>(roleItems);
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

        public async Task<ProductResponse> GetProductByIAsync(string productId, CancellationToken cancellationToken)
        {
            try
            {
                var roleItem = await _productCosmosRepository.GetAsync(productId, cancellationToken: cancellationToken);
                return _mapper.Map<ProductResponse>(roleItem);
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
                throw new ServiceException("Error during getting product, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new ServiceException("Not cosmos related exception, see inner exception!", ex);
            }
        }

        public async Task RemoveProductAsync(string productId, CancellationToken cancellationToken)
        {
            try
            {
                var roleItem = await _productCosmosRepository.GetAsync(productId, cancellationToken: cancellationToken);
                await _productCosmosRepository.DeleteAsync(roleItem, cancellationToken);
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

        public async Task<ProductResponse> UpdateProductAsync(string productId, ProductModel product, CancellationToken cancellationToken)
        {
            try
            {
                var productItem = await _productCosmosRepository.GetAsync(productId, cancellationToken: cancellationToken);
                productItem.Update(product.ProductName, product.ProductDescription, product.Region, product.Culture);
                productItem.Skus.Clear();
                foreach (var sku in product.Skus)
                {
                    var modules = new List<ModuleCosmosDb>();
                    foreach (var id in sku.ModuleIds)
                    {
                        var module = await _moduleCosmosRepository.GetAsync(x => x.Id == id, cancellationToken: cancellationToken);
                        if (!module.Any())
                            throw new NotFoundExeption($"Module with id : {id} not exist");
                        modules.Add(module.First());
                    }   

                    productItem.Skus.Add(new Skus
                    {
                        Sku = sku.Sku,
                        Name = sku.Name,
                        Modules = modules.ToList(),
                    });
                }

                var productResult = await _productCosmosRepository.UpdateAsync(productItem, cancellationToken: cancellationToken);

                return _mapper.Map<ProductResponse>(productResult);
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
                throw new ServiceException("Error during updating product, see inner exception!", ex)
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
