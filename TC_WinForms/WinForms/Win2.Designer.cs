namespace TC_WinForms
{
    partial class Win2
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
            gbxInformation = new GroupBox();
            lblWinInformantion = new Label();
            gbxProjectData = new GroupBox();
            lblProjectVersion = new Label();
            txtProjectVersion = new TextBox();
            lblProjectOperator = new Label();
            txtProjectOperator = new TextBox();
            lblProjectCreationDate = new Label();
            txtProjectCreationDate = new TextBox();
            lblProjectName = new Label();
            txtProjectName = new TextBox();
            lblProjectNum = new Label();
            txtProjectNum = new TextBox();
            btnTcTransPoint = new Button();
            btnNext = new Button();
            gbxFunctionalityChoice = new GroupBox();
            btnOpenSwitchgear = new Button();
            btnTcOilSub = new Button();
            pnlControlPanel = new Panel();
            btnBack = new Button();
            pnlToolBar = new Panel();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            gbxInformation.SuspendLayout();
            gbxProjectData.SuspendLayout();
            gbxFunctionalityChoice.SuspendLayout();
            pnlControlPanel.SuspendLayout();
            pnlToolBar.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // gbxInformation
            // 
            gbxInformation.Controls.Add(lblWinInformantion);
            gbxInformation.Location = new Point(20, 26);
            gbxInformation.Name = "gbxInformation";
            gbxInformation.Size = new Size(356, 54);
            gbxInformation.TabIndex = 0;
            gbxInformation.TabStop = false;
            gbxInformation.Text = "Информационное окно";
            // 
            // lblWinInformantion
            // 
            lblWinInformantion.AutoSize = true;
            lblWinInformantion.Location = new Point(6, 23);
            lblWinInformantion.Name = "lblWinInformantion";
            lblWinInformantion.Size = new Size(219, 20);
            lblWinInformantion.TabIndex = 1;
            lblWinInformantion.Text = "Выбор объекта строительства";
            // 
            // gbxProjectData
            // 
            gbxProjectData.Controls.Add(lblProjectVersion);
            gbxProjectData.Controls.Add(txtProjectVersion);
            gbxProjectData.Controls.Add(lblProjectOperator);
            gbxProjectData.Controls.Add(txtProjectOperator);
            gbxProjectData.Controls.Add(lblProjectCreationDate);
            gbxProjectData.Controls.Add(txtProjectCreationDate);
            gbxProjectData.Controls.Add(lblProjectName);
            gbxProjectData.Controls.Add(txtProjectName);
            gbxProjectData.Controls.Add(lblProjectNum);
            gbxProjectData.Controls.Add(txtProjectNum);
            gbxProjectData.Enabled = false;
            gbxProjectData.Location = new Point(18, 1);
            gbxProjectData.Name = "gbxProjectData";
            gbxProjectData.Size = new Size(330, 344);
            gbxProjectData.TabIndex = 1;
            gbxProjectData.TabStop = false;
            // 
            // lblProjectVersion
            // 
            lblProjectVersion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblProjectVersion.AutoSize = true;
            lblProjectVersion.Location = new Point(20, 265);
            lblProjectVersion.Name = "lblProjectVersion";
            lblProjectVersion.Size = new Size(62, 20);
            lblProjectVersion.TabIndex = 10;
            lblProjectVersion.Text = "Версия:";
            lblProjectVersion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtProjectVersion
            // 
            txtProjectVersion.Location = new Point(35, 285);
            txtProjectVersion.Name = "txtProjectVersion";
            txtProjectVersion.Size = new Size(283, 27);
            txtProjectVersion.TabIndex = 9;
            // 
            // lblProjectOperator
            // 
            lblProjectOperator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblProjectOperator.AutoSize = true;
            lblProjectOperator.Location = new Point(20, 205);
            lblProjectOperator.Name = "lblProjectOperator";
            lblProjectOperator.Size = new Size(146, 20);
            lblProjectOperator.TabIndex = 8;
            lblProjectOperator.Text = "ФИО разработчика:";
            lblProjectOperator.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtProjectOperator
            // 
            txtProjectOperator.Location = new Point(35, 225);
            txtProjectOperator.Name = "txtProjectOperator";
            txtProjectOperator.Size = new Size(283, 27);
            txtProjectOperator.TabIndex = 7;
            // 
            // lblProjectCreationDate
            // 
            lblProjectCreationDate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblProjectCreationDate.AutoSize = true;
            lblProjectCreationDate.Location = new Point(20, 145);
            lblProjectCreationDate.Name = "lblProjectCreationDate";
            lblProjectCreationDate.Size = new Size(113, 20);
            lblProjectCreationDate.TabIndex = 6;
            lblProjectCreationDate.Text = "Дата создания:";
            lblProjectCreationDate.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtProjectCreationDate
            // 
            txtProjectCreationDate.Location = new Point(35, 165);
            txtProjectCreationDate.Name = "txtProjectCreationDate";
            txtProjectCreationDate.Size = new Size(283, 27);
            txtProjectCreationDate.TabIndex = 5;
            // 
            // lblProjectName
            // 
            lblProjectName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblProjectName.AutoSize = true;
            lblProjectName.Location = new Point(20, 85);
            lblProjectName.Name = "lblProjectName";
            lblProjectName.Size = new Size(179, 20);
            lblProjectName.TabIndex = 4;
            lblProjectName.Text = "Наименование объекта:";
            lblProjectName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtProjectName
            // 
            txtProjectName.Location = new Point(35, 105);
            txtProjectName.Name = "txtProjectName";
            txtProjectName.Size = new Size(283, 27);
            txtProjectName.TabIndex = 3;
            // 
            // lblProjectNum
            // 
            lblProjectNum.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblProjectNum.AutoSize = true;
            lblProjectNum.Location = new Point(20, 25);
            lblProjectNum.Name = "lblProjectNum";
            lblProjectNum.Size = new Size(120, 20);
            lblProjectNum.TabIndex = 2;
            lblProjectNum.Text = "Номер объекта:";
            lblProjectNum.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtProjectNum
            // 
            txtProjectNum.Enabled = false;
            txtProjectNum.Location = new Point(35, 45);
            txtProjectNum.Name = "txtProjectNum";
            txtProjectNum.Size = new Size(283, 27);
            txtProjectNum.TabIndex = 1;
            // 
            // btnTcTransPoint
            // 
            btnTcTransPoint.Location = new Point(19, 26);
            btnTcTransPoint.Name = "btnTcTransPoint";
            btnTcTransPoint.Size = new Size(140, 90);
            btnTcTransPoint.TabIndex = 2;
            btnTcTransPoint.Text = "Точка трансформации";
            btnTcTransPoint.UseVisualStyleBackColor = true;
            btnTcTransPoint.Click += btnTcTransPoint_Click;
            // 
            // btnNext
            // 
            btnNext.Enabled = false;
            btnNext.Location = new Point(191, 363);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(160, 45);
            btnNext.TabIndex = 4;
            btnNext.Text = "Далее";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // gbxFunctionalityChoice
            // 
            gbxFunctionalityChoice.Controls.Add(btnOpenSwitchgear);
            gbxFunctionalityChoice.Controls.Add(btnTcOilSub);
            gbxFunctionalityChoice.Controls.Add(btnTcTransPoint);
            gbxFunctionalityChoice.Location = new Point(20, 81);
            gbxFunctionalityChoice.Name = "gbxFunctionalityChoice";
            gbxFunctionalityChoice.Size = new Size(356, 354);
            gbxFunctionalityChoice.TabIndex = 5;
            gbxFunctionalityChoice.TabStop = false;
            gbxFunctionalityChoice.Text = "Выбор функционала";
            // 
            // btnOpenSwitchgear
            // 
            btnOpenSwitchgear.Location = new Point(19, 228);
            btnOpenSwitchgear.Name = "btnOpenSwitchgear";
            btnOpenSwitchgear.Size = new Size(140, 90);
            btnOpenSwitchgear.TabIndex = 4;
            btnOpenSwitchgear.Text = "Подстанция\n ОРУ \nна реклоузерах";
            btnOpenSwitchgear.UseVisualStyleBackColor = true;
            btnOpenSwitchgear.Click += btnOpenSwitchgear_Click;
            // 
            // btnTcOilSub
            // 
            btnTcOilSub.Location = new Point(19, 127);
            btnTcOilSub.Name = "btnTcOilSub";
            btnTcOilSub.Size = new Size(140, 90);
            btnTcOilSub.TabIndex = 3;
            btnTcOilSub.Text = "Нефтяная подстанция";
            btnTcOilSub.UseVisualStyleBackColor = true;
            btnTcOilSub.Click += btnTcOilSub_Click;
            // 
            // pnlControlPanel
            // 
            pnlControlPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            pnlControlPanel.Controls.Add(btnBack);
            pnlControlPanel.Controls.Add(btnNext);
            pnlControlPanel.Controls.Add(gbxProjectData);
            pnlControlPanel.Location = new Point(403, 31);
            pnlControlPanel.Name = "pnlControlPanel";
            pnlControlPanel.Size = new Size(379, 422);
            pnlControlPanel.TabIndex = 8;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(19, 362);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(160, 45);
            btnBack.TabIndex = 5;
            btnBack.Text = "Назад";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // pnlToolBar
            // 
            pnlToolBar.Controls.Add(toolStrip1);
            pnlToolBar.Dock = DockStyle.Top;
            pnlToolBar.Location = new Point(0, 0);
            pnlToolBar.Name = "pnlToolBar";
            pnlToolBar.Size = new Size(782, 25);
            pnlToolBar.TabIndex = 11;
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
            // Win2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 453);
            Controls.Add(pnlToolBar);
            Controls.Add(pnlControlPanel);
            Controls.Add(gbxFunctionalityChoice);
            Controls.Add(gbxInformation);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Win2";
            RightToLeft = RightToLeft.No;
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Выбор объекта строительства";
            FormClosing += Win2_FormClosing;
            gbxInformation.ResumeLayout(false);
            gbxInformation.PerformLayout();
            gbxProjectData.ResumeLayout(false);
            gbxProjectData.PerformLayout();
            gbxFunctionalityChoice.ResumeLayout(false);
            pnlControlPanel.ResumeLayout(false);
            pnlToolBar.ResumeLayout(false);
            pnlToolBar.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox gbxInformation;
        private Label lblWinInformantion;
        private GroupBox gbxProjectData;
        private Button btnTcTransPoint;
        private Button button2;
        private Button btnNext;
        private GroupBox gbxFunctionalityChoice;
        private Button btnTcOilSub;
        private Label lblProjectNum;
        private TextBox txtProjectNum;
        private Panel pnlControlPanel;
        private Button btnBack;
        private Button btnOpenSwitchgear;
        private Panel pnlToolBar;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private Label lblProjectVersion;
        private TextBox txtProjectVersion;
        private Label lblProjectOperator;
        private TextBox txtProjectOperator;
        private Label lblProjectCreationDate;
        private TextBox txtProjectCreationDate;
        private Label lblProjectName;
        private TextBox txtProjectName;
    }
}