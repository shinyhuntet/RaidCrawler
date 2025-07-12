using NLog.Filters;
using PKHeX.Core;
using PKHeX.Drawing;
using PKHeX.Drawing.PokeSprite;
using pkNX.Structures.FlatBuffers.Gen9;
using RaidCrawler.Core.Connection;
using RaidCrawler.Core.Discord;
using RaidCrawler.Core.Structures;
using RaidCrawler.WinForms.SubForms;
using SysBot.Base;
using SysBot.Pokemon;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using static RaidCrawler.Core.Structures.Offsets;
using static System.Buffers.Binary.BinaryPrimitives;
using static SysBot.Base.SwitchButton;
using System.CodeDom.Compiler;
using Microsoft.VisualBasic;

namespace RaidCrawler.WinForms
{
    public partial class MainWindow : Form
    {
        private static CancellationTokenSource Source = new();
        private static CancellationTokenSource DateAdvanceSource = new();

        private static readonly object _connectLock = new();
        private static readonly object _readLock = new();

        private readonly ClientConfig Config = new();
        private static ConnectionWrapperAsync ConnectionWrapper = default!;
        private SwitchConnectionConfig ConnectionConfig = new()
        { Protocol = SwitchProtocol.WiFi, IP = "192.168.0.0", Port = 6000 };

        private readonly Raid RaidContainer;
        private readonly NotificationHandler Webhook;

        private List<RaidFilter> RaidFilters = [];
        private static readonly Image map_base = Image.FromStream(new MemoryStream(Utils.GetBinaryResource("paldea.png")));
        private static readonly Image map_kitakami = Image.FromStream(new MemoryStream(Utils.GetBinaryResource("kitakami.png")));
        private static readonly Image map_blueberry = Image.FromStream(new MemoryStream(Utils.GetBinaryResource("blueberry.png")));
        private static Dictionary<string, float[]>? den_locations_base;
        private static Dictionary<string, float[]>? den_locations_kitakami;
        private static Dictionary<string, float[]>? den_locations_blueberry;

        // statistics
        public int StatDaySkipTries = 0;
        public int StatDaySkipSuccess = 0;
        public string formTitle;

        private static ulong RaidBlockOffsetBase = 0;
        private static ulong RaidBlockOffsetKitakami = 0;
        private static ulong RaidBlockOffsetBlueberry = 0;
        private bool IsReading = false;
        private bool HideSeed = false;
        private bool ShowExtraMoves = false;
        private bool FirstConnect = true;

        private Color DefaultColor;
        private FormWindowState _WindowState;
        private readonly Stopwatch stopwatch = new();
        private readonly Stopwatch timestamp = new();
        private TeraRaidView? teraRaidView;
        public static ulong BaseBlockKeyPointer = 0;
        ulong PlayerOnMountOffset = 0;
        private bool StopAdvances => !Config.EnableFilters || RaidFilters.Count == 0 || RaidFilters.All(x => !x.Enabled);
        private readonly Version CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;

        public MainWindow()
        {
            string build = string.Empty;
#if DEBUG
            var date = File.GetLastWriteTime(AppContext.BaseDirectory);
            build = $" (dev-{date:yyyyMMdd})";
#endif
            var v = CurrentVersion;
            var filterpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filters.json");
            if (File.Exists(filterpath))
                RaidFilters = JsonSerializer.Deserialize<List<RaidFilter>>(File.ReadAllText(filterpath)) ?? [];
            den_locations_base = JsonSerializer.Deserialize<Dictionary<string, float[]>>(Utils.GetStringResource("den_locations_base.json") ?? "{}");
            den_locations_kitakami = JsonSerializer.Deserialize<Dictionary<string, float[]>>(Utils.GetStringResource("den_locations_kitakami.json") ?? "{}");
            den_locations_blueberry = JsonSerializer.Deserialize<Dictionary<string, float[]>>(Utils.GetStringResource("den_locations_blueberry.json") ?? "{}");

            var configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(configpath))
            {
                var text = File.ReadAllText(configpath);
                Config = JsonSerializer.Deserialize<ClientConfig>(text)!;
            }
            else { Config = new(); }

            formTitle = "RaidCrawler v" + v.Major + "." + v.Minor + "." + v.Build + build + " " + Config.InstanceName;
            Text = formTitle;

            // load raids
            RaidContainer = new(Config.Game);

            SpriteBuilder.ShowTeraThicknessStripe = 0x4;
            SpriteBuilder.ShowTeraOpacityStripe = 0xAF;
            SpriteBuilder.ShowTeraOpacityBackground = 0xFF;
            SpriteUtil.ChangeMode(SpriteBuilderMode.SpritesArtwork5668);

            var protocol = Config.Protocol;
            ConnectionConfig = new()
            {
                IP = Config.IP,
                Port = protocol is SwitchProtocol.WiFi ? 6000 : Config.UsbPort,
                Protocol = Config.Protocol,
            };

            InitializeComponent();
            Webhook = new(Config);

            btnOpenMap.Enabled = false;
            Rewards.Enabled = false;
            SendScreenshot.Enabled = false;
            CheckEnableFilters.Checked = Config.EnableFilters;

            if (Config.Protocol is SwitchProtocol.USB)
            {
                InputSwitchIP.Visible = false;
                LabelSwitchIP.Visible = false;
                USB_Port_TB.Visible = true;
                USB_Port_label.Visible = true;
            }
            else
            {
                InputSwitchIP.Visible = true;
                LabelSwitchIP.Visible = true;
                USB_Port_TB.Visible = false;
                USB_Port_label.Visible = false;
            }
        }

        private void UpdateStatus(string status)
        {
            ToolStripStatusLabel.Text = status;
        }

        private void ButtonEnable(object[] obj, bool enable)
        {
            lock (_readLock)
            {
                for (int b = 0; b < obj.Length; b++)
                {
                    if (obj[b] is not Button btn)
                        continue;

                    if (InvokeRequired)
                        Invoke(() => { btn.Enabled = enable; });
                    else btn.Enabled = enable;
                }

                IsReading = !enable;
            }
        }

        private void ShowDialogs(object obj)
        {
            var window = (Form)obj;
            if (window is null)
                return;

            window.StartPosition = FormStartPosition.CenterParent;
            if (InvokeRequired)
                Invoke(() => { window.ShowDialog(); });
            else window.ShowDialog();
        }


        private void ShowMessageBox(string msg, string caption = "")
        {
            caption = caption == "" ? "RaidCrawler Error" : caption;
            if (InvokeRequired)
                Invoke(() => { MessageBox.Show(msg, caption, MessageBoxButtons.OK); });
            else MessageBox.Show(msg, caption, MessageBoxButtons.OK);
        }

        private int GetRaidBoost()
        {
            if (InvokeRequired)
                return Invoke(() => { return RaidBoost.SelectedIndex; });
            return RaidBoost.SelectedIndex;
        }

        public int GetStatDaySkipTries() => StatDaySkipTries;
        public int GetStatDaySkipSuccess() => StatDaySkipSuccess;

        private void MainWindow_Load(object sender, EventArgs e)
        {
            CenterToScreen();
            InputSwitchIP.Text = Config.IP;
            USB_Port_TB.Text = Config.UsbPort.ToString();
            DefaultColor = IVs.BackColor;
            RaidBoost.SelectedIndex = 0;
            ToggleStreamerView();
            CheckForUpdates();
        }

        private void InputSwitchIP_Changed(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Config.IP = textBox.Text;
            ConnectionConfig = new()
            {
                IP = Config.IP,
                Port = Config.Protocol is SwitchProtocol.WiFi ? 6000 : Config.UsbPort,
                Protocol = Config.Protocol,
            };
        }

        private void USB_Port_Changed(object sender, EventArgs e)
        {
            if (Config.Protocol is SwitchProtocol.WiFi)
                return;

            TextBox textBox = (TextBox)sender;
            if (int.TryParse(textBox.Text, out int port) && port >= 0)
            {
                Config.UsbPort = port;
                ConnectionConfig = new()
                {
                    IP = Config.IP,
                    Port = Config.Protocol is SwitchProtocol.WiFi ? 6000 : Config.UsbPort,
                    Protocol = Config.Protocol,
                };
                return;
            }

            ShowMessageBox("Please enter a valid numerical USB port.");
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            lock (_connectLock)
            {
                if (ConnectionWrapper is not null && ConnectionWrapper.Connected)
                    return;

                ConnectionWrapper = new(ConnectionConfig, UpdateStatus);
                Connect(Source.Token);
            }
        }

        private void Connect(CancellationToken token)
        {
            Task.Run(async () =>
            {
                FirstConnect = true;
                ButtonEnable(new[] { ButtonConnect, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, CurrentTimeButton, DateTimeButton }, false);
                Identifier.Enabled = false;
                try
                {
                    (bool success, string err) = await ConnectionWrapper.Connect(token).ConfigureAwait(false);
                    if (!success)
                    {
                        ButtonEnable(new[] { ButtonConnect }, true);
                        ShowMessageBox(err);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ButtonEnable(new[] { ButtonConnect }, true);
                    ShowMessageBox(ex.Message);
                    return;
                }

                UpdateStatus("Detecting game version...");
                string id = await ConnectionWrapper.Connection.GetTitleID(token).ConfigureAwait(false);
                var game = id switch
                {
                    ScarletID => "Scarlet",
                    VioletID => "Violet",
                    _ => "",
                };

                if (game is "")
                {
                    try
                    {
                        (bool success, string err) = await ConnectionWrapper.DisconnectAsync(token).ConfigureAwait(false);
                        if (!success)
                        {
                            ButtonEnable(new[] { ButtonConnect }, true);
                            ShowMessageBox(err);
                            return;
                        }
                    }
                    catch { }
                    finally
                    {
                        ButtonEnable(new[] { ButtonConnect }, true);
                        ShowMessageBox("Unable to detect Pokémon Scarlet or Pokémon Violet running on your Switch!");
                    }
                    return;
                }

                Config.Game = game;
                RaidContainer.SetGame(Config.Game);

                await ParseBlockKeyPointer(token).ConfigureAwait(false);
                UpdateStatus("Reading story progress...");
                Config.Progress = await ConnectionWrapper.GetStoryProgress(token).ConfigureAwait(false);
                Config.EventProgress = Math.Min(Config.Progress, 3);
                var datalist = new List<uint>();
                UpdateStatus("Reading Event Raid identifier...");
                var Eventidentifier = await ConnectionWrapper.Connection.PointerPeek(KBCATEventRaidIdentifier.Size, KBCATEventRaidIdentifier.Pointer!, token).ConfigureAwait(false);
                var eid = ReadUInt32LittleEndian(Eventidentifier);
                UpdateStatus("Active Event Raid identifier read!");
                CurrentIdentifierText.Text = $"{eid}";
                UpdateStatus("Reading Event Raid identifier deeper...");
                byte[]? CustomIdentifier = null;
                try
                {
                    CustomIdentifier = await ConnectionWrapper.Connection.PointerPeek(SevenStarRaid.Size, SevenStarRaid.Pointer!, token).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                if (CustomIdentifier is not null)
                {
                    UpdateStatus("Event raid identifier read!");
                    for (int Offset = 0; Offset < CustomIdentifier.Length; Offset += 8)
                    {
                        var identifier = ReadUInt32LittleEndian(CustomIdentifier.AsSpan(Offset));
                        if (identifier > 0)
                            datalist.Add(identifier);
                        else
                            break;
                    }
                }
                else
                {
                    UpdateStatus("No Addtional Event Raid identifier Found.");
                }
                byte[]? DefeatIdentifier = null;
                try
                {
                    DefeatIdentifier = await ConnectionWrapper.Connection.PointerPeek(KSevenStarRaidsDefeat.Size, KSevenStarRaidsDefeat.Pointer!, token).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                if (DefeatIdentifier is not null)
                {
                    UpdateStatus("Defeated Flag is read!");
                    for (int Offset = 4; Offset < DefeatIdentifier.Length; Offset += 8)
                    {
                        var defeatidentifier = ReadUInt32LittleEndian(DefeatIdentifier.AsSpan(Offset));
                        if (defeatidentifier > 0 && !datalist.Contains(defeatidentifier))
                            datalist.Add(defeatidentifier);
                        if (defeatidentifier <= 0)
                            break;
                    }
                }
                else
                {
                    UpdateStatus("No Additional Event Raid Defeated Flag Found.");
                }

                Identifier.DataSource = datalist.ToArray();

                UpdateStatus("Reading event raid status...");
                try
                {
                    await ReadEventRaids(token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    ButtonEnable(new[] { ButtonConnect }, true);
                    ShowMessageBox($"Error occurred while reading event raids: {ex.Message}");
                    return;
                }

                UpdateStatus("Reading raids...");
                try
                {
                    await ReadRaids(token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    ButtonEnable(new[] { ButtonConnect }, true);
                    ShowMessageBox($"Error occurred while reading raids: {ex.Message}");
                    return;
                }

                UpdateStatus("Checking OHKO State...");
                try
                {
                    await ReadOHKOState(token).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    ButtonEnable(new[] { ButtonConnect }, true);
                    ShowMessageBox($"Error occurred while cheking ohko state: {ex.Message}");
                    return;
                }
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
                Identifier.Enabled = true;
                if (InvokeRequired)
                    Invoke(() => { ComboIndex.Enabled = true; ComboIndex.SelectedIndex = 0; });
                else ComboIndex.SelectedIndex = 0;
                UpdateStatus("Completed!");
                FirstConnect = false;

            }, token);

        }

        private void Disconnect_Click(object sender, EventArgs e)
        {
            lock (_connectLock)
            {
                if (ConnectionWrapper is null || !ConnectionWrapper.Connected)
                    return;

                Disconnect(Source.Token);
            }
        }

        private void Disconnect(CancellationToken token)
        {
            Task.Run(async () =>
            {
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, false);
                try
                {
                    (bool success, string err) = await ConnectionWrapper.DisconnectAsync(token).ConfigureAwait(false);
                    if (!success)
                        ShowMessageBox(err);
                }
                catch (Exception ex)
                {
                    ShowMessageBox(ex.Message);
                }

                Source.Cancel();
                DateAdvanceSource.Cancel();
                Source = new();
                DateAdvanceSource = new();
                RaidBlockOffsetBase = 0;
                RaidBlockOffsetKitakami = 0;
                RaidBlockOffsetBlueberry = 0;
                ButtonEnable(new[] { ButtonConnect }, true);
            }, token);
        }

        private void ButtonPrevious_Click(object sender, EventArgs e)
        {
            var count = RaidContainer.Container.GetRaidCount();
            if (count > 0)
            {
                var index = (ComboIndex.SelectedIndex + count - 1) % count; // Wrap around
                if (ModifierKeys == Keys.Shift)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var chk = (index + count - i) % count;
                        if (StopAdvances || RaidFilters.Any(z => z.FilterSatisfied(RaidContainer.Container.Encounters[chk], RaidContainer.Container.Raids[chk], RaidBoost.SelectedIndex)))
                        {
                            index = chk;
                            break;
                        }
                    }
                }
                ComboIndex.SelectedIndex = index;
            }
        }

        private void ButtonNext_Click(object sender, EventArgs e)
        {
            var count = RaidContainer.Container.GetRaidCount();
            if (count > 0)
            {
                var index = (ComboIndex.SelectedIndex + count + 1) % count; // Wrap around
                if (ModifierKeys == Keys.Shift)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var chk = (index + count + i) % count;
                        if (StopAdvances || RaidFilters.Any(z => z.FilterSatisfied(RaidContainer.Container.Encounters[chk], RaidContainer.Container.Raids[chk], RaidBoost.SelectedIndex)))
                        {
                            index = chk;
                            break;
                        }
                    }
                }
                ComboIndex.SelectedIndex = index;
            }
        }

        private void ButtonAdvanceDate_Click(object sender, EventArgs e)
        {
            if (ConnectionWrapper is null || !ConnectionWrapper.Connected)
                return;

            ButtonAdvanceDate.Visible = false;
            StopAdvance_Button.Visible = true;
            Task.Run(async () => await AdvanceDateClick(DateAdvanceSource.Token).ConfigureAwait(false), Source.Token);
        }
        private async Task<bool> IsInBattle(CancellationToken token)
        {
            var data = await ConnectionWrapper.Connection.ReadBytesMainAsync(Offsets.IsInBattle, 1, token).ConfigureAwait(false);
            return data[0] <= 0x05;
        }
        private async Task AdvanceDateClick(CancellationToken token)
        {
            try
            {
                ButtonEnable(new[] { ButtonViewRAM, ButtonAdvanceDate, ButtonDisconnect, ButtonDownloadEvents, SendScreenshot, ButtonReadRaids, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, false);
                Invoke(() => Label_DayAdvance.Visible = true);
                SearchTimer.Start();
                stopwatch.Restart();
                timestamp.Restart();
                StatDaySkipTries = 0;
                StatDaySkipSuccess = 0;
                _WindowState = WindowState;

                var stop = false;
                var raids = RaidContainer.Container.Raids;
                int skips = 0;
                while (!stop)
                {
                    /*if (skips >= (Config.ZyroMethod ? Config.SystemReset / 2 : Config.SystemReset))
                    {
                        // When raids are generated, the game determines raids for both the current and next day.
                        // In order to avoid rescanning the same raids on a reset, save the game before reset.
                        await ConnectionWrapper.SaveGame(token).ConfigureAwait(false);
                        await ConnectionWrapper.CloseGame(token).ConfigureAwait(false);
                        await ConnectionWrapper.StartGame(Config, token).ConfigureAwait(false);
                        RaidBlockOffsetBase = 0;
                        RaidBlockOffsetKitakami = 0;
                        RaidBlockOffsetBlueberry = 0;
                        skips = 0;

                        // Read the initial raids upon reopening the game to correctly detect if the next advance fails
                        //await ReadRaids(token).ConfigureAwait(false);
                        //raids = RaidContainer.Container.Raids;
                    }*/
                    var advanceTextInit = $"Day Skip Successes {GetStatDaySkipSuccess()} / {GetStatDaySkipTries() + 1}";
                    Invoke(() => Label_DayAdvance.Text = advanceTextInit);
                    if (teraRaidView is not null)
                        Invoke(() => teraRaidView.DaySkips.Text = $"Day Skip Successes {GetStatDaySkipSuccess()} / {GetStatDaySkipTries() + 1}");

                    var previousSeeds = raids.Select(z => z.Seed).ToList();
                    UpdateStatus("Changing date...");

                    // Reset every 1000 to ensure we don't build up too many menus. Does this affect success?

                    bool streamer = Config.StreamerView && teraRaidView is not null;
                    await ConnectionWrapper.AdvanceDate(Config, skips, token, streamer ? teraRaidView!.UpdateProgressBar : null).ConfigureAwait(false);

                    if (Config.WildEncounter == "WildEncounter")
                    {
                        if (await IsInBattle(token).ConfigureAwait(false))
                            UpdateStatus("Wild Encounter Occured. Defeating Wild Pokemon ...");
                        while (await IsInBattle(token).ConfigureAwait(false))
                            await Click(A, 0_800, token).ConfigureAwait(false);
                    }
                    
                    await ReadRaids(token).ConfigureAwait(false);
                    raids = RaidContainer.Container.Raids;

                    Invoke(DisplayRaid);
                    if (streamer)
                        Invoke(DisplayPrettyRaid);

                    stop = StopAdvanceDate(previousSeeds);
                    skips++;

                    var advanceText = $"Day Skip Successes {GetStatDaySkipSuccess()} / {GetStatDaySkipTries()}";
                    Invoke(() => Label_DayAdvance.Text = advanceText);
                    if (teraRaidView is not null)
                        Invoke(() => teraRaidView.DaySkips.Text = advanceText);
                }

                stopwatch.Stop();
                SearchTimer.Stop();
                var timeSpan = stopwatch.Elapsed;
                string time = string.Format("{0:00}:{1:00}:{2:00}:{3:00}",
                timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                time += $"{Environment.NewLine}{Environment.NewLine}Current DateTime: {(DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss")}";
                StatDaySkipTries = 0;
                StatDaySkipSuccess = 0;


                if (Config.PlaySound)
                    System.Media.SystemSounds.Asterisk.Play();

                if (Config.FocusWindow)
                    Invoke(() => { WindowState = _WindowState; Activate(); });

                if (Config.EnableFilters)
                {
                    var encounters = RaidContainer.Container.Encounters;
                    var rewards = RaidContainer.Container.Rewards;
                    var boost = Invoke(() => { return RaidBoost.SelectedIndex; });
                    var satisfiedFilters = new List<(RaidFilter, ITeraRaid, Raid, IReadOnlyList<(int, int, int)>)>();
                    for (int i = 0; i < raids.Count; i++)
                    {
                        foreach (var filter in RaidFilters)
                        {
                            if (filter is null)
                                continue;

                            if (filter.FilterSatisfied(encounters[i], raids[i], boost))
                            {
                                satisfiedFilters.Add((filter, encounters[i], raids[i], rewards[i]));
                                if (InvokeRequired)
                                    Invoke(() => { ComboIndex.SelectedIndex = i; });
                                else ComboIndex.SelectedIndex = i;
                            }
                        }                        
                    }

                    if (Config.EnableNotification)
                    {
                        var Trainer = await GetFakeTrainerSAVSV(token).ConfigureAwait(false);
                        foreach (var satisfied in satisfiedFilters)
                        {
                            var teraType = satisfied.Item3.GetTeraType(satisfied.Item2);
                            var color = TypeColor.GetTypeSpriteColor((byte)teraType);
                            var hexColor = $"{color.R:X2}{color.G:X2}{color.B:X2}";
                            var blank = new PK9
                            {
                                Species = satisfied.Item2.Species,
                                Form = satisfied.Item2.Form,
                            };

                            var spriteName = GetSpriteNameForUrl(
                                blank,
                                satisfied.Item3.CheckIsShiny(satisfied.Item2)
                            );
                            await Webhook
                                .SendNotification(satisfied.Item2, satisfied.Item3, satisfied.Item1, time, satisfied.Item4, hexColor, spriteName, Trainer, Source.Token)
                                .ConfigureAwait(false);
                        }
                    }

                    if (satisfiedFilters.Count > 0)
                        await ConnectionWrapper.SaveGameNonReset(token).ConfigureAwait(false);

                    if (Config.EnableAlertWindow)
                    {
                        await ConnectionWrapper.Click(SwitchButton.HOME, 1_000, token).ConfigureAwait(false);
                        ShowMessageBox($"{Config.AlertWindowMessage}\n\nTime Spent: {time}", "Result found!");

                    }
                    Invoke(() => Text = $"{formTitle} [Match Found in {time}]");
                }
            }
            catch (OperationCanceledException ex)
            {
                ShowMessageBox("Date advance canceled. Detailds: " + ex.ToString(), "Operation Canceled");
                SearchTimer.Stop();
            }
            catch (Exception ex)
            {
                ShowMessageBox("Date advance stopped. Detailds: " + ex.ToString(), "Exception Occured");
                SearchTimer.Stop();
            }

            if (InvokeRequired)
            {
                Invoke(() => { ButtonAdvanceDate.Visible = true; });
                Invoke(() => { StopAdvance_Button.Visible = false; });
            }
            else
            {
                ButtonAdvanceDate.Visible = true;
                StopAdvance_Button.Visible = false;
            }

            ButtonEnable(new[] { ButtonViewRAM, ButtonAdvanceDate, ButtonDisconnect, ButtonDownloadEvents, SendScreenshot, ButtonReadRaids, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
            DateAdvanceSource = new();
        }

        private void StopAdvanceButton_Click(object sender, EventArgs e)
        {
            StopAdvance_Button.Visible = false;
            ButtonAdvanceDate.Visible = true;
            DateAdvanceSource.Cancel();
            DateAdvanceSource = new();
            teraRaidView?.ResetProgressBar();

            stopwatch.Stop();
            SearchTimer.Stop();
        }

        private void ButtonReadRaids_Click(object sender, EventArgs e)
        {
            Task.Run(async () => await ReadRaidsAsync(Source.Token).ConfigureAwait(false), Source.Token);
        }

        private async Task ReadRaidsAsync(CancellationToken token)
        {
            if (IsReading)
            {
                ShowMessageBox("Please wait for the current RAM read to finish.");
                return;
            }

            ButtonEnable(new[] { ButtonViewRAM, ButtonAdvanceDate, ButtonDisconnect, ButtonDownloadEvents, SendScreenshot, ButtonReadRaids, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, false);
            try
            {
                await ReadRaids(token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ShowMessageBox($"Error occurred while reading raids: {ex.Message}");
            }

            ButtonEnable(new[] { ButtonViewRAM, ButtonAdvanceDate, ButtonDisconnect, ButtonDownloadEvents, SendScreenshot, ButtonReadRaids, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
        }

        private void ViewRAM_Click(object sender, EventArgs e)
        {
            if (IsReading)
            {
                ShowMessageBox("Please wait for the current RAM read to finish.");
                return;
            }

            ButtonEnable(new[] { ButtonViewRAM }, false);
            RaidBlockViewer window = default!;

            if (ConnectionWrapper is not null && ConnectionWrapper.Connected && ModifierKeys == Keys.Shift)
            {
                try
                {
                    var data = ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(RaidBlockOffsetBase, (int)RaidBlock.SIZE_BASE, Source.Token).Result;
                    window = new(data, RaidBlockOffsetBase);
                }
                catch (Exception ex)
                {
                    ButtonEnable(new[] { ButtonViewRAM }, true);
                    ShowMessageBox(ex.Message);
                    return;
                }
            }
            else if (RaidContainer.Container.Raids.Count > ComboIndex.SelectedIndex)
            {
                var data = RaidContainer.Container.Raids[ComboIndex.SelectedIndex].GetData();
                window = new(data, RaidBlockOffsetBase);
            }

            ShowDialogs(window);
            ButtonEnable(new[] { ButtonViewRAM }, true);
        }

        private void StopFilter_Click(object sender, EventArgs e)
        {
            var form = new FilterSettings(ref RaidFilters);
            ShowDialogs(form);
        }
        private void ResetRaids_Click(object sender, EventArgs e)
        {
            ResetRaids.Visible = false;
            SetTime.Visible = true;
            Task.Run(async () =>
            {
                try
                {
                    ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, false);
                    var DayseedOffset = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.RaidBlockPointerBase, CancellationToken.None).ConfigureAwait(false);
                    var dayseed = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(DayseedOffset, 8, CancellationToken.None).ConfigureAwait(false);
                    var dayseed_new = dayseed;
                    while (dayseed_new == dayseed)
                    {
                        await CloseGame(CancellationToken.None).ConfigureAwait(false);
                        await StartGameSetTime(CancellationToken.None).ConfigureAwait(false);
                        DayseedOffset = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.RaidBlockPointerBase, CancellationToken.None).ConfigureAwait(false);
                        dayseed_new = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(DayseedOffset, 8, CancellationToken.None).ConfigureAwait(false);
                    }
                    BaseBlockKeyPointer = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.BlockKeyPointer, CancellationToken.None).ConfigureAwait(false);
                    while (BaseBlockKeyPointer == 0)
                    {
                        await Task.Delay(0_100).ConfigureAwait(false);
                        BaseBlockKeyPointer = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.BlockKeyPointer, CancellationToken.None).ConfigureAwait(false);
                    }
                    RaidBlockOffsetBase = 0;
                    RaidBlockOffsetKitakami = 0;
                    RaidBlockOffsetBlueberry = 0;
                    UpdateStatus("Completed!");
                    ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not reset the date: " + ex.ToString());
                    SetTime.Enabled = false;
                    SetTime.Visible = false;
                    ResetRaids.Visible = true;
                    ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, CurrentTimeButton, DateTimeButton }, true);
                    return;
                }
            }
            , CancellationToken.None);
        }

        private void DownloadEvents_Click(object sender, EventArgs e)
        {
            if (ConnectionWrapper is null || !ConnectionWrapper.Connected)
                return;

            if (IsReading)
            {
                ShowMessageBox("Please wait for the current RAM read to finish.");
                return;
            }

            Task.Run(async () => { await DownloadEventsAsync(Source.Token).ConfigureAwait(false); }, Source.Token);
        }

        private async Task DownloadEventsAsync(CancellationToken token)
        {
            ButtonEnable(new[] { ButtonViewRAM, ButtonAdvanceDate, ButtonDisconnect, ButtonDownloadEvents, SendScreenshot, ButtonReadRaids, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton , DateTimeButton }, false);
            UpdateStatus("Reading event raid status...");

            try
            {
                await ReadEventRaids(token, true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ShowMessageBox($"Error occurred while reading event raids: {ex.Message}");
            }

            ButtonEnable(new[] { ButtonViewRAM, ButtonAdvanceDate, ButtonDisconnect, ButtonDownloadEvents, SendScreenshot, ButtonReadRaids, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton , DateTimeButton }, true);
            UpdateStatus("Completed!");
        }

        private void Seed_Click(object sender, EventArgs e)
        {
            if (ModifierKeys == Keys.Shift && RaidContainer.Container.Raids.Count > ComboIndex.SelectedIndex)
            {
                var raid = RaidContainer.Container.Raids[ComboIndex.SelectedIndex];
                Seed.Text = HideSeed ? $"{raid.Seed:X8}" : "Hidden";
                EC.Text = HideSeed ? $"{raid.EC:X8}" : "Hidden";
                PID.Text = (HideSeed ? $"{raid.PID:X8}" : "Hidden") + $"{(raid.IsShiny ? " (☆)" : string.Empty)}";
                HideSeed = !HideSeed;
                ActiveControl = null;
            }
        }

        private void ConfigSettings_Click(object sender, EventArgs e)
        {
            var form = new ConfigWindow(Config);
            ShowDialogs(form);
        }

        private void EnableFilters_Click(object sender, EventArgs e)
        {
            Config.EnableFilters = CheckEnableFilters.Checked;
        }

        private readonly JsonSerializerOptions options = new() { WriteIndented = true };
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            var configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            string output = JsonSerializer.Serialize(Config, options);
            using StreamWriter sw = new(configpath);
            sw.Write(output);

            if (ConnectionWrapper is not null && ConnectionWrapper.Connected)
            {
                try
                {
                    _ = ConnectionWrapper.DisconnectAsync(Source.Token).Result;
                }
                catch { }
            }

            Source.Cancel();
            DateAdvanceSource.Cancel();
            Source = new();
            DateAdvanceSource = new();
        }
        private async Task ReadOHKOState(CancellationToken token)
        {
            var pointer = await ParseDataBlockPointer(KBCATRaidEnemyArray, token).ConfigureAwait(false);
            var raw = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(pointer, KBCATRaidEnemyArray.Size, token).ConfigureAwait(false);
            const string path = "cache";
            var enemypath = Path.Combine(path, "raid_enemy_array");
            if (!File.Exists(enemypath))
                enemypath = Path.Combine(path, "raid_enemy_array_1_3_0");
            if (!File.Exists(enemypath))
                enemypath = Path.Combine(path, "raid_enemy_array_2_0_0");
            if (!File.Exists(enemypath))
                enemypath = Path.Combine(path, "raid_enemy_array_3_0_0");
            if (!File.Exists(enemypath))
            {
                OHKO.Text= "File not Found";
            }
            else
            {
                var tmp = File.ReadAllBytes(enemypath);
                if (tmp.SequenceEqual(raw))
                    OHKO.Text = "Enable OHKO";
                else
                    OHKO.Text = "Restore OHKO";
            }
        }
        private async Task ReadEventRaids(CancellationToken token, bool force = false)
        {
            var prio_file = Path.Combine(Directory.GetCurrentDirectory(), "cache", "raid_priority_array");
            if (File.Exists(prio_file))
            {
                (_, var version) = FlatbufferDumper.DumpDeliveryPriorities(File.ReadAllBytes(prio_file));
                var blk = await ConnectionWrapper.ReadBlockDefault(KBCATRaidPriorityArray, "raid_priority_array.tmp", true, token).ConfigureAwait(false);
                (_, var v2) = FlatbufferDumper.DumpDeliveryPriorities(blk);
                if (version != v2)
                    force = true;

                var tmp_file = Path.Combine(Directory.GetCurrentDirectory(), "cache", "raid_priority_array.tmp");
                if (File.Exists(tmp_file))
                    File.Delete(tmp_file);

                if (v2 == 0) // raid reset
                    return;
            }

            var delivery_raid_prio = await ConnectionWrapper.ReadBlockDefault(KBCATRaidPriorityArray, "raid_priority_array", force, token).ConfigureAwait(false);
            (var group_id, var priority) = FlatbufferDumper.DumpDeliveryPriorities(delivery_raid_prio);
            if (priority == 0)
                return;

            var delivery_raid_fbs = await ConnectionWrapper.ReadBlockDefault(Offsets.KBCATRaidEnemyArray, "raid_enemy_array", force, token).ConfigureAwait(false);
            var delivery_fixed_rewards = await ConnectionWrapper.ReadBlockDefault(KBCATFixedRewardItemArray, "fixed_reward_item_array", force, token).ConfigureAwait(false);
            var delivery_lottery_rewards = await ConnectionWrapper.ReadBlockDefault(KBCATLotteryRewardItemArray, "lottery_reward_item_array", force, token).ConfigureAwait(false);

            RaidContainer.DistTeraRaids = TeraDistribution.GetAllEncounters(delivery_raid_fbs);
            RaidContainer.MightTeraRaids = TeraMight.GetAllEncounters(delivery_raid_fbs);
            RaidContainer.DeliveryRaidPriority = group_id;
            RaidContainer.DeliveryRaidFixedRewards = FlatbufferDumper.DumpFixedRewards(delivery_fixed_rewards);
            RaidContainer.DeliveryRaidLotteryRewards = FlatbufferDumper.DumpLotteryRewards(delivery_lottery_rewards);
        }
        private static Image LayerOverImageShiny(Image baseImage, Shiny shiny)
        {
            // Add shiny star to top left of image.
            Bitmap rare;
            if (shiny is Shiny.AlwaysSquare)
                rare = PKHeX.Drawing.PokeSprite.Properties.Resources.rare_icon_alt_2;
            else
                rare = PKHeX.Drawing.PokeSprite.Properties.Resources.rare_icon_alt;
            rare = new Bitmap(rare, (int)(rare.Width * 1.5), (int)(rare.Height * 1.5));
            return ImageUtil.LayerImage(baseImage, rare, 0, 0, 0.7);
        }
        public async Task<bool> ApplyOHKOFile(string path, CancellationToken token)
        {
            UpdateStatus("Enabling One Hit Kill DamageOutput...");
            var raidEnemyBlock = File.ReadAllBytes(path);
            if (raidEnemyBlock.Length != KBCATRaidEnemyArray.Size)
            {
                UpdateStatus("Invalid File Aborted!");
                return false;
            }
            await WriteBlock(raidEnemyBlock, KBCATRaidEnemyArray, token).ConfigureAwait(false);
            return true;
        }
        public async Task<bool> RestoreOHKOFile(string path, CancellationToken token)
        {
            UpdateStatus("Restoring Original DamageOutput...");
            var raidEnemyBlock = File.ReadAllBytes(path);
            if (raidEnemyBlock.Length != KBCATRaidEnemyArray.Size)
            {
                UpdateStatus("Invalid File Aborted!");
                return false;
            }
            await WriteBlock(raidEnemyBlock, KBCATRaidEnemyArray, token).ConfigureAwait(false);
            await ConnectionWrapper.SaveGame(token).ConfigureAwait(false);
            await ReOpenGame(token).ConfigureAwait(false);
            await ParseBlockKeyPointer(token).ConfigureAwait(false);
            RaidBlockOffsetBase = 0;
            RaidBlockOffsetKitakami = 0;
            RaidBlockOffsetBlueberry = 0;
            return true;

        }
        private string OpenFiles(string path)
        {
            var strings = GenerateDictionary();
            if (path.Equals(""))
            {
                UpdateStatus("path is Empty. Select Files");
                var dialog = new OpenFileDialog
                {
                    Title = strings["ImportNews.Title"],
                    Filter = $"{strings["ImportNews.Filter"]} (*.*)|*.*",
                    FileName = strings["ImportNews.FolderSelection"],
                    ValidateNames = false,
                    CheckFileExists = false,
                    CheckPathExists = true,
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!Path.GetExtension(dialog.FileName).Equals(".zip"))
                        path = Path.GetDirectoryName(dialog.FileName)!;
                    else
                        path = dialog.FileName;
                }
                else
                {
                    return "";
                }
                return path;
            }
            return path;
        }
        private (bool, string, bool, string) FileSelect()
        {
            string pathdemo = "cache edit";
            string path = "";
            string pathdel = "";
            bool valid = false;
            var zip = false;
            path = OpenFiles(path);
            if (File.Exists(path))
            {
                if (Path.GetExtension(path).Equals(".zip"))
                {
                    var tmp = $"{Path.GetDirectoryName(path)}\\tmp";
                    ZipFile.ExtractToDirectory(path, tmp);
                    path = tmp;
                    zip = true;
                }
            }
            if (Directory.Exists(path))
            {
                if (zip)
                {
                    if (IsValidFolderRaid(path))
                        valid = true;
                }
                else
                {
                    if (IsValidFolderRaidNonZip(path))
                        valid = true;
                }
            }
            if (valid)
            {
                if (zip)
                {
                    pathdel = path;
                    path += "\\Files";
                }
                var index = Path.Combine(path, "raid_enemy_array_3_0_0");
                if (!File.Exists(index))
                    index = Path.Combine(path, "raid_enemy_array_2_0_0");
                if (!File.Exists(index))
                    index = Path.Combine(path, "raid_enemy_array_1_3_0");
                if (!File.Exists(index))
                    index = Path.Combine(path, "raid_enemy_array");
                path = index;
            }
            else
            {
                var indexpath = Path.Combine(pathdemo, "raid_enemy_array");
                if (!File.Exists(indexpath))
                    indexpath = Path.Combine(pathdemo, "raid_enemy_array_1_3_0");
                if (!File.Exists(indexpath))
                    indexpath = Path.Combine(pathdemo, "raid_enemy_array_2_0_0");
                if (!File.Exists(indexpath))
                    indexpath = Path.Combine(pathdemo, "raid_enemy_array_3_0_0");

                path = indexpath;
                if (File.Exists(path))
                    valid = true;
            }
            if (valid)
                return (valid, path, zip, pathdel);
            return (false, string.Empty, zip, pathdel);
        }
        private static bool IsValidFolderRaid(string path)
        {
            if (!File.Exists($"{path}\\Files\\raid_enemy_array") && !File.Exists($"{path}\\Files\\raid_enemy_array_1_3_0") && !File.Exists($"{path}\\Files\\raid_enemy_array_2_0_0") && !File.Exists($"{path}\\Files\\raid_enemy_array_3_0_0"))
                return false;

            return true;
        }
        private static bool IsValidFolderRaidNonZip(string path)
        {
            if (!File.Exists($"{path}\\raid_enemy_array") && !File.Exists($"{path}\\raid_enemy_array_1_3_0") && !File.Exists($"{path}\\raid_enemy_array_2_0_0") && !File.Exists($"{path}\\raid_enemy_array_3_0_0"))
                return false;

            return true;
        }
        private static void DeleteFilesAndDirectory(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
                DeleteFilesAndDirectory(dir);

            Directory.Delete(targetDir, false);
        }
        private static Dictionary<string, string> GenerateDictionary()
        {
            var strings = new Dictionary<string, string>
        {
            { "ImportNews.Title", "Open Poké Portal News Zip file or Folder" },
            { "ImportNews.Filter", "All files" },
            { "ImportNews.FolderSelection", "Folder Selection" },
            { "ImportNews.InvalidSelection", "Invalid file(s). Aborted." },
            { "ImportNews.Success", "Succesfully imported Raid Event" },
            { "ImportNews.Error", "Import error! Is the provided file valid?" },
        };
            return strings;
        }
        public async void ChageOHKO(object? sender, EventArgs e)
        {
            ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton , DateTimeButton }, false);
            var (valid, pathwrite, zip, pathdel) = FileSelect();
            if (!valid)
            {
                if (zip)
                    DeleteFilesAndDirectory(pathdel);
                UpdateStatus("Invalid File Selected!");
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton , DateTimeButton }, true);
                return;
            }
            var token = CancellationToken.None;
            var pointer = await ParseDataBlockPointer(KBCATRaidEnemyArray, token).ConfigureAwait(false);
            var raw = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(pointer, KBCATRaidEnemyArray.Size, token).ConfigureAwait(false);
            const string path = "cache";
            var enemypath = Path.Combine(path, "raid_enemy_array");
            if (!File.Exists(enemypath))
                enemypath = Path.Combine(path, "raid_enemy_array_1_3_0");
            if (!File.Exists(enemypath))
                enemypath = Path.Combine(path, "raid_enemy_array_2_0_0");
            if (!File.Exists(enemypath))
                enemypath = Path.Combine(path, "raid_enemy_array_3_0_0");
            if (!File.Exists(enemypath))
            {
                ShowMessageBox("Orignal Raid enemy array data is not found");
                return;
            }
            var tmp = File.ReadAllBytes(enemypath);
            if (tmp.SequenceEqual(raw))
            {
                var sucess = await ApplyOHKOFile(pathwrite, CancellationToken.None).ConfigureAwait(false);
                if (sucess)
                {
                    OHKO.Text = "Restore OHKO";
                    UpdateStatus("Completed!");
                }
            }
            else
            {
                var success = await RestoreOHKOFile(pathwrite, CancellationToken.None).ConfigureAwait(false);
                if (success)
                {
                    OHKO.Text = "Enable OHKO";
                    UpdateStatus("Completed!");
                }
            }
            if (zip)
                DeleteFilesAndDirectory(pathdel);
            ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton , DateTimeButton }, true);

        }


        private void DisplayRaid()
        {
            int index = ComboIndex.SelectedIndex;
            var raids = RaidContainer.Container.Raids;
            if (raids.Count > index)
            {
                Raid raid = raids[index];
                var encounter = RaidContainer.Container.Encounters[index];

                Seed.Text = !HideSeed ? $"{raid.Seed:X8}" : "Hidden";
                EC.Text = !HideSeed ? $"{raid.EC:X8}" : "Hidden";
                PID.Text = GetPIDString(raid, encounter);
                Area.Text = $"{Areas.GetArea((int)(raid.Area - 1), raid.MapParent)} - Den {raid.Den}";
                labelEvent.Visible = raid.IsEvent;

                var teratype = raid.GetTeraType(encounter);
                TeraType.Text = RaidContainer.Strings.types[teratype];

                int StarCount = encounter is TeraDistribution or TeraMight ? encounter.Stars : raid.GetStarCount(raid.Difficulty, Config.Progress, raid.IsBlack);
                Difficulty.Text = string.Concat(Enumerable.Repeat("☆", StarCount));

                var param = encounter.GetParam();
                var blank = new PK9
                {
                    Species = encounter.Species,
                    Form = encounter.Form
                };

                raid.GenerateDataPK9(blank, param, encounter.Shiny, raid.Seed);
                var imagestring = PokeImg(blank, false);
                PictureBox image = new();
                image.Load(imagestring);
                var img = ApplyTeraColor((byte)teratype, image.Image!, SpriteBackgroundType.BottomStripe);
                Shiny shiny = blank.IsShiny ? blank.ShinyXor == 0 ? Shiny.AlwaysSquare : Shiny.AlwaysStar : Shiny.Never;
                if (shiny.IsShiny())
                    img = LayerOverImageShiny(img, shiny);
                var form = ShowdownParsing.GetStringFromForm(encounter.Form, RaidContainer.Strings, encounter.Species, EntityContext.Gen9);
                if (form.Length > 0 && form[0] != '-')
                    form = form.Insert(0, "-");

                Species.Text = $"{RaidContainer.Strings.Species[encounter.Species]}{form}";
                Sprite.Image = img;
                GemIcon.Image = GetDisplayGemImage(teratype, raid);
                Gender.Text = $"{(Gender)blank.Gender}";

                var nature = blank.Nature;
                Nature.Text = $"{RaidContainer.Strings.Natures[(int)nature]}";
                Ability.Text = $"{RaidContainer.Strings.Ability[blank.Ability]}";

                var extra_moves = new ushort[] { 0, 0, 0, 0 };
                for (int i = 0; i < encounter.ExtraMoves.Length; i++)
                {
                    if (i < extra_moves.Length)
                        extra_moves[i] = encounter.ExtraMoves[i];
                }

                Move1.Text = ShowExtraMoves ? RaidContainer.Strings.Move[extra_moves[0]] : RaidContainer.Strings.Move[encounter.Move1];
                Move2.Text = ShowExtraMoves ? RaidContainer.Strings.Move[extra_moves[1]] : RaidContainer.Strings.Move[encounter.Move2];
                Move3.Text = ShowExtraMoves ? RaidContainer.Strings.Move[extra_moves[2]] : RaidContainer.Strings.Move[encounter.Move3];
                Move4.Text = ShowExtraMoves ? RaidContainer.Strings.Move[extra_moves[3]] : RaidContainer.Strings.Move[encounter.Move4];

                PokemonScale.Text = $"{PokeSizeDetailedUtil.GetSizeRating(blank.Scale)}";
                PokemonScaleValue.Text = $"{blank.Scale}";

                Span<int> _ivs = stackalloc int[6];
                blank.GetIVs(_ivs);
                IVs.Text = IVsString(Utils.ToSpeedLast(_ivs));
                toolTip.SetToolTip(IVs, IVsString(Utils.ToSpeedLast(_ivs), true));

                PID.BackColor = raid.CheckIsShiny(encounter) ? Color.Gold : DefaultColor;
                IVs.BackColor = blank.FlawlessIVCount == 6 ? Color.YellowGreen : blank.IV_ATK == 0 && blank.FlawlessIVCount == 5 ? Color.HotPink : blank.IV_SPE == 0 && blank.FlawlessIVCount == 5 ? Color.Azure : blank.FlawlessIVCount == 4 && blank.IV_ATK == 0 && blank.IV_SPE == 0 ? Color.Gold : DefaultColor;
            }
            else { ShowMessageBox($"Unable to display raid at index {index}. Ensure there are no cheats running or anything else that might shift RAM (Edizon, overlays, etc.), then reboot your console and try again."); }
        }

        private static Image? GetDisplayGemImage(int teratype, Raid raid)
        {
            var display_black = raid.IsBlack || raid.Flags == 3;
            var baseImg = display_black ? (Image?)Properties.Resources.ResourceManager.GetObject($"black_{teratype:D2}")
                                        : (Image?)Properties.Resources.ResourceManager.GetObject($"gem_{teratype:D2}");
            if (baseImg is null)
                return null;

            var backlayer = new Bitmap(baseImg.Width + 10, baseImg.Height + 10, baseImg.PixelFormat);
            baseImg = ImageUtil.LayerImage(backlayer, baseImg, 5, 5);
            var pixels = ImageUtil.GetPixelData((Bitmap)baseImg);
            for (int i = 0; i < pixels.Length; i += 4)
            {
                if (pixels[i + 3] == 0)
                {
                    pixels[i] = 0;
                    pixels[i + 1] = 0;
                    pixels[i + 2] = 0;
                }
            }

            baseImg = ImageUtil.GetBitmap(pixels, baseImg.Width, baseImg.Height, baseImg.PixelFormat);
            if (display_black)
            {
                var color = Color.Indigo;
                SpriteUtil.GetSpriteGlow(baseImg, color.B, color.G, color.R, out var glow, false);
                baseImg = ImageUtil.LayerImage(ImageUtil.GetBitmap(glow, baseImg.Width, baseImg.Height, baseImg.PixelFormat), baseImg, 0, 0);
            }
            else if (raid.IsEvent)
            {
                var color = Color.DarkTurquoise;
                SpriteUtil.GetSpriteGlow(baseImg, color.B, color.G, color.R, out var glow, false);
                baseImg = ImageUtil.LayerImage(ImageUtil.GetBitmap(glow, baseImg.Width, baseImg.Height, baseImg.PixelFormat), baseImg, 0, 0);
            }
            return baseImg;
        }

        private void DisplayPrettyRaid()
        {
            if (teraRaidView is null)
            {
                ShowMessageBox("Something went terribly wrong: teraRaidView is not initialized.");
                return;
            }

            int index = ComboIndex.SelectedIndex;
            var raids = RaidContainer.Container.Raids;
            if (raids.Count > index)
            {
                Raid raid = raids[index];
                var encounter = RaidContainer.Container.Encounters[index];

                teraRaidView.Area.Text = $"{Areas.GetArea((int)(raid.Area - 1), raid.MapParent)} - Den {raid.Den}";

                var teratype = raid.GetTeraType(encounter);
                teraRaidView.TeraType.Image = (Bitmap)Properties.Resources.ResourceManager.GetObject("gem_text_" + teratype)!;

                int StarCount = encounter is TeraDistribution ? encounter.Stars : raid.GetStarCount(raid.Difficulty, Config.Progress, raid.IsBlack);
                teraRaidView.Difficulty.Text = string.Concat(Enumerable.Repeat("⭐", StarCount));

                if (encounter is not null)
                {
                    var param = encounter.GetParam();
                    var blank = new PK9
                    {
                        Species = encounter.Species,
                        Form = encounter.Form
                    };

                    raid.GenerateDataPK9(blank, param, encounter.Shiny, raid.Seed);
                    var img = blank.Sprite();

                    teraRaidView.picBoxPokemon.Image = img;
                    var form = Utils.GetFormString(blank.Species, blank.Form, raid.Strings);

                    teraRaidView.Species.Text = $"{RaidContainer.Strings.Species[encounter.Species]}{form}";
                    teraRaidView.Gender.Text = $"{(Gender)blank.Gender}";

                    var nature = blank.Nature;
                    teraRaidView.Nature.Text = $"{RaidContainer.Strings.Natures[(int)nature]}";
                    teraRaidView.Ability.Text = $"{RaidContainer.Strings.Ability[blank.Ability]}";

                    teraRaidView.Move1.Text = encounter.Move1 > 0 ? RaidContainer.Strings.Move[encounter.Move1] : "---";
                    teraRaidView.Move2.Text = encounter.Move2 > 0 ? RaidContainer.Strings.Move[encounter.Move2] : "---";
                    teraRaidView.Move3.Text = encounter.Move3 > 0 ? RaidContainer.Strings.Move[encounter.Move3] : "---";
                    teraRaidView.Move4.Text = encounter.Move4 > 0 ? RaidContainer.Strings.Move[encounter.Move4] : "---";

                    teraRaidView.PokemonScale.Text = $"{PokeSizeDetailedUtil.GetSizeRating(blank.Scale)}";

                    var length = encounter.ExtraMoves.Length < 4 ? 4 : encounter.ExtraMoves.Length;
                    var extra_moves = new ushort[length];
                    for (int i = 0; i < encounter.ExtraMoves.Length; i++)
                        extra_moves[i] = encounter.ExtraMoves[i];

                    teraRaidView.Move5.Text = extra_moves[0] > 0 ? RaidContainer.Strings.Move[extra_moves[0]] : "---";
                    teraRaidView.Move6.Text = extra_moves[1] > 0 ? RaidContainer.Strings.Move[extra_moves[1]] : "---";
                    teraRaidView.Move7.Text = extra_moves[2] > 0 ? RaidContainer.Strings.Move[extra_moves[2]] : "---";
                    teraRaidView.Move8.Text = extra_moves[3] > 0 ? RaidContainer.Strings.Move[extra_moves[3]] : "---";

                    Span<int> _ivs = stackalloc int[6];
                    blank.GetIVs(_ivs);
                    var ivs = Utils.ToSpeedLast(_ivs);
                    // HP
                    teraRaidView.HP.Text = $"{ivs[0]:D2}";
                    teraRaidView.HP.BackColor = Color.FromArgb(0, 5, 25);
                    if (teraRaidView.HP.Text is "31")
                        teraRaidView.HP.BackColor = Color.ForestGreen;
                    else if (teraRaidView.HP.Text is "00")
                        teraRaidView.HP.BackColor = Color.DarkRed;

                    // ATK
                    teraRaidView.ATK.Text = $"{ivs[1]:D2}";
                    teraRaidView.ATK.BackColor = Color.FromArgb(0, 5, 25);
                    if (teraRaidView.ATK.Text is "31")
                        teraRaidView.ATK.BackColor = Color.ForestGreen;
                    else if (teraRaidView.ATK.Text is "00")
                        teraRaidView.ATK.BackColor = Color.DarkRed;

                    // DEF
                    teraRaidView.DEF.Text = $"{ivs[2]:D2}";
                    teraRaidView.DEF.BackColor = Color.FromArgb(0, 5, 25);
                    if (teraRaidView.DEF.Text is "31")
                        teraRaidView.DEF.BackColor = Color.ForestGreen;
                    else if (teraRaidView.DEF.Text is "00")
                        teraRaidView.DEF.BackColor = Color.DarkRed;

                    // SPA
                    teraRaidView.SPA.Text = $"{ivs[3]:D2}";
                    teraRaidView.SPA.BackColor = Color.FromArgb(0, 5, 25);
                    if (teraRaidView.SPA.Text is "31")
                        teraRaidView.SPA.BackColor = Color.ForestGreen;
                    else if (teraRaidView.SPA.Text is "00")
                        teraRaidView.SPA.BackColor = Color.DarkRed;

                    // SPD
                    teraRaidView.SPD.Text = $"{ivs[4]:D2}";
                    teraRaidView.SPD.BackColor = Color.FromArgb(0, 5, 25);
                    if (teraRaidView.SPD.Text is "31")
                        teraRaidView.SPD.BackColor = Color.ForestGreen;
                    else if (teraRaidView.SPD.Text is "00")
                        teraRaidView.SPD.BackColor = Color.DarkRed;

                    // SPEED
                    teraRaidView.SPEED.Text = $"{ivs[5]:D2}";
                    teraRaidView.SPEED.BackColor = Color.FromArgb(0, 5, 25);
                    if (teraRaidView.SPEED.Text is "31")
                        teraRaidView.SPEED.BackColor = Color.ForestGreen;
                    else if (teraRaidView.SPEED.Text is "00")
                        teraRaidView.SPEED.BackColor = Color.DarkRed;


                    var map = GenerateMap(raid, teratype);
                    if (map is null)
                        ShowMessageBox("Error generating map.");
                    teraRaidView.Map.Image = map;

                    // Rewards
                    var rewards = RaidContainer.Container.Rewards[index];

                    teraRaidView.textAbilityPatch.Text = "0";
                    teraRaidView.textAbilityPatch.ForeColor = Color.DimGray;
                    teraRaidView.labelAbilityPatch.ForeColor = Color.DimGray;

                    teraRaidView.textAbilityCapsule.Text = "0";
                    teraRaidView.textAbilityCapsule.ForeColor = Color.DimGray;
                    teraRaidView.labelAbilityCapsule.ForeColor = Color.DimGray;

                    teraRaidView.textBottleCap.Text = "0";
                    teraRaidView.textBottleCap.ForeColor = Color.DimGray;
                    teraRaidView.labelBottleCap.ForeColor = Color.DimGray;

                    teraRaidView.textSweetHerba.Text = "0";
                    teraRaidView.textSweetHerba.ForeColor = Color.DimGray;
                    teraRaidView.labelSweetHerba.ForeColor = Color.DimGray;

                    teraRaidView.textSaltyHerba.Text = "0";
                    teraRaidView.textSaltyHerba.ForeColor = Color.DimGray;
                    teraRaidView.labelSaltyHerba.ForeColor = Color.DimGray;

                    teraRaidView.textBitterHerba.Text = "0";
                    teraRaidView.textBitterHerba.ForeColor = Color.DimGray;
                    teraRaidView.labelBitterHerba.ForeColor = Color.DimGray;

                    teraRaidView.textSourHerba.Text = "0";
                    teraRaidView.textSourHerba.ForeColor = Color.DimGray;
                    teraRaidView.labelSourHerba.ForeColor = Color.DimGray;

                    teraRaidView.textSpicyHerba.Text = "0";
                    teraRaidView.textSpicyHerba.ForeColor = Color.DimGray;
                    teraRaidView.labelSpicyHerba.ForeColor = Color.DimGray;

                    for (int i = 0; i < rewards.Count; i++)
                    {
                        if (rewards[i].Item1 == 645)
                        {
                            teraRaidView.textAbilityCapsule.Text = (int.Parse(teraRaidView.textAbilityCapsule.Text) + 1).ToString();
                            teraRaidView.textAbilityCapsule.ForeColor = Color.White;
                            teraRaidView.labelAbilityCapsule.ForeColor = Color.WhiteSmoke;
                        }
                        if (rewards[i].Item1 == 795)
                        {
                            teraRaidView.textBottleCap.Text = (int.Parse(teraRaidView.textBottleCap.Text) + 1).ToString();
                            teraRaidView.textBottleCap.ForeColor = Color.White;
                            teraRaidView.labelBottleCap.ForeColor = Color.WhiteSmoke;
                        }
                        if (rewards[i].Item1 == 1606)
                        {
                            teraRaidView.textAbilityPatch.Text = (int.Parse(teraRaidView.textAbilityPatch.Text) + 1).ToString();
                            teraRaidView.textAbilityPatch.ForeColor = Color.White;
                            teraRaidView.labelAbilityPatch.ForeColor = Color.WhiteSmoke;
                        }
                        if (rewards[i].Item1 == 1904)
                        {
                            teraRaidView.textSweetHerba.Text = (int.Parse(teraRaidView.textSweetHerba.Text) + 1).ToString();
                            teraRaidView.textSweetHerba.ForeColor = Color.White;
                            teraRaidView.labelSweetHerba.ForeColor = Color.WhiteSmoke;
                        }
                        if (rewards[i].Item1 == 1905)
                        {
                            teraRaidView.textSaltyHerba.Text = (int.Parse(teraRaidView.textSaltyHerba.Text) + 1).ToString();
                            teraRaidView.textSaltyHerba.ForeColor = Color.White;
                            teraRaidView.labelSaltyHerba.ForeColor = Color.WhiteSmoke;
                        }
                        if (rewards[i].Item1 == 1906)
                        {
                            teraRaidView.textSourHerba.Text = (int.Parse(teraRaidView.textSourHerba.Text) + 1).ToString();
                            teraRaidView.textSourHerba.ForeColor = Color.White;
                            teraRaidView.labelSourHerba.ForeColor = Color.WhiteSmoke;
                        }
                        if (rewards[i].Item1 == 1907)
                        {
                            teraRaidView.textBitterHerba.Text = (int.Parse(teraRaidView.textBitterHerba.Text) + 1).ToString();
                            teraRaidView.textBitterHerba.ForeColor = Color.White;
                            teraRaidView.labelBitterHerba.ForeColor = Color.WhiteSmoke;
                        }
                        if (rewards[i].Item1 == 1908)
                        {
                            teraRaidView.textSpicyHerba.Text = (int.Parse(teraRaidView.textSpicyHerba.Text) + 1).ToString();
                            teraRaidView.textSpicyHerba.ForeColor = Color.White;
                            teraRaidView.labelSpicyHerba.ForeColor = Color.WhiteSmoke;
                        }
                    }

                    var shiny = raid.CheckIsShiny(encounter);
                    teraRaidView.Shiny.Visible = shiny;
                    teraRaidView.picShinyAlert.Enabled = shiny;
                }
                else
                {
                    // TODO Clear all the fields.
                }
            }
            else { ShowMessageBox($"Unable to display raid at index {index}. Ensure there are no cheats running or anything else that might shift RAM (Edizon, overlays, etc.), then reboot your console and try again."); }
        }

        private string GetPIDString(Raid raid, ITeraRaid? enc)
        {
            if (HideSeed)
                return "Hidden";

            var shiny_mark = " (☆)";
            var pid = $"{raid.PID:X8}";
            return raid.CheckIsShiny(enc) ? pid + shiny_mark : pid;
        }

        private static string IVsString(int[] ivs, bool verbose = false)
        {
            string s = string.Empty;
            var stats = new[] { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };
            for (int i = 0; i < ivs.Length; i++)
            {
                s += $"{ivs[i]:D2}{(verbose ? " " + stats[i] : string.Empty)}";
                if (i < 5)
                    s += "/";
            }
            return s;
        }

        private static Image ApplyTeraColor(byte elementalType, Image img, SpriteBackgroundType type)
        {
            var color = TypeColor.GetTypeSpriteColor(elementalType);
            var thk = SpriteBuilder.ShowTeraThicknessStripe;
            var op = SpriteBuilder.ShowTeraOpacityStripe;
            var bg = SpriteBuilder.ShowTeraOpacityBackground;
            return ApplyColor(img, type, color, thk, op, bg);
        }

        private static Image ApplyColor(Image img, SpriteBackgroundType type, Color color, int thick, byte opacStripe, byte opacBack)
        {
            if (type == SpriteBackgroundType.BottomStripe)
            {
                int stripeHeight = thick; // from bottom
                if ((uint)stripeHeight > img.Height) // clamp negative & too-high values back to height.
                    stripeHeight = img.Height;

                return ImageUtil.BlendTransparentTo(img, color, opacStripe, img.Width * 4 * (img.Height - stripeHeight));
            }
            if (type == SpriteBackgroundType.TopStripe)
            {
                int stripeHeight = thick; // from top
                if ((uint)stripeHeight > img.Height) // clamp negative & too-high values back to height.
                    stripeHeight = img.Height;

                return ImageUtil.BlendTransparentTo(img, color, opacStripe, 0, (img.Width * 4 * stripeHeight) - 4);
            }
            if (type == SpriteBackgroundType.FullBackground) // full background
                return ImageUtil.BlendTransparentTo(img, color, opacBack);
            return img;
        }

        private static Image? GenerateMap(Raid raid, int teratype)
        {
            var original = PKHeX.Drawing.Misc.TypeSpriteUtil.GetTypeSpriteGem((byte)teratype);
            if (original is null)
                return null;

            var gem = new Bitmap(original, new Size(30, 30));
            SpriteUtil.GetSpriteGlow(gem, 0xFF, 0xFF, 0xFF, out var glow, true);
            gem = ImageUtil.LayerImage(gem, ImageUtil.GetBitmap(glow, gem.Width, gem.Height, gem.PixelFormat), 0, 0);
            if (den_locations_base is null || den_locations_base.Count == 0 || den_locations_kitakami is null || den_locations_kitakami.Count == 0 || den_locations_blueberry is null || den_locations_blueberry.Count == 0)
                return null;

            var locData = raid.MapParent switch
            {
                TeraRaidMapParent.Paldea => den_locations_base,
                TeraRaidMapParent.Kitakami => den_locations_kitakami,
                _ => den_locations_blueberry,
            };
            var map = raid.MapParent switch
            {
                TeraRaidMapParent.Paldea => map_base,
                TeraRaidMapParent.Kitakami => map_kitakami,
                _ => map_blueberry,
            };
            try
            {
                (double x, double z) = GetCoordinate(raid, locData, gem);
                return ImageUtil.LayerImage(map, gem, (int)x, (int)z);
            }
            catch { return null; }
        }
        private static (double x, double z) GetCoordinate(Raid raid, IReadOnlyDictionary<string, float[]> locData, Bitmap gem)
        {
            var m = MapMagic.GetMapMagic(raid.MapParent);
            double x = m.ConvertX(locData[$"{raid.Area}-{raid.LotteryGroup}-{raid.Den}"][0]) - (gem.Size.Width / 2);
            double z = m.ConvertZ(locData[$"{raid.Area}-{raid.LotteryGroup}-{raid.Den}"][2]) - (gem.Size.Height / 2);

            return (x, z);
        }
        private static (string x, string y, string z) GerRealCoords(Raid raid, IReadOnlyDictionary<string, float[]> LocData)
        {
            string x = $"{(LocData[$"{raid.Area}-{raid.LotteryGroup}-{raid.Den}"][0])}";
            string y = $"{(LocData[$"{raid.Area}-{raid.LotteryGroup}-{raid.Den}"][1])}";
            string z = $"{(LocData[$"{raid.Area}-{raid.LotteryGroup}-{raid.Den}"][2])}";
            return (x, y, z);
        }
        private async Task TeleportToDen(Raid raid)
        {
            if (den_locations_base is null || den_locations_base.Count == 0 || den_locations_kitakami is null || den_locations_kitakami.Count == 0 || den_locations_blueberry is null || den_locations_blueberry.Count == 0)
            {
                ShowMessageBox("Den Location Data is null!");
                return;
            }

            var locData = raid.MapParent switch
            {
                TeraRaidMapParent.Paldea => den_locations_base,
                TeraRaidMapParent.Kitakami => den_locations_kitakami,
                _ => den_locations_blueberry,
            };
            try
            {
                (string x, string y, string z) = GerRealCoords(raid, locData);
                if(string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y) || string.IsNullOrEmpty(z))
                {
                    ShowMessageBox("Coord is Empty", "Invalid Coords");
                    return;
                }
                if(!Single.TryParse(x, out _) || !Single.TryParse(y, out _) || !Single.TryParse(z, out _))
                {
                    ShowMessageBox("Coords Format is invalid ", "Invalid Coords Format");
                    return;
                }
                await CollideToSpot(x, y, z, CancellationToken.None).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                ShowMessageBox(ex.ToString(), "Teleport Error");
                return;
            }
        }

        public async Task CollideToSpot(string x, string Y, string z, CancellationToken token)
        {
            PlayerOnMountOffset = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.PlayerOnMountPointer, token).ConfigureAwait(false);
            while (PlayerOnMountOffset == 0)
            {
                await Task.Delay(1_000).ConfigureAwait(false);
                PlayerOnMountOffset = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.PlayerOnMountPointer, token).ConfigureAwait(false);
            }
            var checkcountprev = 0;
            if (await PlayerNotOnMount(token).ConfigureAwait(false))
                await Click(SwitchButton.PLUS, 0_800, token).ConfigureAwait(false);
            while (await PlayerNotOnMount(token).ConfigureAwait(false))
            {
                await Task.Delay(1_000).ConfigureAwait(false);
                await Click(SwitchButton.PLUS, 0_800, token).ConfigureAwait(false);
                checkcountprev++;
                if (!await PlayerNotOnMount(token).ConfigureAwait(false))
                    break;
                if (checkcountprev >= 2)
                    await Click(SwitchButton.B, 0_800, token).ConfigureAwait(false);
            }
            float coordx = Single.Parse(x, NumberStyles.Float);
            byte[] X1 = BitConverter.GetBytes(coordx);
            float coordy = Single.Parse(Y, NumberStyles.Float);
            byte[] Y1 = BitConverter.GetBytes(coordy);
            float coordz = Single.Parse(z, NumberStyles.Float);
            byte[] Z1 = BitConverter.GetBytes(coordz);

            X1 = X1.Concat(Y1).Concat(Z1).ToArray();
            float y = BitConverter.ToSingle(X1, 4);
            y += 200;
            WriteSingleLittleEndian(X1.AsSpan()[4..], y);

            for (int i = 0; i < 15; i++)
                await ConnectionWrapper.Connection.PointerPoke(X1, ConnectionWrapper.CollisionPointer, token).ConfigureAwait(false);

            await Task.Delay(3_000).ConfigureAwait(false);
            await Click(SwitchButton.B, 10_000, token).ConfigureAwait(false);

            await Task.Delay(15_000).ConfigureAwait(false);
            await Click(SwitchButton.PLUS, 0_800, token).ConfigureAwait(false);
            var checkcount = 0;
            while (!await PlayerNotOnMount(token).ConfigureAwait(false))
            {
                await Task.Delay(1_000).ConfigureAwait(false);
                await Click(SwitchButton.PLUS, 0_800, token).ConfigureAwait(false);
                checkcount++;
                if (checkcount >= 2)
                    break;
            }
            await SetStick(SwitchStick.LEFT, 0, 10000, 0_050, token).ConfigureAwait(false);
            await SetStick(SwitchStick.LEFT, 0, 0, 0_500, token).ConfigureAwait(false);
            MessageBox.Show("Teleport Completed!");
        }
        private async Task<bool> PlayerNotOnMount(CancellationToken token)
        {
            var Data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(PlayerOnMountOffset, 1, token).ConfigureAwait(false);
            return Data[0] == 0x00; // 0 nope else yes
        }

        private new async Task Click(SwitchButton b, int delay, CancellationToken token)
        {
            await ConnectionWrapper.Connection.SendAsync(SwitchCommand.Click(b, true), token).ConfigureAwait(false);
            await Task.Delay(delay, token).ConfigureAwait(false);
        }
        private async Task SetStick(SwitchStick stick, short x, short y, int delay, CancellationToken token)
        {
            var cmd = SwitchCommand.SetStick(stick, x, y, true);
            await ConnectionWrapper.Connection.SendAsync(cmd, token).ConfigureAwait(false);
            await Task.Delay(delay, token).ConfigureAwait(false);
        }

        private bool StopAdvanceDate(List<uint> previousSeeds)
        {
            var raids = RaidContainer.Container.Raids;
            var curSeeds = raids.Select(x => x.Seed).ToArray();
            var sameraids = curSeeds.Except(previousSeeds).ToArray().Length == 0;

            StatDaySkipTries++;
            if (sameraids)
                return false;

            StatDaySkipSuccess++;
            if (!Config.EnableFilters)
                return true;

            for (int i = 0; i < RaidFilters.Count; i++)
            {
                var index = 0;
                if (InvokeRequired)
                    index = Invoke(() => { return RaidBoost.SelectedIndex; });
                else index = RaidBoost.SelectedIndex;

                var encounters = RaidContainer.Container.Encounters;
                if (RaidFilters[i].FilterSatisfied(encounters, raids, index))
                    return true;
            }

            return StopAdvances;
        }

        private async Task ReadRaids(CancellationToken token)
        {
            if(Config is { PaldeaScan: false, KitakamiScan: false, BlueberryScan: false})
            {
                ShowMessageBox("Please select a location to scan in your General Settings.", "No locations selected");
                return;
            }
            /*if (File.Exists("Might9Raids.txt"))
                File.Delete("Might9Raids.txt");
            if (File.Exists("Dist9Raids.txt"))
                File.Delete("Dist9Raids.txt");
            if (File.Exists("TeraRaids Paldea.txt"))
                File.Delete("TeraRaids Paldea.txt");
            if (File.Exists("TeraRaids Kitakami.txt"))
                File.Delete("TeraRaids Kitakami.txt");
            if (File.Exists("TeraRaids Blueberry.txt"))
                File.Delete("TeraRaids Blueberry.txt");
            if (File.Exists("Event Raid Group IDs.txt"))
                File.Delete("Event Raid Group IDs.txt");
            if (File.Exists("Raid Rewards Dist9.txt"))
                File.Delete("Raid Rewards Dist9.txt");
            if (File.Exists("Raid Rewards Might9.txt"))
                File.Delete("Raid Rewards Might9.txt");
            if (File.Exists("Raid Rewards Normal.txt"))
                File.Delete("Raid Rewards Normal.txt");*/

            if (RaidBlockOffsetBase == 0)
            {
                UpdateStatus("Caching the raid block pointers...");
                RaidBlockOffsetBase = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.RaidBlockPointerBase, token).ConfigureAwait(false);
                RaidBlockOffsetKitakami = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.RaidBlockPointerKitakami, token).ConfigureAwait(false);
                RaidBlockOffsetBlueberry = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.RaidBlockPointerBlueberry.ToArray(), token).ConfigureAwait(false);
            }

            RaidContainer.Container.ClearRaids();
            RaidContainer.Container.ClearEncounters();
            RaidContainer.Container.ClearRewards();

            uint ValidCount = 0;
            var msg = string.Empty;
            int delivery,
                enc;

            if (Config.PaldeaScan)
            {
                ValidCount += RaidBlock.MAX_COUNT_BASE;
                UpdateStatus("Reading Paldea raid block...");
                msg = string.Empty;
                var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(RaidBlockOffsetBase + RaidBlock.HEADER_SIZE, (int)RaidBlock.SIZE_BASE, token).ConfigureAwait(false);
                (delivery, enc) = RaidContainer.ReadAllRaids(data, Config.Progress, Config.EventProgress, GetRaidBoost(), TeraRaidMapParent.Paldea);
                if (enc > 0)
                    msg += $"Failed to find encounters for {enc} raid(s).\n";

                if (delivery > 0)
                    msg += $"Invalid delivery group ID for {delivery} raid(s). Try deleting the \"cache\" folder.\n";

                if (msg != string.Empty)
                {
                    msg += $"\nMore info can be found in the \"raid_dbg_{TeraRaidMapParent.Paldea}.txt\" file.";
                    ShowMessageBox(msg, "Raid Read Error");
                }
            }
            var raids = RaidContainer.Container.Raids;
            var encounters = RaidContainer.Container.Encounters;
            var rewards = RaidContainer.Container.Rewards;
            RaidContainer.Container.ClearRaids();
            RaidContainer.Container.ClearEncounters();
            RaidContainer.Container.ClearRewards();

            // Kitakami
            if (Config.KitakamiScan)
            {
                ValidCount += RaidBlock.MAX_COUNT_KITAKAMI;
                UpdateStatus("Reading Kitakami raid block...");
                var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(RaidBlockOffsetKitakami, (int)RaidBlock.SIZE_KITAKAMI, token).ConfigureAwait(false);

                msg = string.Empty;
                (delivery, enc) = RaidContainer.ReadAllRaids(data, Config.Progress, Config.EventProgress, GetRaidBoost(), TeraRaidMapParent.Kitakami);
                if (enc > 0)
                    msg += $"Failed to find encounters for {enc} raid(s).\n";

                if (delivery > 0)
                    msg += $"Invalid delivery group ID for {delivery} raid(s). Try deleting the \"cache\" folder.\n";

                if (msg != string.Empty)
                {
                    msg += $"\nMore info can be found in the \"raid_dbg_{TeraRaidMapParent.Kitakami}.txt\" file.";
                    ShowMessageBox(msg, "Raid Read Error");
                }
            }
            var allRaids = raids.Concat(RaidContainer.Container.Raids).ToList().AsReadOnly();
            var allEncounters = encounters.Concat(RaidContainer.Container.Encounters).ToList().AsReadOnly();
            var allRewards = rewards.Concat(RaidContainer.Container.Rewards).ToList().AsReadOnly();
            RaidContainer.Container.ClearRaids();
            RaidContainer.Container.ClearEncounters();
            RaidContainer.Container.ClearRewards();

            if (Config.BlueberryScan)
            {
                ValidCount += RaidBlock.MAX_COUNT_BLUEBERRY;
                UpdateStatus("Reading Blueberry raid block...");
                var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(RaidBlockOffsetBlueberry, (int)RaidBlock.SIZE_BLUEBERRY, token).ConfigureAwait(false);

                msg = string.Empty;
                (delivery, enc) = RaidContainer.ReadAllRaids(data, Config.Progress, Config.EventProgress, GetRaidBoost(), TeraRaidMapParent.Blueberry);
                if (enc > 0)
                    msg += $"Failed to find encounters for {enc} raid(s).\n";

                if (delivery > 0)
                    msg += $"Invalid delivery group ID for {delivery} raid(s). Try deleting the \"cache\" folder.\n";

                if (msg != string.Empty)
                {
                    msg += $"\nMore info can be found in the \"raid_dbg_{TeraRaidMapParent.Blueberry}.txt\" file.";
                    ShowMessageBox(msg, "Raid Read Error");
                }
            }
            allRaids = allRaids.Concat(RaidContainer.Container.Raids).ToList().AsReadOnly();
            allEncounters = allEncounters.Concat(RaidContainer.Container.Encounters).ToList().AsReadOnly();
            allRewards = allRewards.Concat(RaidContainer.Container.Rewards).ToList().AsReadOnly();

            RaidContainer.Container.SetRaids(allRaids);
            RaidContainer.Container.SetEncounters(allEncounters);
            RaidContainer.Container.SetRewards(allRewards);

            if(!FirstConnect)
                UpdateStatus("Completed!");

            var filterMatchCount = Enumerable.Range(0, allRaids.Count).Count(c => RaidFilters.Any(z => z.FilterSatisfied(allEncounters[c], allRaids[c], GetRaidBoost())));
            if (InvokeRequired)
                Invoke(() => { LabelLoadedRaids.Text = $"Matches: {filterMatchCount}"; });
            else LabelLoadedRaids.Text = $"Matches: {filterMatchCount}";

            if (allRaids.Count > ValidCount)
            {
                ButtonEnable(new[] { ButtonPrevious, ButtonNext }, false);
                ShowMessageBox("Bad read, ensure there are no cheats running or anything else that might shift RAM (Edizon, overlays, etc.), then reboot your console and try again.");
                return;
            }
            if (allRaids.Count > 0)
            {
                ButtonEnable(new[] { ButtonPrevious, ButtonNext }, true);
                var dataSource = Enumerable.Range(0, allRaids.Count).Select(z => $"{z + 1:D} / {allRaids.Count:D}").ToArray();
                if (InvokeRequired)
                    Invoke(() => { ComboIndex.DataSource = dataSource; });
                else ComboIndex.DataSource = dataSource;

                if (InvokeRequired)
                    Invoke(() => { ComboIndex.SelectedIndex = ComboIndex.SelectedIndex < allRaids.Count ? ComboIndex.SelectedIndex : 0; });
                else ComboIndex.SelectedIndex = ComboIndex.SelectedIndex < allRaids.Count ? ComboIndex.SelectedIndex : 0;

            }
            else
            {
                ButtonEnable(new[] { ButtonPrevious, ButtonNext }, false);
                ShowMessageBox("Bad read, ensure there are no cheats running or anything else that might shift RAM (Edizon, overlays, etc.), then reboot your console and try again.");
            }
        }

        public void Game_SelectedIndexChanged(string name)
        {
            Config.Game = name;
            RaidContainer.SetGame(name);
            if (RaidContainer.Container.Raids.Count > 0)
                DisplayRaid();
        }
        private async Task ParseBlockKeyPointer(CancellationToken token)
        {
            BaseBlockKeyPointer = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.BlockKeyPointer, token).ConfigureAwait(false);
            while (BaseBlockKeyPointer == 0)
            {
                UpdateStatus("Waiting for the right pointer...");
                await Task.Delay(0_100).ConfigureAwait(false);
                BaseBlockKeyPointer = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.BlockKeyPointer, token).ConfigureAwait(false);
            }

        }
        private async Task<ulong> ParseDataBlockPointer(DataBlock block, CancellationToken token)
        {
            ulong Offset = await ConnectionWrapper.Connection.PointerAll(block.Pointer!, token).ConfigureAwait(false);
            while (Offset == 0)
            {
                await Task.Delay(0_050, token).ConfigureAwait(false);
                Offset = await ConnectionWrapper.Connection.PointerAll(block.Pointer!, token).ConfigureAwait(false);
            }
            return Offset;
        }
        private async void EventFlagReset(object sender, EventArgs e)
        {
            var token = CancellationToken.None;
            ButtonEnable(new[] { ButtonAdvanceDate, ButtonDisconnect, ButtonViewRAM, ButtonReadRaids, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, false);
            await ParseBlockKeyPointer(token).ConfigureAwait(false);
            Identifier.Enabled = false;
            var id = uint.Parse(Identifier.Text);
            UpdateStatus($"target identifier {id}");
            var raid7offset = await ParseDataBlockPointer(SevenStarRaid, token).ConfigureAwait(false);
            var raid7defeat = await ParseDataBlockPointer(KSevenStarRaidsDefeat, token).ConfigureAwait(false);
            var raid7 = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(raid7offset, SevenStarRaid.Size, token).ConfigureAwait(false);
            var raid7defeatdata = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(raid7defeat, KSevenStarRaidsDefeat.Size, token).ConfigureAwait(false);
            if (raid7 is null || raid7defeatdata is null)
            {
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonDisconnect, ButtonViewRAM, ButtonReadRaids, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
                Identifier.Enabled = true;
                return;
            }
            byte[]? raidexcept = new byte[raid7.Length];
            byte[]? raid7defeatexcept = new byte[raid7defeatdata.Length];
            Array.Copy(raid7, raidexcept, raid7.Length);
            Array.Copy(raid7defeatdata, raid7defeatexcept, raid7defeatdata.Length);
            for (int Offset = 0; Offset < raid7.Length; Offset += 8)
            {
                var identifier = ReadUInt32LittleEndian(raid7.AsSpan(Offset));
                if (identifier != 0)
                {
                    if (raid7[Offset + 4] == 1 && identifier == id)
                    {
                        raid7[Offset + 4] = 0;
                        break;
                    }
                }
                else { break; }
            }
            for(int offset = 4; offset < raid7defeatdata.Length; offset += 8)
            {
                var defID = ReadUInt32LittleEndian(raid7defeatdata.AsSpan(offset));
                if (defID != 0)
                {
                    if (defID == id)
                    {
                        WriteUInt32LittleEndian(raid7defeatdata.AsSpan(offset, 4), 0);
                        break;
                    }
                }
                else { break; }
            }
            bool writeflag = false;
            if (!raid7.SequenceEqual(raidexcept))
            {
                await WriteBlock(raid7, SevenStarRaid, token).ConfigureAwait(false);
                UpdateStatus("Finish Resetting Event Raid CaughtFlags!");
                writeflag = true;
            }
            if (!raid7defeatdata.SequenceEqual(raid7defeatexcept))
            {
                await WriteBlock(raid7defeatdata, KSevenStarRaidsDefeat, token).ConfigureAwait(false);
                UpdateStatus("Finish Resetting Event Raid DefeatedFlags!");
                writeflag = true;
            }

            if (writeflag)
            {
                await ConnectionWrapper.SaveGameNonReset(token).ConfigureAwait(false);
            }
            else
            {
                MessageBox.Show("Selected Event Raid Flags was alreay reseted.");
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonDisconnect, ButtonViewRAM, ButtonReadRaids, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
                Identifier.Enabled = true;
                UpdateStatus($"Complete!");
                return;
            }
            (raid7, _) = await ReadEncryptedBlockObject(SevenStarRaid, 0, token).ConfigureAwait(false);
            (raid7defeatdata, _) = await ReadEncryptedBlockObject(KSevenStarRaidsDefeat, 0, token).ConfigureAwait(false);
            if (raid7 is null || raid7defeatdata is null)
            {
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonDisconnect, ButtonViewRAM, ButtonReadRaids, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
                Identifier.Enabled = true;
                return;
            }
            for (int Offset = 0; Offset < raid7.Length; Offset += 8)
            {
                var identifier = ReadUInt32LittleEndian(raid7.AsSpan(Offset));
                if (identifier != 0)
                {
                    if (raid7[Offset + 4] == 1 && identifier == id)
                    {
                        ShowMessageBox("Captured Flag is still true");
                        break;
                    }
                }
                else { break; }
            }
            for (int Offset = 4; Offset < raid7defeatdata.Length; Offset += 8)
            {
                var identifier = ReadUInt32LittleEndian(raid7defeatdata.AsSpan(Offset));
                if (identifier != 0)
                {
                    if (identifier == id)
                    {
                        ShowMessageBox("Defeated Flag is still true");
                        break;
                    }
                }
                else { break; }
            }
            ButtonEnable(new[] { ButtonAdvanceDate, ButtonDisconnect, ButtonViewRAM, ButtonReadRaids, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
            Identifier.Enabled = true;
            MessageBox.Show("Complete!");
            UpdateStatus("Complete!");
        }
        private static async Task<bool> IsOnOverworldTitle(CancellationToken token)
        {
            var offset = await ConnectionWrapper.Connection.PointerAll(ConnectionWrapper.OverworldPointer, token).ConfigureAwait(false);
            if (offset == 0)
                return false;
            return await IsOnOverworld(offset, token).ConfigureAwait(false);
        }

        public static async Task<bool> IsOnOverworld(ulong offset, CancellationToken token)
        {
            var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(offset, 1, token).ConfigureAwait(false);
            return data[0] == 0x11;
        }
        public async Task<SimpleTrainerInfo> GetFakeTrainerSAVSV(CancellationToken token)
        {
            var sav = new SAV9SV();
            var info = sav.MyStatus;
            var read = await ConnectionWrapper.Connection.PointerPeek(info.Data.Length, ConnectionWrapper.MyStatusPointerSV, token).ConfigureAwait(false);
            read.CopyTo(info.Data);
            SimpleTrainerInfo Trainer = new()
            {
                OT = sav.OT,
                TID16 = sav.TID16,
                SID16 = sav.SID16,
                Language = sav.Language,
            };
            return Trainer;
        }

        public async Task PreReOpenGame(CancellationToken token)
        {
            await CloseGame(token).ConfigureAwait(false);
            await PreStartGame(token).ConfigureAwait(false);
        }

        public async Task ReOpenGame(CancellationToken token)
        {
            await CloseGame(token).ConfigureAwait(false);
            await StartGame(token).ConfigureAwait(false);
        }

        public async Task CloseGame(CancellationToken token)
        {
            UpdateStatus("Close out of the game.");
            // Close out of the game
            await Click(B, 0_500, token).ConfigureAwait(false);
            await Click(HOME, 2_000, token).ConfigureAwait(false);
            await Click(X, 1_000, token).ConfigureAwait(false);
            await Click(A, 5_000, token).ConfigureAwait(false);
        }

        public async Task PreStartGame(CancellationToken token)
        {
            UpdateStatus(" Reopening game ...");
            // Open game.
            await Click(A, 1_000, token).ConfigureAwait(false);

            // Menus here can go in the order: Update Prompt -> Profile -> DLC check -> Unable to use DLC.
            //  The user can optionally turn on the setting if they know of a breaking system update incoming.
            await Click(DUP, 0_600, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);
            // If they have DLC on the system and can't use it, requires an UP + A to start the game.
            // Should be harmless otherwise since they'll be in loading screen.
            await Click(DUP, 0_600, token).ConfigureAwait(false);
            await Click(A, 0_600, token).ConfigureAwait(false);

            UpdateStatus("Current Screen is Switch Logo or game load screen. Waiting ...");
            // Switch Logo and game load screen
            await Task.Delay(19_000, token).ConfigureAwait(false);
        }

        public async Task StartGame(CancellationToken token)
        {
            UpdateStatus(" Reopening game ...");
            // Open game.
            await Click(A, 1_000, token).ConfigureAwait(false);

            // Menus here can go in the order: Update Prompt -> Profile -> DLC check -> Unable to use DLC.
            //  The user can optionally turn on the setting if they know of a breaking system update incoming.
            await Click(DUP, 0_600, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);
            // If they have DLC on the system and can't use it, requires an UP + A to start the game.
            // Should be harmless otherwise since they'll be in loading screen.
            await Click(DUP, 0_600, token).ConfigureAwait(false);
            await Click(A, 0_600, token).ConfigureAwait(false);


            // Switch Logo and game load screen
            await Task.Delay(19_000, token).ConfigureAwait(false);

            UpdateStatus(" Waiting for title screen ...");
            for (int i = 0; i < 8; i++)
                await Click(A, 1_000, token).ConfigureAwait(false);

            UpdateStatus(" Waiting for Overworld ...");
            var timer = 5_000;
            while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
            {
                await Task.Delay(1_000, token).ConfigureAwait(false);
                timer -= 1_000;
                // We haven't made it back to overworld after a minute, so press A every 6 seconds hoping to restart the game.
                // Don't risk it if hub is set to avoid updates.
                if (timer <= 0)
                {
                    while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
                        await Click(A, 5_000, token).ConfigureAwait(false);
                    break;
                }
            }

            await Task.Delay(3_000, token).ConfigureAwait(false);
            UpdateStatus(" Back in overworld!");
        }
        public async Task StartGameSetTime(CancellationToken token)
        {

            UpdateStatus($" Date Back {Config.DayBackCount} ...");
            for (int i = 0; i < Config.DayBackCount; i++)
            {
                await ConnectionWrapper.DateBackFaster(token).ConfigureAwait(false);
                await Task.Delay(1_000).ConfigureAwait(false);
            }

            UpdateStatus(" Reopening game ...");
            // Open game.
            await Click(A, 1_000, token).ConfigureAwait(false);

            // Menus here can go in the order: Update Prompt -> Profile -> DLC check -> Unable to use DLC.
            //  The user can optionally turn on the setting if they know of a breaking system update incoming.
            await Click(DUP, 0_600, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);
            // If they have DLC on the system and can't use it, requires an UP + A to start the game.
            // Should be harmless otherwise since they'll be in loading screen.
            await Click(DUP, 0_600, token).ConfigureAwait(false);
            await Click(A, 0_600, token).ConfigureAwait(false);

            // Switch Logo and game load screen
            await Task.Delay(19_000, token).ConfigureAwait(false);
            UpdateStatus(" Waiting for title screen ...");
            for (int i = 0; i < 8; i++)
                await Click(A, 1_000, token).ConfigureAwait(false);

            UpdateStatus(" Waiting for Overworld ...");
            var timer = 5_000;
            while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
            {
                await Task.Delay(1_000, token).ConfigureAwait(false);
                timer -= 1_000;
                // We haven't made it back to overworld after a minute, so press A every 6 seconds hoping to restart the game.
                // Don't risk it if hub is set to avoid updates.
                if (timer <= 0)
                {
                    while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
                        await Click(A, 5_000, token).ConfigureAwait(false);
                    break;
                }
            }

            await Task.Delay(3_000, token).ConfigureAwait(false);
            UpdateStatus(" Back in overworld and reset time!");
        }
        private void SetTime_Click(object sender, EventArgs e)
        {
            SetTime.Visible = false;
            ResetRaids.Visible = true;
            Task.Run(async () =>
            {
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, false);
                var resetTime = await ResetTime(CancellationToken.None).ConfigureAwait(false);
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
                MessageBox.Show($"Current Time set is complete!{Environment.NewLine}Current Time: " + resetTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss"), "Switch NTP Server Clock");
            }, CancellationToken.None);

        }
        private void DateTimeButton_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, false);
                var resetTime = await ResetTime(CancellationToken.None).ConfigureAwait(false);
                ButtonEnable(new[] { ButtonAdvanceDate, ButtonReadRaids, ButtonDisconnect, ButtonViewRAM, ButtonDownloadEvents, SendScreenshot, btnOpenMap, Rewards, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
                MessageBox.Show($"Current Time set is complete!{Environment.NewLine}Current Time: " + resetTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss"), "Switch NTP Server Clock");
            }, CancellationToken.None);
        }
        private async Task<DateTime> GetCurrentTime(CancellationToken token)
        {
            var unixTime = await ConnectionWrapper.GetUnixTime(token).ConfigureAwait(false);
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime curTime = epoch.AddSeconds(unixTime);
            return curTime;
        }
        private void CurrentTimeButton_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    var curTime = await GetCurrentTime(Source.Token).ConfigureAwait(false);
                    TimeViewer time = new(ConnectionWrapper, curTime, Source.Token);
                    time.ShowDialog();
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Could not read Current Time!{Environment.NewLine} {ex}");
                }
            });
            
        }
        private async Task<DateTime> ResetTime(CancellationToken token)
        {
            var NTPTime = await ConnectionWrapper.ResetTimeNTP(token).ConfigureAwait(false);
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime resetTime = epoch.AddSeconds(NTPTime);
            return resetTime;
        }

        public void Protocol_SelectedIndexChanged(SwitchProtocol protocol)
        {
            Config.Protocol = protocol;
            ConnectionConfig = new()
            {
                IP = Config.IP,
                Protocol = Config.Protocol,
                Port = protocol is SwitchProtocol.WiFi ? 6000 : Config.UsbPort,                
            };
            if (protocol is SwitchProtocol.USB)
            {
                InputSwitchIP.Visible = false;
                LabelSwitchIP.Visible = false;
                USB_Port_label.Visible = true;
                USB_Port_TB.Visible = true;
            }
            else
            {
                InputSwitchIP.Visible = true;
                LabelSwitchIP.Visible = true;
                USB_Port_label.Visible = false;
                USB_Port_TB.Visible = false;
            }
        }

        private void DisplayMap(object sender, EventArgs e)
        {
            var raids = RaidContainer.Container.Raids;
            if (raids.Count == 0)
            {
                ShowMessageBox("Raids not loaded.");
                return;
            }

            var raid = raids[ComboIndex.SelectedIndex];
            var encounter = RaidContainer.Container.Encounters[ComboIndex.SelectedIndex];
            var teratype = raid.GetTeraType(encounter);
            var map = GenerateMap(raid, teratype);
            if (map is null)
            {
                ShowMessageBox("Error generating map.");
                return;
            }

            var form = new MapView(map);
            ShowDialogs(form);
        }

        private void Rewards_Click(object sender, EventArgs e)
        {
            if (RaidContainer.Container.Raids.Count == 0)
            {
                ShowMessageBox("Raids not loaded.");
                return;
            }

            var rewards = RaidContainer.Container.Rewards[ComboIndex.SelectedIndex];
            if (rewards is null)
            {
                ShowMessageBox("Error while displaying rewards.");
                return;
            }

            var form = new RewardsView(RaidContainer.Strings.Item, RaidContainer.Strings.Move, rewards);
            ShowDialogs(form);
        }

        private async void Teleport_Click(object sender, EventArgs e)
        {
            ButtonEnable(new[] { ButtonAdvanceDate, ButtonDisconnect, ButtonViewRAM, ButtonReadRaids, ButtonDownloadEvents, SendScreenshot, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, false);
            var raids = RaidContainer.Container.Raids;
            if (raids.Count == 0)
            {
                ShowMessageBox("Raids not loaded.");
                return;
            }

            var raid = raids[ComboIndex.SelectedIndex];
            await TeleportToDen(raid).ConfigureAwait(false);
            ButtonEnable(new[] { ButtonAdvanceDate, ButtonDisconnect, ButtonViewRAM, ButtonReadRaids, ButtonDownloadEvents, SendScreenshot, TeleportToDenButton, EventRaidReset, OHKO, ResetRaids, SetTime, CurrentTimeButton, DateTimeButton }, true);
        }

        private void RaidBoost_SelectedIndexChanged(object sender, EventArgs e)
        {
            RaidContainer.Container.ClearRewards();
            var raids = RaidContainer.Container.Raids;
            var encounters = RaidContainer.Container.Encounters;

            List<List<(int, int, int)>> newRewards = new();
            for (int i = 0; i < raids.Count; i++)
            {
                var raid = raids[i];
                var encounter = encounters[i];
                newRewards.Add(encounter.GetRewards(raid, RaidBoost.SelectedIndex));
            }
            RaidContainer.Container.SetRewards(newRewards);
        }

        private void Move_Clicked(object sender, EventArgs e)
        {
            if (RaidContainer.Container.Raids.Count == 0)
            {
                ShowMessageBox("Raids not loaded.");
                return;
            }

            var encounter = RaidContainer.Container.Encounters[ComboIndex.SelectedIndex];
            if (encounter is null)
                return;

            ShowExtraMoves ^= true;
            LabelMoves.Text = ShowExtraMoves ? "Extra:" : "Moves:";
            LabelMoves.Location = new(LabelMoves.Location.X + (ShowExtraMoves ? 9 : -9), LabelMoves.Location.Y);

            var length = encounter.ExtraMoves.Length < 4 ? 4 : encounter.ExtraMoves.Length;
            var extra_moves = new ushort[length];
            for (int i = 0; i < encounter.ExtraMoves.Length; i++)
                extra_moves[i] = encounter.ExtraMoves[i];

            Move1.Text = ShowExtraMoves ? RaidContainer.Strings.Move[extra_moves[0]] : RaidContainer.Strings.Move[encounter.Move1];
            Move2.Text = ShowExtraMoves ? RaidContainer.Strings.Move[extra_moves[1]] : RaidContainer.Strings.Move[encounter.Move2];
            Move3.Text = ShowExtraMoves ? RaidContainer.Strings.Move[extra_moves[2]] : RaidContainer.Strings.Move[encounter.Move3];
            Move4.Text = ShowExtraMoves ? RaidContainer.Strings.Move[extra_moves[3]] : RaidContainer.Strings.Move[encounter.Move4];
        }

        private void ComboIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RaidContainer.Container.Raids.Count == 0)
                return;

            DisplayRaid();
            if (Config.StreamerView)
                DisplayPrettyRaid();
        }

        private void SendScreenshot_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Webhook.SendScreenshot(ConnectionWrapper.Connection, Source.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    ShowMessageBox($"Could not send the screenshot: {ex.Message}");
                }
            }, Source.Token);
        }

        private void SearchTimer_Elapsed(object sender, EventArgs e)
        {
            if (!stopwatch.IsRunning)
                return;

            var timeSpan = stopwatch.Elapsed;
            string time = string.Format("{0:00}:{1:00}:{2:00}:{3:00}",
            timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            Invoke(() => Text = formTitle + " [Searching for " + time + "]");
            if (Config.StreamerView && teraRaidView is not null)
                Invoke(() => teraRaidView.textSearchTime.Text = time);
        }

        public void TestWebhook()
        {
            Task.Run(async () => await TestWebhookAsync(Source.Token).ConfigureAwait(false), Source.Token);
        }

        private async Task TestWebhookAsync(CancellationToken token)
        {
            var filter = new RaidFilter { Name = "Test Webhook" };
            var satisfied_filters = new List<RaidFilter> { filter };

            int i = -1;
            if (InvokeRequired)
                i = Invoke(() => { return ComboIndex.SelectedIndex; });
            else i = ComboIndex.SelectedIndex;

            var Trainer = await GetFakeTrainerSAVSV(token).ConfigureAwait(false);
            var raids = RaidContainer.Container.Raids;
            var encounters = RaidContainer.Container.Encounters;
            var rewards = RaidContainer.Container.Rewards;
            if (i > -1 && encounters[i] is not null && raids[i] is not null)
            {
                var timeSpan = stopwatch.Elapsed;
                string time = string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                var teraType = raids[i].GetTeraType(encounters[i]);
                var color = TypeColor.GetTypeSpriteColor((byte)teraType);
                var hexColor = $"{color.R:X2}{color.G:X2}{color.B:X2}";

                var blank = new PK9
                {
                    Species = encounters[i].Species,
                    Form = encounters[i].Form,
                    Gender = encounters[i].Gender,
                };
                blank.SetSuggestedFormArgument();

                var spriteName = GetSpriteNameForUrl(blank, raids[i].CheckIsShiny(encounters[i]));
                await Webhook.SendNotification(encounters[i], raids[i], filter, time, rewards[i], hexColor, spriteName, Trainer, token).ConfigureAwait(false);
            }
            else { ShowMessageBox("Please connect to your device and ensure a raid has been found."); }
        }

        public void ToggleStreamerView()
        {
            if (Config.StreamerView)
            {
                teraRaidView = new();
                teraRaidView.Map.Image = map_base;
                teraRaidView.Show();
            }
            else if (!Config.StreamerView && teraRaidView is not null)
            {
                teraRaidView.Close();
            }
        }

        private static string GetSpriteNameForUrl(PK9 pk, bool shiny)
        {
            // Since we're later using this for URL assembly later, we need dashes instead of underscores for forms.
            var spriteName = SpriteName.GetResourceStringSprite(pk.Species, pk.Form, pk.Gender, pk.FormArgument, EntityContext.Gen9, shiny)[1..];
            return spriteName.Replace('_', '-').Insert(0, "_");
        }

        private async Task<(uint, ulong)> ReadEncryptedBlockUint(DataBlock block, ulong init, CancellationToken token)
        {
            var (header, address) = await ReadEncryptedBlockHeader(block, init, token).ConfigureAwait(false);
            return (ReadUInt32LittleEndian(header.AsSpan()[1..]), address);
        }

        private async Task<(byte, ulong)> ReadEncryptedBlockByte(DataBlock block, ulong init, CancellationToken token)
        {
            var (header, address) = await ReadEncryptedBlockHeader(block, init, token).ConfigureAwait(false);
            return (header[1], address);
        }

        private async Task<(byte[]?, ulong)> ReadEncryptedBlockArray(DataBlock block, ulong init, CancellationToken token)
        {
            if (init == 0)
            {
                var address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
                init = address;
            }

            var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(init, 6 + block.Size, token).ConfigureAwait(false);
            data = DecryptBlock(block.Key, data);

            return (data[6..], init);
        }

        private async Task<(bool, ulong)> ReadEncryptedBlockBool(DataBlock block, CancellationToken token)
        {
            var address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
            address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
            var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address, block.Size, token).ConfigureAwait(false);
            var res = DecryptBlock(block.Key, data);
            return (res[0] == 2, address);
        }

        private async Task<(int, ulong)> ReadEncryptedBlockInt32(DataBlock block, ulong init, CancellationToken token)
        {
            var (header, address) = await ReadEncryptedBlockHeader(block, init, token).ConfigureAwait(false);
            return (ReadInt32LittleEndian(header.AsSpan()[1..]), address);
        }

        private async Task<(byte[]?, ulong)> ReadEncryptedBlockObject(DataBlock block, ulong init, CancellationToken token)
        {
            while (init == 0)
            {
                var address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
                init = address;
            }
            var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(init, 5 + block.Size, token).ConfigureAwait(false);
            var res = DecryptBlock(block.Key, data)[5..];

            return (res, init);
        }

        public async Task<bool> WriteBlock(object data, DataBlock block, CancellationToken token, object? toExpect = default)
        {
            if (block.IsEncrypted)
                return await WriteEncryptedBlockSafe(block, toExpect, data, token).ConfigureAwait(false);
            else
                return await WriteDecryptedBlock((byte[])data!, block, token).ConfigureAwait(false);
        }

        private async Task<bool> WriteDecryptedBlock(byte[] data, DataBlock block, CancellationToken token)
        {
            var offset = await ParseDataBlockPointer(block, token).ConfigureAwait(false);
            await ConnectionWrapper.Connection.WriteBytesAbsoluteAsync(data, offset, token).ConfigureAwait(false);

            return true;
        }

        private async Task<bool> WriteEncryptedBlockSafe(DataBlock block, object? toExpect, object toWrite, CancellationToken token)
        {
            if (toExpect == default || toWrite == default)
                return false;

            return block.Type switch
            {
                SCTypeCode.Object => await WriteEncryptedBlockObject(block, (byte[])toExpect, (byte[])toWrite, token).ConfigureAwait(false),
                SCTypeCode.Array => await WriteEncryptedBlockArray(block, (byte[])toExpect, (byte[])toWrite, token).ConfigureAwait(false),
                SCTypeCode.Bool1 or SCTypeCode.Bool2 or SCTypeCode.Bool3 => await WriteEncryptedBlockBool(block, (bool)toExpect, (bool)toWrite, token).ConfigureAwait(false),
                SCTypeCode.Byte or SCTypeCode.SByte => await WriteEncryptedBlockByte(block, (byte)toExpect, (byte)toWrite, token).ConfigureAwait(false),
                SCTypeCode.UInt32 or SCTypeCode.UInt64 => await WriteEncryptedBlockUint(block, (uint)toExpect, (uint)toWrite, token).ConfigureAwait(false),
                SCTypeCode.Int32 => await WriteEncryptedBlockInt32(block, (int)toExpect, (int)toWrite, token).ConfigureAwait(false),
                _ => throw new NotSupportedException($"Block {block.Name} (Type {block.Type}) is currently not supported.")
            };
        }

        private static async Task<bool> WriteEncryptedBlockUint(DataBlock block, uint valueToExpect, uint valueToInject, CancellationToken token)
        {
            ulong address;
            try
            {
                address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
            }
            catch (Exception) { return false; }
            //If we get there without exceptions, the block address is valid
            var header = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address, 5, token).ConfigureAwait(false);
            header = DecryptBlock(block.Key, header);
            //Validate ram data
            var ram = ReadUInt32LittleEndian(header.AsSpan()[1..]);
            if (ram != valueToExpect) return false;
            //If we get there then both block address and block data are valid, we can safely inject
            WriteUInt32LittleEndian(header.AsSpan()[1..], valueToInject);
            header = EncryptBlock(block.Key, header);
            await ConnectionWrapper.Connection.WriteBytesAbsoluteAsync(header, address, token).ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> WriteEncryptedBlockInt32(DataBlock block, int valueToExpect, int valueToInject, CancellationToken token)
        {
            ulong address;
            try
            {
                address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
            }
            catch (Exception) { return false; }
            //If we get there without exceptions, the block address is valid
            var header = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address, 5, token).ConfigureAwait(false);
            header = DecryptBlock(block.Key, header);
            //Validate ram data
            var ram = ReadInt32LittleEndian(header.AsSpan()[1..]);
            if (ram != valueToExpect) return false;
            //If we get there then both block address and block data are valid, we can safely inject
            WriteInt32LittleEndian(header.AsSpan()[1..], valueToInject);
            header = EncryptBlock(block.Key, header);
            await ConnectionWrapper.Connection.WriteBytesAbsoluteAsync(header, address, token).ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> WriteEncryptedBlockByte(DataBlock block, byte valueToExpect, byte valueToInject, CancellationToken token)
        {
            ulong address;
            try
            {
                address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
            }
            catch (Exception) { return false; }
            //If we get there without exceptions, the block address is valid
            var header = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address, 5, token).ConfigureAwait(false);
            header = DecryptBlock(block.Key, header);
            //Validate ram data
            var ram = header[1];
            if (ram != valueToExpect) return false;
            //If we get there then both block address and block data are valid, we can safely inject
            header[1] = valueToInject;
            header = EncryptBlock(block.Key, header);
            await ConnectionWrapper.Connection.WriteBytesAbsoluteAsync(header, address, token).ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> WriteEncryptedBlockArray(DataBlock block, byte[] arrayToExpect, byte[] arrayToInject, CancellationToken token)
        {
            ulong address;
            try
            {
                address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
            }
            catch (Exception) { return false; }
            //If we get there without exceptions, the block address is valid
            var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address, 6 + block.Size, token).ConfigureAwait(false);
            data = DecryptBlock(block.Key, data);
            //Validate ram data
            var ram = data[6..];
            if (!ram.SequenceEqual(arrayToExpect)) return false;
            //If we get there then both block address and block data are valid, we can safely inject
            Array.ConstrainedCopy(arrayToInject, 0, data, 6, block.Size);
            data = EncryptBlock(block.Key, data);
            await ConnectionWrapper.Connection.WriteBytesAbsoluteAsync(data, address, token).ConfigureAwait(false);

            return true;
        }

        private async Task<bool> WriteEncryptedBlockObject(DataBlock block, byte[] arrayToExpect, byte[] arrayToInject, CancellationToken token)
        {
            ulong address;
            try
            {
                address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
            }
            catch (Exception) { return false; }
            //If we get there without exceptions, the block address is valid
            var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address, 5 + block.Size, token).ConfigureAwait(false);
            data = DecryptBlock(block.Key, data);
            //Validate ram data
            var ram = data[5..];
            if (!ram.SequenceEqual(arrayToExpect))
            {
                ShowMessageBox("No right data detected!");
                return false;
            }
            UpdateStatus("Start Restting Flags ...");
            //If we get there then both block address and block data are valid, we can safely inject
            Array.ConstrainedCopy(arrayToInject.ToArray(), 0, data, 5, block.Size);
            data = EncryptBlock(block.Key, data);
            await ConnectionWrapper.Connection.WriteBytesAbsoluteAsync(data, address, token).ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> WriteEncryptedBlockBool(DataBlock block, bool valueToExpect, bool valueToInject, CancellationToken token)
        {
            ulong address;
            try
            {
                address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
            }
            catch (Exception) { return false; }
            //If we get there without exceptions, the block address is valid
            var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address, block.Size, token).ConfigureAwait(false);
            data = DecryptBlock(block.Key, data);
            //Validate ram data
            var ram = data[0] == 2;
            if (ram != valueToExpect) return false;
            //If we get there then both block address and block data are valid, we can safely inject
            data[0] = valueToInject ? (byte)2 : (byte)1;
            data = EncryptBlock(block.Key, data);
            await ConnectionWrapper.Connection.WriteBytesAbsoluteAsync(data, address, token).ConfigureAwait(false);

            return true;
        }

        public static byte[] EncryptBlock(uint key, byte[] block) => DecryptBlock(key, block);

        private async Task<(byte[], ulong)> ReadEncryptedBlockHeader(DataBlock block, ulong init, CancellationToken token)
        {
            if (init == 0)
            {
                var address = await SearchSaveKey(block.Key, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(address + 8, 0x8, token).ConfigureAwait(false), 0);
                init = address;
            }
            var header = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(init, 5, token).ConfigureAwait(false);
            header = DecryptBlock(block.Key, header);

            return (header, init);
        }

        public static async Task<ulong> SearchSaveKey(uint key, CancellationToken token)
        {
            var data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(BaseBlockKeyPointer + 8, 16, token).ConfigureAwait(false);
            var start = BitConverter.ToUInt64(data.AsSpan()[..8]);
            var end = BitConverter.ToUInt64(data.AsSpan()[8..]);

            while (start < end)
            {
                var block_ct = (end - start) / 48;
                var mid = start + (block_ct >> 1) * 48;

                data = await ConnectionWrapper.Connection.ReadBytesAbsoluteAsync(mid, 4, token).ConfigureAwait(false);
                var found = BitConverter.ToUInt32(data);
                if (found == key)
                    return mid;

                if (found >= key)
                    end = mid;
                else start = mid + 48;
            }
            return start;
        }
        public static string PokeImg(PKM pkm, bool canGmax)
        {
            bool md = false;
            bool fd = false;
            string[] baseLink;
            baseLink = "https://raw.githubusercontent.com/zyro670/HomeImages/master/128x128/poke_capture_0001_000_mf_n_00000000_f_n.png".Split('_');

            if (Enum.IsDefined(typeof(GenderDependent), pkm.Species) && !canGmax && pkm.Form is 0)
            {
                if (pkm.Gender is 0 && pkm.Species is not (ushort)PKHeX.Core.Species.Torchic)
                    md = true;
                else fd = true;
            }

            int form = pkm.Species switch
            {
                (ushort)PKHeX.Core.Species.Sinistea or (ushort)PKHeX.Core.Species.Polteageist or (ushort)PKHeX.Core.Species.Rockruff or (ushort)PKHeX.Core.Species.Mothim => 0,
                (ushort)PKHeX.Core.Species.Alcremie when pkm.IsShiny || canGmax => 0,
                _ => pkm.Form,

            };
            if (pkm.Species is (ushort)PKHeX.Core.Species.Sneasel)
            {
                if (pkm.Gender is 0)
                    md = true;
                else fd = true;
            }

            if (pkm.Species is (ushort)PKHeX.Core.Species.Basculegion)
            {
                if (pkm.Gender is 0)
                {
                    md = true;
                    pkm.Form = 0;
                }
                else { pkm.Form = 1; }

                string s = pkm.IsShiny ? "r" : "n";
                string g = md && pkm.Gender is not 1 ? "md" : "fd";
                return $"https://raw.githubusercontent.com/zyro670/HomeImages/master/128x128/poke_capture_0" + $"{pkm.Species}" + "_00" + $"{pkm.Form}" + "_" + $"{g}" + "_n_00000000_f_" + $"{s}" + ".png";
            }

            baseLink[2] = pkm.Species < 10 ? $"000{pkm.Species}" : pkm.Species < 100 && pkm.Species > 9 ? $"00{pkm.Species}" : pkm.Species >= 1000 ? $"{pkm.Species}" : $"0{pkm.Species}";
            baseLink[3] = pkm.Form < 10 ? $"00{form}" : $"0{form}";
            baseLink[4] = pkm.PersonalInfo.OnlyFemale ? "fo" : pkm.PersonalInfo.OnlyMale ? "mo" : pkm.PersonalInfo.Genderless ? "uk" : fd ? "fd" : md ? "md" : "mf";
            baseLink[5] = canGmax ? "g" : "n";
            baseLink[6] = "0000000" + (pkm.Species is (ushort)PKHeX.Core.Species.Alcremie && !canGmax ? pkm.Data[0xE4] : 0);
            baseLink[8] = pkm.IsShiny ? "r.png" : "n.png";
            return string.Join("_", baseLink);
        }
        public static byte[] DecryptBlock(uint key, byte[] block)
        {
            var rng = new SCXorShift32(key);
            for (int i = 0; i < block.Length; i++)
                block[i] = (byte)(block[i] ^ rng.Next());
            return block;
        }

        private void CheckForUpdates()
        {
            Task.Run(async () =>
            {
                Version? latestVersion;
                try { latestVersion = Utils.GetLatestVersion(); }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception while checking for latest version: {ex}");
                    return;
                }

                if (latestVersion is null || latestVersion <= CurrentVersion)
                    return;

                while (!IsHandleCreated) // Wait for form to be ready
                    await Task.Delay(2_000).ConfigureAwait(false);
                await InvokeAsync(() => NotifyNewVersionAvailable(latestVersion));
            });
        }

        private void NotifyNewVersionAvailable(Version version)
        {
            Text += $" - Update v{version.Major}.{version.Minor}.{version.Build} available!";
            UpdateStatus($"Update v{version.Major}.{version.Minor}.{version.Build} available!");
#if !DEBUG
        MessageBox.Show($"Update available! v{version.Major}.{version.Minor}.{version.Build}");
        Process.Start(new ProcessStartInfo("https://github.com/LegoFigure11/RaidCrawler/releases/") { UseShellExecute = true });
#endif
        }
    }
}
