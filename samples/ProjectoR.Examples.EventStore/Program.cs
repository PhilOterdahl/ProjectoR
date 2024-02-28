using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;
using Projector.EventStore.Registration;
using ProjectoR.Examples.Common;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Processes;
using ProjectoR.Examples.Common.Projectors;
using ProjectoR.Examples.EventStore.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

builder
    .Services
    .AddEndpointsApiExplorer()
    .AddOpenApiDocument()
    .AddStudentProcesses()
    .AddLogging(loggingBuilder => loggingBuilder.AddConsole())
    .AddDbContextPool<ApplicationContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("ApplicationContext"))
    )
    .AddScoped<ISampleContext>(provider => provider.GetRequiredService<ApplicationContext>())
    .AddScoped<IStudentRepository, StudentRepository>()
    .AddProjectoR(configurator =>
    {
        configurator.SerializationOptions.UseCustomEventTypeResolver<EventTypeResolver>();
        configurator
            .UseEventStore(
                builder.Configuration.GetConnectionString("EventStoreDB"),
                eventStoreConfigurator =>
                {
                    eventStoreConfigurator
                        .UseEventStoreCheckpointing()
                        .UseSubscription<StudentProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Highest;
                            configure.BatchingOptions.BatchSize = 100;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                        })
                        .UseSubscription<AmountOfStudentsPerCityProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Normal;
                            configure.BatchingOptions.BatchSize = 100;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                        })
                        .UseSubscription<AmountOfStudentsPerCountryProjector>(configure =>
                        {
                            configure.Priority = ProjectorPriority.Lowest;
                            configure.BatchingOptions.BatchSize = 100;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                        });
                }
            );
    });

var userContext = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<ApplicationContext>();

userContext.Database.Migrate();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

app.UseStudentEndpoints();
app.Run();
