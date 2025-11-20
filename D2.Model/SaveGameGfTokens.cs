namespace D2.Model;
// https://pastebin.com/cVTNmGzZ some phrozen keep info in link.
// Adjusted the info from the link, StatusLeft has length 10 and SkillLeft starts 2 later and has length 8.
// Life Mana Stamina is unshifted and needs to be /256 to have the right value.

// TODO: check if stuff is left out when value is 0, i guess key and value disappear in those cases.
// status left, skill left can be the reason of those issues.

public static class SaveGameGfTokens
{
    public static readonly ParserToken Strength = new(25, 10, "Strength", "000000000");
    public static readonly ParserToken Energy = new(44, 10, "Energy", "100000000");
    public static readonly ParserToken Dexterity = new(63, 10, "Dexterity", "010000000");
    public static readonly ParserToken Vitality = new(82, 10, "Vitality", "110000000");
    public static readonly ParserToken StatusLeft = new(101, 10, "StatusLeft", "001000000");
    public static readonly ParserToken SkillLeft = new(120, 8, "SkillLeft", "101000000");

    public static readonly ParserToken Life = new(137, 21, "Life", "011000000");
    public static readonly ParserToken LifeMax = new(167, 21, "LifeMax", "111000000");
    public static readonly ParserToken Mana = new(197, 21, "Mana", "000100000");
    public static readonly ParserToken ManaMax = new(227, 21, "ManaMax", "100100000");
    public static readonly ParserToken Stamina = new(257, 21, "Stamina", "010100000");
    public static readonly ParserToken StaminaMax = new(287, 21, "StaminaMax", "110100000");

    public static readonly ParserToken Level = new(317, 7, "Level", "001100000");
    public static readonly ParserToken Experience = new(333, 32, "Experience", "101100000");

    // TODO: solve the gold issue, instead 0 in test it is 104868.
    public static readonly ParserToken GoldInventory = new(374, 25, "GoldInventory", "011100000");
    public static readonly ParserToken GoldStash = new(408, 25, "GoldStash", "111100000");
}