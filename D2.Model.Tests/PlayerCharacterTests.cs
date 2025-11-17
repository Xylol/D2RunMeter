using NUnit.Framework;
using NUnit.Framework.Legacy;
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
        this.saveGameContent = loader.GetSaveGameContent(saveGameStream);
        this.craftedSaveGameContent = loader.GetSaveGameContent(craftedSaveGameStream);
        this.rasanSaveGameContent = loader.GetSaveGameContent(rasanSaveGameStream);
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
            Strength = 16,
            Energy = 25,
            Dexterity = 27,
            Vitality = 15,
            StatusLeft = 2,
            SkillLeft = 1,
            Life = 44,
            LifeMax = 46,
            Mana = 27,
            ManaMax = 27,
            Stamina = 80,
            StaminaMax = 80,
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
            Strength = 100,
            Energy = 15,
            Dexterity = 130,
            Vitality = 305,
            StatusLeft = 0,
            SkillLeft = 0,
            Life = 1450,
            LifeMax = 1135,
            Stamina = 554,
            StaminaMax = 459,
            Mana = 255,
            ManaMax = 150,
            GoldInventory = 5175,
            GoldStash = 389917,
            Level = 91,
            Experience = 1766245478,
            ExperienceRequiredForCurrentLevel = 1764543065,
            NextLevelAtExperience = 1923762030
        };

        // Act
        //var actual = new SaveGame(this.rasanSaveGameContent,this.parser).GetPlayerCharacter();
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
        var actual = saveGame.GetSubstringStartingWithAsciiGF();

        // Assert
        ClassicAssert.AreEqual(expected, actual);
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
        ClassicAssert.AreEqual(expectedName,actualName);
    }
}