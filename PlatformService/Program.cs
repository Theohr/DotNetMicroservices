using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

// Access configuration and environment
var configuration = builder.Configuration;
var env = builder.Environment;

// Configure services
 if (env.IsProduction())
 {
    Console.WriteLine("--> Using SqlServer Db");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(configuration.GetConnectionString("PlatformsConn")));
 }
 else
 {
     Console.WriteLine("--> Using InMem Db");
     builder.Services.AddDbContext<AppDbContext>(opt =>
         opt.UseInMemoryDatabase("InMem"));
 }

// Add services to the container
builder.Services.AddGrpc();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();

builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcPlatformService>();

app.MapGet("protos/platforms.proto", async context =>
{
    await context.Response.WriteAsync(System.IO.File.ReadAllText("Protos/platforms.proto"));
});
PrepDb.PrepPopulation(app, env.IsProduction());

app.Run();
