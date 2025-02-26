namespace BlazorOllamaGlobal.Client.Models.DataTransfer;

public class UpsertRequest
{
    public object Entity { get; set; }
    public string DataTableName { get; set; }
    public string IdColumnName { get; set; }
    public bool TableGeneratesIDs { get; set; } = true;
    public string EntityTypeName { get; set; }
}