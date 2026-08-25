import Foundation

/// Alle teksten die de speler te zien krijgt, in het Nederlands en het Engels.
/// Zowel de speellogica als het scherm halen hun teksten hier op, zodat er bij
/// het omschakelen niets in de verkeerde taal kan achterblijven.
///
/// Gelijk aan `AkwaierWin/Engine/Taal.cs` in de Windows-versie. Blijft die twee
/// gelijk houden: de teksten in het meldingenvak zijn het enige wat de twee
/// programma's letterlijk moeten delen.
public enum Taal {

    /// false = Nederlands, true = Engels.
    public static var engels = false

    private static func t(_ nl: String, _ en: String) -> String { engels ? en : nl }

    /// De taal van het apparaat. Alleen bij een Nederlandse instelling begint het
    /// spel in het Nederlands; alle andere talen krijgen Engels.
    public static func apparaatIsEngels() -> Bool {
        let voorkeur = Locale.preferredLanguages.first ?? Locale.current.identifier
        return !voorkeur.lowercased().hasPrefix("nl")
    }

    public static var code: String { engels ? "en" : "nl" }

    public static func zetCode(_ code: String) {
        engels = code.lowercased() != "nl"
    }

    // MARK: - namen

    /// DATA 310: het familiespel uit 1987.
    public static var spelernamen: [String] {
        engels
            ? ["GRANDPA", "GRANDMA", "BROTHER", "SISTER", "DAD", "MUM"]
            : ["OPA", "OMA", "BROER", "ZUS", "PA", "MA"]
    }

    public static var naamloosMens: String { t("MENS", "HUMAN") }

    // MARK: - scherm

    public static var titel: String {
        t("AKWAIER  -  naar het BBC BASIC-spel uit 1987",
          "AKWAIER  -  after the BBC BASIC game from 1987")
    }
    public static var kortTitel: String { "Akwaier" }
    public static var nieuwSpel: String { t("Nieuw spel", "New game") }
    public static var scorelijst: String { t("Scorelijst", "High scores") }
    public static var snelSpelen: String { t("snel spelen", "play fast") }
    public static func weergave(_ naam: String) -> String {
        t("Weergave: \(naam)", "View: \(naam)")
    }
    public static var spelAfgelopen: String { t("spel afgelopen", "game over") }
    public static func beurtVan(_ beurt: Int, _ max: Int, _ naam: String) -> String {
        t("beurt \(beurt)/\(max)  -  \(naam)", "turn \(beurt)/\(max)  -  \(naam)")
    }

    // MARK: - paneel

    public static var kopKeten: String { t("KETEN", "CHAIN") }
    public static var kopPrijs: String { t("PRIJS", "PRICE") }
    public static var kopTegels: String { t("TEGELS", "TILES") }
    public static var kopStatus: String { t("STATUS", "STATUS") }
    public static var teStichten: String { t("te stichten", "can be founded") }
    public static var kopAandelen: String { t("AANDELEN PER SPELER", "SHARES PER PLAYER") }
    public static var kopSpeler: String { t("SPELER", "PLAYER") }
    public static var kopGeld: String { t("GELD", "CASH") }
    public static var kopVermogen: String { t("VERMOGEN", "NET WORTH") }
    public static func stenenInDeZak(_ n: Int) -> String {
        t("stenen in de zak: \(n)", "tiles left in the bag: \(n)")
    }
    public static func stenenVan(_ naam: String) -> String {
        t("stenen van \(naam)", "tiles of \(naam)")
    }
    public static var kijkmodusGeenStenen: String {
        t("kijkmodus - geen eigen stenen", "watching - you have no tiles")
    }
    public static var jij: String { t(" (jij)", " (you)") }

    // MARK: - beurt

    public static func jeSpeeltAls(_ nr: Int, _ naam: String) -> String {
        t("Je speelt als speler \(nr): \(naam).", "You are player \(nr): \(naam).")
    }
    public static var kiesEenSteenKort: String {
        t("Kies een steen op het bord of in je hand.",
          "Pick a tile on the board or in your hand.")
    }
    public static var kiesEenSteen: String {
        t("Kies een steen: tik op het bord of op je hand.",
          "Pick a tile: tap the board or your hand.")
    }
    public static var kijkmodus: String {
        t("Kijkmodus: de zes computerspelers spelen zelf.",
          "Watching: the six computer players play by themselves.")
    }
    public static var geenSteenKwijt: String {
        t("Je kunt geen enkele steen kwijt.", "You cannot place a single tile.")
    }
    public static var pasDezeBeurt: String { t("Pas deze beurt", "Pass this turn") }
    public static var handplaatsLeeg: String { t("Die handplaats is leeg.", "That hand slot is empty.") }
    public static func steenIs(_ nr: Int, _ vak: String) -> String {
        t("Steen \(nr) = \(vak)", "Tile \(nr) = \(vak)")
    }
    public static func legLos(_ vak: String) -> String {
        t("Leg \(vak) neer (losse steen)", "Place \(vak) (single tile)")
    }
    public static func legBij(_ vak: String, _ keten: Character) -> String {
        t("Leg \(vak) bij keten \(keten)", "Place \(vak) onto chain \(keten)")
    }
    public static var magHierNiet: String { t("hier leggen mag niet", "you cannot place here") }
    public static func stichtOp(_ vak: String) -> String {
        t("Sticht keten op \(vak)", "Found a chain on \(vak)")
    }
    public static var annuleer: String { t("Annuleer", "Cancel") }
    public static var welkeKetenStichten: String {
        t("Welke keten stichten?", "Which chain do you found?")
    }
    public static var welkeBlijft: String {
        t("Twee even grote ketens. Welke blijft bestaan?",
          "Two chains of equal size. Which one survives?")
    }
    public static func nietInJeHand(_ vak: String) -> String {
        t("\(vak) zit niet in je hand.", "\(vak) is not in your hand.")
    }

    // MARK: - kopen

    public static var nietsTeKopen: String { t("Er valt niets te kopen.", "There is nothing to buy.") }
    public static var verder: String { t("Verder", "Continue") }
    public static var koopHoogstensDrie: String {
        t("Koop hoogstens drie aandelen van één keten.", "Buy at most three shares in one chain.")
    }
    public static var kopenDubbelepunt: String { t("Kopen:", "Buy:") }
    public static var koopNiets: String { t("Koop niets", "Buy nothing") }
    public static func hoeveelVan(_ keten: Character, _ prijs: Int) -> String {
        t("Keten \(keten), \(prijs) per aandeel. Hoeveel?",
          "Chain \(keten), \(prijs) per share. How many?")
    }
    public static var terug: String { t("Terug", "Back") }

    // MARK: - meldingenvak

    public static func stichtKeten(_ naam: String, _ keten: Character, _ vak: String) -> String {
        t("\(naam) sticht keten \(keten) op \(vak)", "\(naam) founds chain \(keten) on \(vak)")
    }
    public static func stichtKetenMetAandeel(_ naam: String, _ keten: Character, _ vak: String) -> String {
        t("\(naam) sticht keten \(keten) op \(vak) (+1 oprichtersaandeel)",
          "\(naam) founds chain \(keten) on \(vak) (+1 founder's share)")
    }
    public static func kanNiet(_ naam: String) -> String {
        t("\(naam) kan niet", "\(naam) cannot move")
    }
    public static func koopt(_ naam: String, _ n: Int, _ keten: Character, _ prijs: Int) -> String {
        t("\(naam) koopt \(n) x \(keten) à \(prijs)", "\(naam) buys \(n) x \(keten) at \(prijs)")
    }
    public static func kooptNiets(_ naam: String) -> String {
        t("\(naam) koopt niets", "\(naam) buys nothing")
    }
    public static func legtLos(_ naam: String, _ vak: String) -> String {
        t("\(naam) legt \(vak) (losse steen)", "\(naam) places \(vak) (single tile)")
    }
    public static func legtBij(_ naam: String, _ vak: String, _ keten: Character) -> String {
        t("\(naam) legt \(vak) bij keten \(keten)", "\(naam) places \(vak) onto chain \(keten)")
    }
    public static func gaatOpIn(_ weg: Character, _ tegels: Int, _ naar: Character, _ pot: Int) -> String {
        t("  keten \(weg) (\(tegels) tegels) gaat op in \(naar), bonus \(pot)",
          "  chain \(weg) (\(tegels) tiles) merges into \(naar), bonus \(pot)")
    }
    public static func uitkering(_ naam: String, _ bedrag: Int) -> String {
        "    \(naam): +\(bedrag)"
    }
    public static func einde(_ reden: String) -> String {
        t("EINDE: \(reden)", "THE END: \(reden)")
    }
    public static func spelAfgelopenMet(_ reden: String) -> String {
        t("Spel afgelopen - \(reden)", "Game over - \(reden)")
    }

    // MARK: - eindredenen

    public static var niemandKanNog: String {
        t("niemand kan nog een steen kwijt", "nobody can place a tile any more")
    }
    public static func beurtenGespeeld(_ n: Int) -> String {
        t("\(n) beurten gespeeld", "\(n) turns played")
    }
    public static func ketenTeGroot(_ keten: Character, _ grens: Int) -> String {
        t("keten \(keten) is groter dan \(grens) tegels",
          "chain \(keten) is larger than \(grens) tiles")
    }

    // MARK: - startvenster

    public static var doeJeMee: String { t("DOE JE MEE, MENS?", "ARE YOU PLAYING, HUMAN?") }
    public static var spelenMee: String {
        engels
            ? "GRANDPA, GRANDMA, BROTHER, SISTER, DAD and MUM are playing."
            : "OPA, OMA, BROER, ZUS, PA en MA spelen mee."
    }
    public static var labelNaam: String { t("Naam:", "Name:") }
    public static var labelPlaats: String { t("Plaats:", "Seat:") }
    public static var labelTaal: String { t("Taal:", "Language:") }
    public static var willekeurig: String { t("willekeurig", "random") }
    public static var ikDoeMee: String { t("Ik doe mee", "I'll play") }
    public static var alleenKijken: String { t("Alleen kijken", "Just watch") }

    // MARK: - scorevenster

    public static var scoreKop: String { t(" NR  SPELER      VERMOGEN", " NR  PLAYER      NET WORTH") }
    public static var nogGeenScores: String { t("  nog geen scores", "  no scores yet") }
    public static var sluiten: String { t("Sluiten", "Close") }

    // MARK: - fouten

    public static var ietsMisMaarLooptDoor: String {
        t("Er ging iets mis, maar het spel loopt door.",
          "Something went wrong, but the game carries on.")
    }
    public static var volledigeMeldingIn: String {
        t("De volledige melding staat in", "The full report is in")
    }
    public static func controlesMislukt(_ n: Int, _ pad: String) -> String {
        t("\(n) controle(s) mislukt.\nVerslag: \(pad)",
          "\(n) check(s) failed.\nReport: \(pad)")
    }
    public static var zelftestTitel: String { t("Akwaier zelftest", "Akwaier self-test") }
}
