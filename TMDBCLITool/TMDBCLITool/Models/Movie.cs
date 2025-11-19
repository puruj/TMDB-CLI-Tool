using System;
using System.Text.Json.Serialization;

namespace TMDBCLITool.Models
{
    public class Movie
    {
        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("release_date")]
        public DateTime ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("vote_count")]
        public long VoteCount { get; set; }
    }
}
