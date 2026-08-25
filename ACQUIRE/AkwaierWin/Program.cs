using Akwaier.Engine;
using Akwaier.Ui;

namespace Akwaier;

internal static class Program
{
    /// <summary>Waar een onverwachte fout wordt vastgelegd.</summary>
    internal static readonly string FoutLog =
        Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? ".", "akwaier-fout.txt");

    [STAThread]
    private static int Main(string[] args)
    {
        // "Akwaier.exe zelftest [verslag.txt]" rekent de spelregels na en schrijft
        // het verslag weg. Handig na een wijziging in de engine.
        if (args.Length > 0 && args[0].Equals("zelftest", StringComparison.OrdinalIgnoreCase))
        {
            string pad = args.Length > 1
                ? args[1]
                : Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? ".",
                               "akwaier-zelftest.txt");
            int fout = Zelftest.Draai(pad);
            if (fout > 0)
                MessageBox.Show(Taal.ControlesMislukt(fout, pad), Taal.ZelftestTitel,
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return fout;
        }

        // Zonder dit sluit Windows Forms het programma af zodra er ergens in de
        // schermafhandeling een fout optreedt: het venster verdwijnt dan zonder
        // melding. Met CatchException blijft het staan en zie je wat er misging.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Meld(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log(e.ExceptionObject as Exception);

        // De taal van de vorige keer, of anders die van Windows zelf.
        Scorelijst.LaadTaal();

        ApplicationConfiguration.Initialize();
        Application.Run(new SpelForm());
        return 0;
    }

    internal static void Log(Exception ex)
    {
        if (ex == null) return;
        try { File.AppendAllText(FoutLog, $"{DateTime.Now:s}\n{ex}\n\n"); } catch (Exception) { }
    }

    private static void Meld(Exception ex)
    {
        Log(ex);
        try
        {
            MessageBox.Show(
                $"{Taal.IetsMisMaarLooptDoor}\n\n{ex.GetType().Name}: {ex.Message}\n\n" +
                $"{Taal.VolledigeMeldingIn}\n{FoutLog}",
                Taal.KortTitel, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception) { }
    }
}
