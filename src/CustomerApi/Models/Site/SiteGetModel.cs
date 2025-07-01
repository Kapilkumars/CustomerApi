using Customer.Metis.Common.Space;

namespace CustomerCustomerApi.Models.Site
{
    public class SiteGetModel
    {
        public string Name { get; set; }
        public GeometryBase? Geometry { get; set; }
        public int? ActiveScene { get; set; }
        public object? Schedule { get; set; }
        public string? Organization { get; set; }
        public string? Address { get; set; }
        public int? Index { get; set; }
        public string? ExchangeName { get; set; }
        public string? ExchangeId { get; set; }
        public string Id { get; set; }
        public string? BuildingType { get; set; }
        public string? Type { get; set; }
        public FileModel? GeoJsonFile { get; set; }
        public List<SiteGetModel> ChildSpaces { get; set; }
        public bool isChanged { get; set; } = false;
    }

    public class FileModel
    {
        public string? FileName { get; set; }
        public string? FileType { get; set; }
    }
}