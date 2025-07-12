using PKHeX.Core;
using RaidCrawler.Core.Structures;
using RaidCrawler.Core;
using SysBot.Base;
using System.Data;
using System.Security.Policy;

namespace RaidCrawler.WinForms.Controls;

public partial class ItemResultGridView : UserControl
{
    public ItemResultGridView()
    {
        InitializeComponent();
    }
    public void Populate(List<InventoryItem> itemSpan, string language)
    {
        try
        {
            var rows = DGV_View.Rows;
            Image img = null!;
            rows.Clear();
            LogUtil.LogText("Row Cleared!");
            rows = rows == null ? DGV_View.Rows : rows;
            var count = 1;
            string url = string.Empty;
            foreach (var item in itemSpan)
            {
                if (Rewards.IsTM(item.Index))
                {
                    LogUtil.LogText($"Adding Item{count}!{Environment.NewLine}Item is TM!{Environment.NewLine}Item Name: {GameInfo.GetStrings(language).itemlist[item.Index]}, Count: {item.Count}");
                    img = Properties.Resources.tm;
                }
                else if (ItemStructure.IsMaterial(item))
                {
                    LogUtil.LogText($"Adding Item{count}!{Environment.NewLine}Item is material!{Environment.NewLine}Item Name: {GameInfo.GetStrings(language).itemlist[item.Index]}, Count: {item.Count}");
                    img = Properties.Resources.material;
                }
                else
                {
                    url = $"https://raw.githubusercontent.com/kwsch/PKHeX/master/PKHeX.Drawing.PokeSprite/Resources/img/Artwork Items/aitem_{item.Index}.png";
                    LogUtil.LogText($"Adding Item{count}!{Environment.NewLine}Item Name: {GameInfo.GetStrings(language).itemlist[item.Index]}, Count: {item.Count}{Environment.NewLine}URL: {url}");
                    img = GetItemImage(url);
                }
                LogUtil.LogText("Finish getting item image!");
                rows.Add(item.Count, img, GameInfo.GetStrings(language).itemlist[item.Index]);
                LogUtil.LogText($"Item {count} is Added!");
                count++;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }

    }
    public void Populate(Span<Item> itemSpan, string language)
    {
        try
        {
            var rows = DGV_View.Rows;
            Image img = null!;
            rows.Clear();
            LogUtil.LogText("Row Cleared!");
            rows = rows == null ? DGV_View.Rows : rows;
            var count = 1;
            string url = string.Empty;
            foreach (var item in itemSpan)
            {
                if (Rewards.IsTM(item.ItemId))
                {
                    LogUtil.LogText($"Adding Item{count}!{Environment.NewLine}Item is TM!{Environment.NewLine}Item Name: {GameInfo.GetStrings(language).itemlist[item.ItemId]}, Count: {item.Count}");
                    img = Properties.Resources.tm;
                }
                else if (ItemStructure.IsMaterial(item))
                {
                    LogUtil.LogText($"Adding Item{count}!{Environment.NewLine}Item is material!{Environment.NewLine}Item Name: {GameInfo.GetStrings(language).itemlist[item.ItemId]}, Count: {item.Count}");
                    img = Properties.Resources.material;
                }
                else
                {
                    url = $"https://raw.githubusercontent.com/kwsch/PKHeX/master/PKHeX.Drawing.PokeSprite/Resources/img/Artwork Items/aitem_{item.ItemId}.png";
                    LogUtil.LogText($"Adding Item{count}!{Environment.NewLine}Item Name: {GameInfo.GetStrings(language).itemlist[item.ItemId]}, Count: {item.Count}{Environment.NewLine}URL: {url}");
                    img = GetItemImage(url);
                }
                LogUtil.LogText("Finish getting item image!");
                rows.Add(item.Count, img, GameInfo.GetStrings(language).itemlist[item.ItemId]);
                LogUtil.LogText($"Item {count} is Added!");
                count++;
            }
        }
        catch(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
    public Image GetItemImage(string url)
    {
        PictureBox pictureBox = new();
        pictureBox.Load(url);
        return pictureBox.Image!;
    }
    public void Clear() => DGV_View.Rows.Clear();
    public void ChangeScroll()
    {
        DGV_View.ScrollBars = ScrollBars.Both;
    }
}

