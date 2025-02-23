using Microsoft.AspNetCore.Components;

namespace BlazorOllamaGlobal.Client.Services;

public class TileService
{
    public event Action<RenderFragment> OnTileRequested;

    public void RequestTile(RenderFragment content)
    {
        OnTileRequested?.Invoke(content);
    }
}

