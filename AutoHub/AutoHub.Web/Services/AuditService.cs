using AutoHub.Web.Data;
using AutoHub.Web.Models;

namespace AutoHub.Web.Services;

public class AuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _http;
    public AuditService(ApplicationDbContext db, IHttpContextAccessor http) { _db = db; _http = http; }

    public async Task WriteAsync(string action, string? entity = null, string? entityId = null, string? details = null, string? actor = null)
    {
        var ctx = _http.HttpContext;
        _db.AuditLogs.Add(new AuditLog {
            Username = actor ?? ctx?.User?.Identity?.Name ?? "anonymous",
            Action = action, EntityName = entity, EntityId = entityId,
            Details = details, IpAddress = ctx?.Connection.RemoteIpAddress?.ToString(),
            OccurredAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
