namespace D2.Model;

public static class ReadabilityHelper
{
    // these magic numbers ensure an error below 1%.
    public static string ConvertToSi(double input)
    {
        var absolute = Math.Abs(input);

        if (absolute < 50500)
        {
            return $"{input:0}";
        }

        if (absolute >= 50500 && absolute < 999500)
        {
            return $"{(input/Math.Pow(10,3)):0}k";
        }

        if (absolute >= 999500 && absolute < 5100000)
        {
            return $"{(input/Math.Pow(10,6)):0.00}M";
        }

        if (absolute >= 5100000 && absolute < 999500000)
        {
            return $"{(input/Math.Pow(10,6)):0.0}M";
        }

        return $"{(input/Math.Pow(10,9)):0.00}G";
    }

    public static string ConvertToHoursAndMinutesText(double inputNumber)
    {
        var hours = (int)inputNumber;
        var minutes = (int)((inputNumber - hours) * 60);

        var result = hours.Equals(0) ? $"{minutes}m" : $"{hours:00}h {minutes:00}m";

        return result;
    }
}