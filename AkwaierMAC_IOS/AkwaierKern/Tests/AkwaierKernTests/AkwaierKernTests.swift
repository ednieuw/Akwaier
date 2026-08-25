import XCTest
@testable import AkwaierKern

/// Draait dezelfde tien proeven als `swift run AkwaierZelftest`, zodat Xcode
/// ze meepakt bij Product > Test.
final class AkwaierKernTests: XCTestCase {

    func testAlleRegels() {
        let uitslag = Zelftest.draai()
        // Het hele verslag in de uitvoer, zodat je bij een fout meteen ziet welke
        // proef het is.
        print(uitslag.verslag)
        XCTAssertEqual(uitslag.fouten, 0, "zelftest meldt \(uitslag.fouten) fout(en)")
    }

    /// Een steennummer hoort precies één vakje aan te wijzen, en alle 225
    /// nummers samen het hele bord. Dit is de meest gemaakte fout bij het
    /// overzetten, want de omrekening in het origineel is ongebruikelijk.
    func testTegelPositieIsEenOpEen() {
        var gezien = Set<String>()
        for n in 1...225 {
            let p = Regels.tegelPositie(n)
            XCTAssertTrue((1...15).contains(p.x), "kolom buiten bereik bij steen \(n)")
            XCTAssertTrue((1...15).contains(p.y), "rij buiten bereik bij steen \(n)")
            gezien.insert("\(p.x),\(p.y)")
        }
        XCTAssertEqual(gezien.count, 225, "niet elk vakje wordt precies één keer geraakt")
    }

    /// De vaknaam is rijletter + kolomnummer, dus B7 is rij B, kolom 7.
    func testVaknaam() {
        XCTAssertEqual(Regels.naam(7, 2), "B7")
        XCTAssertEqual(Regels.naam(15, 15), "O15")
        XCTAssertEqual(Regels.naam(1, 1), "A1")
    }
}
