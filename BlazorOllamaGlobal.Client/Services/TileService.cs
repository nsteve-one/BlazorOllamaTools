using BlazorOllamaGlobal.Client.Models.Tiles;
using Microsoft.AspNetCore.Components;

namespace BlazorOllamaGlobal.Client.Services;

public class TileService
{
    public List<ITileModel> ActiveTiles { get; set; }

    public TileService()
    {
        ActiveTiles = new List<ITileModel>();
    }
    
    public event Action<ITileModel> OnTileRequested;

    public void RequestTile(ITileModel tileModel)
    {
        OnTileRequested?.Invoke(tileModel);
    }
}

