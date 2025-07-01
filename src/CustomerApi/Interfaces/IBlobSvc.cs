using CustomerCustomerApi.Models.Site;

namespace CustomerCustomerApi.Interfaces;

public interface IBlobSvc
{
    Task<string> Upload(string blobName, BinaryData binaryData, bool overwrite,
        IDictionary<string, string>? tags = null, CancellationToken cancellationToken = default);
    Task<IDictionary<string, string>?> GetTags(string blobName, CancellationToken cancellationToken = default);
    Task<IDictionary<string, string>?> GetBackupTags(string blobName, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string filePath);
    Task<bool> ExistsBackupAsync(string filePath);

    Task MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    Task RevertAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath);
}
