import Foundation

/// De volledige spelstand van een partij Akwaier, met alle regels uit
/// AQUIRE.BBC (BBC Micro, "aquire 23/05/87"). Er zit geen schermcode in.
///
/// Regel voor regel gelijk aan `AkwaierWin/Engine/Spel.cs` van de Windows-versie.
/// De regels zijn één op één uit het origineel overgenomen; de aantoonbare
/// programmeerfouten zijn hersteld en staan gemarkeerd met "FIX:".
public final class Spel {

    private var toeval: Toevalsreeks
    private var bord: [[Int]] = []
    private var groottes: [Int] = []
    private var handen: [[Int]] = []     // [speler 0..6][plaats 0..7], 0 = geen steen
    private var rondePassen = 0          // kanniet%

    public private(set) var spelers: [Speler] = []
    public private(set) var zak: [Int] = []

    /// Op welke plaats de mens speelt; 0 betekent kijken.
    public let mensPlaats: Int

    /// W%: het aantal gespeelde beurten.
    public private(set) var beurt = 0

    public private(set) var aanDeBeurt = 0
    public private(set) var klaar = false
    public private(set) var eindreden = ""

    public init(mensPlaats: Int = 0, mensNaam: String = "", zaad: UInt64? = nil) {
        // Eerst de twee eigenschappen zonder standaardwaarde; daarna is self
        // volledig gezet en mag er een methode aangeroepen worden.
        toeval = Toevalsreeks(zaad: zaad ?? UInt64.random(in: 0...UInt64.max))
        self.mensPlaats = mensPlaats

        bord = Array(repeating: Array(repeating: Regels.leeg, count: Regels.grootte + 2),
                     count: Regels.grootte + 2)
        groottes = Array(repeating: 0, count: Regels.aantalKetens + 1)

        // plaats 0 blijft ongebruikt, zodat de spelers 1..6 heten zoals in het origineel
        var lijst: [Speler] = [Speler(nummer: 0, naam: "")]
        for i in 1...Regels.aantalSpelers {
            lijst.append(Speler(nummer: i, naam: Taal.spelernamen[i - 1]))
        }
        spelers = lijst

        if mensPlaats > 0 {
            var naam = mensNaam.trimmingCharacters(in: .whitespacesAndNewlines).uppercased()
            if naam.count > 8 { naam = String(naam.prefix(8)) }
            spelers[mensPlaats].mens = true
            spelers[mensPlaats].naam = naam.isEmpty ? Taal.naamloosMens : naam
        }

        zak = Array(1...(Regels.grootte * Regels.grootte))
        zak.shuffle(using: &toeval)

        // PROCsteen_pakken stopte zodra er nog één steen over was; die bleef
        // ongebruikt liggen. FIX: de zak wordt nu netjes leeggespeeld.
        handen = Array(repeating: Array(repeating: 0, count: Regels.handGrootte),
                       count: Regels.aantalSpelers + 1)
        for p in 1...Regels.aantalSpelers {
            for s in 0..<Regels.handGrootte { handen[p][s] = pak() }
        }
    }

    // MARK: - bord en zak

    private func pak() -> Int {
        zak.isEmpty ? 0 : zak.removeLast()
    }

    /// Een vakje zetten en de ketengroottes bijhouden.
    internal func zetVak(_ x: Int, _ y: Int, _ waarde: Int) {
        let oud = bord[x][y]
        if oud == waarde { return }
        if oud >= 1 { groottes[oud] -= 1 }
        bord[x][y] = waarde
        if waarde >= 1 { groottes[waarde] += 1 }
    }

    /// De inhoud van een vakje; buiten het bord telt als leeg.
    public func vak(_ x: Int, _ y: Int) -> Int {
        (1...Regels.grootte).contains(x) && (1...Regels.grootte).contains(y)
            ? bord[x][y] : Regels.leeg
    }

    /// FNmax(): het aantal tegels van een keten.
    public func ketenGrootte(_ keten: Int) -> Int {
        (1...Regels.aantalKetens).contains(keten) ? groottes[keten] : 0
    }

    /// De steen op een handplaats; 0 als de plaats leeg is.
    public func hand(_ speler: Int, _ plaats: Int) -> Int { handen[speler][plaats] }

    /// sticht%: de ketens die nog gesticht kunnen worden.
    public func teStichten() -> [Int] {
        (1...Regels.aantalKetens).filter { groottes[$0] == 0 }
    }

    public func bestaandeKetens() -> [Int] {
        (1...Regels.aantalKetens).filter { groottes[$0] > 0 }
    }

    // MARK: - geldzaken

    /// PROCbereken_prijs_keten (regel 1900).
    public func prijs(_ keten: Int) -> Int {
        let n = ketenGrootte(keten)
        if n == 0 { return 0 }

        var p = Regels.basisprijs[keten]
        for drempel in Regels.prijsdrempels where n > drempel { p += 100 }

        var uitstaand = 0
        for q in 1...Regels.aantalSpelers { uitstaand += spelers[q].aandelen[keten] }
        return p + uitstaand * 10
    }

    /// PROCvermogen: banksaldo plus de waarde van alle aandelen.
    public func vermogen(_ speler: Int) -> Int {
        var t = spelers[speler].geld
        for c in 1...Regels.aantalKetens { t += prijs(c) * spelers[speler].aandelen[c] }
        return t
    }

    /// PROCsort_aandelen: de spelers gesorteerd op aandeelbezit in een keten,
    /// aflopend; bij gelijk bezit wint het hoogste spelernummer, net als de
    /// sleutel 10*bezit+nummer uit het origineel.
    public func rangorde(_ keten: Int) -> [Int] {
        (1...Regels.aantalSpelers).sorted {
            spelers[$0].aandelen[keten] * 10 + $0 > spelers[$1].aandelen[keten] * 10 + $1
        }
    }

    // MARK: - zetten bekijken

    /// PROCcontroleer_aanliggen (regel 1200), maar zonder het bord aan te raken.
    ///
    /// FIX: het origineel las de buur uit vóór de randcontrole en las daarbij
    /// geheugen buiten de bordarray.
    /// FIX: kiezen% werd nooit teruggezet en bleef tussen beurten staan.
    /// FIX: twee buren van dezelfde keten telden dubbel mee.
    public func bekijk(_ x: Int, _ y: Int) -> Zetkans {
        var aan: [Int] = []
        var los: [(x: Int, y: Int)] = []

        for (dx, dy) in Regels.buren {
            let nx = x + dx, ny = y + dy
            guard (1...Regels.grootte).contains(nx), (1...Regels.grootte).contains(ny) else { continue }
            let v = bord[nx][ny]
            if v >= 1 && v <= Regels.aantalKetens {
                if !aan.contains(v) { aan.append(v) }
            } else if v == Regels.los {
                los.append((nx, ny))
            }
        }

        var besteGrootte = 0, winnaar = 0
        var gelijken: [Int] = []
        for c in aan {
            let n = groottes[c]
            if n > besteGrootte {
                besteGrootte = n; winnaar = c; gelijken = [c]
            } else if n == besteGrootte {
                gelijken.append(c)
            }
        }

        // kanniet%: naast een losse steen leggen mag alleen als je ook een
        // bestaande keten raakt, anders zou er ongemerkt een keten ontstaan.
        return Zetkans(x: x, y: y,
                       mag: !aan.isEmpty || los.isEmpty,
                       aanliggend: aan, losse: los,
                       winnaar: winnaar,
                       gelijkspel: gelijken.count > 1,
                       gelijken: gelijken)
    }

    /// PROCcontroleer_vrij (regel 2500): stichten mag op een vrij liggend vakje.
    public func kanStichten(_ x: Int, _ y: Int) -> Bool {
        guard bord[x][y] == Regels.leeg else { return false }
        for (dx, dy) in Regels.buren where vak(x + dx, y + dy) != Regels.leeg { return false }
        return true
    }

    /// De handplaatsen waarmee deze speler een geldige zet kan doen.
    public func geldigePlaatsen(_ speler: Int) -> [Int] {
        (0..<Regels.handGrootte).filter { s in
            let t = handen[speler][s]
            guard t != 0 else { return false }
            let p = Regels.tegelPositie(t)
            return bekijk(p.x, p.y).mag
        }
    }

    public func stichtbarePlaatsen(_ speler: Int) -> [Int] {
        guard !teStichten().isEmpty else { return [] }
        return (0..<Regels.handGrootte).filter { s in
            let t = handen[speler][s]
            guard t != 0 else { return false }
            let p = Regels.tegelPositie(t)
            return kanStichten(p.x, p.y)
        }
    }

    // MARK: - zetten doen

    private func bijvullen(_ speler: Int, _ plaats: Int) { handen[speler][plaats] = pak() }

    /// PROCstichten: een losse steen wordt meteen een keten van één tegel, en de
    /// stichter krijgt één gratis aandeel.
    @discardableResult
    public func sticht(speler: Int, plaats: Int, keten: Int) throws -> Zetverslag {
        let t = handen[speler][plaats]
        guard t != 0 else { throw OngeldigeZet("geen steen op die handplaats") }
        let p = Regels.tegelPositie(t)
        guard kanStichten(p.x, p.y) else { throw OngeldigeZet("dit vakje ligt niet vrij") }
        guard ketenGrootte(keten) == 0 else { throw OngeldigeZet("die keten bestaat al") }

        zetVak(p.x, p.y, keten)
        spelers[speler].aandelen[keten] += 1          // oprichtersaandeel
        bijvullen(speler, plaats)
        var verslag = Zetverslag(x: p.x, y: p.y)
        verslag.keten = keten
        return verslag
    }

    /// Een steen neerleggen; regels 3174-3210 (mens) en 3680-3710 (computer).
    /// `gekozenWinnaar` geldt alleen bij twee even grote ketens.
    @discardableResult
    public func leg(speler: Int, plaats: Int, gekozenWinnaar: Int = 0) throws -> Zetverslag {
        let t = handen[speler][plaats]
        guard t != 0 else { throw OngeldigeZet("geen steen op die handplaats") }
        let p = Regels.tegelPositie(t)
        let kans = bekijk(p.x, p.y)
        guard kans.mag else { throw OngeldigeZet("daar mag je niet leggen") }

        var verslag = Zetverslag(x: p.x, y: p.y)

        if kans.aanliggend.isEmpty {
            zetVak(p.x, p.y, Regels.los)              // losse steen, nog geen keten
        } else {
            // FIX: in het origineel overschreef PROCcontroleer_aanliggen de door
            // de speler gekozen keten weer met de eigen berekening (regel 3202).
            let w = kans.gelijken.contains(gekozenWinnaar) ? gekozenWinnaar : kans.winnaar
            let verliezers = kans.aanliggend.filter { $0 != w }

            // PROCverdeel_geld draait vóór het omkleuren, zodat de bonus met de
            // ketengroottes van vóór de fusie gerekend wordt.
            for lo in verliezers { verslag.uitkeringen.append(ontbind(verliezer: lo, winnaar: w)) }

            zetVak(p.x, p.y, w)
            for l in kans.losse { zetVak(l.x, l.y, w) }
            if !verliezers.isEmpty {
                for xx in 1...Regels.grootte {
                    for yy in 1...Regels.grootte where verliezers.contains(bord[xx][yy]) {
                        zetVak(xx, yy, w)
                    }
                }
            }

            verslag.keten = w
            verslag.opgeslokt = verliezers
        }

        bijvullen(speler, plaats)
        return verslag
    }

    /// PROCverdeel (regel 2800): de bonus uitkeren en de aandelen voor de helft
    /// omruilen in de winnende keten.
    ///
    /// FIX: de rangschikking kwam uit een tabel die pas na de koopfase van de
    /// vorige speler was bijgewerkt; hier wordt hij vers berekend.
    private func ontbind(verliezer: Int, winnaar: Int) -> Uitkering {
        let grootte = groottes[verliezer]
        let ketenPrijs = prijs(verliezer)
        let rang = rangorde(verliezer)
        let bezit = rang.map { spelers[$0].aandelen[verliezer] }
        let pot = (grootte + bezit.reduce(0, +)) * ketenPrijs

        var uit = Uitkering(keten: verliezer, naar: winnaar, grootte: grootte,
                            prijs: ketenPrijs, pot: pot)

        let deel = Bonus.verdeling(bezit)
        for i in 0..<Regels.aantalSpelers {
            let bedrag = Int((deel[i] * Double(pot)).rounded(.toNearestOrAwayFromZero))
            if bedrag == 0 { continue }
            spelers[rang[i]].geld += bedrag
            uit.bedragen[rang[i]] = bedrag
        }

        for q in 1...Regels.aantalSpelers {
            spelers[q].aandelen[winnaar] += spelers[q].aandelen[verliezer] / 2
            spelers[q].aandelen[verliezer] = 0
        }
        return uit
    }

    /// PROCkoopt (regel 9390): hoogstens drie aandelen van een bestaande keten.
    ///
    /// FIX: de lus in het origineel kon eindeloos doordraaien bij een saldo van
    /// precies 0, en gebruikte > waar >= bedoeld was.
    @discardableResult
    public func koop(speler: Int, keten: Int, aantal gevraagd: Int) -> Int {
        guard (1...Regels.aantalKetens).contains(keten), gevraagd > 0 else { return 0 }
        guard ketenGrootte(keten) > 0 else { return 0 }

        let p = spelers[speler]
        let stuksprijs = prijs(keten)
        var aantal = min(gevraagd, 3)
        while aantal > 0 && p.geld < stuksprijs * aantal { aantal -= 1 }
        guard aantal > 0 else { return 0 }

        p.aandelen[keten] += aantal
        p.geld -= stuksprijs * aantal
        return aantal
    }

    /// De speler kan niets kwijt en slaat de beurt over.
    public func past() { rondePassen += 1 }

    // MARK: - de AI

    /// PROCbekijk_fusie (regel 7000): hoe aantrekkelijk is deze fusie voor mij?
    ///
    /// FIX: het origineel vergeleek een ketennummer met een ketengrootte
    /// (kleinere(N%) <> MAX%) en gaf ook punten aan spelers zonder aandelen.
    private func fusiewaarde(_ kans: Zetkans, _ speler: Int) -> Int {
        var beste = 0
        for c in kans.aanliggend {
            let rang = rangorde(c)
            let eigen = spelers[speler].aandelen[c]
            let w: Int
            if eigen > 0 && (rang[0] == speler || rang[1] == speler) {
                w = c == kans.winnaar ? 5 : 10       // opgeslokte keten levert bonus op
            } else if eigen > 0 {
                w = 2                                // klein belang, weinig winst
            } else {
                w = 4
            }
            if w > beste { beste = w }
        }
        return beste
    }

    /// PROCbepaal_gunstigste_legsteen (regel 5200). nil als er niets kan.
    public func kiesPlaats(_ speler: Int) -> Int? {
        var punten: [(waarde: Int, plaats: Int)] = []
        for s in 0..<Regels.handGrootte {
            let t = handen[speler][s]
            if t == 0 { punten.append((0, s)); continue }
            let p = Regels.tegelPositie(t)
            let kans = bekijk(p.x, p.y)
            let w = !kans.mag ? 0 : (kans.aanliggend.isEmpty ? 2 : fusiewaarde(kans, speler))
            punten.append((w, s))
        }

        punten.sort { $0.waarde != $1.waarde ? $0.waarde > $1.waarde : $0.plaats < $1.plaats }
        if punten[0].waarde == 0 { return nil }

        // FIX: regel 5332 wilde voorkomen dat een al erg grote keten nog groter
        // gemaakt wordt, maar gebruikte het handnummer als ketennummer.
        for i in 0..<punten.count {
            let (waarde, plaats) = punten[i]
            if waarde == 0 { break }
            let p = Regels.tegelPositie(handen[speler][plaats])
            let kans = bekijk(p.x, p.y)
            let restWaarde = punten[(i + 1)...].contains { $0.waarde > 0 }
            if kans.winnaar > 0 && groottes[kans.winnaar] > 15 && waarde < 9 && restWaarde {
                continue
            }
            return plaats
        }
        return punten[0].plaats
    }

    /// PROCkopen (regel 9180): koop daar waar je positie bedreigd wordt.
    ///
    /// FIX: de rangordebewerking zat in het origineel in een decimaal ingepakt
    /// getal, en regel 9275 had een operatorvolgorde-fout waardoor ook
    /// niet-bestaande ketens gekocht konden worden.
    public func kiesAankoop(_ speler: Int) -> Int {
        let bestaand = bestaandeKetens()
        guard !bestaand.isEmpty else { return 0 }

        var kandidaten: [Int] = []
        for c in bestaand {
            let rang = rangorde(c)
            let bezit = rang.map { spelers[$0].aandelen[c] }
            for n in 0..<3 {
                guard rang[n] == speler else { continue }
                if bezit[n + 1] + 4 > bezit[n] {                 // iemand zit vlak achter me
                    kandidaten.append(c)
                } else if n + 2 < Regels.aantalSpelers && bezit[n + 1] == bezit[n + 2] {
                    kandidaten.append(c)
                }
                break
            }
            if kandidaten.count >= 4 { break }
        }

        for c in kandidaten {
            let rang = rangorde(c)
            let bezit = rang.map { spelers[$0].aandelen[c] }
            if rang[0] == speler && bezit[0] - bezit[1] > 4 { continue }   // onbedreigd eerste
            return c
        }
        return bestaand[Int.random(in: 0..<bestaand.count, using: &toeval)]
    }

    /// PROCcomp (regel 3582): de volledige beurt van een computerspeler.
    @discardableResult
    public func computerBeurt(_ speler: Int) -> [String] {
        let p = spelers[speler]
        var regels: [String] = []

        var gesticht = false
        let vrij = stichtbarePlaatsen(speler)
        if let eerste = vrij.first {
            let keuze = teStichten()
            let keten = keuze[Int.random(in: 0..<keuze.count, using: &toeval)]
            if let r = try? sticht(speler: speler, plaats: eerste, keten: keten) {
                regels.append(Taal.stichtKeten(p.naam, Regels.ketenLetter(keten),
                                               Regels.naam(r.x, r.y)))
                gesticht = true
            }
        }

        if !gesticht {
            guard let plaats = kiesPlaats(speler) else {
                past()
                regels.append(Taal.kanNiet(p.naam))
                return regels
            }
            if let r = try? leg(speler: speler, plaats: plaats) {
                regels.append(contentsOf: beschrijf(naam: p.naam, r))
            }
        }

        let keten = kiesAankoop(speler)
        let stuksprijs = prijs(keten)
        let n = koop(speler: speler, keten: keten, aantal: 3)
        regels.append(n > 0
            ? Taal.koopt(p.naam, n, Regels.ketenLetter(keten), stuksprijs)
            : Taal.kooptNiets(p.naam))
        return regels
    }

    /// Een zetverslag omzetten naar leesbare regels voor het meldingenvak.
    public func beschrijf(naam: String, _ r: Zetverslag) -> [String] {
        let plek = Regels.naam(r.x, r.y)
        var uit = [r.keten == 0
            ? Taal.legtLos(naam, plek)
            : Taal.legtBij(naam, plek, Regels.ketenLetter(r.keten))]
        for u in r.uitkeringen {
            uit.append(Taal.gaatOpIn(Regels.ketenLetter(u.keten), u.grootte,
                                     Regels.ketenLetter(u.naar), u.pot))
            for sleutel in u.bedragen.keys.sorted() {
                uit.append(Taal.uitkering(spelers[sleutel].naam, u.bedragen[sleutel]!))
            }
        }
        return uit
    }

    // MARK: - beurtloop

    /// PROCspelen (regel 4000). Levert de speler die aan de beurt is, of 0 als
    /// het spel afgelopen is.
    ///
    /// FIX: de test "iedereen kon niet" stond in het origineel boven in de lus
    /// en werd elke ronde teruggezet voordat hij ooit 6 kon bereiken.
    @discardableResult
    public func volgendeSpeler() -> Int {
        if klaar { return 0 }

        if aanDeBeurt == 0 {
            aanDeBeurt = 1; rondePassen = 0
        } else if aanDeBeurt >= Regels.aantalSpelers {
            if rondePassen >= Regels.aantalSpelers { return beeindig(Taal.niemandKanNog) }
            aanDeBeurt = 1; rondePassen = 0
        } else {
            aanDeBeurt += 1
        }

        beurt += 1
        if beurt >= Regels.maxBeurten { return beeindig(Taal.beurtenGespeeld(Regels.maxBeurten)) }
        for c in 1...Regels.aantalKetens where groottes[c] > Regels.eindKetenGrootte {
            return beeindig(Taal.ketenTeGroot(Regels.ketenLetter(c), Regels.eindKetenGrootte))
        }
        return aanDeBeurt
    }

    private func beeindig(_ reden: String) -> Int {
        klaar = true
        eindreden = reden
        return 0
    }

    /// De eindstand, van hoog naar laag vermogen.
    public func eindstand() -> [(nummer: Int, naam: String, vermogen: Int)] {
        (1...Regels.aantalSpelers)
            .map { (nummer: $0, naam: spelers[$0].naam, vermogen: vermogen($0)) }
            .sorted { $0.vermogen > $1.vermogen }
    }

    // MARK: - alleen voor de zelftest

    /// Zet een stand met de hand klaar. Niet gebruiken tijdens een partij.
    internal func proefstandLeegmaken() {
        for x in 1...Regels.grootte {
            for y in 1...Regels.grootte { zetVak(x, y, Regels.leeg) }
        }
        for q in 1...Regels.aantalSpelers {
            spelers[q].geld = Regels.startgeld
            for c in 1...Regels.aantalKetens { spelers[q].aandelen[c] = 0 }
        }
    }

    /// Legt de opgegeven vakjes als hand klaar. Alleen voor de zelftest.
    internal func proefhand(_ speler: Int, _ vakjes: [(x: Int, y: Int)]) {
        for s in 0..<Regels.handGrootte { handen[speler][s] = 0 }
        for (s, vakje) in vakjes.enumerated() where s < Regels.handGrootte {
            for n in 1...(Regels.grootte * Regels.grootte) {
                let p = Regels.tegelPositie(n)
                if p.x == vakje.x && p.y == vakje.y { handen[speler][s] = n; break }
            }
        }
    }
}
