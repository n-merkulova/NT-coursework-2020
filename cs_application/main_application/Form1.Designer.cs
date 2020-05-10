namespace main_application
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.LetterTextBox = new System.Windows.Forms.TextBox();
            this.SendNewLetterButton = new System.Windows.Forms.Button();
            this.serialPort2 = new System.IO.Ports.SerialPort(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.входящиеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.исходящиеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкаПортовToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.скоростьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBox3 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripComboBox3 = new System.Windows.Forms.ToolStripComboBox();
            this.порт1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.порт2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripComboBox2 = new System.Windows.Forms.ToolStripComboBox();
            this.дополнительноToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.AuthConnectButton = new System.Windows.Forms.Button();
            this.ReceiverComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.port1state_label = new System.Windows.Forms.Label();
            this.port2state_label = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.ReTextbox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.LogintextBox = new System.Windows.Forms.TextBox();
            this.Loginlabel = new System.Windows.Forms.Label();
            this.AuthDisconnectButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // serialPort1
            // 
            this.serialPort1.DataBits = 7;
            this.serialPort1.PortName = "NULL";
            this.serialPort1.RtsEnable = true;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBox1.Location = new System.Drawing.Point(13, 70);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(411, 155);
            this.textBox1.TabIndex = 9;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // LetterTextBox
            // 
            this.LetterTextBox.Location = new System.Drawing.Point(14, 307);
            this.LetterTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.LetterTextBox.Multiline = true;
            this.LetterTextBox.Name = "LetterTextBox";
            this.LetterTextBox.Size = new System.Drawing.Size(260, 76);
            this.LetterTextBox.TabIndex = 10;
            // 
            // SendNewLetterButton
            // 
            this.SendNewLetterButton.BackColor = System.Drawing.Color.RoyalBlue;
            this.SendNewLetterButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SendNewLetterButton.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SendNewLetterButton.ForeColor = System.Drawing.SystemColors.Control;
            this.SendNewLetterButton.Location = new System.Drawing.Point(282, 307);
            this.SendNewLetterButton.Margin = new System.Windows.Forms.Padding(4);
            this.SendNewLetterButton.Name = "SendNewLetterButton";
            this.SendNewLetterButton.Size = new System.Drawing.Size(143, 77);
            this.SendNewLetterButton.TabIndex = 11;
            this.SendNewLetterButton.Text = "Отправить";
            this.SendNewLetterButton.UseVisualStyleBackColor = false;
            this.SendNewLetterButton.Click += new System.EventHandler(this.SendNewLetterButton_Click);
            // 
            // serialPort2
            // 
            this.serialPort2.DataBits = 7;
            this.serialPort2.PortName = "NULL";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(807, 232);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 17);
            this.label4.TabIndex = 34;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.RoyalBlue;
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.ForeColor = System.Drawing.Color.Snow;
            this.button1.Location = new System.Drawing.Point(451, 134);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(91, 28);
            this.button1.TabIndex = 35;
            this.button1.Text = "Открыть";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.настройкаПортовToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(661, 26);
            this.menuStrip1.TabIndex = 36;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.входящиеToolStripMenuItem,
            this.исходящиеToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 24);
            this.toolStripMenuItem1.Text = "Папка сообщений";
            // 
            // входящиеToolStripMenuItem
            // 
            this.входящиеToolStripMenuItem.Name = "входящиеToolStripMenuItem";
            this.входящиеToolStripMenuItem.Size = new System.Drawing.Size(209, 26);
            this.входящиеToolStripMenuItem.Text = "Входящие";
            this.входящиеToolStripMenuItem.Click += new System.EventHandler(this.входящиеToolStripMenuItem_Click);
            // 
            // исходящиеToolStripMenuItem
            // 
            this.исходящиеToolStripMenuItem.Name = "исходящиеToolStripMenuItem";
            this.исходящиеToolStripMenuItem.Size = new System.Drawing.Size(209, 26);
            this.исходящиеToolStripMenuItem.Text = "Исходящие";
            this.исходящиеToolStripMenuItem.Click += new System.EventHandler(this.исходящиеToolStripMenuItem_Click);
            // 
            // настройкаПортовToolStripMenuItem
            // 
            this.настройкаПортовToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.скоростьToolStripMenuItem,
            this.порт1ToolStripMenuItem,
            this.порт2ToolStripMenuItem,
            this.дополнительноToolStripMenuItem});
            this.настройкаПортовToolStripMenuItem.Name = "настройкаПортовToolStripMenuItem";
            this.настройкаПортовToolStripMenuItem.Size = new System.Drawing.Size(149, 24);
            this.настройкаПортовToolStripMenuItem.Text = "Настройка портов";
            // 
            // скоростьToolStripMenuItem
            // 
            this.скоростьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox3,
            this.toolStripSeparator1,
            this.toolStripComboBox3});
            this.скоростьToolStripMenuItem.Name = "скоростьToolStripMenuItem";
            this.скоростьToolStripMenuItem.Size = new System.Drawing.Size(209, 26);
            this.скоростьToolStripMenuItem.Text = "Скорость";
            // 
            // toolStripTextBox3
            // 
            this.toolStripTextBox3.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox3.Name = "toolStripTextBox3";
            this.toolStripTextBox3.Size = new System.Drawing.Size(120, 27);
            this.toolStripTextBox3.Text = "Скороть (бод/с)";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(206, 6);
            // 
            // toolStripComboBox3
            // 
            this.toolStripComboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox3.Items.AddRange(new object[] {
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "76800",
            "115200"});
            this.toolStripComboBox3.Name = "toolStripComboBox3";
            this.toolStripComboBox3.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboBox3.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox3_SelectedIndexChanged);
            // 
            // порт1ToolStripMenuItem
            // 
            this.порт1ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox1});
            this.порт1ToolStripMenuItem.Name = "порт1ToolStripMenuItem";
            this.порт1ToolStripMenuItem.Size = new System.Drawing.Size(209, 26);
            this.порт1ToolStripMenuItem.Text = "Порт 1";
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboBox1.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // порт2ToolStripMenuItem
            // 
            this.порт2ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox2});
            this.порт2ToolStripMenuItem.Name = "порт2ToolStripMenuItem";
            this.порт2ToolStripMenuItem.Size = new System.Drawing.Size(209, 26);
            this.порт2ToolStripMenuItem.Text = "Порт 2";
            // 
            // toolStripComboBox2
            // 
            this.toolStripComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox2.Name = "toolStripComboBox2";
            this.toolStripComboBox2.Size = new System.Drawing.Size(121, 28);
            this.toolStripComboBox2.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox2_SelectedIndexChanged);
            // 
            // дополнительноToolStripMenuItem
            // 
            this.дополнительноToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
            this.дополнительноToolStripMenuItem.Name = "дополнительноToolStripMenuItem";
            this.дополнительноToolStripMenuItem.Size = new System.Drawing.Size(209, 26);
            this.дополнительноToolStripMenuItem.Text = "Дополнительно";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(209, 26);
            this.toolStripMenuItem2.Text = "Справка";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // AuthConnectButton
            // 
            this.AuthConnectButton.BackColor = System.Drawing.Color.RoyalBlue;
            this.AuthConnectButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AuthConnectButton.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AuthConnectButton.ForeColor = System.Drawing.SystemColors.Control;
            this.AuthConnectButton.Location = new System.Drawing.Point(451, 247);
            this.AuthConnectButton.Margin = new System.Windows.Forms.Padding(4);
            this.AuthConnectButton.Name = "AuthConnectButton";
            this.AuthConnectButton.Size = new System.Drawing.Size(91, 27);
            this.AuthConnectButton.TabIndex = 37;
            this.AuthConnectButton.Text = "Вход";
            this.AuthConnectButton.UseVisualStyleBackColor = false;
            this.AuthConnectButton.Click += new System.EventHandler(this.AuthConnectButton_Click);
            // 
            // ReceiverComboBox
            // 
            this.ReceiverComboBox.FormattingEnabled = true;
            this.ReceiverComboBox.Location = new System.Drawing.Point(282, 273);
            this.ReceiverComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.ReceiverComboBox.Name = "ReceiverComboBox";
            this.ReceiverComboBox.Size = new System.Drawing.Size(143, 24);
            this.ReceiverComboBox.TabIndex = 38;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(518, 269);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 17);
            this.label1.TabIndex = 41;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(595, 296);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 17);
            this.label2.TabIndex = 42;
            // 
            // port1state_label
            // 
            this.port1state_label.AutoSize = true;
            this.port1state_label.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.port1state_label.Location = new System.Drawing.Point(535, 335);
            this.port1state_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.port1state_label.Name = "port1state_label";
            this.port1state_label.Size = new System.Drawing.Size(92, 20);
            this.port1state_label.TabIndex = 43;
            this.port1state_label.Text = "Порт1 закрыт";
            // 
            // port2state_label
            // 
            this.port2state_label.AutoSize = true;
            this.port2state_label.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.port2state_label.Location = new System.Drawing.Point(535, 363);
            this.port2state_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.port2state_label.Name = "port2state_label";
            this.port2state_label.Size = new System.Drawing.Size(92, 20);
            this.port2state_label.TabIndex = 44;
            this.port2state_label.Text = "Порт2 закрыт";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(447, 335);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 20);
            this.label3.TabIndex = 45;
            this.label3.Text = "Абонент 1:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(447, 363);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 20);
            this.label5.TabIndex = 46;
            this.label5.Text = "Абонент 2:";
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.SystemColors.Control;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button4.Location = new System.Drawing.Point(550, 134);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(87, 28);
            this.button4.TabIndex = 47;
            this.button4.Text = "Закрыть";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // ReTextbox
            // 
            this.ReTextbox.Location = new System.Drawing.Point(66, 275);
            this.ReTextbox.Margin = new System.Windows.Forms.Padding(4);
            this.ReTextbox.Name = "ReTextbox";
            this.ReTextbox.Size = new System.Drawing.Size(155, 22);
            this.ReTextbox.TabIndex = 48;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 276);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 17);
            this.label6.TabIndex = 49;
            this.label6.Text = "Тема:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(229, 276);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 17);
            this.label7.TabIndex = 50;
            this.label7.Text = "Кому:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(921, 0);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(13, 12);
            this.button2.TabIndex = 51;
            this.button2.Text = "SetInboxUpdate";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(905, 0);
            this.button5.Margin = new System.Windows.Forms.Padding(4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(13, 12);
            this.button5.TabIndex = 52;
            this.button5.Text = "SetOutboxUpdate";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(516, 308);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 17);
            this.label8.TabIndex = 53;
            // 
            // LogintextBox
            // 
            this.LogintextBox.Location = new System.Drawing.Point(507, 217);
            this.LogintextBox.Margin = new System.Windows.Forms.Padding(4);
            this.LogintextBox.Name = "LogintextBox";
            this.LogintextBox.Size = new System.Drawing.Size(130, 22);
            this.LogintextBox.TabIndex = 55;
            // 
            // Loginlabel
            // 
            this.Loginlabel.AutoSize = true;
            this.Loginlabel.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Loginlabel.Location = new System.Drawing.Point(449, 217);
            this.Loginlabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Loginlabel.Name = "Loginlabel";
            this.Loginlabel.Size = new System.Drawing.Size(53, 20);
            this.Loginlabel.TabIndex = 56;
            this.Loginlabel.Text = "Логин:";
            // 
            // AuthDisconnectButton
            // 
            this.AuthDisconnectButton.BackColor = System.Drawing.SystemColors.Control;
            this.AuthDisconnectButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AuthDisconnectButton.Font = new System.Drawing.Font("Arial Narrow", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AuthDisconnectButton.Location = new System.Drawing.Point(550, 247);
            this.AuthDisconnectButton.Margin = new System.Windows.Forms.Padding(4);
            this.AuthDisconnectButton.Name = "AuthDisconnectButton";
            this.AuthDisconnectButton.Size = new System.Drawing.Size(87, 27);
            this.AuthDisconnectButton.TabIndex = 57;
            this.AuthDisconnectButton.Text = "Выйти";
            this.AuthDisconnectButton.UseVisualStyleBackColor = false;
            this.AuthDisconnectButton.Click += new System.EventHandler(this.AuthDisconnectButton_click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label9.Location = new System.Drawing.Point(447, 105);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(177, 19);
            this.label9.TabIndex = 58;
            this.label9.Text = "Управление портами";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Arial", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label10.Location = new System.Drawing.Point(11, 41);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 19);
            this.label10.TabIndex = 59;
            this.label10.Text = "Консоль";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label11.Location = new System.Drawing.Point(446, 306);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(168, 19);
            this.label11.TabIndex = 60;
            this.label11.Text = "Статусы абонентов";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Arial", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label12.Location = new System.Drawing.Point(448, 189);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(114, 19);
            this.label12.TabIndex = 61;
            this.label12.Text = "Авторизация";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Arial", 10.01739F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label13.Location = new System.Drawing.Point(13, 241);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(155, 19);
            this.label13.TabIndex = 62;
            this.label13.Text = "Новое сообщение";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.BackColor = System.Drawing.Color.Transparent;
            this.label14.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label14.Font = new System.Drawing.Font("Arial Black", 11.77391F, System.Drawing.FontStyle.Bold);
            this.label14.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label14.Location = new System.Drawing.Point(447, 56);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 27);
            this.label14.TabIndex = 63;
            this.label14.Text = "Почта";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.BackColor = System.Drawing.Color.Transparent;
            this.label15.Font = new System.Drawing.Font("Arial Black", 28.93044F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.label15.ForeColor = System.Drawing.Color.Chocolate;
            this.label15.Location = new System.Drawing.Point(517, 26);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(131, 67);
            this.label15.TabIndex = 64;
            this.label15.Text = "ИУ5";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.HighlightText;
            this.ClientSize = new System.Drawing.Size(661, 404);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.AuthDisconnectButton);
            this.Controls.Add(this.Loginlabel);
            this.Controls.Add(this.LogintextBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ReTextbox);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.port2state_label);
            this.Controls.Add(this.port1state_label);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ReceiverComboBox);
            this.Controls.Add(this.AuthConnectButton);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.SendNewLetterButton);
            this.Controls.Add(this.LetterTextBox);
            this.Controls.Add(this.textBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Почта ИУ5";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox LetterTextBox;
        private System.Windows.Forms.Button SendNewLetterButton;
        private System.IO.Ports.SerialPort serialPort2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem входящиеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem исходящиеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem настройкаПортовToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem скоростьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem порт1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem порт2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem дополнительноToolStripMenuItem;
        private System.Windows.Forms.Button AuthConnectButton;
        private System.Windows.Forms.ComboBox ReceiverComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox3;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Label port1state_label;
        private System.Windows.Forms.Label port2state_label;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox ReTextbox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox LogintextBox;
        private System.Windows.Forms.Label Loginlabel;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.Button AuthDisconnectButton;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
    }
}

