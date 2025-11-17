namespace D2.Model;

public record Character
{
    public string Name { get; init; } = string.Empty;
    public DateTime LastChangedAt { get; init; }

    public int Level { get; init; }
    public long Experience { get; init; }
    public long NextLevelAtExperience { get; init; }
    public long ExperienceRequiredForCurrentLevel { get; init; }
        
    public int GoldInventory { get; init; }
    public int GoldStash { get; init; }

    public int Strength { get; init; }
    public int Dexterity { get; init; }
    public int Vitality { get; init; }
    public int Energy { get; init; }
    public int StatusLeft { get; init; }

    public int Life { get; init; }
    public int LifeMax { get; init; }

    public int Stamina { get; init; }
    public int StaminaMax { get; init; }

    public int Mana { get; init; }
    public int ManaMax { get; init; }


    public int SkillLeft { get; init; }
}