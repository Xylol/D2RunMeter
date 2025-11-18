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
}