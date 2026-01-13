using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MySecrets.Infrastructure.MongoDB.Documents;

namespace MySecrets.Infrastructure.MongoDB;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<AuditLog> AuditLogs => 
        _database.GetCollection<AuditLog>("audit_logs");

    public IMongoCollection<SecretAccessLog> SecretAccessLogs => 
        _database.GetCollection<SecretAccessLog>("secret_access_logs");

    public IMongoCollection<AuthEventLog> AuthEventLogs => 
        _database.GetCollection<AuthEventLog>("auth_event_logs");

    public IMongoCollection<ExportLog> ExportLogs => 
        _database.GetCollection<ExportLog>("export_logs");
}
