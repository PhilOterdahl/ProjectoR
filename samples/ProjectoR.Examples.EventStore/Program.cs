using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;
using ProjectoR.EntityFrameworkCore.Registration;
using ProjectoR.EventStore.Registration;
using ProjectoR.Examples.EventStore;
using ProjectoR.Examples.EventStore.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services
    .AddLogging(loggingBuilder => loggingBuilder.AddConsole())
    .AddDbContext<UserContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("UserContext")))
    .AddProjectoR(configurator =>
    {
        configurator
            .UseEventStore(
                builder.Configuration.GetConnectionString("EventStoreDB"),
                eventStoreConfigurator =>
                {
                    eventStoreConfigurator
                        .UseProjector<UserProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Highest;
                            configure.BatchingOptions.BatchSize = 100;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                            configure.SerializationOptions
                                .UseClassNameEventTypeResolver()
                                .UseSnakeCaseEventNaming();
                        })
                        .UseProjector<AmountOfUsersInCitiesProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Normal;
                            configure.BatchingOptions.BatchSize = 100;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                            configure.SerializationOptions
                                .UseClassNameEventTypeResolver()
                                .UseSnakeCaseEventNaming();
                        })
                        .UseProjector<AmountOfUsersInCountryProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Lowest;
                            configure.BatchingOptions.BatchSize = 100;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                            configure.SerializationOptions
                                .UseClassNameEventTypeResolver()
                                .UseSnakeCaseEventNaming();
                        });
                }
            )
            .UseEntityFramework(frameworkConfigurator => frameworkConfigurator.UseEntityFrameworkCheckpointing<UserContext>());
    });

var userContext = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<UserContext>();

userContext.Database.Migrate();

var app = builder.Build();
await UserSeeder.Seed(10000, app.Services);

app.Run();
