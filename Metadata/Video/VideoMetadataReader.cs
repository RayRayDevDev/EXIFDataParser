using MetadataExtractor;
using MetadataExtractor.Formats.QuickTime;

namespace EXIFDataParser.Metadata.Video
{
    internal class VideoMetadataReader
    {
       public static DateTime GetDateTimeTakenFromVideo(string filePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                foreach (var directory in directories)
                {
                    if (directory is QuickTimeMovieHeaderDirectory)
                    {
                        if (directory.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out DateTime videoCreationTime))
                        {
                            return videoCreationTime;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore any exceptions while reading video metadata
            }

            DateTime fileCreationTime = File.GetCreationTime(filePath);
            DateTime lastWriteTime = File.GetLastWriteTime(filePath);
            DateTime fallbackTime = fileCreationTime < lastWriteTime ? fileCreationTime : lastWriteTime;

            if (fallbackTime.Year > 2001)
            {
                return fallbackTime;
            }
            else
            {
                return new DateTime(2001, 1, 1, 0, 0, 0);
            }
        }
    }
}
