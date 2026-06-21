namespace RolemasterCharacterCreation.Models;

// Single-row settings for the campaign. There is no Campaign entity (all players share one
// world), so this holds the campaign-wide values the GM can edit, e.g. the name and login URL
// woven into the SMS invite sent when a new player is added.
public class CampaignSettings
{
    public int Id { get; set; }

    // Campaign name shown to players and used in the SMS invite text.
    public required string Name { get; set; }

    // Base URL players open to log in, included in the SMS invite, e.g. "https://rolemaster.isager.dk".
    public required string LoginUrl { get; set; }

    // Optional override of the invite SMS body. Supports the placeholders {campaign}, {url},
    // and {password}. When null/blank, a built-in default template is used.
    public string? InviteSmsTemplate { get; set; }
}
