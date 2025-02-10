using Customer.Profile.V1;

namespace WebApp.Models;

public static class Mappings
{
    public static ProfileModel ToProfileModel(this Profile profile)
    {
        return new ProfileModel(profile.Id, profile.FullName, profile.BirthDate != null ? DateOnly.FromDateTime(profile.BirthDate.ToDateTime()) : null, profile.Phone);
    }
}
