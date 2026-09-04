# Opdracht voor Claude Code: AQUIRE.BBC omzetten naar Windows 11-programma

## Bronmateriaal
- `AQUIRE_detokenized.bas` — volledig gedetokeniseerde BBC BASIC-broncode (682 regels,
  regelnummers 5 t/m 30050). Dit is het originele 1987 BBC Micro-spel "Aquire", een
  vertaling/implementatie van het bordspel **Acquire** (Sid Sackson) voor 6 spelers,
  in het Nederlands. Namen van de spelers zijn hardcoded: OPA, OMA, BROER, ZUS, PA, MA
  (een familiespel).
- Het originele bestand was een binair getokeniseerd BBC BASIC-bestand in het
  "basic80"-formaat (lengte-byte, dan regelnummer little-endian lo/hi — dus AFWIJKEND
  van het standaard Acorn-formaat hi/lo/lengte). Dat is al gedecodeerd; je hoeft dit
  niet opnieuw te doen.

## Doel
Herbouw dit spel als een op zichzelf staand Windows 11-programma (Python, geen externe
build-vereisten die Ed niet makkelijk kan installeren — dus liefst standaardbibliotheek
zoals `tkinter`, geen exotic deps). Eindresultaat: iets wat Ed met een dubbelklik of
`python aquire.py` kan starten op Windows 11.

## Spelregels zoals ze in de BBC-code zitten (lees de .bas voor details)
- Bord: 15×15, coördinaten x%=1..15 (kolommen, letters via `CHR$(x+64)`), y%=1..15
  (rijen). Legale tegel-array `keten(x,y)` opgeslagen op `&A00+15*x+y`, waarde -1 = leeg,
  0..7 (of 1..8, check indexing zorgvuldig — code gebruikt soms 0-based soms 1-based,
  dit moet je exact naspeuren) = kleur/keten-ID, anders "los tegeltje zonder keten"
  wordt apart bijgehouden (zie `steen_leggen`, kleur%=10 voor een net-gelegde losse tegel
  vóór hij bij een keten hoort).
- 8 ketens/aandelen (kleuren A t/m H), genummerd 1..8. Prijs per keten
  (`prijs_keten%`) hangt af van grootte (`FNmax`) volgens een staffel in
  `PROCbereken_prijs_keten` (regel 1900) + eigen aandeelbezit.
- Fusielogica: `PROCcontroleer_aanliggen` (1200) checkt de 4 buren van een gelegde
  tegel, bepaalt welke ketens aanliggen; `PROCkies_grootste` (1250) bepaalt welke keten
  "wint" bij een fusie (grootste blijft bestaan); bij gelijke grootte moet de mens zelf
  kiezen (`kiezen%`).
- Bij fusie (`PROCverander_na_fusie`, 1400 en `PROCverdeel_geld`/`PROCverdeel`, 2700-2850):
  bonus wordt uitgekeerd aan grootste/twee-na-grootste aandeelhouder volgens een
  percentagetabel (regels 2810-2819), aandelen van de opgeslokte keten worden voor de
  helft omgeruild in aandelen van de winnende keten.
- "Stichten" van een nieuwe keten (`PROCstichten`, 4500): als een net gelegde tegel geen
  keten heeft maar wel losse buurtegel(s), ontstaat een nieuwe keten; menselijke speler
  kiest de kleur, computer kiest automatisch (regels 4550-4630, vrij cryptische
  bit-logica met `macht%()` als machten-van-2-bitmasker voor "welke ketens bestaan al").
- Kopen: elke beurt mag een speler tot 3 aandelen kopen (`PROCkopen`/`PROCkoopt`,
  9180-9590), begrensd door `bezit%()` (banksaldo) en beschikbaarheid.
- Computerspeler-AI: `PROCcomp` (3582) + `PROCbepaal_gunstigste_legsteen` (5200) +
  `PROCbepaal_waarde_steen`/`PROCbekijk_fusie` (5550, 7000) — kiest tegel met hoogste
  "waarde" (voorkeur voor fuseren met keten waar de computer zelf aandelen in heeft).
- Einde spel: als een keten >100 tegels heeft OF na 210 beurten (`W%>=210`) stopt het
  spel (`PROCspelen`, 4000-4040) en volgt een score-overzicht (`PROCscore`, 4045).
- Bewaren tussen sessies gebeurde origineel via een lokaal bestand "SCORE"
  (highscore-lijst) — mag je vervangen door een simpel JSON-bestand in dezelfde map.

## Wat Ed als vervolgvraag al beantwoord heeft
Zodra hij kiest tussen (a) grafisch bord met muisbediening, (b) tekstversie zo dicht
mogelijk bij het origineel, of (c) "maakt niet uit" — en tussen (a) exacte 1-op-1
naspeling van de regels of (b) opgeschoonde/gemoderniseerde versie — geef die keuze als
uitgangspunt. Als die antwoorden nog niet zijn doorgegeven aan jou (Code) in de sessie,
vraag Ed ernaar voordat je begint, want dat bepaalt UI-technologie en hoeveel
BBC-eigenaardigheden (bugs/quirks) je moet overnemen.

## Praktische aanwijzingen
1. Lees eerst de hele `.bas` grondig door — het is compact maar leesbaar Nederlands
   BASIC-programma, geen verdere reverse engineering nodig.
2. Bouw een duidelijk gescheiden spel-engine (regels/state) los van de UI-laag, zodat
   een latere UI-wissel niet het hele spel hoeft te herschrijven.
3. Let op 1-based vs 0-based array-indexering in het origineel (BASIC arrays zijn hier
   overwegend 1-based, met kleur%=-1 voor leeg en 10 als tijdelijke marker) — dit is een
   veelvoorkomende bron van off-by-one-fouten bij het overzetten.
4. De `&`-hexadecimale constantes en `?adres`-geheugentoegang (bv. `?&900`, `?&A00`,
   `?&B30`) zijn simpele byte-arrays in het origineel; vertaal deze gewoon naar normale
   Python-lijsten/dicts, geen geheugen-simulatie nodig.
5. Test de fusie- en verkooplogica met een paar handmatige scenario's, want dat is het
   meest foutgevoelige deel van het origineel.

Volledige brontekst staat in `AQUIRE_detokenized.bas` in dezelfde map.
