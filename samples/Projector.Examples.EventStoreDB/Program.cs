using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Projector.Core.TypeResolvers;
using Projector.EventStoreDB.Checkpointing;
using Projector.EventStoreDb.Registration;
using Projector.Examples.EventStoreDB;
using Projector.Examples.EventStoreDB.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services
    .AddEventStoreClient(builder.Configuration.GetConnectionString("EventStoreDB"))
    .AddDbContext<UserContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("UserContext"))
    )
    .AddEventStoreDBCheckpointing()
    .AddEventStoreDBProjection<UserProjector>(options => options.)
    .AddHostedService<UserSeeder>();

var userContext = builder.Services.BuildServiceProvider().GetRequiredService<UserContext>();
userContext.Database.Migrate();

var app = builder.Build();
app.Run();
