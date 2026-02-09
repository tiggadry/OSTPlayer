// ====================================================================
// FILE: GameService.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Services
// LOCATION: Services/
// VERSION: 2.0.0
// CREATED: 2025-08-08
// UPDATED: 2025-08-09
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Game service implementing IGameService interface for Phase 5 dependency injection.
// Provides comprehensive game data access, music file discovery, and game-related operations
// with full async support and progress reporting.
//
// FEATURES:
// - Full IGameService interface implementation
// - Constructor injection with automatic dependency resolution
// - Async/await patterns for non-blocking operations
// - Game library access and filtering with caching
// - Music file discovery and validation
// - Batch operations with progress reporting
// - Comprehensive error handling and health monitoring
// - **PHASE 5**: Production-ready service with optimized performance
// - **PHASE 5**: Advanced caching and batch operation support
//
// DEPENDENCIES (injected):
// - IPlayniteAPI (game database access)
// - ILogger (Playnite logging)
// - IErrorHandlingService (error management)
// - MusicFileHelper (file discovery utilities)
//
// DI ARCHITECTURE:
// - Interface-based dependency injection
// - Constructor injection for all dependencies
// - Service lifetime management through DI container
// - Testable with mock implementations
//
// PERFORMANCE NOTES:
// - Async operations prevent UI blocking
// - Efficient game filtering with caching
// - Batch operations with progress reporting
// - Memory-efficient file enumeration
//
// LIMITATIONS:
// - Requires Playnite API for game access
// - File system dependent for music discovery
// - Cache invalidation based on time only
// - No persistent storage for game metadata
//
// FUTURE REFACTORING:
// FUTURE: Add persistent cache storage for game metadata
// FUTURE: Implement smart cache invalidation based on filesystem changes
// FUTURE: Add support for network-based music libraries
// FUTURE: Implement music library indexing for faster searches
// FUTURE: Add support for custom music file naming conventions
// CONSIDER: Plugin architecture for custom music discovery
// CONSIDER: Integration with game launchers for better detection
// IDEA: Machine learning for automatic music file categorization
// IDEA: Cloud sync for game music library
//
// TESTING:
// - Unit tests for game filtering and search operations
// - Mock testing for Playnite API integration
// - Performance tests for large game libraries
// - File system operation error handling tests
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - Playnite SDK integration
// - Thread-safe for plugin environment
//
// CHANGELOG:
// 2025-08-09 v2.0.0 - Phase 5 DI Implementation completed: Production-ready game service, advanced caching, batch operations
// 2025-08-08 v1.0.0 - Initial implementation for Phase 5 DI
// ====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OstPlayer.Services.Interfaces;
using OstPlayer.Utils;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace OstPlayer.Services
{
    /// <summary>
    /// Game service implementing IGameService interface for Phase 5 dependency injection.
    /// Provides comprehensive game data access and music file operations.
    /// </summary>
    public class GameService : IGameService, IDisposable
    {
        private readonly IPlayniteAPI playniteApi;
        private readonly ILogger logger;
        private readonly ErrorHandlingService errorHandler;
        private volatile bool disposed = false;

        // Caching for performance
        private List<Game> cachedGamesWithMusic;
        private DateTime lastGameScanTime = DateTime.MinValue;
        private readonly TimeSpan cacheValidityPeriod = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Initializes the game service with dependency injection.
        /// All dependencies are resolved automatically by the DI container.
        /// </summary>
        /// <param name="playniteApi">Playnite API for game database access</param>
        /// <param name="logger">Logging service for monitoring</param>
        /// <param name="errorHandler">Error handling service</param>
        public GameService(
            IPlayniteAPI playniteApi = null,
            ILogger logger = null,
            ErrorHandlingService errorHandler = null
        )
        {
            // Initialize dependencies with fallbacks for backward compatibility
            this.playniteApi =
                playniteApi
                ?? throw new ArgumentNullException(nameof(playniteApi), "Playnite API is required");
            this.logger = logger ?? LogManager.GetLogger();
            this.errorHandler = errorHandler ?? new ErrorHandlingService();

            try
            {
                this.logger.Info("GameService initializing with dependency injection...");
                this.logger.Info("GameService initialized successfully with DI");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to initialize GameService with dependency injection");
                this.errorHandler.HandlePlaybackError(ex, "GameService Initialization");
                throw;
            }
        }

        /// <summary>
        /// Gets all games that contain music files.
        /// </summary>
        public async Task<List<Game>> GetGamesWithMusicAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Check cache validity
                if (
                    cachedGamesWithMusic != null
                    && DateTime.Now - lastGameScanTime < cacheValidityPeriod
                )
                {
                    logger.Debug(
                        $"Returning cached games with music ({cachedGamesWithMusic.Count} games)"
                    );
                    return new List<Game>(cachedGamesWithMusic);
                }

                logger.Info("Scanning for games with UniPlaySong music files...");

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var gamesWithMusic = playniteApi
                            .Database.Games.Where(game =>
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                try
                                {
                                    var musicFiles = MusicFileHelper.GetGameMusicFiles(
                                        playniteApi,
                                        game
                                    );
                                    return musicFiles != null && musicFiles.Count > 0;
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn(
                                        ex,
                                        $"Error checking UniPlaySong music files for game: {game.Name}"
                                    );
                                    return false;
                                }
                            })
                            .OrderBy(g => g.Name)
                            .ToList();

                        // Update cache
                        cachedGamesWithMusic = gamesWithMusic;
                        lastGameScanTime = DateTime.Now;

                        logger.Info($"Found {gamesWithMusic.Count} games with UniPlaySong music files");
                        return gamesWithMusic;
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info("Get games with UniPlaySong music operation cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to get games with UniPlaySong music");
                errorHandler.HandlePlaybackError(ex, "Game Discovery");
                return new List<Game>();
            }
        }

        /// <summary>
        /// Gets detailed music information for a specific game.
        /// </summary>
        public async Task<GameMusicInfo> GetGameMusicInfoAsync(
            Game game,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            if (game == null)
            {
                logger.Warn("GetGameMusicInfoAsync called with null game");
                return null;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.Debug($"Getting music info for game: {game.Name}");

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var musicDirectory = MusicFileHelper.GetGameMusicPath(playniteApi, game);
                        var musicFiles = MusicFileHelper.GetGameMusicFiles(playniteApi, game);

                        var totalSize = 0L;
                        foreach (var file in musicFiles)
                        {
                            try
                            {
                                var fileInfo = new FileInfo(file);
                                if (fileInfo.Exists)
                                {
                                    totalSize += fileInfo.Length;
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Warn(ex, $"Error getting file size for: {file}");
                            }
                        }

                        return new GameMusicInfo
                        {
                            Game = game,
                            MusicDirectory = musicDirectory,
                            MusicFiles = musicFiles,
                            TotalTracks = musicFiles.Count,
                            TotalSizeBytes = totalSize,
                            LastScanned = DateTime.Now,
                        };
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Get game music info cancelled for: {game.Name}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to get music info for game: {game.Name}");
                errorHandler.HandlePlaybackError(ex, game.Name);
                return null;
            }
        }

        /// <summary>
        /// Filters games by name or other criteria.
        /// </summary>
        public async Task<List<Game>> FilterGamesAsync(
            string searchText,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var allGames = await GetGamesWithMusicAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return allGames;
                }

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var searchLower = searchText.ToLowerInvariant();
                        return allGames
                            .Where(game =>
                                game.Name.ToLowerInvariant().Contains(searchLower)
                                || (
                                    game.Description?.ToLowerInvariant().Contains(searchLower)
                                    ?? false
                                )
                                || (
                                    game.Developers?.Any(d =>
                                        d.Name.ToLowerInvariant().Contains(searchLower)
                                    ) ?? false
                                )
                                || (
                                    game.Publishers?.Any(p =>
                                        p.Name.ToLowerInvariant().Contains(searchLower)
                                    ) ?? false
                                )
                            )
                            .ToList();
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Filter games operation cancelled for search: {searchText}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to filter games with search: {searchText}");
                return new List<Game>();
            }
        }

        /// <summary>
        /// Finds game by exact name match.
        /// </summary>
        public async Task<Game> FindGameByNameAsync(
            string gameName,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            if (string.IsNullOrEmpty(gameName))
                return null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var allGames = await GetGamesWithMusicAsync(cancellationToken);

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        return allGames.FirstOrDefault(game =>
                            string.Equals(game.Name, gameName, StringComparison.OrdinalIgnoreCase)
                        );
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Find game by name cancelled for: {gameName}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to find game by name: {gameName}");
                return null;
            }
        }

        /// <summary>
        /// Helper struct to replace tuple for .NET Framework 4.6.2 compatibility.
        /// </summary>
        private struct GameFileInfo
        {
            public Game Game { get; set; }
            public string FilePath { get; set; }
        }

        /// <summary>
        /// Validates if a file is a valid audio file.
        /// </summary>
        private bool IsValidAudioFile(string filePath)
        {
            try
            {
                var supportedExtensions = new[] { ".mp3", ".wav", ".flac", ".aac", ".wma" };
                var extension = Path.GetExtension(filePath).ToLowerInvariant();

                return supportedExtensions.Contains(extension)
                    && File.Exists(filePath)
                    && new FileInfo(filePath).Length > 1024; // At least 1KB
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets music files for a specific game.
        /// </summary>
        public async Task<List<string>> GetMusicFilesAsync(
            Game game,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            if (game == null)
                return new List<string>();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return MusicFileHelper.GetGameMusicFiles(playniteApi, game);
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Get UniPlaySong music files cancelled for game: {game.Name}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to get UniPlaySong music files for game: {game.Name}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets music directory path for a game.
        /// </summary>
        public async Task<string> GetMusicDirectoryAsync(
            Game game,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            if (game == null)
                return string.Empty;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return MusicFileHelper.GetGameMusicPath(playniteApi, game);
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Get music directory cancelled for game: {game.Name}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to get music directory for game: {game.Name}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates music file existence and accessibility.
        /// </summary>
        public async Task<bool> ValidateMusicFileAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            if (string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            return File.Exists(filePath)
                                && new FileInfo(filePath).Length > 0
                                && IsValidAudioFile(filePath);
                        }
                        catch
                        {
                            return false;
                        }
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Validate music file cancelled for: {filePath}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to validate music file: {filePath}");
                return false;
            }
        }

        /// <summary>
        /// Scans for new music files in game directory.
        /// </summary>
        public async Task<List<string>> ScanForMusicFilesAsync(
            Game game,
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            if (game == null)
                return new List<string>();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.Debug($"Scanning for UniPlaySong music files in game: {game.Name}");

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var musicDirectory = MusicFileHelper.GetGameMusicPath(playniteApi, game);
                        if (
                            string.IsNullOrEmpty(musicDirectory)
                            || !Directory.Exists(musicDirectory)
                        )
                        {
                            return new List<string>();
                        }

                        var supportedExtensions = new[] { ".mp3", ".wav", ".flac", ".aac", ".wma" };
                        var musicFiles = new List<string>();

                        var allFiles = Directory
                            .GetFiles(musicDirectory, "*.*", SearchOption.AllDirectories)
                            .Where(file =>
                                supportedExtensions.Contains(
                                    Path.GetExtension(file).ToLowerInvariant()
                                )
                            )
                            .ToList();

                        int processedCount = 0;
                        foreach (var file in allFiles)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (IsValidAudioFile(file))
                            {
                                musicFiles.Add(file);
                            }

                            processedCount++;
                            progress?.Report((processedCount * 100) / allFiles.Count);
                        }

                        logger.Debug($"Found {musicFiles.Count} UniPlaySong music files in game: {game.Name}");
                        return musicFiles;
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Scan for UniPlaySong music files cancelled for game: {game.Name}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to scan for UniPlaySong music files in game: {game.Name}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Scans all games for music files with progress reporting.
        /// </summary>
        public async Task<Dictionary<Game, List<string>>> ScanAllGamesForMusicAsync(
            IProgress<ScanProgress> progress = null,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            var results = new Dictionary<Game, List<string>>();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.Info("Starting batch scan for UniPlaySong music files across all games...");

                var allGames = playniteApi.Database.Games.ToList();
                int processedGames = 0;
                int totalFoundFiles = 0;

                foreach (var game in allGames)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    progress?.Report(
                        new ScanProgress
                        {
                            TotalGames = allGames.Count,
                            ProcessedGames = processedGames,
                            CurrentGame = game.Name,
                            Status = $"Scanning {game.Name}...",
                            FoundMusicFiles = totalFoundFiles,
                        }
                    );

                    try
                    {
                        var musicFiles = await ScanForMusicFilesAsync(
                            game,
                            null,
                            cancellationToken
                        );
                        if (musicFiles.Count > 0)
                        {
                            results[game] = musicFiles;
                            totalFoundFiles += musicFiles.Count;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"Failed to scan game: {game.Name}");
                    }

                    processedGames++;
                }

                progress?.Report(
                    new ScanProgress
                    {
                        TotalGames = allGames.Count,
                        ProcessedGames = processedGames,
                        Status = "Scan completed",
                        FoundMusicFiles = totalFoundFiles,
                    }
                );

                logger.Info(
                    $"Batch scan completed. Found music in {results.Count} games ({totalFoundFiles} total files)"
                );
                return results;
            }
            catch (OperationCanceledException)
            {
                logger.Info("Batch scan operation cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to perform batch scan for UniPlaySong music files");
                return results;
            }
        }

        /// <summary>
        /// Validates music files for multiple games.
        /// </summary>
        public async Task<Dictionary<Game, List<string>>> ValidateAllMusicFilesAsync(
            IProgress<ValidationProgress> progress = null,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            var results = new Dictionary<Game, List<string>>();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.Info("Starting batch validation of UniPlaySong music files...");

                var gamesWithMusic = await GetGamesWithMusicAsync(cancellationToken);
                var allMusicFiles = new List<GameFileInfo>();

                // Collect all music files using custom struct instead of tuple
                foreach (var game in gamesWithMusic)
                {
                    var musicFiles = await GetMusicFilesAsync(game, cancellationToken);
                    foreach (var file in musicFiles)
                    {
                        allMusicFiles.Add(new GameFileInfo { Game = game, FilePath = file });
                    }
                }

                int processedFiles = 0;
                int validFiles = 0;
                int invalidFiles = 0;

                foreach (var gameFile in allMusicFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    progress?.Report(
                        new ValidationProgress
                        {
                            TotalFiles = allMusicFiles.Count,
                            ProcessedFiles = processedFiles,
                            CurrentFile = Path.GetFileName(gameFile.FilePath),
                            Status = $"Validating {Path.GetFileName(gameFile.FilePath)}...",
                            ValidFiles = validFiles,
                            InvalidFiles = invalidFiles,
                        }
                    );

                    try
                    {
                        var isValid = await ValidateMusicFileAsync(
                            gameFile.FilePath,
                            cancellationToken
                        );
                        if (isValid)
                        {
                            if (!results.ContainsKey(gameFile.Game))
                            {
                                results[gameFile.Game] = new List<string>();
                            }
                            results[gameFile.Game].Add(gameFile.FilePath);
                            validFiles++;
                        }
                        else
                        {
                            invalidFiles++;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"Error validating file: {gameFile.FilePath}");
                        invalidFiles++;
                    }

                    processedFiles++;
                }

                progress?.Report(
                    new ValidationProgress
                    {
                        TotalFiles = allMusicFiles.Count,
                        ProcessedFiles = processedFiles,
                        Status = "Validation completed",
                        ValidFiles = validFiles,
                        InvalidFiles = invalidFiles,
                    }
                );

                logger.Info(
                    $"Batch validation completed. Valid: {validFiles}, Invalid: {invalidFiles}"
                );
                return results;
            }
            catch (OperationCanceledException)
            {
                logger.Info("Batch validation operation cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to perform batch validation of UniPlaySong music files");
                return results;
            }
        }

        /// <summary>
        /// Gets statistics about music library.
        /// </summary>
        public async Task<MusicLibraryStatistics> GetLibraryStatisticsAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.Debug("Calculating music library statistics...");

                return await Task.Run(
                    async () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var statistics = new MusicLibraryStatistics { LastScanned = DateTime.Now };

                        var allGames = playniteApi.Database.Games.ToList();
                        statistics.TotalGames = allGames.Count;

                        var gamesWithMusic = await GetGamesWithMusicAsync(cancellationToken);
                        statistics.GamesWithMusic = gamesWithMusic.Count;

                        long totalSize = 0;
                        int totalFiles = 0;
                        string largestGame = string.Empty;
                        int largestGameFileCount = 0;
                        var fileTypeDistribution = new Dictionary<string, int>();

                        foreach (var game in gamesWithMusic)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var musicFiles = await GetMusicFilesAsync(game, cancellationToken);
                            totalFiles += musicFiles.Count;

                            if (musicFiles.Count > largestGameFileCount)
                            {
                                largestGameFileCount = musicFiles.Count;
                                largestGame = game.Name;
                            }

                            foreach (var file in musicFiles)
                            {
                                try
                                {
                                    var fileInfo = new FileInfo(file);
                                    if (fileInfo.Exists)
                                    {
                                        totalSize += fileInfo.Length;

                                        var extension = fileInfo.Extension.ToLowerInvariant();
                                        if (fileTypeDistribution.ContainsKey(extension))
                                        {
                                            fileTypeDistribution[extension]++;
                                        }
                                        else
                                        {
                                            fileTypeDistribution[extension] = 1;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn(ex, $"Error processing file: {file}");
                                }
                            }
                        }

                        statistics.TotalMusicFiles = totalFiles;
                        statistics.TotalSizeBytes = totalSize;
                        statistics.LargestGame = largestGame;
                        statistics.LargestGameFileCount = largestGameFileCount;
                        statistics.FileTypeDistribution = fileTypeDistribution;

                        logger.Debug(
                            $"Library statistics calculated: {totalFiles} files, {totalSize} bytes"
                        );
                        return statistics;
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info("Get library statistics operation cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to calculate library statistics");
                return new MusicLibraryStatistics { LastScanned = DateTime.Now };
            }
        }

        /// <summary>
        /// Gets or creates music metadata directory for game.
        /// </summary>
        public async Task<string> EnsureMusicMetadataDirectoryAsync(
            Game game,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            if (game == null)
                return string.Empty;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var metadataDir = Path.Combine(
                            playniteApi.Paths.ExtensionsDataPath,
                            "OstPlayer_f3b0c108-5212-4b34-a303-47e859b31a92",
                            "Metadata",
                            game.Id.ToString()
                        );

                        if (!Directory.Exists(metadataDir))
                        {
                            Directory.CreateDirectory(metadataDir);
                            logger.Debug($"Created metadata directory for game: {game.Name}");
                        }

                        return metadataDir;
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Ensure metadata directory cancelled for game: {game.Name}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to ensure metadata directory for game: {game.Name}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if game has cached metadata.
        /// </summary>
        public async Task<bool> HasCachedMetadataAsync(
            Game game,
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            if (game == null)
                return false;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var metadataDir = Path.Combine(
                            playniteApi.Paths.ExtensionsDataPath,
                            "OstPlayer_f3b0c108-5212-4b34-a303-47e859b31a92",
                            "Metadata",
                            game.Id.ToString()
                        );

                        return Directory.Exists(metadataDir)
                            && Directory.GetFiles(metadataDir, "*.json").Length > 0;
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Check cached metadata cancelled for game: {game.Name}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to check cached metadata for game: {game.Name}");
                return false;
            }
        }

        /// <summary>
        /// Cleans up orphaned metadata files.
        /// </summary>
        public async Task CleanupOrphanedMetadataAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.Info("Starting cleanup of orphaned metadata files...");

                await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var metadataBaseDir = Path.Combine(
                            playniteApi.Paths.ExtensionsDataPath,
                            "OstPlayer_f3b0c108-5212-4b34-a303-47e859b31a92",
                            "Metadata"
                        );

                        if (!Directory.Exists(metadataBaseDir))
                            return;

                        var allGameIds = playniteApi
                            .Database.Games.Select(g => g.Id.ToString())
                            .ToHashSet();
                        var orphanedDirs = new List<string>();

                        foreach (var gameDir in Directory.GetDirectories(metadataBaseDir))
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var gameId = Path.GetFileName(gameDir);
                            if (!allGameIds.Contains(gameId))
                            {
                                orphanedDirs.Add(gameDir);
                            }
                        }

                        foreach (var orphanedDir in orphanedDirs)
                        {
                            try
                            {
                                Directory.Delete(orphanedDir, true);
                                logger.Debug($"Removed orphaned metadata directory: {orphanedDir}");
                            }
                            catch (Exception ex)
                            {
                                logger.Warn(
                                    ex,
                                    $"Failed to remove orphaned directory: {orphanedDir}"
                                );
                            }
                        }

                        logger.Info(
                            $"Cleanup completed. Removed {orphanedDirs.Count} orphaned directories"
                        );
                    },
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.Info("Cleanup orphaned metadata operation cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to cleanup orphaned metadata");
                throw;
            }
        }

        /// <summary>
        /// Configures game service with settings.
        /// </summary>
        public void Configure(OstPlayerSettings settings)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            try
            {
                logger.Info("Configuring GameService with new settings...");

                // Apply cache settings if needed
                if (settings != null)
                {
                    // Invalidate cache if needed based on settings
                    cachedGamesWithMusic = null;
                    lastGameScanTime = DateTime.MinValue;
                }

                logger.Info("GameService configuration updated successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error configuring GameService");
                throw;
            }
        }

        /// <summary>
        /// Performs health check on service components.
        /// </summary>
        public async Task<ServiceHealthStatus> CheckHealthAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(GameService));

            var healthStatus = new ServiceHealthStatus
            {
                IsHealthy = true,
                Status = "Healthy",
                Details = new Dictionary<string, object>(),
            };

            try
            {
                var startTime = DateTime.UtcNow;

                // Check Playnite API availability
                try
                {
                    var gameCount = playniteApi.Database.Games.Count;
                    healthStatus.Details["TotalGames"] = gameCount.ToString();

                    var gamesWithMusic = await GetGamesWithMusicAsync(cancellationToken);
                    healthStatus.Details["GamesWithMusic"] = gamesWithMusic.Count.ToString();
                }
                catch (Exception ex)
                {
                    healthStatus.Details["PlayniteAPI"] = $"Error: {ex.Message}";
                    healthStatus.IsHealthy = false;
                }

                // Check file system access
                try
                {
                    var configPath = playniteApi.Paths.ConfigurationPath;
                    var canAccess = Directory.Exists(configPath);
                    healthStatus.Details["FileSystemAccess"] = canAccess
                        ? "Available"
                        : "Unavailable";

                    if (!canAccess)
                    {
                        healthStatus.IsHealthy = false;
                    }
                }
                catch (Exception ex)
                {
                    healthStatus.Details["FileSystemAccess"] = $"Error: {ex.Message}";
                    healthStatus.IsHealthy = false;
                }

                healthStatus.ResponseTime = DateTime.UtcNow - startTime;

                if (!healthStatus.IsHealthy)
                {
                    healthStatus.Status = "Degraded";
                }
            }
            catch (Exception ex)
            {
                healthStatus.IsHealthy = false;
                healthStatus.Status = "Unhealthy";
                healthStatus.Details["Error"] = ex.Message;
            }

            return healthStatus;
        }

        /// <summary>
        /// Releases all resources used by the GameService.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the GameService and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                try
                {
                    logger.Info("Disposing GameService...");

                    // Clear cache
                    cachedGamesWithMusic = null;

                    logger.Info("GameService disposed successfully");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error disposing GameService");
                }
                finally
                {
                    disposed = true;
                }
            }
        }
    }
}
