using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AuctionApp.Data;
using Newtonsoft.Json;

namespace AuctionApp
{
    public partial class TeamListEditor : Form
    {
        private readonly Form _parentForm;
        private readonly string _teamListPath;
        private readonly List<Team> _teamList;
        private Team _selectedTeam;
        private bool _edited;
        private bool _handleEvents;
        private bool _handleSelectorEdit;

        public ListBox GetPlayerListBox()
        {
            return player_list;
        }
        
        public ListBox GetCostListBox()
        {
            return cost_list;
        }

        public NumericUpDown GetRemainingBudget()
        {
            return remaining_budget;
        }

        public Team GetSelectedTeam()
        {
            return _selectedTeam;
        }
        
        public void UpdateUsedBudget()
        {
            used_budget.Text = _selectedTeam.Players.Sum(player => player.Cost).ToString("0.0");
        }
        
        public TeamListEditor(List<Team> teamList, string teamListPath, Form parentForm)
        {
            _teamListPath = teamListPath;
            _parentForm = parentForm;
            _teamList = teamList;
            FormClosing += TeamListEditor_FormClosing;
            InitializeComponent();
            Initialize();
        }
        
        private void Initialize()
        {
            _edited = false;
            _handleEvents = true;
            _handleSelectorEdit = true;
            
            foreach (var team in _teamList)
            {
                team_selector.Items.Add(team.Captain);
            }
            team_count.Text = _teamList.Count.ToString();
        }
        
        private void TeamListEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_edited)
            {
                var result = MessageBox.Show(@"Do you want to save changes before closing?", @"Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                    case DialogResult.Yes:
                        save_Click(null, null);
                        _parentForm.Show();
                        break;
                    case DialogResult.None:
                    case DialogResult.OK:
                    case DialogResult.Abort:
                    case DialogResult.Retry:
                    case DialogResult.Ignore:
                    case DialogResult.No:
                    default:
                        _parentForm.Show();
                        break;
                }
            }
            else
            {
                _parentForm.Show();
            }
        }
        
        private void add_Click(object sender, EventArgs e)
        {
            _edited = true;
            _selectedTeam = new Team(@"name", 20.0m, new List<Player>());
            _teamList.Add(_selectedTeam);
            team_selector.Items.Add(_selectedTeam.Captain);
            team_selector.SelectedIndex = team_selector.Items.Count - 1;
            name.Text = _selectedTeam.Captain;
            team_count.Text = _teamList.Count.ToString();
            remaining_budget.Text = _selectedTeam.Money.ToString("0.0");
            player_list.Items.Clear();
            cost_list.Items.Clear();
            used_budget.Text = @"0.0";
        }

        private void delete_Click(object sender, EventArgs e)
        {
            _edited = true;
            _selectedTeam = null;
            var index = team_selector.SelectedIndex;
            team_selector.SelectedIndex = -1;
            team_selector.Items.RemoveAt(index);
            _teamList.RemoveAt(index);
            _handleEvents = false;
            name.Text = null;
            _handleEvents = true;
            team_count.Text = _teamList.Count.ToString();
            remaining_budget.Text = @"0.0";
            player_list.Items.Clear();
            cost_list.Items.Clear();
            used_budget.Text = @"0.0";
        }

        private void save_Click(object sender, EventArgs e)
        {
            _edited = false;
            var playerDataListJson = JsonConvert.SerializeObject(_teamList);
            File.WriteAllText(_teamListPath, playerDataListJson);
            MessageBox.Show(@"Edited file has been successfully saved", 
                @"Success", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
        }

        private void name_TextChanged(object sender, EventArgs e)
        {
            if (!_handleEvents) return;
            _edited = true;
            _handleSelectorEdit = false;
            var index = team_selector.SelectedIndex;
            team_selector.Items[index] = name.Text;
            _selectedTeam.Captain = name.Text;
            _handleSelectorEdit = true;
        }
        
        private void remaining_budget_ValueChanged(object sender, EventArgs e)
        {
            _edited = true;
            _selectedTeam.Money = decimal.Parse(remaining_budget.Text);
        }

        private void team_selector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_handleSelectorEdit) return;
            
            if (team_selector.SelectedIndex < 0)
            {
                Enable(false);
                return;
            }

            var editCheck = _edited; // Used in last line
            Enable(true);
            _selectedTeam = _teamList[team_selector.SelectedIndex];
            name.Text = _selectedTeam.Captain;
            remaining_budget.Text = _selectedTeam.Money.ToString("0.0");
            player_list.Items.Clear();
            cost_list.Items.Clear();
            var usedBudget = 0.0m;
            foreach (var player in _selectedTeam.Players)
            {
                player_list.Items.Add(player.Name);
                cost_list.Items.Add(player.Cost);
                usedBudget += player.Cost;
            }
            used_budget.Text = usedBudget.ToString("0.0");
            _edited = editCheck; // Revert _edited to the state before this code block executed
        }
        
        private void player_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            remove_player_button.Enabled = (player_list.SelectedIndex != -1);
            edit_player_button.Enabled = (player_list.SelectedIndex != -1);
        }

        private void add_player_button_Click(object sender, EventArgs e)
        {
            _edited = true;
            var playerForm = new PlayerForm(this);
            playerForm.Show();
        }

        private void remove_player_button_Click(object sender, EventArgs e)
        {
            _edited = true;
            var index = player_list.SelectedIndex;
            var cost = _selectedTeam.Players[index].Cost;
            player_list.SelectedIndex = -1;
            player_list.Items.RemoveAt(index);
            cost_list.Items.RemoveAt(index);
            _selectedTeam.Players.RemoveAt(index);
            remaining_budget.Text = (decimal.Parse(remaining_budget.Text) + cost).ToString("0.0");
        }

        private void edit_player_button_Click(object sender, EventArgs e)
        {
            _edited = true;
            var index = player_list.SelectedIndex;
            var playerForm = new PlayerForm(this, index, _selectedTeam.Players[index].Name, _selectedTeam.Players[index].Cost);
            playerForm.Show();
        }

        private void Enable(bool enabled)
        {
            name.Enabled = enabled;
            remaining_budget.Enabled = enabled;
            delete.Enabled = enabled;
            add_player_button.Enabled = enabled;
            remove_player_button.Enabled = enabled;
            edit_player_button.Enabled = enabled;
            player_list.Enabled = enabled;
            cost_list.Enabled = enabled;
        }
    }
}