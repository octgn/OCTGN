using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Windows.Forms;

namespace Octgn.Launcher;

[SupportedOSPlatform("windows")]
internal partial class Main : Form
{
    private static log4net.ILog Log = log4net.LogManager.GetLogger("Main");

    private readonly Launcher _launcher;
    private readonly CancellationToken _shutdown;

    public Main(Launcher launcher, CancellationToken shutdown) {
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _shutdown = shutdown;

        _shutdown.ThrowIfCancellationRequested();

        InitializeComponent();
    }

    private async void Main_Load(object sender, System.EventArgs e) {
        try {
            await _launcher.RunAsync(_shutdown);
        } catch (OperationCanceledException) {
        } catch (ObjectDisposedException) {
        } catch (Exception ex) {
            Environment.ExitCode = 1;

            Log.Error("Unhandled exception in launcher", ex);

            MessageBox.Show(
                this,
                "An unhandled exception occurred in the launcher. Please see the log file for more details.",
                "Octgn Launcher",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        } finally {
            Close();
        }
    }

    private void timer1_Tick(object sender, EventArgs e) {
        try {
            timer1.Stop();

            Opacity = 1;
            ShowInTaskbar = true;
        } catch (OperationCanceledException) {
        } catch (ObjectDisposedException) {
        } catch (Exception ex) {
            Log.Error("Error setting opacity", ex);
        }
    }
}
