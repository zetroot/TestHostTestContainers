using Refit;

namespace IntegrationTests;

public interface IDataClient
{
    [Post("/data")]
    Task<Guid> Create([Body(BodySerializationMethod.Serialized)]string data);

    [Get("/data/{id}")]
    Task<string> Get(Guid id);
}
