using PKHeX.Core;
using static RaidCrawler.Core.Structures.PrintMode;

namespace RaidCrawler.Core.Structures;

public static class ItemSeedSearcher
{
    private const PrintMode Mode = Regular;
    public static ulong FindNextBonusMode(ulong startTicks, PrintMode targetMode, Span<Item> best, int itemId = 0, bool checkAdjustment = false)
    {
        if (targetMode is not (ItemBonus or BallBonus))
            throw new ArgumentException("Invalid target mode", nameof(targetMode));
        if (itemId != 0 && !ItemPrinter.TableHasItem(Regular, (ushort)itemId))
            throw new ArgumentException("Item ID not found in the table", nameof(itemId));

        Span<Item> result = stackalloc Item[best.Length]; // best case scenario
        while (true)
        {
            var resultMode = ItemPrinter.Print(startTicks, result, Regular);
            if (resultMode != targetMode || (itemId != 0 && !result.ToArray().Any(x => x.ItemId == itemId)))
            {
                startTicks++;
                continue;
            }
            if (checkAdjustment && !IsPassAdjacent(startTicks, result, targetMode))
            {
                startTicks++;
                continue;
            }
            result.CopyTo(best);
            return startTicks;
        }
    }
    private static bool IsPassAdjacent(ulong check, Span<Item> tmp, PrintMode mode)
    {
        // Check for adjacent results to ensure the bonus mode is the same.
        // +/- 2 seconds should be enough to cover even the most inconsistent user.
        // They can then use the adjacent results to see how far off they were while still not wasting resources.
        for (int j = -2; j <= +2; j++)
        {
            var seed = check + unchecked((ulong)j);
            var adj = ItemPrinter.Print(seed, tmp, Mode);
            if (adj != mode)
                return false;
        }
        // Since `tmp` contains the result at +2sec, recalculate the result for +/- 0.
        ItemPrinter.Print(check, tmp, Mode);
        return true;
    }
    public static (ulong Ticks, int Count) SpecificCountItem(ulong start, ulong end, int wantedcount, Span<Item> best, PrintMode mode, params int[] find)
    {
        ulong result = 0;
        int count = -1;
        
        // Just run on a single thread for now.
        Span<Item> items = stackalloc Item[best.Length];
        for (ulong i = start; i <= end; i++)
        {
            _ = ItemPrinter.Print(i, items, mode);
            int c = 0;
            foreach (var item in items)
            {
                if (find.Contains(item.ItemId))
                    c += item.Count;
            }
            
            if (c >= wantedcount)
            {
                count = c;
                result = i;
                items.CopyTo(best);
                return (result, count);
            }
        }
        return (result, count);
    }

    public static (ulong Ticks, int Count) MaxValuablesAny(ulong start, ulong end, Span<Item> best)
    {
        ulong result = 0;
        int count = -1;
        List<int> NewItem = [];

        // Just run on a single thread for now.
        Span<Item> items = stackalloc Item[best.Length];
        for (ulong i = start; i <= end; i++, NewItem = [])
        {
            _ = ItemPrinter.Print(i, items, BallBonus);
            int c = 0;
            foreach (var item in items)
            {
                if (!NewItem.Contains(item.ItemId))
                {
                    NewItem.Add(item.ItemId);
                    c++;
                }
            }

            if (c <= count)
                continue;

            count = c;
            result = i;
            items.CopyTo(best);
        }
        return (result, count);
    }
    public static (ulong Ticks, int Count) MaxResultsAny(ulong start, ulong end, Span<Item> best, PrintMode mode, params int[] find)
    {
        ulong result = 0;
        int count = -1;

        // Just run on a single thread for now.
        Span<Item> items = stackalloc Item[best.Length];
        for (ulong i = start; i <= end; i++)
        {
            _ = ItemPrinter.Print(i, items, mode);
            int c = 0;
            foreach (var item in items)
            {
                if (find.Contains(item.ItemId))
                    c += item.Count;
            }

            if (c <= count)
                continue;

            count = c;
            result = i;
            items.CopyTo(best);
        }
        return (result, count);
    }

}
public enum SearchMode
{
    BonusSearch = 0,
    MaxSpecificItem = 1,
    MaxValuables = 2,
    SpecificItemCount = 3,
}
public enum TimeFormat
{
    DateString = 0,
    Ticks = 1,
}
