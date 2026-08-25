import Foundation

/// De vaste getallen van het spel, zoals ze in AQUIRE.BBC stonden.
public enum Regels {

    /// Het bord is 15 bij 15.
    public static let grootte = 15

    /// Waarde van een leeg vakje (in het origineel 99 of -1).
    public static let leeg = -1

    /// Een losse steen die nog bij geen enkele keten hoort.
    public static let los = 0

    public static let aantalKetens = 8
    public static let letters = Array("ABCDEFGH")

    /// De vier buren, in de volgorde van DATA 1300 uit het origineel.
    public static let buren: [(dx: Int, dy: Int)] = [(1, 0), (-1, 0), (0, 1), (0, -1)]

    /// DATA 20000: basisprijs per keten, plaats 0 blijft leeg.
    public static let basisprijs = [0, 900, 900, 800, 700, 700, 600, 500, 400]

    /// DATA 1980: boven elke drempel komt er 100 bij de prijs.
    public static let prijsdrempels = [0, 1, 2, 3, 4, 9, 14, 19, 24, 35]

    public static let startgeld = 25_000

    /// speelsteen%(8,6): acht stenen in de hand.
    public static let handGrootte = 8

    public static let aantalSpelers = 6

    /// W% >= 210 maakt een einde aan het spel.
    public static let maxBeurten = 210

    /// Een keten groter dan dit beëindigt het spel.
    public static let eindKetenGrootte = 100

    /// Steennummer 1..225 omzetten naar kolom en rij, als regel 2240/2250.
    public static func tegelPositie(_ n: Int) -> (x: Int, y: Int) {
        var y = n % grootte
        if y == 0 { y = grootte }
        var x = n / grootte
        if x == 0 { x = grootte }
        return (x, y)
    }

    /// Kolom en rij als "B7": rijletter gevolgd door kolomnummer.
    public static func naam(_ x: Int, _ y: Int) -> String {
        let letter = Character(UnicodeScalar(UInt8(65 + y - 1)))
        return "\(letter)\(x)"
    }

    public static func ketenLetter(_ c: Int) -> Character {
        (1...aantalKetens).contains(c) ? letters[c - 1] : "?"
    }
}

/// Een toevalsreeks die je met een zaad kunt vastzetten, zodat de zelftest
/// elke keer dezelfde partijen speelt. Swift heeft daar zelf niets voor.
public struct Toevalsreeks: RandomNumberGenerator {
    private var toestand: UInt64

    public init(zaad: UInt64) {
        toestand = zaad &+ 0x9E37_79B9_7F4A_7C15
    }

    public mutating func next() -> UInt64 {
        toestand &+= 0x9E37_79B9_7F4A_7C15
        var z = toestand
        z = (z ^ (z >> 30)) &* 0xBF58_476D_1CE4_E5B9
        z = (z ^ (z >> 27)) &* 0x94D0_49BB_1331_11EB
        return z ^ (z >> 31)
    }
}
