namespace RolemasterCharacterCreation.Identity;

// Marks a freshly-invited player who must still complete their profile (set email + a real
// password) before using the app. Stored as an AspNetUserClaim so it rides along in the auth
// cookie — the middleware gate can check it without a per-request database hit. The claim is
// added when the GM creates the player and removed once the profile is completed.
public static class InviteClaims
{
    public const string Type = "ProfileSetup";
    public const string Required = "Required";
}
