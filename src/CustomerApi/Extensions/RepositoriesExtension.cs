using CustomerCustomerApi.Models;

namespace CustomerCustomerApi.Extensions
{
    public static class RepositoriesExtension
    {

        public static void AddCosmosRepositories(this IServiceCollection services, string connectionString, string databaseName)
        {
           services.AddCosmosRepository(
           options =>
           {
               options.CosmosConnectionString = connectionString;
               options.DatabaseId = databaseName;
               options.ContainerPerItemType = true;
               options.ContainerBuilder.Configure<CustomerCosmosDb>(containerOptions => containerOptions
                      .WithContainer("customers")
                      .WithPartitionKey("/id")
                      .WithoutStrictTypeChecking());
               options.ContainerBuilder.Configure<UserCosmosDb>(containerOptions => containerOptions
                        .WithContainer("users")
                        .WithPartitionKey("/id")
                        .WithoutStrictTypeChecking());
               options.ContainerBuilder.Configure<RoleCosmosDb>(containerOptions => containerOptions
                      .WithContainer("roles")
                      .WithPartitionKey("/id"));
               options.ContainerBuilder.Configure<RbacResourceCosmosDb>(containerOptions => containerOptions
                      .WithContainer("rbac-resources")
                      .WithPartitionKey("/id"));
               options.ContainerBuilder.Configure<RbacActionCosmosDb>(containerOptions => containerOptions
                      .WithContainer("rbac-actions")
                      .WithPartitionKey("/id"));
               options.ContainerBuilder.Configure<ModuleCosmosDb>(containerOptions => containerOptions
                      .WithContainer("modules")
                      .WithPartitionKey("/id"));
               options.ContainerBuilder.Configure<ProductCosmosDb>(containerOptions => containerOptions
                      .WithContainer("products")
                      .WithPartitionKey("/id"));
               options.ContainerBuilder.Configure<SiteCosmosDb>(containerOptions => containerOptions
                      .WithContainer("sites")
                      .WithPartitionKey("/id"));

           }, clientOptions =>
           {
               clientOptions.ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode.Gateway;
           });
        }
    }
}
