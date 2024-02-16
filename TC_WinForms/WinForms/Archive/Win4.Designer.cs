namespace TC_WinForms
{
    partial class Win4
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
            btnBack = new Button();
            btnNext = new Button();
            gbxInformation = new GroupBox();
            lblWinInformantion = new Label();
            gbxFunctionalityChoice = new GroupBox();
            button4 = new Button();
            button7 = new Button();
            button10 = new Button();
            button11 = new Button();
            btnGround = new Button();
            btnFoundation = new Button();
            btnSwitchgear10_6 = new Button();
            btnTransBusbar = new Button();
            btnLightningRod = new Button();
            btnConnections = new Button();
            btnIllumination = new Button();
            btnTrans = new Button();
            btnLineTrap = new Button();
            btnFences = new Button();
            btnSwitchgear35 = new Button();
            pnlToolBar = new Panel();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            gbxInformation.SuspendLayout();
            gbxFunctionalityChoice.SuspendLayout();
            pnlToolBar.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnBack
            // 
            btnBack.Location = new Point(421, 395);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(160, 45);
            btnBack.TabIndex = 9;
            btnBack.Text = "Назад";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // btnNext
            // 
            btnNext.Enabled = false;
            btnNext.Location = new Point(593, 395);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(160, 45);
            btnNext.TabIndex = 8;
            btnNext.Text = "Далее";
            btnNext.UseVisualStyleBackColor = true;
            // 
            // gbxInformation
            // 
            gbxInformation.Controls.Add(lblWinInformantion);
            gbxInformation.Location = new Point(20, 26);
            gbxInformation.Name = "gbxInformation";
            gbxInformation.Size = new Size(356, 54);
            gbxInformation.TabIndex = 10;
            gbxInformation.TabStop = false;
            gbxInformation.Text = "Информационное окно";
            // 
            // lblWinInformantion
            // 
            lblWinInformantion.AutoSize = true;
            lblWinInformantion.Location = new Point(6, 23);
            lblWinInformantion.Name = "lblWinInformantion";
            lblWinInformantion.Size = new Size(276, 20);
            lblWinInformantion.TabIndex = 1;
            lblWinInformantion.Text = "Выбор состава объекта строительства";
            // 
            // gbxFunctionalityChoice
            // 
            gbxFunctionalityChoice.Controls.Add(button4);
            gbxFunctionalityChoice.Controls.Add(button7);
            gbxFunctionalityChoice.Controls.Add(button10);
            gbxFunctionalityChoice.Controls.Add(button11);
            gbxFunctionalityChoice.Controls.Add(btnGround);
            gbxFunctionalityChoice.Controls.Add(btnFoundation);
            gbxFunctionalityChoice.Controls.Add(btnSwitchgear10_6);
            gbxFunctionalityChoice.Controls.Add(btnTransBusbar);
            gbxFunctionalityChoice.Controls.Add(btnLightningRod);
            gbxFunctionalityChoice.Controls.Add(btnConnections);
            gbxFunctionalityChoice.Controls.Add(btnIllumination);
            gbxFunctionalityChoice.Controls.Add(btnTrans);
            gbxFunctionalityChoice.Controls.Add(btnLineTrap);
            gbxFunctionalityChoice.Controls.Add(btnFences);
            gbxFunctionalityChoice.Controls.Add(btnSwitchgear35);
            gbxFunctionalityChoice.Location = new Point(20, 81);
            gbxFunctionalityChoice.Name = "gbxFunctionalityChoice";
            gbxFunctionalityChoice.Size = new Size(730, 308);
            gbxFunctionalityChoice.TabIndex = 11;
            gbxFunctionalityChoice.TabStop = false;
            gbxFunctionalityChoice.Text = "Выбор функционала";
            // 
            // button4
            // 
            button4.Location = new Point(150, 212);
            button4.Name = "button4";
            button4.Size = new Size(140, 90);
            button4.TabIndex = 18;
            button4.UseVisualStyleBackColor = true;
            button4.Visible = false;
            // 
            // button7
            // 
            button7.Location = new Point(442, 212);
            button7.Name = "button7";
            button7.Size = new Size(140, 90);
            button7.TabIndex = 17;
            button7.UseVisualStyleBackColor = true;
            button7.Visible = false;
            // 
            // button10
            // 
            button10.Location = new Point(588, 212);
            button10.Name = "button10";
            button10.Size = new Size(140, 90);
            button10.TabIndex = 16;
            button10.UseVisualStyleBackColor = true;
            button10.Visible = false;
            // 
            // button11
            // 
            button11.Location = new Point(296, 212);
            button11.Name = "button11";
            button11.Size = new Size(140, 90);
            button11.TabIndex = 15;
            button11.UseVisualStyleBackColor = true;
            button11.Visible = false;
            // 
            // btnGround
            // 
            btnGround.Location = new Point(588, 21);
            btnGround.Name = "btnGround";
            btnGround.Size = new Size(140, 90);
            btnGround.TabIndex = 14;
            btnGround.Text = "Контур заземления";
            btnGround.UseVisualStyleBackColor = true;
            btnGround.Click += btnGround_Click;
            // 
            // btnFoundation
            // 
            btnFoundation.Location = new Point(150, 117);
            btnFoundation.Name = "btnFoundation";
            btnFoundation.Size = new Size(140, 90);
            btnFoundation.TabIndex = 12;
            btnFoundation.Text = "Фундамент";
            btnFoundation.UseVisualStyleBackColor = true;
            btnFoundation.Click += btnFoundation_Click;
            // 
            // btnSwitchgear10_6
            // 
            btnSwitchgear10_6.Location = new Point(150, 21);
            btnSwitchgear10_6.Name = "btnSwitchgear10_6";
            btnSwitchgear10_6.Size = new Size(140, 90);
            btnSwitchgear10_6.TabIndex = 11;
            btnSwitchgear10_6.Text = "ОРУ 10/6";
            btnSwitchgear10_6.UseVisualStyleBackColor = true;
            btnSwitchgear10_6.Click += btnSwitchgear10_6_Click;
            // 
            // btnTransBusbar
            // 
            btnTransBusbar.Location = new Point(442, 117);
            btnTransBusbar.Name = "btnTransBusbar";
            btnTransBusbar.Size = new Size(140, 90);
            btnTransBusbar.TabIndex = 9;
            btnTransBusbar.Text = "Ошиновка тр-ра";
            btnTransBusbar.UseVisualStyleBackColor = true;
            btnTransBusbar.Click += btnTransBusbar_Click;
            // 
            // btnLightningRod
            // 
            btnLightningRod.Location = new Point(442, 21);
            btnLightningRod.Name = "btnLightningRod";
            btnLightningRod.Size = new Size(140, 90);
            btnLightningRod.TabIndex = 8;
            btnLightningRod.Text = "Молниеприёмник";
            btnLightningRod.UseVisualStyleBackColor = true;
            btnLightningRod.Click += btnLightningRod_Click;
            // 
            // btnConnections
            // 
            btnConnections.Location = new Point(588, 117);
            btnConnections.Name = "btnConnections";
            btnConnections.Size = new Size(140, 90);
            btnConnections.TabIndex = 7;
            btnConnections.Text = "Узлы подключения";
            btnConnections.UseVisualStyleBackColor = true;
            btnConnections.Click += btnConnections_Click;
            // 
            // btnIllumination
            // 
            btnIllumination.Location = new Point(296, 117);
            btnIllumination.Name = "btnIllumination";
            btnIllumination.Size = new Size(140, 90);
            btnIllumination.TabIndex = 6;
            btnIllumination.Text = "Освещение";
            btnIllumination.UseVisualStyleBackColor = true;
            btnIllumination.Click += btnIllumination_Click;
            // 
            // btnTrans
            // 
            btnTrans.Location = new Point(296, 21);
            btnTrans.Name = "btnTrans";
            btnTrans.Size = new Size(140, 90);
            btnTrans.TabIndex = 5;
            btnTrans.Text = "Трансформатор";
            btnTrans.UseVisualStyleBackColor = true;
            btnTrans.Click += btnTrans_Click;
            // 
            // btnLineTrap
            // 
            btnLineTrap.Location = new Point(4, 213);
            btnLineTrap.Name = "btnLineTrap";
            btnLineTrap.Size = new Size(140, 90);
            btnLineTrap.TabIndex = 4;
            btnLineTrap.Text = "ВЧ связь";
            btnLineTrap.UseVisualStyleBackColor = true;
            btnLineTrap.Click += btnLineTrap_Click;
            // 
            // btnFences
            // 
            btnFences.Location = new Point(4, 117);
            btnFences.Name = "btnFences";
            btnFences.Size = new Size(140, 90);
            btnFences.TabIndex = 3;
            btnFences.Text = "Элемент ограждения";
            btnFences.UseVisualStyleBackColor = true;
            btnFences.Click += btnFences_Click;
            // 
            // btnSwitchgear35
            // 
            btnSwitchgear35.Location = new Point(4, 21);
            btnSwitchgear35.Name = "btnSwitchgear35";
            btnSwitchgear35.Size = new Size(140, 90);
            btnSwitchgear35.TabIndex = 2;
            btnSwitchgear35.Text = "ОРУ 35";
            btnSwitchgear35.UseVisualStyleBackColor = true;
            btnSwitchgear35.Click += btnSwitchgear35_Click;
            // 
            // pnlToolBar
            // 
            pnlToolBar.Controls.Add(toolStrip1);
            pnlToolBar.Dock = DockStyle.Top;
            pnlToolBar.Location = new Point(0, 0);
            pnlToolBar.Name = "pnlToolBar";
            pnlToolBar.Size = new Size(782, 25);
            pnlToolBar.TabIndex = 12;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(782, 27);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(49, 24);
            toolStripButton1.Text = "Файл";
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(69, 24);
            toolStripButton2.Text = "Главная";
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(71, 24);
            toolStripButton3.Text = "Справка";
            // 
            // Win4
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 453);
            Controls.Add(pnlToolBar);
            Controls.Add(gbxFunctionalityChoice);
            Controls.Add(gbxInformation);
            Controls.Add(btnBack);
            Controls.Add(btnNext);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Win4";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Win4";
            FormClosing += Win4_FormClosing;
            gbxInformation.ResumeLayout(false);
            gbxInformation.PerformLayout();
            gbxFunctionalityChoice.ResumeLayout(false);
            pnlToolBar.ResumeLayout(false);
            pnlToolBar.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button btnBack;
        private Button btnNext;
        private GroupBox gbxInformation;
        private Label lblWinInformantion;
        private GroupBox gbxFunctionalityChoice;
        private Button btnLineTrap;
        private Button btnFences;
        private Button btnSwitchgear35;
        private Button btnGround;
        private Button btnFoundation;
        private Button btnSwitchgear10_6;
        private Button btnTransBusbar;
        private Button btnLightningRod;
        private Button btnConnections;
        private Button btnIllumination;
        private Button btnTrans;
        private Button button4;
        private Button button7;
        private Button button10;
        private Button button11;
        private Panel pnlToolBar;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
    }
}