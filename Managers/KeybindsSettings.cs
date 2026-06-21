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
using WinKeys = System.Windows.Forms.Keys;
using GameKeys = Microsoft.Xna.Framework.Input.Keys;

namespace Quest.Managers;

public partial class KeybindsSettings : Form
{
    bool waitingForKey = false;
    string keyString = "";
    int row;
    int col;
    public KeybindsSettings()
    {
        InitializeComponent();
    }
    public void SetBinds(Dictionary<InputAction, InputBinding> binds)
    {
        foreach (var kv in binds)
        {
            BindsGrid.Rows.Add(StringTools.FillCamelSpaces(kv.Key.ToString()), kv.Value.ToString());
        }
    }

    private void BindsGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == 1)
        {
            row = e.RowIndex;
            col = e.ColumnIndex;
            waitingForKey = true;
            PressKeyLabel.Visible = true;
        }
    }
    protected override bool ProcessCmdKey(ref Message msg, WinKeys keyData)
    {
        if (waitingForKey)
        {

            // Parse key data
            bool exit = false;
            if (keyData.HasFlag(WinKeys.Control) || keyData.HasFlag(WinKeys.Shift) || keyData.HasFlag(WinKeys.Alt))
            {
                if (keyData.HasFlag(WinKeys.Control) && !keyString.Contains("Control")) keyString += "Ctrl+";
                if (keyData.HasFlag(WinKeys.Shift) && !keyString.Contains("Shift")) keyString += "Shift+";
                if (keyData.HasFlag(WinKeys.Alt) && !keyString.Contains("Alt")) keyString += "Alt+";
            } else if (keyData != WinKeys.Escape)
            {
                keyString += WinKeyToString.GetValueOrDefault(keyData, keyData.ToString());
                exit = true;
            }


            // Set bind
            if (keyData != WinKeys.Escape)
                BindsGrid.Rows[row].Cells[col].Value = keyString;

            // Exit binding mode
            if (exit || keyData == WinKeys.Escape)
            {
                waitingForKey = false;
                PressKeyLabel.Visible = false;
                keyString = "";
            }

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
            // Get action-bind pair
            string action = row.Cells[0].Value.ToString()!.Replace(" ", "");
            string bind = row.Cells[1].Value.ToString()!;
            // Parse and bind
            InputManager.Rebind(
                Enum.Parse<InputAction>(action, true),
                InputManager.ParseBindingString(bind)
            );
        }

        // Write
        SettingsManager.WriteSettings();
        DebugManager.EndBenchmark("SaveBinds");
    }
    public static readonly Dictionary<WinKeys, string> WinKeyToString = new()
    {
        { WinKeys.D1, "1" },
        { WinKeys.D2, "2" },
        { WinKeys.D3, "3" },
        { WinKeys.D4, "4" },
        { WinKeys.D5, "5" },
        { WinKeys.D6, "6" },
        { WinKeys.D7, "7" },
        { WinKeys.D8, "8" },
        { WinKeys.D9, "9" },
        { WinKeys.D0, "0" },
        { WinKeys.Oem2, "/" },
        { WinKeys.Oem3, "`" },
        { WinKeys.Oem4, "[" },
        { WinKeys.Oem6, "]" },
        { WinKeys.Oem7, "'" },
        { WinKeys.Oemcomma, "," },
        { WinKeys.OemMinus, "-" },
        { WinKeys.OemPeriod, "." },
        { WinKeys.OemPipe, "\\" },
        { WinKeys.Oemplus, "=" },
        { WinKeys.OemSemicolon, ";" },
    };
}
