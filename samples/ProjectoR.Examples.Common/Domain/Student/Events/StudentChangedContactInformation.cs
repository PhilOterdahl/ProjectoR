namespace ProjectoR.Examples.Common.Domain.Student.Events;

public record StudentChangedContactInformation(
    string Id,
    string Mobile,
    string Email
);