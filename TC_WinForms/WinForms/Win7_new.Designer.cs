﻿namespace TC_WinForms.WinForms
{
    partial class Win7_new
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Win7_new));
            pnlToolStrinp = new Panel();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            saveToolStripButton = new ToolStripButton();
            toolStripButton5 = new ToolStripButton();
            updateToolStripButton = new ToolStripButton();
            pnlNavigationBlok = new Panel();
            pnlNavigationBtns = new Panel();
            btnWorkStep = new Button();
            btnTechOperation = new Button();
            btnTool = new Button();
            btnProtection = new Button();
            btnMachine = new Button();
            btnComponent = new Button();
            btnStaff = new Button();
            btnProject = new Button();
            btnTechCard = new Button();
            pnlLable = new Panel();
            pnlDataViewer = new Panel();
            progressBarLoad = new ProgressBar();
            pnlPageControls = new Panel();
            lblPageInfo = new Label();
            btnPreviousPage = new Button();
            btnNextPage = new Button();
            pnlToolStrinp.SuspendLayout();
            toolStrip1.SuspendLayout();
            pnlNavigationBlok.SuspendLayout();
            pnlNavigationBtns.SuspendLayout();
            pnlPageControls.SuspendLayout();
            SuspendLayout();
            // 
            // pnlToolStrinp
            // 
            pnlToolStrinp.Controls.Add(toolStrip1);
            pnlToolStrinp.Dock = DockStyle.Top;
            pnlToolStrinp.Location = new Point(0, 0);
            pnlToolStrinp.Name = "pnlToolStrinp";
            pnlToolStrinp.Size = new Size(1262, 27);
            pnlToolStrinp.TabIndex = 0;
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = SystemColors.InactiveBorder;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, saveToolStripButton, toolStripButton5, updateToolStripButton });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1262, 27);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(49, 24);
            toolStripButton1.Text = "Файл";
            toolStripButton1.Visible = false;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(69, 24);
            toolStripButton2.Text = "Главная";
            toolStripButton2.Visible = false;
            // 
            // saveToolStripButton
            // 
            saveToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            saveToolStripButton.ImageTransparentColor = Color.Magenta;
            saveToolStripButton.Name = "saveToolStripButton";
            saveToolStripButton.Size = new Size(71, 24);
            saveToolStripButton.Text = "Справка";
            saveToolStripButton.Visible = false;
            // 
            // toolStripButton5
            // 
            toolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(87, 24);
            toolStripButton5.Text = "Сохранить";
            toolStripButton5.Visible = false;
            toolStripButton5.Click += toolStripButton5_Click;
            // 
            // updateToolStripButton
            // 
            updateToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            updateToolStripButton.Image = (Image)resources.GetObject("updateToolStripButton.Image");
            updateToolStripButton.ImageTransparentColor = Color.Magenta;
            updateToolStripButton.Name = "updateToolStripButton";
            updateToolStripButton.Size = new Size(82, 24);
            updateToolStripButton.Text = "Обновить";
            updateToolStripButton.Click += updateToolStripButton_Click;
            // 
            // pnlNavigationBlok
            // 
            pnlNavigationBlok.Controls.Add(pnlNavigationBtns);
            pnlNavigationBlok.Controls.Add(pnlLable);
            pnlNavigationBlok.Dock = DockStyle.Left;
            pnlNavigationBlok.Location = new Point(0, 27);
            pnlNavigationBlok.Name = "pnlNavigationBlok";
            pnlNavigationBlok.Size = new Size(198, 808);
            pnlNavigationBlok.TabIndex = 1;
            // 
            // pnlNavigationBtns
            // 
            pnlNavigationBtns.Controls.Add(btnWorkStep);
            pnlNavigationBtns.Controls.Add(btnTechOperation);
            pnlNavigationBtns.Controls.Add(btnTool);
            pnlNavigationBtns.Controls.Add(btnProtection);
            pnlNavigationBtns.Controls.Add(btnMachine);
            pnlNavigationBtns.Controls.Add(btnComponent);
            pnlNavigationBtns.Controls.Add(btnStaff);
            pnlNavigationBtns.Controls.Add(btnProject);
            pnlNavigationBtns.Controls.Add(btnTechCard);
            pnlNavigationBtns.Dock = DockStyle.Fill;
            pnlNavigationBtns.Location = new Point(0, 108);
            pnlNavigationBtns.Name = "pnlNavigationBtns";
            pnlNavigationBtns.Size = new Size(198, 700);
            pnlNavigationBtns.TabIndex = 2;
            // 
            // btnWorkStep
            // 
            btnWorkStep.Dock = DockStyle.Top;
            btnWorkStep.Location = new Point(0, 544);
            btnWorkStep.Name = "btnWorkStep";
            btnWorkStep.Size = new Size(198, 68);
            btnWorkStep.TabIndex = 10;
            btnWorkStep.Text = "Технологические переходы";
            btnWorkStep.UseVisualStyleBackColor = true;
            btnWorkStep.Click += btnWorkStep_Click;
            // 
            // btnTechOperation
            // 
            btnTechOperation.Dock = DockStyle.Top;
            btnTechOperation.Location = new Point(0, 476);
            btnTechOperation.Name = "btnTechOperation";
            btnTechOperation.Size = new Size(198, 68);
            btnTechOperation.TabIndex = 9;
            btnTechOperation.Text = "Технологические операции";
            btnTechOperation.UseVisualStyleBackColor = true;
            btnTechOperation.Click += btnTechOperation_Click;
            // 
            // btnTool
            // 
            btnTool.Dock = DockStyle.Top;
            btnTool.Location = new Point(0, 408);
            btnTool.Name = "btnTool";
            btnTool.Size = new Size(198, 68);
            btnTool.TabIndex = 8;
            btnTool.Text = "Инструменты и приспособления";
            btnTool.UseVisualStyleBackColor = true;
            btnTool.Click += btnTool_Click;
            // 
            // btnProtection
            // 
            btnProtection.Dock = DockStyle.Top;
            btnProtection.Location = new Point(0, 340);
            btnProtection.Name = "btnProtection";
            btnProtection.Size = new Size(198, 68);
            btnProtection.TabIndex = 7;
            btnProtection.Text = "Средства защиты";
            btnProtection.UseVisualStyleBackColor = true;
            btnProtection.Click += btnProtection_Click;
            // 
            // btnMachine
            // 
            btnMachine.Dock = DockStyle.Top;
            btnMachine.Location = new Point(0, 272);
            btnMachine.Name = "btnMachine";
            btnMachine.Size = new Size(198, 68);
            btnMachine.TabIndex = 6;
            btnMachine.Text = "Механизмы";
            btnMachine.UseVisualStyleBackColor = true;
            btnMachine.Click += btnMachine_Click;
            // 
            // btnComponent
            // 
            btnComponent.Dock = DockStyle.Top;
            btnComponent.Location = new Point(0, 204);
            btnComponent.Name = "btnComponent";
            btnComponent.Size = new Size(198, 68);
            btnComponent.TabIndex = 5;
            btnComponent.Text = "Материалы и комплектующие";
            btnComponent.UseVisualStyleBackColor = true;
            btnComponent.Click += btnComponent_Click;
            // 
            // btnStaff
            // 
            btnStaff.Dock = DockStyle.Top;
            btnStaff.Location = new Point(0, 136);
            btnStaff.Name = "btnStaff";
            btnStaff.Size = new Size(198, 68);
            btnStaff.TabIndex = 3;
            btnStaff.Text = "Персонал";
            btnStaff.UseVisualStyleBackColor = true;
            btnStaff.Click += btnStaff_Click;
            // 
            // btnProject
            // 
            btnProject.Dock = DockStyle.Top;
            btnProject.Location = new Point(0, 68);
            btnProject.Name = "btnProject";
            btnProject.Size = new Size(198, 68);
            btnProject.TabIndex = 1;
            btnProject.Text = "Проекты";
            btnProject.UseVisualStyleBackColor = true;
            btnProject.Click += btnProject_Click;
            // 
            // btnTechCard
            // 
            btnTechCard.Dock = DockStyle.Top;
            btnTechCard.Location = new Point(0, 0);
            btnTechCard.Name = "btnTechCard";
            btnTechCard.Size = new Size(198, 68);
            btnTechCard.TabIndex = 0;
            btnTechCard.Text = "Технологические карты";
            btnTechCard.UseVisualStyleBackColor = true;
            btnTechCard.Click += btnTechCard_Click;
            // 
            // pnlLable
            // 
            pnlLable.BackgroundImage = (Image)resources.GetObject("pnlLable.BackgroundImage");
            pnlLable.BackgroundImageLayout = ImageLayout.Zoom;
            pnlLable.Dock = DockStyle.Top;
            pnlLable.Location = new Point(0, 0);
            pnlLable.Name = "pnlLable";
            pnlLable.Size = new Size(198, 108);
            pnlLable.TabIndex = 1;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(198, 27);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(1064, 776);
            pnlDataViewer.TabIndex = 3;
            // 
            // progressBarLoad
            // 
            progressBarLoad.Anchor = AnchorStyles.None;
            progressBarLoad.Location = new Point(454, 203);
            progressBarLoad.Margin = new Padding(2);
            progressBarLoad.Name = "progressBarLoad";
            progressBarLoad.Size = new Size(585, 27);
            progressBarLoad.Style = ProgressBarStyle.Marquee;
            progressBarLoad.TabIndex = 0;
            progressBarLoad.Visible = false;
            // 
            // pnlPageControls
            // 
            pnlPageControls.Controls.Add(lblPageInfo);
            pnlPageControls.Controls.Add(btnPreviousPage);
            pnlPageControls.Controls.Add(btnNextPage);
            pnlPageControls.Dock = DockStyle.Bottom;
            pnlPageControls.Location = new Point(198, 803);
            pnlPageControls.Margin = new Padding(2);
            pnlPageControls.Name = "pnlPageControls";
            pnlPageControls.Size = new Size(1064, 32);
            pnlPageControls.TabIndex = 4;
            pnlPageControls.Visible = false;
            // 
            // lblPageInfo
            // 
            lblPageInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblPageInfo.AutoSize = true;
            lblPageInfo.ImageAlign = ContentAlignment.MiddleRight;
            lblPageInfo.Location = new Point(794, 6);
            lblPageInfo.Margin = new Padding(2, 0, 2, 0);
            lblPageInfo.Name = "lblPageInfo";
            lblPageInfo.Size = new Size(0, 20);
            lblPageInfo.TabIndex = 2;
            // 
            // btnPreviousPage
            // 
            btnPreviousPage.Anchor = AnchorStyles.Top;
            btnPreviousPage.Location = new Point(480, 2);
            btnPreviousPage.Margin = new Padding(2);
            btnPreviousPage.Name = "btnPreviousPage";
            btnPreviousPage.Size = new Size(34, 27);
            btnPreviousPage.TabIndex = 1;
            btnPreviousPage.Text = "⏴";
            btnPreviousPage.UseVisualStyleBackColor = true;
            btnPreviousPage.Click += btnPreviousPage_Click;
            // 
            // btnNextPage
            // 
            btnNextPage.Anchor = AnchorStyles.Top;
            btnNextPage.Location = new Point(584, 2);
            btnNextPage.Margin = new Padding(2);
            btnNextPage.Name = "btnNextPage";
            btnNextPage.Size = new Size(32, 27);
            btnNextPage.TabIndex = 0;
            btnNextPage.Text = "⏵";
            btnNextPage.UseVisualStyleBackColor = true;
            btnNextPage.Click += btnNextPage_Click;
            // 
            // Win7_new
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 835);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlPageControls);
            Controls.Add(progressBarLoad);
            Controls.Add(pnlNavigationBlok);
            Controls.Add(pnlToolStrinp);
            MinimumSize = new Size(1154, 824);
            Name = "Win7_new";
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            FormClosing += Win7_FormClosing;
            Load += Win7_new_Load;
            pnlToolStrinp.ResumeLayout(false);
            pnlToolStrinp.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            pnlNavigationBlok.ResumeLayout(false);
            pnlNavigationBtns.ResumeLayout(false);
            pnlPageControls.ResumeLayout(false);
            pnlPageControls.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlToolStrinp;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private Panel pnlNavigationBlok;
        private Panel pnlDataViewer;
        private ComboBox cmbTechProcessName;
        private Label lblTechProcessName;
        private Panel pnlNavigationBtns;
        private Button btnTechCard;
        private Panel pnlLable;
        private Button btnStaff;
        private Button btnProject;
        private Button btnTechOperation;
        private Button btnTool;
        private Button btnProtection;
        private Button btnMachine;
        private Button btnComponent;
        private Button btnWorkStep;
        private ToolStripButton saveToolStripButton;
        private ToolStripButton toolStripButton5;
        private ToolStripButton updateToolStripButton;
        private ProgressBar progressBarLoad;
        private Panel pnlPageControls;
        private Button btnPreviousPage;
        private Button btnNextPage;
        private Label lblPageInfo;
    }
}