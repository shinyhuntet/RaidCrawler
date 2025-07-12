using PKHeX.Core;
using pkNX.Structures.FlatBuffers.Gen9;
using System.Diagnostics;

namespace RaidCrawler.Core.Structures;

public class TeraEncounter : ITeraRaid
{
    public readonly EncounterTera9 Entity; // Raw data
    public readonly ulong DropTableFix;
    public readonly ulong DropTableRandom;
    public readonly ushort[] ExtraMoves;
    public ushort Species => Entity.Species;
    public byte Form => Entity.Form;
    public byte Gender => Entity.Gender;
    public AbilityPermission Ability => Entity.Ability;
    public byte FlawlessIVCount => Entity.FlawlessIVCount;
    public Shiny Shiny => Entity.Shiny;
    public byte Level => Entity.Level;
    public ushort Move1 => Entity.Moves.Move1;
    public ushort Move2 => Entity.Moves.Move2;
    public ushort Move3 => Entity.Moves.Move3;
    public ushort Move4 => Entity.Moves.Move4;
    public byte Stars => Entity.Stars;
    public byte RandRate => Entity.RandRate;
    ushort[] ITeraRaid.ExtraMoves => ExtraMoves;

    public TeraEncounter(EncounterTera9 enc, ulong fixedrewards, ulong lotteryrewards, ushort[] extras)
    {
        Entity = enc;
        DropTableFix = fixedrewards;
        DropTableRandom = lotteryrewards;
        ExtraMoves = extras.Where(z => z != 0 && !Entity.Moves.Contains(z)).Distinct().ToArray();
        if (ExtraMoves.Length > 4)
            Debug.WriteLine(ExtraMoves);
    }

    public static TeraEncounter[] GetAllEncounters(string[] resources, TeraRaidMapParent map)
    {
        var data = FlatbufferDumper.DumpBaseROMRaids(resources);
        var encs = EncounterTera9.GetArray(data[0], map);
        var extras = data[1];
        var rewards = TeraDistribution.GetRewardTables(data[2]);
        var result = new TeraEncounter[encs.Length];
        for (int i = 0; i < encs.Length; i++)
        {
            var item1 = rewards[i].Item1;
            var item2 = rewards[i].Item2;
            var extra = TeraDistribution.GetExtraMoves(extras[(12 * i)..]);
            result[i] = new TeraEncounter(encs[i], item1, item2, extra);
        }
        return result;
    }

    public static List<(int, int, int)> GetRewards(TeraEncounter enc, uint seed, int teratype, IReadOnlyList<RaidFixedRewards>? fixed_rewards, IReadOnlyList<RaidLotteryRewards>? lottery_rewards, int boost)
    {
        GameStrings Strings = GameInfo.GetStrings("en");
        //var dbgFile = "Raid Rewards Normal.txt";
        //var msg = $"{Environment.NewLine}";
        List<(int, int, int)> result = new();
        if (lottery_rewards is null || fixed_rewards is null)
            return result;

        /*msg += $"{Environment.NewLine}Raid Species:{(Species)enc.Species}";
        msg += $"{Environment.NewLine}Fix Rewards Table:{enc.DropTableFix}, Lottery Rewards Table:{enc.DropTableRandom}{Environment.NewLine}";*/
        var fixed_table = fixed_rewards.FirstOrDefault(z => z.TableName == enc.DropTableFix);
        if (fixed_table is null)
            return result;

        var lottery_table = lottery_rewards.FirstOrDefault(z => z.TableName == enc.DropTableRandom);
        if (lottery_table is null)
            return result;

        // fixed reward
        //msg += $"{Environment.NewLine}Determined Fix Rewards Table Num:{fixed_table.TableName}";
        for (int i = 0; i < RaidFixedRewards.Count; i++)
        {
            var item = fixed_table.GetReward(i);
            if (item is null or {Category : 0, ItemID : 0 })
                continue;

            result.Add((item.ItemID == 0 ? item.Category == 2 ? Rewards.GetTeraShard(teratype) : Rewards.GetMaterial(enc.Species) : item.ItemID, item.Num, item.SubjectType));
            var itemName = result.Last().Item1 switch
            {
                10000 => "Material",
                20000 => "Tera Shard",
                _ => Rewards.IsTM(result.Last().Item1) ? Rewards.GetNameTM(result.Last().Item1, Strings.Item, Strings.Move, Rewards.TMIndexes) : Strings.Item[result.Last().Item1]
            };
            var subject = result.Last().Item3 switch
            {
                1 => "Host",
                2 => "Client",
                3 => "Once",
                _ => "All"
            };
            //msg += $"{Environment.NewLine}Fix Reward {result.Count}{Environment.NewLine}Item:{itemName}, Num:{result.Last().Item2}, SubCategory:{subject}";
        }
        
        // lottery reward
        //msg += $"{Environment.NewLine}{Environment.NewLine}Determined Lottery Rewards Table Num:{lottery_table.TableName}";
        var total = 0;
        for (int i = 0; i < RaidLotteryRewards.RewardItemCount; i++)
        {
            var drop = lottery_table.GetRewardItem(i);
            if (drop is null or {ItemID : 0, Category : 0 })
                continue;
            total += drop.Rate;
            var item = drop.ItemID == 0 ? drop.Category == 2 ? Rewards.GetTeraShard(teratype) : Rewards.GetMaterial(enc.Species) : drop.ItemID;
            var itemName = item switch
            {
                10000 => "Material",
                20000 => "Tera Shard",
                _ => Rewards.IsTM(item) ? Rewards.GetNameTM(item, Strings.Item, Strings.Move, Rewards.TMIndexes) : Strings.Item[item]
            };
            //msg += $"{($"{Environment.NewLine}Lottery Reward {i + 1}{Environment.NewLine}Item:{itemName}, DropRate:{drop.Rate}")}";
        }
        var rand = new Xoroshiro128Plus(seed);
        var count = (int)rand.NextInt(100); // sandwich = extra rolls? how does this work? is this even 100?
        //msg += $"{Environment.NewLine}TotalItemRand:{total}{Environment.NewLine}{Environment.NewLine}ItemCountRandValue:{count}";
        count = Rewards.GetRewardCount(count, enc.Stars) + boost;
        //msg += $"{Environment.NewLine}Lottery Rewards Item Count:{count}";
        for (int i = 0; i < count; i++)
        {
            var roll = (int)rand.NextInt((ulong)total);
            //msg += $"{Environment.NewLine}Lottery Item RandValue is {roll}!";
            for (int j = 0; j < DeliveryRaidLotteryRewardItem.RewardItemCount; j++)
            {
                var item = lottery_table.GetRewardItem(j);
                if (item is null or { ItemID: 0, Category: 0 })
                    continue;

                if (roll < item.Rate)
                {
                    if (item.Category == 0)
                        result.Add((item.ItemID, item.Num, 0));
                    else if (item.Category == 1) 
                        result.Add(item.ItemID == 0 ? (Rewards.GetMaterial(enc.Species), item.Num, 0) : (item.ItemID, item.Num, 0));
                    else 
                        result.Add(item.ItemID == 0 ? (Rewards.GetTeraShard(teratype), item.Num, 0) : (item.ItemID, item.Num, 0));
                    var ItemIndex = item.ItemID == 0 ? item.Category == 1 ? Rewards.GetMaterial(enc.Species) : Rewards.GetTeraShard(teratype) : item.ItemID;
                    var itemName = ItemIndex switch
                    {
                        10000 => "Material",
                        20000 => "Tera Shard",
                        _ => Rewards.IsTM(ItemIndex) ? Rewards.GetNameTM(ItemIndex, Strings.Item, Strings.Move, Rewards.TMIndexes) : Strings.Item[ItemIndex]
                    };
                    //msg += $"{Environment.NewLine}Lottery Reward {i + 1}{Environment.NewLine}Item:{itemName}, Num:{item.Num}";
                    break;
                }
                roll -= item!.Rate;
            }
        }
        //File.AppendAllText(dbgFile, msg);
        return Rewards.ReorderRewards(result);
    }
}
