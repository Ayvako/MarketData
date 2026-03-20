using MarketData.Application.Interfaces;
using MarketData.Infrastructure.Persistence;
using MarketData.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient<IFintachartsAuthService, FintachartsAuthService>();

builder.Services.AddTransient<FintachartsAuthHandler>();

builder.Services.AddHttpClient<IFintachartsRestClient, FintachartsRestClient>()
    .AddHttpMessageHandler<FintachartsAuthHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await db.Database.MigrateAsync().ConfigureAwait(false);
}

await app.RunAsync().ConfigureAwait(false);