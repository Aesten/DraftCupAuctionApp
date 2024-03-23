using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using PlayerDrafter.Data;

namespace PlayerDrafter
{
    public partial class AuctionForm : Form
    {
        private readonly Form _parentForm;
        private readonly List<PlayerData> _playerDataList;
        private readonly List<Team> _teamList;
        private readonly List<TeamFormElements> _teamElements;
        private readonly List<AuctionStep> _auctionSteps;
        
        public AuctionForm(List<PlayerData> playerDataList, List<Team> teamList, Form parentForm, bool shuffle)
        {
            FormClosing += AuctionForm_FormClosing;
            _parentForm = parentForm;
            _auctionSteps = new List<AuctionStep>();
            if (shuffle) Shuffle(playerDataList);
            _playerDataList = playerDataList;
            _teamList = teamList;
            InitializeComponent();
            _teamElements = ListUpTeamElements();
            UpdateState();
            if (teamList.Count > 0) LoadTeams();
        }

        private List<TeamFormElements> ListUpTeamElements()
        {
            return new List<TeamFormElements>
            {
                new TeamFormElements(team1_captain, team1_money, team1_players, team1_expenses),
                new TeamFormElements(team2_captain, team2_money, team2_players, team2_expenses),
                new TeamFormElements(team3_captain, team3_money, team3_players, team3_expenses),
                new TeamFormElements(team4_captain, team4_money, team4_players, team4_expenses),
                new TeamFormElements(team5_captain, team5_money, team5_players, team5_expenses),
                new TeamFormElements(team6_captain, team6_money, team6_players, team6_expenses)
            };
        }

        private void LoadTeams()
        {
            for (var i = 0; i < Math.Min(_teamList.Count, _teamElements.Count); i++)
            {
                team_selector.Items.Add(_teamList[i].Captain);
                _teamElements[i].Captain.Text = _teamList[i].Captain;
                _teamElements[i].Budget.Text = _teamList[i].Money.ToString("0.0");
                foreach (var t in _teamList[i].Players)
                {
                    _teamElements[i].Players.Items.Add(t.Name);
                    _teamElements[i].Costs.Items.Add(t.Cost.ToString("0.0"));
                }
            }
        }

        private void UpdateState()
        {
            var count = _playerDataList.Count;
            counterLabel.Text = Math.Max(0, count - 1).ToString();
            var totalPlayers = _teamList.SelectMany(team => team.Players).Count();
            auction_counter.Text = totalPlayers.ToString();
            UpdatePlayerDataDisplay(count > 0 ? _playerDataList[0] : null);
            queue1.Text = count > 1 ? _playerDataList[1].Player : @"-----";
            queue2.Text = count > 2 ? _playerDataList[2].Player : @"-----";
            queue3.Text = count > 3 ? _playerDataList[3].Player : @"-----";
        }

        private void UpdatePlayerDataDisplay(PlayerData playerData)
        {
            icon1.Image = null;
            icon2.Image = null;
            icon3.Image = null;
            icon4.Image = null;
            icon5.Image = null;
            
            if (playerData == null)
            {
                player.Text = @"-----";
                comment.Text = @"-----";
                return;
            }
            
            player.Text = playerData.Player;
            comment.Text = playerData.Comment;

            switch (playerData.Classes.Count)
            {
                case 0:
                    icon3.Image = Image.FromFile("icons\\inf.png");
                    break;
                case 1:
                    icon3.Image = Image.FromFile($"icons\\{playerData.Classes[0]}.png");
                    break;
                case 2:
                    icon2.Image = Image.FromFile($"icons\\{playerData.Classes[0]}.png");
                    icon4.Image = Image.FromFile($"icons\\{playerData.Classes[1]}.png");
                    break;
                default:
                    icon1.Image = Image.FromFile($"icons\\{playerData.Classes[0]}.png");
                    icon3.Image = Image.FromFile($"icons\\{playerData.Classes[1]}.png");
                    icon5.Image = Image.FromFile($"icons\\{playerData.Classes[2]}.png");
                    break;
            }
        }

        private void UpdateTeams(string name, decimal cost, int index)
        {
            _teamList[index].Players.Add(new Player(name, cost));
            _teamList[index].Money -= cost;
            _teamElements[index].Players.Items.Add(name);
            _teamElements[index].Costs.Items.Add(price.Text);
            
            var expenses = _teamList[index].Players.Sum(p => p.Cost);
            var initialBudget = _teamList[index].Money + expenses;
            _teamElements[index].Budget.Text = half_budget_display.Checked
                ? (initialBudget / 2 - expenses).ToString("0.0")
                : _teamList[index].Money.ToString("0.0");
        }
        
        private static void Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        private void auctionedButton_Click(object sender, EventArgs e)
        {
            if (_playerDataList.Count <= 0) return;
            
            var teamCaptain = team_selector.Text;
            var index = _teamList.FindIndex(team => team.Captain == teamCaptain);
            if (index < 0)
            {
                MessageBox.Show(@"Please select a captain", 
                    @"Invalid Value", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }
            
            var cost = decimal.Parse(price.Text);
            if (cost > decimal.Parse(_teamElements[index].Budget.Text))
            {
                MessageBox.Show(@"The selected captain cannot afford this player!", 
                    @"Invalid Value", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }
            
            var data = _playerDataList[0];
            _playerDataList.RemoveAt(0);
            UpdateTeams(data.Player, cost, index);
            UpdateState();

            price.Text = @"0.0";
            team_selector.SelectedIndex = -1;
            
            Cache();
            
            _auctionSteps.Add(new AuctionStep(data, index));
            undo_button.Enabled = true;
        }
        
        private void undo_button_Click(object sender, EventArgs e)
        {
            var step = _auctionSteps[_auctionSteps.Count - 1];

            if (step.TeamIndex >= 0)
            {
                var playerNumber = _teamList[step.TeamIndex].Players.Count;
                var cost = decimal.Parse(_teamElements[step.TeamIndex].Costs.Items[playerNumber - 1].ToString());
            
                _teamList[step.TeamIndex].Players.RemoveAt(playerNumber - 1);
                _teamList[step.TeamIndex].Money += cost;
                _teamElements[step.TeamIndex].Players.Items.RemoveAt(playerNumber - 1);
                _teamElements[step.TeamIndex].Costs.Items.RemoveAt(playerNumber - 1);
            
                var expenses = _teamList[step.TeamIndex].Players.Sum(p => p.Cost);
                var initialBudget = _teamList[step.TeamIndex].Money + expenses;
                _teamElements[step.TeamIndex].Budget.Text = half_budget_display.Checked
                    ? (initialBudget / 2 - expenses).ToString("0.0")
                    : _teamList[step.TeamIndex].Money.ToString("0.0");
            }
            else
            {
                _playerDataList.RemoveAt(_playerDataList.Count - 1);
            }
            
            _playerDataList.Insert(0, step.Player);
            UpdateState();
            price.Text = @"0.0";
            team_selector.SelectedIndex = -1;
            Cache();
            
            _auctionSteps.RemoveAt(_auctionSteps.Count - 1);
            if (_auctionSteps.Count <= 0) undo_button.Enabled = false;
        }

        private void skipButton_Click(object sender, EventArgs e)
        {
            if (_playerDataList.Count <= 1) return;
            var data = _playerDataList[0];
            _playerDataList.RemoveAt(0);
            _playerDataList.Add(data);
            UpdateState();
            _auctionSteps.Add(new AuctionStep(data, -1));
            undo_button.Enabled = true;
        }
        
        private void AuctionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parentForm.Show();
        }

        private void Cache()
        {
            try
            {
                var playerDataListJson = JsonConvert.SerializeObject(_playerDataList);
                File.WriteAllText(Path.Combine(Program.AppDir, "player_list_cache.json"), playerDataListJson);
                var teamDataListJson = JsonConvert.SerializeObject(_teamList);
                File.WriteAllText(Path.Combine(Program.AppDir, "team_list_cache.json"), teamDataListJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Failed saving cache files: {ex.Message}");
            }
        }

        private class TeamFormElements
        {
            public TeamFormElements(TextBox captain, TextBox budget, ListBox players, ListBox costs)
            {
                Captain = captain;
                Budget = budget;
                Players = players;
                Costs = costs;
            }

            public TextBox Captain { get; }
            public TextBox Budget { get; }
            public ListBox Players { get; }
            public ListBox Costs { get; }
        }

        private class AuctionStep
        {
            public AuctionStep(PlayerData player, int teamIndex)
            {
                Player = player;
                TeamIndex = teamIndex;
            }

            public PlayerData Player { get; }
            public int TeamIndex { get; }
        }

        private void half_budget_display_CheckedChanged(object sender, EventArgs e)
        {
            for (var i = 0; i < _teamList.Count; i++)
            {
                var expenses = _teamList[i].Players.Sum(p => p.Cost);
                var initialBudget = _teamList[i].Money + expenses;
                _teamElements[i].Budget.Text = half_budget_display.Checked
                    ? (initialBudget / 2 - expenses).ToString("0.0")
                    : _teamList[i].Money.ToString("0.0");
            }
        }
    }
}