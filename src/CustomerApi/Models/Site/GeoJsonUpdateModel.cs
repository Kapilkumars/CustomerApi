using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerCustomerApi.Models.Site
{
    public enum FileCategory
    {
        [Description("Map Configuration")]
        MapConfiguration,

        [Description("ETC")]
        ETC
    }

    public class FileUploadRequest
    {
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public FileCategory Category { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }

    //public class GeoJsonUpdateModel

    //{
    //    public string SiteId { get; set; }
    //    public string SpaceId { get; set; }
    //    public string ChildSpaces { get; set; }
    //    public GeometryBase? Geometry { get; set; }
    //    public FileModel GeoJsonFile { get; set; }
    //}

}
