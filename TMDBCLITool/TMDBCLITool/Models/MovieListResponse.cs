using System.Text.Json.Serialization;

namespace TMDBCLITool.Models;

public class MovieListResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("results")]
    public List<Movie> Results { get; set; } = new();

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }

    // Only present on some endpoints (now_playing/upcoming), so nullable
    [JsonPropertyName("dates")]
    public DateRange? Dates { get; set; }
}

public class DateRange
{
    [JsonPropertyName("maximum")]
    public DateTime Maximum { get; set; }

    [JsonPropertyName("minimum")]
    public DateTime Minimum { get; set; }
}
