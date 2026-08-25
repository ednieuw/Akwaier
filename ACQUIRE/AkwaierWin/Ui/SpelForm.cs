using System.Drawing;
using Akwaier.Engine;

namespace Akwaier.Ui;

/// <summary>Een paneel dat zonder flikkeren tekent.</summary>
internal sealed class Tekenvlak : Panel
{
    public Tekenvlak()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
    }
}

/// <summary>Het spelscherm: bord, gegevenspaneel, hand, knoppen en meldingen.</summary>
public sealed class SpelForm : Form
{
    private const int TekenVak = 33;        // vakgrootte bij 96 dpi
    private const int TekenMarge = 22;      // rand met de letters en cijfers
    private const int InfoBreed = 372;

    private Spel _spel;
    private Thema _thema = Thema.Modern;
    private float _s = 1f;
    private int _vak, _marge;

    private string _fase = "rust";
    private int _gekozenPlaats = -1;

    private Tekenvlak _bord, _info, _hand;
    private FlowLayoutPanel _actie;
    private Label _bericht, _beurtLabel;
    private TextBox _log;
    private Button _knopNieuw, _knopScores, _knopWeergave;
    private CheckBox _knopSnel;
    private readonly System.Windows.Forms.Timer _klok = new();
    private Action _straks;

    private Font _fGewoon, _fVet, _fKlein, _fVak, _fTitel;

    public SpelForm()
    {
        Text = Taal.Titel;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AutoScaleMode = AutoScaleMode.None;
        StartPosition = FormStartPosition.CenterScreen;
        // Het pictogram zit als Win32-hulpbron in de exe (ApplicationIcon).
        try { Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath); }
        catch (Exception) { /* zonder pictogram is ook prima */ }

        _klok.Tick += (_, _) => { _klok.Stop(); var a = _straks; _straks = null; a?.Invoke(); };

        BouwOp();
        PasThemaToe();
        Shown += (_, _) => NieuwSpel();
    }

    private int P(double v) => (int)Math.Round(v * _s);

    private int Wacht(int gewoon) => _knopSnel.Checked ? Math.Max(15, gewoon / 8) : gewoon;

    private void Straks(int ms, Action doen) { _straks = doen; _klok.Interval = Math.Max(1, ms); _klok.Start(); }

    // --------------------------------------------------------------- opbouw

    private void BouwOp()
    {
        _s = DeviceDpi / 96f;
        _vak = P(TekenVak);
        _marge = P(TekenMarge);
        int zijde = 2 * _marge + 15 * _vak;
        int rand = P(8);
        int info = P(InfoBreed);

        _knopNieuw = new Button { Text = Taal.NieuwSpel, AutoSize = true };
        _knopNieuw.Click += (_, _) => NieuwSpel();
        _knopScores = new Button { Text = Taal.Scorelijst, AutoSize = true };
        _knopScores.Click += (_, _) => ToonScores();
        _knopSnel = new CheckBox { Text = Taal.SnelSpelen, AutoSize = true };
        _knopWeergave = new Button { Text = Taal.Weergave(Thema.Modern.Naam), AutoSize = true };
        _knopWeergave.Click += (_, _) => WisselWeergave();
        _beurtLabel = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleRight };

        _bord = new Tekenvlak();
        _bord.Paint += TekenBord;
        _bord.MouseClick += BordAangeklikt;
        _info = new Tekenvlak();
        _info.Paint += TekenInfo;
        _hand = new Tekenvlak();
        _hand.Paint += TekenHand;
        _hand.MouseClick += HandAangeklikt;

        _actie = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        _bericht = new Label { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        _log = new TextBox
        {
            Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.None, TabStop = false,
        };

        int y = rand;
        _knopNieuw.Location = new Point(rand, y);
        Controls.Add(_knopNieuw);
        _knopScores.Location = new Point(rand, y);       // wordt hieronder verschoven
        Controls.Add(_knopScores);
        _knopSnel.Location = new Point(rand, y + P(4));
        Controls.Add(_knopSnel);
        Controls.Add(_knopWeergave);
        Controls.Add(_beurtLabel);

        int bordY = y + P(38);
        _bord.SetBounds(rand, bordY, zijde, zijde);
        _info.SetBounds(rand + zijde + rand, bordY, info, zijde);
        Controls.Add(_bord);
        Controls.Add(_info);

        int handY = bordY + zijde + rand;
        _hand.SetBounds(rand, handY, zijde, P(64));
        Controls.Add(_hand);

        int actieY = handY + P(72);
        _actie.SetBounds(rand, actieY, zijde, P(34));
        Controls.Add(_actie);

        int berichtY = actieY + P(38);
        _bericht.SetBounds(rand, berichtY, zijde, P(22));
        Controls.Add(_bericht);

        ClientSize = new Size(rand + zijde + rand + info + rand, berichtY + P(22) + rand);
        _log.SetBounds(rand + zijde + rand, handY, info, ClientSize.Height - handY - rand);
        Controls.Add(_log);

        // de knoppen rechtsboven pas plaatsen nu de breedte bekend is
        _knopNieuw.Location = new Point(rand, y);
        _knopScores.Location = new Point(rand + _knopNieuw.Width + P(6), y);
        _knopSnel.Location = new Point(_knopScores.Right + P(14), y + P(4));
        _knopWeergave.Location = new Point(ClientSize.Width - rand - _knopWeergave.Width, y);
        _beurtLabel.Location = new Point(_knopWeergave.Left - P(16) - _beurtLabel.Width, y + P(4));
    }

    /// <summary>
    /// Maakt de lettertypen van de gekozen weergave. De oude worden pas
    /// weggegooid als geen enkel besturingselement ze meer gebruikt; anders
    /// loopt de eerstvolgende tekenbeurt vast op een opgeruimd lettertype.
    /// </summary>
    private Font[] MaakLettertypen()
    {
        var oud = new[] { _fGewoon, _fVet, _fKlein, _fVak, _fTitel };
        string f = _thema.Lettertype;
        _fGewoon = new Font(f, 9f);
        _fVet = new Font(f, 9f, FontStyle.Bold);
        _fKlein = new Font(f, 7.5f);
        _fVak = new Font(f, 12f, FontStyle.Bold);
        _fTitel = new Font(f, 11f, FontStyle.Bold);
        return oud;
    }

    // ---------------------------------------------------------------- skins

    private void WisselWeergave()
    {
        _thema = _thema == Thema.Modern ? Thema.Retro : Thema.Modern;
        PasThemaToe();
    }

    private void PasThemaToe()
    {
        Font[] oudeLetters = MaakLettertypen();
        var t = _thema;
        BackColor = t.Achtergrond;
        Font = _fGewoon;

        foreach (var knop in new[] { _knopNieuw, _knopScores, _knopWeergave }) KnopKleur(knop);
        _knopSnel.BackColor = t.Achtergrond;
        _knopSnel.ForeColor = t.Voorgrond;
        // Standard tekent een net vinkje en houdt zich wel aan ForeColor; bij
        // System zet Windows de tekst in de systeemkleur en valt die weg op zwart.
        _knopSnel.FlatStyle = FlatStyle.Standard;
        _knopSnel.Font = _fGewoon;
        _beurtLabel.BackColor = t.Achtergrond;
        _beurtLabel.ForeColor = t.Voorgrond;
        _beurtLabel.Font = _fTitel;
        _bericht.BackColor = t.Achtergrond;
        _bericht.ForeColor = t.Gedempt;
        _bericht.Font = _fGewoon;
        _actie.BackColor = t.Achtergrond;
        _log.BackColor = t.LogAchter;
        _log.ForeColor = t.LogVoor;
        _log.Font = _fKlein;
        _bord.BackColor = t.Achtergrond;
        _info.BackColor = t.Achtergrond;
        _hand.BackColor = t.Achtergrond;

        _knopWeergave.Text = Taal.Weergave(t.Naam);
        foreach (Control c in _actie.Controls)
        {
            switch (c)
            {
                case Button b when b.Tag is int keten:      // knop met een ketenkleur
                    b.Font = _fVet;
                    b.BackColor = t.KetenAchter[keten];
                    b.ForeColor = t.KetenVoor[keten];
                    b.FlatAppearance.BorderColor = t.Raster;
                    b.FlatAppearance.MouseOverBackColor = t.KetenAchter[keten];
                    break;
                case Button b:
                    KnopKleur(b);
                    break;
                case Label l:
                    l.Font = _fGewoon;
                    l.BackColor = t.Achtergrond;
                    l.ForeColor = t.Voorgrond;
                    break;
            }
        }

        HerplaatsKop();
        Herteken();
        foreach (var f in oudeLetters) f?.Dispose();
    }

    private void KnopKleur(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = _thema.KnopZweef;
        b.BackColor = _thema.Knop;
        b.ForeColor = _thema.KnopVoor;
        b.Font = _fGewoon;
        b.UseVisualStyleBackColor = false;
    }

    /// <summary>
    /// De vaste opschriften opnieuw zetten nadat er in het startvenster een
    /// andere taal is gekozen. Het bord en het gegevenspaneel halen hun tekst
    /// bij elke tekenbeurt op en hoeven hier niets te doen.
    /// </summary>
    private void PasTaalToe()
    {
        Text = Taal.Titel;
        _knopNieuw.Text = Taal.NieuwSpel;
        _knopScores.Text = Taal.Scorelijst;
        _knopSnel.Text = Taal.SnelSpelen;
        _knopWeergave.Text = Taal.Weergave(_thema.Naam);
        HerplaatsKop();
    }

    private void HerplaatsKop()
    {
        int rand = P(8), y = P(8);
        _knopScores.Location = new Point(rand + _knopNieuw.Width + P(6), y);
        _knopSnel.Location = new Point(_knopScores.Right + P(14), y + P(4));
        _knopWeergave.Location = new Point(ClientSize.Width - rand - _knopWeergave.Width, y);
        _beurtLabel.Location = new Point(_knopWeergave.Left - P(16) - _beurtLabel.Width, y + P(5));
    }

    private void Herteken()
    {
        _bord.Invalidate();
        _info.Invalidate();
        _hand.Invalidate();
        if (_spel != null)
        {
            _beurtLabel.Text = _spel.Klaar
                ? Taal.SpelAfgelopen
                : _spel.AanDeBeurt > 0
                    ? Taal.BeurtVan(_spel.Beurt, Regels.MaxBeurten,
                                    _spel.Spelers[_spel.AanDeBeurt].Naam)
                    : "";
            HerplaatsKop();
        }
    }

    // ----------------------------------------------------------- nieuw spel

    private void NieuwSpel()
    {
        using var dlg = new NieuwSpelForm(_thema, _fGewoon, _fTitel, _fKlein);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        PasTaalToe();
        _spel = new Spel(dlg.Plaats, dlg.SpelerNaam);
        _fase = "rust";
        _gekozenPlaats = -1;
        _log.Clear();
        if (dlg.Plaats > 0)
        {
            Meld(Taal.JeSpeeltAls(dlg.Plaats, _spel.Spelers[dlg.Plaats].Naam));
            Meld(Taal.KiesEenSteenKort);
        }
        else Meld(Taal.Kijkmodus);
        Meld(new string('-', 44));
        Herteken();
        Straks(300, Stap);
    }

    // -------------------------------------------------------------- beurten

    private void Stap()
    {
        if (_spel == null) return;
        int p = _spel.VolgendeSpeler();
        if (p == 0) { Afgelopen(); return; }
        WisKnoppen();
        Herteken();
        if (_spel.Spelers[p].Mens) BeginMensBeurt();
        else Straks(Wacht(320), () => ComputerZet(p));
    }

    private void ComputerZet(int p)
    {
        foreach (string r in _spel.ComputerBeurt(p)) Meld(r);
        Herteken();
        Straks(Wacht(300), Stap);
    }

    private void BeginMensBeurt()
    {
        _gekozenPlaats = -1;
        _fase = "leggen";
        var geldig = _spel.GeldigePlaatsen(_spel.AanDeBeurt);
        Herteken();
        WisKnoppen();
        if (geldig.Count == 0)
        {
            _bericht.Text = Taal.GeenSteenKwijt;
            _fase = "pas";
            Knop(Taal.PasDezeBeurt, () =>
            {
                _spel.Past();
                Meld(Taal.KanNiet(_spel.Spelers[_spel.AanDeBeurt].Naam));
                WisKnoppen();
                Stap();
            });
            return;
        }
        _bericht.Text = Taal.KiesEenSteen;
    }

    private void KiesPlaats(int plaats)
    {
        if (_fase != "leggen" || plaats < 0 || plaats >= Regels.HandGrootte) return;
        int speler = _spel.AanDeBeurt;
        int steen = _spel.Hand(speler, plaats);
        if (steen == 0) { _bericht.Text = Taal.HandplaatsLeeg; return; }

        _gekozenPlaats = plaats;
        var (x, y) = Regels.TegelPositie(steen);
        var kans = _spel.Bekijk(x, y);

        WisKnoppen();
        Tekstje(Taal.SteenIs(plaats + 1, Regels.Naam(x, y)));
        if (kans.Mag)
        {
            string tekst = kans.Aanliggend.Count == 0
                ? Taal.LegLos(Regels.Naam(x, y))
                : Taal.LegBij(Regels.Naam(x, y), Regels.KetenLetter(kans.Winnaar));
            Knop(tekst, () => LegNeer(plaats, 0));
        }
        else Tekstje(Taal.MagHierNiet);

        if (_spel.KanStichten(x, y) && _spel.TeStichten().Count > 0)
            Knop(Taal.StichtOp(Regels.Naam(x, y)), () => VraagStichtKleur(plaats));
        Knop(Taal.Annuleer, BeginMensBeurt);
        Herteken();
    }

    private void VraagStichtKleur(int plaats)
    {
        _fase = "stichten";
        WisKnoppen();
        Tekstje(Taal.WelkeKetenStichten);
        foreach (int c in _spel.TeStichten())
        {
            int keten = c;
            KetenKnop(keten, "", () =>
            {
                var r = _spel.Sticht(_spel.AanDeBeurt, plaats, keten);
                Meld(Taal.StichtKetenMetAandeel(_spel.Spelers[_spel.AanDeBeurt].Naam,
                                                Regels.KetenLetter(keten),
                                                Regels.Naam(r.X, r.Y)));
                BeginKoopfase();
            });
        }
        Knop(Taal.Annuleer, BeginMensBeurt);
    }

    private void LegNeer(int plaats, int gekozenWinnaar)
    {
        var (x, y) = Regels.TegelPositie(_spel.Hand(_spel.AanDeBeurt, plaats));
        var kans = _spel.Bekijk(x, y);
        if (kans.Gelijkspel && gekozenWinnaar == 0)
        {
            _fase = "gelijk";
            WisKnoppen();
            Tekstje(Taal.WelkeBlijft);
            foreach (int c in kans.Gelijken)
            {
                int keten = c;
                KetenKnop(keten, "", () => LegNeer(plaats, keten));
            }
            return;
        }
        var verslag = _spel.Leg(_spel.AanDeBeurt, plaats, gekozenWinnaar);
        foreach (string r in _spel.Beschrijf(_spel.Spelers[_spel.AanDeBeurt].Naam, verslag)) Meld(r);
        BeginKoopfase();
    }

    // ---------------------------------------------------------------- kopen

    private void BeginKoopfase()
    {
        _gekozenPlaats = -1;
        _fase = "kopen";
        Herteken();
        WisKnoppen();

        int geld = _spel.Spelers[_spel.AanDeBeurt].Geld;
        var ketens = _spel.BestaandeKetens().Where(c => _spel.Prijs(c) <= geld).ToList();
        if (ketens.Count == 0)
        {
            _bericht.Text = Taal.NietsTeKopen;
            Knop(Taal.Verder, RondBeurtAf);
            return;
        }
        _bericht.Text = Taal.KoopHoogstensDrie;
        Tekstje(Taal.KopenDubbelepunt);
        foreach (int c in ketens)
        {
            int keten = c;
            KetenKnop(keten, $" {_spel.Prijs(keten)}", () => VraagAantal(keten));
        }
        Knop(Taal.KoopNiets, RondBeurtAf);
    }

    private void VraagAantal(int keten)
    {
        int prijs = _spel.Prijs(keten);
        int geld = _spel.Spelers[_spel.AanDeBeurt].Geld;
        WisKnoppen();
        Tekstje(Taal.HoeveelVan(Regels.KetenLetter(keten), prijs));
        for (int n = 1; n <= 3; n++)
        {
            if (prijs * n > geld) continue;
            int aantal = n;
            Knop(aantal.ToString(), () =>
            {
                int gekocht = _spel.Koop(_spel.AanDeBeurt, keten, aantal);
                if (gekocht > 0)
                    Meld(Taal.Koopt(_spel.Spelers[_spel.AanDeBeurt].Naam, gekocht,
                                    Regels.KetenLetter(keten), prijs));
                RondBeurtAf();
            });
        }
        Knop(Taal.Terug, BeginKoopfase);
    }

    private void RondBeurtAf()
    {
        WisKnoppen();
        _bericht.Text = "";
        Herteken();
        Straks(Wacht(200), Stap);
    }

    // ----------------------------------------------------------------- einde

    private void Afgelopen()
    {
        _fase = "klaar";
        WisKnoppen();
        Herteken();
        Meld(new string('=', 44));
        Meld(Taal.Einde(_spel.Eindreden));
        int plek = 1;
        foreach (var (_, naam, vermogen) in _spel.Eindstand())
            Meld($"{plek++}. {naam,-8} {vermogen}");
        Scorelijst.Bijwerken(Enumerable.Range(1, Regels.AantalSpelers)
            .Select(i => (_spel.Spelers[i].Naam, _spel.Vermogen(i))));
        _bericht.Text = Taal.SpelAfgelopenMet(_spel.Eindreden);
        Knop(Taal.Scorelijst, ToonScores);
        Knop(Taal.NieuwSpel, NieuwSpel);
    }

    private void ToonScores()
    {
        using var dlg = new ScoreForm(_thema, _fGewoon, _fVet);
        dlg.ShowDialog(this);
    }

    // --------------------------------------------------------- knoppenstrook

    private void WisKnoppen()
    {
        foreach (Control c in _actie.Controls.Cast<Control>().ToList()) { _actie.Controls.Remove(c); c.Dispose(); }
    }

    private void Tekstje(string tekst)
    {
        var l = new Label
        {
            Text = tekst, AutoSize = true, ForeColor = _thema.Voorgrond,
            BackColor = _thema.Achtergrond, Font = _fGewoon,
            Margin = new Padding(0, P(6), P(8), 0),
        };
        _actie.Controls.Add(l);
    }

    private void Knop(string tekst, Action doen)
    {
        var b = new Button { Text = tekst, AutoSize = true, Margin = new Padding(P(3), P(2), P(3), 0) };
        b.Click += (_, _) => doen();
        KnopKleur(b);
        _actie.Controls.Add(b);
    }

    private void KetenKnop(int keten, string extra, Action doen)
    {
        var b = new Button
        {
            Text = Regels.KetenLetter(keten) + extra,
            AutoSize = true,
            Margin = new Padding(P(2), P(2), P(2), 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = _thema.KetenAchter[keten],
            ForeColor = _thema.KetenVoor[keten],
            Font = _fVet,
            UseVisualStyleBackColor = false,
            Tag = keten,
        };
        b.FlatAppearance.BorderSize = 1;
        b.FlatAppearance.BorderColor = _thema.Raster;
        b.FlatAppearance.MouseOverBackColor = _thema.KetenAchter[keten];
        b.Click += (_, _) => doen();
        _actie.Controls.Add(b);
    }

    private void Meld(string tekst)
    {
        _log.AppendText(tekst + Environment.NewLine);
        _log.SelectionStart = _log.TextLength;
        _log.ScrollToCaret();
    }

    // --------------------------------------------------------------- tekenen

    private const TextFormatFlags Midden =
        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
    private const TextFormatFlags Links =
        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
    private const TextFormatFlags Rechts =
        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

    private void TekenBord(object zender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var t = _thema;
        g.Clear(t.Achtergrond);
        if (_spel == null) return;

        int mens = _spel.MensPlaats;
        var eigen = new Dictionary<(int, int), int>();
        if (mens > 0 && _spel.AanDeBeurt == mens && !_spel.Klaar)
            for (int s = 0; s < Regels.HandGrootte; s++)
            {
                int steen = _spel.Hand(mens, s);
                if (steen != 0) eigen[Regels.TegelPositie(steen)] = s;
            }

        // randlabels: kolommen boven en onder, rijletters links en rechts
        for (int i = 1; i <= Regels.Grootte; i++)
        {
            string kop = t.BbcKolomkop ? (i % 10).ToString() : i.ToString();
            int px = _marge + (i - 1) * _vak;
            TekstIn(g, kop, _fKlein, new Rectangle(px, 0, _vak, _marge), t.Gedempt, Midden);
            TekstIn(g, kop, _fKlein, new Rectangle(px, _marge + 15 * _vak, _vak, _marge), t.Gedempt, Midden);
            string rij = ((char)('A' + i - 1)).ToString();
            int py = _marge + (i - 1) * _vak;
            TekstIn(g, rij, _fKlein, new Rectangle(0, py, _marge, _vak), t.Gedempt, Midden);
            TekstIn(g, rij, _fKlein, new Rectangle(_marge + 15 * _vak, py, _marge, _vak), t.Gedempt, Midden);
        }

        using var raster = new Pen(t.Raster);
        for (int x = 1; x <= Regels.Grootte; x++)
            for (int y = 1; y <= Regels.Grootte; y++)
            {
                int x0 = _marge + (x - 1) * _vak, y0 = _marge + (y - 1) * _vak;
                var vak = new Rectangle(x0 + 1, y0 + 1, _vak - 2, _vak - 2);
                int v = _spel.Vak(x, y);
                bool isEigen = eigen.TryGetValue((x, y), out int plaats);
                bool mag = isEigen && _spel.Bekijk(x, y).Mag;

                Color achter = v >= 1 && v <= 8 ? t.KetenAchter[v]
                             : v == Regels.Los ? t.Band
                             : mag ? t.Wenk : t.LeegVak;
                using (var kwast = new SolidBrush(achter)) g.FillRectangle(kwast, vak);
                g.DrawRectangle(raster, vak);

                if (v >= 1 && v <= 8)
                    TekstIn(g, Regels.KetenLetter(v).ToString(), _fVak, vak, t.KetenVoor[v], Midden);
                else if (v == Regels.Los)
                {
                    int in8 = Math.Max(2, _vak / 4);
                    using var kwast = new SolidBrush(t.Los);
                    g.FillRectangle(kwast, x0 + in8, y0 + in8, _vak - 2 * in8, _vak - 2 * in8);
                }
                else if (!isEigen)
                {
                    int r = Math.Max(1, _vak / 14);
                    using var kwast = new SolidBrush(t.Stip);
                    g.FillEllipse(kwast, x0 + _vak / 2 - r, y0 + _vak / 2 - r, 2 * r, 2 * r);
                }

                if (isEigen)
                {
                    // net als regel 3571: de handnummers staan op het bord
                    TekstIn(g, (plaats + 1).ToString(), _fVet, vak, mag ? t.Voorgrond : t.Gedempt, Midden);
                    if (plaats == _gekozenPlaats)
                    {
                        using var dik = new Pen(t.Keuze, Math.Max(2, P(3)));
                        g.DrawRectangle(dik, x0 + 2, y0 + 2, _vak - 5, _vak - 5);
                    }
                }
            }
    }

    private void TekenHand(object zender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var t = _thema;
        g.Clear(t.Achtergrond);
        if (_spel == null) return;

        int mens = _spel.MensPlaats;
        if (mens == 0)
        {
            TekstIn(g, Taal.KijkmodusGeenStenen, _fGewoon,
                    new Rectangle(P(4), 0, P(400), _hand.Height), t.Gedempt, Links);
            return;
        }
        TekstIn(g, Taal.StenenVan(_spel.Spelers[mens].Naam), _fKlein,
                new Rectangle(P(4), 0, P(300), P(18)), t.Gedempt, Links);

        var geldig = _spel.AanDeBeurt == mens
            ? new HashSet<int>(_spel.GeldigePlaatsen(mens))
            : new HashSet<int>();

        for (int s = 0; s < Regels.HandGrootte; s++)
        {
            int x0 = P(4) + s * P(66), y0 = P(20);
            var vak = new Rectangle(x0, y0, P(60), P(38));
            int steen = _spel.Hand(mens, s);
            if (steen == 0)
            {
                using var stippel = new Pen(t.Raster) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
                using var leeg = new SolidBrush(t.LeegVak);
                g.FillRectangle(leeg, vak);
                g.DrawRectangle(stippel, vak);
                continue;
            }
            bool kan = geldig.Contains(s);
            using (var kwast = new SolidBrush(kan ? t.Band : t.LeegVak)) g.FillRectangle(kwast, vak);
            using (var pen = new Pen(s == _gekozenPlaats ? t.Keuze : t.Raster,
                                     s == _gekozenPlaats ? Math.Max(2, P(3)) : 1))
                g.DrawRectangle(pen, vak);
            TekstIn(g, (s + 1).ToString(), _fKlein,
                    new Rectangle(x0 + P(6), y0 + P(2), P(16), P(14)), t.Gedempt, Links);
            var (tx, ty) = Regels.TegelPositie(steen);
            TekstIn(g, Regels.Naam(tx, ty), _fVet, vak, kan ? t.Voorgrond : t.Gedempt, Midden);
        }
    }

    private void TekenInfo(object zender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var t = _thema;
        g.Clear(t.Achtergrond);
        if (_spel == null) return;
        int w = _info.Width;

        int Band(int y, params (double X, TextFormatFlags Vlag, string Tekst)[] kolommen)
        {
            using (var kwast = new SolidBrush(t.KopAchter)) g.FillRectangle(kwast, 0, y, w, P(20));
            foreach (var (x, vlag, tekst) in kolommen)
                TekstIn(g, tekst, _fVet, Vlak(x, y, vlag), t.KopVoor, vlag);
            return y + P(22);
        }

        int ry = Band(0, (6, Links, Taal.KopKeten), (150, Rechts, Taal.KopPrijs),
                         (230, Rechts, Taal.KopTegels), (250, Links, Taal.KopStatus));
        for (int c = 1; c <= Regels.AantalKetens; c++)
        {
            Vlagje(g, c, ry);
            int n = _spel.KetenGrootte(c);
            TekstIn(g, n > 0 ? _spel.Prijs(c).ToString() : "-", _fGewoon, Vlak(150, ry, Rechts), t.Voorgrond, Rechts);
            TekstIn(g, n > 0 ? n.ToString() : "-", _fGewoon, Vlak(230, ry, Rechts), t.Voorgrond, Rechts);
            if (n == 0) TekstIn(g, Taal.TeStichten, _fKlein, Vlak(250, ry, Links), t.Gedempt, Links);
            ry += P(19);
        }

        ry = Band(ry + P(6), (6, Links, Taal.KopAandelen));
        for (int i = 1; i <= Regels.AantalSpelers; i++)
            TekstIn(g, i.ToString(), _fKlein,
                    new Rectangle(P(48 + i * 44), ry, P(24), P(16)), t.Gedempt, Midden);
        ry += P(17);
        for (int c = 1; c <= Regels.AantalKetens; c++)
        {
            Vlagje(g, c, ry);
            int[] rang = _spel.Rangorde(c);
            for (int i = 1; i <= Regels.AantalSpelers; i++)
            {
                int n = _spel.Spelers[i].Aandelen[c];
                Color kleur = n == 0 ? t.Raster
                            : i == rang[0] ? t.Keuze
                            : i == rang[1] ? t.Gedempt : t.Voorgrond;
                var vet = n > 0 && (i == rang[0] || i == rang[1]) ? _fVet : _fGewoon;
                TekstIn(g, n > 0 ? n.ToString() : ".", vet,
                        new Rectangle(P(48 + i * 44), ry, P(24), P(18)), kleur, Midden);
            }
            ry += P(19);
        }

        ry = Band(ry + P(6), (8, Links, Taal.KopSpeler), (250, Rechts, Taal.KopGeld),
                             (360, Rechts, Taal.KopVermogen));
        for (int i = 1; i <= Regels.AantalSpelers; i++)
        {
            var p = _spel.Spelers[i];
            bool actief = i == _spel.AanDeBeurt && !_spel.Klaar;
            if (actief)
                using (var kwast = new SolidBrush(t.Band)) g.FillRectangle(kwast, 0, ry, w, P(18));
            string naam = $"{p.Nummer} {p.Naam}" + (p.Mens ? Taal.Jij : "");
            TekstIn(g, naam, actief ? _fVet : _fGewoon, Vlak(8, ry, Links), t.Voorgrond, Links);
            TekstIn(g, p.Geld.ToString(), _fGewoon, Vlak(250, ry, Rechts), t.Voorgrond, Rechts);
            TekstIn(g, _spel.Vermogen(i).ToString(), _fGewoon, Vlak(360, ry, Rechts), t.Voorgrond, Rechts);
            ry += P(19);
        }

        TekstIn(g, Taal.StenenInDeZak(_spel.Zak.Count), _fKlein,
                Vlak(8, ry + P(4), Links), t.Gedempt, Links);
    }

    /// <summary>Het gekleurde vierkantje met de ketenletter in het gegevenspaneel.</summary>
    private void Vlagje(System.Drawing.Graphics g, int keten, int y)
    {
        var vak = new Rectangle(P(6), y, P(24), P(18));
        using (var kwast = new SolidBrush(_thema.KetenAchter[keten])) g.FillRectangle(kwast, vak);
        using (var pen = new Pen(_thema.Raster)) g.DrawRectangle(pen, vak);
        TekstIn(g, Regels.KetenLetter(keten).ToString(), _fVet, vak, _thema.KetenVoor[keten], Midden);
    }

    /// <summary>Een tekstvak van 18 hoog dat links, rechts of midden uitlijnt op x.</summary>
    private Rectangle Vlak(double x, int y, TextFormatFlags vlag)
    {
        int px = P(x), breed = P(150);
        if (vlag == Rechts) return new Rectangle(px - breed, y, breed, P(18));
        return new Rectangle(px, y, breed, P(18));
    }

    private static void TekstIn(System.Drawing.Graphics g, string s, Font f, Rectangle r,
                                Color kleur, TextFormatFlags vlag)
        => TextRenderer.DrawText(g, s, f, r, kleur, vlag);

    // ----------------------------------------------------------------- muis

    private void BordAangeklikt(object zender, MouseEventArgs e)
    {
        if (_spel == null || _fase != "leggen" || _spel.MensPlaats == 0) return;
        int x = (e.X - _marge) / _vak + 1, y = (e.Y - _marge) / _vak + 1;
        if (e.X < _marge || e.Y < _marge || x < 1 || x > Regels.Grootte || y < 1 || y > Regels.Grootte) return;
        for (int s = 0; s < Regels.HandGrootte; s++)
        {
            int steen = _spel.Hand(_spel.AanDeBeurt, s);
            if (steen != 0 && Regels.TegelPositie(steen) == (x, y)) { KiesPlaats(s); return; }
        }
        _bericht.Text = Taal.NietInJeHand(Regels.Naam(x, y));
    }

    private void HandAangeklikt(object zender, MouseEventArgs e)
    {
        if (_fase != "leggen") return;
        int s = (e.X - P(4)) / P(66);
        if (s >= 0 && s < Regels.HandGrootte) KiesPlaats(s);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys toets)
    {
        if (_fase == "leggen" && toets >= Keys.D1 && toets <= Keys.D8)
        {
            KiesPlaats(toets - Keys.D1);
            return true;
        }
        return base.ProcessCmdKey(ref msg, toets);
    }
}
