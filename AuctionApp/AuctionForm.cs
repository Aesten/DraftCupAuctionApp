using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using AuctionApp.JsonObjects;

namespace AuctionApp
{
    public partial class AuctionForm : Form
    {
        private readonly Form _parentForm;
        private readonly string _path;
        private readonly List<TextBox> _upcomingTextBoxes;
        private readonly List<PictureBox> _iconPictureBoxes;
        private readonly List<TeamFormElements> _teamFormElements;

        public AuctionState AuctionStateAccess { get; }

        public AuctionForm(Form parentForm, AuctionState auctionState, string path)
        {
            InitializeComponent();
            FormClosing += AuctionForm_FormClosing;
            
            _parentForm = parentForm;
            AuctionStateAccess = auctionState;
            _path = path;

            half_budget_display.Checked = auctionState.HalfBudgetDisplay;
            title_display.Text = auctionState.Title;
            
            // Create data structures to manage form items in groups
            _upcomingTextBoxes = new List<TextBox> { player_main_display, queue1, queue2, queue3 };
            _iconPictureBoxes = new List<PictureBox> { icon1, icon2, icon3, icon4, icon5 };
            _teamFormElements = new List<TeamFormElements>
            {
                new TeamFormElements(team1_captain, team1_money, team1_players, team1_expenses),
                new TeamFormElements(team2_captain, team2_money, team2_players, team2_expenses),
                new TeamFormElements(team3_captain, team3_money, team3_players, team3_expenses),
                new TeamFormElements(team4_captain, team4_money, team4_players, team4_expenses),
                new TeamFormElements(team5_captain, team5_money, team5_players, team5_expenses),
                new TeamFormElements(team6_captain, team6_money, team6_players, team6_expenses),
                new TeamFormElements(team7_captain, team7_money, team7_players, team7_expenses),
                new TeamFormElements(team8_captain, team8_money, team8_players, team8_expenses)
            };
            
            LoadTeams();
            UpdateQueueDisplayElements();
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

        private void LoadTeams()
        {
            for (var i = 0; i < Math.Min(AuctionStateAccess.Teams.Count, _teamFormElements.Count); i++)
            {
                team_selector.Items.Add(AuctionStateAccess.Teams[i].Captain);
                _teamFormElements[i].Captain.Text = AuctionStateAccess.Teams[i].Captain;
                _teamFormElements[i].Budget.Text = AuctionStateAccess.Teams[i].CurrentBudget.ToString("0.0");
                foreach (var t in AuctionStateAccess.Teams[i].Members)
                {
                    _teamFormElements[i].Players.Items.Add(t.Name);
                    _teamFormElements[i].Costs.Items.Add(t.Cost.ToString("0.0"));
                }
                UpdateBudgetDisplay(i);
            }
        }

        private void UpdateTeamMember_Addition(int teamIndex, decimal cost, string costString)
        {
            var player = AuctionStateAccess.PlayerQueue[0];
            AuctionStateAccess.PlayerQueue.RemoveAt(0);
            AuctionStateAccess.Teams[teamIndex].Members.Add(new AuctionState.PlayerMember
            {
                Name = player.Name,
                Cost = cost,
                Classes = player.Classes
            });
            
            _teamFormElements[teamIndex].Players.Items.Add(player.Name);
            _teamFormElements[teamIndex].Costs.Items.Add(costString);
            UpdateBudgetDisplay(teamIndex);
        }

        private void UpdateTeamMember_Removal(int teamIndex, int playerIndex)
        {
            var player = new Auction.Player
            {
                Name = AuctionStateAccess.Teams[teamIndex].Members[playerIndex].Name,
                Classes = AuctionStateAccess.Teams[teamIndex].Members[playerIndex].Classes
            };
            AuctionStateAccess.PlayerQueue.Insert(0, player);
            AuctionStateAccess.Teams[teamIndex].Members.RemoveAt(playerIndex);
            
            _teamFormElements[teamIndex].Players.Items.RemoveAt(playerIndex);
            _teamFormElements[teamIndex].Costs.Items.RemoveAt(playerIndex);
            UpdateBudgetDisplay(teamIndex);
        }

        public void UpdateQueueDisplayElements()
        {
            var remainder = AuctionStateAccess.QueueNumber;
            player_counter.Text = $@"{AuctionStateAccess.InitialNumber - AuctionStateAccess.QueueNumber} (/{AuctionStateAccess.InitialNumber})";
            auction_counter.Text = AuctionStateAccess.AuctionedNumber.ToString();
            for (var i = 0 ; i < 4 ; i++)
            {
                _upcomingTextBoxes[i].Text = remainder > i ? AuctionStateAccess.PlayerQueue[i].Name : @"-----";
            }

            if (remainder > 0)
            {
                DisplayClassIcons(AuctionStateAccess.PlayerQueue[0]);
            }
            else
            {
                foreach (var iconPictureBox in _iconPictureBoxes)
                {
                    iconPictureBox.Image = null;
                }
            }
        }
        
        private static Image LoadImage(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourceName);
            return stream != null ? Image.FromStream(stream) : null;
        }

        private void DisplayClassIcons(Auction.Player player)
        {
            switch (player.Classes.Count)
            {
                case 0:
                    _iconPictureBoxes[0].Image = null;
                    _iconPictureBoxes[1].Image = null;
                    _iconPictureBoxes[2].Image = LoadImage("AuctionApp.Icons.inf.png");
                    _iconPictureBoxes[3].Image = null;
                    _iconPictureBoxes[4].Image = null;
                    break;
                case 1:
                    _iconPictureBoxes[0].Image = null;
                    _iconPictureBoxes[1].Image = null;
                    _iconPictureBoxes[2].Image = LoadImage($"AuctionApp.Icons.{player.Classes[0]}.png");
                    _iconPictureBoxes[3].Image = null;
                    _iconPictureBoxes[4].Image = null;
                    break;
                case 2:
                    _iconPictureBoxes[0].Image = null;
                    _iconPictureBoxes[1].Image = LoadImage($"AuctionApp.Icons.{player.Classes[0]}.png");
                    _iconPictureBoxes[2].Image = null;
                    _iconPictureBoxes[3].Image = LoadImage($"AuctionApp.Icons.{player.Classes[1]}.png");
                    _iconPictureBoxes[4].Image = null;
                    break;
                default:
                    _iconPictureBoxes[0].Image = LoadImage($"AuctionApp.Icons.{player.Classes[0]}.png");
                    _iconPictureBoxes[1].Image = null;
                    _iconPictureBoxes[2].Image = LoadImage($"AuctionApp.Icons.{player.Classes[1]}.png");
                    _iconPictureBoxes[3].Image = null;
                    _iconPictureBoxes[4].Image = LoadImage($"AuctionApp.Icons.{player.Classes[2]}.png");
                    break;
            }
        }

        private void UpdateBudgetDisplay(int index)
        {
            var team = AuctionStateAccess.Teams[index];
            _teamFormElements[index].Budget.Text = AuctionStateAccess.HalfBudgetDisplay
                ? (team.CurrentBudget - team.InitialBudget / 2).ToString("0.0")
                : team.CurrentBudget.ToString("0.0");
        }
        
        public void Cache()
        {
            AuctionStateAccess.Serialize(_path);
        }
        
        // Events
        
        private void auction_button_Click(object sender, EventArgs e)
        {
            if (AuctionStateAccess.QueueNumber <= 0) return;
            
            var index = team_selector.SelectedIndex;
            
            var cost = decimal.Parse(price.Text);
            if (cost > (AuctionStateAccess.HalfBudgetDisplay ? AuctionStateAccess.Teams[index].CurrentBudgetHalf : AuctionStateAccess.Teams[index].CurrentBudget))
            {
                MessageBox.Show(@"The selected captain cannot afford this player!", @"Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (AuctionStateAccess.Teams[index].Members.Count >= AuctionStateAccess.TeamSize)
            {
                MessageBox.Show(@"The selected captain's team is full!", @"Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            UpdateTeamMember_Addition(index, cost, price.Text);
            UpdateQueueDisplayElements();

            price.Text = @"0.1";
            team_selector.SelectedIndex = -1;
            
            Cache();
        }
        
        private void remove_button_Click(object sender, EventArgs e)
        {
            var teamIndex = _teamFormElements.FindIndex(element => element.Players.SelectedIndex != -1);

            if (teamIndex == -1)
            {
                MessageBox.Show(@"Failed to identify the player to remove", @"Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var playerIndex = _teamFormElements[teamIndex].Players.SelectedIndex;
            UpdateTeamMember_Removal(teamIndex, playerIndex);
            UpdateQueueDisplayElements();
            
            Cache();
        }

        private void skip_button_Click(object sender, EventArgs e)
        {
            if (AuctionStateAccess.QueueNumber <= 0) return;
            var player = AuctionStateAccess.PlayerQueue[0];
            AuctionStateAccess.PlayerQueue.RemoveAt(0);
            AuctionStateAccess.Skipped.Add(player);
            UpdateQueueDisplayElements();
            
            Cache();
        }
        
        private void half_budget_display_CheckedChanged(object sender, EventArgs e)
        {
            AuctionStateAccess.HalfBudgetDisplay = half_budget_display.Checked;
            if (_teamFormElements == null) return; // Check for the initial configuration of the checkbox
            
            for (var i = 0; i < AuctionStateAccess.Teams.Count; i++)
            {
                UpdateBudgetDisplay(i);
            }
        }
        
        private void AuctionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parentForm.Show();
        }

        private void players_SelectedIndexChanged(object sender, EventArgs e)
        {
            remove_button.Enabled = team1_players.SelectedIndex != -1 ||
                                    team2_players.SelectedIndex != -1 ||
                                    team3_players.SelectedIndex != -1 ||
                                    team4_players.SelectedIndex != -1 ||
                                    team5_players.SelectedIndex != -1 ||
                                    team6_players.SelectedIndex != -1 ||
                                    team7_players.SelectedIndex != -1 ||
                                    team8_players.SelectedIndex != -1;
        }

        private void team_selector_SelectedIndexChanged(object sender, EventArgs e)
        {
            auction_button.Enabled = team_selector.SelectedIndex != -1;
        }

        private void manage_skipped_button_Click(object sender, EventArgs e)
        {
            new SkippedPlayersPopup(this).Show();
        }
    }
}