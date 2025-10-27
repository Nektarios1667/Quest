using System.Linq;
using System.Windows.Forms;
using SysColor = System.Drawing.Color;
namespace Quest.Editor;
public class InputField(string label, Func<string, bool>? validate = null, string?[]? dropdownOptions = null)
{
    public string Label { get; set; } = label;
    public Func<string, bool> Validate { get; set; } = validate ?? (_ => true);
    public string?[]? DropdownOptions { get; set; } = dropdownOptions;
}

public static class PopupFactory
{
    public static bool PopupOpen { get; private set; } = false;
    public static Form? Form { get; private set; }
    public static (bool success, string[] values) ShowInputForm(string title, InputField[] fields, bool closeOnEnter = true, Action<string[]>? onSubmit = null)
    {
        if (PopupOpen) return (false, Array.Empty<string>());
        PopupOpen = true;
        // Setup
        Form = new() { Text = title, Width = 400, Height = 100 + fields.Length * 35 };
        List<Control> inputs = [];

        // Fields
        for (int i = 0; i < fields.Length; i++)
        {
            InputField field = fields[i];

            // Label
            Label label = new()
            {
                Text = field.Label,
                Left = 10,
                Top = 10 + i * 35,
                Width = 150
            };

            Control inputControl;
            // Combo
            if (field.DropdownOptions != null)
            {
                ComboBox combo = new()
                {
                    Left = 165,
                    Top = 10 + i * 35,
                    Width = 200,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                combo.Items.AddRange(field.DropdownOptions!);
                combo.SelectedIndex = 0;
                inputControl = combo;
            }
            // Input textbox
            else
            {
                TextBox textBox = new()
                {
                    Left = 165,
                    Top = 10 + i * 35,
                    Width = 200
                };
                inputControl = textBox;
            }

            // Add created controls to the form
            Form.Controls.Add(label);
            Form.Controls.Add(inputControl);
            inputs.Add(inputControl);
        }

        // Error message label
        Label outputLabel = new()
        {
            Name = "OutputLabel",
            Text = "",
            ForeColor = SysColor.Green,
            Left = 10,
            Top = fields.Length * 35 + 38,
            Width = 380
        };
        Form.Controls.Add(outputLabel);

        // Enter button
        Button okButton = new()
        {
            Text = "OK",
            Left = 10,
            Width = 150,
            Top = fields.Length * 35 + 10,
            DialogResult = DialogResult.None
        };

        // Validations
        okButton.Click += (sender, e) =>
        {
            for (int i = 0; i < fields.Length; i++)
            {
                string value = inputs[i] switch
                {
                    TextBox t => t.Text,
                    ComboBox c => c.SelectedItem?.ToString() ?? "",
                    _ => ""
                };

                if (!fields[i].Validate(value))
                {
                    outputLabel.Text = $"Invalid input: {fields[i].Label}";
                    outputLabel.ForeColor = SysColor.Red;
                    return;
                }
            }

            string[] values = GetValues(inputs);

            // If handler
            if (onSubmit != null)
            {
                onSubmit(values);
                return; // Don't close the form now
            }

            // No handler
            if (closeOnEnter)
                Form.DialogResult = DialogResult.OK;
        };

        Form.Controls.Add(okButton);
        Form.AcceptButton = okButton;

        // Results
        DialogResult result = Form.ShowDialog();
        PopupOpen = false;
        if (result == DialogResult.OK)
        {
            string[] values = GetValues(inputs);
            return (true, values);
        }

        return (false, Array.Empty<string>());
    }
    public static void ShowMessage(string title, string message)
    {
        System.Windows.Forms.MessageBox.Show(title, message);
    }
    private static string[] GetValues(List<Control> inputs)
    {
        return inputs.Select(input =>
        {
            return input switch
            {
                TextBox t => t.Text,
                ComboBox c => c.SelectedItem?.ToString() ?? "",
                _ => ""
            };
        }).ToArray();
    }
    public static void SetOutputLabel(string value, SysColor color)
    {
        if (!PopupOpen || Form == null) return;
        var outputLabel = Form.Controls["OutputLabel"]!;
        outputLabel.Text = value;
        outputLabel.ForeColor = color;
        outputLabel.Visible = true;
    }
    public static bool IsNotEmpty(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
    public static bool IsNumeric(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return double.TryParse(value, out _);
    }
    public static bool IsInteger(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return int.TryParse(value, out _);
    }
    public static bool IsPositiveInteger(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (!int.TryParse(value, out int result)) return false;
        return result > 0;
    }
    public static bool IsPositiveIntegerOrZero(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (!int.TryParse(value, out int result)) return false;
        return result >= 0;
    }
    public static bool IsByte(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return byte.TryParse(value, out _);

    }
    public static bool IsUInt16(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return ushort.TryParse(value, out _);
    }
    public static bool IsScaleValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (!double.TryParse(value, out double result)) return false;
        return result > 0 && result <= 25.5;
    }
    public static bool IsTexture(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return Enum.GetNames(typeof(TextureID)).Any(t => t.Equals(value, StringComparison.OrdinalIgnoreCase));
    }
    public static bool IsDecal(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return Enum.GetNames(typeof(DecalType)).Any(t => t.Equals(value, StringComparison.OrdinalIgnoreCase));
    }
}
