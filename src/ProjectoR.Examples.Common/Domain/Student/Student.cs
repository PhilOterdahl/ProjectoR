namespace ProjectoR.Examples.Common.Domain.Student;

public class Student : Aggregate<StudentState>
{
    public string Id => State.Id;
    
    public static Student Enroll(
        string firstName,
        string lastName, 
        string email, 
        string mobile,
        string countryCode, 
        string city, 
        string postalCode, 
        string street) =>
        new(firstName, lastName, email, mobile, countryCode, city, postalCode, street);

    public Student()
    {
    }
    
    private Student(  
        string firstName,
        string lastName,
        string email,
        string mobile,
        string countryCode,
        string city,
        string postalCode,
        string street)
    {
        var enrolled = new StudentWasEnrolled(
            Guid.NewGuid().ToString(), 
            firstName, 
            lastName, 
            email, 
            mobile, 
            countryCode, 
            city, 
            postalCode, 
            street,
            DateTimeOffset.UtcNow
        );
        
        AddEvent(enrolled);
    }

    public Student Relocate(
        string countryCode,
        string city,
        string postalCode,
        string street)
    {
        if (State.GraduationDate.HasValue)
            throw new InvalidOperationException("Can not modify a user that has quit");
        
        var studentRelocated = new StudentRelocated(State.Id, countryCode, city, postalCode, street);
        
        AddEvent(studentRelocated);
        return this;
    }
    
    public Student ChangeContactInformation(
        string mobile,
        string email)
    {
        if (State.GraduationDate.HasValue)
            throw new InvalidOperationException("Can not modify a user that has quit");
        
        var changedContactInformation = new StudentChangedContactInformation(State.Id, mobile, email);
        
        AddEvent(changedContactInformation);
        return this;
    }
    
    public Student Graduate()
    {
        if (State.GraduationDate.HasValue)
            throw new InvalidOperationException("Can not modify a user that has graduated");
        
        var graduated = new StudentGraduated(State.Id, State.CountryCode, DateTimeOffset.UtcNow);
        
        AddEvent(graduated);
        return this;
    }

    protected override StudentState ApplyEvent(StudentState currentState, object @event) =>
        @event switch
        {   
            StudentWasEnrolled studentWasEnrolled => new StudentState
            {
                Id = studentWasEnrolled.Id,
                FirstName = studentWasEnrolled.FirstName,
                CountryCode = studentWasEnrolled.CountryCode,
                LastName = studentWasEnrolled.LastName,
                PostalCode = studentWasEnrolled.PostalCode,
                City = studentWasEnrolled.City,
                Email = studentWasEnrolled.Email,
                Mobile = studentWasEnrolled.Mobile,
                Street = studentWasEnrolled.Street,
                EnrolledDate = studentWasEnrolled.EnrolledDate
            },
            StudentRelocated studentRelocated => currentState with
            {
                City = studentRelocated.City,
                PostalCode = studentRelocated.PostalCode,
                Street = studentRelocated.Street,
                CountryCode = studentRelocated.CountryCode
            },
            StudentChangedContactInformation studentChangedContactInformation => currentState with
            {
                Email = studentChangedContactInformation.Email,
                Mobile = studentChangedContactInformation.Mobile
            },
            StudentGraduated studentGraduated => currentState with
            {
                GraduationDate = studentGraduated.QuitDate
            },
            _ => throw new ArgumentOutOfRangeException(nameof(@event), $"Event of type: {@event.GetType().Name} is not handled by applyEvent method")
        };
    
}