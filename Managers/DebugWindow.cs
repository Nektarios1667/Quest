using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Quest.Managers;

public partial class DebugWindow : Form
{
    private bool _allowClose = false;
    public DebugWindow()
    {
        InitializeComponent();
        this.FormClosing += DebugWindow_FormClosing;
    }
    private void RunOnUI(Action action)
    {
        if (InvokeRequired)
            Invoke(action);
        else
            action();
    }

    public void SetFrameTimes(IEnumerable<(string Name, float Time)> frameTimes)
    {
        RunOnUI(() =>
        {
            FrameTimesListbox.BeginUpdate();

            FrameTimesListbox.Items.Clear();
            FrameTimesListbox.Items.AddRange(
                frameTimes
                    .Select(x => $"{x.Name}: {x.Time:F2} ms")
                    .ToArray());

            FrameTimesListbox.EndUpdate();
        });
    }

    public void SetTimers(IEnumerable<(string Name, float Time)> timers)
    {
        RunOnUI(() =>
        {
            TimersListbox.BeginUpdate();

            TimersListbox.Items.Clear();
            TimersListbox.Items.AddRange(
                timers
                    .Select(x => $"{x.Name}: {x.Time:F2} s")
                    .ToArray());

            TimersListbox.EndUpdate();
        });
    }

    public void SetUIDS(IEnumerable<(string Name, int InUse, int Counter)> uids)
    {
        RunOnUI(() =>
        {
            UIDsListbox.Items.Clear();
            UIDsListbox.Items.AddRange(
                uids
                    .Select(x => $"{x.Name}: {x.InUse} / {x.Counter}")
                    .ToArray());
        });
    }
    public void SetLog(IEnumerable<string> messages)
    {
        RunOnUI(() =>
        {
            LogListbox.Items.Clear();
            LogListbox.Items.AddRange(messages.ToArray());
        });
    }
    public void AddLog(string message)
    {
        RunOnUI(() =>
        {
            LogListbox.Items.Add(message);
        });
    }
    public void SetInfobox(string info)
    {
        RunOnUI(() =>
        {
            Infobox.Text = info;
        });
    }
    public void SetMemoryInfobox(IEnumerable<string> info)
    {
        RunOnUI(() =>
        {
            MemoryListbox.Items.Clear();
            MemoryListbox.Items.AddRange(info.ToArray());
        });
    }
    public void SetLights(IEnumerable<RadialLight> lights)
    {
        RunOnUI(() =>
        {
            LightsListbox.Items.Clear();
            LightsListbox.Items.AddRange(
                lights
                .Select(l => $"{l.Position.X}, {l.Position.Y} | {l.Size / Constants.TileSize.X}t | sf:{l.SingleFrame}")
                .ToArray()
            );
        });
    }
    public void SetScripts(string scriptInfo)
    {
        RunOnUI(() =>
        {
            ScriptsListbox.Items.Clear();
            ScriptsListbox.Items.AddRange(
                scriptInfo.Split('\n')
            );
        });
    }

    public void ForceClose()
    {
        _allowClose = true;
        Close();
    }
    private void DebugWindow_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
            Hide();
        }
    }

    private void ExportLog_Click(object sender, EventArgs e)
    {
        RunOnUI(() =>
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Export log";
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.DefaultExt = "txt";
                dialog.FileName = "document.txt";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllLines(dialog.FileName, LogListbox.Items.Cast<object>().Select(i => i.ToString())!);
                }
            }
        });
    }

}
