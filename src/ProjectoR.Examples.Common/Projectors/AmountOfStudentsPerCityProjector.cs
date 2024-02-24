using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain;
using ProjectoR.Examples.Common.Domain.Student;

namespace ProjectoR.Examples.Common.Projectors;

public class AmountOfStudentsPerCityProjector
{
    public static string ProjectionName => "AmountOfStudentsPerCity";
    
    public static async Task<IDbContextTransaction> PreProcess(
        ISampleContext context,
        CancellationToken cancellationToken) =>
        await context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken);

    public static async Task PostProcess(
        ISampleContext context,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
    
    public static async Task Project(
        StudentWasEnrolled enrolled, 
        IDbContextTransaction transaction,
        ISampleContext context,
        CancellationToken cancellationToken)
    {
        var projectionExists = await context
            .AmountOfStudentsPerCity
            .AnyAsync(projection => projection.City == enrolled.City, cancellationToken: cancellationToken);

        if (projectionExists)
        {
            await context
                .AmountOfStudentsPerCity
                .Where(projection => projection.City == enrolled.City)
                .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.Amount, projection => projection.Amount + 1), cancellationToken: cancellationToken);
            return;
        }
        
        context.AmountOfStudentsPerCity.Add(new AmountOfStudentsPerCityProjection
        {
            City = enrolled.City,
            Amount = 1
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}