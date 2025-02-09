namespace GrpcService.DataAccess.Records;

using System.ComponentModel.DataAnnotations;

public class ProfileRecord
{
    [Key]
    public long Id { get; init; }

    [Required]
    public required string FullName { get; init; }

    [Required]
    public required string Phone { get; init; }
    
    public DateOnly? BirthDate { get; init; }

    public ICollection<AvatarRecord>? Avatars { get; init; }
}
