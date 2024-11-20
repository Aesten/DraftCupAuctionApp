using System.ComponentModel;
using System.Drawing;

namespace AuctionApp
{
    partial class EditorForm
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
            this.player_grid = new System.Windows.Forms.DataGridView();
            this.budget1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.captain1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.budget2 = new System.Windows.Forms.NumericUpDown();
            this.captain2 = new System.Windows.Forms.TextBox();
            this.budget3 = new System.Windows.Forms.NumericUpDown();
            this.captain3 = new System.Windows.Forms.TextBox();
            this.budget4 = new System.Windows.Forms.NumericUpDown();
            this.captain4 = new System.Windows.Forms.TextBox();
            this.budget5 = new System.Windows.Forms.NumericUpDown();
            this.captain5 = new System.Windows.Forms.TextBox();
            this.budget6 = new System.Windows.Forms.NumericUpDown();
            this.captain6 = new System.Windows.Forms.TextBox();
            this.budget7 = new System.Windows.Forms.NumericUpDown();
            this.captain7 = new System.Windows.Forms.TextBox();
            this.budget8 = new System.Windows.Forms.NumericUpDown();
            this.captain8 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.title_box = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.team_size = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.export_csv_button = new System.Windows.Forms.Button();
            this.save_button = new System.Windows.Forms.Button();
            this.nickname = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.class_inf = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.class_arc = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.class_cav = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.player_grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.team_size)).BeginInit();
            this.SuspendLayout();
            // 
            // player_grid
            // 
            this.player_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.player_grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.nickname, this.class_inf, this.class_arc, this.class_cav });
            this.player_grid.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.player_grid.Location = new System.Drawing.Point(36, 156);
            this.player_grid.Name = "player_grid";
            this.player_grid.RowHeadersVisible = false;
            this.player_grid.Size = new System.Drawing.Size(502, 626);
            this.player_grid.TabIndex = 0;
            // 
            // budget1
            // 
            this.budget1.DecimalPlaces = 1;
            this.budget1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.budget1.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.budget1.Location = new System.Drawing.Point(923, 230);
            this.budget1.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this.budget1.Name = "budget1";
            this.budget1.Size = new System.Drawing.Size(85, 29);
            this.budget1.TabIndex = 79;
            this.budget1.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(913, 175);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 28);
            this.label2.TabIndex = 78;
            this.label2.Text = "Budget";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // captain1
            // 
            this.captain1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captain1.Location = new System.Drawing.Point(621, 230);
            this.captain1.Name = "captain1";
            this.captain1.Size = new System.Drawing.Size(268, 29);
            this.captain1.TabIndex = 77;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(697, 175);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(124, 22);
            this.label5.TabIndex = 76;
            this.label5.Text = "Nickname";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(183, 38);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(734, 53);
            this.label7.TabIndex = 80;
            this.label7.Text = "Draftcup Auction Editor\r\n";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(36, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(507, 34);
            this.label1.TabIndex = 81;
            this.label1.Text = "Players";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(686, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(253, 34);
            this.label3.TabIndex = 82;
            this.label3.Text = "Captains";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // budget2
            // 
            this.budget2.DecimalPlaces = 1;
            this.budget2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.budget2.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.budget2.Location = new System.Drawing.Point(923, 280);
            this.budget2.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this.budget2.Name = "budget2";
            this.budget2.Size = new System.Drawing.Size(85, 29);
            this.budget2.TabIndex = 84;
            this.budget2.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // captain2
            // 
            this.captain2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captain2.Location = new System.Drawing.Point(621, 280);
            this.captain2.Name = "captain2";
            this.captain2.Size = new System.Drawing.Size(268, 29);
            this.captain2.TabIndex = 83;
            // 
            // budget3
            // 
            this.budget3.DecimalPlaces = 1;
            this.budget3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.budget3.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.budget3.Location = new System.Drawing.Point(923, 330);
            this.budget3.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this.budget3.Name = "budget3";
            this.budget3.Size = new System.Drawing.Size(85, 29);
            this.budget3.TabIndex = 86;
            this.budget3.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // captain3
            // 
            this.captain3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captain3.Location = new System.Drawing.Point(621, 330);
            this.captain3.Name = "captain3";
            this.captain3.Size = new System.Drawing.Size(268, 29);
            this.captain3.TabIndex = 85;
            // 
            // budget4
            // 
            this.budget4.DecimalPlaces = 1;
            this.budget4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.budget4.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.budget4.Location = new System.Drawing.Point(923, 380);
            this.budget4.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this.budget4.Name = "budget4";
            this.budget4.Size = new System.Drawing.Size(85, 29);
            this.budget4.TabIndex = 88;
            this.budget4.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // captain4
            // 
            this.captain4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captain4.Location = new System.Drawing.Point(621, 380);
            this.captain4.Name = "captain4";
            this.captain4.Size = new System.Drawing.Size(268, 29);
            this.captain4.TabIndex = 87;
            // 
            // budget5
            // 
            this.budget5.DecimalPlaces = 1;
            this.budget5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.budget5.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.budget5.Location = new System.Drawing.Point(923, 430);
            this.budget5.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this.budget5.Name = "budget5";
            this.budget5.Size = new System.Drawing.Size(85, 29);
            this.budget5.TabIndex = 90;
            this.budget5.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // captain5
            // 
            this.captain5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captain5.Location = new System.Drawing.Point(621, 430);
            this.captain5.Name = "captain5";
            this.captain5.Size = new System.Drawing.Size(268, 29);
            this.captain5.TabIndex = 89;
            // 
            // budget6
            // 
            this.budget6.DecimalPlaces = 1;
            this.budget6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.budget6.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.budget6.Location = new System.Drawing.Point(923, 480);
            this.budget6.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this.budget6.Name = "budget6";
            this.budget6.Size = new System.Drawing.Size(85, 29);
            this.budget6.TabIndex = 92;
            this.budget6.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // captain6
            // 
            this.captain6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captain6.Location = new System.Drawing.Point(621, 480);
            this.captain6.Name = "captain6";
            this.captain6.Size = new System.Drawing.Size(268, 29);
            this.captain6.TabIndex = 91;
            // 
            // budget7
            // 
            this.budget7.DecimalPlaces = 1;
            this.budget7.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.budget7.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.budget7.Location = new System.Drawing.Point(923, 530);
            this.budget7.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this.budget7.Name = "budget7";
            this.budget7.Size = new System.Drawing.Size(85, 29);
            this.budget7.TabIndex = 94;
            this.budget7.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // captain7
            // 
            this.captain7.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captain7.Location = new System.Drawing.Point(621, 530);
            this.captain7.Name = "captain7";
            this.captain7.Size = new System.Drawing.Size(268, 29);
            this.captain7.TabIndex = 93;
            // 
            // budget8
            // 
            this.budget8.DecimalPlaces = 1;
            this.budget8.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.budget8.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.budget8.Location = new System.Drawing.Point(923, 580);
            this.budget8.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this.budget8.Name = "budget8";
            this.budget8.Size = new System.Drawing.Size(85, 29);
            this.budget8.TabIndex = 96;
            this.budget8.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // captain8
            // 
            this.captain8.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captain8.Location = new System.Drawing.Point(621, 580);
            this.captain8.Name = "captain8";
            this.captain8.Size = new System.Drawing.Size(268, 29);
            this.captain8.TabIndex = 95;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(686, 653);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(253, 34);
            this.label4.TabIndex = 97;
            this.label4.Text = "Information";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // title_box
            // 
            this.title_box.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title_box.Location = new System.Drawing.Point(697, 711);
            this.title_box.Name = "title_box";
            this.title_box.Size = new System.Drawing.Size(311, 29);
            this.title_box.TabIndex = 98;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(575, 714);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(124, 22);
            this.label6.TabIndex = 99;
            this.label6.Text = "Title:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(686, 760);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(124, 22);
            this.label8.TabIndex = 100;
            this.label8.Text = "Team Size:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // team_size
            // 
            this.team_size.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team_size.Location = new System.Drawing.Point(847, 760);
            this.team_size.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.team_size.Name = "team_size";
            this.team_size.Size = new System.Drawing.Size(85, 29);
            this.team_size.TabIndex = 101;
            this.team_size.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(676, 795);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(156, 18);
            this.label9.TabIndex = 102;
            this.label9.Text = "*excluding the captain*\r\n";
            // 
            // export_csv_button
            // 
            this.export_csv_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.export_csv_button.Location = new System.Drawing.Point(213, 795);
            this.export_csv_button.Name = "export_csv_button";
            this.export_csv_button.Size = new System.Drawing.Size(142, 29);
            this.export_csv_button.TabIndex = 103;
            this.export_csv_button.Text = "Export in CSV";
            this.export_csv_button.UseVisualStyleBackColor = true;
            this.export_csv_button.Click += new System.EventHandler(this.export_csv_button_Click);
            // 
            // save_button
            // 
            this.save_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.save_button.Location = new System.Drawing.Point(12, 12);
            this.save_button.Name = "save_button";
            this.save_button.Size = new System.Drawing.Size(94, 29);
            this.save_button.TabIndex = 104;
            this.save_button.Text = "Save";
            this.save_button.UseVisualStyleBackColor = true;
            this.save_button.Click += new System.EventHandler(this.save_button_Click);
            // 
            // nickname
            // 
            this.nickname.HeaderText = "Nickname";
            this.nickname.Name = "nickname";
            this.nickname.Width = 286;
            // 
            // class_inf
            // 
            this.class_inf.HeaderText = "Infantry";
            this.class_inf.Name = "class_inf";
            this.class_inf.Width = 70;
            // 
            // class_arc
            // 
            this.class_arc.HeaderText = "Archer";
            this.class_arc.Name = "class_arc";
            this.class_arc.Width = 70;
            // 
            // class_cav
            // 
            this.class_cav.HeaderText = "Cavalry";
            this.class_cav.Name = "class_cav";
            this.class_cav.Width = 70;
            // 
            // EditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1064, 859);
            this.Controls.Add(this.save_button);
            this.Controls.Add(this.export_csv_button);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.team_size);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.title_box);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.budget8);
            this.Controls.Add(this.captain8);
            this.Controls.Add(this.budget7);
            this.Controls.Add(this.captain7);
            this.Controls.Add(this.budget6);
            this.Controls.Add(this.captain6);
            this.Controls.Add(this.budget5);
            this.Controls.Add(this.captain5);
            this.Controls.Add(this.budget4);
            this.Controls.Add(this.captain4);
            this.Controls.Add(this.budget3);
            this.Controls.Add(this.captain3);
            this.Controls.Add(this.budget2);
            this.Controls.Add(this.captain2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.budget1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.captain1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.player_grid);
            this.Name = "EditorForm";
            this.Text = "EditorForm";
            ((System.ComponentModel.ISupportInitialize)(this.player_grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budget8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.team_size)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button save_button;

        private System.Windows.Forms.Button export_csv_button;

        private System.Windows.Forms.DataGridViewTextBoxColumn nickname;
        private System.Windows.Forms.DataGridViewCheckBoxColumn class_inf;
        private System.Windows.Forms.DataGridViewCheckBoxColumn class_arc;
        private System.Windows.Forms.DataGridViewCheckBoxColumn class_cav;

        private System.Windows.Forms.Label label9;

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox title_box;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown team_size;

        private System.Windows.Forms.NumericUpDown budget2;
        private System.Windows.Forms.TextBox captain2;
        private System.Windows.Forms.NumericUpDown budget3;
        private System.Windows.Forms.TextBox captain3;
        private System.Windows.Forms.NumericUpDown budget4;
        private System.Windows.Forms.TextBox captain4;
        private System.Windows.Forms.NumericUpDown budget5;
        private System.Windows.Forms.TextBox captain5;
        private System.Windows.Forms.NumericUpDown budget6;
        private System.Windows.Forms.TextBox captain6;
        private System.Windows.Forms.NumericUpDown budget7;
        private System.Windows.Forms.TextBox captain7;
        private System.Windows.Forms.NumericUpDown budget8;
        private System.Windows.Forms.TextBox captain8;

        private System.Windows.Forms.Label label3;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.Label label7;

        private System.Windows.Forms.NumericUpDown budget1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox captain1;
        private System.Windows.Forms.Label label5;

        private System.Windows.Forms.DataGridView player_grid;

        #endregion
    }
}