using System.ComponentModel;

namespace PlayerDrafter
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
            this.label3 = new System.Windows.Forms.Label();
            this.comment = new System.Windows.Forms.TextBox();
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
            this.player_selector.Location = new System.Drawing.Point(85, 78);
            this.player_selector.Name = "player_selector";
            this.player_selector.Size = new System.Drawing.Size(254, 32);
            this.player_selector.TabIndex = 0;
            this.player_selector.SelectedIndexChanged += new System.EventHandler(this.player_selector_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(150, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 34);
            this.label1.TabIndex = 1;
            this.label1.Text = "Player List Editor";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(43, 154);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 22);
            this.label5.TabIndex = 58;
            this.label5.Text = "Name:";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(43, 198);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 22);
            this.label2.TabIndex = 59;
            this.label2.Text = "Classes:";
            // 
            // name
            // 
            this.name.Enabled = false;
            this.name.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.name.Location = new System.Drawing.Point(120, 151);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(177, 29);
            this.name.TabIndex = 60;
            this.name.TextChanged += new System.EventHandler(this.name_TextChanged);
            // 
            // inf_check
            // 
            this.inf_check.Enabled = false;
            this.inf_check.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inf_check.Location = new System.Drawing.Point(130, 192);
            this.inf_check.Name = "inf_check";
            this.inf_check.Size = new System.Drawing.Size(93, 38);
            this.inf_check.TabIndex = 64;
            this.inf_check.Text = "Infantry";
            this.inf_check.UseVisualStyleBackColor = true;
            this.inf_check.CheckedChanged += new System.EventHandler(this.inf_check_CheckedChanged);
            // 
            // arc_check
            // 
            this.arc_check.Enabled = false;
            this.arc_check.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.arc_check.Location = new System.Drawing.Point(250, 192);
            this.arc_check.Name = "arc_check";
            this.arc_check.Size = new System.Drawing.Size(92, 38);
            this.arc_check.TabIndex = 65;
            this.arc_check.Text = "Archer";
            this.arc_check.UseVisualStyleBackColor = true;
            this.arc_check.CheckedChanged += new System.EventHandler(this.arc_check_CheckedChanged);
            // 
            // cav_check
            // 
            this.cav_check.Enabled = false;
            this.cav_check.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cav_check.Location = new System.Drawing.Point(375, 192);
            this.cav_check.Name = "cav_check";
            this.cav_check.Size = new System.Drawing.Size(90, 38);
            this.cav_check.TabIndex = 66;
            this.cav_check.Text = "Cavalry";
            this.cav_check.UseVisualStyleBackColor = true;
            this.cav_check.CheckedChanged += new System.EventHandler(this.cav_check_CheckedChanged);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(43, 239);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 22);
            this.label3.TabIndex = 67;
            this.label3.Text = "Comment:";
            // 
            // comment
            // 
            this.comment.Enabled = false;
            this.comment.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comment.Location = new System.Drawing.Point(150, 236);
            this.comment.Name = "comment";
            this.comment.Size = new System.Drawing.Size(313, 29);
            this.comment.TabIndex = 68;
            this.comment.TextChanged += new System.EventHandler(this.comment_TextChanged);
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
            // player_count
            // 
            this.player_count.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player_count.Location = new System.Drawing.Point(421, 81);
            this.player_count.Name = "player_count";
            this.player_count.Size = new System.Drawing.Size(42, 22);
            this.player_count.TabIndex = 70;
            this.player_count.Text = "0";
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
            this.save.Location = new System.Drawing.Point(213, 298);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(97, 32);
            this.save.TabIndex = 73;
            this.save.Text = "Save";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // PlayerListEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 354);
            this.Controls.Add(this.save);
            this.Controls.Add(this.delete);
            this.Controls.Add(this.add);
            this.Controls.Add(this.player_count);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comment);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cav_check);
            this.Controls.Add(this.arc_check);
            this.Controls.Add(this.inf_check);
            this.Controls.Add(this.name);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.player_selector);
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

        private System.Windows.Forms.TextBox comment;

        private System.Windows.Forms.CheckBox inf_check;
        private System.Windows.Forms.CheckBox arc_check;
        private System.Windows.Forms.CheckBox cav_check;
        private System.Windows.Forms.Label label3;

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox name;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.ComboBox player_selector;

        #endregion
    }
}