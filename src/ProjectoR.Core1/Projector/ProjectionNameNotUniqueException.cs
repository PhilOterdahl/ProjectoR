namespace ProjectoR.Core.Projector;

internal sealed class ProjectionNameNotUniqueException(string projectionName) : InvalidOperationException($"Projector with projectionName: {projectionName} already exists, ProjectionName needs to be unique");