// ********************************************************
// Copyright (C) 2022 Louis S. Berman (louis@squideyes.com)
//
// This file is part of JonesRovers
//
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BackEnd;

public class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((c, b) =>
            {
                if (c.HostingEnvironment.IsDevelopment())
                    b.AddUserSecrets<Program>();
            })
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices((c, s) =>
            {
                var options = new CosmosClientOptions
                {
                    Serializer = new CosmosJsonSerializer(
                        JsonHelper.GetJsonSerializerOptions())
                };

                var client = new CosmosClient(
                    c.Configuration["CosmosConnString"], options);

                s.AddSingleton(
                    client.GetContainer("JonesRovers", "Manifests"));
            })
            .Build();

        host.Run();
    }
}