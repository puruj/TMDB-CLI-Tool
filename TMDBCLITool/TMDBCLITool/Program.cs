using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TMDBCLITool;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        // 1. Validate and map the command
        var command = args.Length > 0 ? args[0] : null;

        if (string.IsNullOrWhiteSpace(command) || command.Equals("help", System.StringComparison.OrdinalIgnoreCase))
        {
            PrintUsage();
            // If no command was passed, treat as error; if "help", exit 0
            return string.IsNullOrWhiteSpace(command) ? 1 : 0;
        }

        var typeOfMovie = command.ToLowerInvariant() switch
        {
            "playing" => "now_playing",
            "popular" => "popular",
            "top" => "top_rated",
            "upcoming" => "upcoming",
            _ => null
        };

        if (typeOfMovie is null)
        {
            Console.Error.WriteLine($"Unknown command: '{command}'");
            Console.Error.WriteLine();
            PrintUsage();
            return 1;
        }

        try
        {
            // 2. Get access token (env var first, then .env file)
            var token = LoadAccessToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.Error.WriteLine(
                    "Error: TMDB access token not configured.\n" +
                    "Set TMDB_ACCESS_TOKEN as an environment variable or in a .env file.");
                return 1;
            }

            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            var client = new TMDBClient(httpClient, token);

            // 3. Call TMDB and get the list
            var movieList = await client.GetMovieListAsync(typeOfMovie);

            if (movieList is null || movieList.Results.Count == 0)
            {
                Console.WriteLine("No movies returned.");
                return 0;
            }

            // 4. Print a nice table of movies
            MovieTablePrinter.Print(movieList.Results);

            return 0;
        }
        catch (TMDBNotFoundException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (TMDBAuthorizationException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (TMDBRateLimitException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Network/API error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Shows usage information for the CLI.
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- <command>");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  playing   List movies that are now playing in theatres");
        Console.WriteLine("  popular   List popular movies");
        Console.WriteLine("  top       List top-rated movies");
        Console.WriteLine("  upcoming  List upcoming movies");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run -- playing");
        Console.WriteLine("  dotnet run -- popular");
        Console.WriteLine("  dotnet run -- top");
        Console.WriteLine("  dotnet run -- upcoming");
    }

    /// <summary>
    /// Tries to load TMDB_ACCESS_TOKEN from:
    /// 1) Environment variable
    /// 2) .env file in the current directory
    /// </summary>
    private static string? LoadAccessToken()
    {
        // 1. Environment variable first
        var token = Environment.GetEnvironmentVariable("TMDB_ACCESS_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
        {
            return token.Trim();
        }

        // 2. Fallback: .env file
        const string envFileName = ".env";
        if (File.Exists(envFileName))
        {
            foreach (var line in File.ReadAllLines(envFileName))
            {
                var trimmed = line.Trim();

                // Ignore empty lines and comments
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                {
                    continue;
                }

                var parts = trimmed.Split('=', 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                var key = parts[0].Trim();
                var value = parts[1].Trim().Trim('"'); // remove surrounding quotes

                if (string.Equals(key, "TMDB_ACCESS_TOKEN", StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }
        }

        return null;
    }
}
