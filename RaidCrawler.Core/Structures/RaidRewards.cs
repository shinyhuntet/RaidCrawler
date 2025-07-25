using FlatSharp.Attributes;
using PKHeX.Core;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace RaidCrawler.Core.Structures;

public static class Rewards
{
    private static readonly int[][] RewardSlots =
    {
        [ 4, 5, 6, 7, 8 ],
        [ 4, 5, 6, 7, 8 ],
        [5, 6, 7, 8, 9],
        [5, 6, 7, 8, 9],
        [6, 7, 8, 9, 10],
        [7, 8, 9, 10, 11],
        [7, 8, 9, 10, 11],
    };

    public static ReadOnlySpan<int> RareRewards => [4, 645, 1606, 1904, 1905, 1906, 1907, 1908, 795];

    public static ReadOnlySpan<ushort> TMIndexes =>
    [
        005, 036, 204, 313, 097, 189, 184, 182, 424, 422,
        423, 352, 067, 491, 512, 522, 060, 109, 168, 574,
        885, 884, 886, 451, 083, 263, 342, 332, 523, 506,
        555, 232, 129, 345, 196, 341, 317, 577, 488, 490,
        314, 500, 101, 374, 525, 474, 419, 203, 521, 241,
        240, 201, 883, 684, 473, 091, 331, 206, 280, 428,
        369, 421, 492, 706, 339, 403, 034, 007, 009, 008,
        214, 402, 486, 409, 115, 113, 350, 127, 337, 605,
        118, 447, 086, 398, 707, 156, 157, 269, 014, 776,
        191, 390, 286, 430, 399, 141, 598, 019, 285, 442,
        349, 408, 441, 164, 334, 404, 529, 261, 242, 271,
        710, 202, 396, 366, 247, 406, 446, 304, 257, 412,
        094, 484, 227, 057, 861, 053, 085, 583, 133, 347,
        270, 676, 226, 414, 179, 058, 604, 580, 678, 581,
        417, 126, 056, 059, 519, 518, 520, 528, 188, 089,
        444, 566, 416, 307, 308, 338, 200, 315, 411, 437,
        542, 433, 405, 063, 413, 394, 087, 370, 076, 434,
        796, 851, 046, 268, 114, 092, 328, 180, 356, 479,
        360, 282, 450, 162, 410, 679, 667, 333, 503, 535,
        669, 253, 264, 311, 803, 807, 812, 814, 809, 808,
        799, 802, 220, 244, 038, 283, 572, 915, 250, 330,
        916, 527, 813, 811, 482, 815, 297, 248, 797, 806,
        800, 675, 784, 319, 174, 912, 913, 914, 917, 918,
    ];
    public static bool IsTM(int item) => item switch
    {
        >= 328 and <= 419 => true, // TM001 to TM092, skip TM000 Mega Punch
        618 or 619 or 620 => true, // TM093 to TM095
        690 or 691 or 692 or 693 => true, // TM096 to TM099
        >= 2160 and <= 2289 => true, // TM100 to TM229
        _ => false,
    };

    public static string GetNameTM(int item, IReadOnlyList<string> items, IReadOnlyList<string> moves, ReadOnlySpan<ushort> tm) => item switch
    {
        >= 328 and <= 419 => $"{items[item]} {moves[tm[001 + item - 328]]}", // TM001 to TM092, skip TM000 Mega Punch
        618 or 619 or 620 => $"{items[item]} {moves[tm[093 + item - 618]]}", // TM093 to TM095
        690 or 691 or 692 or 693 => $"{items[item]} {moves[tm[096 + item - 690]]}", // TM096 to TM099
        _ => $"{items[item]} {moves[tm[100 + item - 2160]]}", // TM100 to TM171
    };

    public static int GetRewardCount(int random, int stars) => random switch
    {
        < 10 => RewardSlots[stars - 1][0],
        < 40 => RewardSlots[stars - 1][1],
        < 70 => RewardSlots[stars - 1][2],
        < 90 => RewardSlots[stars - 1][3],
        _ => RewardSlots[stars - 1][4],
    };


    public static List<(int, int, int)> ReorderRewards(List<(int, int, int)> rewards)
    {
        var rares = new List<(int, int, int)>();
        var commons = new List<(int, int, int)>();
        for (int i = 0; i < rewards.Count; i++)
        {
            if (RareRewards.Contains(rewards[i].Item1))
                rares.Add(rewards[i]);
            else commons.Add(rewards[i]);
        }
        rares.AddRange(commons);
        return rares;
    }

    public static int GetTeraShard(int type) => (MoveType)type switch
    {
        MoveType.Normal => 1862,
        MoveType.Fighting => 1868,
        MoveType.Flying => 1871,
        MoveType.Poison => 1869,
        MoveType.Ground => 1870,
        MoveType.Rock => 1874,
        MoveType.Bug => 1873,
        MoveType.Ghost => 1875,
        MoveType.Steel => 1878,
        MoveType.Fire => 1863,
        MoveType.Water => 1864,
        MoveType.Grass => 1866,
        MoveType.Electric => 1865,
        MoveType.Psychic => 1872,
        MoveType.Ice => 1867,
        MoveType.Dragon => 1876,
        MoveType.Dark => 1877,
        MoveType.Fairy => 1879,
        _ => 20000,
    };


    public static int GetMaterial(int species) => (Species)species switch
    {
        Species.Venonat or Species.Venomoth => 1956,
        Species.Diglett or Species.Dugtrio => 1957,
        Species.Meowth or Species.Persian => 1958,
        Species.Psyduck or Species.Golduck => 1959,
        Species.Mankey or Species.Primeape or Species.Annihilape => 1960,
        Species.Growlithe or Species.Arcanine => 1961,
        Species.Slowpoke or Species.Slowbro or Species.Slowking => 1962,
        Species.Magnemite or Species.Magneton or Species.Magnezone => 1963,
        Species.Grimer or Species.Muk => 1964,
        Species.Shellder or Species.Cloyster => 1965,
        Species.Gastly or Species.Haunter or Species.Gengar => 1966,
        Species.Drowzee or Species.Hypno => 1967,
        Species.Voltorb or Species.Electrode => 1968,
        Species.Scyther or Species.Scizor or Species.Kleavor => 1969,
        Species.Tauros => 1970,
        Species.Magikarp or Species.Gyarados => 1971,
        Species.Ditto => 1972,
        Species.Eevee or Species.Vaporeon or Species.Jolteon
        or Species.Flareon or Species.Espeon or Species.Umbreon
        or Species.Leafeon or Species.Glaceon or Species.Sylveon => 1973,
        Species.Dratini or Species.Dragonair or Species.Dragonite => 1974,
        Species.Pichu or Species.Pikachu or Species.Raichu => 1975,
        Species.Igglybuff or Species.Jigglypuff or Species.Wigglytuff => 1976,
        Species.Mareep or Species.Flaaffy or Species.Ampharos => 1977,
        Species.Hoppip or Species.Skiploom or Species.Jumpluff => 1978,
        Species.Sunkern or Species.Sunflora => 1979,
        Species.Murkrow or Species.Honchkrow => 1980,
        Species.Misdreavus or Species.Mismagius => 1981,
        Species.Girafarig or Species.Farigiraf => 1982,
        Species.Pineco or Species.Forretress => 1983,
        Species.Dunsparce or Species.Dudunsparce => 1984,
        Species.Qwilfish or Species.Overqwil => 1985,
        Species.Heracross => 1986,
        Species.Sneasel or Species.Weavile or Species.Sneasler => 1987,
        Species.Teddiursa or Species.Ursaring or Species.Ursaluna => 1988,
        Species.Delibird => 1989,
        Species.Houndour or Species.Houndoom => 1990,
        Species.Phanpy or Species.Donphan => 1991,
        Species.Stantler or Species.Wyrdeer => 1992,
        Species.Larvitar or Species.Pupitar or Species.Tyranitar => 1993,
        Species.Wingull or Species.Pelipper => 1994,
        Species.Ralts or Species.Kirlia or Species.Gardevoir or Species.Gallade => 1995,
        Species.Surskit or Species.Masquerain => 1996,
        Species.Shroomish or Species.Breloom => 1997,
        Species.Slakoth or Species.Vigoroth or Species.Slaking => 1998,
        Species.Makuhita or Species.Hariyama => 1999,
        Species.Azurill or Species.Marill or Species.Azumarill => 2000,
        Species.Sableye => 2001,
        Species.Meditite or Species.Medicham => 2002,
        Species.Gulpin or Species.Swalot => 2003,
        Species.Numel or Species.Camerupt => 2004,
        Species.Torkoal => 2005,
        Species.Spoink or Species.Grumpig => 2006,
        Species.Cacnea or Species.Cacturne => 2007,
        Species.Swablu or Species.Altaria => 2008,
        Species.Zangoose => 2009,
        Species.Seviper => 2010,
        Species.Barboach or Species.Whiscash => 2011,
        Species.Shuppet or Species.Banette => 2012,
        Species.Tropius => 2013,
        Species.Snorunt or Species.Glalie or Species.Froslass => 2014,
        Species.Luvdisc => 2015,
        Species.Bagon or Species.Shelgon or Species.Salamence => 2016,
        Species.Starly or Species.Staravia or Species.Staraptor => 2017,
        Species.Kricketot or Species.Kricketune => 2018,
        Species.Shinx or Species.Luxio or Species.Luxray => 2019,
        Species.Combee or Species.Vespiquen => 2020,
        Species.Pachirisu => 2021,
        Species.Buizel or Species.Floatzel => 2022,
        Species.Shellos or Species.Gastrodon => 2023,
        Species.Drifloon or Species.Drifblim => 2024,
        Species.Stunky or Species.Skuntank => 2025,
        Species.Bronzor or Species.Bronzong => 2026,
        Species.Bonsly or Species.Sudowoodo => 2027,
        Species.Happiny or Species.Chansey or Species.Blissey => 2028,
        Species.Spiritomb => 2029,
        Species.Gible or Species.Gabite or Species.Garchomp => 2030,
        Species.Riolu or Species.Lucario => 2031,
        Species.Hippopotas or Species.Hippowdon => 2032,
        Species.Croagunk or Species.Toxicroak => 2033,
        Species.Finneon or Species.Lumineon => 2034,
        Species.Snover or Species.Abomasnow => 2035,
        Species.Rotom => 2036,
        Species.Petilil or Species.Lilligant => 2037,
        Species.Basculin or Species.Basculegion => 2038,
        Species.Sandile or Species.Krokorok or Species.Krookodile => 2039,
        Species.Zorua or Species.Zoroark => 2040,
        Species.Gothita or Species.Gothorita or Species.Gothitelle => 2041,
        Species.Deerling or Species.Sawsbuck => 2042,
        Species.Foongus or Species.Amoonguss => 2043,
        Species.Alomomola => 2044,
        Species.Tynamo or Species.Eelektrik or Species.Eelektross => 2045,
        Species.Axew or Species.Fraxure or Species.Haxorus => 2046,
        Species.Cubchoo or Species.Beartic => 2047,
        Species.Cryogonal => 2048,
        Species.Pawniard or Species.Bisharp or Species.Kingambit => 2049,
        Species.Rufflet or Species.Braviary => 2050,
        Species.Deino or Species.Zweilous or Species.Hydreigon => 2051,
        Species.Larvesta or Species.Volcarona => 2052,
        Species.Fletchling or Species.Fletchinder or Species.Talonflame => 2053,
        Species.Scatterbug or Species.Spewpa or Species.Vivillon => 2054,
        Species.Litleo or Species.Pyroar => 2055,
        Species.Flabébé or Species.Floette or Species.Florges => 2056,
        Species.Skiddo or Species.Gogoat => 2057,
        Species.Skrelp or Species.Dragalge => 2058,
        Species.Clauncher or Species.Clawitzer => 2059,
        Species.Hawlucha => 2060,
        Species.Dedenne => 2061,
        Species.Goomy or Species.Sliggoo or Species.Goodra => 2062,
        Species.Klefki => 2063,
        Species.Bergmite or Species.Avalugg => 2064,
        Species.Noibat or Species.Noivern => 2065,
        Species.Yungoos or Species.Gumshoos => 2066,
        Species.Crabrawler or Species.Crabominable => 2067,
        Species.Oricorio => 2068,
        Species.Rockruff or Species.Lycanroc => 2069,
        Species.Mareanie or Species.Toxapex => 2070,
        Species.Mudbray or Species.Mudsdale => 2071,
        Species.Fomantis or Species.Lurantis => 2072,
        Species.Salandit or Species.Salazzle => 2073,
        Species.Bounsweet or Species.Steenee or Species.Tsareena => 2074,
        Species.Oranguru => 2075,
        Species.Passimian => 2076,
        Species.Sandygast or Species.Palossand => 2077,
        Species.Komala => 2078,
        Species.Mimikyu => 2079,
        Species.Bruxish => 2080,
        Species.Chewtle or Species.Drednaw => 2081,
        Species.Skwovet or Species.Greedent => 2082,
        Species.Arrokuda or Species.Barraskewda => 2083,
        Species.Rookidee or Species.Corvisquire or Species.Corviknight => 2084,
        Species.Toxel or Species.Toxtricity => 2085,
        Species.Falinks => 2086,
        Species.Cufant or Species.Copperajah => 2087,
        Species.Rolycoly or Species.Carkol or Species.Coalossal => 2088,
        Species.Silicobra or Species.Sandaconda => 2089,
        Species.Indeedee => 2090,
        Species.Pincurchin => 2091,
        Species.Snom or Species.Frosmoth => 2092,
        Species.Impidimp or Species.Morgrem or Species.Grimmsnarl => 2093,
        Species.Applin or Species.Flapple or Species.Appletun => 2094,
        Species.Sinistea or Species.Polteageist => 2095,
        Species.Hatenna or Species.Hattrem or Species.Hatterene => 2096,
        Species.Stonjourner => 2097,
        Species.Eiscue => 2098,
        Species.Dreepy or Species.Drakloak or Species.Dragapult => 2099,

        Species.Lechonk or Species.Oinkologne => 2103,
        Species.Tarountula or Species.Spidops => 2104,
        Species.Nymble or Species.Lokix => 2105,
        Species.Rellor or Species.Rabsca => 2106,
        Species.Greavard or Species.Houndstone => 2107,
        Species.Flittle or Species.Espathra => 2108,
        Species.Wiglett or Species.Wugtrio => 2109,
        Species.Dondozo => 2110,
        Species.Veluza => 2111,
        Species.Finizen or Species.Palafin => 2112,
        Species.Smoliv or Species.Dolliv or Species.Arboliva => 2113,
        Species.Capsakid or Species.Scovillain => 2114,
        Species.Tadbulb or Species.Bellibolt => 2115,
        Species.Varoom or Species.Revavroom => 2116,
        Species.Orthworm => 2117,
        Species.Tandemaus or Species.Maushold => 2118,
        Species.Cetoddle or Species.Cetitan => 2119,
        Species.Frigibax or Species.Arctibax or Species.Baxcalibur => 2120,
        Species.Tatsugiri => 2121,
        Species.Cyclizar => 2122,
        Species.Pawmi or Species.Pawmo or Species.Pawmot => 2123,

        Species.Wattrel or Species.Kilowattrel => 2126,
        Species.Bombirdier => 2127,
        Species.Squawkabilly => 2128,
        Species.Flamigo => 2129,
        Species.Klawf => 2130,
        Species.Nacli or Species.Naclstack or Species.Garganacl => 2131,
        Species.Glimmet or Species.Glimmora => 2132,
        Species.Shroodle or Species.Grafaiai => 2133,
        Species.Fidough or Species.Dachsbun => 2134,
        Species.Maschiff or Species.Mabosstiff => 2135,
        Species.Bramblin or Species.Brambleghast => 2136,
        Species.Gimmighoul or Species.Gholdengo => 2137,

        Species.Tinkatink or Species.Tinkatuff or Species.Tinkaton => 2156,
        Species.Charcadet or Species.Armarouge or Species.Ceruledge => 2157,
        Species.Toedscool or Species.Toedscruel => 2158,
        Species.Wooper or Species.Quagsire or Species.Clodsire => 2159,

        Species.Ekans or Species.Arbok => 2438,
        Species.Sandshrew or Species.Sandslash => 2439,
        Species.Cleffa or Species.Clefairy or Species.Clefable => 2440,
        Species.Vulpix or Species.Ninetales => 2441,
        Species.Poliwag or Species.Poliwhirl or Species.Poliwrath or Species.Politoed => 2442,
        Species.Bellsprout or Species.Weepinbell or Species.Victreebel => 2443,
        Species.Geodude or Species.Graveler or Species.Golem => 2444,
        Species.Koffing or Species.Weezing => 2445,
        Species.Munchlax or Species.Snorlax => 2446,
        Species.Sentret or Species.Furret => 2447,
        Species.Hoothoot or Species.Noctowl => 2448,
        Species.Spinarak or Species.Ariados => 2449,
        Species.Aipom or Species.Ambipom => 2450,
        Species.Yanma or Species.Yanmega => 2451,
        Species.Gligar or Species.Gliscor => 2452,
        Species.Slugma or Species.Magcargo => 2453,
        Species.Swinub or Species.Piloswine or Species.Mamoswine => 2454,
        Species.Poochyena or Species.Mightyena => 2455,
        Species.Lotad or Species.Lombre or Species.Ludicolo => 2456,
        Species.Seedot or Species.Nuzleaf or Species.Shiftry => 2457,
        Species.Nosepass or Species.Probopass => 2458,
        Species.Volbeat => 2459,
        Species.Illumise => 2460,
        Species.Corphish or Species.Crawdaunt => 2461,
        Species.Feebas or Species.Milotic => 2462,
        Species.Duskull or Species.Dusclops or Species.Dusknoir => 2463,
        Species.Chingling or Species.Chimecho => 2464,
        Species.Timburr or Species.Gurdurr or Species.Conkeldurr => 2465,
        Species.Sewaddle or Species.Swadloon or Species.Leavanny => 2466,
        Species.Ducklett or Species.Swanna => 2467,
        Species.Litwick or Species.Lampent or Species.Chandelure => 2468,
        Species.Mienfoo or Species.Mienshao => 2469,
        Species.Vullaby or Species.Mandibuzz => 2470,
        Species.Carbink => 2471,
        Species.Phantump or Species.Trevenant => 2472,
        Species.Grubbin or Species.Charjabug or Species.Vikavolt => 2473,
        Species.Cutiefly or Species.Ribombee => 2474,
        Species.Jangmoo or Species.Hakamoo or Species.Kommoo => 2475,
        Species.Cramorant => 2476,
        Species.Morpeko => 2477,
        Species.Poltchageist or Species.Sinistcha => 2478,

        Species.Oddish or Species.Gloom or Species.Vileplume or Species.Bellossom => 2484,
        Species.Tentacool or Species.Tentacruel => 2485,
        Species.Doduo or Species.Dodrio => 2486,
        Species.Seel or Species.Dewgong => 2487,
        Species.Exeggcute or Species.Exeggutor => 2488,
        Species.Tyrogue or Species.Hitmonlee or Species.Hitmonchan or Species.Hitmontop => 2489,
        Species.Rhyhorn or Species.Rhydon or Species.Rhyperior => 2490,
        Species.Horsea or Species.Seadra or Species.Kingdra => 2491,
        Species.Elekid or Species.Electabuzz or Species.Electivire => 2492,
        Species.Magby or Species.Magmar or Species.Magmortar => 2493,
        Species.Lapras => 2494,
        Species.Porygon or Species.Porygon2 or Species.PorygonZ => 2495,
        Species.Chinchou or Species.Lanturn => 2496,
        Species.Snubbull or Species.Granbull => 2497,
        Species.Skarmory => 2498,
        Species.Smeargle => 2499,
        Species.Plusle => 2500,
        Species.Minun => 2501,
        Species.Trapinch or Species.Vibrava or Species.Flygon => 2502,
        Species.Beldum or Species.Metang or Species.Metagross => 2503,
        Species.Cranidos or Species.Rampardos => 2504,
        Species.Shieldon or Species.Bastiodon => 2505,
        Species.Blitzle or Species.Zebstrika => 2506,
        Species.Drilbur or Species.Excadrill => 2507,
        Species.Cottonee or Species.Whimsicott => 2508,
        Species.Scraggy or Species.Scrafty => 2509,
        Species.Minccino or Species.Cinccino => 2510,
        Species.Solosis or Species.Duosion or Species.Reuniclus => 2511,
        Species.Joltik or Species.Galvantula => 2512,
        Species.Golett or Species.Golurk => 2513,
        Species.Espurr or Species.Meowstic => 2514,
        Species.Inkay or Species.Malamar => 2515,
        Species.Pikipek or Species.Trumbeak or Species.Toucannon => 2516,
        Species.Dewpider or Species.Araquanid => 2517,
        Species.Comfey => 2518,
        Species.Minior => 2519,
        Species.Milcery => 2520,
        Species.Duraludon or Species.Archaludon => 2521,

        Species.GreatTusk or Species.ScreamTail or Species.BruteBonnet or Species.FlutterMane or Species.SlitherWing
        or Species.RoaringMoon or Species.WalkingWake or Species.GougingFire or Species.RagingBolt => 0,

        Species.IronTreads or Species.IronBundle or Species.IronHands or Species.IronJugulis or Species.IronMoth
        or Species.IronThorns or Species.IronValiant or Species.IronLeaves or Species.IronBoulder or Species.IronCrown => 0,

        _ => 10000,
    };
}


public class RaidFixedRewards
{
    public ulong TableName { get; set; }
    public RaidFixedRewardItemInfo? RewardItem00 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem01 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem02 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem03 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem04 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem05 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem06 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem07 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem08 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem09 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem10 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem11 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem12 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem13 { get; set; }
    public RaidFixedRewardItemInfo? RewardItem14 { get; set; }

    public const int Count = 15;

    public RaidFixedRewardItemInfo? GetReward(int index) => index switch
    {
        00 => RewardItem00,
        01 => RewardItem01,
        02 => RewardItem02,
        03 => RewardItem03,
        04 => RewardItem04,
        05 => RewardItem05,
        06 => RewardItem06,
        07 => RewardItem07,
        08 => RewardItem08,
        09 => RewardItem09,
        10 => RewardItem10,
        11 => RewardItem11,
        12 => RewardItem12,
        13 => RewardItem13,
        14 => RewardItem14,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };
}

public class RaidLotteryRewards
{
    public ulong TableName { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem00 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem01 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem02 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem03 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem04 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem05 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem06 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem07 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem08 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem09 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem10 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem11 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem12 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem13 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem14 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem15 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem16 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem17 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem18 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem19 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem20 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem21 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem22 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem23 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem24 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem25 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem26 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem27 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem28 { get; set; }
    public RaidLotteryRewardItemInfo? RewardItem29 { get; set; }

    public const int RewardItemCount = 30;

    // Get reward item from index
    public RaidLotteryRewardItemInfo? GetRewardItem(int index) => index switch
    {
        00 => RewardItem00,
        01 => RewardItem01,
        02 => RewardItem02,
        03 => RewardItem03,
        04 => RewardItem04,
        05 => RewardItem05,
        06 => RewardItem06,
        07 => RewardItem07,
        08 => RewardItem08,
        09 => RewardItem09,
        10 => RewardItem10,
        11 => RewardItem11,
        12 => RewardItem12,
        13 => RewardItem13,
        14 => RewardItem14,
        15 => RewardItem15,
        16 => RewardItem16,
        17 => RewardItem17,
        18 => RewardItem18,
        19 => RewardItem19,
        20 => RewardItem20,
        21 => RewardItem21,
        22 => RewardItem22,
        23 => RewardItem23,
        24 => RewardItem24,
        25 => RewardItem25,
        26 => RewardItem26,
        27 => RewardItem27,
        28 => RewardItem28,
        29 => RewardItem29,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
    };
}

[JsonSerializable(typeof(RaidLotteryRewardItemInfo))]
[FlatBufferTable, TypeConverter(typeof(ExpandableObjectConverter))]
public class RaidLotteryRewardItemInfo
{
    [FlatBufferItem(0)] public int Category { get; set; }
    [FlatBufferItem(1)] public int ItemID { get; set; }
    [FlatBufferItem(2)] public sbyte Num { get; set; }
    [FlatBufferItem(3)] public int Rate { get; set; }
    [FlatBufferItem(4)] public bool RareItemFlag { get; set; }
}

[JsonSerializable(typeof(RaidFixedRewardItemInfo))]
[FlatBufferTable, TypeConverter(typeof(ExpandableObjectConverter))]
public class RaidFixedRewardItemInfo
{
    [FlatBufferItem(0)] public int Category { get; set; }
    [FlatBufferItem(1)] public int SubjectType { get; set; }
    [FlatBufferItem(2)] public int ItemID { get; set; }
    [FlatBufferItem(3)] public sbyte Num { get; set; }
}
