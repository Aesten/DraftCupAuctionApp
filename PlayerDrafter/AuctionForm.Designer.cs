namespace PlayerDrafter
{
    partial class AuctionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.label1 = new System.Windows.Forms.Label();
            this.player = new System.Windows.Forms.TextBox();
            this.icon1 = new System.Windows.Forms.PictureBox();
            this.icon3 = new System.Windows.Forms.PictureBox();
            this.icon4 = new System.Windows.Forms.PictureBox();
            this.comment = new System.Windows.Forms.TextBox();
            this.icon2 = new System.Windows.Forms.PictureBox();
            this.icon5 = new System.Windows.Forms.PictureBox();
            this.queue1 = new System.Windows.Forms.TextBox();
            this.queue2 = new System.Windows.Forms.TextBox();
            this.queue3 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.auctionedButton = new System.Windows.Forms.Button();
            this.skipButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.counterLabel = new System.Windows.Forms.Label();
            this.team1_players = new System.Windows.Forms.ListBox();
            this.team1_captain = new System.Windows.Forms.TextBox();
            this.team1_money = new System.Windows.Forms.TextBox();
            this.team1_expenses = new System.Windows.Forms.ListBox();
            this.team2_expenses = new System.Windows.Forms.ListBox();
            this.team2_money = new System.Windows.Forms.TextBox();
            this.team2_captain = new System.Windows.Forms.TextBox();
            this.team2_players = new System.Windows.Forms.ListBox();
            this.team3_expenses = new System.Windows.Forms.ListBox();
            this.team3_money = new System.Windows.Forms.TextBox();
            this.team3_captain = new System.Windows.Forms.TextBox();
            this.team3_players = new System.Windows.Forms.ListBox();
            this.team6_expenses = new System.Windows.Forms.ListBox();
            this.team6_money = new System.Windows.Forms.TextBox();
            this.team6_captain = new System.Windows.Forms.TextBox();
            this.team6_players = new System.Windows.Forms.ListBox();
            this.team5_expenses = new System.Windows.Forms.ListBox();
            this.team5_money = new System.Windows.Forms.TextBox();
            this.team5_captain = new System.Windows.Forms.TextBox();
            this.team5_players = new System.Windows.Forms.ListBox();
            this.team4_expenses = new System.Windows.Forms.ListBox();
            this.team4_money = new System.Windows.Forms.TextBox();
            this.team4_captain = new System.Windows.Forms.TextBox();
            this.team4_players = new System.Windows.Forms.ListBox();
            this.team_selector = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.price = new System.Windows.Forms.NumericUpDown();
            this.undo_button = new System.Windows.Forms.Button();
            this.half_budget_display = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.icon1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.icon3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.icon4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.icon2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.icon5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.price)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(279, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 34);
            this.label1.TabIndex = 0;
            this.label1.Text = "Player";
            // 
            // player
            // 
            this.player.Cursor = System.Windows.Forms.Cursors.Default;
            this.player.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.player.Location = new System.Drawing.Point(167, 67);
            this.player.Name = "player";
            this.player.ReadOnly = true;
            this.player.Size = new System.Drawing.Size(319, 38);
            this.player.TabIndex = 1;
            this.player.TabStop = false;
            this.player.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // icon1
            // 
            this.icon1.Location = new System.Drawing.Point(216, 129);
            this.icon1.Name = "icon1";
            this.icon1.Size = new System.Drawing.Size(38, 38);
            this.icon1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.icon1.TabIndex = 3;
            this.icon1.TabStop = false;
            // 
            // icon3
            // 
            this.icon3.Location = new System.Drawing.Point(304, 129);
            this.icon3.Name = "icon3";
            this.icon3.Size = new System.Drawing.Size(38, 38);
            this.icon3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.icon3.TabIndex = 4;
            this.icon3.TabStop = false;
            // 
            // icon4
            // 
            this.icon4.Location = new System.Drawing.Point(348, 129);
            this.icon4.Name = "icon4";
            this.icon4.Size = new System.Drawing.Size(38, 38);
            this.icon4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.icon4.TabIndex = 5;
            this.icon4.TabStop = false;
            // 
            // comment
            // 
            this.comment.Cursor = System.Windows.Forms.Cursors.Default;
            this.comment.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comment.Location = new System.Drawing.Point(80, 191);
            this.comment.Name = "comment";
            this.comment.ReadOnly = true;
            this.comment.Size = new System.Drawing.Size(486, 29);
            this.comment.TabIndex = 6;
            this.comment.TabStop = false;
            this.comment.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // icon2
            // 
            this.icon2.Location = new System.Drawing.Point(260, 129);
            this.icon2.Name = "icon2";
            this.icon2.Size = new System.Drawing.Size(38, 38);
            this.icon2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.icon2.TabIndex = 7;
            this.icon2.TabStop = false;
            // 
            // icon5
            // 
            this.icon5.Location = new System.Drawing.Point(392, 129);
            this.icon5.Name = "icon5";
            this.icon5.Size = new System.Drawing.Size(38, 38);
            this.icon5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.icon5.TabIndex = 8;
            this.icon5.TabStop = false;
            // 
            // queue1
            // 
            this.queue1.Cursor = System.Windows.Forms.Cursors.Default;
            this.queue1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.queue1.Location = new System.Drawing.Point(666, 95);
            this.queue1.Name = "queue1";
            this.queue1.ReadOnly = true;
            this.queue1.Size = new System.Drawing.Size(170, 29);
            this.queue1.TabIndex = 9;
            this.queue1.TabStop = false;
            // 
            // queue2
            // 
            this.queue2.Cursor = System.Windows.Forms.Cursors.Default;
            this.queue2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.queue2.Location = new System.Drawing.Point(666, 130);
            this.queue2.Name = "queue2";
            this.queue2.ReadOnly = true;
            this.queue2.Size = new System.Drawing.Size(170, 29);
            this.queue2.TabIndex = 10;
            this.queue2.TabStop = false;
            // 
            // queue3
            // 
            this.queue3.Cursor = System.Windows.Forms.Cursors.Default;
            this.queue3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.queue3.Location = new System.Drawing.Point(666, 165);
            this.queue3.Name = "queue3";
            this.queue3.ReadOnly = true;
            this.queue3.Size = new System.Drawing.Size(170, 29);
            this.queue3.TabIndex = 11;
            this.queue3.TabStop = false;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(682, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 34);
            this.label2.TabIndex = 12;
            this.label2.Text = "Upcoming";
            // 
            // auctionedButton
            // 
            this.auctionedButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.auctionedButton.Location = new System.Drawing.Point(241, 279);
            this.auctionedButton.Name = "auctionedButton";
            this.auctionedButton.Size = new System.Drawing.Size(96, 29);
            this.auctionedButton.TabIndex = 13;
            this.auctionedButton.Text = "Auctioned";
            this.auctionedButton.UseVisualStyleBackColor = true;
            this.auctionedButton.Click += new System.EventHandler(this.auctionedButton_Click);
            // 
            // skipButton
            // 
            this.skipButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.skipButton.Location = new System.Drawing.Point(462, 256);
            this.skipButton.Name = "skipButton";
            this.skipButton.Size = new System.Drawing.Size(96, 29);
            this.skipButton.TabIndex = 14;
            this.skipButton.Text = "Skip";
            this.skipButton.UseVisualStyleBackColor = true;
            this.skipButton.Click += new System.EventHandler(this.skipButton_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(702, 202);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 18);
            this.label3.TabIndex = 15;
            this.label3.Text = "Remaining:";
            // 
            // counterLabel
            // 
            this.counterLabel.Location = new System.Drawing.Point(763, 202);
            this.counterLabel.Name = "counterLabel";
            this.counterLabel.Size = new System.Drawing.Size(35, 18);
            this.counterLabel.TabIndex = 16;
            this.counterLabel.Text = "this.";
            // 
            // team1_players
            // 
            this.team1_players.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team1_players.ItemHeight = 24;
            this.team1_players.Location = new System.Drawing.Point(36, 377);
            this.team1_players.Name = "team1_players";
            this.team1_players.Size = new System.Drawing.Size(199, 220);
            this.team1_players.TabIndex = 17;
            this.team1_players.TabStop = false;
            // 
            // team1_captain
            // 
            this.team1_captain.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team1_captain.Location = new System.Drawing.Point(36, 342);
            this.team1_captain.Name = "team1_captain";
            this.team1_captain.ReadOnly = true;
            this.team1_captain.Size = new System.Drawing.Size(199, 29);
            this.team1_captain.TabIndex = 19;
            this.team1_captain.TabStop = false;
            // 
            // team1_money
            // 
            this.team1_money.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team1_money.Location = new System.Drawing.Point(241, 342);
            this.team1_money.Name = "team1_money";
            this.team1_money.ReadOnly = true;
            this.team1_money.Size = new System.Drawing.Size(54, 29);
            this.team1_money.TabIndex = 20;
            this.team1_money.TabStop = false;
            // 
            // team1_expenses
            // 
            this.team1_expenses.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team1_expenses.ItemHeight = 24;
            this.team1_expenses.Location = new System.Drawing.Point(241, 377);
            this.team1_expenses.Name = "team1_expenses";
            this.team1_expenses.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.team1_expenses.Size = new System.Drawing.Size(54, 220);
            this.team1_expenses.TabIndex = 21;
            this.team1_expenses.TabStop = false;
            // 
            // team2_expenses
            // 
            this.team2_expenses.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team2_expenses.ItemHeight = 24;
            this.team2_expenses.Location = new System.Drawing.Point(551, 377);
            this.team2_expenses.Name = "team2_expenses";
            this.team2_expenses.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.team2_expenses.Size = new System.Drawing.Size(54, 220);
            this.team2_expenses.TabIndex = 37;
            this.team2_expenses.TabStop = false;
            // 
            // team2_money
            // 
            this.team2_money.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team2_money.Location = new System.Drawing.Point(551, 342);
            this.team2_money.Name = "team2_money";
            this.team2_money.ReadOnly = true;
            this.team2_money.Size = new System.Drawing.Size(54, 29);
            this.team2_money.TabIndex = 36;
            this.team2_money.TabStop = false;
            // 
            // team2_captain
            // 
            this.team2_captain.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team2_captain.Location = new System.Drawing.Point(346, 342);
            this.team2_captain.Name = "team2_captain";
            this.team2_captain.ReadOnly = true;
            this.team2_captain.Size = new System.Drawing.Size(199, 29);
            this.team2_captain.TabIndex = 35;
            this.team2_captain.TabStop = false;
            // 
            // team2_players
            // 
            this.team2_players.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team2_players.ItemHeight = 24;
            this.team2_players.Location = new System.Drawing.Point(346, 377);
            this.team2_players.Name = "team2_players";
            this.team2_players.Size = new System.Drawing.Size(199, 220);
            this.team2_players.TabIndex = 34;
            this.team2_players.TabStop = false;
            // 
            // team3_expenses
            // 
            this.team3_expenses.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team3_expenses.ItemHeight = 24;
            this.team3_expenses.Location = new System.Drawing.Point(856, 377);
            this.team3_expenses.Name = "team3_expenses";
            this.team3_expenses.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.team3_expenses.Size = new System.Drawing.Size(54, 220);
            this.team3_expenses.TabIndex = 41;
            this.team3_expenses.TabStop = false;
            // 
            // team3_money
            // 
            this.team3_money.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team3_money.Location = new System.Drawing.Point(856, 342);
            this.team3_money.Name = "team3_money";
            this.team3_money.ReadOnly = true;
            this.team3_money.Size = new System.Drawing.Size(54, 29);
            this.team3_money.TabIndex = 40;
            this.team3_money.TabStop = false;
            // 
            // team3_captain
            // 
            this.team3_captain.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team3_captain.Location = new System.Drawing.Point(651, 342);
            this.team3_captain.Name = "team3_captain";
            this.team3_captain.ReadOnly = true;
            this.team3_captain.Size = new System.Drawing.Size(199, 29);
            this.team3_captain.TabIndex = 39;
            this.team3_captain.TabStop = false;
            // 
            // team3_players
            // 
            this.team3_players.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team3_players.ItemHeight = 24;
            this.team3_players.Location = new System.Drawing.Point(651, 377);
            this.team3_players.Name = "team3_players";
            this.team3_players.Size = new System.Drawing.Size(199, 220);
            this.team3_players.TabIndex = 38;
            this.team3_players.TabStop = false;
            // 
            // team6_expenses
            // 
            this.team6_expenses.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team6_expenses.ItemHeight = 24;
            this.team6_expenses.Location = new System.Drawing.Point(856, 701);
            this.team6_expenses.Name = "team6_expenses";
            this.team6_expenses.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.team6_expenses.Size = new System.Drawing.Size(54, 220);
            this.team6_expenses.TabIndex = 53;
            this.team6_expenses.TabStop = false;
            // 
            // team6_money
            // 
            this.team6_money.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team6_money.Location = new System.Drawing.Point(856, 666);
            this.team6_money.Name = "team6_money";
            this.team6_money.ReadOnly = true;
            this.team6_money.Size = new System.Drawing.Size(54, 29);
            this.team6_money.TabIndex = 52;
            this.team6_money.TabStop = false;
            // 
            // team6_captain
            // 
            this.team6_captain.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team6_captain.Location = new System.Drawing.Point(651, 666);
            this.team6_captain.Name = "team6_captain";
            this.team6_captain.ReadOnly = true;
            this.team6_captain.Size = new System.Drawing.Size(199, 29);
            this.team6_captain.TabIndex = 51;
            this.team6_captain.TabStop = false;
            // 
            // team6_players
            // 
            this.team6_players.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team6_players.ItemHeight = 24;
            this.team6_players.Location = new System.Drawing.Point(651, 701);
            this.team6_players.Name = "team6_players";
            this.team6_players.Size = new System.Drawing.Size(199, 220);
            this.team6_players.TabIndex = 50;
            this.team6_players.TabStop = false;
            // 
            // team5_expenses
            // 
            this.team5_expenses.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team5_expenses.ItemHeight = 24;
            this.team5_expenses.Location = new System.Drawing.Point(551, 701);
            this.team5_expenses.Name = "team5_expenses";
            this.team5_expenses.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.team5_expenses.Size = new System.Drawing.Size(54, 220);
            this.team5_expenses.TabIndex = 49;
            this.team5_expenses.TabStop = false;
            // 
            // team5_money
            // 
            this.team5_money.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team5_money.Location = new System.Drawing.Point(551, 666);
            this.team5_money.Name = "team5_money";
            this.team5_money.ReadOnly = true;
            this.team5_money.Size = new System.Drawing.Size(54, 29);
            this.team5_money.TabIndex = 48;
            this.team5_money.TabStop = false;
            // 
            // team5_captain
            // 
            this.team5_captain.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team5_captain.Location = new System.Drawing.Point(346, 666);
            this.team5_captain.Name = "team5_captain";
            this.team5_captain.ReadOnly = true;
            this.team5_captain.Size = new System.Drawing.Size(199, 29);
            this.team5_captain.TabIndex = 47;
            this.team5_captain.TabStop = false;
            // 
            // team5_players
            // 
            this.team5_players.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team5_players.ItemHeight = 24;
            this.team5_players.Location = new System.Drawing.Point(346, 701);
            this.team5_players.Name = "team5_players";
            this.team5_players.Size = new System.Drawing.Size(199, 220);
            this.team5_players.TabIndex = 46;
            this.team5_players.TabStop = false;
            // 
            // team4_expenses
            // 
            this.team4_expenses.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team4_expenses.ItemHeight = 24;
            this.team4_expenses.Location = new System.Drawing.Point(241, 701);
            this.team4_expenses.Name = "team4_expenses";
            this.team4_expenses.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.team4_expenses.Size = new System.Drawing.Size(54, 220);
            this.team4_expenses.TabIndex = 45;
            this.team4_expenses.TabStop = false;
            // 
            // team4_money
            // 
            this.team4_money.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team4_money.Location = new System.Drawing.Point(241, 666);
            this.team4_money.Name = "team4_money";
            this.team4_money.ReadOnly = true;
            this.team4_money.Size = new System.Drawing.Size(54, 29);
            this.team4_money.TabIndex = 44;
            this.team4_money.TabStop = false;
            // 
            // team4_captain
            // 
            this.team4_captain.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team4_captain.Location = new System.Drawing.Point(36, 666);
            this.team4_captain.Name = "team4_captain";
            this.team4_captain.ReadOnly = true;
            this.team4_captain.Size = new System.Drawing.Size(199, 29);
            this.team4_captain.TabIndex = 43;
            this.team4_captain.TabStop = false;
            // 
            // team4_players
            // 
            this.team4_players.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team4_players.ItemHeight = 24;
            this.team4_players.Location = new System.Drawing.Point(36, 701);
            this.team4_players.Name = "team4_players";
            this.team4_players.Size = new System.Drawing.Size(199, 220);
            this.team4_players.TabIndex = 42;
            this.team4_players.TabStop = false;
            // 
            // team_selector
            // 
            this.team_selector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.team_selector.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.team_selector.FormattingEnabled = true;
            this.team_selector.Location = new System.Drawing.Point(150, 237);
            this.team_selector.Name = "team_selector";
            this.team_selector.Size = new System.Drawing.Size(187, 32);
            this.team_selector.TabIndex = 54;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(80, 278);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 22);
            this.label4.TabIndex = 56;
            this.label4.Text = "Price:";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(80, 240);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 22);
            this.label5.TabIndex = 57;
            this.label5.Text = "Team:";
            // 
            // price
            // 
            this.price.DecimalPlaces = 1;
            this.price.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.price.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.price.Location = new System.Drawing.Point(150, 278);
            this.price.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
            this.price.Name = "price";
            this.price.Size = new System.Drawing.Size(85, 29);
            this.price.TabIndex = 59;
            this.price.Value = new decimal(new int[] { 1, 0, 0, 65536 });
            // 
            // undo_button
            // 
            this.undo_button.Enabled = false;
            this.undo_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.undo_button.Location = new System.Drawing.Point(360, 256);
            this.undo_button.Name = "undo_button";
            this.undo_button.Size = new System.Drawing.Size(96, 29);
            this.undo_button.TabIndex = 60;
            this.undo_button.Text = "Undo";
            this.undo_button.UseVisualStyleBackColor = true;
            this.undo_button.Click += new System.EventHandler(this.undo_button_Click);
            // 
            // half_budget_display
            // 
            this.half_budget_display.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.half_budget_display.Location = new System.Drawing.Point(666, 251);
            this.half_budget_display.Name = "half_budget_display";
            this.half_budget_display.Size = new System.Drawing.Size(174, 40);
            this.half_budget_display.TabIndex = 61;
            this.half_budget_display.Text = "Display Half Budget";
            this.half_budget_display.UseVisualStyleBackColor = true;
            this.half_budget_display.CheckedChanged += new System.EventHandler(this.half_budget_display_CheckedChanged);
            // 
            // AuctionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(947, 1041);
            this.Controls.Add(this.half_budget_display);
            this.Controls.Add(this.undo_button);
            this.Controls.Add(this.price);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.team_selector);
            this.Controls.Add(this.team6_expenses);
            this.Controls.Add(this.team6_money);
            this.Controls.Add(this.team6_captain);
            this.Controls.Add(this.team6_players);
            this.Controls.Add(this.team5_expenses);
            this.Controls.Add(this.team5_money);
            this.Controls.Add(this.team5_captain);
            this.Controls.Add(this.team5_players);
            this.Controls.Add(this.team4_expenses);
            this.Controls.Add(this.team4_money);
            this.Controls.Add(this.team4_captain);
            this.Controls.Add(this.team4_players);
            this.Controls.Add(this.team3_expenses);
            this.Controls.Add(this.team3_money);
            this.Controls.Add(this.team3_captain);
            this.Controls.Add(this.team3_players);
            this.Controls.Add(this.team2_expenses);
            this.Controls.Add(this.team2_money);
            this.Controls.Add(this.team2_captain);
            this.Controls.Add(this.team2_players);
            this.Controls.Add(this.team1_expenses);
            this.Controls.Add(this.team1_money);
            this.Controls.Add(this.team1_captain);
            this.Controls.Add(this.team1_players);
            this.Controls.Add(this.counterLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.skipButton);
            this.Controls.Add(this.auctionedButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.queue3);
            this.Controls.Add(this.queue2);
            this.Controls.Add(this.queue1);
            this.Controls.Add(this.icon5);
            this.Controls.Add(this.icon2);
            this.Controls.Add(this.comment);
            this.Controls.Add(this.icon4);
            this.Controls.Add(this.icon3);
            this.Controls.Add(this.icon1);
            this.Controls.Add(this.player);
            this.Controls.Add(this.label1);
            this.Name = "AuctionForm";
            this.Text = "Auction";
            ((System.ComponentModel.ISupportInitialize)(this.icon1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.icon3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.icon4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.icon2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.icon5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.price)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.CheckBox half_budget_display;

        private System.Windows.Forms.Button undo_button;

        private System.Windows.Forms.NumericUpDown price;

        private System.Windows.Forms.ComboBox team_selector;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;

        private System.Windows.Forms.ListBox team1_expenses;
        private System.Windows.Forms.TextBox team1_money;
        private System.Windows.Forms.TextBox team1_captain;
        private System.Windows.Forms.ListBox team1_players;
        private System.Windows.Forms.TextBox team2_captain;
        private System.Windows.Forms.ListBox team3_expenses;
        private System.Windows.Forms.TextBox team3_money;
        private System.Windows.Forms.TextBox team3_captain;
        private System.Windows.Forms.ListBox team3_players;
        private System.Windows.Forms.ListBox team6_expenses;
        private System.Windows.Forms.TextBox team6_money;
        private System.Windows.Forms.TextBox team6_captain;
        private System.Windows.Forms.ListBox team6_players;
        private System.Windows.Forms.ListBox team5_expenses;
        private System.Windows.Forms.TextBox team5_money;
        private System.Windows.Forms.TextBox team5_captain;
        private System.Windows.Forms.ListBox team5_players;
        private System.Windows.Forms.ListBox team4_expenses;
        private System.Windows.Forms.TextBox team4_money;
        private System.Windows.Forms.TextBox team4_captain;
        private System.Windows.Forms.ListBox team4_players;

        private System.Windows.Forms.ListBox team2_expenses;
        private System.Windows.Forms.ListBox team2_players;
        private System.Windows.Forms.TextBox team2_money;

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label counterLabel;

        private System.Windows.Forms.Button skipButton;

        private System.Windows.Forms.PictureBox icon2;
        private System.Windows.Forms.PictureBox icon5;
        private System.Windows.Forms.TextBox queue1;
        private System.Windows.Forms.TextBox queue2;
        private System.Windows.Forms.TextBox queue3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button auctionedButton;

        private System.Windows.Forms.PictureBox icon1;
        private System.Windows.Forms.PictureBox icon3;
        private System.Windows.Forms.PictureBox icon4;

        private System.Windows.Forms.TextBox comment;

        private System.Windows.Forms.TextBox player;

        private System.Windows.Forms.Label label1;

        #endregion
    }
}