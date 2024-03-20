using System.ComponentModel;

namespace PlayerDrafter
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.open_player_list_button = new System.Windows.Forms.Button();
            this.player_list_file_path = new System.Windows.Forms.TextBox();
            this.team_list_file_path = new System.Windows.Forms.TextBox();
            this.open_team_list_button = new System.Windows.Forms.Button();
            this.begin_auction_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.load_from_cache_button = new System.Windows.Forms.Button();
            this.edit_player_list = new System.Windows.Forms.Button();
            this.new_player_list = new System.Windows.Forms.Button();
            this.edit_team_list = new System.Windows.Forms.Button();
            this.new_team_list = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // open_player_list_button
            // 
            this.open_player_list_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.open_player_list_button.Location = new System.Drawing.Point(45, 92);
            this.open_player_list_button.Name = "open_player_list_button";
            this.open_player_list_button.Size = new System.Drawing.Size(137, 29);
            this.open_player_list_button.TabIndex = 0;
            this.open_player_list_button.Text = "Open Player List";
            this.open_player_list_button.UseVisualStyleBackColor = true;
            this.open_player_list_button.Click += new System.EventHandler(this.open_player_list_button_Click);
            // 
            // player_list_file_path
            // 
            this.player_list_file_path.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player_list_file_path.Location = new System.Drawing.Point(188, 92);
            this.player_list_file_path.Name = "player_list_file_path";
            this.player_list_file_path.Size = new System.Drawing.Size(441, 29);
            this.player_list_file_path.TabIndex = 1;
            this.player_list_file_path.TabStop = false;
            // 
            // team_list_file_path
            // 
            this.team_list_file_path.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team_list_file_path.Location = new System.Drawing.Point(188, 151);
            this.team_list_file_path.Name = "team_list_file_path";
            this.team_list_file_path.Size = new System.Drawing.Size(441, 29);
            this.team_list_file_path.TabIndex = 3;
            this.team_list_file_path.TabStop = false;
            // 
            // open_team_list_button
            // 
            this.open_team_list_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.open_team_list_button.Location = new System.Drawing.Point(45, 151);
            this.open_team_list_button.Name = "open_team_list_button";
            this.open_team_list_button.Size = new System.Drawing.Size(137, 29);
            this.open_team_list_button.TabIndex = 4;
            this.open_team_list_button.Text = "Open Team List";
            this.open_team_list_button.UseVisualStyleBackColor = true;
            this.open_team_list_button.Click += new System.EventHandler(this.open_team_list_button_Click);
            // 
            // begin_auction_button
            // 
            this.begin_auction_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.begin_auction_button.Location = new System.Drawing.Point(356, 210);
            this.begin_auction_button.Name = "begin_auction_button";
            this.begin_auction_button.Size = new System.Drawing.Size(126, 30);
            this.begin_auction_button.TabIndex = 6;
            this.begin_auction_button.Text = "Begin Auction";
            this.begin_auction_button.UseVisualStyleBackColor = true;
            this.begin_auction_button.Click += new System.EventHandler(this.begin_auction_button_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(340, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 35);
            this.label1.TabIndex = 7;
            this.label1.Text = "Auction App";
            // 
            // load_from_cache_button
            // 
            this.load_from_cache_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.load_from_cache_button.Location = new System.Drawing.Point(45, 210);
            this.load_from_cache_button.Name = "load_from_cache_button";
            this.load_from_cache_button.Size = new System.Drawing.Size(148, 29);
            this.load_from_cache_button.TabIndex = 8;
            this.load_from_cache_button.Text = "Load From Cache";
            this.load_from_cache_button.UseVisualStyleBackColor = true;
            this.load_from_cache_button.Click += new System.EventHandler(this.load_from_cache_button_Click);
            // 
            // edit_player_list
            // 
            this.edit_player_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.edit_player_list.Location = new System.Drawing.Point(635, 92);
            this.edit_player_list.Name = "edit_player_list";
            this.edit_player_list.Size = new System.Drawing.Size(67, 29);
            this.edit_player_list.TabIndex = 9;
            this.edit_player_list.Text = "Edit";
            this.edit_player_list.UseVisualStyleBackColor = true;
            this.edit_player_list.Click += new System.EventHandler(this.edit_player_list_Click);
            // 
            // new_player_list
            // 
            this.new_player_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.new_player_list.Location = new System.Drawing.Point(708, 92);
            this.new_player_list.Name = "new_player_list";
            this.new_player_list.Size = new System.Drawing.Size(67, 29);
            this.new_player_list.TabIndex = 11;
            this.new_player_list.Text = "New";
            this.new_player_list.UseVisualStyleBackColor = true;
            this.new_player_list.Click += new System.EventHandler(this.new_player_list_Click);
            // 
            // edit_team_list
            // 
            this.edit_team_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.edit_team_list.Location = new System.Drawing.Point(635, 151);
            this.edit_team_list.Name = "edit_team_list";
            this.edit_team_list.Size = new System.Drawing.Size(67, 29);
            this.edit_team_list.TabIndex = 12;
            this.edit_team_list.Text = "Edit";
            this.edit_team_list.UseVisualStyleBackColor = true;
            this.edit_team_list.Click += new System.EventHandler(this.edit_team_list_Click);
            // 
            // new_team_list
            // 
            this.new_team_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.new_team_list.Location = new System.Drawing.Point(708, 151);
            this.new_team_list.Name = "new_team_list";
            this.new_team_list.Size = new System.Drawing.Size(67, 29);
            this.new_team_list.TabIndex = 13;
            this.new_team_list.Text = "New";
            this.new_team_list.UseVisualStyleBackColor = true;
            this.new_team_list.Click += new System.EventHandler(this.new_team_list_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 273);
            this.Controls.Add(this.new_team_list);
            this.Controls.Add(this.edit_team_list);
            this.Controls.Add(this.new_player_list);
            this.Controls.Add(this.edit_player_list);
            this.Controls.Add(this.load_from_cache_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.begin_auction_button);
            this.Controls.Add(this.open_team_list_button);
            this.Controls.Add(this.team_list_file_path);
            this.Controls.Add(this.player_list_file_path);
            this.Controls.Add(this.open_player_list_button);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button edit_player_list;
        private System.Windows.Forms.Button edit_team_list;
        private System.Windows.Forms.Button new_player_list;
        private System.Windows.Forms.Button new_team_list;

        private System.Windows.Forms.Button open_player_list_button;
        private System.Windows.Forms.TextBox player_list_file_path;
        private System.Windows.Forms.Button load_from_cache_button;
        private System.Windows.Forms.TextBox team_list_file_path;
        private System.Windows.Forms.Button open_team_list_button;
        private System.Windows.Forms.Button begin_auction_button;
        private System.Windows.Forms.Label label1;

        #endregion
    }
}