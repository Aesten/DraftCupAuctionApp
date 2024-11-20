using System;
using System.IO;
using System.Windows.Forms;
using AuctionApp.JsonObjects;

namespace AuctionApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        
        private void create_auction_button_Click(object sender, EventArgs e)
        {
            OpenAuctionEditor(GetCreateFileDialogResult());
        }
        
        private void edit_auction_button_Click(object sender, EventArgs e)
        {
            OpenAuctionEditor(GetFileDialogResult());
        }

        private void begin_auction_button_Click_1(object sender, EventArgs e)
        {
            OpenAuctionScreen(GenerateAuctionStateFile(GetFileDialogResult()));
        }
        
        private void resume_auction_button_Click(object sender, EventArgs e)
        {
            OpenAuctionScreen(GetFileDialogResult());
        }

        private static string GetFileDialogResult()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = @"Select Auction JSON File";
                openFileDialog.Filter = @"JSON Files (*.json)|*.json";
                openFileDialog.RestoreDirectory = true;
                return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : null;
            }
        }

        private static string GetCreateFileDialogResult()
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = @"Create Auction JSON File";
                saveFileDialog.Filter = @"JSON files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() != DialogResult.OK) return null;

                try
                {
                    File.WriteAllText(saveFileDialog.FileName, @"{}");
                    return saveFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($@"Error creating new JSON file: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }

        private void OpenAuctionEditor(string path)
        {
            var auction = Auction.Deserialize(path);
            if (auction == null)
            {
                MessageBox.Show(@"Error reading JSON file", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Hide();
            var editorForm = new EditorForm(this, auction, path);
            editorForm.Show();
        }

        private void OpenAuctionScreen(string path)
        {
            var auctionState = AuctionState.Deserialize(path);
            if (auctionState == null)
            {
                MessageBox.Show(@"Error reading JSON file", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Hide();
            var auctionForm = new AuctionForm(this, auctionState, path);
            auctionForm.Show();
        }

        private static string GenerateAuctionStateFile(string path)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var auctionStatePath = $"{Path.ChangeExtension(path, null)}.{timestamp}.state.json";
            var auction = Auction.Deserialize(path);
            AuctionState.FromAuction(auction).Serialize(auctionStatePath);
            return auctionStatePath;
        }
    }
}