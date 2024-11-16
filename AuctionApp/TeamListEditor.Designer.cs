using System.ComponentModel;

namespace AuctionApp
{
    partial class TeamListEditor
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
            this.team_selector = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.name = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.team_count = new System.Windows.Forms.Label();
            this.add = new System.Windows.Forms.Button();
            this.delete = new System.Windows.Forms.Button();
            this.save = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.remaining_budget = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.used_budget = new System.Windows.Forms.Label();
            this.cost_list = new System.Windows.Forms.ListBox();
            this.player_list = new System.Windows.Forms.ListBox();
            this.add_player_button = new System.Windows.Forms.Button();
            this.remove_player_button = new System.Windows.Forms.Button();
            this.edit_player_button = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.remaining_budget)).BeginInit();
            this.SuspendLayout();
            // 
            // team_selector
            // 
            this.team_selector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.team_selector.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team_selector.FormattingEnabled = true;
            this.team_selector.Location = new System.Drawing.Point(85, 78);
            this.team_selector.Name = "team_selector";
            this.team_selector.Size = new System.Drawing.Size(254, 32);
            this.team_selector.TabIndex = 0;
            this.team_selector.SelectedIndexChanged += new System.EventHandler(this.team_selector_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(150, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 34);
            this.label1.TabIndex = 1;
            this.label1.Text = "Team List Editor";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(43, 154);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 22);
            this.label5.TabIndex = 58;
            this.label5.Text = "Captain:";
            // 
            // name
            // 
            this.name.Enabled = false;
            this.name.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.name.Location = new System.Drawing.Point(130, 151);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(180, 29);
            this.name.TabIndex = 60;
            this.name.TextChanged += new System.EventHandler(this.name_TextChanged);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(345, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 22);
            this.label4.TabIndex = 69;
            this.label4.Text = "Count:";
            // 
            // team_count
            // 
            this.team_count.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team_count.Location = new System.Drawing.Point(421, 81);
            this.team_count.Name = "team_count";
            this.team_count.Size = new System.Drawing.Size(42, 22);
            this.team_count.TabIndex = 70;
            this.team_count.Text = "0";
            // 
            // add
            // 
            this.add.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.add.Location = new System.Drawing.Point(43, 79);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(30, 31);
            this.add.TabIndex = 71;
            this.add.Text = "+";
            this.add.UseVisualStyleBackColor = true;
            this.add.Click += new System.EventHandler(this.add_Click);
            // 
            // delete
            // 
            this.delete.Enabled = false;
            this.delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.delete.Location = new System.Drawing.Point(366, 150);
            this.delete.Name = "delete";
            this.delete.Size = new System.Drawing.Size(97, 32);
            this.delete.TabIndex = 72;
            this.delete.Text = "Delete";
            this.delete.UseVisualStyleBackColor = true;
            this.delete.Click += new System.EventHandler(this.delete_Click);
            // 
            // save
            // 
            this.save.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.save.Location = new System.Drawing.Point(213, 509);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(97, 32);
            this.save.TabIndex = 73;
            this.save.Text = "Save";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(43, 201);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 28);
            this.label2.TabIndex = 74;
            this.label2.Text = "Remaining Budget:";
            // 
            // remaining_budget
            // 
            this.remaining_budget.DecimalPlaces = 1;
            this.remaining_budget.Enabled = false;
            this.remaining_budget.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.remaining_budget.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.remaining_budget.Location = new System.Drawing.Point(213, 203);
            this.remaining_budget.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this.remaining_budget.Name = "remaining_budget";
            this.remaining_budget.Size = new System.Drawing.Size(85, 29);
            this.remaining_budget.TabIndex = 75;
            this.remaining_budget.Value = new decimal(new int[] { 10, 0, 0, 0 });
            this.remaining_budget.ValueChanged += new System.EventHandler(this.remaining_budget_ValueChanged);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(330, 203);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 22);
            this.label3.TabIndex = 76;
            this.label3.Text = "Used:";
            // 
            // used_budget
            // 
            this.used_budget.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.used_budget.Location = new System.Drawing.Point(396, 203);
            this.used_budget.Name = "used_budget";
            this.used_budget.Size = new System.Drawing.Size(67, 22);
            this.used_budget.TabIndex = 77;
            this.used_budget.Text = "0.0";
            // 
            // cost_list
            // 
            this.cost_list.Enabled = false;
            this.cost_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cost_list.ItemHeight = 24;
            this.cost_list.Location = new System.Drawing.Point(366, 254);
            this.cost_list.Name = "cost_list";
            this.cost_list.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.cost_list.Size = new System.Drawing.Size(97, 220);
            this.cost_list.TabIndex = 79;
            this.cost_list.TabStop = false;
            // 
            // player_list
            // 
            this.player_list.Enabled = false;
            this.player_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player_list.ItemHeight = 24;
            this.player_list.Location = new System.Drawing.Point(85, 254);
            this.player_list.Name = "player_list";
            this.player_list.Size = new System.Drawing.Size(254, 220);
            this.player_list.TabIndex = 78;
            this.player_list.TabStop = false;
            this.player_list.SelectedIndexChanged += new System.EventHandler(this.player_list_SelectedIndexChanged);
            // 
            // add_player_button
            // 
            this.add_player_button.Enabled = false;
            this.add_player_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.add_player_button.Location = new System.Drawing.Point(43, 254);
            this.add_player_button.Name = "add_player_button";
            this.add_player_button.Size = new System.Drawing.Size(30, 56);
            this.add_player_button.TabIndex = 80;
            this.add_player_button.Text = "+";
            this.add_player_button.UseVisualStyleBackColor = true;
            this.add_player_button.Click += new System.EventHandler(this.add_player_button_Click);
            // 
            // remove_player_button
            // 
            this.remove_player_button.Enabled = false;
            this.remove_player_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.remove_player_button.Location = new System.Drawing.Point(43, 418);
            this.remove_player_button.Name = "remove_player_button";
            this.remove_player_button.Size = new System.Drawing.Size(30, 56);
            this.remove_player_button.TabIndex = 81;
            this.remove_player_button.Text = "-";
            this.remove_player_button.UseVisualStyleBackColor = true;
            this.remove_player_button.Click += new System.EventHandler(this.remove_player_button_Click);
            // 
            // edit_player_button
            // 
            this.edit_player_button.Enabled = false;
            this.edit_player_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.edit_player_button.Location = new System.Drawing.Point(43, 336);
            this.edit_player_button.Name = "edit_player_button";
            this.edit_player_button.Size = new System.Drawing.Size(30, 56);
            this.edit_player_button.TabIndex = 82;
            this.edit_player_button.Text = "=";
            this.edit_player_button.UseVisualStyleBackColor = true;
            this.edit_player_button.Click += new System.EventHandler(this.edit_player_button_Click);
            // 
            // TeamListEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 568);
            this.Controls.Add(this.edit_player_button);
            this.Controls.Add(this.remove_player_button);
            this.Controls.Add(this.add_player_button);
            this.Controls.Add(this.cost_list);
            this.Controls.Add(this.player_list);
            this.Controls.Add(this.used_budget);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.remaining_budget);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.save);
            this.Controls.Add(this.delete);
            this.Controls.Add(this.add);
            this.Controls.Add(this.team_count);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.name);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.team_selector);
            this.Name = "TeamListEditor";
            this.Text = "PlayerListEditor";
            ((System.ComponentModel.ISupportInitialize)(this.remaining_budget)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button edit_player_button;

        private System.Windows.Forms.Button add_player_button;
        private System.Windows.Forms.Button remove_player_button;

        private System.Windows.Forms.ListBox cost_list;
        private System.Windows.Forms.ListBox player_list;

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label used_budget;

        private System.Windows.Forms.NumericUpDown remaining_budget;

        private System.Windows.Forms.Label label2;

        private System.Windows.Forms.Button delete;
        private System.Windows.Forms.Button save;

        private System.Windows.Forms.Button add;

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label team_count;

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox name;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.ComboBox team_selector;

        #endregion
    }
}