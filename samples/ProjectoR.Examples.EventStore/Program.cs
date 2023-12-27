using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectoR.Core.Registration;
using Projector.EventStore.Registration;
using ProjectoR.Examples.EventStore;
using ProjectoR.Examples.EventStore.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services
    .AddDbContext<UserContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("UserContext"));
    })
    .AddProjectoR(configurator =>
    {
        configurator
            .UseEventStore(
                builder.Configuration.GetConnectionString("EventStoreDB"),
                eventStoreConfigurator =>
                {
                    eventStoreConfigurator
                        .UseEventStoreCheckpointing()
                        .UseProjector<UserProjector>(configure =>
                        {
                            configure.BatchingOptions.BatchSize = 1000;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(500);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                            configure
                                .SerializationOptions
                                .UseClassNameEventTypeResolver()
                                .UseSnakeCaseEventNaming();
                        });
                }
            );
    })
    .AddHostedService<UserSeeder>();

var userContext = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<UserContext>();

userContext.Database.Migrate();

var app = builder.Build();
app.Run();
