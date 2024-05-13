namespace TC_WinForms.WinForms
{
    partial class Win7_2_Prj
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
            btnAddNewTC = new Button();
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
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 0);
            dgvMain.Name = "dgvMain";
            dgvMain.RowHeadersWidth = 51;
            dgvMain.RowTemplate.Height = 29;
            dgvMain.Size = new Size(800, 340);
            dgvMain.TabIndex = 0;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(pnlFilters);
            pnlControls.Controls.Add(pnlControlBtns);
            pnlControls.Dock = DockStyle.Top;
            pnlControls.Location = new Point(0, 0);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(800, 110);
            pnlControls.TabIndex = 1;
            // 
            // pnlFilters
            // 
            pnlFilters.Controls.Add(cmbProjectName);
            pnlFilters.Controls.Add(lblProjectName);
            pnlFilters.Dock = DockStyle.Fill;
            pnlFilters.Location = new Point(0, 0);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Size = new Size(328, 110);
            pnlFilters.TabIndex = 25;
            // 
            // cmbProjectName
            // 
            cmbProjectName.FormattingEnabled = true;
            cmbProjectName.Items.AddRange(new object[] { "Карты по всем проектам", "Проект 1", "Проект 2", "Проект 3" });
            cmbProjectName.Location = new Point(8, 40);
            cmbProjectName.Name = "cmbProjectName";
            cmbProjectName.Size = new Size(307, 28);
            cmbProjectName.TabIndex = 20;
            cmbProjectName.Text = "Карты по всем проектам";
            // 
            // lblProjectName
            // 
            lblProjectName.AutoSize = true;
            lblProjectName.Location = new Point(7, 20);
            lblProjectName.Name = "lblProjectName";
            lblProjectName.Size = new Size(133, 20);
            lblProjectName.TabIndex = 21;
            lblProjectName.Text = "Выберите проект:";
            // 
            // pnlControlBtns
            // 
            pnlControlBtns.Controls.Add(btnDeleteTC);
            pnlControlBtns.Controls.Add(btnUpdateTC);
            pnlControlBtns.Controls.Add(btnAddNewTC);
            pnlControlBtns.Dock = DockStyle.Right;
            pnlControlBtns.Location = new Point(328, 0);
            pnlControlBtns.Name = "pnlControlBtns";
            pnlControlBtns.Size = new Size(472, 110);
            pnlControlBtns.TabIndex = 24;
            // 
            // btnDeleteTC
            // 
            btnDeleteTC.Location = new Point(320, 12);
            btnDeleteTC.Name = "btnDeleteTC";
            btnDeleteTC.Size = new Size(139, 60);
            btnDeleteTC.TabIndex = 25;
            btnDeleteTC.Text = "Удалить";
            btnDeleteTC.UseVisualStyleBackColor = true;
            // 
            // btnUpdateTC
            // 
            btnUpdateTC.Location = new Point(165, 12);
            btnUpdateTC.Name = "btnUpdateTC";
            btnUpdateTC.Size = new Size(139, 60);
            btnUpdateTC.TabIndex = 24;
            btnUpdateTC.Text = "Редактировать";
            btnUpdateTC.UseVisualStyleBackColor = true;
            // 
            // btnAddNewTC
            // 
            btnAddNewTC.Location = new Point(8, 12);
            btnAddNewTC.Name = "btnAddNewTC";
            btnAddNewTC.Size = new Size(139, 60);
            btnAddNewTC.TabIndex = 23;
            btnAddNewTC.Text = "Добавить";
            btnAddNewTC.UseVisualStyleBackColor = true;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Controls.Add(dgvMain);
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(0, 110);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(800, 340);
            pnlDataViewer.TabIndex = 2;
            // 
            // Win7_2_Prj
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Name = "Win7_2_Prj";
            Text = "Win7_2_Prj";
            Load += Win7_2_Prj_Load;
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
        private Button btnAddNewTC;
        private Panel pnlFilters;
        private ComboBox cmbProjectName;
        private Label lblProjectName;
        private Panel pnlDataViewer;
    }
}