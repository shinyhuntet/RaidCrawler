using RaidCrawler.Core.Structures;

namespace RaidCrawler.WinForms.Util;

public class SearchFilter
{
    public SearchMode searchMode { get; set; }
    public PrintMode CurrentMode { get; set; }
    public PrintMode TargetMode { get; set; }
    public int TargetItem {  get; set; }
    public int TargetCount {  get; set; }
    public ulong StartTicks { get; set; }
    public TimeFormat Format { get; set; }
    public ulong Searchrange { get; set; }
    public bool AdjustTime { get; set; }

    public bool IsFilterSet()
    {
        return (searchMode <= SearchMode.SpecificItemCount && searchMode >= SearchMode.BonusSearch) && (CurrentMode <= PrintMode.BallBonus && CurrentMode >= PrintMode.Regular);
    }
    public bool TargetModeIsSet()
    {
        return TargetMode <= PrintMode.BallBonus && TargetMode >= PrintMode.ItemBonus;
    }
}
