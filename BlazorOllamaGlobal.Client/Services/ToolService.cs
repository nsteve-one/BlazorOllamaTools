using System.Text.Json;
using System.Text.Json.Nodes;
using BlazorOllamaGlobal.Client.Components.Tiles;
using BlazorOllamaGlobal.Client.Models.Chats;
using BlazorOllamaGlobal.Client.Models.Tiles;
using BlazorOllamaGlobal.Client.Models.ToolModels;
using BlazorOllamaGlobal.Client.Services.Tiles;

namespace BlazorOllamaGlobal.Client.Services;

public class ToolService
{
    
    public readonly TileService tileService;
    public readonly NoteService noteService;
    public ToolService(TileService tileService, NoteService noteService)
    {
        this.tileService = tileService;
        this.noteService = noteService;
    }
    public async Task<string> RunToolCalled(string toolName, JsonObject args)
    {
        try
        {
            string toolResult = string.Empty;
            switch (toolName)
            {
                case "GetCurrentTime":
                    toolResult = "The local time is " + GetCurrentTime();
                    tileService.RequestTile(new TimeTileModel(GetCurrentTime()));
                    break;
                case "CreateNewNote":
                    toolResult = "success";
                    var NewNote = new Note() { Id = Guid.NewGuid() };
                    
                    NewNote.Title = args["title"]?.GetValue<string>() ?? "";
                    NewNote.Content = args["content"]?.GetValue<string>() ?? "";
                    
                    tileService.RequestTile(new NoteTileModel(NewNote));
                    break;
                case "SaveNote":
                    var note =
                        (tileService.ActiveTiles.FirstOrDefault(x =>
                            x.IsExiting is not true && x is NoteTileModel) as NoteTileModel)?.GetNote();
                    if (note is not null)
                    {
                        await noteService.SaveNote(note);
                        toolResult = "Note saved successfully.";
                    }
                    else
                    {
                        toolResult = "No note found to save.";
                    }
                    break;
                case "SearchNotes":
                    var titleQuery = args["title"]?.GetValue<string>() ?? "";
                    var notesResult = await noteService.QueryNotes(titleQuery);
                    if (notesResult is not null)
                    {
                        if (notesResult.Count == 1)
                        {
                            toolResult = $"Single note found named {notesResult[0].Title} and is displayed to the user successfully.";
                            tileService.RequestTile(new NoteTileModel(notesResult[0]));
                        }
                        else if (notesResult.Count > 1)
                        {
                            toolResult = $"{notesResult.Count} notes found. Please refine your search parameters.";
                        }
                        else
                        {
                            toolResult = "No note found. Please refine your search parameters.";
                        }
                    }
                    else
                    {
                        toolResult = "No note found. Please refine your search parameters.";
                    }
                    break;
                case "EditCurrentNote":
                    var noteTile = tileService.ActiveTiles.FirstOrDefault(x =>
                        x.IsExiting is not true && x is NoteTileModel) as NoteTileModel;
                    if (noteTile is not null)
                    {
                        var noteToEdit = noteTile.GetNote();
                        noteToEdit.Title = args["title"]?.GetValue<string>() ?? noteToEdit.Title;
                        noteToEdit.Content = args["content"]?.GetValue<string>() ?? noteToEdit.Content;
                        toolResult = "Note edited successfully.";
                        noteTile.HandleNoteSaved(noteToEdit);
                    }
                    else
                    {
                        toolResult = "No note found to edit.";
                    }
                    break;
                default:
                    toolResult = "Unknown tool";
                    break;
            }
            
            
            return $"Tool {toolName} was successfully called. Tool result: {toolResult}.";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("T");
    }

    public async Task<List<ToolDefinition>> GetToolDefinitions()
    {
        var toolDefinitions = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Function = new ToolFunction
                {
                    Name = "GetCurrentTime",
                    Description = "Returns the current time on the server",
                    Parameters = new { } // Define parameters schema if needed
                }
            },
            new ToolDefinition()
            {
                Function = new ToolFunction()
                {
                    Name = "CreateNewNote",
                    Description = "Displays a new note to the user with an optional title and content. Do not add content unless explicitly requested by the user.",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            Title = new
                            {
                                type = "string",
                                description = "The note title.",
                            },
                            Content = new
                            {
                                type = "string",
                                description = "The note content. Content should be in html format.",
                            }
                        }
                    }
                }
            },
            new ToolDefinition
            {
                Function = new ToolFunction
                {
                    Name = "SearchNotes",
                    Description = "Searches or looks for notes and returns a list of saved notes from the server and displays the result to the user.",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            Title = new
                            {
                                type = "string",
                                description = "A search term for the title of the saved notes.",
                            }
                        }
                    }
                }
            },
        };
        
        var activeTileTools = tileService.ActiveTiles.Where(x => x.IsExiting is not true).SelectMany(t => t.GetTileTools()).ToList();
        toolDefinitions.AddRange(activeTileTools);
        return toolDefinitions;
    }
}