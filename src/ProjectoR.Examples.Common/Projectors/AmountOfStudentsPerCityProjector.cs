using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain.Student.Events;

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
    
    public static async Task Project(
        StudentRelocated relocated, 
        ISampleContext context,
        CancellationToken cancellationToken)
    {
        // decrease amount for previous city
        await context
            .AmountOfStudentsPerCity
            .Where(projection => projection.City == relocated.OldAddress.City)
            .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.Amount, projection => projection.Amount - 1), cancellationToken: cancellationToken);
        
        
        // increase amount for new city
        var projectionExists = await context
            .AmountOfStudentsPerCity
            .AnyAsync(projection => projection.City == relocated.NewAddress.City, cancellationToken: cancellationToken);

        if (projectionExists)
        {
            await context
                .AmountOfStudentsPerCity
                .Where(projection => projection.City == relocated.NewAddress.City)
                .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.Amount, projection => projection.Amount + 1), cancellationToken: cancellationToken);
            return;
        }
        
        context.AmountOfStudentsPerCity.Add(new AmountOfStudentsPerCityProjection
        {
            City = relocated.NewAddress.City,
            Amount = 1
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}