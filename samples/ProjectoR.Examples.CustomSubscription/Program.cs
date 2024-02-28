using Microsoft.EntityFrameworkCore;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;
using ProjectoR.EntityFrameworkCore.Registration;
using ProjectoR.Examples.Common;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Processes;
using ProjectoR.Examples.Common.Projectors;
using ProjectoR.Examples.CustomSubscription;
using ProjectoR.Examples.CustomSubscription.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services
    .AddEndpointsApiExplorer()
    .AddOpenApiDocument()
    .AddStudentProcesses()
    .AddLogging(loggingBuilder => loggingBuilder.AddConsole())
    .AddDbContextPool<ApplicationContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ApplicationContext")))
    .AddScoped<ISampleContext>(provider => provider.GetRequiredService<ApplicationContext>())
    .AddScoped<IStudentRepository, StudentRepository>()
    .AddProjectoR(configurator =>
    {
        configurator.SerializationOptions.UseCustomEventTypeResolver<EventTypeResolver>();
        configurator
            .UseCustomSubscription<EfCoreSubscription, StudentProjector>(configure =>
            {
                configure.Priority = ProjectorPriority.Highest;
                configure.BatchingOptions.BatchSize = 100;
                configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                configure.CheckpointingOptions.CheckpointAfterBatch();
            })
            .UseCustomSubscription<EfCoreSubscription, AmountOfStudentsPerCityProjector>(configure =>
            {
                configure.Priority = ProjectorPriority.Normal;
                configure.BatchingOptions.BatchSize = 100;
                configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                configure.CheckpointingOptions.CheckpointAfterBatch();
            })
            .UseCustomSubscription<EfCoreSubscription, AmountOfStudentsPerCountryProjector>(configure =>
            {
                configure.Priority = ProjectorPriority.Lowest;
                configure.BatchingOptions.BatchSize = 100;
                configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                configure.CheckpointingOptions.CheckpointAfterBatch();
            })
            .UseEntityFramework(frameworkConfigurator => frameworkConfigurator.UseEntityFrameworkCheckpointing<ApplicationContext>());
    });

var dbContext = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<ApplicationContext>();

dbContext.Database.Migrate();

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