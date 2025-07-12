using RaidCrawler.WinForms.Controls;

namespace RaidCrawler.WinForms.SubForms
{
    partial class TimeViewer
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
            ItemResult = new ItemResultGridView();
            SwitchTimelabel = new Label();
            CurTime = new TextBox();
            TimeFormat = new ComboBox();
            BackwardHours = new Button();
            FowardHours = new Button();
            ResetTimeButton = new Button();
            ReadCurrentTimeButton = new Button();
            HoursNum = new NumericUpDown();
            FowardMinute = new Button();
            BackwardMinute = new Button();
            MinutesNum = new NumericUpDown();
            SetNTPTimeButton = new Button();
            SetTimeButton = new Button();
            ItemPrinterRNG = new Button();
            CurrentTimelabel = new Label();
            TimeFormatLabel = new Label();
            MinutesLabel = new Label();
            HourLabel = new Label();
            TimeZonelabel = new Label();
            TimeZoneCombo = new ComboBox();
            PrintModeLabel = new Label();
            PrintModeCombo = new ComboBox();
            PrintButton = new Button();
            label1 = new Label();
            PrintItemCombo = new ComboBox();
            FilterButton = new Button();
            UseFilter = new CheckBox();
            SeedLabel = new Label();
            Seed = new TextBox();
            ((System.ComponentModel.ISupportInitialize)HoursNum).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MinutesNum).BeginInit();
            SuspendLayout();
            // 
            // ItemResult
            // 
            ItemResult.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ItemResult.Location = new Point(353, 12);
            ItemResult.Name = "ItemResult";
            ItemResult.Size = new Size(235, 282);
            ItemResult.TabIndex = 21;
            // 
            // SwitchTimelabel
            // 
            SwitchTimelabel.AutoSize = true;
            SwitchTimelabel.Location = new Point(12, 174);
            SwitchTimelabel.Name = "SwitchTimelabel";
            SwitchTimelabel.Size = new Size(73, 15);
            SwitchTimelabel.TabIndex = 0;
            SwitchTimelabel.Text = "Switch Time:";
            // 
            // CurTime
            // 
            CurTime.Location = new Point(95, 171);
            CurTime.Name = "CurTime";
            CurTime.Size = new Size(139, 23);
            CurTime.TabIndex = 0;
            CurTime.TextChanged += CurTime_TextChanged;
            // 
            // TimeFormat
            // 
            TimeFormat.Items.AddRange(new object[] { "Ticks", "DateTime" });
            TimeFormat.Location = new Point(95, 200);
            TimeFormat.Name = "TimeFormat";
            TimeFormat.Size = new Size(94, 23);
            TimeFormat.TabIndex = 0;
            TimeFormat.SelectedIndexChanged += TimeFormat_SelectedIndexChanged;
            // 
            // BackwardHours
            // 
            BackwardHours.Location = new Point(110, 12);
            BackwardHours.Name = "BackwardHours";
            BackwardHours.Size = new Size(79, 23);
            BackwardHours.TabIndex = 0;
            BackwardHours.Text = "Backward";
            BackwardHours.UseVisualStyleBackColor = true;
            BackwardHours.Click += Backward_Click;
            // 
            // FowardHours
            // 
            FowardHours.Location = new Point(12, 12);
            FowardHours.Name = "FowardHours";
            FowardHours.Size = new Size(70, 23);
            FowardHours.TabIndex = 1;
            FowardHours.Text = "Forward";
            FowardHours.UseVisualStyleBackColor = true;
            FowardHours.Click += Forward_Click;
            // 
            // ResetTimeButton
            // 
            ResetTimeButton.Location = new Point(208, 137);
            ResetTimeButton.Name = "ResetTimeButton";
            ResetTimeButton.Size = new Size(102, 23);
            ResetTimeButton.TabIndex = 2;
            ResetTimeButton.Text = "ResetNTPTime";
            ResetTimeButton.UseVisualStyleBackColor = true;
            ResetTimeButton.Click += Reset_Click;
            // 
            // ReadCurrentTimeButton
            // 
            ReadCurrentTimeButton.Location = new Point(12, 137);
            ReadCurrentTimeButton.Name = "ReadCurrentTimeButton";
            ReadCurrentTimeButton.Size = new Size(92, 23);
            ReadCurrentTimeButton.TabIndex = 3;
            ReadCurrentTimeButton.Text = "Read Current";
            ReadCurrentTimeButton.UseVisualStyleBackColor = true;
            ReadCurrentTimeButton.Click += Read_Click;
            // 
            // HoursNum
            // 
            HoursNum.Location = new Point(110, 50);
            HoursNum.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            HoursNum.Name = "HoursNum";
            HoursNum.Size = new Size(69, 23);
            HoursNum.TabIndex = 6;
            HoursNum.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // FowardMinute
            // 
            FowardMinute.Location = new Point(12, 79);
            FowardMinute.Name = "FowardMinute";
            FowardMinute.Size = new Size(92, 23);
            FowardMinute.TabIndex = 7;
            FowardMinute.Text = "Forward Min";
            FowardMinute.UseVisualStyleBackColor = true;
            FowardMinute.Click += FowardMinute_Click;
            // 
            // BackwardMinute
            // 
            BackwardMinute.Location = new Point(110, 79);
            BackwardMinute.Name = "BackwardMinute";
            BackwardMinute.Size = new Size(92, 23);
            BackwardMinute.TabIndex = 8;
            BackwardMinute.Text = "Backward Min";
            BackwardMinute.UseVisualStyleBackColor = true;
            BackwardMinute.Click += BackwardMinute_Click;
            // 
            // MinutesNum
            // 
            MinutesNum.Location = new Point(110, 108);
            MinutesNum.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            MinutesNum.Name = "MinutesNum";
            MinutesNum.Size = new Size(69, 23);
            MinutesNum.TabIndex = 9;
            MinutesNum.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // SetNTPTimeButton
            // 
            SetNTPTimeButton.Location = new Point(110, 137);
            SetNTPTimeButton.Name = "SetNTPTimeButton";
            SetNTPTimeButton.Size = new Size(92, 23);
            SetNTPTimeButton.TabIndex = 10;
            SetNTPTimeButton.Text = "Set Current";
            SetNTPTimeButton.UseVisualStyleBackColor = true;
            SetNTPTimeButton.Click += SetTime_Click;
            // 
            // SetTimeButton
            // 
            SetTimeButton.Location = new Point(12, 271);
            SetTimeButton.Name = "SetTimeButton";
            SetTimeButton.Size = new Size(92, 23);
            SetTimeButton.TabIndex = 11;
            SetTimeButton.Text = "Set Time";
            SetTimeButton.UseVisualStyleBackColor = true;
            SetTimeButton.Click += SetTimeButton_Click;
            // 
            // ItemPrinterRNG
            // 
            ItemPrinterRNG.Location = new Point(110, 271);
            ItemPrinterRNG.Name = "ItemPrinterRNG";
            ItemPrinterRNG.Size = new Size(92, 23);
            ItemPrinterRNG.TabIndex = 12;
            ItemPrinterRNG.Text = "Item Printer";
            ItemPrinterRNG.UseVisualStyleBackColor = true;
            ItemPrinterRNG.Click += ItemPrinterRNG_Click;
            // 
            // CurrentTimelabel
            // 
            CurrentTimelabel.AutoSize = true;
            CurrentTimelabel.Location = new Point(12, 322);
            CurrentTimelabel.Name = "CurrentTimelabel";
            CurrentTimelabel.Size = new Size(77, 15);
            CurrentTimelabel.TabIndex = 13;
            CurrentTimelabel.Text = "Current Time:";
            // 
            // TimeFormatLabel
            // 
            TimeFormatLabel.AutoSize = true;
            TimeFormatLabel.Location = new Point(12, 203);
            TimeFormatLabel.Name = "TimeFormatLabel";
            TimeFormatLabel.Size = new Size(69, 15);
            TimeFormatLabel.TabIndex = 14;
            TimeFormatLabel.Text = "TimeFormat";
            // 
            // MinutesLabel
            // 
            MinutesLabel.AutoSize = true;
            MinutesLabel.Location = new Point(12, 110);
            MinutesLabel.Name = "MinutesLabel";
            MinutesLabel.Size = new Size(80, 15);
            MinutesLabel.TabIndex = 15;
            MinutesLabel.Text = "Ajust Minutes";
            // 
            // HourLabel
            // 
            HourLabel.AutoSize = true;
            HourLabel.Location = new Point(12, 52);
            HourLabel.Name = "HourLabel";
            HourLabel.Size = new Size(69, 15);
            HourLabel.TabIndex = 16;
            HourLabel.Text = "Ajust Hours";
            // 
            // TimeZonelabel
            // 
            TimeZonelabel.AutoSize = true;
            TimeZonelabel.Location = new Point(12, 370);
            TimeZonelabel.Name = "TimeZonelabel";
            TimeZonelabel.Size = new Size(67, 15);
            TimeZonelabel.TabIndex = 17;
            TimeZonelabel.Text = "TimeZones:";
            // 
            // TimeZoneCombo
            // 
            TimeZoneCombo.Location = new Point(95, 367);
            TimeZoneCombo.Name = "TimeZoneCombo";
            TimeZoneCombo.Size = new Size(231, 23);
            TimeZoneCombo.TabIndex = 18;
            TimeZoneCombo.SelectedIndexChanged += TimeZoneCombo_SelectedIndexChanged;
            // 
            // PrintModeLabel
            // 
            PrintModeLabel.AutoSize = true;
            PrintModeLabel.Location = new Point(-1, 418);
            PrintModeLabel.Name = "PrintModeLabel";
            PrintModeLabel.Size = new Size(105, 15);
            PrintModeLabel.TabIndex = 19;
            PrintModeLabel.Text = "Current PrintMode";
            // 
            // PrintModeCombo
            // 
            PrintModeCombo.Items.AddRange(new object[] { "Ticks", "DateTime" });
            PrintModeCombo.Location = new Point(123, 415);
            PrintModeCombo.Name = "PrintModeCombo";
            PrintModeCombo.Size = new Size(111, 23);
            PrintModeCombo.TabIndex = 20;
            // 
            // PrintButton
            // 
            PrintButton.Location = new Point(353, 318);
            PrintButton.Name = "PrintButton";
            PrintButton.Size = new Size(92, 23);
            PrintButton.TabIndex = 22;
            PrintButton.Text = "Print!";
            PrintButton.UseVisualStyleBackColor = true;
            PrintButton.Click += PrintButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(353, 370);
            label1.Name = "label1";
            label1.Size = new Size(96, 15);
            label1.TabIndex = 23;
            label1.Text = "Print Item Count:";
            // 
            // PrintItemCombo
            // 
            PrintItemCombo.Items.AddRange(new object[] { "1", "5", "10" });
            PrintItemCombo.Location = new Point(455, 367);
            PrintItemCombo.Name = "PrintItemCombo";
            PrintItemCombo.Size = new Size(54, 23);
            PrintItemCombo.TabIndex = 25;
            // 
            // FilterButton
            // 
            FilterButton.Enabled = false;
            FilterButton.Location = new Point(353, 414);
            FilterButton.Name = "FilterButton";
            FilterButton.Size = new Size(92, 23);
            FilterButton.TabIndex = 26;
            FilterButton.Text = "Filter";
            FilterButton.UseVisualStyleBackColor = true;
            FilterButton.Click += FilterButton_Click;
            // 
            // UseFilter
            // 
            UseFilter.AutoSize = true;
            UseFilter.Location = new Point(455, 418);
            UseFilter.Name = "UseFilter";
            UseFilter.Size = new Size(79, 19);
            UseFilter.TabIndex = 27;
            UseFilter.Text = "Use Filter?";
            UseFilter.UseVisualStyleBackColor = true;
            UseFilter.CheckedChanged += UseFilter_CheckedChanged;
            // 
            // SeedLabel
            // 
            SeedLabel.AutoSize = true;
            SeedLabel.Location = new Point(12, 237);
            SeedLabel.Name = "SeedLabel";
            SeedLabel.Size = new Size(74, 15);
            SeedLabel.TabIndex = 28;
            SeedLabel.Text = "Current Seed";
            // 
            // Seed
            // 
            Seed.Location = new Point(95, 234);
            Seed.Name = "Seed";
            Seed.Size = new Size(139, 23);
            Seed.TabIndex = 29;
            // 
            // TimeViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(609, 467);
            Controls.Add(Seed);
            Controls.Add(SeedLabel);
            Controls.Add(UseFilter);
            Controls.Add(FilterButton);
            Controls.Add(PrintItemCombo);
            Controls.Add(label1);
            Controls.Add(PrintButton);
            Controls.Add(PrintModeCombo);
            Controls.Add(PrintModeLabel);
            Controls.Add(TimeZoneCombo);
            Controls.Add(TimeZonelabel);
            Controls.Add(HourLabel);
            Controls.Add(MinutesLabel);
            Controls.Add(TimeFormatLabel);
            Controls.Add(CurrentTimelabel);
            Controls.Add(ItemPrinterRNG);
            Controls.Add(SetTimeButton);
            Controls.Add(SetNTPTimeButton);
            Controls.Add(MinutesNum);
            Controls.Add(BackwardMinute);
            Controls.Add(FowardMinute);
            Controls.Add(HoursNum);
            Controls.Add(ReadCurrentTimeButton);
            Controls.Add(ResetTimeButton);
            Controls.Add(FowardHours);
            Controls.Add(BackwardHours);
            Controls.Add(SwitchTimelabel);
            Controls.Add(CurTime);
            Controls.Add(TimeFormat);
            Controls.Add(ItemResult);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TimeViewer";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "TimeViewer";
            ((System.ComponentModel.ISupportInitialize)HoursNum).EndInit();
            ((System.ComponentModel.ISupportInitialize)MinutesNum).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        private Button BackwardHours;
        private Button FowardHours;
        private Button ResetTimeButton;
        private Button ReadCurrentTimeButton;
        private NumericUpDown HoursNum;
        private Button FowardMinute;
        private Button BackwardMinute;
        private NumericUpDown MinutesNum;
        private Button SetNTPTimeButton;
        private Label SwitchTimelabel;
        private TextBox CurTime;
        private ComboBox TimeFormat;
        private Button SetTimeButton;
        private Button ItemPrinterRNG;
        private Label CurrentTimelabel;
        private Label TimeFormatLabel;
        private Label MinutesLabel;
        private Label HourLabel;
        private Label TimeZonelabel;
        private ComboBox TimeZoneCombo;
        private Label PrintModeLabel;
        private ComboBox PrintModeCombo;
        private ItemResultGridView ItemResult;
        private Button PrintButton;
        private Label label1;
        private ComboBox PrintItemCombo;
        private Button FilterButton;
        private CheckBox UseFilter;
        private Label SeedLabel;
        private TextBox Seed;
    }
}
