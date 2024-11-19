using System.ComponentModel;

namespace AuctionApp
{
    partial class SkippedPlayersPopup
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
            this.skipped_player_list = new System.Windows.Forms.ListBox();
            this.title = new System.Windows.Forms.Label();
            this.pick_button = new System.Windows.Forms.Button();
            this.refill_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // skipped_player_list
            // 
            this.skipped_player_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.skipped_player_list.FormattingEnabled = true;
            this.skipped_player_list.ItemHeight = 24;
            this.skipped_player_list.Location = new System.Drawing.Point(51, 50);
            this.skipped_player_list.Name = "skipped_player_list";
            this.skipped_player_list.Size = new System.Drawing.Size(251, 364);
            this.skipped_player_list.TabIndex = 0;
            this.skipped_player_list.SelectedIndexChanged += new System.EventHandler(this.skipped_player_list_SelectedIndexChanged);
            // 
            // title
            // 
            this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.Location = new System.Drawing.Point(71, 9);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(215, 34);
            this.title.TabIndex = 1;
            this.title.Text = "Skipped Players";
            this.title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pick_button
            // 
            this.pick_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pick_button.Location = new System.Drawing.Point(51, 437);
            this.pick_button.Name = "pick_button";
            this.pick_button.Size = new System.Drawing.Size(77, 29);
            this.pick_button.TabIndex = 14;
            this.pick_button.Text = "Pick";
            this.pick_button.UseVisualStyleBackColor = true;
            this.pick_button.Click += new System.EventHandler(this.pick_button_Click);
            // 
            // refill_button
            // 
            this.refill_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.refill_button.Location = new System.Drawing.Point(156, 437);
            this.refill_button.Name = "refill_button";
            this.refill_button.Size = new System.Drawing.Size(146, 29);
            this.refill_button.TabIndex = 15;
            this.refill_button.Text = "Send All to Queue";
            this.refill_button.UseVisualStyleBackColor = true;
            this.refill_button.Click += new System.EventHandler(this.refill_button_Click);
            // 
            // SkippedPlayersPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 495);
            this.Controls.Add(this.refill_button);
            this.Controls.Add(this.pick_button);
            this.Controls.Add(this.title);
            this.Controls.Add(this.skipped_player_list);
            this.Name = "SkippedPlayersPopup";
            this.Text = "SkippedPlayersPopup";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button pick_button;
        private System.Windows.Forms.Button refill_button;

        private System.Windows.Forms.Label title;

        private System.Windows.Forms.ListBox skipped_player_list;

        #endregion
    }
}