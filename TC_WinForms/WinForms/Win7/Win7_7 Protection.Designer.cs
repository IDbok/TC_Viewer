namespace TC_WinForms.WinForms
{
    partial class Win7_7_Protection
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
            btnDeleteObj = new Button();
            btnUpdateObj = new Button();
            btnAddNewObj = new Button();
            pnlDataViewer = new Panel();
            progressBar = new ProgressBar();
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
            dgvMain.Size = new Size(1000, 462);
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
            pnlControls.Size = new Size(1000, 100);
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
            pnlFilters.Size = new Size(402, 100);
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
            pnlControlBtns.Controls.Add(btnDeleteObj);
            pnlControlBtns.Controls.Add(btnUpdateObj);
            pnlControlBtns.Controls.Add(btnAddNewObj);
            pnlControlBtns.Dock = DockStyle.Right;
            pnlControlBtns.Location = new Point(410, 0);
            pnlControlBtns.Margin = new Padding(4);
            pnlControlBtns.Name = "pnlControlBtns";
            pnlControlBtns.Size = new Size(590, 100);
            pnlControlBtns.TabIndex = 24;
            // 
            // btnDeleteObj
            // 
            btnDeleteObj.Location = new Point(400, 15);
            btnDeleteObj.Margin = new Padding(4);
            btnDeleteObj.Name = "btnDeleteObj";
            btnDeleteObj.Size = new Size(174, 75);
            btnDeleteObj.TabIndex = 25;
            btnDeleteObj.Text = "Удалить";
            btnDeleteObj.UseVisualStyleBackColor = true;
            btnDeleteObj.Click += btnDeleteObj_Click;
            // 
            // btnUpdateObj
            // 
            btnUpdateObj.Location = new Point(206, 15);
            btnUpdateObj.Margin = new Padding(4);
            btnUpdateObj.Name = "btnUpdateObj";
            btnUpdateObj.Size = new Size(174, 75);
            btnUpdateObj.TabIndex = 24;
            btnUpdateObj.Text = "Редактировать";
            btnUpdateObj.UseVisualStyleBackColor = true;
            // 
            // btnAddNewObj
            // 
            btnAddNewObj.Location = new Point(10, 15);
            btnAddNewObj.Margin = new Padding(4);
            btnAddNewObj.Name = "btnAddNewObj";
            btnAddNewObj.Size = new Size(174, 75);
            btnAddNewObj.TabIndex = 23;
            btnAddNewObj.Text = "Добавить";
            btnAddNewObj.UseVisualStyleBackColor = true;
            btnAddNewObj.Click += btnAddNewObj_Click;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Controls.Add(progressBar);
            pnlDataViewer.Controls.Add(dgvMain);
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(0, 100);
            pnlDataViewer.Margin = new Padding(4);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(1000, 462);
            pnlDataViewer.TabIndex = 2;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.None;
            progressBar.Location = new Point(226, 84);
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(564, 34);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 2;
            progressBar.Visible = false;
            // 
            // Win7_7_Protection
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 562);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Margin = new Padding(4);
            Name = "Win7_7_Protection";
            Text = "Win7_7_Protection";
            FormClosing += Win7_7_Protection_FormClosing;
            Load += Win7_7_Protection_Load;
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
        private Button btnDeleteObj;
        private Button btnUpdateObj;
        private Button btnAddNewObj;
        private Panel pnlFilters;
        private ComboBox cmbProjectName;
        private Label lblProjectName;
        private Panel pnlDataViewer;
        private ProgressBar progressBar;
    }
}