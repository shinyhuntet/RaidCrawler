using RaidCrawler.Core.Structures;
using RaidCrawler.WinForms.Util;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace RaidCrawler.WinForms.SubForms
{
    public partial class SearchOptinsForm : Form
    {
        private bool TickValueChanged = false;
        private bool DateTimeValueChanged = false;
        private PrintMode PreviousMode;
        private SearchFilter filter;
        public SearchOptinsForm(ref SearchFilter Filter)
        {
            InitializeComponent();
            filter = Filter;
            TicksNum.Value = TimeUtil.GetTime(DateTime.UtcNow);
            CurrentModeCombo.DataSource = Enum.GetValues<PrintMode>().ToList();
            SearchModeCombo.DataSource = Enum.GetValues<SearchMode>().ToList();
            PrintModeCombo.DataSource = Enum.GetValues<PrintMode>().ToList();
            var item = ComboItem.GetList((PrintMode)CurrentModeCombo.SelectedIndex == PrintMode.BallBonus ? ItemPrinter.Balls : ItemPrinter.Items);
            ItemsCombo.InitializeBinding();
            ItemsCombo.DataSource = new BindingSource(item, string.Empty);
            SetFilter(filter);
        }
        private void SearchModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((SearchMode)SearchModeCombo.SelectedIndex is SearchMode.BonusSearch)
            {
                PrintModeCombo.Enabled = true;
                AdjustTime.Enabled = true;
                TargetCountNum.Enabled = false;
                CurrentModeCombo.SelectedIndex = (int)PrintMode.Regular;
            }
            else
            {
                TargetCountNum.Enabled = false;
                PrintModeCombo.Enabled = false;
                AdjustTime.Enabled = false;
                if ((SearchMode)SearchModeCombo.SelectedIndex is SearchMode.MaxValuables)                
                    CurrentModeCombo.SelectedIndex = (int)PrintMode.BallBonus;                                    
                else if ((SearchMode)SearchModeCombo.SelectedIndex is SearchMode.SpecificItemCount)                
                    TargetCountNum.Enabled = true;                                
            }
        }
        private void CurrentModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboItem[] item = [];
            if (PreviousMode is PrintMode.BallBonus)
            {
                item = ComboItem.GetList(ItemPrinter.Items);
                ItemsCombo.InitializeBinding();
                ItemsCombo.DataSource = new BindingSource(item, string.Empty);
            }
            else if ((PrintMode)CurrentModeCombo.SelectedIndex is PrintMode.BallBonus)
            {
                item = ComboItem.GetList(ItemPrinter.Balls);
                ItemsCombo.InitializeBinding();
                ItemsCombo.DataSource = new BindingSource(item, string.Empty);
            }
            PreviousMode = (PrintMode)CurrentModeCombo.SelectedIndex;
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (CurrentModeCombo.SelectedIndex < 0)
            {
                MessageBox.Show("Current Print Mode is not set!");
                return;
            }
            if (SearchModeCombo.SelectedIndex < 0)
            {
                MessageBox.Show("Search Mode is not set!");
                return;
            }
            filter.Format = RB_Tick.Checked ? TimeFormat.Ticks : TimeFormat.DateString;
            filter.StartTicks = RB_Tick.Checked ? (ulong)TicksNum.Value : TimeUtil.GetTime(DateTime.UtcNow);
            filter.Searchrange = (ulong)RangeNum.Value;
            filter.CurrentMode = (PrintMode)CurrentModeCombo.SelectedIndex;
            filter.searchMode = (SearchMode)SearchModeCombo.SelectedIndex;
            filter.TargetMode = PrintModeCombo.Enabled ? (PrintMode)PrintModeCombo.SelectedIndex : PrintMode.Regular;
            filter.AdjustTime = AdjustTime.Enabled ? AdjustTime.Checked : false;
            filter.TargetItem = WinFormsUtil.GetIndex(ItemsCombo);
            filter.TargetCount = TargetCountNum.Enabled && TargetCountNum.Value > 0 ? (int)TargetCountNum.Value : -1;
            Close();
        }
        private void SetFilter(SearchFilter filter)
        {
            RB_Tick.Checked = filter.Format == TimeFormat.Ticks;
            RB_Date.Checked = !RB_Tick.Checked;
            TicksNum.Value = filter.StartTicks;
            RangeNum.Value = filter.Searchrange;
            CurrentModeCombo.SelectedIndex = (int)filter.CurrentMode;
            SearchModeCombo.SelectedIndex = (int)filter.searchMode;
            PrintModeCombo.SelectedIndex = filter.searchMode == SearchMode.BonusSearch ? (int)filter.TargetMode : -1;
            AdjustTime.Checked = filter.AdjustTime;
            ItemsCombo.SelectedValue = filter.TargetItem;
            TargetCountNum.Value = filter.TargetCount < 0 ? 0 : filter.TargetCount;
        }
        private void TicksNum_ValueChanged(object sender, EventArgs e)
        {
            if (TickValueChanged)
            {
                TickValueChanged = false;
                return;
            }
            TicksNum.Value = TimeUtil.IsValidSeed((ulong)TicksNum.Value) ? TicksNum.Value : TimeUtil.MaxSeed;
            TimeText.Text = TimeUtil.GetDateTime((ulong)TicksNum.Value).ToString("yyyy-MM-dd HH:mm:ss");
            DateTimeValueChanged = true;
        }
        private void TimeText_TextChanged(object sender, EventArgs e)
        {
            if (DateTimeValueChanged)
            {
                DateTimeValueChanged = false;
                return;
            }
            if (DateTime.TryParse(TimeText.Text, out var curDate))
            {
                var ticks = TimeUtil.GetTime(curDate);
                ticks = TimeUtil.IsValidSeed(ticks) ? ticks : TimeUtil.MaxSeed;
                TicksNum.Text = ticks.ToString();
                TickValueChanged = true;
            }
        }
    }
}
