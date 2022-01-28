using OnPrem;
using System.Text.Json;
using System.Text.Json.Serialization;
using static OnPrem.HeadsOrTails;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var random = new Random();

var result = new CoinTossResult()
{
    HeadsOrTails = random.Next(2) == 1 ? Heads : Tails
};

var options = new JsonSerializerOptions();

options.Converters.Add(new JsonStringEnumConverter());

app.MapGet("/CoinToss", () => Results.Json(result, options));

app.Run();
