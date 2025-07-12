using PKHeX.Core;

namespace RaidCrawler.Core.Structures;
public class LotteryRoot
{
    public required LotteryTable[] Table { get; set; }
}

public class LotteryTable
{
    public required LotteryItemParam Param { get; set; }
}

public class LotteryItemParam
{
    public required string FlagName { get; set; }
    public required LotteryItemValue Value { get; set; }
}

public class LotteryItemValue
{
    public ushort ItemId { get; set; }
    public int ProductionPriority { get; set; }
    public int EmergePercent { get; set; }
    public uint LotteryItemNumMin { get; set; }
    public uint LotteryItemNumMax { get; set; }

    // Manual Tags
    public uint MinRoll { get; set; }
    public uint MaxRoll { get; set; }

    public override string ToString() => $"[{EmergePercent}: {MinRoll}-{MaxRoll}] ({ProductionPriority}) {ItemId} {GameInfo.GetStrings("en").itemlist[ItemId]}";
}
public class BallRoot
{
    public required BallTable[] Table { get; set; }
}

public class BallTable
{
    public required BallParam Param { get; set; }
}

public class BallParam
{
    public required LotteryItemValue[] Table { get; set; }
}
public enum PrintMode
{
    /// <summary>
    /// Regular print mode. Can trigger <see cref="ItemBonus"/> or <see cref="BallBonus"/>.
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Same as regular mode, but prints 2x the amount of items.
    /// </summary>
    ItemBonus = 1,

    /// <summary>
    /// Prints from a special table that only contains balls.
    /// </summary>
    BallBonus = 2,
}
/// <summary>
/// Simple struct to represent a printed item result.
/// </summary>
/// <param name="ItemId">Item index that was printed.</param>
/// <param name="Count">Count of items for this print result.</param>
public readonly record struct Item(ushort ItemId, ushort Count);

