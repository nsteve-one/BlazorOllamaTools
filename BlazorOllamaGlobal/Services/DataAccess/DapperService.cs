using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using BlazorOllamaGlobal.Client.Attributes;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorOllamaGlobal.Services.DataAccess;

public class DapperService : IDapperService
{
    private readonly ILogger<DapperService> _logger;
    private readonly IConfiguration _config;
    public DapperService(ILogger<DapperService> logger, IConfiguration Configuration)
    {
        _logger = logger;
        _config = Configuration;
    }
    public List<T> Query<T>(string query, object parameters = null)
    {
        try
        {
            using (SqlConnection Connection = new SqlConnection(_config.GetConnectionString("MSSQLConnection")))
            {
                return Connection.Query<T>(query, parameters).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return new List<T>();
        }
    }

    public T QueryFirstOrDefault<T>(string query, object parameters = null)
    {
        try
        {
            using (SqlConnection Connection = new SqlConnection(_config.GetConnectionString("MSSQLConnection")))
            {
                return Connection.Query<T>(query, parameters).FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return default(T);
        }
    }

    public async Task<List<T>> QueryAsync<T>(string query, object parameters = null)
    {
        try
        {

            using (SqlConnection Connection = new SqlConnection(_config.GetConnectionString("MSSQLConnection")))
            {
                var List = await Connection.QueryAsync<T>(query, parameters);

                return List.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return new List<T>();
        }
    }
    
    public async Task<IEnumerable<object>> QueryAsyncDynamic(string query, object parameters, Type entityType)
    {
        // Convert JsonElement parameters to a proper dictionary
        var dapperParameters = ConvertToDapperParameters(parameters);
        
        // Get the generic method definition
        var genericMethodDefinition = this.GetType().GetMethods()
            .Where(m => m.Name == nameof(QueryAsync) && m.IsGenericMethod)
            .Where(m => {
                var methodParams = m.GetParameters();
                return methodParams.Length == 2 &&
                       methodParams[0].ParameterType == typeof(string);
            })
            .FirstOrDefault();

        if (genericMethodDefinition == null)
        {
            throw new InvalidOperationException("Could not find the appropriate QueryAsync method");
        }

        // Create the specific generic method with our entity type
        var methodInfo = genericMethodDefinition.MakeGenericMethod(entityType);

        // Invoke it with the parameters and properly converted parameters
        dynamic task = methodInfo.Invoke(this, new object[] { query, dapperParameters });
        
        // Await the task to get the result
        var typedResult = await task;
        
        // Convert the result to a list of objects
        List<object> result = new List<object>();
        foreach (var item in typedResult)
        {
            result.Add(item);
        }
        
        return result;
    }

private object ConvertToDapperParameters(object parameters)
{
    // If it's null, return null
    if (parameters == null) return null;
    
    // If it's a JsonElement, convert it to a proper object
    if (parameters is System.Text.Json.JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            // Create a dictionary to hold the parameters
            var dict = new Dictionary<string, object>();
            
            // Get all properties from the JsonElement
            foreach (var property in jsonElement.EnumerateObject())
            {
                // Convert each JsonElement value to an appropriate .NET type
                dict[property.Name] = ConvertJsonElementValue(property.Value);
            }
            
            return dict;
        }
    }
    
    // If it's already a compatible type, return it as is
    return parameters;
}

private object ConvertJsonElementValue(System.Text.Json.JsonElement element)
{
    switch (element.ValueKind)
    {
        case System.Text.Json.JsonValueKind.String:
            return element.GetString();
        case System.Text.Json.JsonValueKind.Number:
            if (element.TryGetInt32(out int intValue))
                return intValue;
            if (element.TryGetInt64(out long longValue))
                return longValue;
            if (element.TryGetDouble(out double doubleValue))
                return doubleValue;
            return element.GetRawText();
        case System.Text.Json.JsonValueKind.True:
            return true;
        case System.Text.Json.JsonValueKind.False:
            return false;
        case System.Text.Json.JsonValueKind.Null:
            return null;
        default:
            return element.GetRawText();
    }
}

    public async Task<T> QueryFirstOrDefaultAsync<T>(string query, object parameters = null)
    {
        try
        {
            using (SqlConnection Connection = new SqlConnection(_config.GetConnectionString("MSSQLConnection")))
            {
                var Item = await Connection.QueryAsync<T>(query, parameters);
                return Item.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return default(T);
        }
    }

    public async Task<int> ExecuteScalarAsync(string query, object parameters = null)
    {
        try
        {
            using (SqlConnection Connection = new SqlConnection(_config.GetConnectionString("MSSQLConnection")))
            {
                var count = await Connection.ExecuteScalarAsync<int>(query, parameters);
                return count;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return 0;
        }
    }
    
    public async Task<object> UpsertAsync<T>(SqlConnection Connection, T entity, string dataTableName, string idColumnName, SqlTransaction transaction = null, bool tableGeneratesIDs = true)
    {
        try
        {
            // Get the type and properties of the entity
            var type = typeof(T);
            var properties = type.GetProperties()
                .Where(property =>
                    !property.GetCustomAttributes(false)
                        .OfType<IgnoreUpsert>()
                        .Any());

            // Get the type of the id column
            var idColumnProperty = properties.FirstOrDefault(p => p.Name == idColumnName);
            if (idColumnProperty == null)
            {
                throw new ArgumentException($"The idColumnName '{idColumnName}' was not found in the properties of the entity.");
            }
            var idColumnType = idColumnProperty.PropertyType;

            // If tableGeneratesIDs is false, ensure that the id value is not null or default
            if (!tableGeneratesIDs)
            {
                var idValue = idColumnProperty.GetValue(entity);
                if (idValue == null || IsDefaultValue(idValue))
                {
                    throw new ArgumentException($"The entity's '{idColumnName}' value must not be null or default when 'tableGeneratesIDs' is false.");
                }
            }

            // Build the SQL query for upsert
            var sql = new StringBuilder();
            sql.Append($"DECLARE @outputTbl TABLE ({idColumnName} {GetSqlColumnType(idColumnType)}); ");
            sql.Append($"MERGE {dataTableName} AS target USING (VALUES (");

            // Append the values of the entity properties
            var parameters = new DynamicParameters();

            foreach (var property in properties)
            {
                // Check if the property has a DisplayName attribute
                var columnNameAttribute = property.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute;
                var columnName = columnNameAttribute?.Name ?? property.Name;

                // Append the parameter name to the SQL query
                sql.Append($"@{columnName}, ");

                // Add the parameter to the DynamicParameters object
                var propertyValue = property.GetValue(entity);
                if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                {
                    // Convert it to a nullable datetime and check if it has a valid value
                    var dateValue = propertyValue as DateTime?;
                    if (dateValue.HasValue && dateValue.Value >= new DateTime(1753, 1, 1) && dateValue.Value <= new DateTime(9999, 12, 31))
                    {
                        // Use the value as is
                        parameters.Add(columnName, dateValue.Value, DbType.DateTime);
                    }
                    else
                    {
                        // Use null instead
                        parameters.Add(columnName, null, DbType.DateTime);
                    }
                }
                else
                {
                    // Use the value as is for other types
                    parameters.Add(columnName, propertyValue);
                }
            }

            // Remove the last comma and space
            sql.Length -= 2;

            // Append the rest of the SQL query
            sql.Append(@$")) AS source ({string.Join(", ", properties.Select(p => {
                var columnNameAttribute = p.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute;
                return columnNameAttribute?.Name ?? p.Name;
            }))}) ON target.{idColumnName} = source.{idColumnName} ");
            sql.Append("WHEN MATCHED THEN UPDATE SET ");
            foreach (var property in properties)
            {
                if (property.Name != idColumnName) // Skip the id column
                {
                    var columnNameAttribute = property.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute;
                    var columnName = columnNameAttribute?.Name ?? property.Name;
                    sql.Append($"target.{columnName} = source.{columnName}, ");
                }
            }
            // Remove the last comma and space
            sql.Length -= 2;

            // Only use nonIdentityProperties because the db table should contain a constraint to add a new id
            var nonIdentityProperties = properties.Where(p => p.Name != idColumnName);

            // Determine the properties to insert based on tableGeneratesIDs
            IEnumerable<PropertyInfo> insertProperties;

            if (tableGeneratesIDs)
            {
                insertProperties = nonIdentityProperties;
            }
            else
            {
                insertProperties = properties; // Include idColumnName in insertProperties
            }

            sql.Append(@$" WHEN NOT MATCHED THEN INSERT ({string.Join(", ", insertProperties.Select(p => {
                var columnNameAttribute = p.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute;
                return columnNameAttribute?.Name ?? p.Name;
            }))}) VALUES ({string.Join(", ", insertProperties.Select(p => $"source.{((ColumnAttribute)p.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault())?.Name ?? p.Name}"))})");

            // Add OUTPUT clause to get the inserted or updated id
            sql.Append($" OUTPUT inserted.{idColumnName} INTO @outputTbl; ");
            sql.Append($"SELECT {idColumnName} FROM @outputTbl;");

            // Execute the query using Dapper and fetch the id
            object ID = await Connection.ExecuteScalarAsync(sql.ToString(), parameters, transaction: transaction);
            return ID;
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    
    public async Task<object> UpsertAsyncDynamic(object entity, string dataTableName, string idColumnName, bool tableGeneratesIDs = true)
    {
        // Get the entity type
        Type entityType = entity.GetType();
    
        // Create an array of parameter types for the exact method signature we want
        Type[] parameterTypes = new Type[] { 
            entityType,                // T entity
            typeof(string),            // string dataTableName
            typeof(string),            // string idColumnName
            typeof(bool)               // bool tableGeneratesIDs
        };
    
        // Get the generic method definition - this avoids any ambiguity with overloads
        var genericMethodDefinition = this.GetType().GetMethods()
            .Where(m => m.Name == nameof(UpsertAsync) && m.IsGenericMethod)
            .Where(m => {
                var parameters = m.GetParameters();
                return parameters.Length == 4 &&
                       parameters[1].ParameterType == typeof(string) && 
                       parameters[2].ParameterType == typeof(string) &&
                       parameters[3].ParameterType == typeof(bool);
            })
            .FirstOrDefault();
    
        if (genericMethodDefinition == null)
        {
            throw new InvalidOperationException("Could not find the appropriate UpsertAsync method");
        }
    
        // Create the specific generic method with our entity type
        var methodInfo = genericMethodDefinition.MakeGenericMethod(entityType);
    
        // Invoke it with the parameters
        return await (Task<object>)methodInfo.Invoke(this, new object[] { 
            entity, dataTableName, idColumnName, tableGeneratesIDs 
        });
    }
    
    public async Task<object> UpsertAsync<T>(T entity, string dataTableName, string idColumnName, bool tableGeneratesIDs = true)
    {
        try
        {
            using (SqlConnection Connection = new SqlConnection(_config.GetConnectionString("MSSQLConnection")))
            {
                // Get the type and properties of the entity
                var type = typeof(T);
                var properties = type.GetProperties()
                    .Where(property =>
                        !property.GetCustomAttributes(false)
                            .OfType<IgnoreUpsert>()
                            .Any());

                // Get the type of the id column
                var idColumnProperty = properties.FirstOrDefault(p => p.Name == idColumnName);
                if (idColumnProperty == null)
                {
                    throw new ArgumentException(
                        $"The idColumnName '{idColumnName}' was not found in the properties of the entity.");
                }

                var idColumnType = idColumnProperty.PropertyType;

                // If tableGeneratesIDs is false, ensure that the id value is not null or default
                if (!tableGeneratesIDs)
                {
                    var idValue = idColumnProperty.GetValue(entity);
                    if (idValue == null || IsDefaultValue(idValue))
                    {
                        throw new ArgumentException(
                            $"The entity's '{idColumnName}' value must not be null or default when 'tableGeneratesIDs' is false.");
                    }
                }

                // Build the SQL query for upsert
                var sql = new StringBuilder();
                sql.Append($"DECLARE @outputTbl TABLE ({idColumnName} {GetSqlColumnType(idColumnType)}); ");
                sql.Append($"MERGE {dataTableName} AS target USING (VALUES (");

                // Append the values of the entity properties
                var parameters = new DynamicParameters();

                foreach (var property in properties)
                {
                    // Check if the property has a DisplayName attribute
                    var columnNameAttribute =
                        property.GetCustomAttributes(typeof(ColumnAttribute), false)
                            .FirstOrDefault() as ColumnAttribute;
                    var columnName = columnNameAttribute?.Name ?? property.Name;

                    // Append the parameter name to the SQL query
                    sql.Append($"@{columnName}, ");

                    // Add the parameter to the DynamicParameters object
                    var propertyValue = property.GetValue(entity);
                    if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                    {
                        // Convert it to a nullable datetime and check if it has a valid value
                        var dateValue = propertyValue as DateTime?;
                        if (dateValue.HasValue && dateValue.Value >= new DateTime(1753, 1, 1) &&
                            dateValue.Value <= new DateTime(9999, 12, 31))
                        {
                            // Use the value as is
                            parameters.Add(columnName, dateValue.Value, DbType.DateTime);
                        }
                        else
                        {
                            // Use null instead
                            parameters.Add(columnName, null, DbType.DateTime);
                        }
                    }
                    else
                    {
                        // Use the value as is for other types
                        parameters.Add(columnName, propertyValue);
                    }
                }

                // Remove the last comma and space
                sql.Length -= 2;

                // Append the rest of the SQL query
                sql.Append(@$")) AS source ({string.Join(", ", properties.Select(p => {
                    var columnNameAttribute = p.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute;
                    return columnNameAttribute?.Name ?? p.Name;
                }))}) ON target.{idColumnName} = source.{idColumnName} ");
                sql.Append("WHEN MATCHED THEN UPDATE SET ");
                foreach (var property in properties)
                {
                    if (property.Name != idColumnName) // Skip the id column
                    {
                        var columnNameAttribute =
                            property.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as
                                ColumnAttribute;
                        var columnName = columnNameAttribute?.Name ?? property.Name;
                        sql.Append($"target.{columnName} = source.{columnName}, ");
                    }
                }

                // Remove the last comma and space
                sql.Length -= 2;

                // Only use nonIdentityProperties because the db table should contain a constraint to add a new id
                var nonIdentityProperties = properties.Where(p => p.Name != idColumnName);

                // Determine the properties to insert based on tableGeneratesIDs
                IEnumerable<PropertyInfo> insertProperties;

                if (tableGeneratesIDs)
                {
                    insertProperties = nonIdentityProperties;
                }
                else
                {
                    insertProperties = properties; // Include idColumnName in insertProperties
                }

                sql.Append(@$" WHEN NOT MATCHED THEN INSERT ({string.Join(", ", insertProperties.Select(p => {
                    var columnNameAttribute = p.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute;
                    return columnNameAttribute?.Name ?? p.Name;
                }))}) VALUES ({string.Join(", ", insertProperties.Select(p => $"source.{((ColumnAttribute)p.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault())?.Name ?? p.Name}"))})");

                // Add OUTPUT clause to get the inserted or updated id
                sql.Append($" OUTPUT inserted.{idColumnName} INTO @outputTbl; ");
                sql.Append($"SELECT {idColumnName} FROM @outputTbl;");

                // Execute the query using Dapper and fetch the id
                object ID = await Connection.ExecuteScalarAsync(sql.ToString(), parameters);
                return ID;
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    // Helper method to check if value is default for its type
    private bool IsDefaultValue(object value)
    {
        if (value == null)
            return true;

        var type = value.GetType();

        if (type.IsValueType)
            return value.Equals(Activator.CreateInstance(type));

        return false;
    }


    public async Task<bool> RunSQLAsync(string query, object parameters = null)
    {
        try
        {
            using (SqlConnection Connection = new SqlConnection(_config.GetConnectionString("MSSQLConnection")))
            {
                await Connection.ExecuteAsync(query, parameters);
                return true;
            }
        }
        catch (Exception ex)
        {

            _logger.LogError(ex.Message);
            return false;
        }

    }

    public bool RunSQL(string query, object parameters = null)
    {
        try
        {
            using (SqlConnection Connection = new SqlConnection(Environment.GetEnvironmentVariable("sqldb_connection")))
            {
                Connection.ExecuteAsync(query, parameters);
                return true;
            }
        }
        catch (Exception ex)
        {

            _logger.LogError(ex.Message);
            return false;
        }
    }

    private string GetSqlColumnType(Type type)
    {
        if (type == typeof(int))
        {
            return "int";
        }
        else if (type == typeof(Guid))
        {
            return "uniqueidentifier";
        }
        else
        {
            throw new ArgumentException($"Unsupported ID column type '{type}'.");
        }
    }

    public async Task<bool> ExecuteStoredProcedureAsync(string spName, DynamicParameters parameters = null)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("MSSQLConnection")))
            {
                await conn.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql,
        Func<TFirst, TSecond, TReturn> map,
        object parameters = null,
        string splitOn = "id")
    {
        try
        {
            using (SqlConnection Connection = new SqlConnection(_config.GetConnectionString("MSSQLConnection")))
            {
                var list = await Connection.QueryAsync(sql, map, parameters, splitOn: splitOn);
                return list.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return new List<TReturn>();
        }
    }

    public List<T> Query<T>(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null)
    {
        try
        {
            return Connection.Query<T>(query, parameters, transaction: transaction).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while executing the query.", ex);
            //return new List<T>();

        }
    }

    public T QueryFirstOrDefault<T>(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null)
    {
        try
        {
            return Connection.QueryFirstOrDefault<T>(query, parameters, transaction: transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while executing the query.", ex);
        }
    }

    public async Task<List<T>> QueryAsync<T>(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null)
    {
        try
        {
            var list = await Connection.QueryAsync<T>(query, parameters, transaction: transaction);
            return list.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while executing the query.", ex);
        }
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null)
    {
        try
        {
            return await Connection.QueryFirstOrDefaultAsync<T>(query, parameters, transaction: transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while executing the query.", ex);
        }
    }

    public async Task<bool> RunSQLAsync(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null)
    {
        try
        {
            await Connection.ExecuteAsync(query, parameters, transaction: transaction);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while executing the query.", ex);
        }
    }

    public bool RunSQL(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null)
    {
        try
        {
            Connection.Execute(query, parameters, transaction: transaction);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while executing the query.", ex);
        }
    }

    public async Task<bool> ExecuteStoredProcedureAsync(SqlConnection conn, string spName, DynamicParameters parameters = null, SqlTransaction transaction = null)
    {
        try
        {
            await conn.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure, transaction: transaction);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while executing the query.", ex);
        }
    }

    public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(SqlConnection Connection, string sql,
        Func<TFirst, TSecond, TReturn> map,
        object parameters = null,
        string splitOn = "id", SqlTransaction transaction = null)
    {
        try
        {
            var list = await Connection.QueryAsync(sql, map, parameters, splitOn: splitOn, transaction: transaction);
            return list.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while executing the query.", ex);
        }
    }
}