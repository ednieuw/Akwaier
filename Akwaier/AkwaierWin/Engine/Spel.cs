namespace Akwaier.Engine;

/// <summary>
/// De volledige spelstand van een partij Akwaier, met alle regels uit
/// AQUIRE.BBC (BBC Micro, "aquire 23/05/87"). Er zit geen schermcode in.
///
/// De regels zijn één op één overgenomen; de aantoonbare programmeerfouten uit
/// het origineel zijn hersteld en staan gemarkeerd met "FIX:".
/// </summary>
public sealed class Spel
{
    private readonly Random _toeval;
    private readonly int[,] _bord = new int[Regels.Grootte + 2, Regels.Grootte + 2];
    private readonly int[] _groottes = new int[Regels.AantalKetens + 1];
    private readonly int[,] _handen;              // [speler 1..6, plaats 0..7], 0 = geen steen
    private int _rondePassen;                     // kanniet%

    public Spel(int mensPlaats = 0, string mensNaam = "", int? zaad = null)
    {
        _toeval = zaad.HasValue ? new Random(zaad.Value) : new Random();

        for (int x = 1; x <= Regels.Grootte; x++)
            for (int y = 1; y <= Regels.Grootte; y++)
                _bord[x, y] = Regels.Leeg;

        Spelers = new Speler[Regels.AantalSpelers + 1];
        for (int i = 1; i <= Regels.AantalSpelers; i++)
            Spelers[i] = new Speler(i, Taal.Spelernamen[i - 1]);

        MensPlaats = mensPlaats;
        if (mensPlaats > 0)
        {
            var naam = (mensNaam ?? "").Trim().ToUpperInvariant();
            if (naam.Length > 8) naam = naam[..8];
            Spelers[mensPlaats].Mens = true;
            Spelers[mensPlaats].Naam = naam.Length > 0 ? naam : Taal.NaamloosMens;
        }

        for (int n = 1; n <= Regels.Grootte * Regels.Grootte; n++) Zak.Add(n);
        for (int i = Zak.Count - 1; i > 0; i--)
        {
            int j = _toeval.Next(i + 1);
            (Zak[i], Zak[j]) = (Zak[j], Zak[i]);
        }

        // PROCsteen_pakken stopte zodra er nog één steen over was; die bleef
        // ongebruikt liggen. FIX: de zak wordt nu netjes leeggespeeld.
        _handen = new int[Regels.AantalSpelers + 1, Regels.HandGrootte];
        for (int p = 1; p <= Regels.AantalSpelers; p++)
            for (int s = 0; s < Regels.HandGrootte; s++)
                _handen[p, s] = Pak();
    }

    public Speler[] Spelers { get; }
    public List<int> Zak { get; } = new();

    /// <summary>Op welke plaats de mens speelt; 0 betekent kijken.</summary>
    public int MensPlaats { get; }

    /// <summary>W%: het aantal gespeelde beurten.</summary>
    public int Beurt { get; private set; }

    public int AanDeBeurt { get; private set; }
    public bool Klaar { get; private set; }
    public string Eindreden { get; private set; } = "";

    // ------------------------------------------------------------ bord en zak

    private int Pak()
    {
        if (Zak.Count == 0) return 0;
        int t = Zak[^1];
        Zak.RemoveAt(Zak.Count - 1);
        return t;
    }

    private void ZetVak(int x, int y, int waarde)
    {
        int oud = _bord[x, y];
        if (oud == waarde) return;
        if (oud >= 1) _groottes[oud]--;
        _bord[x, y] = waarde;
        if (waarde >= 1) _groottes[waarde]++;
    }

    /// <summary>De inhoud van een vakje; buiten het bord telt als leeg.</summary>
    public int Vak(int x, int y) =>
        x >= 1 && x <= Regels.Grootte && y >= 1 && y <= Regels.Grootte ? _bord[x, y] : Regels.Leeg;

    /// <summary>FNmax(): het aantal tegels van een keten.</summary>
    public int KetenGrootte(int keten) =>
        keten >= 1 && keten <= Regels.AantalKetens ? _groottes[keten] : 0;

    /// <summary>De steen op een handplaats; 0 als de plaats leeg is.</summary>
    public int Hand(int speler, int plaats) => _handen[speler, plaats];

    /// <summary>sticht%: de ketens die nog gesticht kunnen worden.</summary>
    public List<int> TeStichten()
    {
        var uit = new List<int>();
        for (int c = 1; c <= Regels.AantalKetens; c++)
            if (_groottes[c] == 0) uit.Add(c);
        return uit;
    }

    public List<int> BestaandeKetens()
    {
        var uit = new List<int>();
        for (int c = 1; c <= Regels.AantalKetens; c++)
            if (_groottes[c] > 0) uit.Add(c);
        return uit;
    }

    // ------------------------------------------------------------- geldzaken

    /// <summary>PROCbereken_prijs_keten (regel 1900).</summary>
    public int Prijs(int keten)
    {
        int n = KetenGrootte(keten);
        if (n == 0) return 0;

        int p = Regels.Basisprijs[keten];
        foreach (int drempel in Regels.Prijsdrempels)
            if (n > drempel) p += 100;

        int uitstaand = 0;
        for (int q = 1; q <= Regels.AantalSpelers; q++) uitstaand += Spelers[q].Aandelen[keten];
        return p + uitstaand * 10;
    }

    /// <summary>PROCvermogen: banksaldo plus de waarde van alle aandelen.</summary>
    public int Vermogen(int speler)
    {
        int t = Spelers[speler].Geld;
        for (int c = 1; c <= Regels.AantalKetens; c++) t += Prijs(c) * Spelers[speler].Aandelen[c];
        return t;
    }

    /// <summary>
    /// PROCsort_aandelen: de spelers gesorteerd op aandeelbezit in een keten,
    /// aflopend; bij gelijk bezit wint het hoogste spelernummer, net als de
    /// sleutel 10*bezit+nummer uit het origineel.
    /// </summary>
    public int[] Rangorde(int keten)
    {
        var uit = new[] { 1, 2, 3, 4, 5, 6 };
        Array.Sort(uit, (p, q) =>
        {
            int vp = Spelers[p].Aandelen[keten] * 10 + p;
            int vq = Spelers[q].Aandelen[keten] * 10 + q;
            return vq.CompareTo(vp);
        });
        return uit;
    }

    // -------------------------------------------------------- zetten bekijken

    /// <summary>
    /// PROCcontroleer_aanliggen (regel 1200), maar zonder het bord aan te raken.
    ///
    /// FIX: het origineel las de buur uit vóór de randcontrole en las daarbij
    /// geheugen buiten de bordarray.
    /// FIX: kiezen% werd nooit teruggezet en bleef tussen beurten staan.
    /// FIX: twee buren van dezelfde keten telden dubbel mee.
    /// </summary>
    public Zetkans Bekijk(int x, int y)
    {
        var aan = new List<int>();
        var los = new List<(int, int)>();

        foreach (var (dx, dy) in Regels.Buren)
        {
            int nx = x + dx, ny = y + dy;
            if (nx < 1 || nx > Regels.Grootte || ny < 1 || ny > Regels.Grootte) continue;
            int v = _bord[nx, ny];
            if (v >= 1 && v <= Regels.AantalKetens) { if (!aan.Contains(v)) aan.Add(v); }
            else if (v == Regels.Los) los.Add((nx, ny));
        }

        int besteGrootte = 0, winnaar = 0;
        var gelijken = new List<int>();
        foreach (int c in aan)
        {
            int n = _groottes[c];
            if (n > besteGrootte) { besteGrootte = n; winnaar = c; gelijken = new List<int> { c }; }
            else if (n == besteGrootte) gelijken.Add(c);
        }

        // kanniet%: naast een losse steen leggen mag alleen als je ook een
        // bestaande keten raakt, anders zou er ongemerkt een keten ontstaan.
        return new Zetkans
        {
            X = x,
            Y = y,
            Mag = aan.Count > 0 || los.Count == 0,
            Aanliggend = aan,
            Losse = los,
            Winnaar = winnaar,
            Gelijkspel = gelijken.Count > 1,
            Gelijken = gelijken,
        };
    }

    /// <summary>PROCcontroleer_vrij (regel 2500): stichten mag op een vrij liggend vakje.</summary>
    public bool KanStichten(int x, int y)
    {
        if (_bord[x, y] != Regels.Leeg) return false;
        foreach (var (dx, dy) in Regels.Buren)
            if (Vak(x + dx, y + dy) != Regels.Leeg) return false;
        return true;
    }

    /// <summary>De handplaatsen waarmee deze speler een geldige zet kan doen.</summary>
    public List<int> GeldigePlaatsen(int speler)
    {
        var uit = new List<int>();
        for (int s = 0; s < Regels.HandGrootte; s++)
        {
            int t = _handen[speler, s];
            if (t == 0) continue;
            var (x, y) = Regels.TegelPositie(t);
            if (Bekijk(x, y).Mag) uit.Add(s);
        }
        return uit;
    }

    public List<int> StichtbarePlaatsen(int speler)
    {
        var uit = new List<int>();
        if (TeStichten().Count == 0) return uit;
        for (int s = 0; s < Regels.HandGrootte; s++)
        {
            int t = _handen[speler, s];
            if (t == 0) continue;
            var (x, y) = Regels.TegelPositie(t);
            if (KanStichten(x, y)) uit.Add(s);
        }
        return uit;
    }

    // ------------------------------------------------------------ zetten doen

    private void Bijvullen(int speler, int plaats) => _handen[speler, plaats] = Pak();

    /// <summary>
    /// PROCstichten: een losse steen wordt meteen een keten van één tegel, en de
    /// stichter krijgt één gratis aandeel.
    /// </summary>
    public Zetverslag Sticht(int speler, int plaats, int keten)
    {
        int t = _handen[speler, plaats];
        if (t == 0) throw new OngeldigeZet("geen steen op die handplaats");
        var (x, y) = Regels.TegelPositie(t);
        if (!KanStichten(x, y)) throw new OngeldigeZet("dit vakje ligt niet vrij");
        if (KetenGrootte(keten) != 0) throw new OngeldigeZet("die keten bestaat al");

        ZetVak(x, y, keten);
        Spelers[speler].Aandelen[keten]++;          // oprichtersaandeel
        Bijvullen(speler, plaats);
        return new Zetverslag { X = x, Y = y, Keten = keten };
    }

    /// <summary>
    /// Een steen neerleggen; regels 3174-3210 (mens) en 3680-3710 (computer).
    /// <paramref name="gekozenWinnaar"/> geldt alleen bij twee even grote ketens.
    /// </summary>
    public Zetverslag Leg(int speler, int plaats, int gekozenWinnaar = 0)
    {
        int t = _handen[speler, plaats];
        if (t == 0) throw new OngeldigeZet("geen steen op die handplaats");
        var (x, y) = Regels.TegelPositie(t);
        var kans = Bekijk(x, y);
        if (!kans.Mag) throw new OngeldigeZet("daar mag je niet leggen");

        var verslag = new Zetverslag { X = x, Y = y };

        if (kans.Aanliggend.Count == 0)
        {
            ZetVak(x, y, Regels.Los);               // losse steen, nog geen keten
        }
        else
        {
            // FIX: in het origineel overschreef PROCcontroleer_aanliggen de door
            // de speler gekozen keten weer met de eigen berekening (regel 3202).
            int w = kans.Gelijken.Contains(gekozenWinnaar) ? gekozenWinnaar : kans.Winnaar;
            var verliezers = kans.Aanliggend.Where(c => c != w).ToList();

            // PROCverdeel_geld draait vóór het omkleuren, zodat de bonus met de
            // ketengroottes van vóór de fusie gerekend wordt.
            foreach (int lo in verliezers) verslag.Uitkeringen.Add(Ontbind(lo, w));

            ZetVak(x, y, w);
            foreach (var (lx, ly) in kans.Losse) ZetVak(lx, ly, w);
            if (verliezers.Count > 0)
                for (int xx = 1; xx <= Regels.Grootte; xx++)
                    for (int yy = 1; yy <= Regels.Grootte; yy++)
                        if (verliezers.Contains(_bord[xx, yy])) ZetVak(xx, yy, w);

            verslag.Keten = w;
            verslag.Opgeslokt.AddRange(verliezers);
        }

        Bijvullen(speler, plaats);
        return verslag;
    }

    /// <summary>
    /// PROCverdeel (regel 2800): de bonus uitkeren en de aandelen voor de helft
    /// omruilen in de winnende keten.
    ///
    /// FIX: de rangschikking kwam uit een tabel die pas na de koopfase van de
    /// vorige speler was bijgewerkt; hier wordt hij vers berekend.
    /// </summary>
    private Uitkering Ontbind(int verliezer, int winnaar)
    {
        int grootte = _groottes[verliezer];
        int prijs = Prijs(verliezer);
        int[] rang = Rangorde(verliezer);
        var bezit = rang.Select(q => Spelers[q].Aandelen[verliezer]).ToArray();
        int pot = (grootte + bezit.Sum()) * prijs;

        var uit = new Uitkering
        {
            Keten = verliezer, Naar = winnaar, Grootte = grootte, Prijs = prijs, Pot = pot,
        };

        double[] deel = Bonus.Verdeling(bezit);
        for (int i = 0; i < Regels.AantalSpelers; i++)
        {
            int bedrag = (int)Math.Round(deel[i] * pot, MidpointRounding.AwayFromZero);
            if (bedrag == 0) continue;
            Spelers[rang[i]].Geld += bedrag;
            uit.Bedragen[rang[i]] = bedrag;
        }

        for (int q = 1; q <= Regels.AantalSpelers; q++)
        {
            Spelers[q].Aandelen[winnaar] += Spelers[q].Aandelen[verliezer] / 2;
            Spelers[q].Aandelen[verliezer] = 0;
        }
        return uit;
    }

    /// <summary>
    /// PROCkoopt (regel 9390): hoogstens drie aandelen van een bestaande keten.
    ///
    /// FIX: de lus in het origineel kon eindeloos doordraaien bij een saldo van
    /// precies 0, en gebruikte &gt; waar &gt;= bedoeld was.
    /// </summary>
    public int Koop(int speler, int keten, int aantal)
    {
        if (keten < 1 || keten > Regels.AantalKetens || aantal <= 0) return 0;
        if (KetenGrootte(keten) == 0) return 0;

        var p = Spelers[speler];
        int prijs = Prijs(keten);
        aantal = Math.Min(aantal, 3);
        while (aantal > 0 && p.Geld < prijs * aantal) aantal--;
        if (aantal <= 0) return 0;

        p.Aandelen[keten] += aantal;
        p.Geld -= prijs * aantal;
        return aantal;
    }

    /// <summary>De speler kan niets kwijt en slaat de beurt over.</summary>
    public void Past() => _rondePassen++;

    // ------------------------------------------------------------------- de AI

    /// <summary>
    /// PROCbekijk_fusie (regel 7000): hoe aantrekkelijk is deze fusie voor mij?
    ///
    /// FIX: het origineel vergeleek een ketennummer met een ketengrootte
    /// (kleinere(N%) &lt;&gt; MAX%) en gaf ook punten aan spelers zonder aandelen.
    /// </summary>
    private int Fusiewaarde(Zetkans kans, int speler)
    {
        int beste = 0;
        foreach (int c in kans.Aanliggend)
        {
            int[] rang = Rangorde(c);
            int eigen = Spelers[speler].Aandelen[c];
            int w;
            if (eigen > 0 && (rang[0] == speler || rang[1] == speler))
                w = c == kans.Winnaar ? 5 : 10;      // opgeslokte keten levert bonus op
            else if (eigen > 0)
                w = 2;                               // klein belang, weinig winst
            else
                w = 4;
            if (w > beste) beste = w;
        }
        return beste;
    }

    /// <summary>PROCbepaal_gunstigste_legsteen (regel 5200). -1 als er niets kan.</summary>
    public int KiesPlaats(int speler)
    {
        var punten = new List<(int Waarde, int Plaats)>();
        for (int s = 0; s < Regels.HandGrootte; s++)
        {
            int t = _handen[speler, s];
            if (t == 0) { punten.Add((0, s)); continue; }
            var (x, y) = Regels.TegelPositie(t);
            var kans = Bekijk(x, y);
            int w = !kans.Mag ? 0 : kans.Aanliggend.Count == 0 ? 2 : Fusiewaarde(kans, speler);
            punten.Add((w, s));
        }

        punten.Sort((a, b) => a.Waarde != b.Waarde ? b.Waarde.CompareTo(a.Waarde)
                                                  : a.Plaats.CompareTo(b.Plaats));
        if (punten[0].Waarde == 0) return -1;

        // FIX: regel 5332 wilde voorkomen dat een al erg grote keten nog groter
        // gemaakt wordt, maar gebruikte het handnummer als ketennummer.
        for (int i = 0; i < punten.Count; i++)
        {
            var (waarde, plaats) = punten[i];
            if (waarde == 0) break;
            var (x, y) = Regels.TegelPositie(_handen[speler, plaats]);
            var kans = Bekijk(x, y);
            bool restWaarde = punten.Skip(i + 1).Any(q => q.Waarde > 0);
            if (kans.Winnaar > 0 && _groottes[kans.Winnaar] > 15 && waarde < 9 && restWaarde)
                continue;
            return plaats;
        }
        return punten[0].Plaats;
    }

    /// <summary>
    /// PROCkopen (regel 9180): koop daar waar je positie bedreigd wordt.
    ///
    /// FIX: de rangordebewerking zat in het origineel in een decimaal ingepakt
    /// getal, en regel 9275 had een operatorvolgorde-fout waardoor ook
    /// niet-bestaande ketens gekocht konden worden.
    /// </summary>
    public int KiesAankoop(int speler)
    {
        var bestaand = BestaandeKetens();
        if (bestaand.Count == 0) return 0;

        var kandidaten = new List<int>();
        foreach (int c in bestaand)
        {
            int[] rang = Rangorde(c);
            var bezit = rang.Select(q => Spelers[q].Aandelen[c]).ToArray();
            for (int n = 0; n < 3; n++)
            {
                if (rang[n] != speler) continue;
                if (bezit[n + 1] + 4 > bezit[n]) kandidaten.Add(c);            // iemand zit vlak achter me
                else if (n + 2 < Regels.AantalSpelers && bezit[n + 1] == bezit[n + 2]) kandidaten.Add(c);
                break;
            }
            if (kandidaten.Count >= 4) break;
        }

        foreach (int c in kandidaten)
        {
            int[] rang = Rangorde(c);
            var bezit = rang.Select(q => Spelers[q].Aandelen[c]).ToArray();
            if (rang[0] == speler && bezit[0] - bezit[1] > 4) continue;        // onbedreigd eerste
            return c;
        }
        return bestaand[_toeval.Next(bestaand.Count)];
    }

    /// <summary>PROCcomp (regel 3582): de volledige beurt van een computerspeler.</summary>
    public List<string> ComputerBeurt(int speler)
    {
        var p = Spelers[speler];
        var regels = new List<string>();

        bool gesticht = false;
        var vrij = StichtbarePlaatsen(speler);
        if (vrij.Count > 0)
        {
            var keuze = TeStichten();
            int keten = keuze[_toeval.Next(keuze.Count)];
            var r = Sticht(speler, vrij[0], keten);
            regels.Add(Taal.StichtKeten(p.Naam, Regels.KetenLetter(keten),
                                        Regels.Naam(r.X, r.Y)));
            gesticht = true;
        }

        if (!gesticht)
        {
            int plaats = KiesPlaats(speler);
            if (plaats < 0)
            {
                Past();
                regels.Add(Taal.KanNiet(p.Naam));
                return regels;
            }
            regels.AddRange(Beschrijf(p.Naam, Leg(speler, plaats)));
        }

        int koop = KiesAankoop(speler);
        int prijs = Prijs(koop);
        int n = Koop(speler, koop, 3);
        regels.Add(n > 0
            ? Taal.Koopt(p.Naam, n, Regels.KetenLetter(koop), prijs)
            : Taal.KooptNiets(p.Naam));
        return regels;
    }

    /// <summary>Een zetverslag omzetten naar leesbare regels voor het meldingenvak.</summary>
    public List<string> Beschrijf(string naam, Zetverslag r)
    {
        string plek = Regels.Naam(r.X, r.Y);
        var uit = new List<string>
        {
            r.Keten == 0
                ? Taal.LegtLos(naam, plek)
                : Taal.LegtBij(naam, plek, Regels.KetenLetter(r.Keten)),
        };
        foreach (var u in r.Uitkeringen)
        {
            uit.Add(Taal.GaatOpIn(Regels.KetenLetter(u.Keten), u.Grootte,
                                  Regels.KetenLetter(u.Naar), u.Pot));
            foreach (var b in u.Bedragen.OrderBy(b => b.Key))
                uit.Add(Taal.Uitkering(Spelers[b.Key].Naam, b.Value));
        }
        return uit;
    }

    // -------------------------------------------------------------- beurtloop

    /// <summary>
    /// PROCspelen (regel 4000). Levert de speler die aan de beurt is, of 0 als
    /// het spel afgelopen is.
    ///
    /// FIX: de test "iedereen kon niet" stond in het origineel boven in de lus
    /// en werd elke ronde teruggezet voordat hij ooit 6 kon bereiken.
    /// </summary>
    public int VolgendeSpeler()
    {
        if (Klaar) return 0;

        if (AanDeBeurt == 0) { AanDeBeurt = 1; _rondePassen = 0; }
        else if (AanDeBeurt >= Regels.AantalSpelers)
        {
            if (_rondePassen >= Regels.AantalSpelers)
                return Beeindig(Taal.NiemandKanNog);
            AanDeBeurt = 1; _rondePassen = 0;
        }
        else AanDeBeurt++;

        Beurt++;
        if (Beurt >= Regels.MaxBeurten) return Beeindig(Taal.BeurtenGespeeld(Regels.MaxBeurten));
        for (int c = 1; c <= Regels.AantalKetens; c++)
            if (_groottes[c] > Regels.EindKetenGrootte)
                return Beeindig(Taal.KetenTeGroot(Regels.KetenLetter(c),
                                                 Regels.EindKetenGrootte));
        return AanDeBeurt;
    }

    private int Beeindig(string reden)
    {
        Klaar = true;
        Eindreden = reden;
        return 0;
    }

    /// <summary>De eindstand, van hoog naar laag vermogen.</summary>
    public List<(int Nummer, string Naam, int Vermogen)> Eindstand() =>
        Enumerable.Range(1, Regels.AantalSpelers)
                  .Select(i => (i, Spelers[i].Naam, Vermogen(i)))
                  .OrderByDescending(r => r.Item3)
                  .ToList();
}
