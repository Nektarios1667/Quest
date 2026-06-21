using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quest.Managers;

public partial class KeybindsSettings : Form
{
    bool waitingForKey = false;
    int row;
    int col;
    public KeybindsSettings()
    {
        InitializeComponent();
    }
    public void SetBinds(Dictionary<InputAction, InputBinding> binds)
    {
        foreach (var kv in binds)
            BindsGrid.Rows.Add(kv.Key.ToString(), kv.Value.ToString());
    }

    private void BindsGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == 1)
        {
            row = e.RowIndex;
            col = e.ColumnIndex;
            waitingForKey = true;

            BindsGrid.Rows[row].Cells[col].Value = "Press key...";
        }
    }
    protected override bool ProcessCmdKey(ref Message msg, System.Windows.Forms.Keys keyData)
    {
        if (waitingForKey)
        {
            BindsGrid.Rows[row]
                .Cells[col]
                .Value = keyData;

            waitingForKey = false;

            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        DebugManager.StartBenchmark("SaveBinds");
        // Rebind
        foreach (DataGridViewRow row in BindsGrid.Rows)
        {
            InputManager.Rebind(Enum.Parse<InputAction>(row.Cells[0].Value?.ToString()!, true), InputManager.ParseBindingString(row.Cells[1].Value?.ToString()!));
        }

        // Write
        SettingsManager.WriteSettings();
        DebugManager.EndBenchmark("SaveBinds");
    }
}
