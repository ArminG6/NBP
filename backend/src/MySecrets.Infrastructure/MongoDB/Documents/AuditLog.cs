using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MySecrets.Infrastructure.MongoDB.Documents;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public Guid? UserId { get; set; }

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    [BsonElement("details")]
    public string Details { get; set; } = string.Empty;

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;
}

public class SecretAccessLog : AuditLog
{
    [BsonElement("secretId")]
    public Guid SecretId { get; set; }
}

public class AuthEventLog : AuditLog
{
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("success")]
    public bool Success { get; set; }

    [BsonElement("reason")]
    public string? Reason { get; set; }
}

public class ExportLog : AuditLog
{
    [BsonElement("format")]
    public string Format { get; set; } = string.Empty;

    [BsonElement("secretCount")]
    public int SecretCount { get; set; }
}
