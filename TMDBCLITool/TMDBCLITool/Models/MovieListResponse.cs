using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TMDBCLITool.Models;

public class MovieListResponse
{
    [JsonPropertyName("results")]
    public List<Movie> Results { get; set; } = new();
}
