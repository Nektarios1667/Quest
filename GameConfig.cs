using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Quest;

public sealed class GameplaySettings
{
    public int PlayerBaseSpeed { get; set; } = 300;
    public int MaxStack { get; set; } = 10;
    public int DayLength { get; set; } = 600;
    public int PlayerBaseHealth { get; set; } = 100;
    public int PlayerBaseHunger { get; set; } = 20;
    public int SecondsPerHungerLoss { get; set; } = 45;
    public int SecondsPerNaturalRegen { get; set; } = 3;
    public int NaturalRegenRate { get; set; } = 5;
    public int SecondsPerStarvation { get; set; } = 5;
    public int StarvationRate { get; set; } = 5;
}

public static class GameConfig
{
    public static GameplaySettings Gameplay { get; private set; } = LoadGameplay();

    public static void Reload()
    {
        Gameplay = LoadGameplay();
    }

    private static GameplaySettings LoadGameplay()
    {
        foreach (string candidate in GetCandidatePaths())
        {
            if (!File.Exists(candidate))
                continue;

            try
            {
                string json = File.ReadAllText(candidate);
                var settings = JsonSerializer.Deserialize<GameplaySettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                });

                if (settings != null)
                    return settings;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to load gameplay config at '{candidate}': {ex.Message}");
            }
        }

        return new GameplaySettings();
    }

    private static IEnumerable<string> GetCandidatePaths()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddPath(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            paths.Add(System.IO.Path.GetFullPath(value));
        }

        AddPath(System.IO.Path.Combine(AppContext.BaseDirectory, "GameData", "Config", "gameplay.json"));
        AddPath(System.IO.Path.Combine(AppContext.BaseDirectory, "GameData", "Config", "Gameplay.json"));
        AddPath(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "GameData", "Config", "gameplay.json"));
        AddPath(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "GameData", "Config", "Gameplay.json"));
        AddPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameData", "Config", "gameplay.json"));
        AddPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameData", "Config", "Gameplay.json"));

        return paths;
    }
}
