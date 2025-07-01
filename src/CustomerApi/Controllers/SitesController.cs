using Azure.Storage.Blobs.Models;
using Customer.Metis.Common.Models.Responses;
using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models.Site;
using CustomerCustomerApi.ResponseHelpers;
using CustomerCustomerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;

namespace CustomerCustomerApi.Controllers;

[Authorize(AuthenticationSchemes = "MetisB2C,CloudPortalB2C,ApiKey")]
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = "Sites")]
[Route("api/v1.0/sites")]
[ApiVersion("1.0")]
public class SiteController : ApiControllerBase
{
    private readonly ISiteSvc _sitesSvc;
    private readonly IBlobSvc _blobSvc;

    public SiteController(ILogger<SiteController> logger,
                         ICorrelationIdGenerator correlationIdGenerator,
                         ISiteSvc sitesSvc, IBlobSvc blobSvc)
                         : base(logger, correlationIdGenerator)
    {
        _sitesSvc = sitesSvc;
        _blobSvc = blobSvc;
    }

    /// <summary>
    /// Obtain all sites
    /// </summary>
    /// <param name="siteName"></param>
    /// <returns>
    /// List of sites (ONLY TEMPORARY allow for SiteName for backwardscompatibility)
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SiteResponseModel>), StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetAllAsync([FromQuery] string? siteName = null)
    {
        try
        {
            var result = await _sitesSvc.GetAllAsync(siteName);
            return Ok(result);
        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", "Problem during obtaining sites.");
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Error in SiteSvc, see inner exception", "Unexpected error");
        }
    }

    /// <summary>
    /// Obtain site by siteId
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns>
    /// List of sites
    /// </returns>
    [HttpGet("{siteId}")]
    [ProducesResponseType(typeof(SiteGetModel), StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetByIdAsync(string siteId)
    {
        try
        {
            var result = await _sitesSvc.GetSiteByIdAsync(siteId);
            return Ok(result);
        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", "Problem during obtaining sites.");
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Error in SiteSvc, see inner exception", "Unexpected error");
        }
    }
    /// <summary>
    /// Obtain site by siteId and optional floor
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="spaceId"></param>
    /// <returns>
    /// List of sites
    /// </returns>
    [HttpGet("{siteId}/spaces/{spaceId}")]
    [ProducesResponseType(typeof(SiteResponseModel), StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetSpaceBySiteAndSpaceIdsAsync(string siteId, string spaceId)
    {
        try
        {
            var result = await _sitesSvc.GetSpaceBySiteIdAndSpaceAsync(siteId, spaceId);
            return Ok(result);
        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", "Problem during obtaining sites.");
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Error in SiteSvc, see inner exception", "Unexpected error");
        }
    }
    /// <summary>
    /// Obtain all sites
    /// </summary>
    /// <returns>
    /// List of sites
    /// </returns>
    [HttpGet("metadata")]
    [ProducesResponseType(typeof(SiteMetaDataResponse), StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetMetaDataAsync()
    {
        try
        {
            var result = await _sitesSvc.GetMetaDataAsync();
            return Ok(result);
        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", "Problem during obtaining sites.");
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Error in SiteSvc, see inner exception", "Unexpected error");
        }
    }


    /// <summary>
    /// Create new site with floors(floors is optional)
    /// </summary>
    /// <param name="createSiteApiModel"></param>   
    /// <returns>
    /// Return created site model
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(SiteResponseModel), StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> CreateAsync([FromBody] SiteModel createSiteApiModel)
    {
        try
        {
            var createdModelId = await _sitesSvc.CreateSiteAsync(createSiteApiModel);
            return Ok(createdModelId);
        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Error in SiteSvc, see inner exception", "Unexpected error");
        }
    }

    /// <summary>
    /// Update site with floors(floors is optional)
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="updateSiteApiModel"></param>
    /// <returns>
    /// Return no content response if success
    /// </returns>
    [HttpPut("{siteId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync([FromRoute] string siteId, [FromBody] SiteModel updateSiteApiModel)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(siteId) || updateSiteApiModel == null)
                return BadRequest("Site ID and model data must be provided");

            var updatedModel = await _sitesSvc.UpdateSiteAsync(updateSiteApiModel, siteId);
            return Ok(updatedModel);

        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Unexpected error in UpdateAsync", "Unexpected error");
        }
    }

    /// <summary>
    /// Delete site from db
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns>
    /// Return no content response if success
    /// </returns>
    [HttpDelete("{siteId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> DeleteAsync([FromRoute] string siteId)
    {
        try
        {
            await _sitesSvc.DeleteSiteAsync(siteId);
            return NoContent();
        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", "Problem during updating site.");
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Error in SiteSvc, see inner exception", "Unexpected error");
        }
    }

    /// <summary>
    /// Get Scenes by SiteId, Flooor and Space
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SceneModel>), StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    [Route("{siteId}/scenes")]
    public async Task<IActionResult> GetScenesBySiteIdFloorAndSpace([FromRoute] string siteId, [FromQuery] string? floor = null, [FromQuery] string? space = null)
    {   
        try
        {
            var scenes = _sitesSvc.GetScenesBySiteIdFloorAndSpace(siteId, floor, space);
            return Ok(scenes);
        }
        catch (Exception e)
        {
            return ErrorProblem(e);
        }
    }
    /// <summary>
    /// Get Groups by SiteId, Flooor and Space                                                                              
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    [Route("{siteId}/groups")]
    public async Task<IActionResult> GetGroupsBySiteIdFloorAndSpace([FromRoute] string siteId, [FromQuery] string? floor = null, [FromQuery] string? space = null)
    {
        try
        {
            var groups = _sitesSvc.GetGroupsBySiteIdFloorAndSpace(siteId, floor, space);
            return Ok(groups);
        }
        catch (Exception e)
        {
            return ErrorProblem(e);
        }   
    }

    /// <summary>
    /// Revert blob file to the last backup version.
    /// </summary>
    /// <param name="siteId"></param>
    /// <param name="floorId"></param>
    /// <returns>Returns appropriate status codes based on the outcome.</returns>
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [HttpPost("{siteId}/floor/{floorId}/map-configuration/revert")]
    public async Task<IActionResult> RevertAsync([FromRoute] string siteId, [FromRoute] string floorId)
    {
        try
        {
            // Check siteId and floorId before calling service layer
            if (string.IsNullOrWhiteSpace(siteId) || string.IsNullOrWhiteSpace(floorId))
            {
                return BadRequest("SiteId and FloorId must not be empty");
            }

            var result = await _sitesSvc.RevertSiteAsync(siteId, floorId);

            // Check based on service returning null
            if (result == null)
            {
                return NotFound("Backup file not found");
            }

            return Ok(result);
        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Unexpected error in RevertAsync", "Unexpected error");
        }
    }

    /// <summary>
    /// Update geoJson for each floor.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>
    /// Return no content response if success
    /// </returns>
    [HttpPost("{siteId}/floor/{floorId}/map-configuration")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFileAsync([FromRoute, Required] string siteId, [FromRoute, Required] string floorId, [FromForm] FileUploadRequest fileUpload)
    {
        try
        {
            if (fileUpload == null || fileUpload.File == null || fileUpload.File.Length == 0)
                return BadRequest("No file uploaded.");

            if (string.IsNullOrWhiteSpace(siteId) || string.IsNullOrWhiteSpace(floorId))
                return BadRequest("Site Id and Floor Id must be provided");

            await _sitesSvc.UpdateFileAsync(siteId, floorId, fileUpload);

            return Ok();
        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Unexpected error in updating GeoJson File", "Unexpected error");
        }
    }

    [HttpPut("test-copy-tags")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestCopyTags()  //REMOVE - FOR DEMO PURPOSE ONLY
    {
        try
        {
            string blobContents = "Sample blob data";
            var tags = new Dictionary<string, string>
            {
                { "TestTag1", "test1" },
                { "TestTag2", "test2" },
                { "DateTag", "2025-05-20" }
            };
            var sourcePath = $"test/testFile/source_{DateTime.Now.ToLongTimeString()}.txt";  //sites/test/testFile/source_*.txt
            await _blobSvc.Upload(sourcePath, BinaryData.FromString(blobContents), false, tags);
            
            var destPath = $"test/testFile/dest_{DateTime.Now.ToLongTimeString()}.txt"; //sites_backup/test/testFile/source_*.txt
            await _blobSvc.MoveAsync(sourcePath, destPath);
            return Ok();
        }
        catch (SiteSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Error in SiteSvc, see inner exception", ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Unexpected error in UpdateAsync", "Unexpected error");
        }
    }
}