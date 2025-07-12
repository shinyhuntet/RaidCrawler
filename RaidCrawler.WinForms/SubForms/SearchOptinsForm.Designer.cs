namespace RaidCrawler.WinForms.SubForms
{
    partial class SearchOptinsForm
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
            TimeGroup = new GroupBox();
            RangeLabel = new Label();
            RangeNum = new NumericUpDown();
            TimeText = new TextBox();
            TicksNum = new NumericUpDown();
            RB_Tick = new RadioButton();
            RB_Date = new RadioButton();
            SearchGroup = new GroupBox();
            TargetCountNum = new NumericUpDown();
            TargetCountLabel = new Label();
            AdjustTime = new CheckBox();
            CurrentModeCombo = new ComboBox();
            CurrentModeLabel = new Label();
            ItemsCombo = new ComboBox();
            ItemsLabel = new Label();
            PrintModeCombo = new ComboBox();
            TargetModeLabel = new Label();
            SearchModeLabel = new Label();
            SearchModeCombo = new ComboBox();
            SaveButton = new Button();
            TimeGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)RangeNum).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TicksNum).BeginInit();
            SearchGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TargetCountNum).BeginInit();
            SuspendLayout();
            // 
            // TimeGroup
            // 
            TimeGroup.Controls.Add(RangeLabel);
            TimeGroup.Controls.Add(RangeNum);
            TimeGroup.Controls.Add(TimeText);
            TimeGroup.Controls.Add(TicksNum);
            TimeGroup.Controls.Add(RB_Tick);
            TimeGroup.Controls.Add(RB_Date);
            TimeGroup.Location = new Point(12, 12);
            TimeGroup.Name = "TimeGroup";
            TimeGroup.Size = new Size(230, 136);
            TimeGroup.TabIndex = 0;
            TimeGroup.TabStop = false;
            TimeGroup.Text = "Time";
            // 
            // RangeLabel
            // 
            RangeLabel.AutoSize = true;
            RangeLabel.Location = new Point(6, 103);
            RangeLabel.Name = "RangeLabel";
            RangeLabel.Size = new Size(78, 15);
            RangeLabel.TabIndex = 1;
            RangeLabel.Text = "Search Range";
            // 
            // RangeNum
            // 
            RangeNum.Location = new Point(99, 101);
            RangeNum.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            RangeNum.Name = "RangeNum";
            RangeNum.Size = new Size(87, 23);
            RangeNum.TabIndex = 3;
            // 
            // TimeText
            // 
            TimeText.Location = new Point(99, 72);
            TimeText.Name = "TimeText";
            TimeText.Size = new Size(113, 23);
            TimeText.TabIndex = 1;
            TimeText.TextChanged += TimeText_TextChanged;
            // 
            // TicksNum
            // 
            TicksNum.Location = new Point(6, 71);
            TicksNum.Maximum = new decimal(new int[] { -1423204096, 0, 0, 0 });
            TicksNum.Name = "TicksNum";
            TicksNum.Size = new Size(87, 23);
            TicksNum.TabIndex = 1;
            TicksNum.ValueChanged += TicksNum_ValueChanged;
            // 
            // RB_Tick
            // 
            RB_Tick.AutoSize = true;
            RB_Tick.Location = new Point(6, 47);
            RB_Tick.Name = "RB_Tick";
            RB_Tick.Size = new Size(94, 19);
            RB_Tick.TabIndex = 2;
            RB_Tick.Text = "Specific Ticks";
            RB_Tick.UseVisualStyleBackColor = true;
            // 
            // RB_Date
            // 
            RB_Date.AutoSize = true;
            RB_Date.Checked = true;
            RB_Date.Location = new Point(6, 22);
            RB_Date.Name = "RB_Date";
            RB_Date.Size = new Size(92, 19);
            RB_Date.TabIndex = 1;
            RB_Date.TabStop = true;
            RB_Date.Text = "Current Time";
            RB_Date.UseVisualStyleBackColor = true;
            // 
            // SearchGroup
            // 
            SearchGroup.Controls.Add(TargetCountNum);
            SearchGroup.Controls.Add(TargetCountLabel);
            SearchGroup.Controls.Add(AdjustTime);
            SearchGroup.Controls.Add(CurrentModeCombo);
            SearchGroup.Controls.Add(CurrentModeLabel);
            SearchGroup.Controls.Add(ItemsCombo);
            SearchGroup.Controls.Add(ItemsLabel);
            SearchGroup.Controls.Add(PrintModeCombo);
            SearchGroup.Controls.Add(TargetModeLabel);
            SearchGroup.Controls.Add(SearchModeLabel);
            SearchGroup.Controls.Add(SearchModeCombo);
            SearchGroup.Location = new Point(12, 154);
            SearchGroup.Name = "SearchGroup";
            SearchGroup.Size = new Size(230, 221);
            SearchGroup.TabIndex = 1;
            SearchGroup.TabStop = false;
            SearchGroup.Text = "Search Options";
            // 
            // TargetCountNum
            // 
            TargetCountNum.Enabled = false;
            TargetCountNum.Location = new Point(91, 181);
            TargetCountNum.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            TargetCountNum.Name = "TargetCountNum";
            TargetCountNum.Size = new Size(87, 23);
            TargetCountNum.TabIndex = 4;
            TargetCountNum.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // TargetCountLabel
            // 
            TargetCountLabel.AutoSize = true;
            TargetCountLabel.Location = new Point(6, 183);
            TargetCountLabel.Name = "TargetCountLabel";
            TargetCountLabel.Size = new Size(74, 15);
            TargetCountLabel.TabIndex = 12;
            TargetCountLabel.Text = "Target Count";
            // 
            // AdjustTime
            // 
            AdjustTime.AutoSize = true;
            AdjustTime.Location = new Point(91, 109);
            AdjustTime.Name = "AdjustTime";
            AdjustTime.Size = new Size(88, 19);
            AdjustTime.TabIndex = 11;
            AdjustTime.Text = "Time Adjust";
            AdjustTime.UseVisualStyleBackColor = true;
            // 
            // CurrentModeCombo
            // 
            CurrentModeCombo.FormattingEnabled = true;
            CurrentModeCombo.Location = new Point(91, 21);
            CurrentModeCombo.Name = "CurrentModeCombo";
            CurrentModeCombo.Size = new Size(121, 23);
            CurrentModeCombo.TabIndex = 10;
            CurrentModeCombo.SelectedIndexChanged += CurrentModeCombo_SelectedIndexChanged;
            // 
            // CurrentModeLabel
            // 
            CurrentModeLabel.AutoSize = true;
            CurrentModeLabel.Location = new Point(6, 24);
            CurrentModeLabel.Name = "CurrentModeLabel";
            CurrentModeLabel.Size = new Size(80, 15);
            CurrentModeLabel.TabIndex = 9;
            CurrentModeLabel.Text = "Current Mode";
            // 
            // ItemsCombo
            // 
            ItemsCombo.FormattingEnabled = true;
            ItemsCombo.Location = new Point(90, 134);
            ItemsCombo.Name = "ItemsCombo";
            ItemsCombo.Size = new Size(121, 23);
            ItemsCombo.TabIndex = 8;
            // 
            // ItemsLabel
            // 
            ItemsLabel.AutoSize = true;
            ItemsLabel.Location = new Point(6, 137);
            ItemsLabel.Name = "ItemsLabel";
            ItemsLabel.Size = new Size(65, 15);
            ItemsLabel.TabIndex = 7;
            ItemsLabel.Text = "Target Item";
            // 
            // PrintModeCombo
            // 
            PrintModeCombo.FormattingEnabled = true;
            PrintModeCombo.Location = new Point(90, 80);
            PrintModeCombo.Name = "PrintModeCombo";
            PrintModeCombo.Size = new Size(121, 23);
            PrintModeCombo.TabIndex = 6;
            // 
            // TargetModeLabel
            // 
            TargetModeLabel.AutoSize = true;
            TargetModeLabel.Location = new Point(6, 83);
            TargetModeLabel.Name = "TargetModeLabel";
            TargetModeLabel.Size = new Size(73, 15);
            TargetModeLabel.TabIndex = 5;
            TargetModeLabel.Text = "Target Mode";
            // 
            // SearchModeLabel
            // 
            SearchModeLabel.AutoSize = true;
            SearchModeLabel.Location = new Point(6, 53);
            SearchModeLabel.Name = "SearchModeLabel";
            SearchModeLabel.Size = new Size(76, 15);
            SearchModeLabel.TabIndex = 4;
            SearchModeLabel.Text = "Search Mode";
            // 
            // SearchModeCombo
            // 
            SearchModeCombo.FormattingEnabled = true;
            SearchModeCombo.Location = new Point(91, 50);
            SearchModeCombo.Name = "SearchModeCombo";
            SearchModeCombo.Size = new Size(121, 23);
            SearchModeCombo.TabIndex = 2;
            SearchModeCombo.SelectedIndexChanged += SearchModeCombo_SelectedIndexChanged;
            // 
            // SaveButton
            // 
            SaveButton.Location = new Point(80, 400);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(75, 23);
            SaveButton.TabIndex = 2;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // SearchOptinsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(254, 435);
            Controls.Add(SaveButton);
            Controls.Add(SearchGroup);
            Controls.Add(TimeGroup);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SearchOptinsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "SearchOptionsForm";
            TimeGroup.ResumeLayout(false);
            TimeGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)RangeNum).EndInit();
            ((System.ComponentModel.ISupportInitialize)TicksNum).EndInit();
            SearchGroup.ResumeLayout(false);
            SearchGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)TargetCountNum).EndInit();
            ResumeLayout(false);
        }
        #endregion

        private GroupBox TimeGroup;
        private TextBox TimeText;
        private NumericUpDown TicksNum;
        private RadioButton RB_Tick;
        private RadioButton RB_Date;
        private Label RangeLabel;
        private NumericUpDown RangeNum;
        private GroupBox SearchGroup;
        private Label SearchModeLabel;
        private ComboBox SearchModeCombo;
        private ComboBox PrintModeCombo;
        private Label TargetModeLabel;
        private ComboBox ItemsCombo;
        private Label ItemsLabel;
        private Button SaveButton;
        private ComboBox CurrentModeCombo;
        private Label CurrentModeLabel;
        private CheckBox AdjustTime;
        private NumericUpDown TargetCountNum;
        private Label TargetCountLabel;
    }
}
