using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain.Student.Events;

namespace ProjectoR.Examples.Common.Projectors;

public class AmountOfStudentsPerCountryProjector
{
    public static string ProjectionName => "AmountOfStudentsPerCountry";
    
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
            .AmountOfStudentsPerCountry
            .AnyAsync(projection => projection.CountryCode == enrolled.CountryCode, cancellationToken: cancellationToken);

        if (projectionExists)
        {
            await context
                .AmountOfStudentsPerCountry
                .Where(projection => projection.CountryCode == enrolled.CountryCode)
                .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.Amount, projection => projection.Amount + 1), cancellationToken: cancellationToken);
            return;
        }
        
        context.AmountOfStudentsPerCountry.Add(new AmountOfStudentsPerCountryProjection
        {
            CountryCode = enrolled.CountryCode,
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
            .AmountOfStudentsPerCountry
            .Where(projection => projection.CountryCode == relocated.OldAddress.CountryCode)
            .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.Amount, projection => projection.Amount - 1), cancellationToken: cancellationToken);
        
        // increase amount for new city
        var projectionExists = await context
            .AmountOfStudentsPerCountry
            .AnyAsync(projection => projection.CountryCode == relocated.NewAddress.CountryCode, cancellationToken: cancellationToken);

        if (projectionExists)
        {
            await context
                .AmountOfStudentsPerCountry
                .Where(projection => projection.CountryCode == relocated.NewAddress.CountryCode)
                .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.Amount, projection => projection.Amount + 1), cancellationToken: cancellationToken);
            return;
        }
        
        context.AmountOfStudentsPerCountry.Add(new AmountOfStudentsPerCountryProjection
        {
            CountryCode = relocated.NewAddress.CountryCode,
            Amount = 1
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}