using System.ComponentModel;

namespace AuctionApp
{
    partial class PlayerListEditor
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
            this.player_selector = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.name = new System.Windows.Forms.TextBox();
            this.inf_check = new System.Windows.Forms.CheckBox();
            this.arc_check = new System.Windows.Forms.CheckBox();
            this.cav_check = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.player_count = new System.Windows.Forms.Label();
            this.add = new System.Windows.Forms.Button();
            this.delete = new System.Windows.Forms.Button();
            this.save = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // player_selector
            // 
            this.player_selector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.player_selector.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player_selector.FormattingEnabled = true;
            this.player_selector.Location = new System.Drawing.Point(170, 150);
            this.player_selector.Margin = new System.Windows.Forms.Padding(6);
            this.player_selector.Name = "player_selector";
            this.player_selector.Size = new System.Drawing.Size(504, 50);
            this.player_selector.TabIndex = 0;
            this.player_selector.SelectedIndexChanged += new System.EventHandler(this.player_selector_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(300, 40);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(438, 65);
            this.label1.TabIndex = 1;
            this.label1.Text = "Player List Editor";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(86, 296);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(142, 42);
            this.label5.TabIndex = 58;
            this.label5.Text = "Name:";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(86, 381);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(162, 42);
            this.label2.TabIndex = 59;
            this.label2.Text = "Classes:";
            // 
            // name
            // 
            this.name.Enabled = false;
            this.name.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.name.Location = new System.Drawing.Point(240, 290);
            this.name.Margin = new System.Windows.Forms.Padding(6);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(350, 50);
            this.name.TabIndex = 60;
            this.name.TextChanged += new System.EventHandler(this.name_TextChanged);
            // 
            // inf_check
            // 
            this.inf_check.Enabled = false;
            this.inf_check.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inf_check.Location = new System.Drawing.Point(260, 369);
            this.inf_check.Margin = new System.Windows.Forms.Padding(6);
            this.inf_check.Name = "inf_check";
            this.inf_check.Size = new System.Drawing.Size(186, 73);
            this.inf_check.TabIndex = 64;
            this.inf_check.Text = "Infantry";
            this.inf_check.UseVisualStyleBackColor = true;
            this.inf_check.CheckedChanged += new System.EventHandler(this.inf_check_CheckedChanged);
            // 
            // arc_check
            // 
            this.arc_check.Enabled = false;
            this.arc_check.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.arc_check.Location = new System.Drawing.Point(500, 369);
            this.arc_check.Margin = new System.Windows.Forms.Padding(6);
            this.arc_check.Name = "arc_check";
            this.arc_check.Size = new System.Drawing.Size(184, 73);
            this.arc_check.TabIndex = 65;
            this.arc_check.Text = "Archer";
            this.arc_check.UseVisualStyleBackColor = true;
            this.arc_check.CheckedChanged += new System.EventHandler(this.arc_check_CheckedChanged);
            // 
            // cav_check
            // 
            this.cav_check.Enabled = false;
            this.cav_check.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cav_check.Location = new System.Drawing.Point(750, 369);
            this.cav_check.Margin = new System.Windows.Forms.Padding(6);
            this.cav_check.Name = "cav_check";
            this.cav_check.Size = new System.Drawing.Size(189, 73);
            this.cav_check.TabIndex = 66;
            this.cav_check.Text = "Cavalry";
            this.cav_check.UseVisualStyleBackColor = true;
            this.cav_check.CheckedChanged += new System.EventHandler(this.cav_check_CheckedChanged);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(690, 156);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(140, 42);
            this.label4.TabIndex = 69;
            this.label4.Text = "Count:";
            // 
            // player_count
            // 
            this.player_count.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player_count.Location = new System.Drawing.Point(842, 156);
            this.player_count.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.player_count.Name = "player_count";
            this.player_count.Size = new System.Drawing.Size(84, 42);
            this.player_count.TabIndex = 70;
            this.player_count.Text = "0";
            // 
            // add
            // 
            this.add.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.add.Location = new System.Drawing.Point(86, 152);
            this.add.Margin = new System.Windows.Forms.Padding(6);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(60, 60);
            this.add.TabIndex = 71;
            this.add.Text = "+";
            this.add.UseVisualStyleBackColor = true;
            this.add.Click += new System.EventHandler(this.add_Click);
            // 
            // delete
            // 
            this.delete.Enabled = false;
            this.delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.delete.Location = new System.Drawing.Point(732, 288);
            this.delete.Margin = new System.Windows.Forms.Padding(6);
            this.delete.Name = "delete";
            this.delete.Size = new System.Drawing.Size(194, 62);
            this.delete.TabIndex = 72;
            this.delete.Text = "Delete";
            this.delete.UseVisualStyleBackColor = true;
            this.delete.Click += new System.EventHandler(this.delete_Click);
            // 
            // save
            // 
            this.save.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.save.Location = new System.Drawing.Point(418, 477);
            this.save.Margin = new System.Windows.Forms.Padding(6);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(194, 62);
            this.save.TabIndex = 73;
            this.save.Text = "Save";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // PlayerListEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 585);
            this.Controls.Add(this.save);
            this.Controls.Add(this.delete);
            this.Controls.Add(this.add);
            this.Controls.Add(this.player_count);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cav_check);
            this.Controls.Add(this.arc_check);
            this.Controls.Add(this.inf_check);
            this.Controls.Add(this.name);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.player_selector);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "PlayerListEditor";
            this.Text = "PlayerListEditor";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button delete;
        private System.Windows.Forms.Button save;

        private System.Windows.Forms.Button add;

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label player_count;

        private System.Windows.Forms.CheckBox inf_check;
        private System.Windows.Forms.CheckBox arc_check;
        private System.Windows.Forms.CheckBox cav_check;

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox name;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.ComboBox player_selector;

        #endregion
    }
}