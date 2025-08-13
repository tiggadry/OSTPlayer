// ====================================================================
// FILE: Mp3MetadataReader.cs
// PROJECT: OstPlayer - Playnite Plugin for Game Soundtrack Management
// MODULE: Utils
// LOCATION: Utils/
// VERSION: 1.0.0
// CREATED: 2025-08-06
// UPDATED: 2025-08-06
// AUTHOR: TiggAdry
// ====================================================================
//
// PURPOSE:
// Static utility class for reading MP3 metadata using TagLibSharp library.
// Extracts comprehensive metadata including ID3 tags, cover art, and duration
// information, mapping them to the internal TrackMetadataModel structure.
//
// FEATURES:
// - Complete ID3v1/ID3v2 tag reading
// - Cover art extraction with BitmapImage conversion
// - Track numbering and total tracks parsing
// - Duration calculation and formatting
// - TRCK frame parsing for complex track numbering
// - Defensive error handling for corrupted files
//
// DEPENDENCIES:
// - TagLib# (TagLibSharp for MP3 metadata reading)
// - System.Windows.Media.Imaging (BitmapImage for cover art)
// - System.IO (MemoryStream for image processing)
// - OstPlayer.Models.TrackMetadataModel (output model)
//
// UTILITY FUNCTIONS:
// - ReadMetadata: Main entry point for metadata extraction
// - SetCoverImage: Extracts and converts album artwork
// - SetBasicTags: Reads standard ID3 tags (title, artist, etc.)
// - SetTrackNumbers: Parses track numbering information
// - GetTrckFrame: Extracts raw TRCK frame data
// - ParseTrckFrame: Parses "track/total" format strings
//
// METADATA MAPPING:
// - Title: ID3 title or filename fallback
// - Artist: Joined performers list
// - Album: Album name
// - Year: Release year as string
// - Genre: Joined genres list
// - Comment: ID3 comment field
// - Duration: Formatted as "mm:ss"
// - TrackNumber/TotalTracks: From Track/TrackCount or TRCK frame
//
// PERFORMANCE NOTES:
// - Single-pass file reading
// - Efficient image memory management
// - Minimal string allocations
// - Fast fallback mechanisms
//
// LIMITATIONS:
// - MP3 format only (no FLAC, OGG support)
// - Single cover image extraction
// - Limited error recovery for corrupted tags
// - No custom tag support
//
// FUTURE REFACTORING:
// TODO: Add support for additional audio formats (FLAC, OGG, WAV)
// TODO: Extract multiple cover images and artwork types
// TODO: Add custom tag reading capabilities
// TODO: Implement metadata validation and cleanup
// TODO: Add async metadata reading for large files
// TODO: Implement metadata caching for performance
// TODO: Add support for embedded lyrics extraction
// TODO: Extract interface for multiple format support
// CONSIDER: Plugin architecture for different metadata readers
// IDEA: Machine learning for metadata quality improvement
//
// TESTING:
// - Unit tests with various MP3 file types
// - Performance tests with large files
// - Error handling tests with corrupted files
// - Memory usage tests for cover art extraction
//
// ERROR HANDLING:
// - Try-catch blocks around all TagLib operations
// - Graceful degradation for missing metadata
// - Null safety throughout the pipeline
// - Silent failure with null return
//
// COMPATIBILITY:
// - .NET Framework 4.6.2
// - C# 7.3
// - TagLib# 2.x
// - WPF BitmapImage support
//
// CHANGELOG:
// 2025-08-06 v1.0.0 - Initial implementation with comprehensive MP3 metadata reading
// ====================================================================

using System.IO;
using System.Windows.Media.Imaging;
using OstPlayer.Models;

namespace OstPlayer.Utils
{
    /// <summary>
    /// Provides static methods for reading MP3 metadata and mapping it to TrackMetadataModel.
    /// THREAD SAFETY: All methods are static and thread-safe (no shared state)
    /// ERROR HANDLING: Defensive programming with graceful degradation on failures
    /// PERFORMANCE: Single-pass reading with minimal memory allocation
    /// </summary>
    public static class Mp3MetadataReader
    {
        /// <summary>
        /// Main entry point: Reads metadata from the given MP3 file path.
        /// WORKFLOW: File validation -> TagLib creation -> Metadata extraction -> Model mapping
        /// ERROR STRATEGY: Returns null on any failure (corrupted files, access issues, etc.)
        /// PERFORMANCE: Single file operation with minimal resource usage
        /// </summary>
        /// <param name="fullPath">Full file system path to the MP3 file</param>
        /// <returns>TrackMetadataModel with extracted data, or null if reading fails</returns>
        public static TrackMetadataModel ReadMetadata(string fullPath)
        {
            // INPUT VALIDATION: Check file existence before expensive TagLib operations
            // EARLY EXIT: Prevents TagLib exceptions and provides fast failure path
            if (!System.IO.File.Exists(fullPath))
                return null;
                
            try
            {
                // TAGLIB INITIALIZATION: Use TagLibSharp to open the MP3 file
                // RESOURCE MANAGEMENT: TagLib.File implements IDisposable, but using statement 
                // not used here for simplicity - TagLib handles cleanup internally
                var file = TagLib.File.Create(fullPath);
                var metadata = new TrackMetadataModel();
                
                // EXTRACTION PIPELINE: Process metadata in logical order
                // ORDER: Cover art first (memory intensive), then tags, then track numbers
                SetCoverImage(file, metadata);      // Extract embedded album artwork
                SetBasicTags(file, metadata, fullPath);  // Extract standard ID3 tags  
                SetTrackNumbers(file, metadata);    // Extract track numbering info
                
                return metadata;
            }
            catch
            {
                // GRACEFUL DEGRADATION: Catch all exceptions and return null
                // REASONS FOR FAILURE:
                // - Corrupted MP3 file structure
                // - Unsupported MP3 encoding
                // - File access permissions
                // - TagLib internal errors
                // - Memory allocation failures
                // TODO: Add proper logging when ILogger is available
                return null;
            }
        }

        /// <summary>
        /// Extracts the cover image from the MP3 file and sets it in the metadata.
        /// ALGORITHM: Uses first available picture from ID3 Pictures array
        /// MEMORY MANAGEMENT: Creates BitmapImage with proper disposal pattern
        /// IMAGE PROCESSING: Converts raw bytes to WPF-compatible BitmapImage
        /// </summary>
        /// <param name="file">TagLib file object with loaded MP3 data</param>
        /// <param name="metadata">Target metadata model to populate</param>
        private static void SetCoverImage(TagLib.File file, TrackMetadataModel metadata)
        {
            // AVAILABILITY CHECK: Ensure Pictures array exists and has content
            // DEFENSIVE: TagLib.Tag.Pictures can be null or empty for files without artwork
            if (file.Tag.Pictures != null && file.Tag.Pictures.Length > 0)
            {
                // SELECTION STRATEGY: Use first picture (usually the primary album cover)
                // ALTERNATIVES: Could implement logic to prefer specific picture types
                // (Front Cover, Back Cover, Artist, etc.) based on Picture.Type property
                var pic = file.Tag.Pictures[0];
                
                // MEMORY STREAM: Convert byte array to stream for BitmapImage
                // DISPOSAL: Using statement ensures MemoryStream is properly disposed
                using (var ms = new MemoryStream(pic.Data.Data))
                {
                    // BITMAPIMAGE CREATION: WPF-compatible image format
                    var img = new BitmapImage();
                    img.BeginInit();
                    
                    // CACHING STRATEGY: OnLoad caches the image data immediately
                    // BENEFIT: Allows MemoryStream to be disposed while preserving image
                    // ALTERNATIVE: OnDemand would keep file reference longer
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = ms;
                    img.EndInit();
                    
                    // FREEZE: Makes BitmapImage thread-safe and improves performance
                    // REQUIREMENT: Essential for cross-thread access in WPF applications
                    img.Freeze();
                    metadata.Cover = img;
                }
            }
            // IMPLICIT: If no pictures found, metadata.Cover remains null (default)
        }

        /// <summary>
        /// Extracts basic tags (title, artist, album, year, genre, comment, duration) from the MP3 file.
        /// FALLBACK STRATEGY: Uses filename if title tag is missing
        /// ARRAY HANDLING: Safely joins string arrays with comma separation
        /// VALIDATION: Handles missing or invalid tag values gracefully
        /// </summary>
        /// <param name="file">TagLib file object with loaded MP3 data</param>
        /// <param name="metadata">Target metadata model to populate</param>
        /// <param name="fullPath">Original file path for filename fallback</param>
        private static void SetBasicTags(TagLib.File file, TrackMetadataModel metadata, string fullPath)
        {
            // TITLE EXTRACTION: Primary from ID3 tag, fallback to filename
            // FALLBACK LOGIC: If Tag.Title is null/empty, use filename without extension
            // UI BENEFIT: Ensures every track has a displayable name
            metadata.Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(fullPath);
            
            // ARTIST HANDLING: Join multiple performers into single string
            // ARRAY SAFETY: Null array is replaced with empty array to prevent Join() exceptions
            // FORMAT: "Artist1, Artist2, Artist3" for multiple performers
            metadata.Artist = string.Join(", ", file.Tag.Performers ?? new string[0]);
            
            // ALBUM: Direct mapping with null safety
            // EMPTY STRING: Use empty string instead of null for consistent UI binding
            metadata.Album = file.Tag.Album ?? string.Empty;
            
            // YEAR CONVERSION: Convert uint year to string with validation
            // VALIDATION: Only convert if year > 0 (TagLib uses 0 for missing year)
            // FORMAT: Simple ToString() conversion (e.g., "2023")
            metadata.Year = file.Tag.Year > 0 ? file.Tag.Year.ToString() : string.Empty;
            
            // GENRE HANDLING: Join multiple genres similar to artists
            // MULTIPLE GENRES: Some files have multiple genre tags
            // FORMAT: "Rock, Alternative, Indie" for multiple genres
            metadata.Genre = string.Join(", ", file.Tag.Genres ?? new string[0]);
            
            // COMMENT: Direct mapping with null safety
            metadata.Comment = file.Tag.Comment ?? string.Empty;
            
            // DURATION FORMATTING: Convert TimeSpan to user-friendly format
            // FORMAT: "mm:ss" (e.g., "3:45" for 3 minutes 45 seconds)
            // PRECISION: Seconds are rounded down (3:45.7 becomes "3:45")
            metadata.Duration = file.Properties.Duration.ToString("mm\\:ss");
        }

        /// <summary>
        /// Extracts track number and total tracks from the MP3 file.
        /// STRATEGY: Try standard TagLib properties first, fallback to raw TRCK frame
        /// TRCK FRAME: Handles complex "track/total" format in ID3v2 tags
        /// EDGE CASES: Manages various numbering formats and missing data
        /// </summary>
        /// <param name="file">TagLib file object with loaded MP3 data</param>
        /// <param name="metadata">Target metadata model to populate</param>
        private static void SetTrackNumbers(TagLib.File file, TrackMetadataModel metadata)
        {
            // PRIMARY EXTRACTION: Use TagLib's built-in track number properties
            // PROPERTIES: Tag.Track (current track) and Tag.TrackCount (total tracks)
            metadata.TrackNumber = file.Tag.Track;
            metadata.TotalTracks = file.Tag.TrackCount;
            
            // SUCCESS CHECK: If both values are available, we're done
            // OPTIMIZATION: Avoids expensive TRCK frame parsing when not needed
            if (metadata.TrackNumber != 0 && metadata.TotalTracks != 0)
                return;
                
            // FALLBACK STRATEGY: Parse raw TRCK frame for missing data
            // USE CASE: Some MP3 files have TRCK frame but TagLib doesn't parse it correctly
            // EXAMPLE: TRCK frame might contain "02/12" but TagLib only sees Track=2, TrackCount=0
            string trck = GetTrckFrame(file);
            if (!string.IsNullOrEmpty(trck))
                ParseTrckFrame(trck, metadata);
        }

        /// <summary>
        /// Gets the raw TRCK frame (track number/total) from ID3v2 tags.
        /// ID3V2 STRUCTURE: TRCK frame contains track number and optionally total tracks
        /// FORMAT: Can be "5", "5/12", "05/12", etc.
        /// FRAME ACCESS: Direct access to ID3v2 frame data bypassing TagLib's parsing
        /// </summary>
        /// <param name="file">TagLib file object with loaded MP3 data</param>
        /// <returns>Raw TRCK frame content as string, or null if not found</returns>
        private static string GetTrckFrame(TagLib.File file)
        {
            // ID3V2 TAG ACCESS: Get ID3v2-specific tag interface
            // CASTING: Required because TagLib.File.Tag is generic interface
            var id3v2 = file.GetTag(TagLib.TagTypes.Id3v2) as TagLib.Id3v2.Tag;
            if (id3v2 == null)
                return null;  // File doesn't have ID3v2 tags
                
            // FRAME ITERATION: Search through all TRCK frames
            // MULTIPLE FRAMES: Theoretically possible to have multiple TRCK frames
            foreach (var frame in id3v2.GetFrames("TRCK"))
            {
                // FRAME TYPE CHECK: Ensure this is a text information frame
                // SAFETY: TRCK should always be TextInformationFrame, but cast for safety
                var trckFrame = frame as TagLib.Id3v2.TextInformationFrame;
                if (trckFrame != null)
                    return trckFrame.ToString();  // Return first found TRCK frame content
            }
            return null;  // No TRCK frame found
        }

        /// <summary>
        /// Parses the TRCK frame (e.g. "2/12") and sets TrackNumber and TotalTracks.
        /// FORMATS SUPPORTED: "5", "05", "5/12", "05/12", " 5 / 12 ", etc.
        /// EDGE CASES: Handles leading zeros, whitespace, missing total, malformed data
        /// SAFETY: Graceful handling of unexpected formats without exceptions
        /// </summary>
        /// <param name="trck">Raw TRCK frame content</param>
        /// <param name="metadata">Target metadata model to populate</param>
        private static void ParseTrckFrame(string trck, TrackMetadataModel metadata)
        {
            // INPUT SANITIZATION: Remove spaces and trim for consistent parsing
            // CLEANUP: Handles formats like " 5 / 12 " -> "5/12"
            var cleaned = trck.Replace(" ", string.Empty).Trim();
            
            // SPLIT OPERATION: Separate track number from total tracks
            // DELIMITER: "/" is standard separator in TRCK frames
            var parts = cleaned.Split('/');
            
            // VARIABLE INITIALIZATION: Prepare parsing variables
            uint num = 0, total = 0;
            
            // PART EXTRACTION: Get individual components with bounds checking
            string part0 = parts.Length > 0 ? parts[0].TrimStart('0') : null;  // Track number
            string part1 = parts.Length > 1 ? parts[1].TrimStart('0') : null;  // Total tracks
            
            // LEADING ZERO HANDLING: TrimStart('0') converts "05" -> "5"
            // EDGE CASE: "00" becomes empty string, which is handled by null check
            // BENEFIT: Prevents uint.Parse() issues with leading zeros
            
            // TRACK NUMBER PARSING: Convert first part to track number
            // VALIDATION: Only set if parsing succeeds and result is not empty
            // EDGE CASE: Empty string after TrimStart('0') indicates "00" -> set to 0
            if (!string.IsNullOrEmpty(part0) && uint.TryParse(part0, out num))
                metadata.TrackNumber = num;
                
            // TOTAL TRACKS PARSING: Convert second part to total tracks
            // OPTIONAL: Total tracks part might not exist (format: "5" instead of "5/12")
            if (!string.IsNullOrEmpty(part1) && uint.TryParse(part1, out total))
                metadata.TotalTracks = total;
                
            // ERROR HANDLING: Malformed data is silently ignored
            // EXAMPLES OF IGNORED DATA: "abc/def", "5/", "/12", "", "5/0"
            // PHILOSOPHY: Partial success is better than complete failure
        }
    }
}
