using System.Text.Json.Serialization;

namespace CustomerCustomerApi.Models.Site;

public class GeoJsonObject
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("features")] 
    public List<Feature> Features { get; set; } = new List<Feature>();
}

public class Feature
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; }

    [JsonPropertyName("properties")]
    public Properties? Properties { get; set; }
}

public class Geometry
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("coordinates")]
    public List<List<List<double>>> Coordinates { get; set; }
}

public class Properties
{
    [JsonPropertyName("exchangeId")]
    public int ExchangeId { get; set; }

    [JsonPropertyName("metisSpaceId")]
    public string MetisSpaceId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }
}