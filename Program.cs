using System.Diagnostics;
using System.Text.RegularExpressions;
using ExifLib;
using MediaInfo;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor;
using TagLib;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

                Dictionary<string, string> originalFileNames = new Dictionary<string, string>();
                RenameFilesBasedOnExifDate(folderPath, originalFileNames);

                Console.WriteLine("Do you want to undo the renaming? (y/n)");
                string undoChoice = Console.ReadLine();

                if (undoChoice.ToLower() == "y")
                {
                    UndoRenaming(originalFileNames);
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

        static void RenameFilesBasedOnExifDate(string folderPath, Dictionary<string, string> originalFileNames)
        {
            var files = System.IO.Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories);
            int defaultDateCounter = 0;
            Regex namingSchemeRegex = new Regex(@"^\d{4}-\d{2}-\d{2} \d{2}_\d{2}_\d{2}(\s\(\d+\))?");

            foreach (var file in files)
            {
                if (namingSchemeRegex.IsMatch(Path.GetFileNameWithoutExtension(file)))
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

                if (System.IO.File.Exists(newFilePath))
                {
                    int counter = 1;
                    while (System.IO.File.Exists(Path.Combine(Path.GetDirectoryName(file), newFileName + $" ({counter})" + Path.GetExtension(file))))
                    {
                        counter++;
                    }
                    newFilePath = Path.Combine(Path.GetDirectoryName(file), newFileName + $" ({counter})" + Path.GetExtension(file));
                }

                Console.WriteLine($"Renaming '{Path.GetFileName(file)}' to '{Path.GetFileName(newFilePath)}'");
                System.IO.File.Move(file, newFilePath);
            }
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

            return new DateTime(2001, 1, 1, 0, 0, 0);
        }
        static DateTime GetDateTimeOriginalFromExif(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new ExifReader(stream))
                {
                    if (reader.GetTagValue(ExifTags.DateTimeOriginal, out DateTime dateTaken))
                    {
                        return dateTaken;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore any exceptions while reading EXIF data
            }

            return new DateTime(2001, 1, 1, 0, 0, 0);
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
                        if (directory.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out DateTime creationTime))
                        {
                            return creationTime;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore any exceptions while reading video metadata
            }

            return new DateTime(2001, 1, 1, 0, 0, 0);
        }
        static void UndoRenaming(Dictionary<string, string> originalFileNames)
        {
            foreach (var entry in originalFileNames)
            {
                string currentFilePath = entry.Key;
                string originalFilePath = entry.Value;

                if (System.IO.File.Exists(currentFilePath))
                {
                    Console.WriteLine($"Renaming '{Path.GetFileName(currentFilePath)}' back to '{Path.GetFileName(originalFilePath)}'");
                    System.IO.File.Move(currentFilePath, originalFilePath);
                }
            }
        }
    }
}