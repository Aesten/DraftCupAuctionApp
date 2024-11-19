using System.ComponentModel;

namespace AuctionApp
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
            this.create_auction_button = new System.Windows.Forms.Button();
            this.edit_auction_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.begin_auction_button = new System.Windows.Forms.Button();
            this.resume_auction_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // create_auction_button
            // 
            this.create_auction_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.create_auction_button.Location = new System.Drawing.Point(71, 83);
            this.create_auction_button.Name = "create_auction_button";
            this.create_auction_button.Size = new System.Drawing.Size(137, 29);
            this.create_auction_button.TabIndex = 0;
            this.create_auction_button.Text = "Create Auction";
            this.create_auction_button.UseVisualStyleBackColor = true;
            this.create_auction_button.Click += new System.EventHandler(this.create_auction_button_Click);
            // 
            // edit_auction_button
            // 
            this.edit_auction_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.edit_auction_button.Location = new System.Drawing.Point(71, 127);
            this.edit_auction_button.Name = "edit_auction_button";
            this.edit_auction_button.Size = new System.Drawing.Size(137, 29);
            this.edit_auction_button.TabIndex = 4;
            this.edit_auction_button.Text = "Edit Auction";
            this.edit_auction_button.UseVisualStyleBackColor = true;
            this.edit_auction_button.Click += new System.EventHandler(this.edit_auction_button_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(138, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 35);
            this.label1.TabIndex = 7;
            this.label1.Text = "Auction App";
            // 
            // begin_auction_button
            // 
            this.begin_auction_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.begin_auction_button.Location = new System.Drawing.Point(233, 83);
            this.begin_auction_button.Name = "begin_auction_button";
            this.begin_auction_button.Size = new System.Drawing.Size(137, 29);
            this.begin_auction_button.TabIndex = 8;
            this.begin_auction_button.Text = "Begin Auction";
            this.begin_auction_button.UseVisualStyleBackColor = true;
            this.begin_auction_button.Click += new System.EventHandler(this.begin_auction_button_Click_1);
            // 
            // resume_auction_button
            // 
            this.resume_auction_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resume_auction_button.Location = new System.Drawing.Point(233, 127);
            this.resume_auction_button.Name = "resume_auction_button";
            this.resume_auction_button.Size = new System.Drawing.Size(137, 29);
            this.resume_auction_button.TabIndex = 9;
            this.resume_auction_button.Text = "Resume Auction";
            this.resume_auction_button.UseVisualStyleBackColor = true;
            this.resume_auction_button.Click += new System.EventHandler(this.resume_auction_button_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 189);
            this.Controls.Add(this.resume_auction_button);
            this.Controls.Add(this.begin_auction_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.edit_auction_button);
            this.Controls.Add(this.create_auction_button);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button begin_auction_button;
        private System.Windows.Forms.Button resume_auction_button;

        private System.Windows.Forms.Button create_auction_button;
        private System.Windows.Forms.Button edit_auction_button;
        private System.Windows.Forms.Label label1;

        #endregion
    }
}