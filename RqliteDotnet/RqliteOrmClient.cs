using System.Data;
using RqliteDotnet.Dto;

namespace RqliteDotnet;

public interface IRqliteOrmClient : IRqliteClient
{
    /// <summary>
    /// Query Rqlite DB and return result as an instance of T
    /// </summary>
    /// <param name="query">Query to execute</param>
    /// <typeparam name="T">Type of result object</typeparam>
    /// <returns></returns>
    Task<List<T>> Query<T>(string query) where T: new();

    Task<List<U>> QueryParams<T, U>(string query, params T[] qps) 
        where T: QueryParameter 
        where U : new();
}

public class RqliteOrmClient : RqliteClient, IRqliteOrmClient
{
    public RqliteOrmClient(HttpClient client) : base(client) { }

    public RqliteOrmClient(string uri, HttpClient? client = null) : base(uri, client) {}

    /// <summary>
    /// Query Rqlite DB and return result as an instance of T
    /// </summary>
    /// <param name="query">Query to execute</param>
    /// <typeparam name="T">Type of result object</typeparam>
    /// <returns></returns>
    public async Task<List<T>> Query<T>(string query) where T : new()
    {
        var response = await Query(query);
        if (response.Results!.Count > 1)
            throw new DataException("Query returned more than 1 result. At the moment only 1 result supported");
        var res = response.Results[0];

        if (!string.IsNullOrEmpty(res.Error))
            throw new InvalidOperationException(res.Error);
        var list = new List<T>();

        for (int i = 0; i < res.Values?.Count; i++)
        {
            var dto = new T();

            foreach (var prop in typeof(T).GetProperties())
            {
                var index = res.Columns.FindIndex(c => c.ToLower() == prop.Name.ToLower());
                var val = GetValue(res.Types[index], res.Values[i][index]);

                prop.SetValue(dto, val);
            }

            list.Add(dto);
        }

        return list;
    }

    public async Task<List<U>> QueryParams<T, U>(string query, params T[] qps)
        where T : QueryParameter
        where U : new()
    {
        var response = await QueryParams(query, qps);
        if (response.Results!.Count > 1)
            throw new DataException("Query returned more than 1 result. At the moment only 1 result supported");
        var res = response.Results[0];

        if (!string.IsNullOrEmpty(res.Error))
            throw new InvalidOperationException(res.Error);
        var list = new List<U>();

        for (int i = 0; i < res.Values?.Count; i++)
        {
            var dto = new U();

            foreach (var prop in typeof(U).GetProperties())
            {
                var index = res.Columns.FindIndex(c => c.ToLower() == prop.Name.ToLower());
                var val = GetValue(res.Types[index], res.Values[i][index]);

                prop.SetValue(dto, val);
            }

            list.Add(dto);
        }

        return list;
    }
}