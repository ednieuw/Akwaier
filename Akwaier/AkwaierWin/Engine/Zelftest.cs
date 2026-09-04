using System.Text;

namespace Akwaier.Engine;

/// <summary>
/// Rekent de regels na, zodat je bij een wijziging meteen ziet of er iets stuk
/// is gegaan. Starten met: Akwaier.exe zelftest [verslagbestand]
/// </summary>
public static class Zelftest
{
    private static StringBuilder _uit;
    private static int _fout;

    public static int Draai(string verslagPad)
    {
        _uit = new StringBuilder();
        _fout = 0;

        Prijsstaffel();
        Legaliteit();
        LosseSteen();
        EnkeleFusie();
        GelijkeKetens();
        DrievoudigeFusie();
        Stichten();
        Kopen();
        Verdeelsleutel();
        VolledigePartijen();

        Regel("");
        Regel(_fout == 0 ? "ALLES GOED" : $"{_fout} CONTROLE(S) MISLUKT");

        try { File.WriteAllText(verslagPad, _uit.ToString()); } catch (Exception) { }
        return _fout;
    }

    // ------------------------------------------------------------ hulpmiddelen

    private static void Regel(string s) => _uit.AppendLine(s);

    private static void Kop(string s) { Regel(""); Regel(s); }

    private static void Eis(bool goed, string wat)
    {
        Regel($"  {(goed ? "ok  " : "FOUT")}: {wat}");
        if (!goed) _fout++;
    }

    /// <summary>Een leeg bord met zes schone spelers, om een stand na te bouwen.</summary>
    private static Spel Schoon(int zaad = 1)
    {
        var g = new Spel(0, "", zaad);
        for (int x = 1; x <= 15; x++)
            for (int y = 1; y <= 15; y++)
                Vul(g, x, y, Regels.Leeg);
        for (int q = 1; q <= 6; q++)
        {
            g.Spelers[q].Geld = Regels.Startgeld;
            for (int c = 1; c <= 8; c++) g.Spelers[q].Aandelen[c] = 0;
        }
        return g;
    }

    /// <summary>Een vakje rechtstreeks zetten; alleen voor het opbouwen van een proefstand.</summary>
    private static void Vul(Spel g, int x, int y, int waarde)
    {
        var veld = typeof(Spel).GetField("_bord",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var groottes = typeof(Spel).GetField("_groottes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var bord = (int[,])veld.GetValue(g);
        var maten = (int[])groottes.GetValue(g);
        int oud = bord[x, y];
        if (oud == waarde) return;
        if (oud >= 1) maten[oud]--;
        bord[x, y] = waarde;
        if (waarde >= 1) maten[waarde]++;
    }

    /// <summary>Een hand met de opgegeven vakjes klaarleggen.</summary>
    private static void Handvol(Spel g, int speler, params (int X, int Y)[] vakjes)
    {
        var veld = typeof(Spel).GetField("_handen",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var handen = (int[,])veld.GetValue(g);
        for (int s = 0; s < Regels.HandGrootte; s++) handen[speler, s] = 0;
        for (int s = 0; s < vakjes.Length; s++)
        {
            for (int n = 1; n <= 225; n++)
                if (Regels.TegelPositie(n) == vakjes[s]) { handen[speler, s] = n; break; }
        }
    }

    // -------------------------------------------------------------- de proeven

    private static void Prijsstaffel()
    {
        Kop("1 prijsstaffel");
        var g = Schoon();
        for (int x = 1; x <= 5; x++) Vul(g, x, 1, 3);         // keten C, vijf tegels
        Eis(g.Prijs(3) == 1300, $"5 tegels C kost 1300 (is {g.Prijs(3)})");
        g.Spelers[1].Aandelen[3] = 4;
        Eis(g.Prijs(3) == 1340, $"met 4 uitstaande aandelen 1340 (is {g.Prijs(3)})");
        Eis(g.Prijs(4) == 0, "een keten die niet bestaat kost niets");
    }

    private static void Legaliteit()
    {
        Kop("2 legaliteit");
        var g = Schoon();
        Vul(g, 5, 5, Regels.Los);
        Eis(!g.Bekijk(5, 6).Mag, "naast een losse steen leggen mag niet");
        Eis(g.Bekijk(8, 8).Mag, "op een leeg stuk bord leggen mag wel");
        Vul(g, 6, 6, 2);
        Eis(g.Bekijk(5, 6).Mag, "het mag zodra je ook keten B raakt");
        Eis(g.KanStichten(8, 8), "een vrij liggend vakje is stichtbaar");
        Eis(!g.KanStichten(5, 6), "een vakje met buren niet");
    }

    private static void LosseSteen()
    {
        Kop("3 losse steen wordt opgeslokt");
        var g = Schoon();
        Handvol(g, 1, (5, 6));
        Vul(g, 5, 5, Regels.Los);
        Vul(g, 6, 6, 2); Vul(g, 7, 6, 2);
        var r = g.Leg(1, 0);
        Eis(r.Keten == 2 && g.Vak(5, 5) == 2, "de losse steen gaat mee in keten B");
        Eis(g.KetenGrootte(2) == 4, $"B groeit van 2 naar 4 (is {g.KetenGrootte(2)})");
    }

    private static void EnkeleFusie()
    {
        Kop("4 fusie met bonus");
        var g = Schoon();
        Handvol(g, 1, (4, 1));
        for (int x = 1; x <= 3; x++) Vul(g, x, 1, 1);         // A: drie tegels
        for (int x = 5; x <= 10; x++) Vul(g, x, 1, 2);        // B: zes tegels
        g.Spelers[1].Aandelen[1] = 6;
        g.Spelers[2].Aandelen[1] = 3;
        g.Spelers[3].Aandelen[1] = 1;
        int prijsA = g.Prijs(1);

        var r = g.Leg(1, 0);
        var u = r.Uitkeringen[0];
        Eis(u.Keten == 1 && u.Naar == 2, "A gaat op in B");
        Eis(u.Pot == (3 + 10) * prijsA,
            $"pot = (3 tegels + 10 aandelen) x {prijsA} = {(3 + 10) * prijsA} (is {u.Pot})");
        Eis(u.Bedragen[1] == (int)Math.Round(2.0 / 3.0 * u.Pot, MidpointRounding.AwayFromZero),
            $"grootaandeelhouder krijgt twee derde ({u.Bedragen.GetValueOrDefault(1)})");
        Eis(u.Bedragen[2] == (int)Math.Round(1.0 / 3.0 * u.Pot, MidpointRounding.AwayFromZero),
            $"tweede krijgt een derde ({u.Bedragen.GetValueOrDefault(2)})");
        Eis(!u.Bedragen.ContainsKey(3), "de derde aandeelhouder krijgt niets");
        Eis(g.Spelers[1].Aandelen[2] == 3 && g.Spelers[2].Aandelen[2] == 1
            && g.Spelers[3].Aandelen[2] == 0, "aandelen half omgeruild: 6->3, 3->1, 1->0");
        Eis(g.Spelers[1].Aandelen[1] == 0, "de aandelen A zijn weg");
        Eis(g.KetenGrootte(2) == 10 && g.KetenGrootte(1) == 0, "B is nu 10 tegels, A bestaat niet meer");
        Eis(g.TeStichten().Contains(1), "A kan opnieuw gesticht worden");
    }

    private static void GelijkeKetens()
    {
        Kop("5 twee even grote ketens");
        var g = Schoon();
        Handvol(g, 1, (4, 1));
        for (int x = 1; x <= 3; x++) Vul(g, x, 1, 1);
        for (int x = 5; x <= 7; x++) Vul(g, x, 1, 2);
        var kans = g.Bekijk(4, 1);
        Eis(kans.Gelijkspel && kans.Gelijken.Count == 2, "het gelijkspel wordt gezien");
        var r = g.Leg(1, 0, 1);                              // de speler kiest A
        Eis(r.Keten == 1 && g.KetenGrootte(1) == 7 && g.KetenGrootte(2) == 0,
            "de keuze telt: A blijft en wordt 7 tegels");
    }

    private static void DrievoudigeFusie()
    {
        Kop("6 drievoudige fusie");
        var g = Schoon();
        Handvol(g, 1, (2, 2));
        Vul(g, 1, 2, 1); Vul(g, 3, 2, 2); Vul(g, 2, 1, 3);
        for (int x = 4; x <= 8; x++) Vul(g, x, 2, 2);
        g.Spelers[1].Aandelen[1] = 2; g.Spelers[1].Aandelen[3] = 2;
        var r = g.Leg(1, 0);
        Eis(r.Keten == 2 && r.Opgeslokt.Count == 2, "A en C gaan beide op in B");
        Eis(r.Uitkeringen.Count == 2, "er zijn twee uitkeringen");
        Eis(g.KetenGrootte(2) == 9 && g.KetenGrootte(1) == 0 && g.KetenGrootte(3) == 0,
            $"B is nu 9 tegels (is {g.KetenGrootte(2)})");
    }

    private static void Stichten()
    {
        Kop("7 stichten");
        var g = Schoon();
        Handvol(g, 1, (8, 8));
        g.Sticht(1, 0, 5);
        Eis(g.Vak(8, 8) == 5 && g.KetenGrootte(5) == 1, "keten E begint met één tegel");
        Eis(g.Spelers[1].Aandelen[5] == 1, "de stichter krijgt één gratis aandeel");
        Eis(!g.TeStichten().Contains(5), "E staat niet meer op de stichtlijst");
    }

    private static void Kopen()
    {
        Kop("8 kopen");
        var g = Schoon();
        for (int x = 1; x <= 5; x++) Vul(g, x, 1, 3);        // C kost 1300
        g.Spelers[1].Geld = 3000;
        int n = g.Koop(1, 3, 3);
        Eis(n == 2, $"drie gevraagd, twee betaalbaar (is {n})");
        Eis(g.Spelers[1].Geld == 400 && g.Spelers[1].Aandelen[3] == 2, "saldo en bezit kloppen");
        Eis(g.Koop(1, 3, 3) == 0, "met 400 over kan er niets meer bij");
        Eis(g.Koop(1, 4, 3) == 0, "een keten die niet bestaat levert niets op");
    }

    private static void Verdeelsleutel()
    {
        Kop("9 verdeelsleutel");
        var gevallen = new (int[] Bezit, double[] Verwacht)[]
        {
            (new[]{9,0,0,0,0,0}, new double[]{1,0,0,0,0,0}),
            (new[]{5,5,5,0,0,0}, new[]{1/3.0,1/3.0,1/3.0,0,0,0}),
            (new[]{5,5,2,0,0,0}, new double[]{.5,.5,0,0,0,0}),
            (new[]{9,2,2,2,2,2}, new[]{2/3.0,1/15.0,1/15.0,1/15.0,1/15.0,1/15.0}),
            (new[]{9,2,2,2,2,0}, new[]{2/3.0,1/12.0,1/12.0,1/12.0,1/12.0,0}),
            (new[]{9,2,2,2,0,0}, new[]{2/3.0,1/9.0,1/9.0,1/9.0,0,0}),
            (new[]{9,2,2,0,0,0}, new[]{2/3.0,1/6.0,1/6.0,0,0,0}),
            (new[]{9,4,1,0,0,0}, new[]{2/3.0,1/3.0,0,0,0,0}),
        };
        bool alles = true, telt = true;
        foreach (var (bezit, verwacht) in gevallen)
        {
            double[] got = Bonus.Verdeling(bezit);
            for (int i = 0; i < 6; i++) if (Math.Abs(got[i] - verwacht[i]) > 1e-9) alles = false;
            if (Math.Abs(got.Sum() - 1) > 1e-9) telt = false;
        }
        Eis(alles, "alle acht gevallen geven de juiste breuken");
        Eis(telt, "elke verdeling telt op tot 1");
    }

    private static void VolledigePartijen()
    {
        Kop("10 zestig volledig uitgespeelde partijen");
        int zetten = 0, fouten = 0;
        string eerste = "";
        for (int zaad = 0; zaad < 60; zaad++)
        {
            var g = new Spel(0, "", zaad);
            int n = 0;
            while (true)
            {
                int p = g.VolgendeSpeler();
                if (p == 0) break;
                g.ComputerBeurt(p);
                n++;
                string mis = Controleer(g);
                if (mis != null) { fouten++; if (eerste == "") eerste = $"zaad {zaad}, zet {n}: {mis}"; break; }
                if (n > 400) { fouten++; if (eerste == "") eerste = $"zaad {zaad}: loopt niet af"; break; }
            }
            zetten += n;
        }
        Eis(fouten == 0, fouten == 0
            ? $"na elke zet kloppen bord, ketens, aandelen, geld en de 225 stenen ({zetten} zetten)"
            : $"{fouten} partij(en) misgegaan, eerste: {eerste}");
    }

    /// <summary>Rekent na of de spelstand nog samenhangend is.</summary>
    private static string Controleer(Spel g)
    {
        var geteld = new int[9];
        var bezet = new HashSet<(int, int)>();
        for (int x = 1; x <= 15; x++)
            for (int y = 1; y <= 15; y++)
            {
                int v = g.Vak(x, y);
                if (v != Regels.Leeg && v != Regels.Los && (v < 1 || v > 8))
                    return $"vakje {Regels.Naam(x, y)} heeft waarde {v}";
                if (v >= 1) geteld[v]++;
                if (v != Regels.Leeg) bezet.Add((x, y));
            }
        for (int c = 1; c <= 8; c++)
            if (g.KetenGrootte(c) != geteld[c])
                return $"keten {Regels.KetenLetter(c)} telt {g.KetenGrootte(c)}, op het bord {geteld[c]}";

        for (int q = 1; q <= 6; q++)
        {
            if (g.Spelers[q].Geld < 0) return $"speler {q} staat rood";
            for (int c = 1; c <= 8; c++)
                if (g.Spelers[q].Aandelen[c] < 0) return $"speler {q} heeft negatieve aandelen";
        }

        var stenen = new HashSet<int>(g.Zak);
        for (int p = 1; p <= 6; p++)
            for (int s = 0; s < Regels.HandGrootte; s++)
            {
                int t = g.Hand(p, s);
                if (t == 0) continue;
                if (!stenen.Add(t)) return $"steen {t} komt dubbel voor";
                if (bezet.Contains(Regels.TegelPositie(t)))
                    return $"steen {t} wijst naar een bezet vakje";
            }
        if (stenen.Count + bezet.Count != 225)
            return $"{stenen.Count} losse + {bezet.Count} gelegde stenen is geen 225";
        return null;
    }
}
