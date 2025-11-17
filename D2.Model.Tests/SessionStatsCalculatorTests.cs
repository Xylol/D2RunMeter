using FluentAssertions;
using NUnit.Framework;

namespace D2.Model.Tests;

[TestFixture]
public class SessionStatsCalculatorTests
{
    [Test]
    public void CalculateExperiencePerHour_With5MinuteSession_ReturnsCorrectRate()
    {
        // Arrange
        var sessionStart = new DateTime(2025, 1, 1, 12, 0, 0);
        var currentTime = new DateTime(2025, 1, 1, 12, 5, 0);  // 5 minutes later
        var sessionStartExp = 1_000_000L;
        var currentExp = 1_010_000L;  // 10k exp gained

        // Act
        var result = SessionStatsCalculator.CalculateExperiencePerHour(
            currentExp, sessionStartExp, sessionStart, currentTime);

        // Assert
        result.Should().BeApproximately(120_000, 1);  // 10k in 5min = 120k/hour
    }

    [Test]
    public void CalculateExperiencePerHour_With3HourSession_ReturnsCorrectRate()
    {
        // Arrange
        var sessionStart = new DateTime(2025, 1, 1, 12, 0, 0);
        var currentTime = new DateTime(2025, 1, 1, 15, 0, 0);  // 3 hours later
        var sessionStartExp = 1_000_000L;
        var currentExp = 1_360_000L;  // 360k exp gained

        // Act
        var result = SessionStatsCalculator.CalculateExperiencePerHour(
            currentExp, sessionStartExp, sessionStart, currentTime);

        // Assert
        result.Should().BeApproximately(120_000, 1);  // Same rate as 5min test
    }

    [Test]
    public void CalculateExperiencePerHour_VeryShortSession_ReturnsZero()
    {
        // Arrange
        var sessionStart = new DateTime(2025, 1, 1, 12, 0, 0);
        var currentTime = sessionStart.AddMilliseconds(1);  // 1ms later
        var sessionStartExp = 1_000_000L;
        var currentExp = 1_010_000L;

        // Act
        var result = SessionStatsCalculator.CalculateExperiencePerHour(
            currentExp, sessionStartExp, sessionStart, currentTime);

        // Assert
        result.Should().Be(0);  // Prevents division by very small number
    }

    [Test]
    public void CalculateGoldPerHour_WithGoldGained_ReturnsCorrectRate()
    {
        // Arrange
        var sessionStart = new DateTime(2025, 1, 1, 12, 0, 0);
        var currentTime = new DateTime(2025, 1, 1, 12, 30, 0);  // 30 min
        var sessionStartGold = 15_000L;
        var currentGold = 20_000L;  // 5k gold gained

        // Act
        var result = SessionStatsCalculator.CalculateGoldPerHour(
            currentGold, sessionStartGold, sessionStart, currentTime);

        // Assert
        result.Should().BeApproximately(10_000, 1);  // 5k in 30min = 10k/hour
    }

    [Test]
    public void CalculateLevelUpProgressPercentage_HalfwayThrough_Returns50Percent()
    {
        // Arrange
        var currentLevelExp = 1_755_000_000L;  // Level 85 requires
        var nextLevelExp = 1_935_000_000L;     // Level 86 requires
        var currentExp = 1_845_000_000L;       // Halfway between

        // Act
        var result = SessionStatsCalculator.CalculateLevelUpProgressPercentage(
            currentExp, currentLevelExp, nextLevelExp);

        // Assert
        result.Should().BeApproximately(50.0, 0.01);
    }

    [Test]
    public void CalculateLevelUpProgressPercentage_AtLevelStart_Returns0Percent()
    {
        // Arrange
        var currentLevelExp = 1_000_000L;
        var nextLevelExp = 1_200_000L;
        var currentExp = 1_000_000L;  // Just hit the level

        // Act
        var result = SessionStatsCalculator.CalculateLevelUpProgressPercentage(
            currentExp, currentLevelExp, nextLevelExp);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void CalculateLevelUpProgressPercentage_AlmostAtNextLevel_ReturnsNear100()
    {
        // Arrange
        var currentLevelExp = 1_000_000L;
        var nextLevelExp = 1_200_000L;
        var currentExp = 1_199_000L;  // 1k away from level

        // Act
        var result = SessionStatsCalculator.CalculateLevelUpProgressPercentage(
            currentExp, currentLevelExp, nextLevelExp);

        // Assert
        result.Should().BeApproximately(99.5, 0.1);
    }

    [Test]
    public void CalculateLevelUpProgressPercentage_NoExpDifference_ReturnsZero()
    {
        // Arrange - edge case where levels have same exp (shouldn't happen in D2)
        var currentLevelExp = 1_000_000L;
        var nextLevelExp = 1_000_000L;
        var currentExp = 1_000_000L;

        // Act
        var result = SessionStatsCalculator.CalculateLevelUpProgressPercentage(
            currentExp, currentLevelExp, nextLevelExp);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void CalculateHoursForLevelUp_Need200kExpAt50kPerHour_Returns4Hours()
    {
        // Arrange
        var currentExp = 1_100_000L;
        var nextLevelExp = 1_300_000L;
        var expPerHour = 50_000.0;

        // Act
        var result = SessionStatsCalculator.CalculateHoursForLevelUp(
            currentExp, nextLevelExp, expPerHour);

        // Assert
        result.Should().BeApproximately(4.0, 0.01);
    }

    [Test]
    public void CalculateHoursForLevelUp_ZeroExpPerHour_ReturnsMaxValue()
    {
        // Arrange
        var currentExp = 1_100_000L;
        var nextLevelExp = 1_300_000L;
        var expPerHour = 0.0;

        // Act
        var result = SessionStatsCalculator.CalculateHoursForLevelUp(
            currentExp, nextLevelExp, expPerHour);

        // Assert
        result.Should().Be(double.MaxValue);
    }

    [Test]
    public void CalculateRunsForLevelUp_Need200kExpAt5kPerRun_Returns40Runs()
    {
        // Arrange
        var sessionStartExp = 1_000_000L;
        var currentExp = 1_050_000L;  // 50k gained
        var nextLevelExp = 1_250_000L;  // Need 200k more
        var runCounter = 10;  // 10 runs done = 5k exp/run average

        // Act
        var result = SessionStatsCalculator.CalculateRunsForLevelUp(
            currentExp, sessionStartExp, nextLevelExp, runCounter);

        // Assert
        result.Should().BeApproximately(40.0, 0.01);  // 200k / 5k = 40 runs
    }

    [Test]
    public void CalculateRunsForLevelUp_NoRunsCompleted_ReturnsMaxValue()
    {
        // Arrange
        var sessionStartExp = 1_000_000L;
        var currentExp = 1_000_000L;
        var nextLevelExp = 1_200_000L;
        var runCounter = 0;

        // Act
        var result = SessionStatsCalculator.CalculateRunsForLevelUp(
            currentExp, sessionStartExp, nextLevelExp, runCounter);

        // Assert
        result.Should().Be(double.MaxValue);
    }

    [Test]
    public void CalculateRunsForLevelUp_VariableRunExp_UsesTotalAverage()
    {
        // Arrange - simulates runs with different exp amounts
        var sessionStartExp = 1_000_000L;
        var currentExp = 1_030_000L;  // Gained: 12k, 8k, 10k = 30k total
        var nextLevelExp = 1_150_000L;  // Need 120k more
        var runCounter = 3;  // Average = 30k/3 = 10k per run

        // Act
        var result = SessionStatsCalculator.CalculateRunsForLevelUp(
            currentExp, sessionStartExp, nextLevelExp, runCounter);

        // Assert
        result.Should().BeApproximately(12.0, 0.01);  // 120k / 10k = 12 runs
    }

    [Test]
    public void CalculateSessionMinutes_47MinutesElapsed_Returns47()
    {
        // Arrange
        var sessionStart = new DateTime(2025, 1, 1, 12, 0, 0);
        var currentTime = new DateTime(2025, 1, 1, 12, 47, 0);

        // Act
        var result = SessionStatsCalculator.CalculateSessionMinutes(sessionStart, currentTime);

        // Assert
        result.Should().Be(47.0);
    }

    [Test]
    public void CalculateSessionMinutes_MultipleHours_ReturnsCorrectMinutes()
    {
        // Arrange
        var sessionStart = new DateTime(2025, 1, 1, 10, 15, 0);
        var currentTime = new DateTime(2025, 1, 1, 13, 45, 30);  // 3h 30m 30s

        // Act
        var result = SessionStatsCalculator.CalculateSessionMinutes(sessionStart, currentTime);

        // Assert
        result.Should().BeApproximately(210.5, 0.1);  // 3*60 + 30 + 0.5
    }
}
