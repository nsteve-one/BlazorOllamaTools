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
}