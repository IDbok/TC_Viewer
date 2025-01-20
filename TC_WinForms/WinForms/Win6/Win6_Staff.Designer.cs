using TC_WinForms.DataProcessing;

namespace TC_WinForms.WinForms
{
    partial class Win6_Staff
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
            dgvMain.Margin = new Padding(3, 2, 3, 2);
            dgvMain.Name = "dgvMain";
            dgvMain.RowHeadersWidth = 51;
            dgvMain.RowTemplate.Height = 29;
            dgvMain.Size = new Size(970, 293);
            dgvMain.TabIndex = 0;
            dgvMain.CellEndEdit += dgvMain_CellEndEdit;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(pnlFilters);
            pnlControls.Controls.Add(pnlControlBtns);
            pnlControls.Dock = DockStyle.Top;
            pnlControls.Location = new Point(0, 0);
            pnlControls.Margin = new Padding(3, 2, 3, 2);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(970, 45);
            pnlControls.TabIndex = 1;
            // 
            // pnlFilters
            // 
            pnlFilters.Dock = DockStyle.Left;
            pnlFilters.Location = new Point(0, 0);
            pnlFilters.Margin = new Padding(3, 2, 3, 2);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Size = new Size(264, 45);
            pnlFilters.TabIndex = 25;
            // 
            // pnlControlBtns
            // 
            pnlControlBtns.Controls.Add(btnMoveDown);
            pnlControlBtns.Controls.Add(btnDeleteObj);
            pnlControlBtns.Controls.Add(btnMoveUp);
            pnlControlBtns.Controls.Add(btnAddNewObj);
            pnlControlBtns.Dock = DockStyle.Right;
            pnlControlBtns.Location = new Point(540, 0);
            pnlControlBtns.Margin = new Padding(3, 2, 3, 2);
            pnlControlBtns.Name = "pnlControlBtns";
            pnlControlBtns.Size = new Size(430, 45);
            pnlControlBtns.TabIndex = 24;
            // 
            // btnMoveDown
            // 
            btnMoveDown.Location = new Point(401, 22);
            btnMoveDown.Margin = new Padding(2);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(23, 20);
            btnMoveDown.TabIndex = 1;
            btnMoveDown.Text = "▼";
            btnMoveDown.UseVisualStyleBackColor = true;
            btnMoveDown.Visible = false;
            btnMoveDown.Click += btnMoveDown_Click;
            // 
            // btnDeleteObj
            // 
            btnDeleteObj.Location = new Point(281, 7);
            btnDeleteObj.Margin = new Padding(3, 2, 3, 2);
            btnDeleteObj.Name = "btnDeleteObj";
            btnDeleteObj.Size = new Size(100, 32);
            btnDeleteObj.TabIndex = 25;
            btnDeleteObj.Text = "Удалить";
            btnDeleteObj.UseVisualStyleBackColor = true;
            btnDeleteObj.Click += btnDeleteObj_Click;
            // 
            // btnMoveUp
            // 
            btnMoveUp.Location = new Point(401, 0);
            btnMoveUp.Margin = new Padding(2);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(23, 20);
            btnMoveUp.TabIndex = 0;
            btnMoveUp.Text = "▲";
            btnMoveUp.UseVisualStyleBackColor = true;
            btnMoveUp.Visible = false;
            btnMoveUp.Click += btnMoveUp_Click;
            // 
            // btnAddNewObj
            // 
            btnAddNewObj.Location = new Point(175, 7);
            btnAddNewObj.Margin = new Padding(3, 2, 3, 2);
            btnAddNewObj.Name = "btnAddNewObj";
            btnAddNewObj.Size = new Size(100, 32);
            btnAddNewObj.TabIndex = 23;
            btnAddNewObj.Text = "Добавить";
            btnAddNewObj.UseVisualStyleBackColor = true;
            btnAddNewObj.Click += btnAddNewObj_Click;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Controls.Add(dgvMain);
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(0, 45);
            pnlDataViewer.Margin = new Padding(3, 2, 3, 2);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(970, 293);
            pnlDataViewer.TabIndex = 2;
            // 
            // Win6_Staff
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(970, 338);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(986, 377);
            Name = "Win6_Staff";
            Text = "Win6_Staff";
            Load += Win6_Staff_Load;
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
        private Button btnAddNewObj;
        private Panel pnlFilters;
        private Panel pnlDataViewer;
        private Button btnMoveDown;
        private Button btnMoveUp;
    }
}