using System.Text.Json;
using System.Text.Json.Serialization;

namespace Akwaier.Engine;

/// <summary>Eén regel uit de scorelijst.</summary>
public sealed class Scoreregel
{
    [JsonPropertyName("naam")] public string Naam { get; set; } = "";
    [JsonPropertyName("vermogen")] public int Vermogen { get; set; }
}

/// <summary>De inhoud van het scorebestand: de gekozen taal en de ranglijst.</summary>
public sealed class Scorebestand
{
    /// <summary>"nl" of "en"; leeg als er nog nooit een taal gekozen is.</summary>
    [JsonPropertyName("taal")] public string TaalCode { get; set; } = "";
    [JsonPropertyName("scores")] public List<Scoreregel> Scores { get; set; } = new();
}

/// <summary>
/// Vervangt het bestand "SCORE" uit het origineel (regels 4050-4195) door een
/// leesbaar JSON-bestand naast het programma. Behalve de ranglijst staat ook de
/// taalkeuze erin, zodat het spel de volgende keer in dezelfde taal begint.
/// </summary>
public static class Scorelijst
{
    private const int Bewaar = 30;

    private static readonly JsonSerializerOptions Opties = new() { WriteIndented = true };

    public static string Pad => Path.Combine(
        Path.GetDirectoryName(Environment.ProcessPath) ?? ".", "akwaier-scores.json");

    /// <summary>
    /// Leest het bestand. Een bestand van vóór de taalkeuze bevat alleen een
    /// lijst; dat wordt nog gewoon gelezen.
    /// </summary>
    public static Scorebestand Lees()
    {
        try
        {
            if (!File.Exists(Pad)) return new Scorebestand();
            string tekst = File.ReadAllText(Pad);
            using var doc = JsonDocument.Parse(tekst);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                return new Scorebestand
                {
                    Scores = JsonSerializer.Deserialize<List<Scoreregel>>(tekst) ?? new(),
                };
            return JsonSerializer.Deserialize<Scorebestand>(tekst) ?? new Scorebestand();
        }
        catch (Exception)
        {
            // Een stukgelopen of met de hand aangepast bestand mag het spel niet
            // tegenhouden; de lijst begint dan gewoon opnieuw.
            return new Scorebestand();
        }
    }

    private static void Schrijf(Scorebestand bestand)
    {
        try { File.WriteAllText(Pad, JsonSerializer.Serialize(bestand, Opties)); }
        catch (Exception) { /* alleen-lezen map: dan geen lijst, maar wel doorspelen */ }
    }

    public static List<Scoreregel> Bijwerken(IEnumerable<(string Naam, int Vermogen)> nieuw)
    {
        var bestand = Lees();
        bestand.Scores.AddRange(nieuw.Select(n => new Scoreregel
        {
            Naam = n.Naam,
            Vermogen = n.Vermogen,
        }));
        bestand.Scores = bestand.Scores.OrderByDescending(r => r.Vermogen).Take(Bewaar).ToList();
        bestand.TaalCode = Taal.Code;
        Schrijf(bestand);
        return bestand.Scores;
    }

    /// <summary>
    /// Zet de taal op de bewaarde keuze. Staat er nog niets in het bestand, dan
    /// beslist de taalinstelling van Windows.
    /// </summary>
    public static void LaadTaal()
    {
        string code = File.Exists(Pad) ? Lees().TaalCode : "";
        if (string.IsNullOrWhiteSpace(code)) Taal.Engels = Taal.PcIsEngels();
        else Taal.ZetCode(code);
    }

    public static void BewaarTaal()
    {
        var bestand = Lees();
        if (bestand.TaalCode == Taal.Code && File.Exists(Pad)) return;
        bestand.TaalCode = Taal.Code;
        Schrijf(bestand);
    }
}
