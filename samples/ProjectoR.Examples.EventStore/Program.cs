using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;
using ProjectoR.EventStore.Registration;
using ProjectoR.Examples.Common;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Projectors;
using ProjectoR.Examples.EventStore;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services
    .AddLogging(loggingBuilder => loggingBuilder.AddConsole())
    .AddDbContextPool<ApplicationContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ApplicationContext")))
    .AddScoped<ISampleContext>(provider => provider.GetRequiredService<ApplicationContext>())
    .AddProjectoR(configurator =>
    {
        configurator
            .UseEventStore(
                builder.Configuration.GetConnectionString("EventStoreDB"),
                eventStoreConfigurator =>
                {
                    eventStoreConfigurator
                        .UseEventStoreCheckpointing()
                        // .UseSubscription<UserProjector>(configure =>
                        // {
                        //     configure.Priority = ProjectorPriority.Highest;
                        //     configure.BatchingOptions.BatchSize = 100;
                        //     configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                        //     configure.CheckpointingOptions.CheckpointAfterBatch();
                        //     configure.SerializationOptions
                        //         .UseClassNameEventTypeResolver()
                        //         .UseSnakeCaseEventNaming();
                        // });
                        .UseSubscription<AmountOfUsersInCitiesProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Normal;
                            configure.BatchingOptions.BatchSize = 100;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                            configure.SerializationOptions
                                .UseClassNameEventTypeResolver()
                                .UseSnakeCaseEventNaming();
                        });
                    // .UseSubscription<AmountOfUsersInCountryProjector>(configure =>
                    // {
                    //     configure.Priority = ProjectorPriority.Lowest;
                    //     configure.BatchingOptions.BatchSize = 100;
                    //     configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                    //     configure.CheckpointingOptions.CheckpointAfterBatch();
                    //     configure.SerializationOptions
                    //         .UseClassNameEventTypeResolver()
                    //         .UseSnakeCaseEventNaming();
                    // });
                }
            );
    });

var userContext = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<ApplicationContext>();

userContext.Database.Migrate();

var app = builder.Build();
await Seeder.Seed(1000, app.Services);

app.Run();
