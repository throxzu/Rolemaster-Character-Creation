using Microsoft.AspNetCore.Identity;

namespace RolemasterCharacterCreation.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public List<Character> Characters { get; set; } = [];
}
