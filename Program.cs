using ImageMagick;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.QuickTime;
using System.Text.RegularExpressions;


namespace ExifDateRenamer
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter the folder path:");
                string folderPath = Console.ReadLine();

                if (!System.IO.Directory.Exists(folderPath))
                {
                    Console.WriteLine("Invalid folder path. Exiting.");
                    return;
                }

                Console.WriteLine("Do you want to recheck all file names? (y/n)");
                string recheckChoice = Console.ReadLine();
                bool recheckAllFiles = recheckChoice.ToLower() == "y";

                Dictionary<string, string> originalFileNames = new Dictionary<string, string>();
                int renamedFilesCount = RenameFilesBasedOnExifDate(folderPath, originalFileNames, recheckAllFiles);

                if (renamedFilesCount > 0)
                {
                    Console.WriteLine("Undo renaming? (y/n)");
                    string undoChoice = Console.ReadLine();

                    if (undoChoice.ToLower() == "y")
                    {
                        UndoRenaming(originalFileNames);
                    }
                }

                Console.WriteLine("Do you want to process another folder? (y/n)");
                string anotherFolderChoice = Console.ReadLine();

                if (anotherFolderChoice.ToLower() != "y")
                {
                    break;
                }
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static int RenameFilesBasedOnExifDate(string folderPath, Dictionary<string, string> originalFileNames, bool recheckAllFileNames)
        {
            var files = System.IO.Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories);
            int defaultDateCounter = 0;
            Regex namingSchemeRegex = new Regex(@"^\d{4}-\d{2}-\d{2} \d{2}_\d{2}_\d{2}(\s\(\d+\))?");

            int renamedFilesCount = 0;

            foreach (var file in files)
            {
                if (!recheckAllFileNames && namingSchemeRegex.IsMatch(Path.GetFileNameWithoutExtension(file)))
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
                bool conflictFound;
                int conflictCounter = 0;
                while (System.IO.File.Exists(newFilePath))
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
                        System.IO.File.Move(file, newFilePath);
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
                return GetDateTimeOriginalFromExif(filePath);
            }
            else if (fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi" || fileExtension == ".mkv")
            {
                return GetDateTimeTakenFromVideo(filePath);
            }
            else if (fileExtension == ".heic")
            {
                return GetDateTimeOriginalFromHeic(filePath);
            }

            return new DateTime(2001, 1, 1, 0, 0, 0);
        }

        static DateTime GetDateTimeOriginalFromHeic(string filePath)
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



        static DateTime GetDateTimeOriginalFromExif(string filePath)
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
    



    static DateTime GetDateTimeTakenFromVideo(string filePath)
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


        static void UndoRenaming(Dictionary<string, string> originalFileNames)
        {
            foreach (var entry in originalFileNames)
            {
                string currentFilePath = entry.Key;
                string originalFilePath = entry.Value;

                if (File.Exists(currentFilePath))
                {
                    try
                    {
                        Console.WriteLine($"Renaming '{Path.GetFileName(currentFilePath)}' back to '{Path.GetFileName(originalFilePath)}'");
                        System.IO.File.Move(currentFilePath, originalFilePath);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

    }
}