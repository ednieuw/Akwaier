import Foundation

/// Eén van de zes spelers, met banksaldo en aandelenbezit.
public final class Speler {

    public init(nummer: Int, naam: String) {
        self.nummer = nummer
        self.naam = naam
        self.geld = Regels.startgeld
        self.aandelen = Array(repeating: 0, count: Regels.aantalKetens + 1)
    }

    public let nummer: Int
    public var naam: String

    /// Waar of de speler door de mens bediend wordt.
    public var mens = false

    /// bezit%(): het banksaldo.
    public var geld: Int

    /// aandelen%(): bezit per keten, plaats 1..8.
    public var aandelen: [Int]
}

/// Wat er van een vakje te zeggen valt, zonder het bord aan te raken.
public struct Zetkans {
    public let x: Int
    public let y: Int

    /// Onwaar als kanniet% waar zou zijn: hier leggen mag niet.
    public let mag: Bool

    /// De aanliggende ketens, ontdubbeld, in de volgorde van DATA 1300.
    public let aanliggend: [Int]

    /// Aanliggende losse stenen; die worden mee opgeslokt.
    public let losse: [(x: Int, y: Int)]

    /// De grootste aanliggende keten; 0 als er geen keten aanligt.
    public let winnaar: Int

    /// kiezen%: meerdere even grote ketens, de speler moet kiezen.
    public let gelijkspel: Bool

    public let gelijken: [Int]

    public var fuseert: Bool { aanliggend.count > 1 }
}

/// Wat één opgeslokte keten aan bonus uitkeert.
public struct Uitkering {
    public let keten: Int
    public let naar: Int
    public let grootte: Int
    public let prijs: Int
    public let pot: Int
    public var bedragen: [Int: Int] = [:]
}

/// Verslag van één gespeelde steen.
public struct Zetverslag {
    public let x: Int
    public let y: Int

    /// De keten waar de steen bij hoort; 0 voor een losse steen.
    public var keten: Int = 0

    public var opgeslokt: [Int] = []
    public var uitkeringen: [Uitkering] = []
}

/// Wordt gemeld als een zet tegen de regels ingaat.
public struct OngeldigeZet: Error, CustomStringConvertible {
    public let description: String
    public init(_ bericht: String) { description = bericht }
}
