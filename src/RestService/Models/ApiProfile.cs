namespace RestService.Models;

public class ApiProfile
{
    public long Id { get; set; }

    public required string Name { get; set; }

    public required string Phone { get; set; }

    public DateOnly? BirthDate { get; set; }
}