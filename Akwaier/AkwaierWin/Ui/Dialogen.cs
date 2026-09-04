using System.Drawing;
using Akwaier.Engine;

namespace Akwaier.Ui;

/// <summary>Vervangt "DOE JE MEE MENS ?" uit regel 2009 van het origineel.</summary>
public sealed class NieuwSpelForm : Form
{
    private readonly TextBox _naam;
    private readonly ComboBox _plaats;
    private readonly ComboBox _taal;
    private readonly Label _kop, _onder, _lNaam, _lPlaats, _lTaal;
    private readonly Button _mee, _kijk;
    private readonly bool _taalBijBinnenkomst;

    public string SpelerNaam => _naam.Text;

    /// <summary>De plaats van de mens aan tafel; 0 betekent alleen kijken.</summary>
    public int Plaats { get; private set; }

    public NieuwSpelForm(Thema thema, Font gewoon, Font titel, Font klein)
    {
        _taalBijBinnenkomst = Taal.Engels;
        Text = Taal.NieuwSpel;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = MaximizeBox = false;
        ShowInTaskbar = false;
        BackColor = thema.Achtergrond;
        Font = gewoon;
        // Zonder dit schaalt Windows de knoppen nog eens bovenop de eigen
        // schaling hieronder, en valt de onderste rij buiten het venster.
        AutoScaleMode = AutoScaleMode.None;

        float s = DeviceDpi / 96f;
        int P(double v) => (int)Math.Round(v * s);

        int veldX = P(96), veldBreed = P(140);

        _kop = new Label
        {
            AutoSize = true, Font = titel,
            ForeColor = thema.Voorgrond, BackColor = thema.Achtergrond,
            Location = new Point(P(20), P(16)),
        };
        _onder = new Label
        {
            AutoSize = true, Font = klein,
            ForeColor = thema.Gedempt, BackColor = thema.Achtergrond,
            Location = new Point(P(20), P(42)),
        };
        _lNaam = new Label
        {
            AutoSize = true, ForeColor = thema.Voorgrond,
            BackColor = thema.Achtergrond, Location = new Point(P(20), P(74)),
        };
        _naam = new TextBox { Text = "ED", Location = new Point(veldX, P(71)), Width = veldBreed };
        _lPlaats = new Label
        {
            AutoSize = true, ForeColor = thema.Voorgrond,
            BackColor = thema.Achtergrond, Location = new Point(P(20), P(106)),
        };
        _plaats = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(veldX, P(103)), Width = veldBreed,
        };
        _lTaal = new Label
        {
            AutoSize = true, ForeColor = thema.Voorgrond,
            BackColor = thema.Achtergrond, Location = new Point(P(20), P(138)),
        };
        _taal = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(veldX, P(135)), Width = veldBreed,
        };
        // De taalnamen staan er in hun eigen taal, zodat ze in beide gevallen
        // te herkennen zijn.
        _taal.Items.AddRange(new object[] { "Nederlands", "English" });
        _taal.SelectedIndex = Taal.Engels ? 1 : 0;
        _taal.SelectedIndexChanged += (_, _) =>
        {
            Taal.Engels = _taal.SelectedIndex == 1;
            ZetTeksten();
        };

        _mee = new Button
        {
            Location = new Point(P(20), P(176)), Width = P(126), Height = P(30),
        };
        _kijk = new Button
        {
            Location = new Point(P(156), P(176)), Width = P(126), Height = P(30),
        };
        foreach (var b in new[] { _mee, _kijk })
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = thema.Knop;
            b.ForeColor = thema.KnopVoor;
            b.UseVisualStyleBackColor = false;
        }

        _mee.Click += (_, _) =>
        {
            Plaats = _plaats.SelectedIndex == 0
                ? new Random().Next(1, Regels.AantalSpelers + 1)
                : _plaats.SelectedIndex;
            DialogResult = DialogResult.OK;
        };
        _kijk.Click += (_, _) => { Plaats = 0; DialogResult = DialogResult.OK; };

        // Bij afbreken telt de taalkeuze niet: dan blijft alles zoals het was.
        FormClosed += (_, _) =>
        {
            if (DialogResult == DialogResult.OK) Scorelijst.BewaarTaal();
            else Taal.Engels = _taalBijBinnenkomst;
        };

        Controls.AddRange(new Control[]
        {
            _kop, _onder, _lNaam, _naam, _lPlaats, _plaats, _lTaal, _taal, _mee, _kijk,
        });
        AcceptButton = _mee;
        ZetTeksten();
    }

    /// <summary>
    /// Alle opschriften opnieuw zetten, ook na een wissel van taal. De breedte
    /// wordt daarna opnieuw gemeten: de Engelse regel met de zes namen is
    /// langer dan de Nederlandse en viel er anders buiten.
    /// </summary>
    private void ZetTeksten()
    {
        Text = Taal.NieuwSpel;
        _kop.Text = Taal.DoeJeMee;
        _onder.Text = Taal.SpelenMee;
        _lNaam.Text = Taal.LabelNaam;
        _lPlaats.Text = Taal.LabelPlaats;
        _lTaal.Text = Taal.LabelTaal;
        _mee.Text = Taal.IkDoeMee;
        _kijk.Text = Taal.AlleenKijken;

        int gekozen = _plaats.Items.Count > 0 ? Math.Max(0, _plaats.SelectedIndex) : 0;
        _plaats.Items.Clear();
        _plaats.Items.Add(Taal.Willekeurig);
        for (int i = 1; i <= Regels.AantalSpelers; i++) _plaats.Items.Add(i.ToString());
        _plaats.SelectedIndex = gekozen;

        int rand = (int)Math.Round(20 * DeviceDpi / 96f);
        int nodig = new[]
        {
            Breedte(_kop), Breedte(_onder), _naam.Right, _plaats.Right, _taal.Right, _kijk.Right,
        }.Max() + rand;
        ClientSize = new Size(Math.Max((int)Math.Round(302 * DeviceDpi / 96f), nodig),
                              _mee.Bottom + (int)Math.Round(16 * DeviceDpi / 96f));
    }

    /// <summary>De ruimte die een opschrift echt nodig heeft, vanaf de linkerrand.</summary>
    private static int Breedte(Label l) =>
        l.Left + TextRenderer.MeasureText(l.Text, l.Font).Width;
}

/// <summary>De scorelijst, die het bestand "SCORE" uit het origineel vervangt.</summary>
public sealed class ScoreForm : Form
{
    public ScoreForm(Thema thema, Font gewoon, Font vet)
    {
        Text = Taal.Scorelijst;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = MaximizeBox = false;
        ShowInTaskbar = false;
        BackColor = thema.Achtergrond;
        Font = gewoon;
        // Zonder dit schaalt Windows de knoppen nog eens bovenop de eigen
        // schaling hieronder, en valt de onderste rij buiten het venster.
        AutoScaleMode = AutoScaleMode.None;

        float s = DeviceDpi / 96f;
        int P(double v) => (int)Math.Round(v * s);

        // Een tabel met bedragen loopt alleen recht in een lettertype met vaste
        // letterbreedte; het gewone lettertype van de moderne weergave niet.
        var tabel = new Font("Consolas", 9.5f);
        var tabelVet = new Font("Consolas", 9.5f, FontStyle.Bold);
        FormClosed += (_, _) => { tabel.Dispose(); tabelVet.Dispose(); };

        var lijst = Scorelijst.Lees().Scores;
        var vak = new TextBox
        {
            Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.None, BackColor = thema.LogAchter,
            ForeColor = thema.LogVoor, Font = tabel, TabStop = false,
            Location = new Point(P(12), P(38)), Size = new Size(P(280), P(320)),
        };
        var kop = new Label
        {
            Text = Taal.ScoreKop, AutoSize = false, Font = tabelVet,
            BackColor = thema.KopAchter, ForeColor = thema.KopVoor,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(P(12), P(12)), Size = new Size(P(280), P(22)),
        };

        if (lijst.Count == 0) vak.Text = Taal.NogGeenScores;
        else
            vak.Lines = lijst.Select((r, i) => $" {i + 1,2}  {r.Naam,-10} {r.Vermogen,10}").ToArray();

        var sluit = new Button
        {
            Text = Taal.Sluiten, Location = new Point(P(107), vak.Bottom + P(10)),
            Width = P(90), Height = P(30),
            FlatStyle = FlatStyle.Flat, BackColor = thema.Knop, ForeColor = thema.KnopVoor,
            UseVisualStyleBackColor = false,
        };
        sluit.FlatAppearance.BorderSize = 0;
        sluit.Click += (_, _) => Close();

        Controls.AddRange(new Control[] { kop, vak, sluit });
        AcceptButton = sluit;
        ClientSize = new Size(P(304), sluit.Bottom + P(12));
    }
}
