namespace Akwaier.Engine;

/// <summary>De verdeelsleutel van de fusiebonus, tabel uit regels 2810-2819.</summary>
public static class Bonus
{
    private const double Tweederde = 2.0 / 3.0;

    /// <summary>
    /// Levert per rangplaats het deel van de pot. <paramref name="bezit"/> bevat het
    /// aandeelbezit van de zes spelers, aflopend gesorteerd.
    ///
    /// FIX: in het origineel stond de test B=C (regel 2814) voor de test B=C=D
    /// (regel 2816), waardoor die laatste onbereikbaar was. Hier staan de tests
    /// van specifiek naar algemeen.
    /// FIX: de breuken .66/.33/.17/.11/.085/.061 waren afgeronde benaderingen die
    /// samen op 0,99 uitkwamen in plaats van 1. Hier staan de exacte breuken.
    /// </summary>
    public static double[] Verdeling(IReadOnlyList<int> bezit)
    {
        int a = bezit[0], b = bezit[1], c = bezit[2], d = bezit[3], e = bezit[4], f = bezit[5];

        if (b == 0)                                     // maar één aandeelhouder
            return new double[] { 1, 0, 0, 0, 0, 0 };
        if (a == b && b == c)                           // drie gelijk aan kop
            return new[] { 1 / 3.0, 1 / 3.0, 1 / 3.0, 0, 0, 0 };
        if (a == b)                                     // twee gelijk aan kop
            return new double[] { 0.5, 0.5, 0, 0, 0, 0 };
        if (b == c && c == d && d == e && e == f)       // nummers 2 tot en met 6 gelijk
            return new[] { Tweederde, 1 / 15.0, 1 / 15.0, 1 / 15.0, 1 / 15.0, 1 / 15.0 };
        if (b == c && c == d && d == e)                 // nummers 2 tot en met 5 gelijk
            return new[] { Tweederde, 1 / 12.0, 1 / 12.0, 1 / 12.0, 1 / 12.0, 0 };
        if (b == c && c == d)                           // nummers 2 tot en met 4 gelijk
            return new[] { Tweederde, 1 / 9.0, 1 / 9.0, 1 / 9.0, 0, 0 };
        if (b == c)                                     // nummers 2 en 3 gelijk
            return new[] { Tweederde, 1 / 6.0, 1 / 6.0, 0, 0, 0 };

        return new[] { Tweederde, 1 / 3.0, 0, 0, 0, 0 };
    }
}
