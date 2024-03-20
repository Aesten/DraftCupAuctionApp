using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using PlayerDrafter.Data;

namespace PlayerDrafter
{
    public partial class PlayerListEditor : Form
    {
        private readonly Form _parentForm;
        private readonly string _playerDataListPath;
        private readonly List<PlayerData> _playerDataList;
        private PlayerData _selectedPlayer;
        private bool _edited;
        
        public PlayerListEditor(List<PlayerData> playerList, string playerDataListPath, Form parentForm)
        {
            _playerDataListPath = playerDataListPath;
            _parentForm = parentForm;
            _playerDataList = playerList;
            FormClosing += PlayerListEditor_FormClosing;
            InitializeComponent();
            Initialize();
        }
        
        private void Initialize()
        {
            foreach (var player in _playerDataList)
            {
                player_selector.Items.Add(player.Player);
            }
            player_count.Text = _playerDataList.Count.ToString();
        }
        
        private void PlayerListEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_edited)
            {
                var result = MessageBox.Show(@"Do you want to save changes before closing?", 
                    @"Confirmation", 
                    MessageBoxButtons.YesNoCancel, 
                    MessageBoxIcon.Question);
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

        private void player_selector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (player_selector.SelectedIndex < 0)
            {
                Enable(false);
                return;
            }
            
            var editCheck = _edited; // Used in last line
            Enable(true);
            _selectedPlayer = _playerDataList[player_selector.SelectedIndex];
            name.Text = _selectedPlayer.Player;
            comment.Text = _selectedPlayer.Comment;
            inf_check.Checked = _selectedPlayer.Classes.Contains("inf");
            arc_check.Checked = _selectedPlayer.Classes.Contains("arc");
            cav_check.Checked = _selectedPlayer.Classes.Contains("cav");
            _edited = editCheck; // Revert _edited to the state before this code block executed
        }

        private void add_Click(object sender, EventArgs e)
        {
            _edited = true;
            _selectedPlayer = new PlayerData(@"name", new List<string>(), @"comment");
            _playerDataList.Add(_selectedPlayer);
            player_selector.Items.Add(@"name");
            player_selector.SelectedIndex = player_selector.Items.Count - 1;
            player_count.Text = _playerDataList.Count.ToString();
            name.Text = @"name";
            comment.Text = @"comment";
            inf_check.Checked = false;
            arc_check.Checked = false;
            cav_check.Checked = false;
        }

        private void delete_Click(object sender, EventArgs e)
        {
            _edited = true;
            _selectedPlayer = null;
            var index = player_selector.SelectedIndex;
            player_selector.SelectedIndex = - 1;
            player_selector.Items.RemoveAt(index);
            _playerDataList.RemoveAt(index);
            player_count.Text = _playerDataList.Count.ToString();
            name.Text = null;
            comment.Text = null;
            inf_check.Checked = false;
            arc_check.Checked = false;
            cav_check.Checked = false;
        }
        
        private void save_Click(object sender, EventArgs e)
        {
            _edited = false;
            var playerDataListJson = JsonConvert.SerializeObject(_playerDataList);
            File.WriteAllText(_playerDataListPath, playerDataListJson);
            MessageBox.Show(@"Edited file has been successfully saved", 
                @"Success", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
        }

        private void name_TextChanged(object sender, EventArgs e)
        {
            _edited = true;
            var index = player_selector.SelectedIndex;
            player_selector.SelectedIndexChanged -= player_selector_SelectedIndexChanged;
            player_selector.Items[index] = name.Text;
            player_selector.SelectedIndexChanged += player_selector_SelectedIndexChanged;
            _selectedPlayer.Player = name.Text;
        }

        private void comment_TextChanged(object sender, EventArgs e)
        {
            _edited = true;
            _selectedPlayer.Comment = comment.Text;
        }

        private void inf_check_CheckedChanged(object sender, EventArgs e)
        {
            _edited = true;
            if (inf_check.Checked) AddToClassesIfUnique("inf");
            else _selectedPlayer.Classes.Remove("inf");
        }

        private void arc_check_CheckedChanged(object sender, EventArgs e)
        {
            _edited = true;
            if (arc_check.Checked) AddToClassesIfUnique("arc");
            else _selectedPlayer.Classes.Remove("arc");
        }

        private void cav_check_CheckedChanged(object sender, EventArgs e)
        {
            _edited = true;
            if (cav_check.Checked) AddToClassesIfUnique("cav");
            else _selectedPlayer.Classes.Remove("cav");
        }

        private void AddToClassesIfUnique(string clazz)
        {
            if (!_selectedPlayer.Classes.Contains(clazz)) _selectedPlayer.Classes.Add(clazz);
        }

        private void Enable(bool enabled)
        {
            name.Enabled = enabled;
            comment.Enabled = enabled;
            inf_check.Enabled = enabled;
            arc_check.Enabled = enabled;
            cav_check.Enabled = enabled;
            delete.Enabled = enabled;
        }
    }
}