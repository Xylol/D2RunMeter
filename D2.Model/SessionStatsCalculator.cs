namespace D2.Model;

public static class SessionStatsCalculator
{
    public static double CalculateExperiencePerHour(long currentExp, long sessionStartExp, DateTime sessionStartTime, DateTime currentTime)
    {
        var elapsedHours = (currentTime - sessionStartTime).TotalHours;
        if (elapsedHours < 0.0001) return 0;

        var expGained = currentExp - sessionStartExp;
        return expGained / elapsedHours;
    }

    public static double CalculateGoldPerHour(long currentGold, long sessionStartGold, DateTime sessionStartTime, DateTime currentTime)
    {
        var elapsedHours = (currentTime - sessionStartTime).TotalHours;
        if (elapsedHours < 0.0001) return 0;

        var goldGained = currentGold - sessionStartGold;
        return goldGained / elapsedHours;
    }

    public static double CalculateLevelUpProgressPercentage(long currentExp, long currentLevelExp, long nextLevelExp)
    {
        var differenceBetweenThresholds = nextLevelExp - currentLevelExp;
        if (differenceBetweenThresholds == 0) return 0;

        double experienceCollectedOnThisLevel = currentExp - currentLevelExp;
        return (experienceCollectedOnThisLevel / differenceBetweenThresholds) * 100;
    }

    public static double CalculateHoursForLevelUp(long currentExp, long nextLevelExp, double expPerHour)
    {
        if (expPerHour <= 0) return double.MaxValue;

        var experienceDelta = nextLevelExp - currentExp;
        return experienceDelta / expPerHour;
    }

    public static double CalculateRunsForLevelUp(long currentExp, long sessionStartExp, long nextLevelExp, int runCounter)
    {
        if (runCounter <= 0) return double.MaxValue;

        var totalSessionExp = currentExp - sessionStartExp;
        var avgExpPerRun = (double)totalSessionExp / runCounter;

        if (avgExpPerRun <= 0) return double.MaxValue;

        var experienceDelta = nextLevelExp - currentExp;
        return experienceDelta / avgExpPerRun;
    }

    public static double CalculateSessionMinutes(DateTime sessionStartTime, DateTime currentTime)
    {
        return (currentTime - sessionStartTime).TotalMinutes;
    }

    public static double CalculateRunsPerHour(int runCounter, DateTime sessionStartTime, DateTime currentTime)
    {
        var elapsedHours = (currentTime - sessionStartTime).TotalHours;
        if (elapsedHours < 0.0001) return 0;

        return runCounter / elapsedHours;
    }
}
