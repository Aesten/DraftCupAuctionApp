using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using PlayerDrafter.Data;

namespace PlayerDrafter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void begin_auction_button_Click(object sender, EventArgs e)
        {
            StartAuction(player_list_file_path.Text, team_list_file_path.Text, true);
        }

        private void open_player_list_button_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = @"Select Player List JSON File";
            openFileDialog.Filter = @"JSON Files (*.json)|*.json";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                player_list_file_path.Text = openFileDialog.FileName;
            }
        }

        private void open_team_list_button_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = @"Select Team List JSON File";
            openFileDialog.Filter = @"JSON Files (*.json)|*.json";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                team_list_file_path.Text = openFileDialog.FileName;
            }
        }

        private void load_from_cache_button_Click(object sender, EventArgs e)
        {
            StartAuction(Path.Combine(Program.AppDir, "player_list_cache.json"), 
                Path.Combine(Program.AppDir, "team_list_cache.json"),
                false);
        }

        private void edit_player_list_Click(object sender, EventArgs e)
        {
            var playerListFilePath = player_list_file_path.Text;
            
            if (string.IsNullOrEmpty(playerListFilePath))
            {
                MessageBox.Show(@"Please select a JSON file for the player list.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            if (!File.Exists(playerListFilePath))
            {
                MessageBox.Show(@"The selected player list file does not exist.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            try
            {
                var playerListContent = File.ReadAllText(playerListFilePath);
                var playerList = JsonConvert.DeserializeObject<List<PlayerData>>(playerListContent);
                
                if (!DataUtil.IsValid(playerList))
                {
                    MessageBox.Show(@"Invalid JSON content for the player list.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                Hide();
                var playerListEditor = new PlayerListEditor(playerList, playerListFilePath, this);
                playerListEditor.Show();
            }
            catch (JsonReaderException)
            {
                MessageBox.Show(@"Invalid JSON format. Error loading JSON file.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An unexpected error occurred: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void new_player_list_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = @"JSON files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    File.WriteAllText(saveFileDialog.FileName, @"[]");
                    player_list_file_path.Text = saveFileDialog.FileName;
                    edit_player_list_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($@"Error creating new JSON file: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void edit_team_list_Click(object sender, EventArgs e)
        {
            var teamListFilePath = team_list_file_path.Text;
            
            if (string.IsNullOrEmpty(teamListFilePath))
            {
                MessageBox.Show(@"Please select a JSON file for the team list.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            if (!File.Exists(teamListFilePath))
            {
                MessageBox.Show(@"The selected team list file does not exist.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            try
            {
                var teamListContent = File.ReadAllText(teamListFilePath);
                var teamList = JsonConvert.DeserializeObject<List<Team>>(teamListContent);
                
                if (!DataUtil.IsValid(teamList))
                {
                    MessageBox.Show(@"Invalid JSON content for the team list.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                Hide();
                var teamListEditor = new TeamListEditor(teamList, teamListFilePath, this);
                teamListEditor.Show();
            }
            catch (JsonReaderException)
            {
                MessageBox.Show(@"Invalid JSON format. Error loading JSON file.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An unexpected error occurred: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void new_team_list_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = @"JSON files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    File.WriteAllText(saveFileDialog.FileName, @"[]");
                    team_list_file_path.Text = saveFileDialog.FileName;
                    edit_team_list_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($@"Error creating new JSON file: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void StartAuction(string playerListPath, string teamListPath, bool shuffle)
        {
            if (!File.Exists(playerListPath))
            {
                MessageBox.Show(@"No cached player list file exists.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            if (!File.Exists(teamListPath))
            {
                MessageBox.Show(@"No cached team list file exists.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var playerListContent = File.ReadAllText(playerListPath);
                var playerList = JsonConvert.DeserializeObject<List<PlayerData>>(playerListContent);
                
                if (!DataUtil.IsValid(playerList))
                {
                    MessageBox.Show(@"Invalid JSON content for the player list.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var teamListContent = File.ReadAllText(teamListPath);
                var teamList = JsonConvert.DeserializeObject<List<Team>>(teamListContent);
                
                if (!DataUtil.IsValid(teamList))
                {
                    MessageBox.Show(@"Invalid JSON content for the team list.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                Hide();
                var auctionForm = new AuctionForm(playerList, teamList, this, shuffle);
                auctionForm.Show();
            }
            catch (JsonReaderException)
            {
                MessageBox.Show(@"Invalid JSON format. Error loading cached JSON.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An unexpected error occurred: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}