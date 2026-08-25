# Opdracht: Akwaier voor iPad en Mac

**Voor Claude Code op de MacBook.** Je leest dit op een machine waar dit werk niet
gemaakt is. Alles wat je nodig hebt staat in deze map; je hoeft niets van de
Windows-machine te halen.

---

## Wat het wordt

Een SwiftUI-app **Akwaier** die op iPad én Mac draait: een omzetting van het
BBC Micro-spel `AQUIRE.BBC` uit 1987. Het spel heet Akwaier en niet Acquire,
omdat die naam van het bordspel van Sid Sackson is.

De Windows-versie is af en werkt. Deze app moet **dezelfde partij spelen** — niet
dezelfde knoppen hebben.

## Wat er al ligt

| map | inhoud |
|---|---|
| `AkwaierKern/` | een complete SwiftPM-package met **de hele spelkern in Swift**, plus een zelftest |
| `Referentie/CSharp-engine/` | dezelfde engine in C#, waar de Swift-versie regel voor regel uit vertaald is |
| `Referentie/AQUIRE_detokenized.bas` | de originele BBC BASIC-broncode uit 1987 |
| `Referentie/HANDLEIDING.md` | de volledige spelregels in gewone taal, met rekenvoorbeelden |
| `Referentie/schermbeeld-*.png` | hoe de Windows-versie eruitziet |

> **Let op.** De Swift-code in `AkwaierKern/` is op een Windows-machine geschreven
> en daar **nooit gecompileerd** — er staat geen Swift op. Reken erop dat je een
> handvol compilerfouten moet wegwerken. De *logica* is wel getoetst: de
> C#-versie waar dit uit vertaald is, doorstaat alle tien proeven.

## Stap 1 — laat de kern eerst draaien

```bash
cd AkwaierKern
swift test
```

Dat draait tien proeven, waaronder zestig volledig uitgespeelde partijen waarbij
na elke zet gecontroleerd wordt of bord, ketengroottes, aandelen, geld en de 225
stenen nog kloppen. Er hoort te staan:

```
10 zestig volledig uitgespeelde partijen
  ok  : na elke zet kloppen bord, ketens, aandelen, geld en de 225 stenen (…)

ALLES GOED
```

Wil je het losse verslag zien: `swift run AkwaierZelftest`.

**Ga pas verder als dit groen is.** Bouw geen scherm bovenop een kern die niet
klopt; de fusielogica is het lastigste deel en fouten daarin zie je pas veel later.

Krijg je compilerfouten, verbeter dan de Swift-code — niet de proeven. Loopt een
*proef* mis, vergelijk dan met `Referentie/CSharp-engine/Spel.cs`: dat is de
werkende versie.

## Stap 2 — het Xcode-project

Maak een nieuw multiplatform App-project **Akwaier** (iOS + macOS). Sleep de map
`AkwaierKern` erin als lokale package (File > Add Package Dependencies > Add
Local), zodat de kern een eigen module blijft en je hem apart kunt blijven
toetsen. Voeg hem als dependency toe aan beide targets.

De strikte scheiding tussen kern en scherm is het hele punt van de opzet: de
kern weet niets van SwiftUI, en het scherm rekent zelf niets uit.

## Stap 3 — het scherm

Bouw dit met SwiftUI. De Windows-indeling is een goede leidraad, maar neem hem
niet klakkeloos over: een iPad is geen venster met knoppenstroken.

Het spelscherm heeft vier dingen nodig:

1. **Het bord**, 15×15. Rijen A t/m O van boven naar beneden, kolommen 1 t/m 15.
   Een vakje heet `B7`: rijletter, dan kolomnummer.
2. **Het gegevenspaneel**: per keten de prijs en het aantal tegels; het
   aandeelbezit van de zes spelers met de grootste en op één na grootste
   aandeelhouder gemarkeerd; en per speler geld en vermogen.
3. **De hand**: acht stenen, waarvan je ziet welke je kwijt kunt.
4. **Het meldingenvak**: wat iedere speler doet, inclusief de bonusuitkeringen.

Aanwijzingen die er echt toe doen:

- Op iPad staat het bord links en het paneel rechts; in staande stand of op een
  iPhone-formaat moet het paneel eronder of achter een tab. Gebruik
  `horizontalSizeClass`, geen vaste maten.
- Het bord is **vierkant**: bereken de vakgrootte uit de beschikbare ruimte
  (`GeometryReader`), en teken het niet op vaste pixels.
- Tikken op een vakje dat in je hand zit, kiest die steen. Op de Mac mag klikken
  hetzelfde doen. Toon de gekozen steen met een duidelijk kader.
- De acht ketens hebben elk een vaste kleur. Neem ze over uit
  `Referentie/CSharp-engine/` of uit de schermafdrukken. Zorg dat ze in
  donkere modus ook werken — dat is op iOS geen keuze.
- De computerspelers spelen met een korte pauze tussen de zetten, zodat je het
  kunt volgen. Eén schakelaar "snel spelen" haalt die pauze eruit.

## Stap 4 — taal

`Taal.swift` heeft alle teksten al, in het Nederlands en het Engels. Het scherm
mag **geen enkele losse tekst** bevatten; alles komt uit `Taal`.

De taal wordt in het startvenster gekozen, staat de eerste keer op de instelling
van het apparaat (alleen Nederlands als het apparaat Nederlands is, anders
Engels), en wordt bewaard in het scorebestand. Zie `Scorelijst.laadTaal()` en
`Scorelijst.bewaarTaal()`.

## Stap 5 — het startvenster

Naam, plaats aan tafel (of willekeurig), taal, en de keuze *Ik doe mee* of
*Alleen kijken*. In het origineel was dat de vraag "DOE JE MEE MENS ?".

---

## De vier valkuilen

Dit zijn de plekken waar het bij het overzetten misging. Ze staan ook in
`HANDLEIDING.md`, maar hier staat waarom ze verwarrend zijn.

**1 · Stichten kan alleen op een vrij liggend vakje.** Alle vier de buren moeten
leeg zijn, en de keten begint met één tegel. Dit is *niet* zoals het bordspel
Acquire, waar een keten ontstaat door naast een losse steen te leggen. Wie de
gewone regels kent, denkt hier aan een fout en gaat iets "repareren" wat goed is.
Het staat zo in `PROCcontroleer_vrij`, regel 2500 van het origineel.

**2 · Naast een losse steen leggen mag niet** als je niet tegelijk een bestaande
keten raakt. Anders zou er ongemerkt een keten ontstaan.

**3 · De omrekening van steennummer naar vakje is ongebruikelijk.**

```swift
var y = n % 15; if y == 0 { y = 15 }
var x = n / 15; if x == 0 { x = 15 }
```

Dat lijkt scheef, maar het is een sluitende één-op-één-afbeelding van 1…225 op
het hele bord. Er is een aparte test voor (`testTegelPositieIsEenOpEen`).
Niet "verbeteren".

**4 · De bonus wordt berekend vóór het omkleuren.** Bij een fusie moet je de
grootte en de prijs van de verdwijnende keten nemen zoals ze wáren, en pas
daarna de tegels omkleuren. Andersom klopt de uitkering niet.

---

## Wat je bewust *niet* moet doen

- **De spelregels moderniseren.** Dit is de BBC-versie uit 1987, niet het
  bordspel. Prijsstaffel, de 100-tegelregel, 210 beurten en de verdeelsleutel
  staan vast.
- **De kern aan het scherm knopen.** Geen SwiftUI-import in `AkwaierKern`.
- **De naam Acquire gebruiken** in de app, de bundle-id of op het scherm. Het
  heet Akwaier. Alleen in verwijzingen naar het origineel uit 1987 mag `AQUIRE`
  blijven staan — zo heetten die bestanden nu eenmaal.

## Hoe je weet dat het klopt

- `swift test` blijft groen.
- Speel een partij uit in kijkmodus en vergelijk het verloop met
  `Referentie/schermbeeld-modern.png`: dezelfde soort meldingen, ketens die
  groeien en fuseren, en aan het eind een keten van meer dan 100 tegels.
- De fusie uit de proef moet exact deze getallen geven: keten A van 3 tegels bij
  prijs 1300 met tien uitstaande aandelen levert een pot van **16.900**, waarvan
  **11.267** naar de grootste en **5.633** naar de tweede aandeelhouder gaat.
  Diezelfde bedragen komen er in de Windows-versie uit.

---

*Volledige spelregels: `Referentie/HANDLEIDING.md`. Wat er in de omzetting is
rechtgezet ten opzichte van de BASIC-code van 1987: zie de `FIX:`-opmerkingen in
`Referentie/CSharp-engine/Spel.cs` en `AkwaierKern/Sources/AkwaierKern/Spel.swift`.*
