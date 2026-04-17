using System.ComponentModel;

namespace GenAIAgent;

public static class WeatherTool
{
    [Description("Gets the current weather for a specified location.")]
    public static string GetWeather([Description("The location to get the weather for.")] string location)
    {
        return $"The current weather in {location} is sunny with a temperature of 25°C.";
    }
}