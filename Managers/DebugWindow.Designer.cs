namespace Quest.Managers;

partial class DebugWindow
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
        TimerLabel = new System.Windows.Forms.Label();
        FrameTimesLabel = new System.Windows.Forms.Label();
        TimersListbox = new System.Windows.Forms.ListBox();
        FrameTimesListbox = new System.Windows.Forms.ListBox();
        UIDLabel = new System.Windows.Forms.Label();
        UIDsListbox = new System.Windows.Forms.ListBox();
        LogLabel = new System.Windows.Forms.Label();
        LogListbox = new System.Windows.Forms.ListBox();
        Infobox = new System.Windows.Forms.TextBox();
        MemoryListbox = new System.Windows.Forms.ListBox();
        MemoryLabel = new System.Windows.Forms.Label();
        SuspendLayout();
        // 
        // TimerLabel
        // 
        TimerLabel.AutoSize = true;
        TimerLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        TimerLabel.ForeColor = System.Drawing.Color.CornflowerBlue;
        TimerLabel.Location = new System.Drawing.Point(69, 1);
        TimerLabel.Name = "TimerLabel";
        TimerLabel.Size = new System.Drawing.Size(68, 25);
        TimerLabel.TabIndex = 1;
        TimerLabel.Text = "Timers";
        // 
        // FrameTimesLabel
        // 
        FrameTimesLabel.AutoSize = true;
        FrameTimesLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        FrameTimesLabel.ForeColor = System.Drawing.Color.CornflowerBlue;
        FrameTimesLabel.Location = new System.Drawing.Point(259, 1);
        FrameTimesLabel.Name = "FrameTimesLabel";
        FrameTimesLabel.Size = new System.Drawing.Size(118, 25);
        FrameTimesLabel.TabIndex = 2;
        FrameTimesLabel.Text = "Frame Times";
        // 
        // TimersListbox
        // 
        TimersListbox.BackColor = System.Drawing.SystemColors.ActiveBorder;
        TimersListbox.FormattingEnabled = true;
        TimersListbox.ItemHeight = 15;
        TimersListbox.Location = new System.Drawing.Point(8, 27);
        TimersListbox.Name = "TimersListbox";
        TimersListbox.Size = new System.Drawing.Size(197, 289);
        TimersListbox.TabIndex = 4;
        // 
        // FrameTimesListbox
        // 
        FrameTimesListbox.BackColor = System.Drawing.SystemColors.ActiveBorder;
        FrameTimesListbox.FormattingEnabled = true;
        FrameTimesListbox.ItemHeight = 15;
        FrameTimesListbox.Location = new System.Drawing.Point(211, 27);
        FrameTimesListbox.Name = "FrameTimesListbox";
        FrameTimesListbox.Size = new System.Drawing.Size(205, 499);
        FrameTimesListbox.TabIndex = 5;
        // 
        // UIDLabel
        // 
        UIDLabel.AutoSize = true;
        UIDLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        UIDLabel.ForeColor = System.Drawing.Color.CornflowerBlue;
        UIDLabel.Location = new System.Drawing.Point(84, 317);
        UIDLabel.Name = "UIDLabel";
        UIDLabel.Size = new System.Drawing.Size(53, 25);
        UIDLabel.TabIndex = 6;
        UIDLabel.Text = "UIDS";
        // 
        // UIDsListbox
        // 
        UIDsListbox.BackColor = System.Drawing.SystemColors.ActiveBorder;
        UIDsListbox.FormattingEnabled = true;
        UIDsListbox.ItemHeight = 15;
        UIDsListbox.Location = new System.Drawing.Point(8, 342);
        UIDsListbox.Name = "UIDsListbox";
        UIDsListbox.Size = new System.Drawing.Size(197, 124);
        UIDsListbox.TabIndex = 7;
        // 
        // LogLabel
        // 
        LogLabel.AutoSize = true;
        LogLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        LogLabel.ForeColor = System.Drawing.Color.CornflowerBlue;
        LogLabel.Location = new System.Drawing.Point(625, -1);
        LogLabel.Name = "LogLabel";
        LogLabel.Size = new System.Drawing.Size(43, 25);
        LogLabel.TabIndex = 8;
        LogLabel.Text = "Log";
        // 
        // LogListbox
        // 
        LogListbox.BackColor = System.Drawing.SystemColors.ActiveBorder;
        LogListbox.FormattingEnabled = true;
        LogListbox.ItemHeight = 15;
        LogListbox.Location = new System.Drawing.Point(422, 27);
        LogListbox.Name = "LogListbox";
        LogListbox.Size = new System.Drawing.Size(442, 499);
        LogListbox.TabIndex = 9;
        // 
        // Infobox
        // 
        Infobox.BackColor = System.Drawing.SystemColors.WindowText;
        Infobox.Font = new System.Drawing.Font("Segoe UI", 12F);
        Infobox.ForeColor = System.Drawing.Color.White;
        Infobox.Location = new System.Drawing.Point(870, 28);
        Infobox.Multiline = true;
        Infobox.Name = "Infobox";
        Infobox.ReadOnly = true;
        Infobox.Size = new System.Drawing.Size(302, 498);
        Infobox.TabIndex = 10;
        // 
        // MemoryListbox
        // 
        MemoryListbox.BackColor = System.Drawing.SystemColors.ActiveBorder;
        MemoryListbox.FormattingEnabled = true;
        MemoryListbox.ItemHeight = 15;
        MemoryListbox.Location = new System.Drawing.Point(8, 496);
        MemoryListbox.Name = "MemoryListbox";
        MemoryListbox.Size = new System.Drawing.Size(197, 154);
        MemoryListbox.TabIndex = 12;
        // 
        // MemoryLabel
        // 
        MemoryLabel.AutoSize = true;
        MemoryLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        MemoryLabel.ForeColor = System.Drawing.Color.CornflowerBlue;
        MemoryLabel.Location = new System.Drawing.Point(69, 469);
        MemoryLabel.Name = "MemoryLabel";
        MemoryLabel.Size = new System.Drawing.Size(82, 25);
        MemoryLabel.TabIndex = 11;
        MemoryLabel.Text = "Memory";
        // 
        // DebugWindow
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.SystemColors.ControlText;
        ClientSize = new System.Drawing.Size(1184, 661);
        Controls.Add(MemoryListbox);
        Controls.Add(MemoryLabel);
        Controls.Add(Infobox);
        Controls.Add(LogListbox);
        Controls.Add(LogLabel);
        Controls.Add(UIDsListbox);
        Controls.Add(UIDLabel);
        Controls.Add(FrameTimesListbox);
        Controls.Add(TimersListbox);
        Controls.Add(FrameTimesLabel);
        Controls.Add(TimerLabel);
        Name = "DebugWindow";
        Text = "Debug Window";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private System.Windows.Forms.Label TimerLabel;
    private System.Windows.Forms.Label FrameTimesLabel;
    private System.Windows.Forms.ListBox TimersListbox;
    private System.Windows.Forms.ListBox FrameTimesListbox;
    private System.Windows.Forms.Label UIDLabel;
    private System.Windows.Forms.ListBox UIDsListbox;
    private System.Windows.Forms.Label LogLabel;
    private System.Windows.Forms.ListBox LogListbox;
    private System.Windows.Forms.TextBox Infobox;
    private System.Windows.Forms.ListBox MemoryListbox;
    private System.Windows.Forms.Label MemoryLabel;
}