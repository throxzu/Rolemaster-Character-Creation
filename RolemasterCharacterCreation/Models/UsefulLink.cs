namespace RolemasterCharacterCreation.Models;

// A GM-curated bookmark to a useful page on the internet, shown on the GM "Useful Links" page.
public class UsefulLink
{
    public int Id { get; set; }
    public required string Title { get; set; }

    // Full URL including scheme, e.g. "https://rolemaster.org".
    public required string Url { get; set; }

    // Optional note describing why the link is useful.
    public string? Description { get; set; }

    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
