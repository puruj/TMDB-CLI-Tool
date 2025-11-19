using TMDBCLITool.Models;
using TMDBCLITool;
using System.Net;

namespace TMDBCLIToolTests
{
    namespace TMDBCLIToolTests
    {
        public class MovieTablePrinterTests
        {
            [Fact]
            public void Print_NoMovies_WritesNoMoviesMessage()
            {
                // Arrange
                var movies = Enumerable.Empty<Movie>();
                var originalOut = Console.Out;
                var writer = new StringWriter();
                Console.SetOut(writer);

                try
                {
                    // Act
                    MovieTablePrinter.Print(movies);

                    // Assert
                    var output = writer.ToString().TrimEnd();
                    Assert.Equal("No movies to display.", output);
                }
                finally
                {
                    // Always restore console
                    Console.SetOut(originalOut);
                }
            }

            [Fact]
            public void Print_SingleMovie_PrintsHeaderAndRow()
            {
                // Arrange
                var movie = new Movie
                {
                    Title = "Inception",
                    ReleaseDate = new DateTime(2010, 7, 16),
                    VoteAverage = 8.8,
                    VoteCount = 12345
                };

                var originalOut = Console.Out;
                var writer = new StringWriter();
                Console.SetOut(writer);

                try
                {
                    // Act
                    MovieTablePrinter.Print(new[] { movie });

                    // Assert
                    var output = writer.ToString();

                    // Check header
                    Assert.Contains("Title", output);
                    Assert.Contains("Year", output);
                    Assert.Contains("Rating", output);
                    Assert.Contains("Votes", output);

                    // Check basic content
                    Assert.Contains("Inception", output);
                    Assert.Contains("2010", output);
                    Assert.Contains("8.8", output);
                    Assert.Contains("12,345", output); // thousands format
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            }

            [Fact]
            public void Print_LongTitle_IsTruncatedWithEllipsis()
            {
                // Arrange
                var longTitle = new string('A', 60);
                var movie = new Movie
                {
                    Title = longTitle,
                    ReleaseDate = new DateTime(2024, 1, 1),
                    VoteAverage = 7.1,
                    VoteCount = 1
                };

                var originalOut = Console.Out;
                var writer = new StringWriter();
                Console.SetOut(writer);

                try
                {
                    // Act
                    MovieTablePrinter.Print(new[] { movie });

                    // Assert
                    var output = writer.ToString();

                    // Title width is 40, we keep 37 chars then add "..."
                    var expectedPrefix = new string('A', 37) + "...";
                    Assert.Contains(expectedPrefix, output);
                    Assert.DoesNotContain(longTitle, output); // full title should not appear
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            }
        }

        public class TMDBClientTests
        {
            [Fact]
            public async Task GetMovieListAsync_Success_ReturnsParsedMovies()
            {
                // Arrange
                var json = """
            {
              "results": [
                {
                  "title": "The Matrix",
                  "release_date": "1999-03-31",
                  "vote_average": 8.7,
                  "vote_count": 20000
                }
              ]
            }
            """;

                var handler = new StubHttpMessageHandler(_ =>
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json)
                    });

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://api.themoviedb.org/3/")
                };

                var client = new TMDBClient(httpClient, "dummy-token");

                // Act
                var result = await client.GetMovieListAsync("now_playing");

                // Assert
                Assert.NotNull(result);
                Assert.Single(result!.Results);

                var movie = result.Results[0];
                Assert.Equal("The Matrix", movie.Title);
                Assert.Equal(1999, movie.ReleaseDate.Year);
                Assert.Equal(8.7, movie.VoteAverage);
                Assert.Equal(20000, movie.VoteCount);
            }

            [Fact]
            public async Task GetMovieListAsync_404_ThrowsNotFoundException()
            {
                // Arrange
                var handler = new StubHttpMessageHandler(_ =>
                    new HttpResponseMessage(HttpStatusCode.NotFound));

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://api.themoviedb.org/3/")
                };

                var client = new TMDBClient(httpClient, "dummy-token");

                // Act & Assert
                var ex = await Assert.ThrowsAsync<TMDBNotFoundException>(
                    () => client.GetMovieListAsync("made_up_type"));

                Assert.Equal("made_up_type", ex.TypeOfMovie);
            }

            [Theory]
            [InlineData(HttpStatusCode.Unauthorized)]
            [InlineData(HttpStatusCode.Forbidden)]
            public async Task GetMovieListAsync_UnauthorizedOrForbidden_ThrowsAuthorizationException(HttpStatusCode status)
            {
                // Arrange
                var handler = new StubHttpMessageHandler(_ =>
                    new HttpResponseMessage(status));

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://api.themoviedb.org/3/")
                };

                var client = new TMDBClient(httpClient, "dummy-token");

                // Act & Assert
                await Assert.ThrowsAsync<TMDBAuthorizationException>(
                    () => client.GetMovieListAsync("now_playing"));
            }

            [Fact]
            public async Task GetMovieListAsync_429_ThrowsRateLimitException()
            {
                // Arrange
                var handler = new StubHttpMessageHandler(_ =>
                    new HttpResponseMessage((HttpStatusCode)429));

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://api.themoviedb.org/3/")
                };

                var client = new TMDBClient(httpClient, "dummy-token");

                // Act & Assert
                await Assert.ThrowsAsync<TMDBRateLimitException>(
                    () => client.GetMovieListAsync("now_playing"));
            }

            [Fact]
            public async Task GetMovieListAsync_OtherErrorStatus_ThrowsHttpRequestException()
            {
                // Arrange
                var handler = new StubHttpMessageHandler(_ =>
                    new HttpResponseMessage(HttpStatusCode.InternalServerError));

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://api.themoviedb.org/3/")
                };

                var client = new TMDBClient(httpClient, "dummy-token");

                // Act & Assert
                await Assert.ThrowsAsync<HttpRequestException>(
                    () => client.GetMovieListAsync("now_playing"));
            }
        }
    }

    public class ProgramTests
    {
        [Fact]
        public async Task Main_NoArgs_PrintsUsageAndReturns1()
        {
            // Arrange
            var originalOut = Console.Out;
            var writerOut = new StringWriter();
            Console.SetOut(writerOut);

            try
            {
                // Act
                var code = await Program.Main(Array.Empty<string>());

                // Assert
                var output = writerOut.ToString();
                Assert.Contains("Usage:", output);
                Assert.Equal(1, code);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public async Task Main_HelpCommand_PrintsUsageAndReturns0()
        {
            var originalOut = Console.Out;
            var writerOut = new StringWriter();
            Console.SetOut(writerOut);

            try
            {
                var code = await Program.Main(new[] { "help" });

                var output = writerOut.ToString();
                Assert.Contains("Usage:", output);
                Assert.Equal(0, code);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public async Task Main_UnknownCommand_PrintsErrorAndReturns1()
        {
            var originalOut = Console.Out;
            var originalErr = Console.Error;
            var writerErr = new StringWriter();
            Console.SetError(writerErr);

            try
            {
                var code = await Program.Main(new[] { "badcommand" });

                var error = writerErr.ToString();
                Assert.Contains("Unknown command", error);
                Assert.Equal(1, code);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalErr);
            }
        }

        [Fact]
        public async Task Main_PlayingWithoutToken_PrintsTokenErrorAndReturns1()
        {
            // Arrange: ensure env var is not set for this test
            Environment.SetEnvironmentVariable("TMDB_ACCESS_TOKEN", null);

            var originalErr = Console.Error;
            var writerErr = new StringWriter();
            Console.SetError(writerErr);

            try
            {
                var code = await Program.Main(new[] { "playing" });

                var error = writerErr.ToString();
                Assert.Contains("TMDB access token not configured", error);
                Assert.Equal(1, code);
            }
            finally
            {
                Console.SetError(originalErr);
            }
        }
    }
}