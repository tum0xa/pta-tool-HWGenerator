namespace LibraryCompare.Core.Interfaces
{
    public interface ILibraryElement
    {
        string Name { get; set; }
        string Author { get; set; }
        string FolderPath { get; set; }
    }
}
