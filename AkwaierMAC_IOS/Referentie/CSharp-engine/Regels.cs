namespace Akwaier.Engine;

/// <summary>
/// De vaste getallen van het spel, zoals ze in AQUIRE.BBC stonden.
/// </summary>
public static class Regels
{
    /// <summary>Het bord is 15 bij 15.</summary>
    public const int Grootte = 15;

    /// <summary>Waarde van een leeg vakje (in het origineel 99 of -1).</summary>
    public const int Leeg = -1;

    /// <summary>Een losse steen die nog bij geen enkele keten hoort.</summary>
    public const int Los = 0;

    public const int AantalKetens = 8;
    public const string Letters = "ABCDEFGH";

    /// <summary>De vier buren, in de volgorde van DATA 1300 uit het origineel.</summary>
    public static readonly (int Dx, int Dy)[] Buren = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    /// <summary>DATA 20000: basisprijs per keten, plaats 0 blijft leeg.</summary>
    public static readonly int[] Basisprijs = { 0, 900, 900, 800, 700, 700, 600, 500, 400 };

    /// <summary>DATA 1980: boven elke drempel komt er 100 bij de prijs.</summary>
    public static readonly int[] Prijsdrempels = { 0, 1, 2, 3, 4, 9, 14, 19, 24, 35 };

    public const int Startgeld = 25_000;

    /// <summary>speelsteen%(8,6): acht stenen in de hand.</summary>
    public const int HandGrootte = 8;

    public const int AantalSpelers = 6;

    /// <summary>W% &gt;= 210 maakt een einde aan het spel.</summary>
    public const int MaxBeurten = 210;

    /// <summary>Een keten groter dan dit beëindigt het spel.</summary>
    public const int EindKetenGrootte = 100;

    /// <summary>Steennummer 1..225 omzetten naar kolom en rij, als regel 2240/2250.</summary>
    public static (int X, int Y) TegelPositie(int n)
    {
        int y = n % Grootte; if (y == 0) y = Grootte;
        int x = n / Grootte; if (x == 0) x = Grootte;
        return (x, y);
    }

    /// <summary>Kolom en rij als "B7": rijletter gevolgd door kolomnummer.</summary>
    public static string Naam(int x, int y) => $"{(char)('A' + y - 1)}{x}";

    public static char KetenLetter(int c) => c >= 1 && c <= 8 ? Letters[c - 1] : '?';
}
