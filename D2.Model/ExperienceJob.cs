using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace D2.Model
{
    public class ExperienceJob
    {
        private readonly CharacterDataLoader characterDataLoader;
        private Character characterData;
        private readonly List<long> experienceHistory = new List<long>();
        private readonly List<long> goldHistory = new List<long>();
        private long previousExperience;
        private long previousGold;
        private const int TimeBetweenInfoPrintsInSeconds = 5;
        private const int TimeBetweenCharacterRefreshInSeconds = 1;
        private readonly DateTime sessionStartedAt = DateTime.Now;
        private const double OneHourInSeconds = 3600.0;
        private int runCounter;
        private readonly List<long> expOfRuns = new List<long>();
        private DateTime previousChangedAt;

        public ExperienceJob(CharacterDataLoader characterDataLoader)
        {
            this.characterDataLoader = characterDataLoader;
            this.characterData = this.characterDataLoader.GetCurrentCharacterData();

            this.previousExperience = GetExperience();
            this.previousGold = GetGold();
            this.previousChangedAt = GetCharacterLastChangedAt();
        }

        public void Run()
        {
            Console.Clear();
            Console.WriteLine($"{this.characterData.Name} Level: {this.characterData.Level}\n");
            var headerFormatting = "{0,-20}{1,-15}{2,-10}{3,-10}{4,-10}{5,-10}{6,-10}{7,-15}";
            Console.WriteLine(headerFormatting, "Date","LvlUp Eta", "Exp/h","Gold/h","LvlUp","Timer","Runs","LvlUp Runs ETA");

            for (int i = 0; i > -1; i++)
            {
                this.characterData = this.characterDataLoader.GetCurrentCharacterData();

                var currentExperience = GetExperience();

                var currentLastChangedAt = this.characterData.LastChangedAt;
                if (currentLastChangedAt > this.previousChangedAt)
                {
                    this.runCounter++;
                    this.expOfRuns.Add(currentExperience - this.previousExperience);
                    this.previousChangedAt = currentLastChangedAt;
                }

                var currentGold = GetGold();

                UpdateHistory(currentExperience,currentGold);

                var currentGoldPerHour = GetCurrentGoldPerHour();

                var experienceThresholdForLevelUp = GetExperienceThresholdForLevelUp();
                var currentExperiencePerHour = GetCurrentExperiencePerHour();
                var experienceDelta = experienceThresholdForLevelUp - currentExperience;

                double hoursForLevelUp = 999999999;
                double runsForLevelUp = 999999999;
                if (currentExperiencePerHour > 0)
                {
                    hoursForLevelUp = experienceDelta / currentExperiencePerHour;
                    runsForLevelUp = experienceDelta / this.expOfRuns.Average();
                }

                if (i % TimeBetweenInfoPrintsInSeconds == 0)
                {
                    Console.Write($"\r{headerFormatting}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        ReadabilityHelper.ConvertToHoursAndMinutesText(hoursForLevelUp),
                        ReadabilityHelper.ConvertToSi(currentExperiencePerHour),
                        ReadabilityHelper.ConvertToSi(currentGoldPerHour),
                        $"{CalculateLvlUpProgressPercentage():0.00}" +"%",
                        $"{(DateTime.Now - this.sessionStartedAt).TotalMinutes:0}" + "m",
                        this.runCounter,
                        $"{runsForLevelUp:0}");
                }

                Thread.Sleep(TimeBetweenCharacterRefreshInSeconds * 1000);
            }
        }

        private double GetCurrentExperiencePerHour()
        {
            var result = this.experienceHistory.Average() * (OneHourInSeconds / TimeBetweenCharacterRefreshInSeconds);
            return result;
        }

        private double GetCurrentGoldPerHour()
        {
            var result = this.goldHistory.Average() * (OneHourInSeconds / TimeBetweenCharacterRefreshInSeconds);
            return result;
        }

        private void UpdateHistory(long currentExperience, long currentGold)
        {
            this.experienceHistory.Add(currentExperience - this.previousExperience);
            this.previousExperience = currentExperience;

            this.goldHistory.Add(currentGold - this.previousGold);
            this.previousGold = currentGold;
        }

        private long GetExperience() => this.characterData.Experience;

        private long GetGold() => this.characterData.GoldInventory + this.characterData.GoldStash;

        private long GetExperienceThresholdForLevelUp() => this.characterData.NextLevelAtExperience;

        private long GetExperienceThresholdOfCurrentLevel() => this.characterData.ExperienceRequiredForCurrentLevel;

        private DateTime GetCharacterLastChangedAt() => this.characterData.LastChangedAt;

        // TODO: naming
        private double CalculateLvlUpProgressPercentage()
        {
            var currentExp = GetExperience();
            var thresholdCurrentLevel = GetExperienceThresholdOfCurrentLevel();
            var thresholdNextLevel = GetExperienceThresholdForLevelUp();
            var differenceBetweenTresholds = thresholdNextLevel - thresholdCurrentLevel;
            double experienceCollectedOnThisLevel = currentExp - thresholdCurrentLevel;
            return (experienceCollectedOnThisLevel / differenceBetweenTresholds) * 100;
        }
    }
}