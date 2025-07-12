using PKHeX.Core;
using RaidCrawler.Core.Connection;
using RaidCrawler.Core.Structures;
using RaidCrawler.WinForms.Util;
using static SysBot.Base.SwitchButton;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace RaidCrawler.WinForms.SubForms
{
    public partial class TimeViewer : Form
    {
        private readonly ConnectionWrapperAsync Executor;
        private readonly CancellationToken token;
        private string PreviousId;
        private ItemStructure itemStructure;
        private ulong ItemOffset;
        private SearchFilter Filter = new();
        public TimeViewer(ConnectionWrapperAsync executor, DateTime curTime, CancellationToken Token)
        {
            InitializeComponent();
            TimeZoneCombo.DataSource = TimeZoneInfo.GetSystemTimeZones();
            TimeZoneCombo.DisplayMember = "DisplayName";
            TimeZoneCombo.ValueMember = "Id";
            PrintModeCombo.DataSource = Enum.GetValues(typeof(PrintMode)).Cast<PrintMode>();
            Executor = executor;
            itemStructure = new(Executor);
            token = Token;
            CurTime.Text = curTime.ToString("yyyy/MM/dd HH:mm:ss");
            Seed.Text = TimeUtil.GetTime(curTime).ToString();
            PreviousId = "Utc";
        }
        private void TimeZoneCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TimeZoneCombo.SelectedIndex < 0 || TimeZoneCombo.SelectedValue == null)
                return;
            if(string.IsNullOrEmpty(PreviousId))
            {
                PreviousId = "UTC";
            }
            if(DateTime.TryParse(CurTime.Text, out var time))
            {
                time = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time, PreviousId,(string)TimeZoneCombo.SelectedValue);
                CurTime.Text = time.ToString("yyyy/MM/dd HH:mm:ss");
                PreviousId = (string)TimeZoneCombo.SelectedValue;
            }
            else if(ulong.TryParse(CurTime.Text, out var ticks))
            {
                GetCurrentSeed();
                PreviousId = (string)TimeZoneCombo.SelectedValue;
            }
        }
        private void CurTime_TextChanged(object sender, EventArgs e)
        {
            GetCurrentSeed();
        }
        private void TimeFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TimeFormat.SelectedIndex < 0)
                return;
            if(TimeFormat.SelectedIndex == 0)
            {
                if (DateTime.TryParse(CurTime.Text, out var time))
                {
                    if (TimeZoneCombo.SelectedIndex < 0 || TimeZoneCombo.SelectedValue == null)
                        return;
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue);
                    DateTime curdate = TimeZoneInfo.ConvertTimeToUtc(time, info);
                    var Diff = curdate - epoch;
                    CurTime.Text = $"{Diff.TotalSeconds}";
                }
                else if(ulong.TryParse(CurTime.Text, out var _))
                {
                    return;
                }
                else
                {
                    CurTime.Text = string.Empty;
                }
            }
            else
            {
                if (ulong.TryParse(CurTime.Text, out var ticks))
                {
                    if (TimeZoneCombo.SelectedIndex < 0 || TimeZoneCombo.SelectedValue == null)
                        return;
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime curTime = epoch.AddSeconds(ticks);
                    TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue);
                    curTime = TimeZoneInfo.ConvertTimeFromUtc(curTime, info);
                    CurTime.Text = curTime.ToString("yyyy/MM/dd HH:mm:ss");
                }
                else if(DateTime.TryParse(CurTime.Text, out var _))
                {
                    return;
                }
                else
                {
                    CurTime.Text = string.Empty;
                }
            }
        }
        private void SetTimeButton_Click(object sender, EventArgs e)
        {
            ulong SetTime = 0;
            DisableOptins();
            try
            {
                if (DateTime.TryParse(CurTime.Text, out var time))
                {
                    var timeset = GetTimeSet(time);
                    if (timeset == null)
                        return;
                    SetTime = TimeUtil.GetTime(timeset.Value);                    
                }
                else if (ulong.TryParse(CurTime.Text, out var ticks))
                {
                    SetTime = ticks;
                }
                else
                {
                    MessageBox.Show("Time Format is wrong!");
                    EnableOptions();
                    return;
                }
                Task.Run(async() => await Executor.SetCurrentTime(SetTime, token).ConfigureAwait(false));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                EnableOptions();
                return;
            }
            MessageBox.Show($"Modifing Time is success!{Environment.NewLine}SetTime: {GetTimeLocal(SetTime):yyyy/MM/dd HH:mm:ss}");
            EnableOptions();
        }
        private async void Backward_Click(object sender, EventArgs e)
        {
            DisableOptins();
            await Executor.BackHour((int)HoursNum.Value, 0_400, token).ConfigureAwait(false);
            EnableOptions();
            string fal = HoursNum.Value == 1 ? "hour" : "hours";
            MessageBox.Show($"Done. We skipped {HoursNum.Value} {fal} backward.");
        }

        private async void Forward_Click(object sender, EventArgs e)
        {
            DisableOptins();
            await Executor.SkipHour((int)HoursNum.Value, 0_400, token).ConfigureAwait(false);
            EnableOptions();
            string fal = HoursNum.Value == 1 ? "hour" : "hours";
            MessageBox.Show($"Done. We skipped {HoursNum.Value} {fal} forward.");
        }
        private async void BackwardMinute_Click(object sender, EventArgs e)
        {
            DisableOptins();
            await Executor.BackMinutes((int)MinutesNum.Value, 0_400, token).ConfigureAwait(false);
            EnableOptions();
            string fal = MinutesNum.Value == 1 ? "minute" : "minutes";
            MessageBox.Show($"Done. We skipped {MinutesNum.Value} {fal} backward.");
        }

        private async void FowardMinute_Click(object sender, EventArgs e)
        {
            DisableOptins();
            await Executor.SkipMinutes((int)MinutesNum.Value, 0_400, token).ConfigureAwait(false);
            EnableOptions();
            string fal = MinutesNum.Value == 1 ? "minute" : "minutes";
            MessageBox.Show($"Done. We skipped {MinutesNum.Value} {fal} forward.");
        }

        // unixTime currently fails, hide assets for now.
        private async void Read_Click(object sender, EventArgs e)
        {
            DisableOptins();
            DateTime curTime = DateTime.Now;
            try
            {
                var unixTime = await Executor.GetUnixTime(token).ConfigureAwait(false);
                curTime = GetTime(unixTime);
                if (InvokeRequired)
                    Invoke(() => GetTimeString(curTime));
                else
                    GetTimeString(curTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                EnableOptions();
                return;
            }
            MessageBox.Show($"Current System Time is {curTime.ToLongDateString()} {curTime.ToLongTimeString()} ", "Current Switch System Clock");
            EnableOptions();
        }
        private async void SetTime_Click(object sender, EventArgs e)
        {
            DisableOptins();
            DateTime dateTime = DateTime.UtcNow;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            await Executor.SetCurrentTime((ulong)(dateTime - epoch).TotalSeconds, token).ConfigureAwait(false);
            MessageBox.Show($"System Time is set to {dateTime.ToLocalTime().ToLongDateString()} {dateTime.ToLocalTime().ToLongTimeString()}", "Windows System Clock");
            EnableOptions();
        }

        private void ItemPrinterRNG_Click(object sender, EventArgs e)
        {
            CurrentTimelabel.Text = "Current Time:";
            DisableOptins();
            try
            {
                Task.Run(async () =>
                {
                    await DoItemPrinterRNG(token).ConfigureAwait(false);
                    MessageBox.Show("Item Printer RNG is completed!");
                    EnableOptions();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                EnableOptions();                
            }
        }
        private async Task DoItemPrinterRNG(CancellationToken token)
        {
            if (TimeZoneCombo.SelectedIndex < 0 || TimeZoneCombo.SelectedValue == null)
                return;
            if (!int.TryParse(PrintItemCombo.Text, out int item))
            {
                MessageBox.Show("PrintItemCount is Invalid!");
                return;
            }
            if (item is (<1) or > 10)
                throw new ArgumentOutOfRangeException(nameof(item), "Items must be between 1 and 10.");
            var shift = item == 1 ? 2 : item == 5 ? 1 : 0;
            ItemOffset = await Executor.Connection.PointerAll(Executor.ItemBlock, token).ConfigureAwait(false);
            List<InventoryPouch> items = await itemStructure.ReadGiftItem(ItemOffset, token).ConfigureAwait(false);
            ItemStructure.LogBag(items);
            DateTime date = DateTime.Now;
            ulong SetTime = 0;
            if (DateTime.TryParse(CurTime.Text, out var time))
            {
                var timeset = GetTimeSet(time);
                if (timeset == null)
                    return;
                SetTime = TimeUtil.GetTime(timeset.Value);                
            }
            else if (ulong.TryParse(CurTime.Text, out var ticks))
            {
                SetTime = ticks;
            }
            else
            {
                MessageBox.Show("Time Format is wrong!");
                EnableOptions();
                return;
            }
            await Executor.Click(A, 2_500, token).ConfigureAwait(false);
            await Executor.Click(A, 0_800, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
            ulong TimeSet = SetTime - 20;
            await Executor.SetCurrentTime(TimeSet, token).ConfigureAwait(false);
            while (date > await GetSwitchTime(token).ConfigureAwait(false) + TimeSpan.FromSeconds(1.0))
                await Task.Delay(0_500, token).ConfigureAwait(false);
            await Executor.Click(A, 5_500, token).ConfigureAwait(false);
            for (int i = 0; i < shift; i++)
                await Executor.Click(L, 0_800, token).ConfigureAwait(false);
            await Executor.Click(X, 0_800, token).ConfigureAwait(false);
            for (int i = 0; i < 2; i++)
                await Executor.Click(A, 0_600, token).ConfigureAwait(false);
            await Task.Delay(5_000, token).ConfigureAwait(false);
            for(int i = 0; i < 3;i++)
                await Executor.Click(A, 1_500, token).ConfigureAwait(false);
            await Executor.Click(A, 10_000, token).ConfigureAwait(false);
            await Executor.Click(A, 1_500, token).ConfigureAwait(false);
            await Executor.Click(A, Filter.IsFilterSet() && (Filter.TargetMode == PrintMode.BallBonus || Filter.TargetMode == PrintMode.ItemBonus) ? 20_000 : 5_000, token).ConfigureAwait(false);
            await Executor.Click(B, 3_000, token).ConfigureAwait(false);
            List<InventoryItem> DiffItems = await itemStructure.GetDiffItems(items, token).ConfigureAwait(false);
            ItemResult.Populate(DiffItems, "en");
        }
        private void PrintButton_Click(object sender, EventArgs e)
        {
            if (UseFilter.Checked)
                SearchItems();
            else
                SearchCurrentItem();
        }
        private void SearchCurrentItem()
        {
            if (PrintModeCombo.SelectedIndex < 0)
                PrintModeCombo.SelectedIndex = 0;
            if (!int.TryParse(PrintItemCombo.Text, out int items))
            {
                MessageBox.Show("PrintItemCount is Invalid!");
                return;
            }
            if (items is (<1) or > 10)
                throw new ArgumentOutOfRangeException(nameof(items), "Items must be between 1 and 10.");
            if(!ulong.TryParse(Seed.Text, out ulong curSeed))
            {
                MessageBox.Show("Current Seed Format is wrong!");
                return;
            }
            
            DisableOptins();
            Span<Item> itemSpan = stackalloc Item[items];
            ulong ticks = curSeed;
            var finalMode = ItemPrinter.Print(ticks, itemSpan, (PrintMode)PrintModeCombo.SelectedIndex);
            ItemResult.Populate(itemSpan, "en");
            MessageBox.Show($"Current {(TimeFormat.SelectedIndex == 0 ? "seed: " + ticks.ToString() : "Time: " + CurTime.Text)}, Next Mode: {finalMode}");
            EnableOptions();
        }
        private void SearchItems()
        {
            if (TimeZoneCombo.SelectedValue == null)
                return;
            if(!Filter.IsFilterSet())
            {
                MessageBox.Show("Filter is not set!");
                return;
            }
            if (!int.TryParse(PrintItemCombo.Text, out int items))
            {
                MessageBox.Show("PrintItemCount is Invalid!");
                return;
            }
            if (items is (<1) or > 10)
                throw new ArgumentOutOfRangeException(nameof(items), "Items must be between 1 and 10.");
            if(Filter.searchMode is SearchMode.SpecificItemCount && Filter.TargetCount <= 0)
            {
                MessageBox.Show("Target Item Count is not set!");
                return;
            }
            Span<Item> itemSpan = stackalloc Item[items];
            DisableOptins();
            ulong ticks = 0;
            int Count = 0;
            switch (Filter.searchMode)
            {
                case SearchMode.BonusSearch: ticks = ItemSeedSearcher.FindNextBonusMode(Filter.StartTicks, Filter.TargetMode, itemSpan, Filter.TargetItem, Filter.AdjustTime); break;
                case SearchMode.MaxSpecificItem: (ticks, Count) = ItemSeedSearcher.MaxResultsAny(Filter.StartTicks, Filter.StartTicks + Filter.Searchrange, itemSpan, Filter.CurrentMode, Filter.TargetItem); break;
                case SearchMode.MaxValuables: (ticks, Count) = ItemSeedSearcher.MaxValuablesAny(Filter.StartTicks, Filter.StartTicks + Filter.Searchrange, itemSpan); break;
                case SearchMode.SpecificItemCount: (ticks, Count) = ItemSeedSearcher.SpecificCountItem(Filter.StartTicks, Filter.StartTicks + Filter.Searchrange, Filter.TargetCount, itemSpan, Filter.CurrentMode, Filter.TargetItem); break;
            }
            Seed.Text = ticks.ToString();
            if (TimeFormat.SelectedIndex == 0)
            {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue);
                CurTime.Text = (ticks - info.BaseUtcOffset.TotalSeconds).ToString();                
            }
            else
            {
                /*DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime curTime = epoch.AddSeconds(ticks);
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue);
                curTime = TimeZoneInfo.ConvertTimeFromUtc(curTime, info);
                CurTime.Text = curTime.ToString("yyyy/MM/dd HH:mm:ss");*/
                CurTime.Text = TimeUtil.GetDateTime(ticks).ToString();
            }
            if (ticks > 0 && (Count > 0 || Filter.searchMode is SearchMode.BonusSearch))
            {
                ItemResult.Populate(itemSpan, "en");
                MessageBox.Show($"Target {(TimeFormat.SelectedIndex == 0 ? "Ticks" : "Time")}: {CurTime.Text}{(Count > 0 ? ", Target Item Count: " + Count.ToString() : "")}{(Filter.TargetModeIsSet() ? ", Target Mode: " + Filter.TargetMode.ToString() : "")}");                
            }
            else
            {
                ItemResult.Clear();
                MessageBox.Show("Target Seed is not found!");
            }                        
            EnableOptions();
        }
        private async void Reset_Click(object sender, EventArgs e)
        {
            DisableOptins();
            DateTime resetTime = DateTime.Now;
            try
            {
                var NTPTime = await Executor.ResetTimeNTP(token).ConfigureAwait(false);
                resetTime = GetTime(NTPTime);
                if (InvokeRequired)
                    Invoke(() => GetTimeString(resetTime));
                else
                    GetTimeString(resetTime);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                EnableOptions();
                return;
            }
            MessageBox.Show($"Reset Time{Environment.NewLine}{resetTime.ToString("yyyy/MM/dd HH:mm:ss")}", "Switch NTP SystemClock");
            EnableOptions();
        }
        private async Task<DateTime> GetSwitchTime(CancellationToken token)
        {
            var unixTime = await Executor.GetUnixTime(token).ConfigureAwait(false);
            var curTime = GetTime(unixTime);
            var displayTime = GetTimeLocal((ulong)unixTime);
            CurrentTimelabel.Text = "Current Time: " + displayTime.ToString("yyyy/MM/dd HH:mm:ss") + " Delay -1s";
            return curTime;
        }
        private void GetTimeString(DateTime time)
        {
            if (TimeZoneCombo.SelectedIndex < 0 || TimeZoneCombo.SelectedValue == null)
                return;
            TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue);
            if (TimeFormat.SelectedIndex == 0)
            {
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var Diff = time - epoch;
                CurTime.Text = $"{Diff.TotalSeconds}";
            }
            else
            {
                time = TimeZoneInfo.ConvertTimeFromUtc(time, info);
                CurTime.Text = time.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }
        private DateTime GetTime(long ticks)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime curTime = epoch.AddSeconds(ticks);
            return curTime;
        }
        private DateTime GetTimeLocal(ulong ticks)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime curTime = epoch.AddSeconds(ticks);
            TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue!);
            curTime = TimeZoneInfo.ConvertTimeFromUtc(curTime, info);
            return curTime;
        }
        private DateTime? GetTimeSet(long ticks)
        {
            if (TimeZoneCombo.SelectedIndex < 0 || TimeZoneCombo.SelectedValue == null)
                return null;
            TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue);
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(epoch, info);
            time = time.AddSeconds(ticks);
            time = TimeZoneInfo.ConvertTimeToUtc(time, info);
            return time;
        }
        private DateTime? GetTimeSet(DateTime time)
        {
            if (TimeZoneCombo.SelectedIndex < 0 || TimeZoneCombo.SelectedValue == null)
                return null;
            TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue);
            time = TimeZoneInfo.ConvertTimeToUtc(time, info);
            return time;
        }
        private void GetCurrentSeed()
        {
            if (TimeZoneCombo.SelectedIndex < 0 || TimeZoneCombo.SelectedValue == null)
                return;
            if(ulong.TryParse(CurTime.Text, out ulong ticks))
            {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue);
                var seed = ticks + info.BaseUtcOffset.TotalSeconds;
                MessageBox.Show($"Current Time Zone: {TimeZoneCombo.SelectedValue}, BaseUtc Offset(Seconds): {info.BaseUtcOffset.TotalSeconds}");
                Seed.Text = seed.ToString();
            }
            else if(DateTime.TryParse(CurTime.Text, out DateTime time))
            {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById((string)TimeZoneCombo.SelectedValue);
                var UTCTime = TimeZoneInfo.ConvertTimeToUtc(time, info);
                var DiffUtc = TimeUtil.GetTime(UTCTime);
                var DiffLocal = TimeUtil.GetTime(time);
                var SeedDiff = DiffLocal - DiffUtc;
                MessageBox.Show($"Current Time Zone: {TimeZoneCombo.SelectedValue}, BaseUtc Offset(Seconds): {SeedDiff}");
                Seed.Text = TimeUtil.GetTime(time).ToString();
            }
        }
        private void UseFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (UseFilter.Checked)
            {
                FilterButton.Enabled = true;
            }
            else
            {
                FilterButton.Enabled = false;
                Filter = new();
            }
        }
        private void FilterButton_Click(object sender, EventArgs e)
        {
            using SearchOptinsForm form = new(ref Filter);
            form.ShowDialog();
        }
        private void DisableOptins()
        {
            BackwardHours.Enabled = false;
            FowardHours.Enabled = false;
            ResetTimeButton.Enabled = false;
            ReadCurrentTimeButton.Enabled = false;
            FowardMinute.Enabled = false;
            BackwardMinute.Enabled = false;
            HoursNum.Enabled = false;
            MinutesNum.Enabled = false;
            CurTime.Enabled = false;
            TimeFormat.Enabled = false;
            SetTimeButton.Enabled = false;
            SetNTPTimeButton.Enabled = false;
            ItemPrinterRNG.Enabled = false;
            PrintButton.Enabled = false;
            PrintItemCombo.Enabled = false;
            PrintModeCombo.Enabled = false;
            UseFilter.Enabled = false;
            FilterButton.Enabled = false;
        }

        private void EnableOptions()
        {
            BackwardHours.Enabled = true;
            FowardHours.Enabled = true;
            ResetTimeButton.Enabled = true;
            ReadCurrentTimeButton.Enabled = true;
            FowardMinute.Enabled = true;
            BackwardMinute.Enabled = true;
            HoursNum.Enabled = true;
            MinutesNum.Enabled = true;
            CurTime.Enabled = true;
            TimeFormat.Enabled = true;
            SetTimeButton.Enabled = true;
            SetNTPTimeButton.Enabled = true;
            ItemPrinterRNG.Enabled = true;
            PrintButton.Enabled = true;
            PrintItemCombo.Enabled = true;
            PrintModeCombo.Enabled = true;
            UseFilter.Enabled = true;
            FilterButton.Enabled = UseFilter.Checked;
        }
    }
}
