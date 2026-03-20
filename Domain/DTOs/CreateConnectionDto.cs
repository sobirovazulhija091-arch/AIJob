namespace Domain.DTOs;

public class CreateConnectionDto
{
    public int AddresseeId { get; set; }  // User to connect with (RequesterId comes from auth)
}
