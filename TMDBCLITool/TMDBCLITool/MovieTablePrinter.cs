using TMDBCLITool.Models;
namespace TMDBCLITool;

public static class MovieTablePrinter
{
    public static void Print(IEnumerable<Movie> movies)
    {
        var list = movies.ToList();

        if (list.Count == 0)
        {
            Console.WriteLine("No movies to display.");
            return;
        }

        const int titleWidth = 40;

        // Header
        Console.WriteLine($"{"Title",-titleWidth} {"Year",6} {"Rating",6} {"Votes",10}");

        Console.WriteLine(new string('-', titleWidth + 1 + 6 + 1 + 6 + 1 + 10));

        foreach (var movie in list)
        {
            var title = movie.Title.Length > titleWidth ? movie.Title[..(titleWidth - 3)] + "..." : movie.Title;

            var year = movie.ReleaseDate == default ? "" : movie.ReleaseDate.Year.ToString();

            var rating = movie.VoteAverage.ToString("0.0");
            var votes = movie.VoteCount.ToString("N0"); // 12,345 style

            Console.WriteLine($"{title,-titleWidth} {year,6} {rating,6} {votes,10}");
        }
    }
}
