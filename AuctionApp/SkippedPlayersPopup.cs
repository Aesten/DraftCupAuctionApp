using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AuctionApp.JsonObjects;

namespace AuctionApp
{
    public partial class SkippedPlayersPopup : Form
    {
        private readonly AuctionForm _parentForm;
        
        public SkippedPlayersPopup(AuctionForm parentForm)
        {
            _parentForm = parentForm;
            InitializeComponent();
            foreach (var player in parentForm.AuctionStateAccess.Skipped)
            {
                skipped_player_list.Items.Add(player.Name);
            }
        }

        private void skipped_player_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            pick_button.Enabled = skipped_player_list.SelectedIndex != -1;
        }

        private void pick_button_Click(object sender, EventArgs e)
        {
            var player = _parentForm.AuctionStateAccess.Skipped[skipped_player_list.SelectedIndex];
            _parentForm.AuctionStateAccess.Skipped.RemoveAt(skipped_player_list.SelectedIndex);
            _parentForm.AuctionStateAccess.PlayerQueue.Insert(0, player);
            
            _parentForm.UpdateQueueDisplayElements();
            _parentForm.Cache();
            Close();
        }

        private void refill_button_Click(object sender, EventArgs e)
        {
            foreach (var player in _parentForm.AuctionStateAccess.Skipped)
            {
                _parentForm.AuctionStateAccess.PlayerQueue.Add(player);
            }
            
            _parentForm.AuctionStateAccess.Skipped = new List<AuctionState.Player>();
            
            _parentForm.UpdateQueueDisplayElements();
            _parentForm.Cache();
            Close();
        }
    }
}