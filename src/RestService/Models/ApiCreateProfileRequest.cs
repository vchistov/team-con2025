namespace RestService.Models;

public record ApiCreateProfileRequest(string Name, string Phone, DateOnly? BirthDate);
