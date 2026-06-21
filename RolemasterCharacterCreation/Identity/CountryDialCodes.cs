namespace RolemasterCharacterCreation.Identity;

// A country a player's phone number can belong to. Drives the country-code dropdown on the
// GM "add player" form, where the chosen Dial is prepended to the local number to form the
// E.164 number stored as the player's UserName/PhoneNumber. Start with Denmark only; add more
// rows here (and a matching /wwwroot/flags/{code}.svg) to support other countries.
public sealed record CountryDialCode(string Code, string Dial, string Name)
{
    // Flag image served from wwwroot, e.g. "/flags/dk.svg".
    public string ImagePath => $"/flags/{Code.ToLowerInvariant()}.svg";
}

public static class CountryDialCodes
{
    public static readonly IReadOnlyList<CountryDialCode> All =
    [
        new("DK", "+45", "Denmark"),
    ];

    public static CountryDialCode Default => All[0];

    public static CountryDialCode ByDial(string dial) =>
        All.FirstOrDefault(c => c.Dial == dial) ?? Default;
}
