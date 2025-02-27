using BlazorOllamaGlobal.Client.Models.ToolModels;
using BlazorOllamaGlobal.Client.Services.DataAccess;

namespace BlazorOllamaGlobal.Client.Services.Tiles;

public class NoteService
{
    public readonly DapperClient _dapperClient;
    public NoteService(DapperClient dapperClient)
    {
        _dapperClient = dapperClient;
    }

    public async Task<Note> SaveNote(Note note)
    {
        try
        {
            var resultid = await _dapperClient.UpsertAsync(note, "Note", "Id");
            var resultString = resultid.ToString();
            note.Id = Guid.Parse(resultString);
            return note;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    public async Task<List<Note>> QueryNotes(string searchTerm)
    {
        try
        {
            return await _dapperClient.QueryAsync<Note>(
                "SELECT * FROM Note WHERE LOWER(Title) LIKE @searchTerm", 
                new { searchTerm = $"%{searchTerm.Trim().ToLower()}%" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Note>> GetAllNotes()
    {
        try
        {
            return await _dapperClient.QueryAsync<Note>(
                "SELECT * FROM Note;");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}