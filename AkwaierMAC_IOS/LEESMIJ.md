# Akwaier — map voor de MacBook

Deze map is bedoeld om in zijn geheel naar de MacBook gekopieerd te worden. Daar
laat je Claude Code er de iPad/Mac-versie van bouwen.

## Waar te beginnen

1. **`OPDRACHT.md`** — geef dit aan Claude Code op de Mac. Daar staat wat er
   gebouwd moet worden, in welke volgorde, en welke vier valkuilen er zijn.
2. `AkwaierKern/` — de spelkern is er al, in Swift. Eerst `swift test` draaien.
3. `Referentie/` — alles om op terug te vallen.

## De naam

Het spel heet **Akwaier**. Acquire is de naam van het bordspel van Sid Sackson;
die gebruiken we niet in de app, de bundle-id of op het scherm. Alleen de
bestanden van het origineel uit 1987 houden hun naam `AQUIRE.*` — zo heetten ze.

De Windows-versie in `..\ACQUIRE\` heet sinds 23 augustus 2026 ook Akwaier: map
`AkwaierWin\`, programma `Akwaier-app\Akwaier.exe`. De map zelf heet nog ACQUIRE,
omdat daar de bestanden van 1987 in staan.

## Inhoud

```
OPDRACHT.md                      de opdracht voor Claude Code op de Mac
LEESMIJ.md                       dit bestand

AkwaierKern/                     SwiftPM-package met de spelkern
  Package.swift
  Sources/AkwaierKern/
    Regels.swift                 vaste getallen, steennummer -> vakje, toevalsreeks
    Speler.swift                 speler, zetkans, uitkering, zetverslag
    Bonus.swift                  de verdeelsleutel van de fusiebonus
    Spel.swift                   alle regels: leggen, stichten, fuseren, kopen, de AI
    Taal.swift                   alle teksten, Nederlands en Engels
    Scorelijst.swift             scorebestand en taalkeuze
    Zelftest.swift               tien proeven, waaronder zestig hele partijen
  Sources/AkwaierZelftest/       swift run AkwaierZelftest
  Tests/AkwaierKernTests/        swift test, en Product > Test in Xcode

Referentie/
  AQUIRE_detokenized.bas         de originele BBC BASIC-broncode uit 1987
  CSharp-engine/*.cs             dezelfde kern in C#, de bron van de vertaling
  HANDLEIDING.md                 de volledige spelregels in gewone taal
  schermbeeld-*.png              hoe de Windows-versie eruitziet
  akwaier-pictogram.ico          het pictogram (moet nog naar .icns/asset catalog)
  make_icon.py                   het scriptje dat dat pictogram tekent
```

## Wat er nog niet is

- **Het scherm.** Er is nog geen enkele SwiftUI-code; dat is het werk op de Mac.
- **Het pictogram voor Apple.** `make_icon.py` tekent een PNG-achtige tegel met
  een A; die moet nog naar een asset catalog (en voor de Mac naar `.icns`). Het
  scriptje is zonder tekenpakket geschreven, dus het draait ook op de Mac met
  gewoon `python3`.
- **De Swift-code is nooit gecompileerd.** Er staat geen Swift op de
  Windows-machine waar dit gemaakt is. De logica is wel getoetst via de
  C#-versie, die alle tien proeven doorstaat.

## Gelijk houden met Windows

Wijkt de Swift-versie straks ergens van de Windows-versie af, houd dat dan bij in
een `WIJZIGINGEN-Swift.md` in deze map — dezelfde werkwijze als bij Klaverjas.
Dan kan de C#-kant later dezelfde kant op en blijven de twee dezelfde partij
spelen.
