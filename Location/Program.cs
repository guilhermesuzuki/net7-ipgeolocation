using Location.Facades;
using Location.Interfaces;
using Location.Middlewares;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Services.IpLocation;
using Services.IpLocation.Concrete;
using Services.WeatherForecast;
using Services.WeatherForecast.Concrete;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//adding MVC to the web-application
builder.Services.AddMvc();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

//adding session capabilities to the web-application
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//adding the dependency injections
builder.Services.AddSingleton<ILocationService, IpInfoDb>(x => new IpInfoDb("INSERT YOUR KEY HERE", x.GetRequiredService<IMemoryCache>()));
builder.Services.AddSingleton<ILocationService, IpApi>();
builder.Services.AddSingleton<IUserFacade, UserFacade>();
builder.Services.AddSingleton<IWeatherService, OpenWeatherMap>(x => new OpenWeatherMap("INSERT YOUR KEY HERE", x.GetRequiredService<IMemoryCache>()));

var app = builder.Build();

app.UseSession();
app.UseMiddleware<LocationMiddleware>();
app.UseMiddleware<WeatherMiddleware>();
app.MapGet("/", (HttpContext context, IUserFacade userFacade) =>
{
    var location = userFacade.Location;
    var weather = userFacade.Weather;

    var sb = new StringBuilder();
    if (location != null) sb.Append($"Your location is {location.City} - {location.Country} ({location.CountryCode}). ");
    if (weather != null) sb.Append($"The weather temperature is {weather.Temperature}�C, {weather.WeatherDescription}. ");

    return sb.ToString() != "" ? sb.ToString() : "No user location/weather.";
});
app.Run();
