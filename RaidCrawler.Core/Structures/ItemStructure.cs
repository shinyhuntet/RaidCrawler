using PKHeX.Core;
using RaidCrawler.Core.Connection;
using SysBot.Base;

namespace RaidCrawler.Core.Structures;

public class ItemStructure
{
    private readonly ConnectionWrapperAsync Executor;
    private ulong ItemBlockOffset;
    public ItemStructure(ConnectionWrapperAsync executor)
    {
        Executor = executor;
    }
    public static void LogBag(List<InventoryPouch> pouches)
    {
        foreach(InventoryPouch pouch in pouches)
        {
            LogAllItems(pouch.Items, pouch.Type);
        }
    }
    private static void LogAllItems(InventoryItem[] items, InventoryType type)
    {
        var ItemList = GameInfo.GetStrings("en").itemlist;
        int ItemCount = 1;
        LogUtil.LogText($"{Environment.NewLine}All Bag Items(Type: {type})");
        foreach (var item in items)
        {
            if (item.Index <= 0 || item.Count <= 0)
                continue;
            LogUtil.LogText($"Item {ItemCount}: {ItemList[item.Index]}(Item Number: {item.Index}), Count: {item.Count}");
            ItemCount++;
        }
        LogUtil.LogText("\n");
    }
    private List<InventoryItem> GrabItemsDiff(InventoryItem[] Newitems, InventoryItem[] Olditems)
    {
        var Diffitems = Newitems.Where(x => !Olditems.Contains(x) && x.Count > 0).ToArray();
        var DiffitemsOld = Olditems.Where(x => !Newitems.Contains(x) && x.Count > 0).ToArray();
        var ChangeItemIndex = DiffitemsOld.Select(x => x.Index).ToList();
        List<InventoryItem> NewItems = [];
        foreach (var item in Diffitems)
        {
            if (!ChangeItemIndex.Contains(item.Index))
            {
                NewItems.Add(item);
            }
            else
            {
                var CountChangeItem = DiffitemsOld.Where(z => z.Index == item.Index).FirstOrDefault();
                if (CountChangeItem == null)
                    continue;
                item.Count -= CountChangeItem.Count;
                NewItems.Add(item);
            }
        }
        return NewItems;
    }
    private List<InventoryItem> GrabAllDiffItems(InventoryItem[] NewItems, InventoryItem[] OldItems)
    {
        List<InventoryItem> ChangedItems = [];
        var Diffitems = NewItems.Where(z => !OldItems.Contains(z) && z.Index > 0).ToArray();
        foreach(var item in Diffitems)
        {
            var CountChangeItem = OldItems.Where(z => z.Index == item.Index).FirstOrDefault();
            if (CountChangeItem == null)
                continue;
            item.Count -= CountChangeItem.Count;
            ChangedItems.Add(item);
        }
        return ChangedItems;
           
    }
    private async Task<SAV9SV> GetFakeTrainerSAVSV(CancellationToken token)
    {
        var sav = new SAV9SV();
        var info = sav.MyStatus;
        var read = await Executor.Connection.PointerPeek(info.Data.Length, Executor.MyStatusPointerSV, token).ConfigureAwait(false);
        read.CopyTo(info.Data);
        return sav;
    }
    public static bool IsMaterial(InventoryItem item)
    {
        return ((item.Index >= 1956 && item.Index <= 2099) || (item.Index >= 2103 && item.Index <= 2123) || (item.Index >= 2126 && item.Index <= 2137) || (item.Index >= 2156 && item.Index <= 2159) || (item.Index >= 2438 && item.Index <= 2521) || item.Index == 10000);
    }
    public static bool IsMaterial(Item item)
    {
        return ((item.ItemId >= 1956 && item.ItemId <= 2099) || (item.ItemId >= 2103 && item.ItemId <= 2123) || (item.ItemId >= 2126 && item.ItemId <= 2137) || (item.ItemId >= 2156 && item.ItemId <= 2159) || (item.ItemId >= 2438 && item.ItemId <= 2521) || item.ItemId == 10000);
    }
    public async Task<List<InventoryPouch>> ReadGiftItem(ulong ItemOffset, CancellationToken token)
    {
        SAV9SV TrainerSav = await GetFakeTrainerSAVSV(token).ConfigureAwait(false);
        while (ItemOffset == 0)
        {
            await Task.Delay(0_050, token).ConfigureAwait(false);
            ItemOffset = await Executor.Connection.PointerAll(Executor.ItemBlock, token).ConfigureAwait(false);
        }
        var data = await Executor.Connection.ReadBytesAbsoluteAsync(ItemOffset, TrainerSav.Items.Data.Length, token).ConfigureAwait(false);
        data.CopyTo(TrainerSav.Items.Data);
        return TrainerSav.Inventory.ToList();
    }
    public async Task<List<InventoryItem>> GetDiffItems(List<InventoryPouch> ItemData, CancellationToken token)
    {
        ItemBlockOffset = await Executor.Connection.PointerAll(Executor.ItemBlock, token).ConfigureAwait(false);
        List<InventoryPouch> ItemDataNew = await ReadGiftItem(ItemBlockOffset, token).ConfigureAwait(false);
        List<InventoryItem> DiffItems = [];
        int attempts = 0;
        while (ItemDataNew.SequenceEqual(ItemData))
        {
            LogUtil.LogText("Item sequence is same! Reloading...");
            await Task.Delay(0_050).ConfigureAwait(false);
            ItemDataNew = await ReadGiftItem(ItemBlockOffset, token).ConfigureAwait(false);
            attempts++;
            if (attempts >= 60)
                break;
        }
        if (!ItemDataNew.SequenceEqual(ItemData))
        {
            LogUtil.LogText("New Items is Found!");
            LogBag(ItemDataNew);
            LogUtil.LogText("Gift Item Found!");
            for (int i = 0; i < Math.Min(ItemDataNew.Count, ItemData.Count); i++)
            {
                var diffItems = GrabAllDiffItems(ItemDataNew[i].Items, ItemData[i].Items);
                var success = diffItems.FirstOrDefault() != null && diffItems.Count > 0;
                if (!success)
                    continue;
                DiffItems = DiffItems.Concat(diffItems).ToList();
            }
        }
        return DiffItems;
    }
}
