namespace RolemasterCharacterCreation.Client;

public sealed class UserInfo
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required string[] Roles { get; init; }
}
