namespace EXIFDataParser.Utilities.FileManagement
{
    internal class RenamingUtility
    {
        public static int RenameFilesBasedOnExifDate(string folderPath, Dictionary<string, string> originalFileNames, bool recheckAllFileNames)
        {
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories);

            int defaultDateCounter = 0;

            int renamedFilesCount = 0;

            foreach (var file in files)
            {
                if (!recheckAllFileNames && FileNameUtilities.IsNamingSchemeConforming(Path.GetFileNameWithoutExtension(file)))
                {
                    Console.WriteLine($"Skipping '{Path.GetFileName(file)}' as it already conforms to the naming scheme.");
                    continue;
                }

                DateTime fileDate = GetFileDateTime(file);
                string newFileName;

                if (fileDate == new DateTime(2001, 1, 1, 0, 0, 0))
                {
                    fileDate = fileDate.AddSeconds(defaultDateCounter);
                    defaultDateCounter++;
                }

                newFileName = fileDate.ToString("yyyy-MM-dd HH_mm_ss");
                string newFilePath = Path.Combine(Path.GetDirectoryName(file), newFileName + Path.GetExtension(file));
                int conflictCounter = 0;
                while (File.Exists(newFilePath))
                {
                    conflictCounter++;
                    DateTime incrementedDate = fileDate.AddSeconds(conflictCounter);
                    if (incrementedDate.Minute == fileDate.Minute && incrementedDate.Hour == fileDate.Hour && incrementedDate.Day == fileDate.Day)
                    {
                        fileDate = incrementedDate;
                        newFileName = fileDate.ToString("yyyy-MM-dd HH_mm_ss");
                        newFilePath = Path.Combine(Path.GetDirectoryName(file), newFileName + Path.GetExtension(file));
                    }
                    else if (conflictCounter >= 59)
                    {
                        // Reset the seconds to 0 and start incrementing again
                        fileDate = fileDate.AddSeconds(-59);
                        conflictCounter = 0;
                    }
                }
                Console.WriteLine($"Renaming '{Path.GetFileName(file)}' to '{Path.GetFileName(newFilePath)}'");
                try
                {
                    if (!originalFileNames.ContainsKey(newFilePath))
                    {
                        originalFileNames.Add(newFilePath, file);
                        File.Move(file, newFilePath);
                        renamedFilesCount++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    continue;
                }
            }

            return renamedFilesCount;
        }
        static DateTime GetFileDateTime(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".tiff" || fileExtension == ".png")
            {
                return Metadata.Exif.ExifMetadataReader.GetFileDateTimeOriginalFromExif(filePath);
            }
            else if (fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi" || fileExtension == ".mkv")
            {
                return Metadata.Video.VideoMetadataReader.GetDateTimeTakenFromVideo(filePath);
            }
            else if (fileExtension == ".heic")
            {
                return Metadata.Heic.HeicMetadataReader.GetDateTimeOriginalFromHeic(filePath);
            }

            return new DateTime(2001, 1, 1, 0, 0, 0);
        }
    }
}
