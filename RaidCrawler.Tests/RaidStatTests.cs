using FluentAssertions;
using Xunit;
using System.ComponentModel;
using PKHeX.Core;
using RaidCrawler.Core.Structures;

namespace RaidCrawler.Tests;

public class RaidStatTests : TestUtil
{
    private const string SennaDitto = "RaidCrawler.Tests.Blocks.senna_9_132_Modest_31_0_31_31_31_31_SL"; // Progress: 4 story
    private const string NewtShinyBounsweet = "RaidCrawler.Tests.Blocks.newt_56_761_Calm_4_7_4_3_31_25_VL"; // Progress: 0 story
    private const string HexManiacLisaHippopotas = "RaidCrawler.Tests.Blocks.lisa_30_449_Gentle_21_31_25_21_8_31_SL"; // Progress: 4 story

    [Theory]
    [InlineData(SennaDitto, 4, 9, Species.Ditto, new int[] { 31, 0, 31, 31, 31, 31 }, Nature.Modest, false)]
    [InlineData(NewtShinyBounsweet, 0, 56, Species.Bounsweet, new int[] { 4, 7, 4, 3, 31, 25 }, Nature.Calm, true)]
    [InlineData(HexManiacLisaHippopotas, 4, 30, Species.Hippopotas, new int[] { 21, 31, 25, 21, 8, 31 }, Nature.Gentle, false)]
    [Description("Test known stats for a given raid encounter to make sure they match expected values.")]
    public void StatsCorrect(string path, int storyPrg, int denIndex, Species species, int[] ivs, Nature nature, bool shiny)
    {
        var raids = GetRaidContainer(path, storyPrg);
        raids.Item1.Should().Be((0, 0));

        var container = raids.Item2;
        container.Should().NotBeNull();
        if (container is null)
            return;

        var raid = container!.Raids[denIndex];
        var enc = container.Encounters[denIndex];
        enc.Species.Should().Be((ushort)species);

        var param = enc.GetParam();
        var blank = new PK9
        {
            Species = enc.Species,
            Form = enc.Form
        };

        raid.GenerateDataPK9(blank, param, enc.Shiny, raid.Seed);
        Span<int> _ivs = stackalloc int[6];
        blank.GetIVs(_ivs);
        var encIVs = Utils.ToSpeedLast(_ivs);
        encIVs.SequenceEqual(ivs).Should().BeTrue();
        blank.Nature.Should().Be((nature));
        blank.IsShiny.Should().Be(shiny);
    }
}
