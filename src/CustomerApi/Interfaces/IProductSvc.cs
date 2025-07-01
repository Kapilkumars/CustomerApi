using CustomerCustomerApi.Models.Product;

namespace CustomerCustomerApi.Interfaces
{
    public interface IProductSvc
    {
        Task<List<ProductResponse>> GetAllProductsAsync(CancellationToken cancellationToken);
        Task<ProductResponse> GetProductByIAsync(string productId, CancellationToken cancellationToken);
        Task<ProductResponse> CreateProductAsync(ProductModel product, CancellationToken cancellationToken);
        Task<ProductResponse> UpdateProductAsync(string productId, ProductModel product, CancellationToken cancellationToken);
        Task RemoveProductAsync(string productId, CancellationToken cancellationToken);
    }
}
