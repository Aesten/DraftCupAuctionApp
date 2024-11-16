using System;
using System.Windows.Forms;
using AuctionApp.Data;

namespace AuctionApp
{
    public partial class PlayerForm : Form
    {
        private readonly TeamListEditor _parentForm;
        private readonly int _index;
        private readonly decimal _initialCost;
        
        public PlayerForm(TeamListEditor parentForm)
        {
            _parentForm = parentForm;
            _index = -1;
            _initialCost = 0;
            InitializeComponent();
        }
        
        public PlayerForm(TeamListEditor parentForm, int index, string playerName, decimal playerCost)
        {
            _parentForm = parentForm;
            _index = index;
            _initialCost = playerCost;
            InitializeComponent();
            name.Text = playerName;
            cost.Text = playerCost.ToString("0.0");
        }


        private void confirm_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(name.Text))
            {
                var playerCost = decimal.Parse(cost.Text);
                var remainingBudget = decimal.Parse(_parentForm.GetRemainingBudget().Text);

                if (remainingBudget + _initialCost < playerCost)
                {
                    MessageBox.Show($@"Player cost is over the remaining budget", 
                        @"Warning", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Warning);
                    return;
                }
                
                if (_index == -1)
                {
                    _parentForm.GetPlayerListBox().Items.Add(name.Text);
                    _parentForm.GetCostListBox().Items.Add(cost.Text);
                    _parentForm.GetSelectedTeam().Players.Add(new Player(name.Text, decimal.Parse(cost.Text)));
                }
                else
                {
                    _parentForm.GetPlayerListBox().Items[_index] = name.Text;
                    _parentForm.GetCostListBox().Items[_index] = cost.Text;
                    var player = _parentForm.GetSelectedTeam().Players[_index];
                    player.Name = name.Text;
                    player.Cost = decimal.Parse(cost.Text);
                }

                _parentForm.GetRemainingBudget().Text = (remainingBudget + _initialCost - playerCost).ToString("0.0");
                _parentForm.UpdateUsedBudget();
                Close();
                _parentForm.Show();
            }
            else
            {
                MessageBox.Show($@"Name cannot be empty", 
                    @"Warning", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
            }
        }
    }
}