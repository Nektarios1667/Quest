namespace Quest.Editor.Generator;

partial class QLVViewer
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
        FileLabel = new System.Windows.Forms.Label();
        SaveTree = new System.Windows.Forms.TreeView();
        fileSystemWatcher1 = new System.IO.FileSystemWatcher();
        openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
        SelectButton = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)fileSystemWatcher1).BeginInit();
        SuspendLayout();
        // 
        // FileLabel
        // 
        FileLabel.AutoSize = true;
        FileLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        FileLabel.Location = new System.Drawing.Point(12, 9);
        FileLabel.Name = "FileLabel";
        FileLabel.Size = new System.Drawing.Size(0, 21);
        FileLabel.TabIndex = 0;
        // 
        // SaveTree
        // 
        SaveTree.Location = new System.Drawing.Point(12, 33);
        SaveTree.Name = "SaveTree";
        SaveTree.Size = new System.Drawing.Size(776, 405);
        SaveTree.TabIndex = 1;
        // 
        // fileSystemWatcher1
        // 
        fileSystemWatcher1.EnableRaisingEvents = true;
        fileSystemWatcher1.SynchronizingObject = this;
        // 
        // openFileDialog1
        // 
        openFileDialog1.FileName = "openFileDialog1";
        // 
        // SelectButton
        // 
        SelectButton.Location = new System.Drawing.Point(713, 4);
        SelectButton.Name = "SelectButton";
        SelectButton.Size = new System.Drawing.Size(75, 23);
        SelectButton.TabIndex = 2;
        SelectButton.Text = "Select...";
        SelectButton.UseVisualStyleBackColor = true;
        SelectButton.Click += SelectButton_Click;
        // 
        // QLVViewer
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(800, 450);
        Controls.Add(SelectButton);
        Controls.Add(SaveTree);
        Controls.Add(FileLabel);
        Name = "QLVViewer";
        Text = "Quest Level File Viewer";
        ((System.ComponentModel.ISupportInitialize)fileSystemWatcher1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label FileLabel;
    private System.Windows.Forms.TreeView SaveTree;
    private System.IO.FileSystemWatcher fileSystemWatcher1;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.Button SelectButton;
}