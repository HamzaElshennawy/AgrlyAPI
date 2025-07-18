using AgrlyAPI.Models.Files;
using AgrlyAPI.Models.Users;
using AgrlyAPI.Models.Users;
using System;


namespace AgrlyAPI.Services;
public class DBService
{
    private readonly Supabase.Client _client;

    public DBService(Supabase.Client client)
    {
        _client = client;
    }

    public Supabase.Client GetClient()
    {
        return _client;
    }

    public async Task<T> GetSingleRecordAsync<T>(string tableName, long id) where T : FilesUser, new()
    {
        // Fix for CS0310: Ensure T has a public parameterless constructor by adding 'new()' constraint.
        var response = await _client.From<T>().Where(x => x.Id == id).Get();

        // Fix for CS1501: Replace the incorrect overload of 'Where' with a lambda expression.
        return response.Models.FirstOrDefault();
    }
}
