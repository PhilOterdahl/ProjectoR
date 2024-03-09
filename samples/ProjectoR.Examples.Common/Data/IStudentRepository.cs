using ProjectoR.Examples.Common.Domain.Student;

namespace ProjectoR.Examples.Common.Data;

public interface IStudentRepository
{
    Task<Student?> TryLoad(string id, CancellationToken cancellationToken);
    
    Task<Student> Load(string id, CancellationToken cancellationToken);
    
    Task CommitEvents(Student student, CancellationToken cancellationToken);
}