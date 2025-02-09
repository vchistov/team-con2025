namespace GrpcService.DataAccess.Records;

using System.ComponentModel.DataAnnotations;

public class AvatarRecord
{
    [Key]
    public long Id { get; init; }

    [Required]
    public long ProfileId { get; init; }

    [Required]
    public required byte[] Content { get; init; }

    public required ProfileRecord Profile { get; init; }
}
