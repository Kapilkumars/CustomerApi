using Azure.Core;

namespace CustomerCustomerApi.Services;

public class BlobSvcOptions
{
    public string PrimaryContainerName { get; set; }
    public string BackupContainerName { get; set; }
    public string ConnectionString { get; set; }
    public RetryOptions Retry { get; set; }

    public BlobSvcOptions(string primarycontainerName, string connectionString, string? backupContainerName = null)
    {
        PrimaryContainerName = primarycontainerName;
        BackupContainerName = backupContainerName;
        ConnectionString = connectionString;
        Retry = new RetryOptions();
    }
}

public class RetryOptions
{
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(2);
    public int MaxRetries { get; set; } = 5;
    public RetryMode Mode { get; set; } = RetryMode.Exponential;
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromSeconds(100);
}