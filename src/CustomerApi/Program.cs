using Customer.Metis.Logging.Correlation;
using Customer.Metis.Logging.Enrichers;
using Customer.Metis.SettingsProviders;
using Customer.Metis.SettingsProviders.Interfaces;
using CustomerCustomerApi.Extensions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Services;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using SendGrid.Extensions.DependencyInjection;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using SumoLogic.Logging.Serilog.Extensions;
using Swashbuckle.AspNetCore.Filters;
using System.Globalization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

string environmentName = "Local";
switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
{
    case "Developement":
        environmentName = "Dev";
        break;
    case "Local":
        environmentName = "Local";
        break;
    case "PRODUCTION":
        environmentName = "Prod";
        break;
}

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

//Settings providers setup
var jsonSettingsProvider = new JsonSettingsProvider(new[] { $"AppSettings/appsettings.{environmentName}.json" }, null, false);
var _aggregateSettingsProvider = new AggregateSettingsProvider();

var azureKeyVaultUrl = jsonSettingsProvider.GetSetting("AzureKeyVaultUrl");
var tenant = jsonSettingsProvider.GetSetting("AZURE_TENANT_ID");
var appClient = jsonSettingsProvider.GetSetting("AZURE_CLIENT_ID");
var secret = jsonSettingsProvider.GetSetting("AZURE_CLIENT_SECRET");

var kvProvider = new AzureKeyVaultSettingsProvider(azureKeyVaultUrl, tenant, appClient, secret);

_aggregateSettingsProvider.AddProvider(jsonSettingsProvider, 2);
_aggregateSettingsProvider.AddProvider(kvProvider, 1);

builder.Services.AddSingleton<ISettingsProvider>(_aggregateSettingsProvider);
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddScoped<IBlobSvc>(b => new BlobSvc(
    new BlobSvcOptions("sites",
        _aggregateSettingsProvider.GetSetting("BlobMetisSiteDataConnection"), "sites-backup"),
    b.GetService<ILogger<BlobSvc>>()));

//Set logging
//https://github.com/serilog/serilog-aspnetcore

var sumoEndPoint = _aggregateSettingsProvider.GetSetting("CustomerAPISumoHttpEndpoint") ?? "";
var sumoSource = _aggregateSettingsProvider.GetSetting("CustomerAPISumoSourceName");
var sumoSourceCategory = _aggregateSettingsProvider.GetSetting("CustomerAPISumoSourceCategory");
builder.Services.AddHttpContextAccessor();
builder.Host.ConfigureLogging(loggingBuilder =>
{
    loggingBuilder.Configure(options =>
    {
        options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId | ActivityTrackingOptions.ParentId;
    });
});
builder.Host.UseSerilog((context, services, configuration) => configuration
                    .MinimumLevel.Debug()//enable this using a flag
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.BufferedSumoLogic(
                        new Uri(sumoEndPoint),
                        sourceName: sumoSource,
                        formatter: new JsonFormatter(),
                        sourceCategory: sumoSourceCategory)
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithSpan()
                    .Enrich.CorrelationEnricher()
                    );
builder.Services.AddCorrelationIdGeneratorService();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(setupAction =>
{
    setupAction.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = "See the errors property for details.",
            Instance = context.HttpContext.Request.Path
        };
        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

        return new UnprocessableEntityObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});
builder.Services.AddSwaggerGen(c =>
{
    //  c.SwaggerDoc("UserJwt", new OpenApiInfo { Title = "UserJwt", Version = "v1" });
    c.SwaggerDoc("Users", new OpenApiInfo { Title = "Users", Version = "v1" });
    c.SwaggerDoc("Customers", new OpenApiInfo { Title = "Customers", Version = "v1" });
    c.SwaggerDoc("Sites", new OpenApiInfo { Title = "Sites", Version = "v1" });
    c.SwaggerDoc("RBAC", new OpenApiInfo { Title = "RBAC", Version = "v1" });
    c.SwaggerDoc("Modules", new OpenApiInfo { Title = "Modules", Version = "v1" });
    c.SwaggerDoc("Products", new OpenApiInfo { Title = "Products", Version = "v1" });


    // Set the comments path for the Swagger JSON and UI.
    c.ExampleFilters();
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
    c.OperationFilter<AddResponseHeadersFilter>();
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\". Visit https://metisrestclientapp.azurewebsites.net" +
            " and use your AAD credentials to login and to obtain the token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
      {
          new OpenApiSecurityScheme
          {
              Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
          },
          new string[] {}
      }
  });
    c.AddSecurityDefinition("ApiKeyHeader", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter your API Key",
        Name = "x-api-key",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
      {
          new OpenApiSecurityScheme
          {
              Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKeyHeader" }
          },
          new string[] {}
      }
  });
});

builder.Services.AddSwaggerExamples();

//Localize the App
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
         new CultureInfo("en-US")
     };
    options.DefaultRequestCulture = new RequestCulture("en-US", "en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddCosmosRepositories(_aggregateSettingsProvider.GetSetting("CosmosConnection"), "metis-customers");

// Add services to the container.

var azureAdConfig = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string>
    {
        {"AzureAdB2C:Instance", _aggregateSettingsProvider.GetSetting("Instance") },
        { "AzureAdB2C:Domain", _aggregateSettingsProvider.GetSetting("Domain")},
        { "AzureAdB2C:ClientId", _aggregateSettingsProvider.GetSetting("ClientIdCloudPortal")},
        { "AzureAdB2C:SignedOutCallbackPath", _aggregateSettingsProvider.GetSetting("SignedOutCallbackPath")},
        { "AzureAdB2C:SignUpSignInPolicyId", _aggregateSettingsProvider.GetSetting("SignUpSignInPolicyId")},
    }).Build();

builder.Services.AddAuthentication("CloudPortalB2C").AddMicrosoftIdentityWebApi(azureAdConfig, "AzureAdB2C", "CloudPortalB2C");

var azureAdConfigMetis = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string>
    {
        {"AzureAdB2CMetis:Instance", _aggregateSettingsProvider.GetSetting("Instance") },
        { "AzureAdB2CMetis:Domain", _aggregateSettingsProvider.GetSetting("Domain")},
        { "AzureAdB2CMetis:ClientId", _aggregateSettingsProvider.GetSetting("ClientIdMetis")},
        { "AzureAdB2CMetis:SignedOutCallbackPath", _aggregateSettingsProvider.GetSetting("SignedOutCallbackPath")},
        { "AzureAdB2CMetis:SignUpSignInPolicyId", _aggregateSettingsProvider.GetSetting("SignUpSignInPolicyId")},
    }).Build();

builder.Services.AddAuthentication().AddMicrosoftIdentityWebApi(azureAdConfigMetis, "AzureAdB2CMetis", "MetisB2C");
builder.Services.AddAuthentication().AddScheme<ApiKeySchemeOptions, ApiKeyHandler>("ApiKey", (options) =>
{
    options.ApiKey = _aggregateSettingsProvider.GetSetting("CustomerApiS2SApiKey") ?? options.ApiKey;
});

if (environmentName != "Prod")
    IdentityModelEventSource.ShowPII = true;

var secrets = _aggregateSettingsProvider.GetSetting(_aggregateSettingsProvider.GetSetting("AzureB2CClientSecretKVKey"));
builder.Services.AddScoped<IGraphService, GraphService>(x => new GraphService(new GraphServiceOptions()
{
    Issuer = _aggregateSettingsProvider.GetSetting("AzureB2CIssuer"),
    AzureB2CTenantId = _aggregateSettingsProvider.GetSetting("AzureB2CTenantId"),
    AzureB2CGraphAccessAppRegClientId = _aggregateSettingsProvider.GetSetting("AzureB2CGraphAccessAppRegClientId"),
    AzureB2CGraphAccessAppRegClientSecret = _aggregateSettingsProvider.GetSetting(_aggregateSettingsProvider.GetSetting("AzureB2CClientSecretKVKey"))
}, x.GetService<ILogger<GraphService>>()));


builder.Services.AddSendGrid(options =>
    options.ApiKey = _aggregateSettingsProvider.GetSetting("SendGridApiKey")
    );

//Services
builder.Services.AddScoped<IEmailSvc, EmailSvc>();
builder.Services.AddScoped<IUserSvc, UserSvc>();
builder.Services.AddScoped<IRoleSvc, RoleSvc>();
builder.Services.AddScoped<IRbacResourceSvc, RbacResourceSvc>();
builder.Services.AddScoped<IRbacActionSvc, RbacActionSvc>();
builder.Services.AddScoped<IModuleSvc, ModuleSvc>();
builder.Services.AddScoped<IProductSvc, ProductSvc>();
builder.Services.AddScoped<ICustomerSvc, CustomerSvc>();
builder.Services.AddScoped<ISiteSvc, SiteSvc>();
builder.Services.AddScoped<IAuthToUserProvider, AuthToUserProvider>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();
app.UseStaticFiles();
// Configure the HTTP request pipeline.
//{
app.UseSwagger();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.OAuthClientId("c1075366-e591-4ed9-aaaf-a2ec4c6a02ad");//TODO: do we need this
    c.OAuthUsePkce();
    c.SwaggerEndpoint("./Users/swagger.json", "Users");
    c.SwaggerEndpoint("./Customers/swagger.json", "Customers");
    c.SwaggerEndpoint("./Sites/swagger.json", "Sites");
    c.SwaggerEndpoint("./RBAC/swagger.json", "RBAC");
    c.SwaggerEndpoint("./Modules/swagger.json", "Modules");
    c.SwaggerEndpoint("./Products/swagger.json", "Products");
    c.InjectStylesheet("./../swagger-ui/swaggerCustomFlat.css");
});
//} 
app.UseCors("corsapp");

//app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (environmentName == "Local")
{
    app.UseDeveloperExceptionPage();
    app.MapControllers().AllowAnonymous();
}
else
{
    app.MapControllers();
}

app.Run();