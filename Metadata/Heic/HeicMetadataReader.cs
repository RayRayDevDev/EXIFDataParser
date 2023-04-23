using ImageMagick;

namespace EXIFDataParser.Metadata.Heic
{
    internal class HeicMetadataReader
    {
       public static DateTime GetDateTimeOriginalFromHeic(string filePath)
        {
            try
            {
                using (var image = new MagickImage(filePath))
                {
                    if (image.GetAttribute("EXIF:DateTimeOriginal") is string dateStr)
                    {
                        if (DateTime.TryParseExact(dateStr, "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime dateTaken))
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
