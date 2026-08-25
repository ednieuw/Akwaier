import Foundation

/// Rekent de regels na, zodat je bij een wijziging meteen ziet of er iets stuk
/// is gegaan. Dezelfde tien proeven als `AkwaierWin/Engine/Zelftest.cs` op
/// Windows, met dezelfde verwachte uitkomsten.
///
/// Draaien: `swift run AkwaierZelftest` of `swift test`.
public enum Zelftest {

    public struct Uitslag {
        public var verslag: String
        public var fouten: Int
        public var goed: Bool { fouten == 0 }
    }

    private static var uit: [String] = []
    private static var fout = 0

    public static func draai() -> Uitslag {
        uit = []
        fout = 0

        prijsstaffel()
        legaliteit()
        losseSteen()
        enkeleFusie()
        gelijkeKetens()
        drievoudigeFusie()
        stichten()
        kopen()
        verdeelsleutel()
        volledigePartijen()

        regel("")
        regel(fout == 0 ? "ALLES GOED" : "\(fout) CONTROLE(S) MISLUKT")
        return Uitslag(verslag: uit.joined(separator: "\n"), fouten: fout)
    }

    // MARK: - hulpmiddelen

    private static func regel(_ s: String) { uit.append(s) }
    private static func kop(_ s: String) { regel(""); regel(s) }

    private static func eis(_ goed: Bool, _ wat: String) {
        regel("  \(goed ? "ok  " : "FOUT"): \(wat)")
        if !goed { fout += 1 }
    }

    /// Een leeg bord met zes schone spelers, om een stand na te bouwen.
    private static func schoon(_ zaad: UInt64 = 1) -> Spel {
        let g = Spel(mensPlaats: 0, mensNaam: "", zaad: zaad)
        g.proefstandLeegmaken()
        return g
    }

    // MARK: - de proeven

    private static func prijsstaffel() {
        kop("1 prijsstaffel")
        let g = schoon()
        for x in 1...5 { g.zetVak(x, 1, 3) }              // keten C, vijf tegels
        eis(g.prijs(3) == 1300, "5 tegels C kost 1300 (is \(g.prijs(3)))")
        g.spelers[1].aandelen[3] = 4
        eis(g.prijs(3) == 1340, "met 4 uitstaande aandelen 1340 (is \(g.prijs(3)))")
        eis(g.prijs(4) == 0, "een keten die niet bestaat kost niets")
    }

    private static func legaliteit() {
        kop("2 legaliteit")
        let g = schoon()
        g.zetVak(5, 5, Regels.los)
        eis(!g.bekijk(5, 6).mag, "naast een losse steen leggen mag niet")
        eis(g.bekijk(8, 8).mag, "op een leeg stuk bord leggen mag wel")
        g.zetVak(6, 6, 2)
        eis(g.bekijk(5, 6).mag, "het mag zodra je ook keten B raakt")
        eis(g.kanStichten(8, 8), "een vrij liggend vakje is stichtbaar")
        eis(!g.kanStichten(5, 6), "een vakje met buren niet")
    }

    private static func losseSteen() {
        kop("3 losse steen wordt opgeslokt")
        let g = schoon()
        g.proefhand(1, [(5, 6)])
        g.zetVak(5, 5, Regels.los)
        g.zetVak(6, 6, 2); g.zetVak(7, 6, 2)
        guard let r = try? g.leg(speler: 1, plaats: 0) else {
            return eis(false, "de zet werd geweigerd")
        }
        eis(r.keten == 2 && g.vak(5, 5) == 2, "de losse steen gaat mee in keten B")
        eis(g.ketenGrootte(2) == 4, "B groeit van 2 naar 4 (is \(g.ketenGrootte(2)))")
    }

    private static func enkeleFusie() {
        kop("4 fusie met bonus")
        let g = schoon()
        g.proefhand(1, [(4, 1)])
        for x in 1...3 { g.zetVak(x, 1, 1) }              // A: drie tegels
        for x in 5...10 { g.zetVak(x, 1, 2) }             // B: zes tegels
        g.spelers[1].aandelen[1] = 6
        g.spelers[2].aandelen[1] = 3
        g.spelers[3].aandelen[1] = 1
        let prijsA = g.prijs(1)

        guard let r = try? g.leg(speler: 1, plaats: 0), let u = r.uitkeringen.first else {
            return eis(false, "de fusie kwam niet tot stand")
        }
        eis(u.keten == 1 && u.naar == 2, "A gaat op in B")
        eis(u.pot == (3 + 10) * prijsA,
            "pot = (3 tegels + 10 aandelen) x \(prijsA) = \((3 + 10) * prijsA) (is \(u.pot))")
        let tweederde = Int((2.0 / 3.0 * Double(u.pot)).rounded(.toNearestOrAwayFromZero))
        let eenderde = Int((1.0 / 3.0 * Double(u.pot)).rounded(.toNearestOrAwayFromZero))
        eis(u.bedragen[1] == tweederde,
            "grootaandeelhouder krijgt twee derde (\(u.bedragen[1] ?? 0))")
        eis(u.bedragen[2] == eenderde, "tweede krijgt een derde (\(u.bedragen[2] ?? 0))")
        eis(u.bedragen[3] == nil, "de derde aandeelhouder krijgt niets")
        eis(g.spelers[1].aandelen[2] == 3 && g.spelers[2].aandelen[2] == 1
            && g.spelers[3].aandelen[2] == 0, "aandelen half omgeruild: 6->3, 3->1, 1->0")
        eis(g.spelers[1].aandelen[1] == 0, "de aandelen A zijn weg")
        eis(g.ketenGrootte(2) == 10 && g.ketenGrootte(1) == 0,
            "B is nu 10 tegels, A bestaat niet meer")
        eis(g.teStichten().contains(1), "A kan opnieuw gesticht worden")
    }

    private static func gelijkeKetens() {
        kop("5 twee even grote ketens")
        let g = schoon()
        g.proefhand(1, [(4, 1)])
        for x in 1...3 { g.zetVak(x, 1, 1) }
        for x in 5...7 { g.zetVak(x, 1, 2) }
        let kans = g.bekijk(4, 1)
        eis(kans.gelijkspel && kans.gelijken.count == 2, "het gelijkspel wordt gezien")
        guard let r = try? g.leg(speler: 1, plaats: 0, gekozenWinnaar: 1) else {
            return eis(false, "de zet werd geweigerd")
        }
        eis(r.keten == 1 && g.ketenGrootte(1) == 7 && g.ketenGrootte(2) == 0,
            "de keuze telt: A blijft en wordt 7 tegels")
    }

    private static func drievoudigeFusie() {
        kop("6 drievoudige fusie")
        let g = schoon()
        g.proefhand(1, [(2, 2)])
        g.zetVak(1, 2, 1); g.zetVak(3, 2, 2); g.zetVak(2, 1, 3)
        for x in 4...8 { g.zetVak(x, 2, 2) }
        g.spelers[1].aandelen[1] = 2
        g.spelers[1].aandelen[3] = 2
        guard let r = try? g.leg(speler: 1, plaats: 0) else {
            return eis(false, "de zet werd geweigerd")
        }
        eis(r.keten == 2 && r.opgeslokt.count == 2, "A en C gaan beide op in B")
        eis(r.uitkeringen.count == 2, "er zijn twee uitkeringen")
        eis(g.ketenGrootte(2) == 9 && g.ketenGrootte(1) == 0 && g.ketenGrootte(3) == 0,
            "B is nu 9 tegels (is \(g.ketenGrootte(2)))")
    }

    private static func stichten() {
        kop("7 stichten")
        let g = schoon()
        g.proefhand(1, [(8, 8)])
        guard (try? g.sticht(speler: 1, plaats: 0, keten: 5)) != nil else {
            return eis(false, "stichten werd geweigerd")
        }
        eis(g.vak(8, 8) == 5 && g.ketenGrootte(5) == 1, "keten E begint met één tegel")
        eis(g.spelers[1].aandelen[5] == 1, "de stichter krijgt één gratis aandeel")
        eis(!g.teStichten().contains(5), "E staat niet meer op de stichtlijst")
    }

    private static func kopen() {
        kop("8 kopen")
        let g = schoon()
        for x in 1...5 { g.zetVak(x, 1, 3) }              // C kost 1300
        g.spelers[1].geld = 3000
        let n = g.koop(speler: 1, keten: 3, aantal: 3)
        eis(n == 2, "drie gevraagd, twee betaalbaar (is \(n))")
        eis(g.spelers[1].geld == 400 && g.spelers[1].aandelen[3] == 2, "saldo en bezit kloppen")
        eis(g.koop(speler: 1, keten: 3, aantal: 3) == 0, "met 400 over kan er niets meer bij")
        eis(g.koop(speler: 1, keten: 4, aantal: 3) == 0,
            "een keten die niet bestaat levert niets op")
    }

    private static func verdeelsleutel() {
        kop("9 verdeelsleutel")
        let gevallen: [([Int], [Double])] = [
            ([9, 0, 0, 0, 0, 0], [1, 0, 0, 0, 0, 0]),
            ([5, 5, 5, 0, 0, 0], [1 / 3.0, 1 / 3.0, 1 / 3.0, 0, 0, 0]),
            ([5, 5, 2, 0, 0, 0], [0.5, 0.5, 0, 0, 0, 0]),
            ([9, 2, 2, 2, 2, 2], [2 / 3.0, 1 / 15.0, 1 / 15.0, 1 / 15.0, 1 / 15.0, 1 / 15.0]),
            ([9, 2, 2, 2, 2, 0], [2 / 3.0, 1 / 12.0, 1 / 12.0, 1 / 12.0, 1 / 12.0, 0]),
            ([9, 2, 2, 2, 0, 0], [2 / 3.0, 1 / 9.0, 1 / 9.0, 1 / 9.0, 0, 0]),
            ([9, 2, 2, 0, 0, 0], [2 / 3.0, 1 / 6.0, 1 / 6.0, 0, 0, 0]),
            ([9, 4, 1, 0, 0, 0], [2 / 3.0, 1 / 3.0, 0, 0, 0, 0]),
        ]
        var alles = true, telt = true
        for (bezit, verwacht) in gevallen {
            let got = Bonus.verdeling(bezit)
            for i in 0..<6 where abs(got[i] - verwacht[i]) > 1e-9 { alles = false }
            if abs(got.reduce(0, +) - 1) > 1e-9 { telt = false }
        }
        eis(alles, "alle acht gevallen geven de juiste breuken")
        eis(telt, "elke verdeling telt op tot 1")
    }

    private static func volledigePartijen() {
        kop("10 zestig volledig uitgespeelde partijen")
        var zetten = 0, fouten = 0
        var eerste = ""
        for zaad in 0..<60 {
            let g = Spel(mensPlaats: 0, mensNaam: "", zaad: UInt64(zaad))
            var n = 0
            while true {
                let p = g.volgendeSpeler()
                if p == 0 { break }
                g.computerBeurt(p)
                n += 1
                if let mis = controleer(g) {
                    fouten += 1
                    if eerste.isEmpty { eerste = "zaad \(zaad), zet \(n): \(mis)" }
                    break
                }
                if n > 400 {
                    fouten += 1
                    if eerste.isEmpty { eerste = "zaad \(zaad): loopt niet af" }
                    break
                }
            }
            zetten += n
        }
        eis(fouten == 0, fouten == 0
            ? "na elke zet kloppen bord, ketens, aandelen, geld en de 225 stenen (\(zetten) zetten)"
            : "\(fouten) partij(en) misgegaan, eerste: \(eerste)")
    }

    /// Rekent na of de spelstand nog samenhangend is.
    private static func controleer(_ g: Spel) -> String? {
        var geteld = Array(repeating: 0, count: Regels.aantalKetens + 1)
        var bezet = Set<Int>()
        for x in 1...Regels.grootte {
            for y in 1...Regels.grootte {
                let v = g.vak(x, y)
                if v != Regels.leeg && v != Regels.los && (v < 1 || v > Regels.aantalKetens) {
                    return "vakje \(Regels.naam(x, y)) heeft waarde \(v)"
                }
                if v >= 1 { geteld[v] += 1 }
                if v != Regels.leeg { bezet.insert(x * 100 + y) }
            }
        }
        for c in 1...Regels.aantalKetens where g.ketenGrootte(c) != geteld[c] {
            return "keten \(Regels.ketenLetter(c)) telt \(g.ketenGrootte(c)), op het bord \(geteld[c])"
        }

        for q in 1...Regels.aantalSpelers {
            if g.spelers[q].geld < 0 { return "speler \(q) staat rood" }
            for c in 1...Regels.aantalKetens where g.spelers[q].aandelen[c] < 0 {
                return "speler \(q) heeft negatieve aandelen"
            }
        }

        var stenen = Set(g.zak)
        for p in 1...Regels.aantalSpelers {
            for s in 0..<Regels.handGrootte {
                let t = g.hand(p, s)
                if t == 0 { continue }
                if stenen.contains(t) { return "steen \(t) komt dubbel voor" }
                stenen.insert(t)
                let plek = Regels.tegelPositie(t)
                if bezet.contains(plek.x * 100 + plek.y) {
                    return "steen \(t) wijst naar een bezet vakje"
                }
            }
        }
        if stenen.count + bezet.count != Regels.grootte * Regels.grootte {
            return "\(stenen.count) losse + \(bezet.count) gelegde stenen is geen 225"
        }
        return nil
    }
}
