using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TMDBCLITool.Models;


namespace TMDBCLITool
{
    public class TMDBClient
    {
        private readonly System.Net.Http.HttpClient _httpClient;
        private static JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public TMDBClient(HttpClient httpClient, string accessToken)
        {
            _httpClient = httpClient;

            // Only set once; can tweak this if HttpClient is used elsewhere
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            }

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public async Task<MovieListResponse?> GetMovieListAsync(string typeOfMovie, System.Threading.CancellationToken cancellationToken = default)
        {
            // This becomes https://api.themoviedb.org/3/movie/now_playing?language=en-US&page=1
            var url = $"movie/{typeOfMovie}?language=en-US&page=1";

            using var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // Wrong path / unknown type
                throw new TMDBNotFoundException(typeOfMovie);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                // Bad / missing token, or not allowed
                throw new TMDBAuthorizationException();
            }

            // TMDB uses 429 for rate limiting
            if ((int)response.StatusCode == 429)
            {
                throw new TMDBRateLimitException();
            }

            response.EnsureSuccessStatusCode();

            // Deserialize straight from the content
            var result = await response.Content.ReadFromJsonAsync<MovieListResponse>(_options, cancellationToken);

            return result;
        }
    }

    public sealed class TMDBNotFoundException : Exception
    {
        public string TypeOfMovie { get; }

        public TMDBNotFoundException(string typeOfMovie) : base($"TMDB resource for '{typeOfMovie}' not found.")
        {
            TypeOfMovie = typeOfMovie;
        }
    }

    public sealed class TMDBAuthorizationException : Exception
    {
        public TMDBAuthorizationException() : base("TMDB API authorization failed. Check your access token.")
        {
        }
    }

    public sealed class TMDBRateLimitException : Exception
    {
        public TMDBRateLimitException() : base("TMDB API rate limit exceeded.")
        {
        }
    }
}
