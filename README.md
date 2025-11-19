# TMDB CLI Tool

CLI tool that fetches movie data from [The Movie Database (TMDB)](https://www.themoviedb.org/) and prints it as a formatted table in your terminal.

This project is an implementation of the Roadmap.sh project:  
https://roadmap.sh/projects/tmdb-cli

---

## Features

- Fetches movies from TMDB for four categories:
  - **Now Playing** (`playing`)
  - **Popular** (`popular`)
  - **Top Rated** (`top`)
  - **Upcoming** (`upcoming`)
- Displays results in a clean table with columns:
  - Title (truncated to keep the table readable)
  - Year
  - Rating
  - Vote count
- Handles common error cases:
  - Missing / invalid TMDB access token
  - Network / API errors
  - TMDB-specific errors (404, authorization issues, rate limiting)
- Written in **C# / .NET 8** as a console app
- Can be run with `dotnet run` or installed as a **.NET global tool**

---

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- A TMDB account and **v4 API Read Access Token**

You can create an account and get your token from TMDB here:  
https://www.themoviedb.org/settings/api

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/puruj/TMDB-CLI-Tool.git
cd TMDB-CLI-Tool/TMDBCLITool
```

The main C# project lives in the `TMDBCLITool` folder.

### 2. Configure your TMDB access token

The app looks for an access token in this order:

1. `TMDB_ACCESS_TOKEN` **environment variable**
2. A `.env` file in the working directory

#### Option A – Environment variable (recommended)

**Windows (PowerShell):**

```powershell
$env:TMDB_ACCESS_TOKEN = "your_tmdb_v4_read_access_token_here"
```

**Linux / macOS (bash/zsh):**

```bash
export TMDB_ACCESS_TOKEN="your_tmdb_v4_read_access_token_here"
```

#### Option B – `.env` file

Create a file named `.env` in the `TMDBCLITool` project directory:

```env
TMDB_ACCESS_TOKEN="your_tmdb_v4_read_access_token_here"
```

Comments (`# like this`) and empty lines are ignored.

---

## Running the CLI

From the `TMDBCLITool` project folder:

```bash
dotnet run -- playing
dotnet run -- popular
dotnet run -- top
dotnet run -- upcoming
```

### Commands

The first argument selects the type of movies:

| Command   | TMDB endpoint | Description                 |
|----------|----------------|-----------------------------|
| `playing` | `now_playing`  | Movies that are now playing |
| `popular` | `popular`      | Popular movies              |
| `top`     | `top_rated`    | Top-rated movies            |
| `upcoming`| `upcoming`     | Upcoming releases           |

### Help / usage

```bash
dotnet run -- help
# or just
dotnet run
```

This prints:

- Available commands
- Example usages
- A reminder about the `TMDB_ACCESS_TOKEN` configuration

---

## Example Output

```text
Title                                   Year  Rating      Votes
-------------------------------------- ------ ------ ----------
Dune: Part Two                          2024    8.4    25,123
Kung Fu Panda 4                         2024    7.2    10,987
Poor Things                             2023    8.1     8,456
...
```

- Titles longer than 40 characters are truncated with `...`
- Ratings are shown with one decimal
- Votes are formatted with thousands separators (e.g. `12,345`)

---

## Installing as a .NET Global Tool (optional)

This project is also configured to be packed as a .NET tool with the command name **`tmdb-movies`**.

From the `TMDBCLITool` project directory:

```bash
# Build and pack the tool
dotnet pack -c Release

# Install globally from the generated nupkg
dotnet tool install --global Puru.TmdbMovies.Cli --add-source ./nupkg
```

After installation you can run:

```bash
tmdb-movies playing
tmdb-movies popular
tmdb-movies top
tmdb-movies upcoming
```

(You still need to set `TMDB_ACCESS_TOKEN` as described above.)

To uninstall:

```bash
dotnet tool uninstall --global Puru.TmdbMovies.Cli
```

---

## Project Structure

```text
TMDB-CLI-Tool/
│
├── TMDBCLITool/               # Main C# project
│   ├── Models/
│   │   ├── Movie.cs
│   │   └── MovieListResponse.cs
│   ├── MovieTablePrinter.cs   # Renders movies as a table
│   ├── TMDBClient.cs          # HTTP client for TMDB API
│   └── Program.cs             # CLI entry point and command handling
│
├── TMDBCLIToolTests/          # xUnit tests for the CLI
│   └── ...
│
├── .github/workflows/
│   └── dotnet.yml             # CI: build & test on GitHub Actions
│
└── README.md                  # You are here
```

---

## Error Handling

The CLI tries to fail **loudly and clearly**:

- Missing token  
  → Prints: `Error: TMDB access token not configured...` and exits with code `1`.

- Unknown command  
  → Prints `Unknown command: '<input>'`, shows usage, exits with code `1`.

- TMDB API issues:
  - `404` → “Not found” error for the requested type
  - `401 / 403` → Authorization error (bad token/missing permissions)
  - `429` → Rate limit error
  - Other HTTP/network errors → Printed as `Network/API error: ...`

- Unexpected exceptions are caught and printed as `Unexpected error: ...` with a non-zero exit code.

---

## How This Maps to the Roadmap.sh Project

Original project description:  
https://roadmap.sh/projects/tmdb-cli

- ✅ Runs from the command line  
- ✅ Fetches **popular**, **top-rated**, **upcoming**, and **now playing** movies from the TMDB API  
- ✅ User specifies what they want to see via a command argument (`playing`, `popular`, `top`, `upcoming`)  
- ✅ Handles errors gracefully and prints helpful messages  
- ✅ README explains how to set up and run the app

---

## Future Improvements

Some ideas to extend this CLI:

- Add `--limit` flag to control how many movies are printed
- Add search by title (e.g. `tmdb-movies search "Inception"`)
- Support additional filters (language, region, minimum vote count, etc.)
- Output as JSON or CSV in addition to the table view
- Add richer tests around formatting and edge cases

---

If you have any feedback or suggestions, feel free to open an issue or a pull request on the repository.
