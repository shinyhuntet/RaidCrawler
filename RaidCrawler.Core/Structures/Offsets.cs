using PKHeX.Core;

namespace RaidCrawler.Core.Structures;

public abstract class Offsets
{
    public const string ScarletID = "0100A3D008C5C000";
    public const string VioletID = "01008F6008C5E000";

    public IReadOnlyList<long> RaidBlockPointerBase => [0x4741FA0, 0x198, 0x88, 0x40]; // ver 4.0.0
    public IReadOnlyList<long> RaidBlockPointerKitakami => [0x4741FA0, 0x198, 0x88, 0xCD8]; // ver 4.0.0
    public IReadOnlyList<long> RaidBlockPointerBlueberry => [0x4741FA0, 0x198, 0x88, 0x1958]; // ver 4.0.0
    public IReadOnlyList<long> BlockKeyPointer => [0x47350D8, 0xD8, 0x0, 0x0, 0x30, 0x0]; // ver 4.0.0
    public IReadOnlyList<uint> DifficultyFlags => [0xEC95D8EF, 0xA9428DFE, 0x9535F471, 0x6E7F8220];
    public IReadOnlyList<long> OverworldPointer { get; } = new long[] { 0x473ADE0, 0x160, 0xE8, 0x28 }; // ver 4.0.0
    public IReadOnlyList<long> CollisionPointer { get; } = new long[] { 0x4734F78, 0x70, 0x48, 0x0, 0x08, 0x80 }; // ver 4.0.0
    public IReadOnlyList<long> PlayerOnMountPointer { get; } = new long[] { 0x4734F78, 0x70, 0x48, 0x0, 0x08, 0x70 }; // ver 4.0.0
    public IReadOnlyList<long> ItemBlock { get; } = new long[] { 0x47350D8, 0x1C0, 0xC8, 0x40 }; // ver 4.0.0
    public IReadOnlyList<long> MyStatusPointerSV { get; } = new long[] { 0x47350D8, 0x1C0, 0x0, 0x40 }; // ver 4.0.0

    public readonly static uint IsInBattle = 0x047B0830; // ver 4.0.0


    public static readonly DataBlock KBCATEventRaidIdentifier = new()
    {
        Name = "KBCATEventRaidIdentifier",
        Key = 0x37B99B4D,
        Type = SCTypeCode.Object,
        Pointer = new long[] { 0x4763C80, 0x08, 0x288, 0xE300 }, // ver 4.0.0
        IsEncrypted = false,
        Size = 0x04,
    };

    public static readonly DataBlock KBCATFixedRewardItemArray = new()
    {
        Name = "KBCATFixedRewardItemArray",
        Key = 0x7D6C2B82,
        Type = SCTypeCode.Object,
        Pointer = new long[] { 0x4763C80, 0x08, 0x288, 0xE340, 0x0 }, // ver 4.0.0
        IsEncrypted = false,
        Size = 0x6B40,
    };

    public static readonly DataBlock KBCATLotteryRewardItemArray = new()
    {
        Name = "KBCATLotteryRewardItemArray",
        Key = 0xA52B4811,
        Type = SCTypeCode.Object,
        Pointer = new long[] { 0x4763C80, 0x08, 0x288, 0xE378, 0x0 }, // ver 4.0.0
        IsEncrypted = false,
        Size = 0xD0D8,
    };

    public static readonly DataBlock KBCATRaidEnemyArray = new()
    {
        Name = "KBCATRaidEnemyArray",
        Key = 0x0520A1B0,
        Type = SCTypeCode.Object,
        Pointer = new long[] { 0x4763C80, 0x08, 0x288, 0xE308, 0x0 }, // ver 4.0.0
        IsEncrypted = false,
        Size = 0x7530,
    };

    public static readonly DataBlock KBCATRaidPriorityArray = new()
    {
        Name = "KBCATRaidPriorityArray",
        Key = 0x095451E4,
        Type = SCTypeCode.Object,
        Pointer = new long[] { 0x4763C80, 0x08, 0x288, 0xE3B0, 0x0 }, // ver 4.0.0
        IsEncrypted = false,
        Size = 0x58,
    };


    public static readonly DataBlock SevenStarRaid = new()
    {
        Name = "SevenStarRaid",
        Key = 0x8B14392F,
        Type = SCTypeCode.Object,
        Pointer = new long[] { 0x47350D8, 0x1C0, 0x88, 0x25E8 }, // ver 4.0.0
        IsEncrypted = false,
        Size = 0x5DC0,
    };

    public static readonly DataBlock KSevenStarRaidsDefeat = new()
    {
        Name = "KSevenStarRaidsDefeat",
        Key = 0xA4BA4848,
        Type = SCTypeCode.Object,
        Pointer = new List<long>() { 0x4763C80, 0x08, 0x288, 0x8538 }, // ver 4.0.0
        IsEncrypted = false,
        Size = 0x5DC4,
    };
    public const uint BCATRaidBinaryLocation = 0x520A1B0; // Thanks Lincoln-LM!
    public const uint BCATRaidPriorityLocation = 0x95451E4; // Thanks Lincoln-LM!
    public const uint BCATRaidFixedRewardLocation = 0x7D6C2B82;
    public const uint BCATRaidLotteryRewardLocation = 0xA52B4811;
}
public class DataBlock
{
    public string? Name { get; set; }
    public uint Key { get; set; }
    public SCTypeCode Type { get; set; }
    public SCTypeCode SubType { get; set; }
    public IReadOnlyList<long>? Pointer { get; set; }
    public bool IsEncrypted { get; set; }
    public int Size { get; set; }
}
