
namespace EXIFDataParser.Main
{
    internal class RParserApp
    {
        private Dictionary<string, string>? _originalFileNames;
        private int _renamedFilesCount;

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("1. Rename files based on Exif date");
                Console.WriteLine("2. Undo renaming");
                Console.WriteLine("3. Exit");
                Console.WriteLine("Enter your choice (1/2/3):");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            RenameFiles();
                            break;
                        case 2:
                            UndoRenaming();
                            break;
                        case 3:
                            Console.WriteLine("Exiting...");
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please try again.");
                }
            }

        }
        private void RenameFiles()
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

            _originalFileNames = new Dictionary<string, string>();
            _renamedFilesCount = Utilities.FileManagement.RenamingUtility.RenameFilesBasedOnExifDate(folderPath, _originalFileNames, recheckAllFiles);

            if (_renamedFilesCount > 0)
            {
                Console.WriteLine("Do you want to undo renaming? (y/n)");
                string undoChoice = Console.ReadLine();

                if (undoChoice.ToLower() == "y")
                {
                    UndoRenaming();
                }
            }
        }
        private void UndoRenaming()
        {
            Utilities.FileManagement.UndoRenamingUtility.UndoRenaming(_originalFileNames);
        }
    }
}

