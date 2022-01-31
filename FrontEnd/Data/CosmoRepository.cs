using Common;
using Microsoft.Azure.Cosmos;

namespace FrontEnd;

public class CosmosRepository
{
    private readonly CosmosClient client;
    private readonly Database database;
    private readonly Container container;

    public CosmosRepository(
        string connString, string databaseId, string containerId)
    {
        var options = new CosmosClientOptions
        {
            Serializer = new CosmosJsonSerializer(
                JsonHelper.GetJsonSerializerOptions())
        };

        client = new CosmosClient(connString, options);

        database = client.GetDatabase(databaseId);
        container = database.GetContainer(containerId);
    }

    private class MinMax
    {
        public Rover Rover { get; init; }
        public DateOnly MinDate { get; init; }
        public DateOnly MaxDate { get; init; }
    }

    public async Task<SortedDictionary<Rover, (DateOnly, DateOnly)>> GetRoverMinMaxAsync()
    {
        var query = new QueryDefinition(
            "SELECT c.rover, MIN(c.date) AS minDate, MAX(c.date) AS maxDate FROM c GROUP BY c.rover");

        var iterator = container.GetItemQueryIterator<MinMax>(query);

        var data = new SortedDictionary<Rover, (DateOnly, DateOnly)>();

        while (iterator.HasMoreResults)
        {
            var results = await iterator.ReadNextAsync();

            foreach (MinMax result in results)
                data.Add(result.Rover, (result.MinDate, result.MaxDate));
        }

        return data;
    }
}