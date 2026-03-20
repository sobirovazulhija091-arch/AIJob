namespace Domain.DTOs;

public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PagedQuery : PagedRequest
{
}
