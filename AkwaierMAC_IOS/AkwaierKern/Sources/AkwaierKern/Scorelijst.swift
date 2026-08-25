import Foundation

/// Eén regel uit de scorelijst.
public struct Scoreregel: Codable, Equatable {
    public var naam: String
    public var vermogen: Int

    public init(naam: String, vermogen: Int) {
        self.naam = naam
        self.vermogen = vermogen
    }
}

/// De inhoud van het scorebestand: de gekozen taal en de ranglijst.
public struct Scorebestand: Codable {
    /// "nl" of "en"; leeg als er nog nooit een taal gekozen is.
    public var taal: String = ""
    public var scores: [Scoreregel] = []

    public init() {}
}

/// Vervangt het bestand "SCORE" uit het origineel (regels 4050-4195) door een
/// leesbaar JSON-bestand. Behalve de ranglijst staat ook de taalkeuze erin,
/// zodat het spel de volgende keer in dezelfde taal begint.
///
/// Op Windows staat dit bestand naast het programma. Dat kan op iOS niet, dus
/// hier gaat het naar Application Support — op de iPad de enige plek waar een
/// app mag schrijven, en op de Mac de gebruikelijke.
public enum Scorelijst {

    private static let bewaar = 30

    public static var pad: URL = {
        let map = (try? FileManager.default.url(for: .applicationSupportDirectory,
                                                in: .userDomainMask,
                                                appropriateFor: nil, create: true))
            ?? URL(fileURLWithPath: NSTemporaryDirectory())
        let eigen = map.appendingPathComponent("Akwaier", isDirectory: true)
        try? FileManager.default.createDirectory(at: eigen, withIntermediateDirectories: true)
        return eigen.appendingPathComponent("akwaier-scores.json")
    }()

    /// Leest het bestand. Een bestand van vóór de taalkeuze bevat alleen een
    /// lijst; dat wordt nog gewoon gelezen.
    public static func lees() -> Scorebestand {
        guard let data = try? Data(contentsOf: pad) else { return Scorebestand() }
        if let bestand = try? JSONDecoder().decode(Scorebestand.self, from: data) {
            return bestand
        }
        if let lijst = try? JSONDecoder().decode([Scoreregel].self, from: data) {
            var bestand = Scorebestand()
            bestand.scores = lijst
            return bestand
        }
        // Een stukgelopen of met de hand aangepast bestand mag het spel niet
        // tegenhouden; de lijst begint dan gewoon opnieuw.
        return Scorebestand()
    }

    private static func schrijf(_ bestand: Scorebestand) {
        let codeerder = JSONEncoder()
        codeerder.outputFormatting = [.prettyPrinted, .sortedKeys]
        guard let data = try? codeerder.encode(bestand) else { return }
        try? data.write(to: pad, options: .atomic)
    }

    @discardableResult
    public static func bijwerken(_ nieuw: [(naam: String, vermogen: Int)]) -> [Scoreregel] {
        var bestand = lees()
        bestand.scores.append(contentsOf: nieuw.map {
            Scoreregel(naam: $0.naam, vermogen: $0.vermogen)
        })
        bestand.scores.sort { $0.vermogen > $1.vermogen }
        if bestand.scores.count > bewaar { bestand.scores = Array(bestand.scores.prefix(bewaar)) }
        bestand.taal = Taal.code
        schrijf(bestand)
        return bestand.scores
    }

    /// Zet de taal op de bewaarde keuze. Staat er nog niets in het bestand, dan
    /// beslist de taalinstelling van het apparaat.
    public static func laadTaal() {
        let code = lees().taal
        if code.trimmingCharacters(in: .whitespaces).isEmpty {
            Taal.engels = Taal.apparaatIsEngels()
        } else {
            Taal.zetCode(code)
        }
    }

    public static func bewaarTaal() {
        var bestand = lees()
        guard bestand.taal != Taal.code || !FileManager.default.fileExists(atPath: pad.path) else { return }
        bestand.taal = Taal.code
        schrijf(bestand)
    }
}
