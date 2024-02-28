namespace TC_WinForms.WinForms
{
    partial class Win7_1_TCs
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
            dgvMain = new DataGridView();
            pnlControls = new Panel();
            pnlFilters = new Panel();
            cmbProjectName = new ComboBox();
            lblProjectName = new Label();
            pnlControlBtns = new Panel();
            btnDeleteTC = new Button();
            btnUpdateTC = new Button();
            btnCreateTC = new Button();
            pnlDataViewer = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            pnlControls.SuspendLayout();
            pnlFilters.SuspendLayout();
            pnlControlBtns.SuspendLayout();
            pnlDataViewer.SuspendLayout();
            SuspendLayout();
            // 
            // dgvMain
            // 
            dgvMain.AllowUserToAddRows = false;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 0);
            dgvMain.Margin = new Padding(4);
            dgvMain.Name = "dgvMain";
            dgvMain.RowHeadersWidth = 51;
            dgvMain.RowTemplate.Height = 29;
            dgvMain.Size = new Size(1000, 424);
            dgvMain.TabIndex = 0;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(pnlFilters);
            pnlControls.Controls.Add(pnlControlBtns);
            pnlControls.Dock = DockStyle.Top;
            pnlControls.Location = new Point(0, 0);
            pnlControls.Margin = new Padding(4);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(1000, 138);
            pnlControls.TabIndex = 1;
            // 
            // pnlFilters
            // 
            pnlFilters.Controls.Add(cmbProjectName);
            pnlFilters.Controls.Add(lblProjectName);
            pnlFilters.Dock = DockStyle.Left;
            pnlFilters.Location = new Point(0, 0);
            pnlFilters.Margin = new Padding(4);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Size = new Size(402, 138);
            pnlFilters.TabIndex = 25;
            // 
            // cmbProjectName
            // 
            cmbProjectName.FormattingEnabled = true;
            cmbProjectName.Items.AddRange(new object[] { "Карты по всем проектам", "Проект 1", "Проект 2", "Проект 3" });
            cmbProjectName.Location = new Point(10, 50);
            cmbProjectName.Margin = new Padding(4);
            cmbProjectName.Name = "cmbProjectName";
            cmbProjectName.Size = new Size(383, 33);
            cmbProjectName.TabIndex = 20;
            cmbProjectName.Text = "Карты по всем проектам";
            // 
            // lblProjectName
            // 
            lblProjectName.AutoSize = true;
            lblProjectName.Location = new Point(9, 25);
            lblProjectName.Margin = new Padding(4, 0, 4, 0);
            lblProjectName.Name = "lblProjectName";
            lblProjectName.Size = new Size(157, 25);
            lblProjectName.TabIndex = 21;
            lblProjectName.Text = "Выберите проект:";
            // 
            // pnlControlBtns
            // 
            pnlControlBtns.Controls.Add(btnDeleteTC);
            pnlControlBtns.Controls.Add(btnUpdateTC);
            pnlControlBtns.Controls.Add(btnCreateTC);
            pnlControlBtns.Dock = DockStyle.Right;
            pnlControlBtns.Location = new Point(410, 0);
            pnlControlBtns.Margin = new Padding(4);
            pnlControlBtns.Name = "pnlControlBtns";
            pnlControlBtns.Size = new Size(590, 138);
            pnlControlBtns.TabIndex = 24;
            // 
            // btnDeleteTC
            // 
            btnDeleteTC.Location = new Point(400, 15);
            btnDeleteTC.Margin = new Padding(4);
            btnDeleteTC.Name = "btnDeleteTC";
            btnDeleteTC.Size = new Size(174, 75);
            btnDeleteTC.TabIndex = 25;
            btnDeleteTC.Text = "Удалить";
            btnDeleteTC.UseVisualStyleBackColor = true;
            btnDeleteTC.Click += btnDeleteTC_Click;
            // 
            // btnUpdateTC
            // 
            btnUpdateTC.Location = new Point(206, 15);
            btnUpdateTC.Margin = new Padding(4);
            btnUpdateTC.Name = "btnUpdateTC";
            btnUpdateTC.Size = new Size(174, 75);
            btnUpdateTC.TabIndex = 24;
            btnUpdateTC.Text = "Редактировать";
            btnUpdateTC.UseVisualStyleBackColor = true;
            btnUpdateTC.Click += btnUpdateTC_Click;
            // 
            // btnCreateTC
            // 
            btnCreateTC.Location = new Point(10, 15);
            btnCreateTC.Margin = new Padding(4);
            btnCreateTC.Name = "btnCreateTC";
            btnCreateTC.Size = new Size(174, 75);
            btnCreateTC.TabIndex = 23;
            btnCreateTC.Text = "Добавить";
            btnCreateTC.UseVisualStyleBackColor = true;
            btnCreateTC.Click += btnCreateTC_Click;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Controls.Add(dgvMain);
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(0, 138);
            pnlDataViewer.Margin = new Padding(4);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(1000, 424);
            pnlDataViewer.TabIndex = 2;
            // 
            // Win7_1_TCs
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 562);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Margin = new Padding(4);
            Name = "Win7_1_TCs";
            Text = "Win7_1_TCs";
            Load += Win7_1_TCs_Load;
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            pnlControls.ResumeLayout(false);
            pnlFilters.ResumeLayout(false);
            pnlFilters.PerformLayout();
            pnlControlBtns.ResumeLayout(false);
            pnlDataViewer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvMain;
        private Panel pnlControls;
        private Panel pnlControlBtns;
        private Button btnDeleteTC;
        private Button btnUpdateTC;
        private Button btnCreateTC;
        private Panel pnlFilters;
        private ComboBox cmbProjectName;
        private Label lblProjectName;
        private Panel pnlDataViewer;
    }
}