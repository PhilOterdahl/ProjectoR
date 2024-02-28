using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain.Student.Events;

namespace ProjectoR.Examples.Common.Projectors;

public class UserProjector
{
    public static string ProjectionName => "Student";
    
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
        context.Students.Add(new StudentProjection
        {
            Id = enrolled.Id,
            FirstName = enrolled.FirstName,
            LastName = enrolled.LastName,
            Address = new Address
            {
                CountryCode = enrolled.CountryCode,
                City = enrolled.City,
                Street = enrolled.Street,
                PostalCode = enrolled.PostalCode
            },
            ContactInformation = new ContactInformation
            {
                Email = enrolled.Email,
                Mobile = enrolled.Mobile
            }
        });

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task Project(StudentRelocated relocated, ISampleContext context, CancellationToken cancellationToken) =>
        await context
            .Students
            .Where(user => user.Id == relocated.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.Address.City, relocated.NewAddress.City)
                    .SetProperty(projection => projection.Address.PostalCode, relocated.NewAddress.PostalCode)
                    .SetProperty(projection => projection.Address.Street, relocated.NewAddress.Street),
                cancellationToken
            );

    public async Task Project(StudentChangedContactInformation changedContactInformation, ISampleContext context, CancellationToken cancellationToken) =>
        await context
            .Students
            .Where(user => user.Id == changedContactInformation.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.ContactInformation.Email, changedContactInformation.Email)
                    .SetProperty(projection => projection.ContactInformation.Mobile, changedContactInformation.Mobile),
                cancellationToken
            );

    public async Task Project(StudentGraduated studentGraduated, ISampleContext context, CancellationToken cancellationToken) =>
        await context
            .Students
            .Where(user => user.Id == studentGraduated.Id)
            .ExecuteDeleteAsync(cancellationToken);
}