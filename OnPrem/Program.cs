// ********************************************************
// Copyright (C) 2022 Louis S. Berman (louis@squideyes.com)
//
// This file is part of JonesRovers
//
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace OnPrem;

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
            .ConfigureFunctionsWorkerDefaults((c, b) =>
            {
            })
            .ConfigureServices((c, s) =>
            {
            })
            .Build();

        host.Run();
    }
}