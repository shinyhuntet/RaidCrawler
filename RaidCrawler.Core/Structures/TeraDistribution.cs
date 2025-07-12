using PKHeX.Core;
using pkNX.Structures.FlatBuffers.Gen9;
using System.Diagnostics;
using static System.Buffers.Binary.BinaryPrimitives;

namespace RaidCrawler.Core.Structures;

public class TeraDistribution : ITeraRaid
{
    public readonly EncounterDist9 Entity; // Raw data
    public readonly ulong DropTableFix;
    public readonly ulong DropTableRandom;
    public readonly ushort[] ExtraMoves;
    public readonly sbyte DeliveryGroupID;

    public ushort Species => Entity.Species;
    public byte Form => Entity.Form;
    public byte Gender => Entity.Gender;
    public AbilityPermission Ability => Entity.Ability;
    public byte FlawlessIVCount => Entity.FlawlessIVCount;
    public Shiny Shiny => Entity.Shiny;
    public Nature Nature => Entity.Nature;
    
    public byte Level => Entity.Level;
    public IndividualValueSet IVs => Entity.IVs;
    public ushort Move1 => Entity.Moves.Move1;
    public ushort Move2 => Entity.Moves.Move2;
    public ushort Move3 => Entity.Moves.Move3;
    public ushort Move4 => Entity.Moves.Move4;
    public byte Stars => Entity.Stars;
    public byte RandRate => Entity.RandRate;
    ushort[] ITeraRaid.ExtraMoves => ExtraMoves;

    public static bool AvailableInGame(ITeraRaid9 enc, string game) => enc switch
    {
        EncounterDist9 encd => game switch
        {
            "Scarlet" => encd.RandRate0TotalScarlet + encd.RandRate1TotalScarlet + encd.RandRate2TotalScarlet + encd.RandRate3TotalScarlet != 0,
            "Violet" => encd.RandRate0TotalViolet + encd.RandRate1TotalViolet + encd.RandRate2TotalViolet + encd.RandRate3TotalViolet != 0,
            _ => false,
        },
        EncounterMight9 encm => game switch
        {
            "Scarlet" => encm.RandRate0TotalScarlet + encm.RandRate1TotalScarlet + encm.RandRate2TotalScarlet + encm.RandRate3TotalScarlet != 0,
            "Violet" => encm.RandRate0TotalViolet + encm.RandRate1TotalViolet + encm.RandRate2TotalViolet + encm.RandRate3TotalViolet != 0,
            _ => false,
        },
        _ => false,
    };
    public static bool AvailableInStage(ITeraRaid9 enc, string game, int stage) => enc switch
     {
         EncounterDist9 encd => game switch
         {
             "Scarlet" => encd.GetRandRateTotalScarlet(stage) > 0,
             "Violet" => encd.GetRandRateTotalViolet(stage) > 0,
             _ => false,
         },
         EncounterMight9 encm => game switch
         {
             "Scarlet" => encm.GetRandRateTotalScarlet(stage) > 0,
             "Violet" => encm.GetRandRateTotalViolet(stage) > 0,
             _ => false,
         },
         _ => false,
     };
    public TeraDistribution(EncounterDist9 enc, ulong fixedrewards, ulong lotteryrewards, ushort[] extras, sbyte group)
    {
        Entity = enc;
        DropTableFix = fixedrewards;
        DropTableRandom = lotteryrewards;
        ExtraMoves = extras.Where(z => z != 0 && !Entity.Moves.Contains(z)).Distinct().ToArray();
        DeliveryGroupID = group;
        if (ExtraMoves.Length > 4)
            Debug.WriteLine(ExtraMoves);
    }

    public static TeraDistribution[] GetAllEncounters(ReadOnlyMemory<byte> encounters)
    {
        var all = FlatbufferDumper.DumpDistributionRaids(encounters);
        var type2 = EncounterDist9.GetArray(all[0]);
        var rewards2 = GetRewardTables(all[2]);
        var extra2 = all[4];
        var group2 = all[6];
        var result = new TeraDistribution[type2.Length];
        for (int i = 0; i < result.Length; i++)
        {
            var i1 = rewards2[i].Item1;
            var i2 = rewards2[i].Item2;
            var extras = extra2[(12 * i)..];
            result[i] = new TeraDistribution(type2[i], i1, i2, GetExtraMoves(extras), (sbyte)group2[i]);
        }
        return result;
    }

    public static (ulong, ulong)[] GetRewardTables(ReadOnlySpan<byte> rewards)
    {
        var count = rewards.Length / 0x10;
        var result = new (ulong, ulong)[count];
        for (int i = 0; i < result.Length; i++)
        {
            var reward1 = ReadUInt64LittleEndian(rewards[(0x10 * i)..]);
            var reward2 = ReadUInt64LittleEndian(rewards[((0x10 * i) + 0x8)..]);
            result[i] = (reward1, reward2);
        }
        return result;
    }

    public static ushort[] GetExtraMoves(ReadOnlySpan<byte> extra)
    {
        var result = new ushort[6];
        for (int i = 0; i < result.Length; i++)
            result[i] = ReadUInt16LittleEndian(extra[(0x2 * i)..]);
        return result;
    }

    public static List<(int, int, int)> GetRewards(TeraDistribution enc, uint seed, int teratype, IReadOnlyList<DeliveryRaidFixedRewardItem>? fixed_rewards, IReadOnlyList<DeliveryRaidLotteryRewardItem>? lottery_rewards, int boost)
    {
        GameStrings Strings = GameInfo.GetStrings("en");
        /*var dbgFile = "Raid Rewards Dist9.txt";
        var msg = $"{Environment.NewLine}";*/
        List<(int, int, int)> result = [];
        if (lottery_rewards is null)
            return result;

        if (fixed_rewards is null)
            return result;

        /*msg += $"{Environment.NewLine}Raid Species:{(Species)enc.Species}, DeleveryGroupID:{enc.DeliveryGroupID}";
        msg += $"{Environment.NewLine}Fix Rewards Table:{enc.DropTableFix}, Lottery Rewards Table:{enc.DropTableRandom}{Environment.NewLine}";*/
        var fixed_table = fixed_rewards.FirstOrDefault(z => z.TableName == enc.DropTableFix);
        if (fixed_table is null)
            return result;

        //msg += $"{Environment.NewLine}Determined Fix Rewards Table Num:{fixed_table.TableName}";
        var lottery_table = lottery_rewards.FirstOrDefault(z => z.TableName == enc.DropTableRandom);
        if (lottery_table is null)
            return result;
        // fixed reward
        for (int i = 0; i < DeliveryRaidFixedRewardItem.Count; i++)
        {
            var item = fixed_table.GetReward(i);
            if (item is { Category: 0, ItemID: 0 })
                continue;
            var itemID = GetActualItemID(enc, teratype, item);
            result.Add((itemID, item.Num, item.SubjectType));
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

        /* lottery reward
        msg += $"{Environment.NewLine}{Environment.NewLine}Determined Lottery Rewards Table Num:{lottery_table.TableName}";*/
        var total = 0;
        for (int i = 0; i < DeliveryRaidLotteryRewardItem.RewardItemCount; i++)
        {
            var drop = lottery_table.GetRewardItem(i);
            if (drop is null or { ItemID: 0, Category: 0 })
                continue;
            var item = GetActualLotteryItemID(enc, teratype, drop);
            total += drop.Rate;
            var itemName = item switch
            {
                10000 => "Material",
                20000 => "Tera Shard",
                _ => Rewards.IsTM(item) ? Rewards.GetNameTM(item, Strings.Item, Strings.Move, Rewards.TMIndexes) : Strings.Item[item]
            };
            //msg += $"{($"{Environment.NewLine}Lottery Reward {i + 1}{Environment.NewLine}Item:{itemName}, DropRate:{drop.Rate}")}";
        }
        var rand = new Xoroshiro128Plus(seed);
        var count = (int)rand.NextInt(100);
        //msg += $"{Environment.NewLine}TotalItemRand:{total}{Environment.NewLine}{Environment.NewLine}ItemCountRandValue:{count}";
        count = Rewards.GetRewardCount(count, enc.Stars) + boost;
        //msg += $"{Environment.NewLine}Lottery Rewards Item Count:{count}";
        for (int i = 0; i < count; i++)
        {
            var roll = (int)rand.NextInt((ulong)total);
            //msg += $"{Environment.NewLine}ItemRandRoll:{roll}";
            for (int j = 0; j < DeliveryRaidLotteryRewardItem.RewardItemCount; j++)
            {
                var item = lottery_table.GetRewardItem(j);
                if (item is null or {ItemID : 0,  Category : 0 })
                    continue;

                if (roll < item.Rate)
                {
                    if (item.Category == 0)
                        result.Add((item.ItemID, item.Num, 0));
                    else if (item.Category == 1)
                        result.Add(item.ItemID == 0 ? (Rewards.GetMaterial(enc.Species), item.Num, 0) : (item.ItemID, item.Num, 0));
                    else result.Add(item.ItemID == 0 ? (Rewards.GetTeraShard(teratype), item.Num, 0) : (item.ItemID, item.Num, 0));
                    var ItemIndex = GetActualLotteryItemID(enc, teratype, item);
                    var itemName = ItemIndex switch
                    {
                        10000 => "Material",
                        20000 => "Tera Shard",
                        _ => Rewards.IsTM(ItemIndex) ? Rewards.GetNameTM(ItemIndex, Strings.Item, Strings.Move, Rewards.TMIndexes) : Strings.Item[ItemIndex]
                    };
                    //msg += $"{Environment.NewLine}Lottery Reward {i + 1}{Environment.NewLine}Item:{itemName}, Num:{item.Num}";
                    break;
                }
                roll -= item.Rate;
            }
        }
        //File.AppendAllText(dbgFile, msg);
        return Rewards.ReorderRewards(result);
    }
    private static int GetActualItemID(ISpeciesForm enc, int teraType, pkNX.Structures.FlatBuffers.Gen9.RaidFixedRewardItemInfo item)
    {
        if (item.ItemID != 0)
            return item.ItemID;
        if (item.Category == 2)
            return Rewards.GetTeraShard(teraType);
        return Rewards.GetMaterial(enc.Species);
    }
    private static int GetActualLotteryItemID(ISpeciesForm enc, int teraType, pkNX.Structures.FlatBuffers.Gen9.RaidLotteryRewardItemInfo item)
    {
        if (item.ItemID != 0)
            return item.ItemID;
        if (item.Category == 2)
            return Rewards.GetTeraShard(teraType);
        return Rewards.GetMaterial(enc.Species);
    }
}

public class TeraMight : ITeraRaid
{
    public readonly EncounterMight9 Entity; // Raw data
    public readonly ulong DropTableFix;
    public readonly ulong DropTableRandom;
    public readonly ushort[] ExtraMoves;
    public readonly sbyte DeliveryGroupID;

    public ushort Species => Entity.Species;
    public byte Form => Entity.Form;
    public byte Gender => Entity.Gender;
    public AbilityPermission Ability => Entity.Ability;
    public byte FlawlessIVCount => Entity.FlawlessIVCount;
    public Shiny Shiny => Entity.Shiny;
    public Nature Nature => Entity.Nature;
    public byte Level => Entity.Level;
    public IndividualValueSet IVs => Entity.IVs;
    public ushort Move1 => Entity.Moves.Move1;
    public ushort Move2 => Entity.Moves.Move2;
    public ushort Move3 => Entity.Moves.Move3;
    public ushort Move4 => Entity.Moves.Move4;
    public byte Stars => Entity.Stars;
    public byte RandRate => Entity.RandRate;
    ushort[] ITeraRaid.ExtraMoves => ExtraMoves;

    public static bool AvailableInGame(ITeraRaid9 enc, string game) => enc switch
    {
        EncounterDist9 encd => game switch
        {
            "Scarlet" => encd.RandRate0TotalScarlet + encd.RandRate1TotalScarlet + encd.RandRate2TotalScarlet + encd.RandRate3TotalScarlet != 0,
            "Violet" => encd.RandRate0TotalViolet + encd.RandRate1TotalViolet + encd.RandRate2TotalViolet + encd.RandRate3TotalViolet != 0,
            _ => false,
        },
        EncounterMight9 encm => game switch
        {
            "Scarlet" => encm.RandRate0TotalScarlet + encm.RandRate1TotalScarlet + encm.RandRate2TotalScarlet + encm.RandRate3TotalScarlet != 0,
            "Violet" => encm.RandRate0TotalViolet + encm.RandRate1TotalViolet + encm.RandRate2TotalViolet + encm.RandRate3TotalViolet != 0,
            _ => false,
        },
        _ => false,
    };
    public static bool AvailableInStage(ITeraRaid9 enc, string game, int stage) => enc switch
    {
        EncounterDist9 encd => game switch
        {
            "Scarlet" => encd.GetRandRateTotalScarlet(stage) > 0,
            "Violet" => encd.GetRandRateTotalViolet(stage) > 0,
            _ => false,
        },
        EncounterMight9 encm => game switch
        {
            "Scarlet" => encm.GetRandRateTotalScarlet(stage) > 0,
            "Violet" => encm.GetRandRateTotalViolet(stage) > 0,
            _ => false,
        },
        _ => false,
    };

    public TeraMight(
        EncounterMight9 enc,
        ulong fixedrewards,
        ulong lotteryrewards,
        ushort[] extras,
        sbyte group
    )
    {
        Entity = enc;
        DropTableFix = fixedrewards;
        DropTableRandom = lotteryrewards;
        ExtraMoves = extras
            .Where(z => z != 0 && !Entity.Moves.Contains(z))
            .Distinct()
            .ToArray();
        DeliveryGroupID = group;
        if (ExtraMoves.Length > 4)
            Debug.WriteLine(ExtraMoves);
    }

    public static TeraMight[] GetAllEncounters(ReadOnlyMemory<byte> encounters)
    {
        var all = FlatbufferDumper.DumpDistributionRaids(encounters);
        var type3 = EncounterMight9.GetArray(all[1]);
        var rewards3 = GetRewardTables(all[3]);
        var extra3 = all[5];
        var group3 = all[7];
        var result = new TeraMight[type3.Length];
        for (int i = 0; i < result.Length; i++)
        {
            var item1 = rewards3[i].Item1;
            var item2 = rewards3[i].Item2;
            var extra = GetExtraMoves(extra3[(12 * i)..]);
            result[i] = new TeraMight(type3[i], item1, item2, extra, (sbyte)group3[i]);
        }
        return result;
    }

    public static (ulong, ulong)[] GetRewardTables(ReadOnlySpan<byte> rewards)
    {
        var count = rewards.Length / 0x10;
        var result = new (ulong, ulong)[count];
        for (int i = 0; i < result.Length; i++)
        {
            var item1 = ReadUInt64LittleEndian(rewards[(0x10 * i)..]);
            var item2 = ReadUInt64LittleEndian(rewards[((0x10 * i) + 0x8)..]);
            result[i] = (item1, item2);
        }
        return result;
    }

    public static ushort[] GetExtraMoves(ReadOnlySpan<byte> extra)
    {
        var result = new ushort[6];
        for (int i = 0; i < result.Length; i++)
            result[i] = ReadUInt16LittleEndian(extra[(0x2 * i)..]);
        return result;
    }

    public static List<(int, int, int)> GetRewards(
        TeraMight enc,
        uint seed,
        int teratype,
        IReadOnlyList<DeliveryRaidFixedRewardItem>? fixed_rewards,
        IReadOnlyList<DeliveryRaidLotteryRewardItem>? lottery_rewards,
        int boost
    )
    {
        GameStrings Strings = GameInfo.GetStrings("en");
        /*var dbgFile = "Raid Rewards Might9.txt";
        var msg = $"{Environment.NewLine}";*/
        List<(int, int, int)> result = [];
        if (lottery_rewards is null)
            return result;

        if (fixed_rewards is null)
            return result;

        /*msg += $"{Environment.NewLine}Raid Species:{(Species)enc.Species}, DeleveryGroupID:{enc.DeliveryGroupID}";
        msg += $"{Environment.NewLine}Fix Rewards Table:{enc.DropTableFix}, Lottery Rewards Table:{enc.DropTableRandom}{Environment.NewLine}";*/
        var fixed_table = fixed_rewards.FirstOrDefault(z => z.TableName == enc.DropTableFix);
        if (fixed_table is null)
            return result;

        //msg += $"{Environment.NewLine}Determined Fix Rewards Table Num:{fixed_table.TableName}";
        var lottery_table = lottery_rewards
            .FirstOrDefault(z => z.TableName == enc.DropTableRandom);
        if (lottery_table is null)
            return result;

        // fixed reward
        for (int i = 0; i < DeliveryRaidFixedRewardItem.Count; i++)
        {
            var item = fixed_table.GetReward(i);
            if (item is null or { Category : 0,  ItemID : 0 })
                continue;
            var itemID = GetActualItemID(enc, teratype, item);
            result.Add((itemID, item.Num, item.SubjectType));
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
        for (int i = 0; i < DeliveryRaidLotteryRewardItem.RewardItemCount; i++)
        {
            var info = lottery_table.GetRewardItem(i);
            if (info is null or { ItemID : 0, Category : 0 })
                continue;
            total += info.Rate;
            var item = GetActualLotteryItemID(enc, teratype, info);
            var itemName = item switch
            {
                10000 => "Material",
                20000 => "Tera Shard",
                _ => Rewards.IsTM(item) ? Rewards.GetNameTM(item, Strings.Item, Strings.Move, Rewards.TMIndexes) : Strings.Item[item]
            };
            //msg += $"{($"{Environment.NewLine}Lottery Reward {i + 1}{Environment.NewLine}Item:{itemName}, DropRate:{info.Rate}")}";
        }

        var rand = new Xoroshiro128Plus(seed);
        var count = (int)rand.NextInt(100);
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
                if (item is null or { ItemID : 0, Category : 0 })
                    continue;

                if (roll < item.Rate)
                {
                    if (item.Category == 0)
                        result.Add((item.ItemID, item.Num, 0));
                    else if (item.Category == 1)
                        result.Add(item.ItemID == 0 ? (Rewards.GetMaterial(enc.Species), item.Num, 0) : (item.ItemID, item.Num, 0));
                    else
                        result.Add(item.ItemID == 0 ? (Rewards.GetTeraShard(teratype), item.Num, 0) : (item.ItemID, item.Num, 0));
                    var itemID = GetActualLotteryItemID(enc, teratype, item);
                    var itemName = itemID switch
                    {
                        10000 => "Material",
                        20000 => "Tera Shard",
                        _ => Rewards.IsTM(itemID) ? Rewards.GetNameTM(itemID, Strings.Item, Strings.Move, Rewards.TMIndexes) : Strings.Item[itemID]
                    };
                    //msg += $"{Environment.NewLine}Lottery Reward {i + 1}{Environment.NewLine}Item:{itemName}, Num:{item.Num}";
                    break;
                }
                roll -= item.Rate;
            }
        }
        //File.AppendAllText(dbgFile, msg);
        return Rewards.ReorderRewards(result);
    }
    private static int GetActualItemID(ISpeciesForm enc, int teraType, pkNX.Structures.FlatBuffers.Gen9.RaidFixedRewardItemInfo item)
    {
        if (item.ItemID != 0)
            return item.ItemID;
        if (item.Category == 2)
            return Rewards.GetTeraShard(teraType);
        return Rewards.GetMaterial(enc.Species);
    }
    private static int GetActualLotteryItemID(ISpeciesForm enc, int teraType, pkNX.Structures.FlatBuffers.Gen9.RaidLotteryRewardItemInfo item)
    {
        if (item.ItemID != 0)
            return item.ItemID;
        if (item.Category == 2)
            return Rewards.GetTeraShard(teraType);
        return Rewards.GetMaterial(enc.Species);
    }
}
