using System.Text.Json;
using BlazorOllamaGlobal.Client.Models.DataTransfer;
using BlazorOllamaGlobal.Services.DataAccess;
using Newtonsoft.Json.Linq;

namespace BlazorOllamaGlobal.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DapperController : ControllerBase
{
    private readonly IDapperService _dapperService;
    private readonly ILogger<DapperController> _logger;

    public DapperController(IDapperService dapperService, ILogger<DapperController> logger)
    {
        _dapperService = dapperService;
        _logger = logger;
    }

    [HttpPost("query")]
    public async Task<ActionResult<List<T>>> QueryAsync<T>([FromBody] QueryRequest request)
    {
        try
        {
            var result = await _dapperService.QueryAsync<T>(request.Query, request.Parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query");
            return StatusCode(500, "An error occurred while executing the query");
        }
    }



    [HttpPost("upsert")]
    public async Task<ActionResult<object>> UpsertAsync([FromBody] UpsertRequest request)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.EntityTypeName))
            {
                Type entityType = Type.GetType(request.EntityTypeName) ?? 
                                  AppDomain.CurrentDomain.GetAssemblies()
                                      .SelectMany(a => a.GetTypes())
                                      .FirstOrDefault(t => t.FullName == request.EntityTypeName || 
                                                           t.Name == request.EntityTypeName);
                
                object result = null;
                if (entityType != null)
                {
                    // Convert using Newtonsoft.Json for more reliable conversion
                    JObject jObject;
                
                    if (request.Entity is JsonElement jsonElement)
                    {
                        // Convert from System.Text.Json to Newtonsoft
                        string json = jsonElement.GetRawText();
                        jObject = JObject.Parse(json);
                    }
                    else
                    {
                        // Convert to JObject
                        jObject = JObject.FromObject(request.Entity);
                    }
                
                    // Convert to strongly typed object
                    result = await _dapperService.UpsertAsyncDynamic(
                        jObject.ToObject(entityType), 
                        request.DataTableName, 
                        request.IdColumnName, 
                        request.TableGeneratesIDs);
                }
                return Ok(result);
            }
            else
            {
                return BadRequest("Entity type name is required");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting data: {Message}", ex.Message);
            return StatusCode(500, "An error occurred while upserting data");
        }
    }
}