namespace TC_WinForms.WinForms
{
    partial class Win6
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
            cmbTechProcessName = new ComboBox();
            dgvTcObjects = new DataGridView();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            cmbTechCardName = new ComboBox();
            btnBack = new Button();
            btnSaveChanges = new Button();
            btnShowStaffs = new Button();
            btnShowComponents = new Button();
            btnShowMachines = new Button();
            btnShowProtections = new Button();
            btnShowTools = new Button();
            btnShowWorkSteps = new Button();
            pnlNavigationTC = new Panel();
            pnlControls = new Panel();
            pnlTable = new Panel();
            panel1 = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvTcObjects).BeginInit();
            toolStrip1.SuspendLayout();
            pnlNavigationTC.SuspendLayout();
            pnlControls.SuspendLayout();
            pnlTable.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // cmbTechProcessName
            // 
            cmbTechProcessName.FormattingEnabled = true;
            cmbTechProcessName.Location = new Point(6, 4);
            cmbTechProcessName.Name = "cmbTechProcessName";
            cmbTechProcessName.Size = new Size(307, 28);
            cmbTechProcessName.TabIndex = 20;
            // 
            // dgvTcObjects
            // 
            dgvTcObjects.AllowUserToOrderColumns = true;
            dgvTcObjects.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvTcObjects.BorderStyle = BorderStyle.None;
            dgvTcObjects.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTcObjects.Location = new Point(0, 0);
            dgvTcObjects.Name = "dgvTcObjects";
            dgvTcObjects.RowHeadersWidth = 51;
            dgvTcObjects.RowTemplate.Height = 29;
            dgvTcObjects.Size = new Size(540, 334);
            dgvTcObjects.TabIndex = 22;
            dgvTcObjects.CellEndEdit += dgvTcObjects_CellEndEdit;
            dgvTcObjects.CellMouseDown += dgvTcObjects_CellMouseDown;
            dgvTcObjects.CellMouseUp += dgvTcObjects_CellMouseUp;
            dgvTcObjects.RowsRemoved += dgvTcObjects_RowsRemoved;
            dgvTcObjects.UserDeletingRow += dgvTcObjects_UserDeletingRow;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(782, 27);
            toolStrip1.TabIndex = 19;
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
            // cmbTechCardName
            // 
            cmbTechCardName.FormattingEnabled = true;
            cmbTechCardName.Location = new Point(442, 4);
            cmbTechCardName.Name = "cmbTechCardName";
            cmbTechCardName.Size = new Size(307, 28);
            cmbTechCardName.TabIndex = 23;
            cmbTechCardName.SelectedIndexChanged += cmbTechCardName_SelectedIndexChanged;
            // 
            // btnBack
            // 
            btnBack.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnBack.Location = new Point(181, 8);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(176, 45);
            btnBack.TabIndex = 26;
            btnBack.Text = "Назад";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSaveChanges.Enabled = false;
            btnSaveChanges.Location = new Point(362, 8);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(176, 45);
            btnSaveChanges.TabIndex = 25;
            btnSaveChanges.Text = "Сохранить";
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // btnShowStaffs
            // 
            btnShowStaffs.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnShowStaffs.Location = new Point(5, 4);
            btnShowStaffs.Name = "btnShowStaffs";
            btnShowStaffs.Size = new Size(230, 50);
            btnShowStaffs.TabIndex = 27;
            btnShowStaffs.Text = "Требования к составу бригады и квалификации";
            btnShowStaffs.UseVisualStyleBackColor = true;
            btnShowStaffs.Click += btnShowStaffs_Click;
            // 
            // btnShowComponents
            // 
            btnShowComponents.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnShowComponents.Location = new Point(5, 60);
            btnShowComponents.Name = "btnShowComponents";
            btnShowComponents.Size = new Size(230, 50);
            btnShowComponents.TabIndex = 28;
            btnShowComponents.Text = " Требования к материалам и комплектующим";
            btnShowComponents.UseVisualStyleBackColor = true;
            btnShowComponents.Click += btnShowComponents_Click;
            // 
            // btnShowMachines
            // 
            btnShowMachines.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnShowMachines.Location = new Point(5, 116);
            btnShowMachines.Name = "btnShowMachines";
            btnShowMachines.Size = new Size(230, 50);
            btnShowMachines.TabIndex = 29;
            btnShowMachines.Text = "Требования к механизмам";
            btnShowMachines.UseVisualStyleBackColor = true;
            btnShowMachines.Click += btnShowMachines_Click;
            // 
            // btnShowProtections
            // 
            btnShowProtections.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnShowProtections.Location = new Point(5, 172);
            btnShowProtections.Name = "btnShowProtections";
            btnShowProtections.Size = new Size(230, 50);
            btnShowProtections.TabIndex = 30;
            btnShowProtections.Text = "Требования к средствам защиты";
            btnShowProtections.UseVisualStyleBackColor = true;
            btnShowProtections.Click += btnShowProtections_Click;
            // 
            // btnShowTools
            // 
            btnShowTools.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnShowTools.Location = new Point(5, 228);
            btnShowTools.Name = "btnShowTools";
            btnShowTools.Size = new Size(230, 50);
            btnShowTools.TabIndex = 31;
            btnShowTools.Text = "Требования к инструментам и приспособлениям";
            btnShowTools.UseVisualStyleBackColor = true;
            btnShowTools.Click += btnShowTools_Click;
            // 
            // btnShowWorkSteps
            // 
            btnShowWorkSteps.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnShowWorkSteps.Location = new Point(5, 284);
            btnShowWorkSteps.Name = "btnShowWorkSteps";
            btnShowWorkSteps.Size = new Size(230, 50);
            btnShowWorkSteps.TabIndex = 32;
            btnShowWorkSteps.Text = "Ход работ";
            btnShowWorkSteps.UseVisualStyleBackColor = true;
            btnShowWorkSteps.Click += btnShowWorkSteps_Click;
            // 
            // pnlNavigationTC
            // 
            pnlNavigationTC.Controls.Add(cmbTechCardName);
            pnlNavigationTC.Controls.Add(cmbTechProcessName);
            pnlNavigationTC.Dock = DockStyle.Top;
            pnlNavigationTC.Location = new Point(0, 27);
            pnlNavigationTC.Name = "pnlNavigationTC";
            pnlNavigationTC.Size = new Size(782, 36);
            pnlNavigationTC.TabIndex = 33;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(btnShowWorkSteps);
            pnlControls.Controls.Add(btnShowTools);
            pnlControls.Controls.Add(btnShowProtections);
            pnlControls.Controls.Add(btnShowMachines);
            pnlControls.Controls.Add(btnShowComponents);
            pnlControls.Controls.Add(btnShowStaffs);
            pnlControls.Dock = DockStyle.Left;
            pnlControls.Location = new Point(0, 63);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(242, 390);
            pnlControls.TabIndex = 34;
            // 
            // pnlTable
            // 
            pnlTable.Controls.Add(dgvTcObjects);
            pnlTable.Dock = DockStyle.Fill;
            pnlTable.Location = new Point(242, 63);
            pnlTable.Name = "pnlTable";
            pnlTable.Size = new Size(540, 390);
            pnlTable.TabIndex = 35;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnBack);
            panel1.Controls.Add(btnSaveChanges);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(242, 394);
            panel1.Name = "panel1";
            panel1.Size = new Size(540, 59);
            panel1.TabIndex = 36;
            // 
            // Win6
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 453);
            Controls.Add(panel1);
            Controls.Add(pnlTable);
            Controls.Add(pnlControls);
            Controls.Add(pnlNavigationTC);
            Controls.Add(toolStrip1);
            Name = "Win6";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Win6";
            FormClosing += Win6_FormClosing;
            ((System.ComponentModel.ISupportInitialize)dgvTcObjects).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            pnlNavigationTC.ResumeLayout(false);
            pnlControls.ResumeLayout(false);
            pnlTable.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cmbTechProcessName;
        private DataGridView dgvTcObjects;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ComboBox cmbTechCardName;
        private Button btnBack;
        private Button btnSaveChanges;
        private Button btnShowStaffs;
        private Button btnShowComponents;
        private Button btnShowMachines;
        private Button btnShowProtections;
        private Button btnShowTools;
        private Button btnShowWorkSteps;
        private Panel pnlNavigationTC;
        private Panel pnlControls;
        private Panel pnlTable;
        private Panel panel1;
    }
}