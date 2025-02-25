using BlazorOllamaGlobal.Client.Models.ToolModels;

namespace BlazorOllamaGlobal.Client.Services.Tiles;

public class NoteService
{
    public async Task SaveNote(Note note)
    {
        // Save note to database
        // For now, just print it to the console
        Console.WriteLine($"Note saved: {note.Title} - {note.Content}");
    }
}