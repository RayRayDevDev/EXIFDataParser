using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.IO;

namespace EXIFDataParser.Metadata.Exif
{
    internal class ExifMetadataReader
    {
        public static DateTime GetFileDateTimeOriginalFromExif(string filePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);
                foreach (var directory in directories)
                {
                    if (directory is ExifSubIfdDirectory)
                    {
                        if (directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime dateTaken))
                        {
                            return dateTaken;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore any exceptions while reading metadata
            }

            // Fallback to file's creation time or last write time if metadata is not available
            DateTime creationTime = File.GetCreationTime(filePath);
            DateTime lastWriteTime = File.GetLastWriteTime(filePath);
            DateTime fallbackTime = creationTime < lastWriteTime ? creationTime : lastWriteTime;

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
