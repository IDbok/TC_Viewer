namespace TC_WinForms.WinForms
{
    partial class Win6_Tool
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
            pnlControlBtns = new Panel();
            btnMoveDown = new Button();
            btnDeleteObj = new Button();
            btnMoveUp = new Button();
            btnUpdateObj = new Button();
            btnAddNewObj = new Button();
            pnlDataViewer = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            pnlControls.SuspendLayout();
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
            dgvMain.Size = new Size(1000, 474);
            dgvMain.TabIndex = 0;
            dgvMain.CellEndEdit += dgvMain_CellEndEdit;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(pnlFilters);
            pnlControls.Controls.Add(pnlControlBtns);
            pnlControls.Dock = DockStyle.Top;
            pnlControls.Location = new Point(0, 0);
            pnlControls.Margin = new Padding(4);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(1000, 88);
            pnlControls.TabIndex = 1;
            // 
            // pnlFilters
            // 
            pnlFilters.Dock = DockStyle.Left;
            pnlFilters.Location = new Point(0, 0);
            pnlFilters.Margin = new Padding(4);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Size = new Size(377, 88);
            pnlFilters.TabIndex = 25;
            // 
            // pnlControlBtns
            // 
            pnlControlBtns.Controls.Add(btnMoveDown);
            pnlControlBtns.Controls.Add(btnDeleteObj);
            pnlControlBtns.Controls.Add(btnMoveUp);
            pnlControlBtns.Controls.Add(btnUpdateObj);
            pnlControlBtns.Controls.Add(btnAddNewObj);
            pnlControlBtns.Dock = DockStyle.Right;
            pnlControlBtns.Location = new Point(385, 0);
            pnlControlBtns.Margin = new Padding(4);
            pnlControlBtns.Name = "pnlControlBtns";
            pnlControlBtns.Size = new Size(615, 88);
            pnlControlBtns.TabIndex = 24;
            // 
            // btnMoveDown
            // 
            btnMoveDown.Location = new Point(573, 38);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(32, 32);
            btnMoveDown.TabIndex = 1;
            btnMoveDown.Text = "▼";
            btnMoveDown.UseVisualStyleBackColor = true;
            //btnMoveDown.Click += btnMoveDown_Click;
            // 
            // btnDeleteObj
            // 
            btnDeleteObj.Location = new Point(401, 11);
            btnDeleteObj.Margin = new Padding(4);
            btnDeleteObj.Name = "btnDeleteObj";
            btnDeleteObj.Size = new Size(143, 53);
            btnDeleteObj.TabIndex = 25;
            btnDeleteObj.Text = "Удалить";
            btnDeleteObj.UseVisualStyleBackColor = true;
            btnDeleteObj.Click += btnDeleteObj_Click;
            // 
            // btnMoveUp
            // 
            btnMoveUp.Location = new Point(573, 0);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(32, 32);
            btnMoveUp.TabIndex = 0;
            btnMoveUp.Text = "▲";
            btnMoveUp.UseVisualStyleBackColor = true;
            //btnMoveUp.Click += btnMoveUp_Click;
            // 
            // btnUpdateObj
            // 
            btnUpdateObj.Location = new Point(236, 11);
            btnUpdateObj.Margin = new Padding(4);
            btnUpdateObj.Name = "btnUpdateObj";
            btnUpdateObj.Size = new Size(143, 53);
            btnUpdateObj.TabIndex = 24;
            btnUpdateObj.Text = "Редактировать";
            btnUpdateObj.UseVisualStyleBackColor = true;
            // 
            // btnAddNewObj
            // 
            btnAddNewObj.Location = new Point(70, 11);
            btnAddNewObj.Margin = new Padding(4);
            btnAddNewObj.Name = "btnAddNewObj";
            btnAddNewObj.Size = new Size(143, 53);
            btnAddNewObj.TabIndex = 23;
            btnAddNewObj.Text = "Добавить";
            btnAddNewObj.UseVisualStyleBackColor = true;
            btnAddNewObj.Click += btnAddNewObj_Click;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Controls.Add(dgvMain);
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(0, 88);
            pnlDataViewer.Margin = new Padding(4);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(1000, 474);
            pnlDataViewer.TabIndex = 2;
            // 
            // Win6_Tool
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 562);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Margin = new Padding(4);
            Name = "Win6_Tool";
            Text = "Win6_Tool";
            FormClosing += Win6_Tool_FormClosing;
            Load += Win6_Tool_Load;
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            pnlControls.ResumeLayout(false);
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
        private Panel pnlDataViewer;
        private Button btnMoveDown;
        private Button btnMoveUp;
    }
}