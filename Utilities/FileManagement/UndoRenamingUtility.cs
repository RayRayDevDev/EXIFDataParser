namespace EXIFDataParser.Utilities.FileManagement
{
    internal class UndoRenamingUtility
    {
        public static void UndoRenaming(Dictionary<string, string> originalFileNames)
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
                        File.Move(currentFilePath, originalFilePath);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }
}
