using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});
builder.Services.AddSingleton<IReviewRepository, MongoReviewRepository>();
builder.Services.AddSingleton<InMemoryRateLimiter>();

builder.Services.Configure<DeepLSettings>(builder.Configuration.GetSection("DeepL"));
builder.Services.AddHttpClient<ITranslationService, DeepLTranslationService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Render's $PORT is assigned at deploy time; Kestrel must bind to it or the platform
// reports "no open ports detected". Falls back to 5216 for local `dotnet run`.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5216";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Fail fast at boot rather than silently on first request: an empty Mongo connection
// string outside Development means the deploy is misconfigured.
var mongoConnectionString = builder.Configuration["Mongo:ConnectionString"];
if (!app.Environment.IsDevelopment() && string.IsNullOrEmpty(mongoConnectionString))
{
    throw new InvalidOperationException(
        "Mongo:ConnectionString is not configured. Set the Mongo__ConnectionString environment variable before starting the app.");
}

// Make a misconfigured deploy visible immediately in the Render logs instead of being
// discovered later as a mystery CORS error in a browser console.
Console.WriteLine($"CORS AllowedOrigins: [{string.Join(", ", allowedOrigins)}]");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Behind Render's reverse proxy, HttpContext.Connection.RemoteIpAddress is the proxy's
// address unless we opt in to reading X-Forwarded-For/X-Forwarded-Proto. Render's proxy
// IP isn't known ahead of time, so KnownNetworks/KnownProxies are cleared to trust
// whatever X-Forwarded-For arrives (spoofable, but acceptable as a spam speed-bump for
// the rate limiter, not a security boundary).
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
