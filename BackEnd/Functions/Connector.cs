// ********************************************************
// Copyright (C) 2022 Louis S. Berman (louis@squideyes.com)
//
// This file is part of JonesRovers
//
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace BackEnd;

public class Connector
{
    public class TimerInfo
    {
        public bool IsPastDue { get; set; }
        public ScheduleStatus ScheduleStatus { get; set; }
    }

    public class ScheduleStatus
    {
        public DateTime Last { get; set; }
        public DateTime Next { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    private readonly ILogger logger;
    private readonly Container container;
    private readonly PhotosClient client;

    public Connector(ILoggerFactory loggerFactory,
        IConfiguration config, Container container)
    {
        logger = loggerFactory.CreateLogger<Connector>();

        this.container = container;

        client = new PhotosClient(config["NasaApiKey"]);
    }

    [Function("GetAndUpsertManifests")]
    public async Task GetAndUpsertManifestsAsync(
        [TimerTrigger("0 0 4 * * *", RunOnStartup = true)] TimerInfo info)
    {
        var existing = await client.GetManifestsAsync();

        var manifestIds = await GetManifestIdsAsync(container);

        var missing = existing.Where(
            m => !manifestIds.Contains(m.Id)).ToList();

        if (missing.Count == 0)
        {
            logger.LogInformation("There are no missing manifests!");

            return;
        }

        int count = 0;

        logger.LogInformation(
            $"{missing.Count:N0} missing manifest(s) enqueued to be saved.");

        foreach (var manifest in missing)
        {
            await container.UpsertItemAsync(manifest);

            logger.LogInformation(
                $"UPSERTED {manifest} ({++count:00000} of {missing.Count:00000})");
        }
    }

    [Function("GetPhotos")]
    public async Task<HttpResponseData> GetPhotosAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get",
        Route = "GetPhotos/{rover}/{date}/{camera?}")] HttpRequestData request,
        string rover, string date, string camera)
    {
        HttpResponseData BadRequest(string name, string kind)
        {
            var response = request.CreateResponse(HttpStatusCode.BadRequest);

            response.Headers.Add("Content-Type", "plain/text; charset=utf-8");

            response.WriteString($"The \"{name}\" {kind} is invalid!");

            return response;
        }

        if (!Enum.TryParse(rover, out Rover r))
            return BadRequest(nameof(rover), nameof(Rover));

        if (!DateOnly.TryParse(date, out DateOnly d))
            return BadRequest(nameof(date), nameof(DateOnly));

        var hasCamera = !string.IsNullOrWhiteSpace(camera);

        Camera c = default;

        if (hasCamera && !Enum.TryParse(camera, out c))
            return BadRequest(nameof(camera), nameof(Camera));

        HttpResponseData response = null;

        var id = Manifest.GetId(r, d);

        string CouldNotBeRead() =>
            $"The {r}/{d:MM-dd-yyyy} photos could not be retrieved ";

        try
        {
            var item = await container.ReadItemAsync<Manifest>(
                id, new PartitionKey(r.ToString()));

            var photos = await client.GetPhotosAsync(
                item.Resource, hasCamera ? c : null);

            response = request.CreateResponse(HttpStatusCode.OK);

            response.Headers.Add(
                "Content-Type", "application/json; charset=utf-8");

            if (item.StatusCode == HttpStatusCode.OK)
            {
                response.WriteString(JsonSerializer.Serialize(
                    photos, JsonHelper.GetJsonSerializerOptions()));
            }
            else
            {
                var errorInfo = new ErrorInfo(
                    item.StatusCode, CouldNotBeRead());

                response.WriteString(JsonSerializer.Serialize(
                    errorInfo, JsonHelper.GetJsonSerializerOptions()));
            }
        }
        catch (CosmosException error)
        {
            response = request.CreateResponse(error.StatusCode);

            response.Headers.Add(
                "Content-Type", "application/json; charset=utf-8");

            var errorInfo = new ErrorInfo(error.StatusCode,
                $"{CouldNotBeRead()} (Error: {error.Message})");

            response.WriteString(JsonSerializer.Serialize(
                errorInfo, JsonHelper.GetJsonSerializerOptions()));
        }

        return response;
    }

    private static async Task<HashSet<string>> GetManifestIdsAsync(
        Container container)
    {
        var ids = new HashSet<string>();

        var options = new QueryRequestOptions()
        {
            MaxItemCount = -1
        };

        string continuation = null;

        using FeedIterator<Manifest> iterator =
            container.GetItemQueryIterator<Manifest>(
                new QueryDefinition("SELECT * FROM c"),
                requestOptions: options,
                continuationToken: continuation);

        while (iterator.HasMoreResults)
        {
            FeedResponse<Manifest> response =
                await iterator.ReadNextAsync();

            foreach (var id in response.Select(r => r.Id))
                ids.Add(id);
        }

        return ids;
    }
}