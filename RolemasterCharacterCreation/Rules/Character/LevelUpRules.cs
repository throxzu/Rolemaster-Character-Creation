using RolemasterCharacterCreation.Models;

namespace RolemasterCharacterCreation.Rules;

/// <summary>
/// When a character may advance, and when the creation wizard is allowed to run.
///
/// The wizard serves two purposes only — finishing initial creation, and carrying out a
/// level-up — and both of them are things that have already been started. Holding that in
/// one place matters: the page used to infer "a level-up is under way" from
/// <c>Level &gt; 1</c>, which stays true forever once a character has levelled, so a player
/// who finished a level-up could re-enter the wizard and draw a fresh DP budget. The
/// authoritative marker is the frozen baseline, not the level.
/// </summary>
public static class LevelUpRules
{
    /// <summary>Experience needed for the next level, as a cumulative running total.</summary>
    public const int XpPerLevel = 10000;

    /// <summary>
    /// <c>WizardStep</c> once initial creation has been completed — the index of the last
    /// creation step (see <c>CharacterWizard.Steps</c>).
    /// </summary>
    public const int CreationFinishedStep = 5;

    public static int XpForNextLevel(Character c) => XpPerLevel * c.Level;

    public static bool HasXpForNextLevel(Character c) => c.CurrentXp >= XpForNextLevel(c);

    /// <summary>
    /// A level-up is in flight exactly while its frozen skill baseline exists. Level Up
    /// writes it; Finish and Cancel clear it.
    /// </summary>
    public static bool IsLevelUpInProgress(Character c) => !string.IsNullOrEmpty(c.LevelUpBaselineJson);

    /// <summary>
    /// The marker check is load-bearing: the level is not incremented until Finish, so a
    /// character part-way through its first level-up is still Level 1 and would otherwise
    /// look like an unfinished creation.
    /// </summary>
    public static bool IsCreationInProgress(Character c) =>
        !IsLevelUpInProgress(c) && c.Level == 1 && c.WizardStep < CreationFinishedStep;

    /// <summary>
    /// Whether the wizard has anything to do for this character. Having enough experience is
    /// deliberately *not* enough: that enables the Level Up button, which is the only path
    /// that freezes the baseline the wizard measures spending against.
    /// </summary>
    public static bool CanEnterWizard(Character c) =>
        IsCreationInProgress(c) || IsLevelUpInProgress(c);
}
