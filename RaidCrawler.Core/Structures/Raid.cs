using PKHeX.Core;
using pkNX.Structures.FlatBuffers.Gen9;
using System.Text.Json;
using static System.Buffers.Binary.BinaryPrimitives;

namespace RaidCrawler.Core.Structures;

public class Raid
// See also https://github.com/kwsch/PKHeX/blob/master/PKHeX.Core/Saves/Substructures/Gen9/RaidSpawnList9.cs
{
    public const byte SIZE = 0x20;

    public string Game { get; private set; } = "Scarlet";
    public GameStrings Strings { get; private set; }
    
    public TeraEncounter[]? GemTeraRaidsBase;
    public TeraEncounter[]? GemTeraRaidsKitakami;
    public TeraEncounter[]? GemTeraRaidsBlueberry;
    public TeraDistribution[]? DistTeraRaids;
    public TeraMight[]? MightTeraRaids;
    public IReadOnlyList<RaidFixedRewards>? BaseFixedRewards;
    public IReadOnlyList<RaidLotteryRewards>? BaseLotteryRewards;
    public IReadOnlyList<DeliveryRaidFixedRewardItem>? DeliveryRaidFixedRewards;
    public IReadOnlyList<DeliveryRaidLotteryRewardItem>? DeliveryRaidLotteryRewards;
    public DeliveryGroupID DeliveryRaidPriority = new() { GroupID = new() };
    private readonly byte[] Data; // Raw data
    public readonly RaidContainer Container;
    public TeraRaidMapParent MapParent;
    private readonly string[] Raid_data_base =
    [
        "raid_enemy_01_array.bin",
        "raid_enemy_02_array.bin",
        "raid_enemy_03_array.bin",
        "raid_enemy_04_array.bin",
        "raid_enemy_05_array.bin",
        "raid_enemy_06_array.bin",
    ];
    private readonly string[] Raid_data_su1 =
    [
        "su1_raid_enemy_01_array.bin",
        "su1_raid_enemy_02_array.bin",
        "su1_raid_enemy_03_array.bin",
        "su1_raid_enemy_04_array.bin",
        "su1_raid_enemy_05_array.bin",
        "su1_raid_enemy_06_array.bin",
    ];
    private readonly string[] RaidDataBlueberry =
    [
        "su2_raid_enemy_01_array.bin",
        "su2_raid_enemy_02_array.bin",
        "su2_raid_enemy_03_array.bin",
        "su2_raid_enemy_04_array.bin",
        "su2_raid_enemy_05_array.bin",
        "su2_raid_enemy_06_array.bin",
    ];

    public Raid(string game)
    {
        Game = game;
        Container = new(Game);
        Data = Array.Empty<byte>();
        Strings = GameInfo.GetStrings("en");
        GemTeraRaidsBase = TeraEncounter.GetAllEncounters(Raid_data_base, TeraRaidMapParent.Paldea);
        GemTeraRaidsKitakami = TeraEncounter.GetAllEncounters(Raid_data_su1, TeraRaidMapParent.Kitakami);
        GemTeraRaidsBlueberry = TeraEncounter.GetAllEncounters(RaidDataBlueberry, TeraRaidMapParent.Blueberry);
        BaseFixedRewards = JsonSerializer.Deserialize<IReadOnlyList<RaidFixedRewards>>(Utils.GetStringResource("raid_fixed_reward_item_array.json") ?? "[]");
        BaseLotteryRewards = JsonSerializer.Deserialize<IReadOnlyList<RaidLotteryRewards>>(Utils.GetStringResource("raid_lottery_reward_item_array.json") ?? "[]");
    }

    public Raid(string game, Span<byte> data, TeraRaidMapParent map = TeraRaidMapParent.Paldea)
    {
        Game = game;
        Container = new(Game);
        Data = data.ToArray();
        Strings = GameInfo.GetStrings("en");
        MapParent = map;
    }


    public bool IsValid => Validate();
    public bool IsActive => ReadUInt32LittleEndian(Data.AsSpan(0x00)) == 1;
    public uint Area => ReadUInt32LittleEndian(Data.AsSpan(0x04));
    public uint LotteryGroup => ReadUInt32LittleEndian(Data.AsSpan(0x08));
    public uint Den => ReadUInt32LittleEndian(Data.AsSpan(0x0C));
    public uint Seed => ReadUInt32LittleEndian(Data.AsSpan(0x10));
    public uint Flags => ReadUInt32LittleEndian(Data.AsSpan(0x18));
    public bool IsBlack => Flags == 1;
    public bool IsEvent => Flags >= 2;

    public int TeraType => GetTeraType(Seed);
    public uint Difficulty => GetDifficulty(Seed);

    public uint EC => GenericRaidData[0];
    public uint PID => GenericRaidData[2];
    public bool IsShiny => GenericRaidData[3] == 1;

    uint[] GenericRaidData => GenerateGenericRaidData(Seed);

    // Methods
    private bool Validate()
    {
        if (Seed == 0 || !IsActive)
            return false;
        if (!IsValidMap())
            return false;

        GenerateGenericRaidData(Seed);
        return true;
    }
    private bool IsValidMap()
    {
        if (MapParent == TeraRaidMapParent.Paldea)
            return Area <= 22;
        if (MapParent == TeraRaidMapParent.Kitakami)
            return Area <= 11;
        if (MapParent == TeraRaidMapParent.Blueberry)
            return Area <= 8;
        return false;
    }

    public byte[] GetData() => Data;

    public void SetGame(string game)
    {
        Game = game;
    }

    private static int GetTeraType(uint Seed)
    {
        var rng = new Xoroshiro128Plus(Seed);
        return (int)rng.NextInt(18);
    }

    private static uint[] GenerateGenericRaidData(uint Seed)
    {
        var rng = new Xoroshiro128Plus(Seed);
        uint EC = (uint)rng.NextInt();
        uint TIDSID = (uint)rng.NextInt();
        uint PID = (uint)rng.NextInt();
        bool IsShiny = (((PID >> 16) ^ (PID & 0xFFFF)) >> 4) == ((TIDSID >> 16) ^ (TIDSID & 0xFFFF)) >> 4;
        var Shiny = IsShiny ? 1u : 0;
        return [ EC, TIDSID, PID, Shiny ];
    }

    private static uint GetDifficulty(uint Seed)
    {
        var rng = new Xoroshiro128Plus(Seed);
        return (uint)rng.NextInt(100);
    }
    public void GenerateDataPK9(PK9 pk, GenerateParam9 param, Shiny isShiny, uint seed)
    {
        var criteria = new EncounterCriteria { Shiny = isShiny };
        bool check = Encounter9RNG.GenerateData(pk, param, criteria, seed);
        if (!check)
        {
            criteria = new EncounterCriteria { Shiny = pk.IsShiny ? Shiny.Always : Shiny.Random };
            Encounter9RNG.GenerateData(pk, param, criteria, seed);
        }
    }
}
