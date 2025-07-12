using PKHeX.Core;

namespace RaidCrawler.Core.Structures;

public static class EncounterExtensions
{
    public static List<(int, int, int)> GetRewards(this ITeraRaid encounter, Raid raid, int sandwich_boost)
    {
        return encounter switch
        {
            TeraMight => TeraMight.GetRewards((TeraMight)encounter, raid.Seed, raid.GetTeraType(encounter), raid.DeliveryRaidFixedRewards, raid.DeliveryRaidLotteryRewards, sandwich_boost),
            TeraDistribution => TeraDistribution.GetRewards((TeraDistribution)encounter, raid.Seed, raid.GetTeraType(encounter), raid.DeliveryRaidFixedRewards, raid.DeliveryRaidLotteryRewards, sandwich_boost),
            TeraEncounter => TeraEncounter.GetRewards((TeraEncounter)encounter, raid.Seed, raid.GetTeraType(encounter), raid.BaseFixedRewards, raid.BaseLotteryRewards, sandwich_boost),
            _ => throw new NotImplementedException($"Unknown encounter for rewards: {encounter.GetType()}"),
        }; ;
    }

    public static GenerateParam9 GetParam(this ITeraRaid encounter)
    {
        var gender = GetGender(encounter);
        if (encounter is TeraMight { Entity: { } em })
            return new GenerateParam9(em.Species, gender, em.FlawlessIVCount, 1, 0, 0, em.ScaleType, em.Scale, em.Ability, em.Shiny, em.Nature, em.IVs);
        else if (encounter is TeraDistribution { Entity : { } dm } dist)
            return new GenerateParam9(dist.Species, gender, dist.FlawlessIVCount, 1, 0, 0, dm.ScaleType, dm.Scale, dist.Ability, dist.Shiny, dist.Nature, dist.IVs);
        return new GenerateParam9(encounter.Species, gender, encounter.FlawlessIVCount, 1, 0, 0, SizeType9.RANDOM, 0, encounter.Ability, encounter.Shiny);
    }
    private static byte GetGender(ISpeciesForm enc) => enc switch
    {
        TeraMight { Entity.Gender: < 2 } tm => tm.Entity.Gender switch
        {
            0 => PersonalInfo.RatioMagicMale,
            1 => PersonalInfo.RatioMagicFemale,
            _ => PersonalInfo.RatioMagicGenderless,
        },
        TeraDistribution { Entity.Gender: < 2 } dm => dm.Entity.Gender switch
        {
            0 => PersonalInfo.RatioMagicMale,
            1 => PersonalInfo.RatioMagicFemale,
            _ => PersonalInfo.RatioMagicGenderless,
        },
        _ => PersonalTable.SV.GetFormEntry(enc.Species, enc.Form).Gender,
    };
}
