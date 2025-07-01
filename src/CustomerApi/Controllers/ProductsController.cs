using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models.Product;
using CustomerCustomerApi.ResponseHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace CustomerCustomerApi.Controllers
{
    [Authorize(AuthenticationSchemes = "MetisB2C,CloudPortalB2C")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails404NotFound), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ApiExplorerSettings(GroupName = "Products")]
    [Route("api/v1.0/products")]
    [ApiVersion("1.0")]
    public class ProductsController : ApiControllerBase
    {
        private readonly IProductSvc _productSvc;

        public ProductsController(IProductSvc productSvc,
                                 ILogger<ProductsController> logger, ICorrelationIdGenerator
                                 correlationIdGenerator)
                                 : base(logger, correlationIdGenerator)
        {
            _productSvc = productSvc;
        }

        /// <summary>
        /// Obtain product by id
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetProductByIdAsync([Required][FromRoute] string productId, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productSvc.GetProductByIAsync(productId, cancellationToken);
                return Ok(product);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected errors occurred.", apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while obtaining product", ex.Message, "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Obtain all products
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllProductsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var products = await _productSvc.GetAllProductsAsync(cancellationToken);
                if (products is null || !products.Any())
                    return NotFound(new ProblemDetails404NotFound("Not Found in DB", $"Ensure product list is refreshed!", HttpContext));
                else
                    return Ok(products);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected errors occurred.", apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while obtaining product", ex.Message, "Problem otaining product, contact support.");
            }
        }

        /// <summary>
        /// Create product
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateProductAsync([FromBody] ProductModel product, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _productSvc.CreateProductAsync(product, cancellationToken);
                return Ok(result);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, "Unexpected errors occurred.", apiResponseMessage: ex.Message);
            }
            catch (NotFoundExeption ex)
            {
                return ErrorProblem(ex, statusCode: ex.HttpStatusCode, apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while creating the product", ex.Message, "Problem creating the product, contact support.");
            }
        }

        /// <summary>
        /// Update product
        /// </summary>
        /// <param name="product"></param>
        /// <param name="productId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{productId}")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateProductAsync([Required][FromRoute] string productId, ProductModel product, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _productSvc.UpdateProductAsync(productId, product, cancellationToken);
                return Ok(result);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected errors occurred.", apiResponseMessage: ex.Message);
            }
            catch (NotFoundExeption ex)
            {
                return ErrorProblem(ex, statusCode: ex.HttpStatusCode, apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while updating the produc", ex.Message, "Problem updating the produc, contact support.");
            }
        }

        /// <summary>
        /// Delete product
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteProductAsync([Required][FromRoute] string productId, CancellationToken cancellationToken)
        {
            try
            {
                await _productSvc.RemoveProductAsync(productId, cancellationToken);
                return Accepted();
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected errors occurred.", apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while removing the product", ex.Message, "Unexpected errors occurred.");
            }
        }
    }
}