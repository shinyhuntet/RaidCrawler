using PKHeX.Core;
using SysBot.Pokemon;
using System.Numerics;
using System.Reflection;

namespace RaidCrawler.Core.Structures;

public static partial class Utils
{
    private static readonly Assembly thisAssembly;
    private static readonly Dictionary<string, string> resourceNameMap;

    static Utils()
    {
        thisAssembly = Assembly.GetExecutingAssembly()!;
        resourceNameMap = BuildLookup(thisAssembly.GetManifestResourceNames());
    }

    private static Dictionary<string, string> BuildLookup(IReadOnlyCollection<string> manifestNames)
    {
        var result = new Dictionary<string, string>(manifestNames.Count);
        foreach (var resName in manifestNames)
        {
            var fileName = GetFileName(resName);
            if (!result.ContainsKey(fileName))
                result.Add(fileName, resName);
        }
        return result;
    }

    private static string GetFileName(string resName)
    {
        var period = resName.LastIndexOf('.', resName.Length - 6);
        var start = period + 1;
        System.Diagnostics.Debug.Assert(start != 0);

        // text file fetch excludes ".txt" (mixed case...); other extensions are used (all lowercase).
        return resName.EndsWith(".txt", StringComparison.Ordinal) ? resName[start..^4].ToLowerInvariant() : resName[start..];
    }

    public static byte[] GetBinaryResource(string name)
    {
        if (!resourceNameMap.TryGetValue(name, out var resName))
            return [];

        using var resource = thisAssembly.GetManifestResourceStream(resName);
        if (resource is null)
            return [];

        var buffer = new byte[resource.Length];
        _ = resource.Read(buffer, 0, (int)resource.Length);
        return buffer;
    }

    public static string? GetStringResource(string name)
    {
        if (!resourceNameMap.TryGetValue(name.ToLowerInvariant(), out var resourceName))
            return null;

        using var resource = thisAssembly.GetManifestResourceStream(resourceName);
        if (resource is null)
            return null;

        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }

    public static Version? GetLatestVersion()
    {
        const string endpoint = "https://api.github.com/repos/LegoFigure11/RaidCrawler/releases/latest";
        var response = NetUtil.GetStringFromURL(new Uri(endpoint));
        if (response is null) return null;

        const string tag = "tag_name";
        var index = response.IndexOf(tag, StringComparison.Ordinal);
        if (index == -1) return null;

        var first = response.IndexOf('"', index + tag.Length + 1) + 1;
        if (first == 0) return null;

        var second = response.IndexOf('"', first);
        if (second == -1) return null;

        var tagString = response.AsSpan()[first..second].TrimStart('v');

        var patchIndex = tagString.IndexOf('-');
        if (patchIndex != -1) tagString = tagString.ToString().Remove(patchIndex).AsSpan();

        return !Version.TryParse(tagString, out var latestVersion) ? null : latestVersion;
    }

    public static async Task<Image> GetPokemonImage(PKM pk)
    {
        using HttpClient client = new();
        var url = await client.GetStreamAsync(PokeImg(pk, false)).ConfigureAwait(false);
        Image Pokemon = Image.FromStream(url);

        return Pokemon;
    }

    public static string PokeImg(PKM pkm, bool canGmax)
    {
        bool md = false;
        bool fd = false;
        string[] baseLink;
        baseLink = "https://raw.githubusercontent.com/zyro670/HomeImages/master/128x128/poke_capture_0001_000_mf_n_00000000_f_n.png".Split('_');

        if (Enum.IsDefined(typeof(GenderDependent), pkm.Species) && !canGmax && pkm.Form is 0)
        {
            if (pkm.Gender is 0 && pkm.Species is not (ushort)PKHeX.Core.Species.Torchic)
                md = true;
            else fd = true;
        }

        int form = pkm.Species switch
        {
            (ushort)PKHeX.Core.Species.Sinistea or (ushort)PKHeX.Core.Species.Polteageist or (ushort)PKHeX.Core.Species.Rockruff or (ushort)PKHeX.Core.Species.Mothim => 0,
            (ushort)PKHeX.Core.Species.Alcremie when pkm.IsShiny || canGmax => 0,
            _ => pkm.Form,

        };
        if (pkm.Species is (ushort)PKHeX.Core.Species.Sneasel)
        {
            if (pkm.Gender is 0)
                md = true;
            else fd = true;
        }

        if (pkm.Species is (ushort)PKHeX.Core.Species.Basculegion)
        {
            if (pkm.Gender is 0)
            {
                md = true;
                pkm.Form = 0;
            }
            else { pkm.Form = 1; }

            string s = pkm.IsShiny ? "r" : "n";
            string g = md && pkm.Gender is not 1 ? "md" : "fd";
            return $"https://raw.githubusercontent.com/zyro670/HomeImages/master/128x128/poke_capture_0" + $"{pkm.Species}" + "_00" + $"{pkm.Form}" + "_" + $"{g}" + "_n_00000000_f_" + $"{s}" + ".png";
        }

        baseLink[2] = pkm.Species < 10 ? $"000{pkm.Species}" : pkm.Species < 100 && pkm.Species > 9 ? $"00{pkm.Species}" : pkm.Species >= 1000 ? $"{pkm.Species}" : $"0{pkm.Species}";
        baseLink[3] = pkm.Form < 10 ? $"00{form}" : $"0{form}";
        baseLink[4] = pkm.PersonalInfo.OnlyFemale ? "fo" : pkm.PersonalInfo.OnlyMale ? "mo" : pkm.PersonalInfo.Genderless ? "uk" : fd ? "fd" : md ? "md" : "mf";
        baseLink[5] = canGmax ? "g" : "n";
        baseLink[6] = "0000000" + (pkm.Species is (ushort)PKHeX.Core.Species.Alcremie && !canGmax ? pkm.Data[0xE4] : 0);
        baseLink[8] = pkm.IsShiny ? "r.png" : "n.png";
        return string.Join("_", baseLink);
    }

    public static string GetFormString(ushort species, byte form, GameStrings formStrings, EntityContext context = EntityContext.Gen9)
    {
        var result = ShowdownParsing.GetStringFromForm(form, formStrings, species, context);
        if (result.Length > 0 && result[0] != '-')
            return result.Insert(0, "-");
        return result;
    }

    public static int[] ToSpeedLast(ReadOnlySpan<int> ivs) => [ivs[0], ivs[1], ivs[2], ivs[4], ivs[5], ivs[3]];
}
