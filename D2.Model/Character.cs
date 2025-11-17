using System;

namespace D2.Model
{
    public class Character
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string GameType { get; set; }
        public string Game { get; set; }
        public string Class { get; set; }
        public DateTime LastChangedAt { get; set; }

        public int Level { get; set; }
        public long Experience { get; set; }
        public long NextLevelAtExperience { get; set; }
        public long ExperienceRequiredForCurrentLevel { get; set; }

        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Vitality { get; set; }
        public int Energy { get; set; }
        public int StatusLeft { get; set; }

        public int Life { get; set; }
        public int LifeMax { get; set; }

        public int Stamina { get; set; }
        public int StaminaMax { get; set; }

        public int Mana { get; set; }
        public int ManaMax { get; set; }

        public int ResistFire { get; set; }
        public int ResistIce { get; set; }
        public int ResistLightning { get; set; }

        public int GoldInventory { get; set; }
        public int GoldStash { get; set; }

        public int SkillLeft { get; set; }
    }
}