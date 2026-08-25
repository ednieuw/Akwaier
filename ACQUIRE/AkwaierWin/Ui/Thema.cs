using System.Drawing;

namespace Akwaier.Ui;

/// <summary>
/// Een van de twee weergaven. De kleuren van "Retro" zijn de paren uit
/// DATA 29000-29007 van het origineel, omgezet naar de acht kleuren van MODE 6.
/// </summary>
public sealed class Thema
{
    public string Naam { get; private init; }
    public string Lettertype { get; private init; }

    public Color Achtergrond { get; private init; }
    public Color Voorgrond { get; private init; }
    public Color Gedempt { get; private init; }
    public Color Raster { get; private init; }
    public Color LeegVak { get; private init; }
    public Color Stip { get; private init; }
    public Color Los { get; private init; }
    public Color KopAchter { get; private init; }
    public Color KopVoor { get; private init; }
    public Color Band { get; private init; }
    public Color Keuze { get; private init; }
    public Color Wenk { get; private init; }
    public Color LogAchter { get; private init; }
    public Color LogVoor { get; private init; }
    public Color Knop { get; private init; }
    public Color KnopVoor { get; private init; }

    /// <summary>Kleur van een knop waar de muis boven zweeft.</summary>
    public Color KnopZweef { get; private init; }

    /// <summary>Achter- en voorgrondkleur per keten, plaats 1..8.</summary>
    public Color[] KetenAchter { get; private init; }
    public Color[] KetenVoor { get; private init; }

    /// <summary>Toont de kolomkop als "123456789012345", zoals op het BBC-scherm.</summary>
    public bool BbcKolomkop { get; private init; }

    private static Color K(int rgb) => Color.FromArgb(unchecked((int)0xFF000000) | rgb);

    public static readonly Thema Modern = new()
    {
        Naam = "Modern",
        Lettertype = "Segoe UI",
        Achtergrond = K(0xECEFF1),
        Voorgrond = K(0x212121),
        Gedempt = K(0x78909C),
        Raster = K(0xCFD8DC),
        LeegVak = K(0xF5F7F8),
        Stip = K(0xDBE2E6),
        Los = K(0x90A4AE),
        KopAchter = K(0x37474F),
        KopVoor = K(0xFFFFFF),
        Band = K(0xE3E8EA),
        Keuze = K(0xFF6F00),
        Wenk = K(0xB0BEC5),
        LogAchter = K(0xFFFFFF),
        LogVoor = K(0x37474F),
        Knop = K(0x37474F),
        KnopVoor = K(0xFFFFFF),
        KnopZweef = K(0x546E7A),
        BbcKolomkop = false,
        KetenAchter = new[]
        {
            Color.Empty,
            K(0x455A64), K(0x2E7D32), K(0xF9A825), K(0x1565C0),
            K(0x8E24AA), K(0x00838F), K(0x5D4037), K(0xC62828),
        },
        KetenVoor = new[]
        {
            Color.Empty,
            K(0xFFFFFF), K(0xFFFFFF), K(0x3E2723), K(0xFFFFFF),
            K(0xFFFFFF), K(0xFFFFFF), K(0xFFFFFF), K(0xFFFFFF),
        },
    };

    public static readonly Thema Retro = new()
    {
        Naam = "Retro BBC",
        Lettertype = "Consolas",
        Achtergrond = K(0x000000),
        Voorgrond = K(0xFFFFFF),
        Gedempt = K(0x00FFFF),
        Raster = K(0x303030),
        LeegVak = K(0x000000),
        Stip = K(0x404040),
        Los = K(0x00FF00),
        KopAchter = K(0x00FFFF),
        KopVoor = K(0x000000),
        Band = K(0x101010),
        Keuze = K(0xFFFF00),
        Wenk = K(0x005000),
        LogAchter = K(0x000000),
        LogVoor = K(0x00FF00),
        Knop = K(0x0000BB),
        KnopVoor = K(0xFFFFFF),
        KnopZweef = K(0x3333FF),
        BbcKolomkop = true,
        // Kleurparen uit DATA 29000-29007 van het origineel, met twee aanpassingen
        // voor de leesbaarheid: het vlak van keten A staat op 20,20,20 in plaats
        // van zuiver zwart, anders valt het weg tegen de achtergrond, en de F
        // krijgt een zwarte letter op het lichtblauwe vlak in plaats van een witte.
        KetenAchter = new[]
        {
            Color.Empty,
            K(0x141414), K(0x00FF00), K(0xFFFF00), K(0x0000FF),
            K(0xFF00FF), K(0x00FFFF), K(0xFFFFFF), K(0xFF0000),
        },
        KetenVoor = new[]
        {
            Color.Empty,
            K(0xFFFFFF), K(0xFFFFFF), K(0x0000FF), K(0xFFFFFF),
            K(0xFFFFFF), K(0x000000), K(0x0000FF), K(0xFFFFFF),
        },
    };
}
