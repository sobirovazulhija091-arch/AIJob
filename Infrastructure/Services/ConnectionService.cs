using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class ConnectionService(ApplicationDbContext dbContext) : IConnectionService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> SendRequestAsync(int requesterId, int addresseeId)
    {
        if (requesterId == addresseeId)
            return new Response<string>(HttpStatusCode.BadRequest, "Cannot connect with yourself");

        var addresseeExists = await context.Users.AnyAsync(u => u.Id == addresseeId);
        if (!addresseeExists)
            return new Response<string>(HttpStatusCode.BadRequest, "User not found");

        var exists = await context.Connections.AnyAsync(c =>
            (c.RequesterId == requesterId && c.AddresseeId == addresseeId) ||
            (c.RequesterId == addresseeId && c.AddresseeId == requesterId));
        if (exists)
            return new Response<string>(HttpStatusCode.BadRequest, "Connection already exists or pending");

        var connection = new Connection
        {
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = ConnectionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        await context.Connections.AddAsync(connection);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Connection request sent");
    }

    public async Task<Response<string>> RespondToRequestAsync(int connectionId, int userId, ConnectionStatus status)
    {
        var conn = await context.Connections.FindAsync(connectionId);
        if (conn == null)
            return new Response<string>(HttpStatusCode.NotFound, "Connection not found");
        if (conn.AddresseeId != userId)
            return new Response<string>(HttpStatusCode.Forbidden, "Only the addressee can respond");
        if (conn.Status != ConnectionStatus.Pending)
            return new Response<string>(HttpStatusCode.BadRequest, "Request already responded");

        conn.Status = status;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, status == ConnectionStatus.Accepted ? "Connection accepted" : "Connection declined");
    }

    public async Task<Response<Connection>> GetByIdAsync(int id)
    {
        var get = await context.Connections.FindAsync(id);
        if (get == null)
            return new Response<Connection>(HttpStatusCode.NotFound, "Connection not found");
        return new Response<Connection>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<Connection>>> GetByUserIdAsync(int userId)
    {
        var list = await context.Connections
            .Where(c => (c.RequesterId == userId || c.AddresseeId == userId) && c.Status == ConnectionStatus.Accepted)
            .ToListAsync();
        return new Response<List<Connection>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<List<Connection>>> GetPendingRequestsAsync(int userId)
    {
        var list = await context.Connections
            .Where(c => c.AddresseeId == userId && c.Status == ConnectionStatus.Pending)
            .ToListAsync();
        return new Response<List<Connection>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> RemoveConnectionAsync(int connectionId, int userId)
    {
        var conn = await context.Connections.FindAsync(connectionId);
        if (conn == null)
            return new Response<string>(HttpStatusCode.NotFound, "Connection not found");
        if (conn.RequesterId != userId && conn.AddresseeId != userId)
            return new Response<string>(HttpStatusCode.Forbidden, "Not your connection");

        context.Connections.Remove(conn);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Connection removed");
    }
}
