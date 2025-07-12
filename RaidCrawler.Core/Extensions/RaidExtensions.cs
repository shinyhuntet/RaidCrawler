using PKHeX.Core;
using pkNX.Structures.FlatBuffers.Gen9;

namespace RaidCrawler.Core.Structures;

public static class RaidExtensions
{
    public static ITeraRaid? GetTeraEncounter(this Raid raid, int progress, int id)
    {
        if (raid.IsEvent)
            return raid.GetDistributionEncounter(progress, raid.Flags == 3, id);

        return raid.MapParent switch
        {
            TeraRaidMapParent.Paldea => raid.GetEncounterBase(progress, raid.IsBlack),
            TeraRaidMapParent.Kitakami => raid.GetEncounterKitakami(progress, raid.IsBlack),
            TeraRaidMapParent.Blueberry => raid.GetEncounterBlueberry(progress, raid.IsBlack),
            _ => raid.GetEncounterBase(progress, raid.IsBlack),
        };
    }
    public static ITeraRaid? GetEncounterBase(this Raid raid, int stage, bool black)
    {
        //var dbgFile = "TeraRaids Paldea.txt";
        //var msg = $"{Environment.NewLine}";
        var clone = new Xoroshiro128Plus(raid.Seed);
        var starcount = black ? 6 : raid.GetStarCount((uint)clone.NextInt(100), stage, false);
        //msg += $"StarCount:{starcount}";
        var total = raid.Game == "Scarlet" ? GetRateTotalBaseSL(starcount) : GetRateTotalBaseVL(starcount);
        var speciesroll = clone.NextInt((ulong)total);
        //msg += $"{Environment.NewLine}Total rand:{total}, Species Rand value:{speciesroll}";
        if (raid.GemTeraRaidsBase is not null)
        {
            foreach (TeraEncounter enc in raid.GemTeraRaidsBase)
            {
                if (enc.Stars != starcount)
                    continue;

                var minimum = raid.Game == "Scarlet" ? enc.Entity.RandRateMinScarlet : enc.Entity.RandRateMinViolet;
                //msg += $"{Environment.NewLine}Species:{(Species)enc.Species}, RandMinValue:{minimum}";
                if (minimum >= 0 && (uint)((int)speciesroll - minimum) < enc.Entity.RandRate)
                {
                    //msg += $"{Environment.NewLine}Encounter Determined!{Environment.NewLine}Species:{(Species)enc.Species}{Environment.NewLine}";
                    //File.AppendAllText(dbgFile, msg);
                    return enc;
                }
            }
        }
        return null;
    }
    public static ITeraRaid? GetEncounterKitakami(this Raid raid, int stage, bool black)
    {
        //var dbgFile = "TeraRaids Kitakami.txt";
        //var msg = $"{Environment.NewLine}";
        var clone = new Xoroshiro128Plus(raid.Seed);
        var starcount = black ? 6 : raid.GetStarCount((uint)clone.NextInt(100), stage, false);
        //msg += $"StarCount:{starcount}";
        var total = raid.Game == "Scarlet" ? GetRateTotalKitakamiSL(starcount) : GetRateTotalKitakamiVL(starcount);
        var speciesroll = clone.NextInt((ulong)total);
        //msg += $"{Environment.NewLine}Total rand:{total}, Species Rand value:{speciesroll}";
        if (raid.GemTeraRaidsKitakami is not null)
        {
            foreach (TeraEncounter enc in raid.GemTeraRaidsKitakami)
            {
                if (enc.Stars != starcount)
                    continue;
                var minimum =
                    raid.Game == "Scarlet" ? enc.Entity.RandRateMinScarlet : enc.Entity.RandRateMinViolet;
                //msg += $"{Environment.NewLine}Species:{(Species)enc.Species}, RandMinValue:{minimum}";
                if (minimum >= 0 && (uint)((int)speciesroll - minimum) < enc.Entity.RandRate)
                {
                    //msg += $"{Environment.NewLine}Encounter Determined!{Environment.NewLine}Species:{(Species)enc.Species}{Environment.NewLine}";
                    //File.AppendAllText(dbgFile, msg);
                    return enc;
                }
            }
        }
        return null;
    }
    public static ITeraRaid? GetEncounterBlueberry(this Raid raid, int stage, bool black)
    {
        //var dbgFile = "TeraRaids Blueberry.txt";
        //var msg = $"{Environment.NewLine}";
        var clone = new Xoroshiro128Plus(raid.Seed);
        var starcount = black ? 6 : raid.GetStarCount((uint)clone.NextInt(100), stage, false);
        //msg += $"StarCount:{starcount}";
        var total = GetRateTotalBlueberry(starcount);
        var speciesroll = clone.NextInt((ulong)total);
        //msg += $"{Environment.NewLine}Total rand:{total}, Species Rand value:{speciesroll}";
        if (raid.GemTeraRaidsBlueberry is not null)
        {
            foreach (TeraEncounter enc in raid.GemTeraRaidsBlueberry)
            {
                if (enc.Stars != starcount)
                    continue;

                var minimum = raid.Game == "Scarlet" ? enc.Entity.RandRateMinScarlet : enc.Entity.RandRateMinViolet;
                //msg += $"{Environment.NewLine}Species:{(Species)enc.Species}, RandMinValue:{minimum}";
                if (minimum >= 0 && (uint)((int)speciesroll - minimum) < enc.Entity.RandRate)
                {
                    //msg += $"{Environment.NewLine}Encounter Determined!{Environment.NewLine}Species:{(Species)enc.Species}{Environment.NewLine}";
                    //File.AppendAllText(dbgFile, msg);
                    return enc;
                }
            }
        }
        //File.AppendAllText(dbgFile, msg);
        return null;
    }

    public static ITeraRaid? GetDistributionEncounter(this Raid raid, int stage, bool isFixed, int groupid)
    {
        if (stage < 0)
            return null;

        if (!isFixed)
        {
            if (raid.DistTeraRaids == null)
                return null;

            /*var dbgFile = "Dist9Raids.txt";
            var msg = $"{Environment.NewLine}Calculating...";*/
            foreach (TeraDistribution enc in raid.DistTeraRaids)
            {
                if (enc.DeliveryGroupID != groupid)
                    continue;

                var total = raid.Game == "Scarlet" ? enc.Entity.GetRandRateTotalScarlet(stage) : enc.Entity.GetRandRateTotalViolet(stage);
                //msg += $"{Environment.NewLine}Raid Species:{(Species)enc.Species},DeliveryGroupID:{enc.DeliveryGroupID}, Rate:{enc.RandRate}, Total RandCount:{total}";
                if (total > 0)
                {
                    var rand = new Xoroshiro128Plus(raid.Seed);
                    _ = rand.NextInt(100);
                    var val = rand.NextInt(total);
                    var min = raid.Game == "Scarlet" ? enc.Entity.GetRandRateMinScarlet(stage) : enc.Entity.GetRandRateMinViolet(stage);
                    //msg += $"{Environment.NewLine}Rand Value:{val}, RandRateMinValue:{min}";
                    if ((uint)((int)val - min) < enc.RandRate)
                    {
                        /*msg += $"{Environment.NewLine}Complete!{Environment.NewLine}";
                        File.AppendAllText(dbgFile, msg);*/
                        return enc;
                    }
                }
            }
        }
        else
        {
            if (raid.MightTeraRaids == null)
                return null;
            
            foreach (TeraMight enc in raid.MightTeraRaids)
            {
                /*var dbgFile = "Might9Raids.txt";
                var msg = string.Empty;*/
                if (enc.DeliveryGroupID != groupid)
                    continue;

                var total = raid.Game == "Scarlet" ? enc.Entity.GetRandRateTotalScarlet(stage) : enc.Entity.GetRandRateTotalViolet(stage);
                //msg += $"DeliveryGroupID:{enc.DeliveryGroupID}, RaidSpecies:{(Species)enc.Species}, RandRateTotal:{total}";
                if (total > 0)
                {
                    //File.AppendAllText(dbgFile, msg);
                    return enc;
                }

            }
        }
        return null;
    }

    public static (int delivery, int encounter) ReadAllRaids(this Raid container, byte[] data, int storyPrg, int eventPrg, int boost, TeraRaidMapParent type)
    {
        var dbgFile = $"raid_dbg-{type}.txt";
        if (File.Exists(dbgFile))
            File.Delete(dbgFile);
        
        var count = data.Length / Raid.SIZE;
        List<int> possible_groups = [];
        if (container.DistTeraRaids is not null) 
        {
            foreach (TeraDistribution e in container.DistTeraRaids)
            {
                if (TeraDistribution.AvailableInGame(e.Entity, container.Game) && TeraDistribution.AvailableInStage(e.Entity, container.Game, eventPrg) && !possible_groups.Contains(e.DeliveryGroupID))
                    possible_groups.Add(e.DeliveryGroupID);
            }
        }
        if (container.MightTeraRaids is not null)
        {
            foreach (TeraMight e in container.MightTeraRaids)
            {
                if (storyPrg == 4 && TeraMight.AvailableInGame(e.Entity, container.Game)&& TeraMight.AvailableInStage(e.Entity, container.Game, eventPrg) && !possible_groups.Contains(e.DeliveryGroupID))
                    possible_groups.Add(e.DeliveryGroupID);
            }
        }
        (int delivery, int encounter) failed = (0, 0);
        List<Raid> newRaids = new();
        List<ITeraRaid> newTera = new();
        List<List<(int, int, int)>> newRewards = new();
        int eventct = 0;
        int ct = 0;
        var groups = container.DeliveryRaidPriority.GroupID;
        for (int j = 0; j < groups.Table_Length; j++)
            ct += groups.Table(j);

        for (int i = 0; i < count; i++)
        {
            var raid = new Raid(container.Game, data.AsSpan(i * Raid.SIZE, Raid.SIZE), type)
            {
                GemTeraRaidsBase = container.GemTeraRaidsBase,
                GemTeraRaidsKitakami = container.GemTeraRaidsKitakami,
                GemTeraRaidsBlueberry = container.GemTeraRaidsBlueberry,
                DistTeraRaids = container.DistTeraRaids,
                MightTeraRaids = container.MightTeraRaids,
                DeliveryRaidPriority = container.DeliveryRaidPriority,
                DeliveryRaidFixedRewards = container.DeliveryRaidFixedRewards,
                DeliveryRaidLotteryRewards = container.DeliveryRaidLotteryRewards,
                BaseFixedRewards = container.BaseFixedRewards,
                BaseLotteryRewards = container.BaseLotteryRewards,
            };


            if (raid.Den == 0)
            {
                eventct++;
                continue;
            }
            if (!raid.IsValid)
                continue;

            var progress = raid.IsEvent ? eventPrg : storyPrg;
            var raid_delivery_group_id = -1;
            try
            {
                raid_delivery_group_id = raid.GetDeliveryGroupID(eventct, container.DeliveryRaidPriority, possible_groups);
            }
            catch(Exception ex)
            {
                var extra = $"Group ID: {raid_delivery_group_id}\nisFixed: {raid.Flags == 3}\nisBlack: {raid.IsBlack}\nisEvent: {raid.IsEvent}\n\n";
                var msg = $"{ex.Message}\nDen: {raid.Den}\nProgress: {progress}\nDifficulty: {raid.Difficulty}\n{extra}";
                File.AppendAllText(dbgFile, msg);
                failed.delivery++;
                continue;
            }

            var encounter = raid.GetTeraEncounter(progress, raid_delivery_group_id);
            if (encounter is null)
            {
                var extra = raid.IsEvent ? $"isFixed: {raid.Flags == 3}\nGroup ID: {raid_delivery_group_id}\n\n" : $"isBlack: {raid.IsBlack}\n\n";
                var msg = $"No encounters found for the given{(raid.IsEvent ? " distribution" : "")} raid.\nDen: {raid.Den}\nProgress: {progress}\n{extra}";
                File.AppendAllText(dbgFile, msg);
                failed.encounter++;
                continue;
            }

            if (raid.IsEvent) 
                eventct++;
            newRaids.Add(raid);
            newTera.Add(encounter);
            newRewards.Add(encounter.GetRewards(raid, boost));
            
        }

        container.Container.SetRaids(newRaids);
        container.Container.SetEncounters(newTera);
        container.Container.SetRewards(newRewards);
        return failed;
    }

    public static bool CheckIsShiny(this Raid raid, ITeraRaid? enc)
    {
        if (enc is null)
            return raid.IsShiny;

        if (enc.Shiny == Shiny.Never)
            return false;

        if (enc.Shiny.IsShiny())
            return true;
        return raid.IsShiny;
    }

    public static int GetTeraType(this Raid raid, ITeraRaid? encounter)
    {
        if (encounter is null)
            return raid.TeraType;

        if (encounter is TeraDistribution { Entity: ITeraRaid9 gem })
            return (int)gem.TeraType > 1 ? (int)gem.TeraType - 2 : raid.TeraType;

        if (encounter is TeraMight { Entity : ITeraRaid9 gemm })
            return (int)gemm.TeraType > 1 ? (int)gemm.TeraType - 2 : raid.TeraType;

        return raid.TeraType;
    }

    public static int GetStarCount(this Raid _, uint difficulty, int progress, bool isBlack)
    {
        if (isBlack)
            return 6;

        return GetStarCount(difficulty, progress);
    }
    private static int GetStarCount(uint difficulty, int progress) => progress switch
    {
        0 => difficulty switch
        {
            > 80 => 2,
            _ => 1,
        },
        1 => difficulty switch
        {
            > 70 => 3,
            > 30 => 2,
            _ => 1,
        },
        2 => difficulty switch
        {
            > 70 => 4,
            > 40 => 3,
            > 20 => 2,
            _ => 1,
        },
        3 => difficulty switch
        {
            > 75 => 5,
            > 40 => 4,
            _ => 3,
        },
        4 => difficulty switch
        {
            > 70 => 5,
            > 30 => 4,
            _ => 3,
        },
        _ => 1,
    };    
    public static int GetDeliveryGroupID(this Raid raid, int eventct, DeliveryGroupID ids, List<int> possible_groups)
    {
        if (!raid.IsEvent)
            return -1;
        /*var dbgFile = "Event Raid Group IDs.txt";
        var msg = string.Empty;
        var possible = string.Empty;
        for (int i = 0; i < possible_groups.Count; i++)
            possible += $" {possible_groups[i]}";
        msg = $"{Environment.NewLine}Raid index(Eventct):{eventct}, possible groups:{possible}";*/
        var groups = ids.GroupID;
        for (int j = 0; j < groups.Table_Length; j++)
        {
            var ct = groups.Table(j);
            //msg += $"{Environment.NewLine}group id index:{j}, Group range:{ct}, Current Eventct:{eventct}";
            if (!possible_groups.Contains(j + 1)) continue;
            if (eventct < ct)
            {
                /*msg += $"{Environment.NewLine}Group Id is determined! ID:{j + 1}{Environment.NewLine}";
                File.AppendAllText(dbgFile, msg);*/
                return j + 1;
            }
            eventct -= ct;
        }
        throw new Exception("Found event out of priority range.");
    }

    private static short GetRateTotalBaseSL(int star) => star switch
    {
        1 => 5800,
        2 => 5300,
        3 => 7400,
        4 => 8800, // Scarlet has one more encounter.
        5 => 9100,
        6 => 6500,
        _ => 0,
    };

    private static short GetRateTotalBaseVL(int star) => star switch
    {
        1 => 5800,
        2 => 5300,
        3 => 7400,
        4 => 8700, // Violet has one less encounter.
        5 => 9100,
        6 => 6500,
        _ => 0,
    };
    private static short GetRateTotalKitakamiSL(int star) => star switch
    {
        1 => 1500,
        2 => 1500,
        3 => 2500,
        4 => 2100,
        5 => 2250,
        6 => 2475, // Scarlet has one more
        _ => 0,
    };

    private static short GetRateTotalKitakamiVL(int star) => star switch
    {
        1 => 1500,
        2 => 1500,
        3 => 2500,
        4 => 2100,
        5 => 2250,
        6 => 2574, // Violet has one less
        _ => 0,
    };
    private static short GetRateTotalBlueberry(int star) => star switch
    {
        // Both games have the same number of encounters
        1 => 1100,
        2 => 1100,
        3 => 2000,
        4 => 1900,
        5 => 2100,
        6 => 2600,
        _ => 0,
    };
}
