using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AuctionApp.JsonObjects;

namespace AuctionApp
{
    public partial class EditorForm : Form
    {
        private readonly Form _parentForm;
        private readonly Auction _auction;
        private readonly string _path;
        private readonly List<TextBox> _captainElements;
        private readonly List<NumericUpDown> _budgetElements;
        
        public EditorForm(Form parentForm, Auction auction, string path)
        {
            _parentForm = parentForm;
            _auction = auction;
            _path = path;
            InitializeComponent();
            FormClosing += AuctionEditorForm_FormClosing;

            _captainElements = new List<TextBox>
            {
                captain1, captain2, captain3, captain4, captain5, captain6, captain7, captain8
            };
            _budgetElements = new List<NumericUpDown>
            {
                budget1, budget2, budget3, budget4, budget5, budget6, budget7, budget8
            };
            
            DisplayData();
        }
        
        private void AuctionEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parentForm.Show();
        }

        private void DisplayData()
        {
            title_box.Text = _auction.Title;
            team_size.Text = _auction.TeamSize.ToString();
            
            for (var i = 0 ; i < _auction.Captains.Count ; i++)
            {
                _captainElements[i].Text = _auction.Captains[i].Name;
                _budgetElements[i].Text = _auction.Captains[i].Budget.ToString("0.0");
            }

            foreach (var player in _auction.Players)
            {
                player_grid.Rows.Add(
                    player.Name,
                    player.Classes.Contains("inf"),
                    player.Classes.Contains("arc"),
                    player.Classes.Contains("cav"));
            }
        }

        private void ParseData()
        {
            _auction.Title = title_box.Text;
            _auction.TeamSize = int.Parse(team_size.Text);

            var captains = new List<Auction.Captain>();
            for (var i = 0 ; i < 8 ; i++)
            {
                if (_captainElements[i].Text != "")
                {
                    captains.Add(new Auction.Captain
                    {
                        Name = _captainElements[i].Text,
                        Budget = decimal.Parse(_budgetElements[i].Text)
                    });
                }
            }

            _auction.Captains = captains;
            
            var players = new List<Auction.Player>();
            
            foreach (DataGridViewRow row in player_grid.Rows)
            {
                if (row.IsNewRow) continue;
                var player = new Auction.Player
                {
                    Name = row.Cells[0]?.Value.ToString(),
                    Classes = new List<string>()
                };
                if (Convert.ToBoolean(row.Cells[1].Value)) player.Classes.Add("inf");
                if (Convert.ToBoolean(row.Cells[2].Value)) player.Classes.Add("arc");
                if (Convert.ToBoolean(row.Cells[3].Value)) player.Classes.Add("cav");
                players.Add(player);
            }

            _auction.Players = players;
        }

        private void Save()
        {
            ParseData();
            _auction.Serialize(_path);
        }

        // Events

        private void save_button_Click(object sender, EventArgs e)
        {
            Save();
            MessageBox.Show($@"Modifications have been saved!", @"Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void export_csv_button_Click(object sender, EventArgs e)
        {
            Save();
            
            using (var writer = new StreamWriter($@"{_path}.csv"))
            {
                writer.WriteLine("Player,INF,ARC,CAV");
                
                foreach (var player in _auction.Players)
                {
                    var inf = player.Classes.Contains("inf") ? "x" : "";
                    var arc = player.Classes.Contains("arc") ? "x" : "";
                    var cav = player.Classes.Contains("cav") ? "x" : "";

                    writer.WriteLine($"{player.Name},{inf},{arc},{cav}");
                }
            }
            
            MessageBox.Show($@"Successfully exported to {_path}.csv!", @"Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}