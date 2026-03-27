using Domain.DTOs;

public interface IConnectionService
{
    Task<Response<string>> SendRequestAsync(int requesterId, int addresseeId);
    Task<Response<string>> SendRequestByEmailAsync(int requesterId, string addresseeEmail);
    Task<Response<string>> RespondToRequestAsync(int connectionId, int userId, ConnectionStatus status);
    Task<Response<Connection>> GetByIdAsync(int id);
    Task<Response<List<Connection>>> GetByUserIdAsync(int userId);
    Task<Response<List<Connection>>> GetPendingRequestsAsync(int userId);
    Task<Response<List<Connection>>> GetAllForUserAsync(int userId);
    Task<Response<string>> RemoveConnectionAsync(int connectionId, int userId);
}
