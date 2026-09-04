using System.Globalization;

namespace Akwaier.Engine;

/// <summary>
/// Alle teksten die de speler te zien krijgt, in het Nederlands en het Engels.
/// Zowel de speellogica als het scherm halen hun teksten hier op, zodat er bij
/// het omschakelen niets in de verkeerde taal kan achterblijven.
/// </summary>
public static class Taal
{
    /// <summary>false = Nederlands, true = Engels.</summary>
    public static bool Engels { get; set; }

    private static string T(string nl, string en) => Engels ? en : nl;

    /// <summary>
    /// De taal van Windows zelf. Alleen bij een Nederlandse instelling begint
    /// het spel in het Nederlands; alle andere talen krijgen Engels.
    /// </summary>
    public static bool PcIsEngels()
    {
        try
        {
            return !CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                               .Equals("nl", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return true;
        }
    }

    public static string Code => Engels ? "en" : "nl";

    public static void ZetCode(string code) =>
        Engels = !string.Equals(code, "nl", StringComparison.OrdinalIgnoreCase);

    // ------------------------------------------------------------- namen

    /// <summary>DATA 310: het familiespel uit 1987.</summary>
    public static string[] Spelernamen => Engels
        ? new[] { "GRANDPA", "GRANDMA", "BROTHER", "SISTER", "DAD", "MUM" }
        : new[] { "OPA", "OMA", "BROER", "ZUS", "PA", "MA" };

    public static string NaamloosMens => T("MENS", "HUMAN");

    // ------------------------------------------------------------ scherm

    public static string Titel => T("AKWAIER  -  naar het BBC BASIC-spel uit 1987",
                                    "AKWAIER  -  after the BBC BASIC game from 1987");
    public static string KortTitel => "Akwaier";
    public static string NieuwSpel => T("Nieuw spel", "New game");
    public static string Scorelijst => T("Scorelijst", "High scores");
    public static string SnelSpelen => T("snel spelen", "play fast");
    public static string Weergave(string naam) => T($"Weergave: {naam}", $"View: {naam}");
    public static string SpelAfgelopen => T("spel afgelopen", "game over");
    public static string BeurtVan(int beurt, int max, string naam) =>
        T($"beurt {beurt}/{max}  -  {naam}", $"turn {beurt}/{max}  -  {naam}");

    // ---------------------------------------------------------- paneel

    public static string KopKeten => T("KETEN", "CHAIN");
    public static string KopPrijs => T("PRIJS", "PRICE");
    public static string KopTegels => T("TEGELS", "TILES");
    public static string KopStatus => T("STATUS", "STATUS");
    public static string TeStichten => T("te stichten", "can be founded");
    public static string KopAandelen => T("AANDELEN PER SPELER", "SHARES PER PLAYER");
    public static string KopSpeler => T("SPELER", "PLAYER");
    public static string KopGeld => T("GELD", "CASH");
    public static string KopVermogen => T("VERMOGEN", "NET WORTH");
    public static string StenenInDeZak(int n) =>
        T($"stenen in de zak: {n}", $"tiles left in the bag: {n}");
    public static string StenenVan(string naam) => T($"stenen van {naam}", $"tiles of {naam}");
    public static string KijkmodusGeenStenen =>
        T("kijkmodus - geen eigen stenen", "watching - you have no tiles");
    public static string Jij => T(" (jij)", " (you)");

    // ------------------------------------------------------------ beurt

    public static string JeSpeeltAls(int nr, string naam) =>
        T($"Je speelt als speler {nr}: {naam}.", $"You are player {nr}: {naam}.");
    public static string KiesEenSteenKort =>
        T("Kies een steen op het bord of in je hand, of druk 1-8.",
          "Pick a tile on the board or in your hand, or press 1-8.");
    public static string KiesEenSteen =>
        T("Kies een steen: klik op het bord of op je hand, of druk 1-8.",
          "Pick a tile: click the board or your hand, or press 1-8.");
    public static string Kijkmodus =>
        T("Kijkmodus: de zes computerspelers spelen zelf.",
          "Watching: the six computer players play by themselves.");
    public static string GeenSteenKwijt =>
        T("Je kunt geen enkele steen kwijt.", "You cannot place a single tile.");
    public static string PasDezeBeurt => T("Pas deze beurt", "Pass this turn");
    public static string HandplaatsLeeg => T("Die handplaats is leeg.", "That hand slot is empty.");
    public static string SteenIs(int nr, string vak) =>
        T($"Steen {nr} = {vak}", $"Tile {nr} = {vak}");
    public static string LegLos(string vak) =>
        T($"Leg {vak} neer (losse steen)", $"Place {vak} (single tile)");
    public static string LegBij(string vak, char keten) =>
        T($"Leg {vak} bij keten {keten}", $"Place {vak} onto chain {keten}");
    public static string MagHierNiet => T("hier leggen mag niet", "you cannot place here");
    public static string StichtOp(string vak) =>
        T($"Sticht keten op {vak}", $"Found a chain on {vak}");
    public static string Annuleer => T("Annuleer", "Cancel");
    public static string WelkeKetenStichten => T("Welke keten stichten?", "Which chain do you found?");
    public static string WelkeBlijft =>
        T("Twee even grote ketens. Welke blijft bestaan?",
          "Two chains of equal size. Which one survives?");
    public static string NietInJeHand(string vak) =>
        T($"{vak} zit niet in je hand.", $"{vak} is not in your hand.");

    // ------------------------------------------------------------ kopen

    public static string NietsTeKopen => T("Er valt niets te kopen.", "There is nothing to buy.");
    public static string Verder => T("Verder", "Continue");
    public static string KoopHoogstensDrie =>
        T("Koop hoogstens drie aandelen van één keten.",
          "Buy at most three shares in one chain.");
    public static string KopenDubbelepunt => T("Kopen:", "Buy:");
    public static string KoopNiets => T("Koop niets", "Buy nothing");
    public static string HoeveelVan(char keten, int prijs) =>
        T($"Keten {keten}, {prijs} per aandeel. Hoeveel?",
          $"Chain {keten}, {prijs} per share. How many?");
    public static string Terug => T("Terug", "Back");

    // ------------------------------------------------------ meldingenvak

    public static string StichtKeten(string naam, char keten, string vak) =>
        T($"{naam} sticht keten {keten} op {vak}",
          $"{naam} founds chain {keten} on {vak}");
    public static string StichtKetenMetAandeel(string naam, char keten, string vak) =>
        T($"{naam} sticht keten {keten} op {vak} (+1 oprichtersaandeel)",
          $"{naam} founds chain {keten} on {vak} (+1 founder's share)");
    public static string KanNiet(string naam) => T($"{naam} kan niet", $"{naam} cannot move");
    public static string Koopt(string naam, int n, char keten, int prijs) =>
        T($"{naam} koopt {n} x {keten} à {prijs}",
          $"{naam} buys {n} x {keten} at {prijs}");
    public static string KooptNiets(string naam) =>
        T($"{naam} koopt niets", $"{naam} buys nothing");
    public static string LegtLos(string naam, string vak) =>
        T($"{naam} legt {vak} (losse steen)", $"{naam} places {vak} (single tile)");
    public static string LegtBij(string naam, string vak, char keten) =>
        T($"{naam} legt {vak} bij keten {keten}",
          $"{naam} places {vak} onto chain {keten}");
    public static string GaatOpIn(char weg, int tegels, char naar, int pot) =>
        T($"  keten {weg} ({tegels} tegels) gaat op in {naar}, bonus {pot}",
          $"  chain {weg} ({tegels} tiles) merges into {naar}, bonus {pot}");
    public static string Uitkering(string naam, int bedrag) => $"    {naam}: +{bedrag}";
    public static string Einde(string reden) => T($"EINDE: {reden}", $"THE END: {reden}");
    public static string SpelAfgelopenMet(string reden) =>
        T($"Spel afgelopen - {reden}", $"Game over - {reden}");

    // -------------------------------------------------------- eindredenen

    public static string NiemandKanNog =>
        T("niemand kan nog een steen kwijt", "nobody can place a tile any more");
    public static string BeurtenGespeeld(int n) =>
        T($"{n} beurten gespeeld", $"{n} turns played");
    public static string KetenTeGroot(char keten, int grens) =>
        T($"keten {keten} is groter dan {grens} tegels",
          $"chain {keten} is larger than {grens} tiles");

    // ------------------------------------------------------- startvenster

    public static string DoeJeMee => T("DOE JE MEE, MENS?", "ARE YOU PLAYING, HUMAN?");
    public static string SpelenMee => Engels
        ? "GRANDPA, GRANDMA, BROTHER, SISTER, DAD and MUM are playing."
        : "OPA, OMA, BROER, ZUS, PA en MA spelen mee.";
    public static string LabelNaam => T("Naam:", "Name:");
    public static string LabelPlaats => T("Plaats:", "Seat:");
    public static string LabelTaal => T("Taal:", "Language:");
    public static string Willekeurig => T("willekeurig", "random");
    public static string IkDoeMee => T("Ik doe mee", "I'll play");
    public static string AlleenKijken => T("Alleen kijken", "Just watch");

    // -------------------------------------------------------- scorevenster

    public static string ScoreKop => T(" NR  SPELER      VERMOGEN", " NR  PLAYER      NET WORTH");
    public static string NogGeenScores => T("  nog geen scores", "  no scores yet");
    public static string Sluiten => T("Sluiten", "Close");

    // ------------------------------------------------------------ fouten

    public static string IetsMisMaarLooptDoor =>
        T("Er ging iets mis, maar het spel loopt door.",
          "Something went wrong, but the game carries on.");
    public static string VolledigeMeldingIn =>
        T("De volledige melding staat in", "The full report is in");
    public static string ControlesMislukt(int n, string pad) =>
        T($"{n} controle(s) mislukt.\nVerslag: {pad}",
          $"{n} check(s) failed.\nReport: {pad}");
    public static string ZelftestTitel => T("Akwaier zelftest", "Akwaier self-test");
}
