namespace RaidCrawler.WinForms
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components=new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            toolTip=new ToolTip(components);
            ButtonAdvanceDate=new Button();
            CheckEnableFilters=new CheckBox();
            ButtonDisconnect=new Button();
            ButtonConnect=new Button();
            InputSwitchIP=new TextBox();
            LabelSwitchIP=new Label();
            LabelLoadedRaids=new Label();
            TeraType=new TextBox();
            LabelTeraType=new Label();
            PID=new TextBox();
            LabelPID=new Label();
            EC=new TextBox();
            LabelEC=new Label();
            Seed=new TextBox();
            LabelSeed=new Label();
            ButtonNext=new Button();
            ButtonPrevious=new Button();
            Area=new TextBox();
            LabelUNK_2=new Label();
            IVs=new TextBox();
            LabelIVs=new Label();
            CurrentIdentifierText=new TextBox();
            CurrentIdentifier=new Label();
            ButtonReadRaids=new Button();
            labelEvent=new Label();
            Difficulty=new TextBox();
            LabelDifficulty=new Label();
            ButtonViewRAM=new Button();
            Species=new TextBox();
            LabelSpecies=new Label();
            LabelMoves=new Label();
            Move1=new TextBox();
            Move2=new TextBox();
            Move4=new TextBox();
            Move3=new TextBox();
            PokemonScale=new TextBox();
            PokemonScaleValue=new TextBox();
            Nature=new TextBox();
            LabelNature=new Label();
            LabelScale=new Label();
            LabelScaleValue=new Label();
            Gender=new TextBox();
            LabelGender=new Label();
            StopFilter=new Button();
            Sprite=new PictureBox();
            Ability=new TextBox();
            LabelAbility=new Label();
            GemIcon=new PictureBox();
            ButtonDownloadEvents=new Button();
            ConfigSettings=new Button();
            Rewards=new Button();
            LabelSandwichBonus=new Label();
            RaidBoost=new ComboBox();
            ComboIndex=new ComboBox();
            SendScreenshot=new Button();
            TeleportToDenButton=new Button();
            EventRaidReset=new Button();
            Identifier=new ComboBox();
            SearchTimer=new System.Timers.Timer();
            btnOpenMap=new Button();
            groupBox1=new GroupBox();
            statusStrip1=new StatusStrip();
            StatusLabel=new ToolStripStatusLabel();
            ToolStripStatusLabel=new ToolStripStatusLabel();
            Label_DayAdvance=new ToolStripStatusLabel();
            USB_Port_label=new Label();
            USB_Port_TB=new TextBox();
            StopAdvance_Button=new Button();
            OHKO=new Button();
            ResetRaids=new Button();
            SetTime=new Button();
            CurrentTimeButton=new Button();
            DateTimeButton=new Button();
            ((System.ComponentModel.ISupportInitialize)Sprite).BeginInit();
            ((System.ComponentModel.ISupportInitialize)GemIcon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SearchTimer).BeginInit();
            groupBox1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // ButtonAdvanceDate
            // 
            ButtonAdvanceDate.Enabled=false;
            ButtonAdvanceDate.Location=new Point(134, 131);
            ButtonAdvanceDate.Margin=new Padding(5, 4, 5, 4);
            ButtonAdvanceDate.Name="ButtonAdvanceDate";
            ButtonAdvanceDate.Size=new Size(110, 36);
            ButtonAdvanceDate.TabIndex=81;
            ButtonAdvanceDate.Text="Advance Date";
            toolTip.SetToolTip(ButtonAdvanceDate, "Advance Date performs one (1) time set.\r\n\r\nIf Stop Filters are defined, Advance Date\r\ncontinues advancing the date until a stop\r\nfilter has been hit.");
            ButtonAdvanceDate.UseVisualStyleBackColor=true;
            ButtonAdvanceDate.Click+=ButtonAdvanceDate_Click;
            // 
            // CheckEnableFilters
            // 
            CheckEnableFilters.AutoSize=true;
            CheckEnableFilters.Checked=true;
            CheckEnableFilters.CheckState=CheckState.Checked;
            CheckEnableFilters.Location=new Point(134, 368);
            CheckEnableFilters.Margin=new Padding(3, 4, 3, 4);
            CheckEnableFilters.Name="CheckEnableFilters";
            CheckEnableFilters.Size=new Size(119, 24);
            CheckEnableFilters.TabIndex=119;
            CheckEnableFilters.Text="Enable Filters";
            toolTip.SetToolTip(CheckEnableFilters, "Enable Filters enables or disables all filters\r\nentirely.\r\n\r\nEnabled - Advance Date will continue until\r\na match occurs from a filter.\r\n\r\nDisabled - Advance Date will only advance\r\none (1) day.");
            CheckEnableFilters.UseVisualStyleBackColor=true;
            CheckEnableFilters.Click+=EnableFilters_Click;
            // 
            // ButtonDisconnect
            // 
            ButtonDisconnect.Enabled=false;
            ButtonDisconnect.Location=new Point(134, 47);
            ButtonDisconnect.Margin=new Padding(5, 4, 5, 4);
            ButtonDisconnect.Name="ButtonDisconnect";
            ButtonDisconnect.Size=new Size(111, 36);
            ButtonDisconnect.TabIndex=11;
            ButtonDisconnect.Text="Disconnect";
            ButtonDisconnect.UseVisualStyleBackColor=true;
            ButtonDisconnect.Click+=Disconnect_Click;
            // 
            // ButtonConnect
            // 
            ButtonConnect.Location=new Point(15, 47);
            ButtonConnect.Margin=new Padding(5, 4, 5, 4);
            ButtonConnect.Name="ButtonConnect";
            ButtonConnect.Size=new Size(111, 36);
            ButtonConnect.TabIndex=10;
            ButtonConnect.Text="Connect";
            ButtonConnect.UseVisualStyleBackColor=true;
            ButtonConnect.Click+=ButtonConnect_Click;
            // 
            // InputSwitchIP
            // 
            InputSwitchIP.Location=new Point(96, 8);
            InputSwitchIP.Margin=new Padding(5, 4, 5, 4);
            InputSwitchIP.Name="InputSwitchIP";
            InputSwitchIP.Size=new Size(147, 27);
            InputSwitchIP.TabIndex=8;
            InputSwitchIP.Text="www.www.www.www";
            InputSwitchIP.TextChanged+=InputSwitchIP_Changed;
            // 
            // LabelSwitchIP
            // 
            LabelSwitchIP.AutoSize=true;
            LabelSwitchIP.Location=new Point(15, 12);
            LabelSwitchIP.Margin=new Padding(5, 0, 5, 0);
            LabelSwitchIP.Name="LabelSwitchIP";
            LabelSwitchIP.Size=new Size(71, 20);
            LabelSwitchIP.TabIndex=6;
            LabelSwitchIP.Text="Switch IP:";
            // 
            // LabelLoadedRaids
            // 
            LabelLoadedRaids.AutoSize=true;
            LabelLoadedRaids.Font=new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            LabelLoadedRaids.Location=new Point(14, 137);
            LabelLoadedRaids.Name="LabelLoadedRaids";
            LabelLoadedRaids.Size=new Size(85, 20);
            LabelLoadedRaids.TabIndex=12;
            LabelLoadedRaids.Text="Matches: 0";
            // 
            // TeraType
            // 
            TeraType.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            TeraType.Location=new Point(338, 203);
            TeraType.Margin=new Padding(5, 4, 5, 4);
            TeraType.Name="TeraType";
            TeraType.ReadOnly=true;
            TeraType.Size=new Size(108, 25);
            TeraType.TabIndex=49;
            // 
            // LabelTeraType
            // 
            LabelTeraType.AutoSize=true;
            LabelTeraType.Location=new Point(265, 208);
            LabelTeraType.Name="LabelTeraType";
            LabelTeraType.Size=new Size(74, 20);
            LabelTeraType.TabIndex=48;
            LabelTeraType.Text="Tera Type:";
            LabelTeraType.TextAlign=ContentAlignment.MiddleRight;
            // 
            // PID
            // 
            PID.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            PID.Location=new Point(338, 91);
            PID.Margin=new Padding(5, 4, 5, 4);
            PID.Name="PID";
            PID.ReadOnly=true;
            PID.Size=new Size(108, 25);
            PID.TabIndex=47;
            // 
            // LabelPID
            // 
            LabelPID.AutoSize=true;
            LabelPID.Location=new Point(298, 93);
            LabelPID.Name="LabelPID";
            LabelPID.Size=new Size(35, 20);
            LabelPID.TabIndex=46;
            LabelPID.Text="PID:";
            LabelPID.TextAlign=ContentAlignment.MiddleRight;
            // 
            // EC
            // 
            EC.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            EC.Location=new Point(338, 53);
            EC.Margin=new Padding(5, 4, 5, 4);
            EC.Name="EC";
            EC.ReadOnly=true;
            EC.Size=new Size(108, 25);
            EC.TabIndex=45;
            // 
            // LabelEC
            // 
            LabelEC.AutoSize=true;
            LabelEC.Location=new Point(303, 56);
            LabelEC.Name="LabelEC";
            LabelEC.Size=new Size(29, 20);
            LabelEC.TabIndex=44;
            LabelEC.Text="EC:";
            LabelEC.TextAlign=ContentAlignment.MiddleRight;
            // 
            // Seed
            // 
            Seed.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Seed.Location=new Point(338, 16);
            Seed.Margin=new Padding(5, 4, 5, 4);
            Seed.Name="Seed";
            Seed.ReadOnly=true;
            Seed.Size=new Size(108, 25);
            Seed.TabIndex=43;
            Seed.Click+=Seed_Click;
            // 
            // LabelSeed
            // 
            LabelSeed.AutoSize=true;
            LabelSeed.Location=new Point(290, 19);
            LabelSeed.Name="LabelSeed";
            LabelSeed.Size=new Size(45, 20);
            LabelSeed.TabIndex=42;
            LabelSeed.Text="Seed:";
            LabelSeed.TextAlign=ContentAlignment.MiddleRight;
            // 
            // ButtonNext
            // 
            ButtonNext.Enabled=false;
            ButtonNext.Location=new Point(170, 91);
            ButtonNext.Margin=new Padding(3, 4, 3, 4);
            ButtonNext.Name="ButtonNext";
            ButtonNext.Size=new Size(51, 33);
            ButtonNext.TabIndex=56;
            ButtonNext.Text=">>";
            ButtonNext.UseVisualStyleBackColor=true;
            ButtonNext.Click+=ButtonNext_Click;
            // 
            // ButtonPrevious
            // 
            ButtonPrevious.Enabled=false;
            ButtonPrevious.Location=new Point(34, 91);
            ButtonPrevious.Margin=new Padding(3, 4, 3, 4);
            ButtonPrevious.Name="ButtonPrevious";
            ButtonPrevious.Size=new Size(51, 33);
            ButtonPrevious.TabIndex=55;
            ButtonPrevious.Text="<<";
            ButtonPrevious.UseVisualStyleBackColor=true;
            ButtonPrevious.Click+=ButtonPrevious_Click;
            // 
            // Area
            // 
            Area.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Area.Location=new Point(338, 315);
            Area.Margin=new Padding(5, 4, 5, 4);
            Area.Name="Area";
            Area.ReadOnly=true;
            Area.Size=new Size(309, 25);
            Area.TabIndex=61;
            Area.Click+=DisplayMap;
            // 
            // LabelUNK_2
            // 
            LabelUNK_2.AutoSize=true;
            LabelUNK_2.Location=new Point(293, 320);
            LabelUNK_2.Name="LabelUNK_2";
            LabelUNK_2.Size=new Size(43, 20);
            LabelUNK_2.TabIndex=60;
            LabelUNK_2.Text="Area:";
            LabelUNK_2.TextAlign=ContentAlignment.MiddleRight;
            // 
            // IVs
            // 
            IVs.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            IVs.Location=new Point(338, 277);
            IVs.Margin=new Padding(5, 4, 5, 4);
            IVs.Name="IVs";
            IVs.ReadOnly=true;
            IVs.Size=new Size(309, 25);
            IVs.TabIndex=69;
            // 
            // LabelIVs
            // 
            LabelIVs.AutoSize=true;
            LabelIVs.Location=new Point(303, 283);
            LabelIVs.Name="LabelIVs";
            LabelIVs.Size=new Size(30, 20);
            LabelIVs.TabIndex=68;
            LabelIVs.Text="IVs:";
            LabelIVs.TextAlign=ContentAlignment.MiddleRight;
            // 
            // CurrentIdentifierText
            // 
            CurrentIdentifierText.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CurrentIdentifierText.Location=new Point(177, 531);
            CurrentIdentifierText.Margin=new Padding(5, 4, 5, 4);
            CurrentIdentifierText.Name="CurrentIdentifierText";
            CurrentIdentifierText.ReadOnly=true;
            CurrentIdentifierText.Size=new Size(97, 25);
            CurrentIdentifierText.TabIndex=69;
            CurrentIdentifierText.TextAlign=HorizontalAlignment.Center;
            // 
            // CurrentIdentifier
            // 
            CurrentIdentifier.AutoSize=true;
            CurrentIdentifier.Location=new Point(14, 533);
            CurrentIdentifier.Name="CurrentIdentifier";
            CurrentIdentifier.Size=new Size(163, 20);
            CurrentIdentifier.TabIndex=60;
            CurrentIdentifier.Text="Current Event Identifier:";
            CurrentIdentifier.TextAlign=ContentAlignment.MiddleRight;
            // 
            // ButtonReadRaids
            // 
            ButtonReadRaids.Enabled=false;
            ButtonReadRaids.Location=new Point(7, 29);
            ButtonReadRaids.Margin=new Padding(5, 4, 5, 4);
            ButtonReadRaids.Name="ButtonReadRaids";
            ButtonReadRaids.Size=new Size(103, 33);
            ButtonReadRaids.TabIndex=80;
            ButtonReadRaids.Text="Read Raids";
            ButtonReadRaids.UseVisualStyleBackColor=true;
            ButtonReadRaids.Click+=ButtonReadRaids_Click;
            // 
            // labelEvent
            // 
            labelEvent.AutoSize=true;
            labelEvent.Font=new Font("Segoe UI", 9F, FontStyle.Bold|FontStyle.Italic, GraphicsUnit.Point);
            labelEvent.ForeColor=SystemColors.ControlText;
            labelEvent.Location=new Point(567, 85);
            labelEvent.Name="labelEvent";
            labelEvent.Size=new Size(92, 20);
            labelEvent.TabIndex=84;
            labelEvent.Text="~~Event~~";
            labelEvent.TextAlign=ContentAlignment.MiddleLeft;
            labelEvent.Visible=false;
            // 
            // Difficulty
            // 
            Difficulty.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Difficulty.Location=new Point(537, 203);
            Difficulty.Margin=new Padding(5, 4, 5, 4);
            Difficulty.Name="Difficulty";
            Difficulty.ReadOnly=true;
            Difficulty.Size=new Size(110, 25);
            Difficulty.TabIndex=86;
            // 
            // LabelDifficulty
            // 
            LabelDifficulty.AutoSize=true;
            LabelDifficulty.Location=new Point(463, 208);
            LabelDifficulty.Name="LabelDifficulty";
            LabelDifficulty.Size=new Size(70, 20);
            LabelDifficulty.TabIndex=85;
            LabelDifficulty.Text="Difficulty:";
            LabelDifficulty.TextAlign=ContentAlignment.MiddleRight;
            // 
            // ButtonViewRAM
            // 
            ButtonViewRAM.Enabled=false;
            ButtonViewRAM.Location=new Point(119, 29);
            ButtonViewRAM.Margin=new Padding(3, 4, 3, 4);
            ButtonViewRAM.Name="ButtonViewRAM";
            ButtonViewRAM.Size=new Size(103, 33);
            ButtonViewRAM.TabIndex=89;
            ButtonViewRAM.Text="Dump Raid";
            ButtonViewRAM.UseVisualStyleBackColor=true;
            ButtonViewRAM.Click+=ViewRAM_Click;
            // 
            // Species
            // 
            Species.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Species.Location=new Point(338, 128);
            Species.Margin=new Padding(5, 4, 5, 4);
            Species.Name="Species";
            Species.ReadOnly=true;
            Species.Size=new Size(309, 25);
            Species.TabIndex=93;
            // 
            // LabelSpecies
            // 
            LabelSpecies.AutoSize=true;
            LabelSpecies.Location=new Point(275, 133);
            LabelSpecies.Name="LabelSpecies";
            LabelSpecies.Size=new Size(62, 20);
            LabelSpecies.TabIndex=92;
            LabelSpecies.Text="Species:";
            LabelSpecies.TextAlign=ContentAlignment.MiddleRight;
            // 
            // LabelMoves
            // 
            LabelMoves.AutoSize=true;
            LabelMoves.Location=new Point(279, 373);
            LabelMoves.Name="LabelMoves";
            LabelMoves.Size=new Size(55, 20);
            LabelMoves.TabIndex=94;
            LabelMoves.Text="Moves:";
            LabelMoves.TextAlign=ContentAlignment.MiddleRight;
            // 
            // Move1
            // 
            Move1.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Move1.Location=new Point(338, 352);
            Move1.Margin=new Padding(5, 4, 5, 4);
            Move1.Name="Move1";
            Move1.ReadOnly=true;
            Move1.Size=new Size(151, 25);
            Move1.TabIndex=95;
            Move1.Click+=Move_Clicked;
            // 
            // Move2
            // 
            Move2.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Move2.Location=new Point(496, 352);
            Move2.Margin=new Padding(5, 4, 5, 4);
            Move2.Name="Move2";
            Move2.ReadOnly=true;
            Move2.Size=new Size(151, 25);
            Move2.TabIndex=96;
            Move2.Click+=Move_Clicked;
            // 
            // Move4
            // 
            Move4.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Move4.Location=new Point(496, 389);
            Move4.Margin=new Padding(5, 4, 5, 4);
            Move4.Name="Move4";
            Move4.ReadOnly=true;
            Move4.Size=new Size(151, 25);
            Move4.TabIndex=98;
            Move4.Click+=Move_Clicked;
            // 
            // Move3
            // 
            Move3.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Move3.Location=new Point(338, 389);
            Move3.Margin=new Padding(5, 4, 5, 4);
            Move3.Name="Move3";
            Move3.ReadOnly=true;
            Move3.Size=new Size(151, 25);
            Move3.TabIndex=97;
            Move3.Click+=Move_Clicked;
            // 
            // PokemonScale
            // 
            PokemonScale.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            PokemonScale.Location=new Point(338, 427);
            PokemonScale.Margin=new Padding(5, 4, 5, 4);
            PokemonScale.Name="PokemonScale";
            PokemonScale.ReadOnly=true;
            PokemonScale.Size=new Size(53, 25);
            PokemonScale.TabIndex=131;
            // 
            // PokemonScaleValue
            // 
            PokemonScaleValue.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            PokemonScaleValue.Location=new Point(367, 463);
            PokemonScaleValue.Margin=new Padding(5, 4, 5, 4);
            PokemonScaleValue.Name="PokemonScaleValue";
            PokemonScaleValue.ReadOnly=true;
            PokemonScaleValue.Size=new Size(53, 25);
            PokemonScaleValue.TabIndex=131;
            // 
            // Nature
            // 
            Nature.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Nature.Location=new Point(537, 240);
            Nature.Margin=new Padding(5, 4, 5, 4);
            Nature.Name="Nature";
            Nature.ReadOnly=true;
            Nature.Size=new Size(110, 25);
            Nature.TabIndex=106;
            // 
            // LabelNature
            // 
            LabelNature.AutoSize=true;
            LabelNature.Location=new Point(477, 245);
            LabelNature.Name="LabelNature";
            LabelNature.Size=new Size(57, 20);
            LabelNature.TabIndex=105;
            LabelNature.Text="Nature:";
            LabelNature.TextAlign=ContentAlignment.MiddleRight;
            // 
            // LabelScale
            // 
            LabelScale.AutoSize=true;
            LabelScale.Location=new Point(280, 432);
            LabelScale.Name="LabelScale";
            LabelScale.Size=new Size(47, 20);
            LabelScale.TabIndex=130;
            LabelScale.Text="Scale:";
            LabelScale.TextAlign=ContentAlignment.MiddleRight;
            // 
            // LabelScaleValue
            // 
            LabelScaleValue.AutoSize=true;
            LabelScaleValue.Location=new Point(280, 468);
            LabelScaleValue.Name="LabelScaleValue";
            LabelScaleValue.Size=new Size(76, 20);
            LabelScaleValue.TabIndex=130;
            LabelScaleValue.Text="RealScale:";
            LabelScaleValue.TextAlign=ContentAlignment.MiddleRight;
            // 
            // Gender
            // 
            Gender.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Gender.Location=new Point(338, 240);
            Gender.Margin=new Padding(5, 4, 5, 4);
            Gender.Name="Gender";
            Gender.ReadOnly=true;
            Gender.Size=new Size(108, 25);
            Gender.TabIndex=104;
            // 
            // LabelGender
            // 
            LabelGender.AutoSize=true;
            LabelGender.Location=new Point(275, 245);
            LabelGender.Name="LabelGender";
            LabelGender.Size=new Size(60, 20);
            LabelGender.TabIndex=103;
            LabelGender.Text="Gender:";
            LabelGender.TextAlign=ContentAlignment.MiddleRight;
            // 
            // StopFilter
            // 
            StopFilter.Location=new Point(14, 364);
            StopFilter.Margin=new Padding(3, 4, 3, 4);
            StopFilter.Name="StopFilter";
            StopFilter.Size=new Size(111, 31);
            StopFilter.TabIndex=107;
            StopFilter.Text="Edit Filters";
            StopFilter.UseVisualStyleBackColor=true;
            StopFilter.Click+=StopFilter_Click;
            // 
            // Sprite
            // 
            Sprite.Location=new Point(569, 9);
            Sprite.Margin=new Padding(3, 4, 3, 4);
            Sprite.Name="Sprite";
            Sprite.Size=new Size(78, 75);
            Sprite.SizeMode=PictureBoxSizeMode.StretchImage;
            Sprite.TabIndex=108;
            Sprite.TabStop=false;
            // 
            // Ability
            // 
            Ability.Font=new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Ability.Location=new Point(338, 165);
            Ability.Margin=new Padding(5, 4, 5, 4);
            Ability.Name="Ability";
            Ability.ReadOnly=true;
            Ability.Size=new Size(309, 25);
            Ability.TabIndex=110;
            // 
            // LabelAbility
            // 
            LabelAbility.AutoSize=true;
            LabelAbility.Location=new Point(280, 169);
            LabelAbility.Name="LabelAbility";
            LabelAbility.Size=new Size(55, 20);
            LabelAbility.TabIndex=109;
            LabelAbility.Text="Ability:";
            LabelAbility.TextAlign=ContentAlignment.MiddleRight;
            // 
            // GemIcon
            // 
            GemIcon.Location=new Point(496, 9);
            GemIcon.Margin=new Padding(3, 4, 3, 4);
            GemIcon.Name="GemIcon";
            GemIcon.Size=new Size(64, 75);
            GemIcon.SizeMode=PictureBoxSizeMode.Zoom;
            GemIcon.TabIndex=111;
            GemIcon.TabStop=false;
            // 
            // ButtonDownloadEvents
            // 
            ButtonDownloadEvents.Enabled=false;
            ButtonDownloadEvents.Location=new Point(119, 67);
            ButtonDownloadEvents.Margin=new Padding(3, 4, 3, 4);
            ButtonDownloadEvents.Name="ButtonDownloadEvents";
            ButtonDownloadEvents.Size=new Size(103, 33);
            ButtonDownloadEvents.TabIndex=112;
            ButtonDownloadEvents.Text="Pull Events";
            ButtonDownloadEvents.UseVisualStyleBackColor=true;
            ButtonDownloadEvents.Click+=DownloadEvents_Click;
            // 
            // ConfigSettings
            // 
            ConfigSettings.Location=new Point(14, 403);
            ConfigSettings.Margin=new Padding(3, 4, 3, 4);
            ConfigSettings.Name="ConfigSettings";
            ConfigSettings.Size=new Size(232, 31);
            ConfigSettings.TabIndex=115;
            ConfigSettings.Text="Open Settings";
            ConfigSettings.UseVisualStyleBackColor=true;
            ConfigSettings.Click+=ConfigSettings_Click;
            // 
            // Rewards
            // 
            Rewards.Location=new Point(119, 104);
            Rewards.Margin=new Padding(3, 4, 3, 4);
            Rewards.Name="Rewards";
            Rewards.Size=new Size(103, 33);
            Rewards.TabIndex=116;
            Rewards.Text="Rewards";
            Rewards.UseVisualStyleBackColor=true;
            Rewards.Click+=Rewards_Click;
            // 
            // LabelSandwichBonus
            // 
            LabelSandwichBonus.AutoSize=true;
            LabelSandwichBonus.Location=new Point(15, 331);
            LabelSandwichBonus.Name="LabelSandwichBonus";
            LabelSandwichBonus.Size=new Size(151, 20);
            LabelSandwichBonus.TabIndex=118;
            LabelSandwichBonus.Text="Raid Sandwich Boost:";
            // 
            // RaidBoost
            // 
            RaidBoost.FormattingEnabled=true;
            RaidBoost.Items.AddRange(new object[] { "0", "1", "2", "3" });
            RaidBoost.Location=new Point(189, 327);
            RaidBoost.Margin=new Padding(3, 4, 3, 4);
            RaidBoost.Name="RaidBoost";
            RaidBoost.Size=new Size(54, 28);
            RaidBoost.TabIndex=117;
            RaidBoost.Text="w";
            RaidBoost.SelectedIndexChanged+=RaidBoost_SelectedIndexChanged;
            // 
            // ComboIndex
            // 
            ComboIndex.BackColor=SystemColors.Window;
            ComboIndex.DropDownStyle=ComboBoxStyle.DropDownList;
            ComboIndex.Enabled=false;
            ComboIndex.FormattingEnabled=true;
            ComboIndex.Location=new Point(91, 91);
            ComboIndex.Margin=new Padding(3, 4, 3, 4);
            ComboIndex.Name="ComboIndex";
            ComboIndex.Size=new Size(73, 28);
            ComboIndex.TabIndex=120;
            ComboIndex.SelectedIndexChanged+=ComboIndex_SelectedIndexChanged;
            // 
            // SendScreenshot
            // 
            SendScreenshot.Location=new Point(7, 67);
            SendScreenshot.Margin=new Padding(3, 4, 3, 4);
            SendScreenshot.Name="SendScreenshot";
            SendScreenshot.Size=new Size(103, 33);
            SendScreenshot.TabIndex=121;
            SendScreenshot.Text="Screenshot";
            SendScreenshot.UseVisualStyleBackColor=true;
            SendScreenshot.Click+=SendScreenshot_Click;
            // 
            // TeleportToDenButton
            // 
            TeleportToDenButton.Enabled=false;
            TeleportToDenButton.Location=new Point(14, 447);
            TeleportToDenButton.Margin=new Padding(3, 4, 3, 4);
            TeleportToDenButton.Name="TeleportToDenButton";
            TeleportToDenButton.Size=new Size(114, 33);
            TeleportToDenButton.TabIndex=121;
            TeleportToDenButton.Text="TeleportToDenButton";
            TeleportToDenButton.UseVisualStyleBackColor=true;
            TeleportToDenButton.Click+=Teleport_Click;
            // 
            // EventRaidReset
            // 
            EventRaidReset.Enabled=false;
            EventRaidReset.Location=new Point(143, 447);
            EventRaidReset.Margin=new Padding(3, 4, 3, 4);
            EventRaidReset.Name="EventRaidReset";
            EventRaidReset.Size=new Size(120, 33);
            EventRaidReset.TabIndex=121;
            EventRaidReset.Text="EventRaidReset";
            EventRaidReset.UseVisualStyleBackColor=true;
            EventRaidReset.Click+=EventFlagReset;
            // 
            // Identifier
            // 
            Identifier.Enabled=false;
            Identifier.FormattingEnabled=true;
            Identifier.Location=new Point(143, 493);
            Identifier.Margin=new Padding(3, 4, 3, 4);
            Identifier.Name="Identifier";
            Identifier.Size=new Size(114, 28);
            Identifier.TabIndex=0;
            // 
            // SearchTimer
            // 
            SearchTimer.Enabled=true;
            SearchTimer.Interval=1D;
            SearchTimer.SynchronizingObject=this;
            SearchTimer.Elapsed+=SearchTimer_Elapsed;
            // 
            // btnOpenMap
            // 
            btnOpenMap.Location=new Point(7, 104);
            btnOpenMap.Margin=new Padding(3, 4, 3, 4);
            btnOpenMap.Name="btnOpenMap";
            btnOpenMap.Size=new Size(103, 33);
            btnOpenMap.TabIndex=124;
            btnOpenMap.Text="Open Map";
            btnOpenMap.UseVisualStyleBackColor=true;
            btnOpenMap.Click+=DisplayMap;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(ButtonViewRAM);
            groupBox1.Controls.Add(ButtonDownloadEvents);
            groupBox1.Controls.Add(btnOpenMap);
            groupBox1.Controls.Add(SendScreenshot);
            groupBox1.Controls.Add(Rewards);
            groupBox1.Controls.Add(ButtonReadRaids);
            groupBox1.Location=new Point(15, 172);
            groupBox1.Margin=new Padding(3, 4, 3, 4);
            groupBox1.Name="groupBox1";
            groupBox1.Padding=new Padding(3, 4, 3, 4);
            groupBox1.Size=new Size(229, 147);
            groupBox1.TabIndex=125;
            groupBox1.TabStop=false;
            groupBox1.Text="Raid Controls";
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize=new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { StatusLabel, ToolStripStatusLabel, Label_DayAdvance });
            statusStrip1.Location=new Point(0, 570);
            statusStrip1.Name="statusStrip1";
            statusStrip1.Padding=new Padding(1, 0, 16, 0);
            statusStrip1.Size=new Size(663, 26);
            statusStrip1.SizingGrip=false;
            statusStrip1.TabIndex=126;
            statusStrip1.Text="statusStrip1";
            // 
            // StatusLabel
            // 
            StatusLabel.Name="StatusLabel";
            StatusLabel.Size=new Size(52, 20);
            StatusLabel.Text="Status:";
            // 
            // ToolStripStatusLabel
            // 
            ToolStripStatusLabel.Name="ToolStripStatusLabel";
            ToolStripStatusLabel.Size=new Size(110, 20);
            ToolStripStatusLabel.Text="Not connected.";
            // 
            // Label_DayAdvance
            // 
            Label_DayAdvance.Name="Label_DayAdvance";
            Label_DayAdvance.Size=new Size(172, 20);
            Label_DayAdvance.Text="Day Skip Successes: 0 / 0";
            Label_DayAdvance.Visible=false;
            // 
            // USB_Port_label
            // 
            USB_Port_label.AutoSize=true;
            USB_Port_label.Location=new Point(15, 12);
            USB_Port_label.Name="USB_Port_label";
            USB_Port_label.Size=new Size(70, 20);
            USB_Port_label.TabIndex=127;
            USB_Port_label.Text="USB Port:";
            // 
            // USB_Port_TB
            // 
            USB_Port_TB.Location=new Point(96, 8);
            USB_Port_TB.Margin=new Padding(3, 4, 3, 4);
            USB_Port_TB.Name="USB_Port_TB";
            USB_Port_TB.Size=new Size(147, 27);
            USB_Port_TB.TabIndex=128;
            USB_Port_TB.Text="w";
            USB_Port_TB.TextAlign=HorizontalAlignment.Center;
            USB_Port_TB.TextChanged+=USB_Port_Changed;
            // 
            // StopAdvance_Button
            // 
            StopAdvance_Button.Location=new Point(134, 131);
            StopAdvance_Button.Margin=new Padding(3, 4, 3, 4);
            StopAdvance_Button.Name="StopAdvance_Button";
            StopAdvance_Button.Size=new Size(110, 36);
            StopAdvance_Button.TabIndex=129;
            StopAdvance_Button.Text="Stop";
            StopAdvance_Button.UseVisualStyleBackColor=true;
            StopAdvance_Button.Visible=false;
            StopAdvance_Button.Click+=StopAdvanceButton_Click;
            // 
            // OHKO
            // 
            OHKO.Enabled=false;
            OHKO.Location=new Point(14, 493);
            OHKO.Margin=new Padding(3, 4, 3, 4);
            OHKO.Name="OHKO";
            OHKO.Size=new Size(115, 33);
            OHKO.TabIndex=129;
            OHKO.Text="Enable OHKO";
            OHKO.UseVisualStyleBackColor=true;
            OHKO.Click+=ChageOHKO;
            // 
            // ResetRaids
            // 
            ResetRaids.Enabled=false;
            ResetRaids.Location=new Point(527, 520);
            ResetRaids.Margin=new Padding(3, 4, 3, 4);
            ResetRaids.Name="ResetRaids";
            ResetRaids.Size=new Size(120, 33);
            ResetRaids.TabIndex=132;
            ResetRaids.Text="ResetRaids";
            ResetRaids.UseVisualStyleBackColor=true;
            ResetRaids.Click+=ResetRaids_Click;
            // 
            // SetTime
            // 
            SetTime.Location=new Point(527, 520);
            SetTime.Margin=new Padding(3, 4, 3, 4);
            SetTime.Name="SetTime";
            SetTime.Size=new Size(120, 33);
            SetTime.TabIndex=133;
            SetTime.Text="Set Time";
            SetTime.UseVisualStyleBackColor=true;
            SetTime.Visible=false;
            SetTime.Click+=SetTime_Click;
            // 
            // CurrentTimeButton
            // 
            CurrentTimeButton.Enabled=false;
            CurrentTimeButton.Location=new Point(407, 520);
            CurrentTimeButton.Margin=new Padding(3, 4, 3, 4);
            CurrentTimeButton.Name="CurrentTimeButton";
            CurrentTimeButton.Size=new Size(109, 33);
            CurrentTimeButton.TabIndex=134;
            CurrentTimeButton.Text="Adjust Time";
            CurrentTimeButton.UseVisualStyleBackColor=true;
            CurrentTimeButton.Click+=CurrentTimeButton_Click;
            // 
            // DateTimeButton
            // 
            DateTimeButton.Enabled=false;
            DateTimeButton.Location=new Point(523, 479);
            DateTimeButton.Margin=new Padding(3, 4, 3, 4);
            DateTimeButton.Name="DateTimeButton";
            DateTimeButton.Size=new Size(128, 33);
            DateTimeButton.TabIndex=135;
            DateTimeButton.Text="Set Current Time";
            DateTimeButton.UseVisualStyleBackColor=true;
            DateTimeButton.Click +=DateTimeButton_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions=new SizeF(8F, 20F);
            AutoScaleMode=AutoScaleMode.Font;
            ClientSize=new Size(663, 596);
            Controls.Add(DateTimeButton);
            Controls.Add(CurrentTimeButton);
            Controls.Add(SetTime);
            Controls.Add(ResetRaids);
            Controls.Add(StopAdvance_Button);
            Controls.Add(OHKO);
            Controls.Add(USB_Port_TB);
            Controls.Add(USB_Port_label);
            Controls.Add(statusStrip1);
            Controls.Add(groupBox1);
            Controls.Add(ComboIndex);
            Controls.Add(CheckEnableFilters);
            Controls.Add(LabelSandwichBonus);
            Controls.Add(LabelLoadedRaids);
            Controls.Add(RaidBoost);
            Controls.Add(ConfigSettings);
            Controls.Add(GemIcon);
            Controls.Add(Ability);
            Controls.Add(LabelAbility);
            Controls.Add(Sprite);
            Controls.Add(StopFilter);
            Controls.Add(Nature);
            Controls.Add(LabelNature);
            Controls.Add(Gender);
            Controls.Add(LabelGender);
            Controls.Add(Move4);
            Controls.Add(Move3);
            Controls.Add(Move2);
            Controls.Add(Move1);
            Controls.Add(LabelScale);
            Controls.Add(LabelScaleValue);
            Controls.Add(PokemonScale);
            Controls.Add(PokemonScaleValue);
            Controls.Add(LabelMoves);
            Controls.Add(Species);
            Controls.Add(LabelSpecies);
            Controls.Add(Difficulty);
            Controls.Add(LabelDifficulty);
            Controls.Add(labelEvent);
            Controls.Add(TeleportToDenButton);
            Controls.Add(EventRaidReset);
            Controls.Add(Identifier);
            Controls.Add(ButtonAdvanceDate);
            Controls.Add(IVs);
            Controls.Add(LabelIVs);
            Controls.Add(CurrentIdentifier);
            Controls.Add(CurrentIdentifierText);
            Controls.Add(Area);
            Controls.Add(LabelUNK_2);
            Controls.Add(ButtonNext);
            Controls.Add(ButtonPrevious);
            Controls.Add(TeraType);
            Controls.Add(LabelTeraType);
            Controls.Add(PID);
            Controls.Add(LabelPID);
            Controls.Add(EC);
            Controls.Add(LabelEC);
            Controls.Add(Seed);
            Controls.Add(LabelSeed);
            Controls.Add(ButtonDisconnect);
            Controls.Add(ButtonConnect);
            Controls.Add(InputSwitchIP);
            Controls.Add(LabelSwitchIP);
            FormBorderStyle=FormBorderStyle.FixedDialog;
            Icon=(Icon)resources.GetObject("$this.Icon");
            Margin=new Padding(3, 4, 3, 4);
            MaximizeBox=false;
            Name="MainWindow";
            FormClosing+=MainWindow_FormClosing;
            Load+=MainWindow_Load;
            ((System.ComponentModel.ISupportInitialize)Sprite).EndInit();
            ((System.ComponentModel.ISupportInitialize)GemIcon).EndInit();
            ((System.ComponentModel.ISupportInitialize)SearchTimer).EndInit();
            groupBox1.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        private ToolTip toolTip;
        private Button ButtonDisconnect;
        private Button ButtonConnect;
        private Button EventRaidReset;
        private TextBox InputSwitchIP;
        private Label LabelSwitchIP;
        private Label LabelLoadedRaids;
        private TextBox TeraType;
        private Label LabelTeraType;
        private TextBox PID;
        private Label LabelPID;
        private TextBox EC;
        private Label LabelEC;
        private TextBox Seed;
        private Label LabelSeed;
        private Button ButtonNext;
        private Button ButtonPrevious;
        private TextBox Area;
        private Label LabelUNK_2;
        private TextBox IVs;
        private Label LabelIVs;
        private Button ButtonReadRaids;
        private ComboBox Identifier;
        private Label CurrentIdentifier;
        private TextBox CurrentIdentifierText;
        private Button ButtonAdvanceDate;
        private Label labelEvent;
        private TextBox Difficulty;
        private Label LabelDifficulty;
        private Button ButtonViewRAM;
        private TextBox Species;
        private Label LabelSpecies;
        private Label LabelMoves;
        private TextBox Move1;
        private TextBox Move2;
        private TextBox Move4;
        private TextBox Move3;
        private TextBox PokemonScale;
        private TextBox PokemonScaleValue;
        private TextBox Nature;
        private Label LabelNature;
        private Label LabelScale;
        private Label LabelScaleValue;
        private TextBox Gender;
        private Label LabelGender;
        private Button StopFilter;
        private PictureBox Sprite;
        private TextBox Ability;
        private Label LabelAbility;
        private PictureBox GemIcon;
        private Button ButtonDownloadEvents;
        private Button ConfigSettings;
        private Button Rewards;
        private Label LabelSandwichBonus;
        private ComboBox RaidBoost;
        private CheckBox CheckEnableFilters;
        private ComboBox ComboIndex;
        private Button SendScreenshot;
        private Button TeleportToDenButton;
        private System.Timers.Timer SearchTimer;
        private Button btnOpenMap;
        private GroupBox groupBox1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel StatusLabel;
        private ToolStripStatusLabel ToolStripStatusLabel;
        private Label USB_Port_label;
        private TextBox USB_Port_TB;
        private Button StopAdvance_Button;
        private Button OHKO;
        private ToolStripStatusLabel Label_DayAdvance;
        private Button ResetRaids;
        private Button SetTime;
        private Button CurrentTimeButton;
        private Button DateTimeButton;
    }
}