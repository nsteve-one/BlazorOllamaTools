using System.Data.SqlClient;
using Dapper;

namespace BlazorOllamaGlobal.Services.DataAccess;

public interface IDapperService
{
    public List<T> Query<T>(string query, object parameters = null);
    List<T> Query<T>(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null);
    public T QueryFirstOrDefault<T>(string query, object parameters = null);
    public T QueryFirstOrDefault<T>(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null);
    public Task<List<T>> QueryAsync<T>(string query, object parameters = null);
    public Task<IEnumerable<object>> QueryAsyncDynamic(string query, object parameters, Type entityType);
    public Task<List<T>> QueryAsync<T>(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null);
    public Task<T> QueryFirstOrDefaultAsync<T>(string query, object parameters = null);
    public Task<int> ExecuteScalarAsync(string query, object parameters = null);
    public Task<T> QueryFirstOrDefaultAsync<T>(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null);
    public  Task<bool> RunSQLAsync(string query, object parameters = null);
    public  Task<bool> RunSQLAsync(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null);
    public bool RunSQL(string query, object parameters = null);
    public bool RunSQL(SqlConnection Connection, string query, object parameters = null, SqlTransaction transaction = null);

    public Task<object> UpsertAsync<T>(SqlConnection Connection, T entity, string dataTableName, string idColumnName, SqlTransaction transaction = null, bool tableGeneratesIDs = true);
    public Task<object> UpsertAsyncDynamic(object entity, string dataTableName, string idColumnName, bool tableGeneratesIDs = true);
    public Task<object> UpsertAsync<T>(T entity, string dataTableName, string idColumnName, bool tableGeneratesIDs = true);
    public Task<bool> ExecuteStoredProcedureAsync(string spName, DynamicParameters parameters = null);
    public Task<bool> ExecuteStoredProcedureAsync(SqlConnection Connection, string spName, DynamicParameters parameters = null, SqlTransaction transaction = null);

    public Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql,
        Func<TFirst, TSecond, TReturn> map,
        object parameters = null,
        string splitOn = "id");
    public Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(SqlConnection Connection, string sql,
        Func<TFirst, TSecond, TReturn> map,
        object parameters = null,
        string splitOn = "id", SqlTransaction transaction = null);
}