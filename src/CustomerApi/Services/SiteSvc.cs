using AutoMapper;
using Customer.Metis.Common.Models.Responses;
using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Helpers;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Site;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Extensions;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace CustomerCustomerApi.Services;

public class SiteSvc : ISiteSvc
{
    #region Privates
    private readonly ILogger<SiteSvc> _logger;
    private readonly IRepository<SiteCosmosDb> _cosmosRepository;
    private readonly IMapper _mapper;
    private readonly ICorrelationIdGenerator _correlationIdGenerator;
    private readonly IBlobSvc _blobSvc;

    #endregion

    public SiteSvc(ILogger<SiteSvc> logger,
        IRepositoryFactory repositoryFactory,
        IMapper mapper,
        ICorrelationIdGenerator correlationIdGenerator, IBlobSvc blobSvc)
    {
        _logger = logger;
        _cosmosRepository = repositoryFactory.RepositoryOf<SiteCosmosDb>();
        _mapper = mapper;
        _correlationIdGenerator = correlationIdGenerator;
        _blobSvc = blobSvc;
    }

    public async Task<List<SiteResponseModel>> GetAllAsync(string? siteName = null)
    {
        try
        {
            var items = Enumerable.Empty<SiteCosmosDb>();
            if (!string.IsNullOrEmpty(siteName))
            {
                items = await _cosmosRepository.GetAsync(x => x.Name == siteName);
            }
            else
            {
                items = await _cosmosRepository.GetAsync(_ => true);
            }
            return _mapper.Map<List<SiteResponseModel>>(items);
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception.Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task<SiteResponseModel?> TryFindByIdAsync(string id)
    {
        try
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new Exception("Site id cant be null or empty.");
            }

            var item = (await _cosmosRepository.GetAsync(x => x.Id == id)).FirstOrDefault();

            return item is null ? null : _mapper.Map<SiteResponseModel>(item);
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception.Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task<SiteResponseModel> CreateSiteAsync(SiteModel model)
    {
        try
        {
            if (await ExistAsync(model.Name))
                throw new SiteSvcException("Site with this site name already exist")
                {
                    HttpStatusCode = HttpStatusCode.Conflict
                };

            if (model.ChildSpaces.Any(x => x.Name == model.Name))
                throw new SiteSvcException("Site name mast be unique")
                {
                    HttpStatusCode = HttpStatusCode.Conflict
                };

            // initializing model with ids for further mapping and blob uploading
            model.Id = Guid.NewGuid().ToString();
            FillSpacesEmptyIdsWithValues(model.ChildSpaces);

            var siteItem = _mapper.Map<SiteCosmosDb>(model);

            //TODO - we should not be processing any geojson before the site is created as there is no siteId and floorId
            //What is this supposed to be doing?
            //foreach (var childSpace in model.ChildSpaces)
            //    await EnrichGeoJsonWithSpaceIds(siteItem.Id, childSpace);

            var space = await _cosmosRepository.CreateAsync(siteItem);
            return _mapper.Map<SiteResponseModel>(space);
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to create site. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (SiteSvcException ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception. {ex.Message}. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.HttpStatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception. Failed to create site. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task<bool> ExistAsync(string siteName)
    {
        try
        {
            var items = await _cosmosRepository.GetAsync(x => x.Name == siteName);
            return items.Any();
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to create site. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception. Failed to create site. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task<bool> ExistAsync(string siteName, string floorName)
    {
        try
        {
            var items = await _cosmosRepository.GetAsync(x => x.Name == siteName);
            var result = items.Any(x => x.ChildSpaces.Any(c => c.Name == floorName));

            return result;
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to create site. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception. Failed to create site. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task<bool> SpaceWithFloorExistAsync(string siteId, string floorId)
    {
        try
        {
            var items = await _cosmosRepository.GetAsync(x => x.Id == siteId);
            var res = items.Any(x => x.ChildSpaces.Any(c => c.Id == floorId));

            return res;
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to create site. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception. Failed to create site. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task<SiteResponseModel> UpdateSiteAsync(SiteModel model, string id)
    {
        try
        {
            // TODO - this is wrong as we only do first level instead of recursive!!!
            // Check for duplicate names in child spaces first, before any repository calls.
            //if (model.ChildSpaces != null && model.ChildSpaces.GroupBy(cs => cs.Id).Any(g => g.Count() > 1))
            //{
            //    var exception = new SiteSvcException("Site Id must be unique among child spaces")
            //    {
            //        HttpStatusCode = HttpStatusCode.Conflict
            //    };
            //    throw exception;
            //}
            FillSpacesEmptyIdsWithValues(model.ChildSpaces);
            var siteItem = await _cosmosRepository.GetAsync(id);
            if (siteItem == null)
            {
                throw new SiteSvcException("Site not found")
                {
                    HttpStatusCode = HttpStatusCode.NotFound
                };
            }
            siteItem.ActiveScene = model.ActiveScene;
            siteItem.Schedule = model.Schedule;
            siteItem.Geometry = model.Geometry;
            siteItem.ChildSpaces = model.ChildSpaces != null ? _mapper.Map<List<SiteCosmosDb>>(model.ChildSpaces) : new List<SiteCosmosDb>();

            var space = await _cosmosRepository.UpdateAsync(siteItem);
            return _mapper.Map<SiteResponseModel>(space);
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos DB error. Failed to update site. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (Exception ex)
        {
            if (ex is SiteSvcException)
                throw;

            _logger.LogError(ex, $"Unexpected error during update for site {id}");
            try
            {
                await HandleUpdateException(model, id, ex);
            }
            catch (SiteSvcException svcEx)
            {
                _logger.LogError(svcEx, $"Rollback process threw SiteSvcException for site {id}");
            }
            throw new SiteSvcException(_logger, $"Update failed for site {id}. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    private async Task HandleUpdateException(SiteModel model, string id, Exception ex)
    {
        try
        {
            _logger.LogError(ex, $"Update failed, attempting rollback for site {id}");

            var changedSpaces = model.ChildSpaces?.Where(cs => cs.isChanged).ToList();

            if (changedSpaces == null || !changedSpaces.Any())
            {
                _logger.LogWarning($"No changed child spaces found to rollback for site {id}");
                return;
            }

            foreach (var childSpace in changedSpaces)
            {
                if (string.IsNullOrWhiteSpace(childSpace.Id))
                {
                    _logger.LogWarning("Skipping rollback for child space with missing ID.");
                    continue;
                }

                string filePath = $"{id}/{childSpace.Id}.json";

                bool backupExists = await _blobSvc.ExistsBackupAsync(filePath);
                if (backupExists)
                {
                    await _blobSvc.RevertAsync(filePath, filePath);
                    _logger.LogInformation($"Reverted file from backup for child space {childSpace.Id}");
                }
                else
                {
                    _logger.LogWarning($"No backup found for child space {childSpace.Id}");
                }
            }
        }
        catch (Exception rollbackEx)
        {
            _logger.LogError(rollbackEx, $"Rollback failed for site {id}");
        }
        finally
        {
            throw new SiteSvcException(_logger, $"Failed to update site {id}. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task DeleteSiteAsync(string id)
    {
        try
        {
            var sites = await _cosmosRepository.GetAsync(x => x.Id == id);
            if (!sites.Any())
            {
                var inner = new Exception("Site name must be unique");
                throw new SiteSvcException(_logger, $"No site found with id: {id}. CorrelationId: {_correlationIdGenerator.Get()}", inner)
                {
                    HttpStatusCode = HttpStatusCode.Conflict
                };
            }
            await _cosmosRepository.DeleteAsync(sites.First());
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to delete sites. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (SiteSvcException ex)
        {
            throw;
            //throw new SiteSvcException(_logger, $"Failed to delete site. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            //{
            //    HttpStatusCode = ex.HttpStatusCode
            //};
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception.Failed to delete sites. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task<SiteGetModel> GetSiteByIdAsync(string id)
    {
        try
        {
            var sites = await _cosmosRepository.GetAsync(x => x.Id == id);
            var site = _mapper.Map<SiteGetModel>(sites.First());

            foreach (var childSpace in site.ChildSpaces)
            {
                var tags = await _blobSvc.GetTags($"{site.Id}/{childSpace.Id}.json");
                if (tags is { Count: > 0 })
                {
                    childSpace.GeoJsonFile = new FileModel
                    {
                        FileName = tags.TryGetValue("FileName", out var fileName) ? fileName : childSpace.Id,
                        FileType = tags.TryGetValue("FileType", out var fileType) ? fileType : string.Empty
                    };
                }
            }

            return site;
        }
        catch (SiteSvcException)
        {
            throw;
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception.Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }
    public async Task<SiteResponseModel> GetSpaceBySiteIdAndSpaceAsync(string siteId, string spaceName)
    {
        try
        {
            var site = await _cosmosRepository.GetAsync(x => x.Id == siteId).FirstOrDefaultAsync();
            var space = FindSpaceBySpaceName(site, spaceName);
            return _mapper.Map<SiteResponseModel>(space);
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (SiteSvcException ex)
        {
            throw new SiteSvcException(_logger, $"Failed to fetch site. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.HttpStatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception.Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task<SiteModel> GetSpaceBySiteIdAndSpaceAsyncNew(string siteId, string spaceName)
    {
        try
        {
            var site = await _cosmosRepository.GetAsync(x => x.Id == siteId).FirstOrDefaultAsync();
            var space = FindSpaceBySpaceName(site, spaceName);
            return _mapper.Map<SiteModel>(space);
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (SiteSvcException ex)
        {
            throw new SiteSvcException(_logger, $"Failed to fetch site. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.HttpStatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception.Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public List<SceneModel> GetScenesBySiteIdFloorAndSpace(string id, string? floor = null, string? space = null)
    {
        try
        {
            List<SceneModel> scenes = new List<SceneModel>();
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(floor) && !string.IsNullOrEmpty(space))
            {
                scenes.Add(new SceneModel("1", "Scene 1"));
                scenes.Add(new SceneModel("2", "Scene 2"));
                scenes.Add(new SceneModel("3", "Scene 3"));
                scenes.Add(new SceneModel("4", "Scene 4"));
                scenes.Add(new SceneModel("5", "Scene 5"));
                scenes.Add(new SceneModel("6", "Scene 6"));
                scenes.Add(new SceneModel("7", "Scene 7"));
                scenes.Add(new SceneModel("8", "Scene 8"));
            }
            else if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(floor))
            {
                scenes.Add(new SceneModel("1", "Scene 1"));
                scenes.Add(new SceneModel("2", "Scene 2"));
                scenes.Add(new SceneModel("3", "Scene 3"));
                scenes.Add(new SceneModel("4", "Scene 4"));
            }
            else if (!string.IsNullOrEmpty(id))
            {
                scenes.Add(new SceneModel("6", "Scene 6"));
                scenes.Add(new SceneModel("7", "Scene 7"));
                scenes.Add(new SceneModel("8", "Scene 8"));
            }
            return scenes;
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Unable to fetch scenes for siteId: {id}. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public List<SceneModel> GetGroupsBySiteIdFloorAndSpace(string id, string? floor = null, string? space = null)
    {
        try
        {
            List<SceneModel> groups = new List<SceneModel>();
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(floor) && !string.IsNullOrEmpty(space))
            {
                groups.Add(new SceneModel("1", "ALL"));
                groups.Add(new SceneModel("2", "Down Lights"));
                groups.Add(new SceneModel("3", "Spot Lights"));
                groups.Add(new SceneModel("4", "Wall Lights"));
                groups.Add(new SceneModel("5", "Exhibit 1"));
                groups.Add(new SceneModel("6", "Exhibit 2"));
                groups.Add(new SceneModel("7", "Exhibit 3"));
                groups.Add(new SceneModel("8", "Exhibit 4"));
            }
            else if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(floor))
            {
                groups.Add(new SceneModel("1", "ALL"));
                groups.Add(new SceneModel("2", "Down Lights"));
                groups.Add(new SceneModel("3", "Spot Lights"));
                groups.Add(new SceneModel("4", "Wall Lights"));
            }
            else if (!string.IsNullOrEmpty(id))
            {
                groups.Add(new SceneModel("1", "ALL"));
                groups.Add(new SceneModel("2", "Down Lights"));
                groups.Add(new SceneModel("3", "Spot Lights"));
                groups.Add(new SceneModel("4", "Wall Lights"));
                groups.Add(new SceneModel("4", "Emergency Lights"));
            }
            return groups;
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Unable to fetch scenes for siteId: {id}. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }
    public async Task<List<SiteMetaDataResponse>> GetMetaDataAsync()
    {
        try
        {
            var sites = await _cosmosRepository.GetAsync(_ => true);
            var result = new List<SiteMetaDataResponse>();

            foreach (var site in sites)
            {
                result.Add(new SiteMetaDataResponse()
                {
                    Id = site.Id,
                    Name = site.Name,
                    Organization = site.Organization,
                    Type = site.BuildingType,
                    TotalFloors = site.ChildSpaces.Count(),
                });
            }

            return result;
        }
        catch (CosmosException ex)
        {
            throw new SiteSvcException(_logger, $"Cosmos Related exception. Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (SiteSvcException ex)
        {
            throw new SiteSvcException(_logger, $"Failed to fetch site. CorrelationId: {_correlationIdGenerator.Get()}", ex)
            {
                HttpStatusCode = ex.HttpStatusCode
            };
        }
        catch (Exception ex)
        {
            throw new SiteSvcException(_logger, $"Not a cosmos Related Exception.Failed to fetch sites. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    public async Task<string?> RevertSiteAsync(string siteId, string floorId)
    {
        try
        {
            string backupFilePath = $"{siteId}/{floorId}.json";
            string originalFilePath = $"{siteId}/{floorId}.json";

            // If there is no backup it will return null 
            if (!await _blobSvc.ExistsBackupAsync(backupFilePath))
                return null;

            await _blobSvc.RevertAsync(backupFilePath, originalFilePath);

            return "Revert successful";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to revert site {siteId}, floor {floorId}");
            throw new SiteSvcException(_logger, $"Unexpected error during revert. CorrelationId: {_correlationIdGenerator.Get()}", ex);
        }
    }

    //Breath first search algorithm to traverse the building by space levels
    private SiteCosmosDb? FindSpaceBySpaceName(SiteCosmosDb? parent, string spaceName)
    {
        if (parent == null)
            return null;

        var cleanSpaceName = spaceName.Replace('+', ' ');

        Queue<SiteCosmosDb> queue = new Queue<SiteCosmosDb>();
        queue.Enqueue(parent);
        int levelIndex = 0;

        while (queue.Count > 0)
        {
            int size = queue.Count;
            for (int i = 0; i < size; i++)
            {
                SiteCosmosDb currentNode = queue.Dequeue();
                if (currentNode.Name == cleanSpaceName)
                    return currentNode;

                if (currentNode.ChildSpaces != null)
                {
                    foreach (var child in currentNode.ChildSpaces)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            levelIndex++;
        }

        return null;
    }

    private async Task<byte[]> EnrichGeoJsonWithSpaceIds(string siteId, string spaceId, IFormFile geojson)
    {
        GeoJsonObject? geoJsonObject;
        try
        {
            //TODO - files will never be stored in the site - we will be dynamically fetching them from blob by file type
            var site = await _cosmosRepository.GetAsync(siteId);

            using (var memoryStream = new MemoryStream())
            {
                await geojson.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                geoJsonObject = await JsonSerializer.DeserializeAsync<GeoJsonObject>(memoryStream);
                foreach (var feature in geoJsonObject?.Features.Where(f => f.Properties is { Category: "Rooms" }) ??
                Array.Empty<Feature>())
                {
                    // TODO - this will only traverse first level! Handle nested spaces!!!
                    // only map when exchange id equal to geoJson exchange Id
                    // add condition - feature.Properties!.exchangeId = space.exchangeid
                    foreach (var spaces in site.ChildSpaces)
                    {
                        foreach (var childspaces in spaces.ChildSpaces)
                        {
                            if (feature.Properties!.ExchangeId.ToString() == childspaces?.ExchangeId.ToString())
                            {
                                feature.Properties!.MetisSpaceId = childspaces.Id;
                            }
                        }

                    }
                }

            }

            byte[] modifiedFileBytes;
            using (var ms = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(ms, geoJsonObject);
                modifiedFileBytes = ms.ToArray();
            }

            await _blobSvc.Upload($"{siteId}/{spaceId}.json",
                BinaryData.FromBytes(modifiedFileBytes),
                overwrite: true,
                tags: new Dictionary<string, string>
                {
                    { "FileName", BlobHelper.SanitizeTagName(geojson.FileName) ?? spaceId },
                    { "FileType", geojson.ContentType ?? string.Empty }
                });


        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during parsing and uploading geojson to blob for space with {siteId} Id. CorrelationId: {_correlationIdGenerator.Get()}. ErrorMessage: {ex.Message}");
        }
        return null;
    }

    private void FillSpacesEmptyIdsWithValues(List<SiteModel> spaces)
    {
        //TODO - Why are we having spaces without Ids? can you please point to such cases? We should not be having functions like this one!
        foreach (var space in spaces.Where(x => string.IsNullOrWhiteSpace(x.Id)))
        {
            space.Id = Guid.NewGuid().ToString();
            if (space.ChildSpaces != null && space.ChildSpaces.Any())
                FillSpacesEmptyIdsWithValues(space.ChildSpaces);
        }
    }

    public async Task UpdateFileAsync(string siteId, string floorId, FileUploadRequest fileUpload)
    {
        var correlationId = _correlationIdGenerator.Get();
        try
        {
            // Check and backup old file
            string category = fileUpload.Category.ToString();
            string filePath = $"{siteId}/{floorId}.json";
            if (await _blobSvc.ExistsAsync(filePath))
            {
                try
                {
                    if (await _blobSvc.ExistsBackupAsync(filePath))
                    {
                        await _blobSvc.DeleteAsync(filePath);
                    }

                    await _blobSvc.MoveAsync(filePath, filePath);
                }
                catch (Exception moveEx)
                {
                    _logger.LogError($"Failed to move existing GeoJSON file to backup for site {siteId}, space {floorId}.CorrelationId: {correlationId} .ErrorMessage: {moveEx}");
                }
            }

            //TODO - Update the content of the geojson file before we upload or move it
            if (fileUpload.Category == FileCategory.MapConfiguration)
            {
                var bytes = await EnrichGeoJsonWithSpaceIds(siteId, floorId, fileUpload.File);
            }

            //TODO - this should not be here at all. Please make sure that the front end calls UpdateSiteAsync to add the new space which is the floor ahead of processing the geojson
            //var siteItem = await _cosmosRepository.GetAsync(siteId);
            //if (siteItem.ChildSpaces != null)
            //{
            //    //TODO - child spaces can be nested so we need to traverse all!
            //    int index = siteItem.ChildSpaces.FindIndex(cs => cs.Id == model.SpaceId);
            //    if (index >= 0)
            //    {
            //        siteItem.ChildSpaces[index] = updatedChild;
            //    }
            //}
            //var space = await _cosmosRepository.UpdateAsync(siteItem);
        }
        catch (Exception ex)
        {

            _logger.LogError($"GeoJSON single update failed for site {siteId}, floorId {floorId}.CorrelationId: {correlationId}. Error: {ex.Message}");
            throw new SiteSvcException(_logger, $"Unexpected error during GeoJSON update. CorrelationId: {correlationId}", ex);
        }
    }
}