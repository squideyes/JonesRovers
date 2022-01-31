// ********************************************************
// Copyright (C) 2022 Louis S. Berman (louis@squideyes.com)
//
// This file is part of JonesRovers
//
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using FrontEnd;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new CosmosRepository(
    builder.Configuration["Cosmos:ConnString"], 
    builder.Configuration["Cosmos:DatabaseId"], 
    builder.Configuration["Cosmos:ContainerId"]));

builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();

app.MapFallbackToPage("/_Host");

app.Run();