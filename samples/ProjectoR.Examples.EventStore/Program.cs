using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;
using ProjectoR.EntityFrameworkCore.Registration;
using Projector.EventStore.Registration;
using ProjectoR.Examples.EventStore;
using ProjectoR.Examples.EventStore.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services
    .AddDbContext<UserContext>(options => options
        .UseNpgsql(builder.Configuration.GetConnectionString("UserContext"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
    )
    .AddProjectoR(configurator =>
    {
        configurator.MaxConcurrency = 10;
        configurator.PrioritizationBatchSize = 100;
        configurator.PrioritizationTime = TimeSpan.FromMilliseconds(100);
        configurator
            .UseEventStore(
                builder.Configuration.GetConnectionString("EventStoreDB"),
                eventStoreConfigurator =>
                {
                    eventStoreConfigurator
                        .UseProjector<UserProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Lowest;
                            configure.BatchingOptions.BatchSize = 1000;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(500);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                            configure
                                .SerializationOptions
                                .UseClassNameEventTypeResolver()
                                .UseSnakeCaseEventNaming();
                        })
                        .UseProjector<AmountOfUsersInCitiesProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Normal;
                            configure.BatchingOptions.BatchSize = 1000;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(500);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                            configure
                                .SerializationOptions
                                .UseClassNameEventTypeResolver()
                                .UseSnakeCaseEventNaming();
                        })
                        .UseProjector<AmountOfUsersInCountryProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Highest;
                            configure.BatchingOptions.BatchSize = 1000;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(500);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                            configure
                                .SerializationOptions
                                .UseClassNameEventTypeResolver()
                                .UseSnakeCaseEventNaming();
                        });
                }
            )
            .UseEntityFramework(frameworkConfigurator => frameworkConfigurator.UseEntityFrameworkCheckpointing<UserContext>());
    })
    .AddHostedService<UserSeeder>();

var userContext = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<UserContext>();

userContext.Database.Migrate();

var app = builder.Build();
app.Run();
