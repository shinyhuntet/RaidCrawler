using PKHeX.Core;
using RaidCrawler.Core.Structures;
using System.Globalization;
using System.Text.Json;

namespace RaidCrawler.WinForms.SubForms
{
    public partial class FilterSettings : Form
    {
        private bool LangChange = false;
        private readonly List<RaidFilter> filters;
        private readonly BindingSource bs = [];
        private readonly TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

        public FilterSettings(ref List<RaidFilter> filters)
        {
            InitializeComponent();
            this.filters = filters;
            var settings = Properties.Settings.Default;
            Species.DataSource = Enum.GetValues<Species>().Where(z => z != PKHeX.Core.Species.MAX_COUNT).ToArray();
            TeraType.DataSource = Enum.GetValues<MoveType>().Where(z => z != MoveType.Any).ToArray();
            LanguageCombo.DataSource = Enum.GetValues<LanguageID>().Where(z => z != LanguageID.None && z != LanguageID.UNUSED_6).ToArray();
            LanguageCombo.SelectedIndex = 1;
            ItemList.DataSource = GameInfo.GetStrings("en").Item;
            NatureBox.DataSource = GameInfo.GetStrings("en").natures;

            Stars.SelectedIndex = 0;
            StarsComp.SelectedIndex = 0;
            HPComp.SelectedIndex = 0;
            AtkComp.SelectedIndex = 0;
            DefComp.SelectedIndex = 0;
            SpaComp.SelectedIndex = 0;
            SpdComp.SelectedIndex = 0;
            SpeComp.SelectedIndex = 0;

            ResetActiveFilters();
            if (ActiveFilters.Items.Count > 0)
                ActiveFilters.SelectedIndex = 0;
            if (ActiveFilters.SelectedIndex == -1)
                Remove.Enabled = false;
        }

        public void ResetActiveFilters()
        {
            if (bs.DataSource == null)
            {
                bs.DataSource = filters;
                ActiveFilters.DataSource = bs;
                ActiveFilters.DisplayMember = "Name";
            }
            else
            {
                bs.ResetBindings(false);
            }
            for (int i = 0; i < filters.Count; i++)
                ActiveFilters.SetItemChecked(i, filters[i].Enabled);
        }

        public void SelectFilter(RaidFilter filter)
        {
            FilterName.Text = filter.Name;
            Species.SelectedIndex = filter.Species != null ? (int)filter.Species : 0;
            Form.Value = filter.Form != null ? (int)filter.Form : 0;
            Nature.Text = filter.Nature != null ? string.Join(",", filter.Nature.Select(x => x.ToString()).ToArray()) : string.Empty;
            Stars.SelectedIndex = filter.Stars != null ? (int)filter.Stars - 1 : 0;
            StarsComp.SelectedIndex = filter.StarsComp;
            TeraType.SelectedIndex = filter.TeraType != null ? (int)filter.TeraType : 0;
            Gender.SelectedIndex = filter.Gender != null ? (int)filter.Gender : 0;
            SpeciesCheck.Checked = filter.Species != null;
            FormCheck.Checked = filter.Form != null;
            NatureCheck.Checked = filter.Nature != null;
            StarCheck.Checked = filter.Stars != null;
            TeraCheck.Checked = filter.TeraType != null;
            GenderCheck.Checked = filter.Gender != null;
            ScaleCheck.Checked = filter.ScaleList != null && filter.ScaleList.Count > 0;
            for (int i = 0; i < ScaleList.Items.Count; i++)
            {
                ScaleList.SetItemChecked(i, false);
            }
            if (filter.ScaleList != null && filter.ScaleList.Count > 0)
            {
                foreach (var val in filter.ScaleList)
                {
                    for (int i = 0; i < ScaleList.Items.Count; i++)
                    {
                        if ((PokeSizeDetailed)ScaleList.Items[i] == val)
                            ScaleList.SetItemChecked(i, true);
                    }
                }
            }
            ShinyCheck.Checked = filter.Shiny;
            SquareCheck.Checked = filter.Square;
            ECCheck.Checked = filter.RareEC;
            CheckRewards.Checked = filter.RewardItems != null && filter.RewardsCount > 0;
            Rewards.Text = filter.RewardItems != null ? string.Join(",", filter.RewardItems.Select(x => GameInfo.GetStrings("en").Item[x]).ToList())
                                                        : $"{GameInfo.GetStrings("en").Item[645]},{GameInfo.GetStrings("en").Item[795]},{GameInfo.GetStrings("en").Item[1606]},{GameInfo.GetStrings("en").Item[1904]},{GameInfo.GetStrings("en").Item[1905]},{GameInfo.GetStrings("en").Item[1906]},{GameInfo.GetStrings("en").Item[1907]},{GameInfo.GetStrings("en").Item[1908]}";
            RewardsComp.SelectedIndex = filter.RewardsComp;
            RewardsCount.Value = filter.RewardsCount;
            BatchFilters.Text = filter.BatchFilters != null ? string.Join(Environment.NewLine, filter.BatchFilters) : string.Empty;

            var ivbin = filter.IVBin;
            HP.Checked = (ivbin & 1) == 1;
            Atk.Checked = ((ivbin >> 1) & 1) == 1;
            Def.Checked = ((ivbin >> 2) & 1) == 1;
            SpA.Checked = ((ivbin >> 3) & 1) == 1;
            SpD.Checked = ((ivbin >> 4) & 1) == 1;
            Spe.Checked = ((ivbin >> 5) & 1) == 1;

            var ivvals = filter.IVVals;
            IVHP.Value = ivvals & 31;
            IVATK.Value = (ivvals >> 5) & 31;
            IVDEF.Value = (ivvals >> 10) & 31;
            IVSPA.Value = (ivvals >> 15) & 31;
            IVSPD.Value = (ivvals >> 20) & 31;
            IVSPE.Value = (ivvals >> 25) & 31;

            var ivcomp = filter.IVComps;
            HPComp.SelectedIndex = (ivcomp & 7);
            AtkComp.SelectedIndex = (ivcomp >> 3) & 7;
            DefComp.SelectedIndex = (ivcomp >> 6) & 7;
            SpaComp.SelectedIndex = (ivcomp >> 9) & 7;
            SpdComp.SelectedIndex = (ivcomp >> 12) & 7;
            SpeComp.SelectedIndex = (ivcomp >> 15) & 7;

            IVHP.Enabled = HP.Checked;
            IVATK.Enabled = Atk.Checked;
            IVDEF.Enabled = Def.Checked;
            IVSPA.Enabled = SpA.Checked;
            IVSPD.Enabled = SpD.Checked;
            IVSPE.Enabled = Spe.Checked;

            HPComp.Enabled = HP.Checked;
            AtkComp.Enabled = Atk.Checked;
            DefComp.Enabled = Def.Checked;
            SpaComp.Enabled = SpA.Checked;
            SpdComp.Enabled = SpD.Checked;
            SpeComp.Enabled = Spe.Checked;

            Species.Enabled = SpeciesCheck.Checked;
            Nature.Enabled = NatureCheck.Checked;
            NatureBox.Enabled = NatureCheck.Checked;
            Stars.Enabled = StarCheck.Checked;
            StarsComp.Enabled = StarCheck.Checked;
            Rewards.Enabled = CheckRewards.Checked;
            ButtonOpenRewardsList.Enabled = CheckRewards.Checked;
            RewardsCount.Enabled = CheckRewards.Checked;
            RewardsComp.Enabled = CheckRewards.Checked;
            ItemList.Enabled = CheckRewards.Checked;
            TeraType.Enabled = TeraCheck.Checked;
            Gender.Enabled = GenderCheck.Checked;
            ScaleList.Enabled = ScaleCheck.Checked;
        }

        private void Add_Filter_Click(object sender, EventArgs e)
        {
            if (FilterName.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Name is a required field!");
                return;
            }

            RaidFilter filter = new();
            var ivbin = ToInt(HP.Checked) << 0 | ToInt(Atk.Checked) << 1 | ToInt(Def.Checked) << 2 |
                        ToInt(SpA.Checked) << 3 | ToInt(SpD.Checked) << 4 | ToInt(Spe.Checked) << 5;
            var ivcomps = HPComp.SelectedIndex << 0 | AtkComp.SelectedIndex << 3 | DefComp.SelectedIndex << 6 |
                          SpaComp.SelectedIndex << 9 | SpdComp.SelectedIndex << 12 | SpeComp.SelectedIndex << 15;
            var ivvals = (int)IVHP.Value << 0 | (int)IVATK.Value << 5 | (int)IVDEF.Value << 10 |
                         (int)IVSPA.Value << 15 | (int)IVSPD.Value << 20 | (int)IVSPE.Value << 25;

            filter.Name = FilterName.Text.Trim();
            filter.Species = SpeciesCheck.Checked ? Species.SelectedIndex : null;
            filter.Form = FormCheck.Checked ? (int)Form.Value : null;
            filter.Nature = NatureCheck.Checked ? Nature.Text.Split(',').Select(z => (Nature)(Enum.Parse(typeof(Nature), textInfo.ToTitleCase(z.Trim().Replace(" ", ""))))).ToArray() : null;
            filter.Stars = StarCheck.Checked ? Stars.SelectedIndex + 1 : null;
            filter.StarsComp = StarsComp.SelectedIndex;
            filter.TeraType = TeraCheck.Checked ? TeraType.SelectedIndex : null;
            filter.Gender = GenderCheck.Checked ? Gender.SelectedIndex : null;
            filter.ScaleList = new();
            if (ScaleCheck.Checked && ScaleList.CheckedItems.Count > 0 && ScaleList.CheckedItems != null)
            {
                foreach (PokeSizeDetailed size in ScaleList.CheckedItems)
                {
#pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                    filter.ScaleList.Add(size);
#pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
                }
            }
            else
            {
                filter.ScaleList = null;
            }
            filter.Shiny = ShinyCheck.Checked;
            filter.Square = SquareCheck.Checked;
            filter.RareEC = ECCheck.Checked;
            filter.IVBin = ivbin;
            filter.IVVals = ivvals;
            filter.IVComps = ivcomps;
            filter.RewardItems = CheckRewards.Checked ? Rewards.Text.Split(',').Where(z => GameInfo.GetStrings("en").Item.ToList().IndexOf(textInfo.ToTitleCase(z.Trim().ToLower())) != -1 || GameInfo.GetStrings("en").Item.ToList().IndexOf(z) != -1).Select(z => GameInfo.GetStrings("en").Item.ToList().IndexOf(z) != -1 ? GameInfo.GetStrings("en").Item.ToList().IndexOf(z) : GameInfo.GetStrings("en").Item.ToList().IndexOf(textInfo.ToTitleCase(z.Trim().ToLower()))).ToList().Distinct().ToList() : null;
            filter.RewardsCount = (int)RewardsCount.Value;
            filter.RewardsComp = RewardsComp.SelectedIndex;
            filter.BatchFilters = BatchFilters.Text.Trim() == string.Empty ? null : BatchFilters.Text.Split(Environment.NewLine);
            filter.Enabled = true;
            var msgitem = "Item List";
            if (filter.RewardItems != null && filter.RewardItems.Count > 0 && filter.RewardsCount > 0)
            {
                foreach (int item in filter.RewardItems)
                    msgitem += $"{Environment.NewLine}Item Name: {GameInfo.GetStrings("en").Item[item]}, Item Index: {item}";
            }
            else
            {
                msgitem += $"{Environment.NewLine}Null";
            }

            if (filter.IsFilterSet())
            {
                for (int i = 0; i < ActiveFilters.Items.Count; i++)
                {
                    var f = filters.ElementAt(i);
                    if (f.Name == filter.Name)
                    {
                        filters.RemoveAt(i);
                        break;
                    }
                }

                filters.Add(filter);
                ResetActiveFilters();
                ActiveFilters.SelectedIndex = ActiveFilters.Items.Count - 1;
                MessageBox.Show(msgitem);
            }
            else
            {
                MessageBox.Show("You have not set any stop conditions. No filter will be added.");
            }
        }

        private static int ToInt(bool b) => b ? 1 : 0;
        private void ItemList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(LangChange)
            {
                LangChange = false;
                return;
            }
            if (ItemList.Items == null || ItemList.Items.Count == 0 || ItemList.SelectedIndex <= 0)
                return;

            if (string.IsNullOrEmpty(Rewards.Text))
                Rewards.Text = GameInfo.GetStrings("en").Item[ItemList.SelectedIndex];
            else
                Rewards.Text += "," + GameInfo.GetStrings("en").Item[ItemList.SelectedIndex];

            var rewards = Rewards.Text.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(z => textInfo.ToTitleCase(z.Trim().Replace(" ", "").ToLower())).ToList();
            rewards = rewards.Distinct().ToList();
            if(rewards != null && rewards.Count > 0)
                Rewards.Text = string.Join(",", rewards);
        }
        private void NatureBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LangChange)
            {
                LangChange = false;
                return;
            }
            if (NatureBox.Items == null || NatureBox.Items.Count == 0 ||  NatureBox.SelectedIndex < 0)
                return;

            if (string.IsNullOrEmpty(Nature.Text))
                Nature.Text = GameInfo.GetStrings("en").natures[NatureBox.SelectedIndex];
            else
                Nature.Text += "," + GameInfo.GetStrings("en").natures[NatureBox.SelectedIndex];

            var nature = Nature.Text.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(z => textInfo.ToTitleCase(z.Trim().Replace(" ", "").ToLower())).ToList();
            nature = nature.Distinct().ToList();
            if(nature != null && nature.Count > 0)
                Nature.Text = string.Join(",", nature);
        }
        private void LanguageCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var itemindex = ItemList.Enabled ? ItemList.SelectedIndex : -1;
            var natureindex = NatureBox.Enabled ? NatureBox.SelectedIndex : -1;
            LangChange = true;
            ItemList.DataSource = GameInfo.GetStrings(GameLanguage.LanguageCode(LanguageCombo.SelectedIndex)).itemlist;
            LangChange = true;
            NatureBox.DataSource = GameInfo.GetStrings(GameLanguage.LanguageCode(LanguageCombo.SelectedIndex)).natures;
            if (itemindex != -1)
                ItemList.SelectedIndex = itemindex;
            if (natureindex != -1)
                NatureBox.SelectedIndex = natureindex;
        }

        private void SpeciesCheck_CheckedChanged(object sender, EventArgs e)
        {
            Species.Enabled = SpeciesCheck.Checked;
        }
        private void Species_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!SpeciesCheck.Checked || Species.SelectedIndex == -1) 
            {
                Form.Maximum = 0;
                return;
            }
           Form.Maximum = PersonalTable.SV.GetFormEntry((ushort)Species.SelectedIndex, 0).FormCount - 1;
        }
        private void FormCheck_CheckedChanged(object sender, EventArgs e)
        {
            Form.Enabled = FormCheck.Checked;
        }

        private void NatureCheck_CheckedChanged(object sender, EventArgs e)
        {
            Nature.Enabled = NatureCheck.Checked;
            NatureBox.Enabled = NatureCheck.Checked;
        }

        private void StarCheck_CheckedChanged(object sender, EventArgs e)
        {
            Stars.Enabled = StarCheck.Checked;
            StarsComp.Enabled = StarCheck.Checked;
        }

        private void TeraCheck_CheckedChanged(object sender, EventArgs e)
        {
            TeraType.Enabled = TeraCheck.Checked;
        }

        private void GenderCheck_CheckedChanged(object sender, EventArgs e)
        {
            Gender.Enabled = GenderCheck.Checked;
        }

        private void ScaleListCheck_CheckedChanged(object sender, EventArgs e)
        {
            ScaleList.Enabled = ScaleCheck.Checked;
        }

        private void HP_CheckedChanged(object sender, EventArgs e)
        {
            IVHP.Enabled = HP.Checked;
            HPComp.Enabled = HP.Checked;
        }

        private void Atk_CheckedChanged(object sender, EventArgs e)
        {
            IVATK.Enabled = Atk.Checked;
            AtkComp.Enabled = Atk.Checked;
        }

        private void Def_CheckedChanged(object sender, EventArgs e)
        {
            IVDEF.Enabled = Def.Checked;
            DefComp.Enabled = Def.Checked;
        }

        private void SpA_CheckedChanged(object sender, EventArgs e)
        {
            IVSPA.Enabled = SpA.Checked;
            SpaComp.Enabled = SpA.Checked;
        }

        private void SpD_CheckedChanged(object sender, EventArgs e)
        {
            IVSPD.Enabled = SpD.Checked;
            SpdComp.Enabled = SpD.Checked;
        }

        private void Spe_CheckedChanged(object sender, EventArgs e)
        {
            IVSPE.Enabled = Spe.Checked;
            SpeComp.Enabled = Spe.Checked;
        }

        private void FilterSettings_FormClosing(object sender, EventArgs e)
        {
            HashSet<int> indexset = new(ActiveFilters.CheckedIndices.Cast<int>());
            for (int i = 0; i < filters.Count; i++)
                filters[i].Enabled = indexset.Contains(i);

            string output = JsonSerializer.Serialize(filters);
            using StreamWriter sw = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filters.json"));
            sw.Write(output);
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (ActiveFilters.Items.Count == 0 || ActiveFilters.SelectedIndex == -1)
                return;

            var idx = ActiveFilters.SelectedIndex;
            filters.RemoveAt(idx);
            ResetActiveFilters();
        }

        private void ActiveFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            Remove.Enabled = ActiveFilters.SelectedIndex >= 0;
            if (ActiveFilters.SelectedIndex < 0)
                return;
            SelectFilter(filters[ActiveFilters.SelectedIndex]);
        }

        private void ActiveFilters_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            filters[e.Index].Enabled = e.NewValue == CheckState.Checked;
        }

        private void FilterName_TextChanged(object sender, EventArgs e)
        {
            if (ActiveFilters.SelectedIndex > -1 && FilterName.Text == filters[ActiveFilters.SelectedIndex].Name)
                Add.Text = "Update Filter";
            else Add.Text = "Add Filter";
        }

        private void CheckRewards_CheckedChanged(object sender, EventArgs e)
        {
            Rewards.Enabled = CheckRewards.Checked;
            ButtonOpenRewardsList.Enabled = CheckRewards.Checked;
            RewardsComp.Enabled = CheckRewards.Checked;
            RewardsCount.Enabled = CheckRewards.Checked;
            ItemList.Enabled = CheckRewards.Checked;
        }

        private void ButtonOpenRewardsList_Click(object sender, EventArgs e)
        {
            if (Rewards.Text is not null && Rewards.Text != string.Empty)
            {
                List<int> IDs = Rewards.Text.Split(',').Where(x => GameInfo.GetStrings("en").Item.ToList().IndexOf(x) != -1 || GameInfo.GetStrings("en").Item.ToList().IndexOf(textInfo.ToTitleCase(x.Trim().ToLower())) != -1).Select(x => GameInfo.GetStrings("en").Item.ToList().IndexOf(x) != -1 ? GameInfo.GetStrings("en").Item.ToList().IndexOf(x) : GameInfo.GetStrings("en").Item.ToList().IndexOf(textInfo.ToTitleCase(x.Trim().ToLower()))).ToList().Distinct().ToList();
                using ItemIDs form = new(IDs);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    List<int> s = new();
                    if (form.CheckAbilityCapsule.Checked) s.Add(645);
                    if (form.CheckBottleCap.Checked) s.Add(795);
                    if (form.CheckAbilityPatch.Checked) s.Add(1606);
                    if (form.CheckSweet.Checked) s.Add(1904);
                    if (form.CheckSalty.Checked) s.Add(1905);
                    if (form.CheckSour.Checked) s.Add(1906);
                    if (form.CheckBitter.Checked) s.Add(1907);
                    if (form.CheckSpicy.Checked) s.Add(1908);
                    for (int i = 0; i < IDs.Count; i++)
                    {
                        if (IDs[i] != 645 && IDs[i] != 795 && IDs[i] != 1606 && IDs[i] != 1904 && IDs[i] != 1905 && IDs[i] != 1906 && IDs[i] != 1907 && IDs[i] != 1908)
                            s.Add(IDs[i]);
                    }


                    Rewards.Text = string.Join(",", s.Select(x => GameInfo.GetStrings("en").Item[x]).ToList());
                }
            }
            else
            {
                MessageBox.Show("Enter number or Item name!");
            }
        }

        private void ActiveFilters_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            ListBox lb = (ListBox)sender;
            Graphics g = e.Graphics;
            RaidFilter filter = (RaidFilter)lb.Items[e.Index];

            g.FillRectangle(new SolidBrush(((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? ColorTranslator.FromHtml("#000078d7") : Color.White), e.Bounds);
            g.DrawString(filter.Name, new Font(Name = "Segoe UI", 9), new SolidBrush(filter.Enabled ? e.ForeColor : Color.Gray), new PointF(e.Bounds.X, e.Bounds.Y));

            e.DrawFocusRectangle();
        }

        private void ShinyCheck_CheckedChanged(object sender, EventArgs e)
        {
            SquareCheck.Enabled = ShinyCheck.Checked;
            if (!ShinyCheck.Checked) SquareCheck.Checked = false;
        }
    }
}
