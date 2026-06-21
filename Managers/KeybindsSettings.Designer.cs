using System.Windows.Forms;

namespace Quest.Managers;

partial class KeybindsSettings
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
        BindsGrid = new DataGridView();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        SaveButton = new Button();
        CancelButton = new Button();
        PressKeyLabel = new Label();
        ((System.ComponentModel.ISupportInitialize)BindsGrid).BeginInit();
        SuspendLayout();
        // 
        // BindsGrid
        // 
        BindsGrid.AllowUserToAddRows = false;
        BindsGrid.AllowUserToDeleteRows = false;
        BindsGrid.AllowUserToResizeColumns = false;
        BindsGrid.AllowUserToResizeRows = false;
        BindsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        BindsGrid.BackgroundColor = System.Drawing.SystemColors.ControlDarkDark;
        BindsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        BindsGrid.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2 });
        BindsGrid.Location = new System.Drawing.Point(9, 9);
        BindsGrid.MultiSelect = false;
        BindsGrid.Name = "BindsGrid";
        BindsGrid.ReadOnly = true;
        BindsGrid.RowHeadersVisible = false;
        BindsGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        BindsGrid.Size = new System.Drawing.Size(780, 398);
        BindsGrid.TabIndex = 0;
        BindsGrid.CellDoubleClick += BindsGrid_CellDoubleClick;
        // 
        // dataGridViewTextBoxColumn1
        // 
        dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gray;
        dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle1;
        dataGridViewTextBoxColumn1.HeaderText = "Action";
        dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        dataGridViewTextBoxColumn1.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn2
        // 
        dataGridViewCellStyle2.BackColor = System.Drawing.Color.Gray;
        dataGridViewTextBoxColumn2.DefaultCellStyle = dataGridViewCellStyle2;
        dataGridViewTextBoxColumn2.HeaderText = "Bind";
        dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
        dataGridViewTextBoxColumn2.ReadOnly = true;
        // 
        // SaveButton
        // 
        SaveButton.BackColor = System.Drawing.Color.Gray;
        SaveButton.Location = new System.Drawing.Point(696, 413);
        SaveButton.Name = "SaveButton";
        SaveButton.Size = new System.Drawing.Size(92, 25);
        SaveButton.TabIndex = 1;
        SaveButton.Text = "Save";
        SaveButton.UseVisualStyleBackColor = false;
        SaveButton.Click += SaveButton_Click;
        // 
        // CancelButton
        // 
        CancelButton.BackColor = System.Drawing.Color.Gray;
        CancelButton.Location = new System.Drawing.Point(9, 413);
        CancelButton.Name = "CancelButton";
        CancelButton.Size = new System.Drawing.Size(92, 25);
        CancelButton.TabIndex = 2;
        CancelButton.Text = "Cancel";
        CancelButton.UseVisualStyleBackColor = false;
        CancelButton.Click += CancelButton_Click;
        // 
        // PressKeyLabel
        // 
        PressKeyLabel.AutoSize = true;
        PressKeyLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        PressKeyLabel.Location = new System.Drawing.Point(279, 410);
        PressKeyLabel.Name = "PressKeyLabel";
        PressKeyLabel.Size = new System.Drawing.Size(244, 25);
        PressKeyLabel.TabIndex = 3;
        PressKeyLabel.Text = "Press key or escape to exit...";
        PressKeyLabel.Visible = false;
        // 
        // KeybindsSettings
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = System.Drawing.SystemColors.ControlText;
        ClientSize = new System.Drawing.Size(800, 450);
        Controls.Add(PressKeyLabel);
        Controls.Add(CancelButton);
        Controls.Add(SaveButton);
        Controls.Add(BindsGrid);
        ForeColor = System.Drawing.Color.White;
        Name = "KeybindsSettings";
        Text = "KeybindsSettings";
        ((System.ComponentModel.ISupportInitialize)BindsGrid).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.DataGridView BindsGrid;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private Button SaveButton;
    private Button CancelButton;
    private Label PressKeyLabel;
}