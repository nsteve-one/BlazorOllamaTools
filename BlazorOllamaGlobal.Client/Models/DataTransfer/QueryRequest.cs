namespace BlazorOllamaGlobal.Client.Models.DataTransfer;

public class QueryRequest
{
    public string Query { get; set; }
    public object Parameters { get; set; }
    public string EntityTypeName { get; set; }
}