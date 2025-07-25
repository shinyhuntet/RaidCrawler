using PKHeX.Core;
using pkNX.Structures.FlatBuffers.Gen9;
using System.ComponentModel;
using System.Text.Json;

namespace RaidCrawler.Core.Structures;

public record RaidContainer
{
    public IReadOnlyList<Raid> Raids { get; private set; } = new List<Raid>();
    public IReadOnlyList<ITeraRaid> Encounters { get; private set; } = new List<ITeraRaid>();
    public IReadOnlyList<IReadOnlyList<(int, int, int)>> Rewards { get; private set; } = new List<List<(int, int, int)>>();
    public string Game { get; private set; } = "Scarlet";
    public GameStrings Strings { get; private set; }

    public RaidContainer(string game)
    {
        Game = game;
        Strings = GameInfo.GetStrings("en");
    }
    public int GetRaidCount() => Raids.Count;
    public void ClearRaids() => Raids = new List<Raid>();
    public void SetRaids(IReadOnlyList<Raid> raids) => Raids = raids;

    public int GetEncounterCount() => Encounters.Count;
    public void ClearEncounters() => Encounters = new List<ITeraRaid>();
    public void SetEncounters(IReadOnlyList<ITeraRaid> encs) => Encounters = encs;

    public int GetRewardsCount() => Rewards.Count;
    public void ClearRewards() => Rewards = new List<List<(int, int, int)>>();
    public void SetRewards(IReadOnlyList<IReadOnlyList<(int, int, int)>> rewards) => Rewards = rewards;

    public void SetGame(string game) => Game = game;

}
