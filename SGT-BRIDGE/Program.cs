using SGT_BRIDGE.Endpoints;
using SGT_BRIDGE.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using System.Reflection;
using System.Net;
using SGT_BRIDGE.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var versionRevision = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = version,
        Contact = new OpenApiContact
        {
            Name = "bstdio.com",
            Url = new Uri("https://bstdio.com")
        },
        Title = "Subiekt GT REST API",
        Description = $"Subiekt GT REST API Bridge ({versionRevision})",
    });

    var xmlfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlfile));
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddSingleton<SubiektGT>();
builder.Services.AddDbContext<TxContext>(options => options.UseSqlite("Data Source=tx.db"));

builder.Services.AddAuthentication().AddBearerToken();
builder.Services.AddAuthorization();

if (builder.Environment.IsDevelopment()) 
{ 
    builder.Services.AddHttpLogging(logging => {
        logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestBody;
    });
}
else
{
    builder.Services.AddHttpLogging(logging => {
        logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath;
    });

    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.Limits.MaxRequestHeadersTotalSize = 65536;
        serverOptions.Limits.MaxRequestLineSize = 8192;

        serverOptions.ListenAnyIP(5071, listenOptions =>
        {
            listenOptions.UseHttps("certs/ssl.pfx", "ramboo");
        });
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseHttpLogging();

app.RegisterIndexEndpoint();
app.RegisterProductEndpoint();
app.RegisterPackageEndpoint();
app.RegisterPriceEndpoint();
app.RegisterUserEndpoint();

var localIp = Dns.GetHostEntry(Dns.GetHostName())
    .AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork 
        && (ip.ToString().StartsWith("192.") || ip.ToString().StartsWith("10.")));

app.Urls.Add($"http://{localIp}:5071");
app.Urls.Add($"https://{localIp}:7271");

app.Run();

