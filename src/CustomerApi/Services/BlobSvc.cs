using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;

namespace CustomerCustomerApi.Services;

public class BlobSvc : IBlobSvc
{
    private BlobContainerClient _primarycontainer;
    private BlobContainerClient _backupContainer;
    private ILogger<BlobSvc> _logger;
    private BlobSvcOptions _options;

    public BlobSvc(BlobSvcOptions options, ILogger<BlobSvc> logger, BlobContainerClient? primaryContainer = null, BlobContainerClient? backupContainer = null)
    {
        _options = options;
        _logger = logger;

        _primarycontainer = primaryContainer ?? new BlobContainerClient(_options.ConnectionString, options.PrimaryContainerName, new BlobClientOptions
        {
            Retry = {
             Delay = _options.Retry.Delay,
             MaxRetries = _options.Retry.MaxRetries,
             Mode = _options.Retry.Mode,
             MaxDelay = _options.Retry.MaxDelay,
             NetworkTimeout = _options.Retry.NetworkTimeout
         }
        });
        _backupContainer = backupContainer ?? new BlobContainerClient(_options.ConnectionString, options.BackupContainerName, new BlobClientOptions
        {
            Retry = {
             Delay = _options.Retry.Delay,
             MaxRetries = _options.Retry.MaxRetries,
             Mode = _options.Retry.Mode,
             MaxDelay = _options.Retry.MaxDelay,
             NetworkTimeout = _options.Retry.NetworkTimeout
         }
        });

        _primarycontainer.CreateIfNotExists();
        _backupContainer.CreateIfNotExists();
    }

    public async Task<string> Upload(string blobName, BinaryData binaryData, bool overwrite, IDictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var blob = _primarycontainer.GetBlobClient(blobName);
            await blob.UploadAsync(binaryData, overwrite, cancellationToken);
            if (tags != null)
                await blob.SetTagsAsync(tags, cancellationToken: cancellationToken);

            return blob.Uri.AbsoluteUri;
        }   
        catch (Exception e)
        {
            throw new BlobSvcException(_logger, $"Could not upload the file: {blobName}", e);
        }
    }

    public async Task<IDictionary<string, string>?> GetTags(string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            var blob = _primarycontainer.GetBlockBlobClient(blobName);
            var tags = await blob.GetTagsAsync(cancellationToken: cancellationToken);
            return tags.Value.Tags;
        }
        catch (Exception e)
        {
            _logger.LogError("Could not retrieve the tags for blob {BlobName}", blobName);
            return null;
        }
    }

    public async Task<IDictionary<string, string>?> GetBackupTags(string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            var blob = _backupContainer.GetBlobClient(blobName);
            var tags = await blob.GetTagsAsync(cancellationToken: cancellationToken);
            return tags.Value.Tags;
        }
        catch (Exception e)
        {
            _logger.LogError("Could not retrieve the tags for blob {BlobName}", blobName);
            return null;
        }
    }

    public async Task<bool> ExistsAsync(string filePath)
    {
        BlobClient blobClient = _primarycontainer.GetBlobClient(filePath);
        return await blobClient.ExistsAsync();
    }

    public async Task<bool> ExistsBackupAsync(string filePath)
    {
        BlobClient blobClient = _backupContainer.GetBlobClient(filePath);
        return await blobClient.ExistsAsync();
    }

    public async Task MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            BlockBlobClient destBlobClient = _backupContainer.GetBlockBlobClient(destinationPath);
            BlobClient sourceBlob = _primarycontainer.GetBlobClient(sourcePath);
            Uri sasUri = sourceBlob.GenerateSasUri(
                permissions: BlobSasPermissions.Read | BlobSasPermissions.Tag,
                expiresOn: DateTimeOffset.UtcNow.AddHours(1));
            // Create options to pass during the copy operation
            var copyOptions = new BlobCopyFromUriOptions
            {
                CopySourceTagsMode = BlobCopySourceTagsMode.Copy
            };
            // Start the copy operation with tags applied
            await destBlobClient.SyncCopyFromUriAsync(sasUri, copyOptions, cancellationToken);
        }
        catch (OperationCanceledException cancelEx)
        {
            // Handle cancellation (if the operation was cancelled via the cancellation token)
            throw new BlobSvcException(_logger, $"Operation was canceled: {cancelEx.Message}", cancelEx);
            // Optionally rethrow or handle cancellation logic
        }
        catch (Exception ex)
        {
            // Catch any other unhandled exceptions
            throw new BlobSvcException(_logger, $"An unexpected error occurred: {ex.Message}", ex);
            // Optionally log or take additional action for other exceptions
            throw;
        }
    }

    public async Task RevertAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            BlockBlobClient sourceBlob = _primarycontainer.GetBlockBlobClient(sourcePath);
            BlobClient destinationBlob = _backupContainer.GetBlobClient(destinationPath);
            Uri sasUri = destinationBlob.GenerateSasUri(
                permissions: BlobSasPermissions.Read | BlobSasPermissions.Tag,
                expiresOn: DateTimeOffset.UtcNow.AddHours(1));
            // Retrieve existing tags from site-backup container
            var existingTags = await GetBackupTags(sourcePath, cancellationToken) ?? new Dictionary<string, string>();

            // Create options to pass during the copy operation
            var copyOptions = new BlobCopyFromUriOptions
            {
                CopySourceTagsMode = BlobCopySourceTagsMode.Copy
            };

            // Start the copy operation with tags applied
            var copyOp = await sourceBlob.SyncCopyFromUriAsync(sasUri, copyOptions, cancellationToken);
        }
        catch (OperationCanceledException cancelEx)
        {
            // Handle cancellation (if the operation was cancelled via the cancellation token)
            throw new BlobSvcException(_logger, $"Operation was canceled: {cancelEx.Message}", cancelEx);
            // Optionally rethrow or handle cancellation logic
        }
        catch (Exception ex)
        {
            // Catch any other unhandled exceptions
            throw new BlobSvcException(_logger, $"An unexpected error occurred: {ex.Message}", ex);
            // Optionally log or take additional action for other exceptions
        }
    }

    public async Task DeleteAsync(string filePath) 
    {
        BlobClient blobClient = _backupContainer.GetBlobClient(filePath);
        await blobClient.DeleteIfExistsAsync();
    }
}