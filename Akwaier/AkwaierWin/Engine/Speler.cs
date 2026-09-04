namespace Akwaier.Engine;

/// <summary>Eén van de zes spelers, met banksaldo en aandelenbezit.</summary>
public sealed class Speler
{
    public Speler(int nummer, string naam)
    {
        Nummer = nummer;
        Naam = naam;
        Geld = Regels.Startgeld;
    }

    public int Nummer { get; }
    public string Naam { get; set; }

    /// <summary>Waar of de speler door de mens bediend wordt.</summary>
    public bool Mens { get; set; }

    /// <summary>bezit%(): het banksaldo.</summary>
    public int Geld { get; set; }

    /// <summary>aandelen%(): bezit per keten, plaats 1..8.</summary>
    public int[] Aandelen { get; } = new int[Regels.AantalKetens + 1];
}

/// <summary>Wat er van een vakje te zeggen valt, zonder het bord aan te raken.</summary>
public sealed class Zetkans
{
    public int X { get; init; }
    public int Y { get; init; }

    /// <summary>Onwaar als kanniet% waar zou zijn: hier leggen mag niet.</summary>
    public bool Mag { get; init; }

    /// <summary>De aanliggende ketens, ontdubbeld, in de volgorde van DATA 1300.</summary>
    public List<int> Aanliggend { get; init; } = new();

    /// <summary>Aanliggende losse stenen; die worden mee opgeslokt.</summary>
    public List<(int X, int Y)> Losse { get; init; } = new();

    /// <summary>De grootste aanliggende keten; 0 als er geen keten aanligt.</summary>
    public int Winnaar { get; init; }

    /// <summary>kiezen%: meerdere even grote ketens, de speler moet kiezen.</summary>
    public bool Gelijkspel { get; init; }

    public List<int> Gelijken { get; init; } = new();

    public bool Fuseert => Aanliggend.Count > 1;
}

/// <summary>Wat één opgeslokte keten aan bonus uitkeert.</summary>
public sealed class Uitkering
{
    public int Keten { get; init; }
    public int Naar { get; init; }
    public int Grootte { get; init; }
    public int Prijs { get; init; }
    public int Pot { get; init; }
    public Dictionary<int, int> Bedragen { get; } = new();
}

/// <summary>Verslag van één gespeelde steen.</summary>
public sealed class Zetverslag
{
    public int X { get; init; }
    public int Y { get; init; }

    /// <summary>De keten waar de steen bij hoort; 0 voor een losse steen.</summary>
    public int Keten { get; set; }

    public List<int> Opgeslokt { get; } = new();
    public List<Uitkering> Uitkeringen { get; } = new();
}

/// <summary>Wordt gemeld als een zet tegen de regels ingaat.</summary>
public sealed class OngeldigeZet : Exception
{
    public OngeldigeZet(string bericht) : base(bericht) { }
}
