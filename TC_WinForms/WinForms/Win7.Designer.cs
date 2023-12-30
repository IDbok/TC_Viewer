namespace TC_WinForms.WinForms
{
    partial class Win7
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Win7));
            pnlToolStrinp = new Panel();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            pnlNavigationBlok = new Panel();
            pnlNavigationBtns = new Panel();
            button8 = new Button();
            button4 = new Button();
            button7 = new Button();
            button6 = new Button();
            button5 = new Button();
            button3 = new Button();
            btnProject = new Button();
            btnTechCard = new Button();
            pnlLable = new Panel();
            pnlComands = new Panel();
            pnlControlBtns = new Panel();
            btnDeleteTC = new Button();
            btnUpdateTC = new Button();
            btnAddNewTC = new Button();
            cmbProjectName = new ComboBox();
            lblProjectName = new Label();
            pnlDataViewer = new Panel();
            dgvMain = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            pnlToolStrinp.SuspendLayout();
            toolStrip1.SuspendLayout();
            pnlNavigationBlok.SuspendLayout();
            pnlNavigationBtns.SuspendLayout();
            pnlComands.SuspendLayout();
            pnlControlBtns.SuspendLayout();
            pnlDataViewer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            SuspendLayout();
            // 
            // pnlToolStrinp
            // 
            pnlToolStrinp.Controls.Add(toolStrip1);
            pnlToolStrinp.Dock = DockStyle.Top;
            pnlToolStrinp.Location = new Point(0, 0);
            pnlToolStrinp.Margin = new Padding(4, 4, 4, 4);
            pnlToolStrinp.Name = "pnlToolStrinp";
            pnlToolStrinp.Size = new Size(1478, 34);
            pnlToolStrinp.TabIndex = 0;
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = SystemColors.InactiveBorder;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1478, 34);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            toolStrip1.ItemClicked += toolStrip1_ItemClicked;
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(57, 29);
            toolStripButton1.Text = "Файл";
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(80, 29);
            toolStripButton2.Text = "Главная";
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(85, 29);
            toolStripButton3.Text = "Справка";
            // 
            // pnlNavigationBlok
            // 
            pnlNavigationBlok.Controls.Add(pnlNavigationBtns);
            pnlNavigationBlok.Controls.Add(pnlLable);
            pnlNavigationBlok.Dock = DockStyle.Left;
            pnlNavigationBlok.Location = new Point(0, 34);
            pnlNavigationBlok.Margin = new Padding(4, 4, 4, 4);
            pnlNavigationBlok.Name = "pnlNavigationBlok";
            pnlNavigationBlok.Size = new Size(248, 907);
            pnlNavigationBlok.TabIndex = 1;
            // 
            // pnlNavigationBtns
            // 
            pnlNavigationBtns.Controls.Add(button8);
            pnlNavigationBtns.Controls.Add(button4);
            pnlNavigationBtns.Controls.Add(button7);
            pnlNavigationBtns.Controls.Add(button6);
            pnlNavigationBtns.Controls.Add(button5);
            pnlNavigationBtns.Controls.Add(button3);
            pnlNavigationBtns.Controls.Add(btnProject);
            pnlNavigationBtns.Controls.Add(btnTechCard);
            pnlNavigationBtns.Dock = DockStyle.Fill;
            pnlNavigationBtns.Location = new Point(0, 135);
            pnlNavigationBtns.Margin = new Padding(4, 4, 4, 4);
            pnlNavigationBtns.Name = "pnlNavigationBtns";
            pnlNavigationBtns.Size = new Size(248, 772);
            pnlNavigationBtns.TabIndex = 2;
            // 
            // button8
            // 
            button8.Dock = DockStyle.Top;
            button8.Location = new Point(0, 595);
            button8.Margin = new Padding(4, 4, 4, 4);
            button8.Name = "button8";
            button8.Size = new Size(248, 85);
            button8.TabIndex = 9;
            button8.Text = "Технологиеские операции";
            button8.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Dock = DockStyle.Top;
            button4.Location = new Point(0, 510);
            button4.Margin = new Padding(4, 4, 4, 4);
            button4.Name = "button4";
            button4.Size = new Size(248, 85);
            button4.TabIndex = 8;
            button4.Text = "Инструменты и приспособления";
            button4.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            button7.Dock = DockStyle.Top;
            button7.Location = new Point(0, 425);
            button7.Margin = new Padding(4, 4, 4, 4);
            button7.Name = "button7";
            button7.Size = new Size(248, 85);
            button7.TabIndex = 7;
            button7.Text = "Средства защиты";
            button7.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            button6.Dock = DockStyle.Top;
            button6.Location = new Point(0, 340);
            button6.Margin = new Padding(4, 4, 4, 4);
            button6.Name = "button6";
            button6.Size = new Size(248, 85);
            button6.TabIndex = 6;
            button6.Text = "Механизмы";
            button6.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Dock = DockStyle.Top;
            button5.Location = new Point(0, 255);
            button5.Margin = new Padding(4, 4, 4, 4);
            button5.Name = "button5";
            button5.Size = new Size(248, 85);
            button5.TabIndex = 5;
            button5.Text = "Материалы и комплектующие";
            button5.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Dock = DockStyle.Top;
            button3.Location = new Point(0, 170);
            button3.Margin = new Padding(4, 4, 4, 4);
            button3.Name = "button3";
            button3.Size = new Size(248, 85);
            button3.TabIndex = 3;
            button3.Text = "Персонал";
            button3.UseVisualStyleBackColor = true;
            // 
            // btnProject
            // 
            btnProject.Dock = DockStyle.Top;
            btnProject.Location = new Point(0, 85);
            btnProject.Margin = new Padding(4, 4, 4, 4);
            btnProject.Name = "btnProject";
            btnProject.Size = new Size(248, 85);
            btnProject.TabIndex = 1;
            btnProject.Text = "Проекты";
            btnProject.UseVisualStyleBackColor = true;
            // 
            // btnTechCard
            // 
            btnTechCard.Dock = DockStyle.Top;
            btnTechCard.Location = new Point(0, 0);
            btnTechCard.Margin = new Padding(4, 4, 4, 4);
            btnTechCard.Name = "btnTechCard";
            btnTechCard.Size = new Size(248, 85);
            btnTechCard.TabIndex = 0;
            btnTechCard.Text = "Технологические карты";
            btnTechCard.UseVisualStyleBackColor = true;
            // 
            // pnlLable
            // 
            pnlLable.BackgroundImage = (Image)resources.GetObject("pnlLable.BackgroundImage");
            pnlLable.BackgroundImageLayout = ImageLayout.Zoom;
            pnlLable.Dock = DockStyle.Top;
            pnlLable.Location = new Point(0, 0);
            pnlLable.Margin = new Padding(4, 4, 4, 4);
            pnlLable.Name = "pnlLable";
            pnlLable.Size = new Size(248, 135);
            pnlLable.TabIndex = 1;
            // 
            // pnlComands
            // 
            pnlComands.Controls.Add(pnlControlBtns);
            pnlComands.Controls.Add(cmbProjectName);
            pnlComands.Controls.Add(lblProjectName);
            pnlComands.Dock = DockStyle.Top;
            pnlComands.Location = new Point(248, 34);
            pnlComands.Margin = new Padding(4, 4, 4, 4);
            pnlComands.Name = "pnlComands";
            pnlComands.Size = new Size(1230, 104);
            pnlComands.TabIndex = 2;
            // 
            // pnlControlBtns
            // 
            pnlControlBtns.Controls.Add(btnDeleteTC);
            pnlControlBtns.Controls.Add(btnUpdateTC);
            pnlControlBtns.Controls.Add(btnAddNewTC);
            pnlControlBtns.Dock = DockStyle.Right;
            pnlControlBtns.Location = new Point(640, 0);
            pnlControlBtns.Margin = new Padding(4, 4, 4, 4);
            pnlControlBtns.Name = "pnlControlBtns";
            pnlControlBtns.Size = new Size(590, 104);
            pnlControlBtns.TabIndex = 23;
            // 
            // btnDeleteTC
            // 
            btnDeleteTC.Location = new Point(400, 15);
            btnDeleteTC.Margin = new Padding(4, 4, 4, 4);
            btnDeleteTC.Name = "btnDeleteTC";
            btnDeleteTC.Size = new Size(175, 75);
            btnDeleteTC.TabIndex = 25;
            btnDeleteTC.Text = "Удалить";
            btnDeleteTC.UseVisualStyleBackColor = true;
            // 
            // btnUpdateTC
            // 
            btnUpdateTC.Location = new Point(206, 15);
            btnUpdateTC.Margin = new Padding(4, 4, 4, 4);
            btnUpdateTC.Name = "btnUpdateTC";
            btnUpdateTC.Size = new Size(175, 75);
            btnUpdateTC.TabIndex = 24;
            btnUpdateTC.Text = "Редактировать";
            btnUpdateTC.UseVisualStyleBackColor = true;
            // 
            // btnAddNewTC
            // 
            btnAddNewTC.Location = new Point(10, 15);
            btnAddNewTC.Margin = new Padding(4, 4, 4, 4);
            btnAddNewTC.Name = "btnAddNewTC";
            btnAddNewTC.Size = new Size(175, 75);
            btnAddNewTC.TabIndex = 23;
            btnAddNewTC.Text = "Добавить";
            btnAddNewTC.UseVisualStyleBackColor = true;
            // 
            // cmbProjectName
            // 
            cmbProjectName.FormattingEnabled = true;
            cmbProjectName.Items.AddRange(new object[] { "Карты по всем проектам", "Проект 1", "Проект 2", "Проект 3" });
            cmbProjectName.Location = new Point(8, 40);
            cmbProjectName.Margin = new Padding(4, 4, 4, 4);
            cmbProjectName.Name = "cmbProjectName";
            cmbProjectName.Size = new Size(383, 33);
            cmbProjectName.TabIndex = 18;
            cmbProjectName.Text = "Карты по всем проектам";
            // 
            // lblProjectName
            // 
            lblProjectName.AutoSize = true;
            lblProjectName.Location = new Point(6, 15);
            lblProjectName.Margin = new Padding(4, 0, 4, 0);
            lblProjectName.Name = "lblProjectName";
            lblProjectName.Size = new Size(157, 25);
            lblProjectName.TabIndex = 19;
            lblProjectName.Text = "Выберите проект:";
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Controls.Add(dgvMain);
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(248, 138);
            pnlDataViewer.Margin = new Padding(4, 4, 4, 4);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(1230, 803);
            pnlDataViewer.TabIndex = 3;
            // 
            // dgvMain
            // 
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5 });
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 0);
            dgvMain.Margin = new Padding(4, 4, 4, 4);
            dgvMain.Name = "dgvMain";
            dgvMain.RowHeadersWidth = 51;
            dgvMain.RowTemplate.Height = 29;
            dgvMain.Size = new Size(1230, 803);
            dgvMain.TabIndex = 0;
            // 
            // Column1
            // 
            Column1.HeaderText = "ID";
            Column1.MinimumWidth = 6;
            Column1.Name = "Column1";
            Column1.Width = 50;
            // 
            // Column2
            // 
            Column2.HeaderText = "Артикул";
            Column2.MinimumWidth = 6;
            Column2.Name = "Column2";
            Column2.Width = 125;
            // 
            // Column3
            // 
            Column3.HeaderText = "Наименование";
            Column3.MinimumWidth = 6;
            Column3.Name = "Column3";
            Column3.Width = 200;
            // 
            // Column4
            // 
            Column4.HeaderText = "Тех. процесс";
            Column4.MinimumWidth = 6;
            Column4.Name = "Column4";
            Column4.Width = 200;
            // 
            // Column5
            // 
            Column5.HeaderText = "Доп. Информация";
            Column5.MinimumWidth = 6;
            Column5.Name = "Column5";
            Column5.Width = 350;
            // 
            // Win7
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1478, 941);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlComands);
            Controls.Add(pnlNavigationBlok);
            Controls.Add(pnlToolStrinp);
            Margin = new Padding(4, 4, 4, 4);
            Name = "Win7";
            Text = "Form1";
            FormClosing += Win7_FormClosing;
            pnlToolStrinp.ResumeLayout(false);
            pnlToolStrinp.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            pnlNavigationBlok.ResumeLayout(false);
            pnlNavigationBtns.ResumeLayout(false);
            pnlComands.ResumeLayout(false);
            pnlComands.PerformLayout();
            pnlControlBtns.ResumeLayout(false);
            pnlDataViewer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlToolStrinp;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private Panel pnlNavigationBlok;
        private Panel pnlComands;
        private Panel pnlDataViewer;
        private Panel pnlControlBtns;
        private Button btnDeleteTC;
        private Button btnUpdateTC;
        private Button btnAddNewTC;
        private ComboBox cmbTechProcessName;
        private Label lblTechProcessName;
        private DataGridView dgvMain;
        private ComboBox cmbProjectName;
        private Label lblProjectName;
        private Panel pnlNavigationBtns;
        private Button btnTechCard;
        private Panel pnlLable;
        private Button button3;
        private Button btnProject;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private Button button8;
        private Button button4;
        private Button button7;
        private Button button6;
        private Button button5;
    }
}