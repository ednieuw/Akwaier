# AKWAIER voor Windows 11

Omzetting van `AQUIRE.BBC` (BBC Micro, "aquire 23/05/87") naar een Windows-programma
met muisbediening. Het spel, de spelers en de regels zijn die van het origineel.

## Starten

Dubbelklik op **`Akwaier-app\Akwaier.exe`**.

Er hoeft niets geïnstalleerd te worden. Het is één self-contained bestand met de
complete .NET 8-runtime erin, vandaar de omvang van 162 MB. Als Windows vraagt om
"​.NET 8.0 Desktop Runtime" te downloaden, komt dat van een tussenbuild en niet van
dit bestand — die vraag hoor je bij `Akwaier-app\Akwaier.exe` niet te krijgen.

Het programma schrijft twee bestanden naast zichzelf:

- `akwaier-scores.json` — de scorelijst, die het bestand `SCORE` van het origineel vervangt.
- `akwaier-fout.txt` — alleen als er onverwacht iets misgaat.

## Twee weergaven

Rechtsboven staat de knop **Weergave**, die wisselt tussen:

- **Modern** — rustige kleuren, kolomnummers 1 t/m 15, lichte achtergrond.
- **Retro BBC** — de kleurparen uit `DATA 29000-29007` van het origineel
  (A zwart, B groen, C geel, D blauw, E magenta, F lichtblauw, G wit, H rood),
  zwarte achtergrond, en de kolomkop `123456789012345` van het BBC-scherm. Twee
  kleuren zijn bijgesteld omdat ze niet te lezen waren: het vlak van keten A staat
  op 20,20,20 in plaats van zuiver zwart, en de F heeft een zwarte letter.

Beide werken met de muis; de weergave verandert alleen het uiterlijk.

Hoe je speelt staat in `HANDLEIDING.md`.

## Taal

Nederlands en Engels, te kiezen in de startdialoog. Alles gaat mee: de knoppen, het
gegevenspaneel, het meldingenvak en de namen van de medespelers (OPA/OMA/... wordt
GRANDPA/GRANDMA/...). De eerste keer volgt het spel de taalinstelling van Windows;
alleen bij een Nederlandse instelling begint het in het Nederlands. Je keuze wordt
bewaard in `akwaier-scores.json` en geldt de volgende keer meteen.

Alle teksten staan bij elkaar in `AkwaierWin\Engine\Taal.cs`, net als bij Klaverjas.

## Bediening

- Klik op een steen in je hand, of op het bijbehorende vakje op het bord. Je acht
  stenen staan als cijfers 1 t/m 8 op het bord, net als in regel 3571 van het
  origineel. De toetsen 1 t/m 8 werken ook.
- Onderin verschijnt dan **Leg neer**, en waar dat mag ook **Sticht keten**.
- Bij een fusie tussen twee even grote ketens kies je zelf welke blijft bestaan.
- Daarna koop je hoogstens drie aandelen van één keten, of niets.
- **snel spelen** maakt de wachttijd tussen de computerbeurten bijna nul.
- **Alleen kijken** in de startdialoog laat de zes computerspelers zelf spelen.

## Wat er in deze map staat

| bestand | inhoud |
|---|---|
| `Akwaier-app\Akwaier.exe` | **het programma** — self-contained, niets te installeren |
| `AkwaierWin\` | de C#-broncode: `Engine\` heeft alle regels, `Ui\` alleen schermwerk |
| `akwaier.ico`, `make_icon.py` | het pictogram en het scriptje dat het tekent |
| `HANDLEIDING.md` | de speelhandleiding: scherm, beurt, regels en tactiek |
| `handleiding.html` + `schermbeeld-*.png` | dezelfde handleiding als webpagina met schermafdrukken; die vier PNG's horen erbij |
| `akwaier.py`, `akwaier_engine.py`, `START_AKWAIER.bat` | een eerdere Python/tkinter-versie van hetzelfde spel; die blijft staan als tweede uitvoering om de regels tegen af te lezen |
| `AQUIRE_detokenized.bas` | de originele BBC BASIC-broncode |
| `AQUIRE.BBC`, `AQUIRE.EXE`, `BBCBASIC.EXE`, `SCORE*` | het origineel uit 1987 |

De engine staat helemaal los van het scherm, dus een andere weergave kan er later
voor gezet worden zonder de regels aan te raken.

## Zelf bouwen

```bash
dotnet publish AkwaierWin -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o Akwaier-app
```

De SDK staat in `%USERPROFILE%\.dotnet`; zet `DOTNET_ROOT` daarheen voordat je bouwt,
want in `C:\Program Files\dotnet` staat alleen .NET 6.

De regels nalopen na een wijziging:

```bash
Akwaier-app\Akwaier.exe zelftest
```

Dat rekent alle proeven na en schrijft `akwaier-zelftest.txt` ernaast; bij een fout
komt er ook een melding in beeld.

## De regels zoals ze in de BBC-code zaten

- Bord 15×15, rijen A t/m O, kolommen 1 t/m 15. Een steen heet `B7`: rij B, kolom 7.
- Acht ketens A t/m H. Prijs = basisprijs (900, 900, 800, 700, 700, 600, 500, 400)
  + 100 per gehaalde drempel (1, 2, 3, 4, 5, 10, 15, 20, 25, 36 tegels)
  + 10 per uitstaand aandeel.
- Een keten **stichten** kan alleen op een vakje waar alle vier de buren leeg zijn.
  De keten begint met één tegel en de stichter krijgt één gratis aandeel.
- Leg je een steen zonder te stichten op een vrij vakje, dan wordt het een **losse
  steen**. Die hoort bij geen enkele keten en wordt pas opgeslokt zodra iemand
  ernaast legt én daarbij een bestaande keten raakt.
- Naast een losse steen leggen zonder een keten te raken **mag niet** — dan zou er
  ongemerkt een keten ontstaan.
- Bij een **fusie** wint de grootste keten; bij gelijke grootte kiest de speler.
  De bonuspot is (aantal tegels + aantal uitstaande aandelen) × aandeelprijs en
  gaat naar de grootste aandeelhouders volgens de staffel uit regels 2810-2819.
  De aandelen van de opgeslokte keten worden voor de helft omgeruild in aandelen
  van de winnende keten.
- Het spel eindigt bij een keten van meer dan 100 tegels, na 210 beurten, of als
  alle zes de spelers een ronde lang niets kwijt kunnen.

## Wat er is rechtgezet

Het origineel is één op één nagespeeld, met alleen deze aantoonbare programmeer-
fouten hersteld. Elke correctie staat in de broncode gemarkeerd met `FIX:`.

| regel in `.bas` | fout | wat er nu gebeurt |
|---|---|---|
| 1215 | buur werd uitgelezen vóór de randcontrole, en las geheugen buiten het bord | eerst randcontrole |
| 1220 | twee buren van dezelfde keten telden dubbel mee bij een fusie | ketens worden ontdubbeld |
| 1255 | `kiezen%` werd nooit teruggezet en bleef tussen beurten staan | per zet opnieuw bepaald |
| 1430-1440 | een tegel kon bij een dubbele vermelding twee keer omgekleurd worden | elke tegel één keer |
| 2430 | `PROClege_steen_terug` wiste het middenvakje nóg eens in plaats van de buur te herstellen | vervangen door een proefzet die het bord niet aanraakt |
| 2810-2819 | de test `B=C=D` stond ná `B=C` en was daardoor onbereikbaar; de breuken telden op tot 0,99 in plaats van 1 | tests van specifiek naar algemeen, exacte breuken |
| 3202 | de door de speler gekozen "grootste keten" werd meteen weer overschreven | de keuze telt |
| 4013 | de test "iedereen kan niet" werd elke ronde teruggezet vóór hij 6 kon bereiken | aan het einde van de ronde getoetst |
| 4265, 9275 | operatorvolgorde: `x AND (a<>a)` en `m AND (s=m)` in plaats van `(x AND a)<>a` | haakjes goedgezet |
| 5332-5333 | de AI gebruikte het handnummer als ketennummer, en regel 5333 was dode code | de AI kijkt nu echt naar de ketengrootte |
| 7040-7041 | een ketennummer werd vergeleken met een ketengrootte; spelers zonder aandelen kregen punten | vergelijking met de winnende keten, alleen echte aandeelhouders |
| 9415 | de kooplus kon eindeloos doordraaien bij een saldo van precies 0 | begrensd |
| 1601 | de laatste steen bleef altijd ongebruikt in de zak liggen | de zak wordt leeggespeeld |
| 3715 | `PROCverkopen` werd aangeroepen maar bestond nergens | vervallen; er is geen verkoopfase |

De computerspeler denkt verder precies zoals in het origineel: hij sticht een keten
zodra dat kan, kiest anders de steen met de hoogste "waarde" (voorkeur voor een
fusie waarbij een keten verdwijnt waarin hij zelf grootaandeelhouder is), en koopt
daar waar zijn positie bedreigd wordt.

## Controle

`Akwaier.exe zelftest` rekent tien proeven na: prijsstaffel, legaliteit, het
opslokken van losse stenen, enkelvoudige en drievoudige fusie, gelijkspel, stichten,
kopen, alle acht gevallen van de verdeelsleutel, en zestig volledig uitgespeelde
partijen waarbij na elke zet gecontroleerd wordt dat bord, ketengroottes, aandelen,
geld en de 225 stenen blijven kloppen. De uitkomsten van de fusieproeven (pot 16900,
uitkering 11267 en 5633) zijn in de C#- en de Python-versie tot op de gulden gelijk.
