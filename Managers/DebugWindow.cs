using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public void SetFrameTimes(IEnumerable<(string Name, double Time)> frameTimes)
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
            UIDsListbox.BeginUpdate();

            UIDsListbox.Items.Clear();
            UIDsListbox.Items.AddRange(
                uids
                    .Select(x => $"{x.Name}: {x.InUse} / {x.Counter}")
                    .ToArray());

            UIDsListbox.EndUpdate();
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
}
