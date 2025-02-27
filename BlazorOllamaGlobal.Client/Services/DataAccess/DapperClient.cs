using System.Net.Http.Json;
using System.Text.Json;
using BlazorOllamaGlobal.Client.Models.DataTransfer;

namespace BlazorOllamaGlobal.Client.Services.DataAccess;

public class DapperClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public DapperClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<List<T>> QueryAsync<T>(string query, object parameters = null)
    {
        var request = new QueryRequest
        {
            Query = query,
            Parameters = parameters,
            EntityTypeName = typeof(T).AssemblyQualifiedName
        };

        var response = await _httpClient.PostAsJsonAsync("api/dapper/query", request);
    
        if (response.IsSuccessStatusCode)
        {
            // Get the raw JSON content
            var json = await response.Content.ReadAsStringAsync();
        
            // Deserialize manually to ensure proper typing
            return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);
        }
    
        throw new HttpRequestException($"Query failed: {response.ReasonPhrase}");
    }

    public async Task<object> UpsertAsync(object entity, string dataTableName, string idColumnName, bool tableGeneratesIDs = true)
    {
        var request = new UpsertRequest
        {
            Entity = entity,
            DataTableName = dataTableName,
            IdColumnName = idColumnName,
            TableGeneratesIDs = tableGeneratesIDs,
            EntityTypeName = entity?.GetType().AssemblyQualifiedName
        };

        var response = await _httpClient.PostAsJsonAsync($"api/dapper/upsert", request);
    
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
        }
    
        throw new HttpRequestException($"Upsert failed: {response.ReasonPhrase}");
    }
}