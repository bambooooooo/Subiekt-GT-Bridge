using SGT_BRIDGE.Endpoints;
using SGT_BRIDGE.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using System.Reflection;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var versionRevision = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

builder.Services.AddAuthentication().AddBearerToken();
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
}

app.RegisterIndexEndpoint();
app.RegisterProductEndpoint();
app.RegisterPackageEndpoint();

var localIp = Dns.GetHostEntry(Dns.GetHostName())
    .AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork 
        && (ip.ToString().StartsWith("192.") || ip.ToString().StartsWith("10.")));

app.Urls.Add($"http://{localIp}:5071");

app.Run();

