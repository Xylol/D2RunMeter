using NUnit.Framework;
using FluentAssertions;

namespace D2.Model.Tests;

[TestFixture]
public class PlayerCharacterTests
{
    private byte[] saveGameContent = null!;
    private byte[] craftedSaveGameContent = null!;
    private byte[] rasanSaveGameContent = null!;

    [SetUp]
    public void SetUp()
    {
        var saveGameStream = TestHelper.ResourceStream("D2.Model.Tests.SaveGames.Testnec.d2s");
        var craftedSaveGameStream = TestHelper.ResourceStream("D2.Model.Tests.SaveGames.HeaderAndGf.d2s");
        var rasanSaveGameStream = TestHelper.ResourceStream("D2.Model.Tests.SaveGames.Rasan.d2s");
        var loader = new ContentLoader();
        saveGameContent = loader.GetSaveGameContent(saveGameStream);
        craftedSaveGameContent = loader.GetSaveGameContent(craftedSaveGameStream);
        rasanSaveGameContent = loader.GetSaveGameContent(rasanSaveGameStream);
    }

    [Test]
    public void WhenWeLoad_SaveGameFromFile_WeAssertAllAttributes()
    {
        // Arrange
        var changedDate = new DateTime(2020,03,22);
        var expected = new Character
        {
            Name = "testnec",
            LastChangedAt = changedDate,
            Level = 2,
            Experience = 541,
            GoldInventory = 0,
            GoldStash = 0,
            ExperienceRequiredForCurrentLevel = 500,
            NextLevelAtExperience = 1500L
        };


        // Act
        var actual = new SaveGame(this.saveGameContent, changedDate).GetPlayerCharacter();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void GetRasanStats()
    {
        // Arrange
        var changedDate = new DateTime(2020,03,22);
        var expected = new Character()
        {
            Name = "Rasan",
            LastChangedAt = changedDate,
            GoldInventory = 5175,
            GoldStash = 389917,
            Level = 91,
            Experience = 1766245478,
            ExperienceRequiredForCurrentLevel = 1764543065,
            NextLevelAtExperience = 1923762030
        };

        // Act
        var actual = new SaveGame(this.rasanSaveGameContent, changedDate).GetPlayerCharacter();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void GetSubstringStartingWithAsciiGF_WhenSaveGameProvided_WeAssertWeGetGFUntilEndSubstring()
    {
        // Arrange
        var changedDate = new DateTime(2020,03,22);
        var expected = "011001110110011000000000"; // gf in bits starting at index 40
        var saveGame = new SaveGame(this.craftedSaveGameContent, changedDate);

        // Act
        var actual = saveGame.GetSubstringStartingWithAsciiGf();

        // Assert
        actual.Should().Be(expected);
    }

    [Test]
    public void GetName_WhenTestGameProvided_WeAssertNameOfCharacter()
    {
        // Arrange
        var changedDate = new DateTime(2020,03,22);
        var saveGame = new SaveGame(this.saveGameContent, changedDate);
        var expectedName = "testnec";

        // Act
        var actualName = saveGame.GetName();

        // Assert
        actualName.Should().Be(expectedName);
    }
}