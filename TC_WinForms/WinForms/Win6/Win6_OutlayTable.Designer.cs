namespace TC_WinForms.WinForms.Win6
{
    partial class Win6_OutlayTable
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
            tlpnlOutlayTable = new TableLayoutPanel();
            pnlControls = new Panel();
            btnCalculateOutlay = new Button();
            pnlDgv = new Panel();
            dgvMain = new DataGridView();
            idColumn = new DataGridViewTextBoxColumn();
            tcIdColumn = new DataGridViewTextBoxColumn();
            typeColumn = new DataGridViewTextBoxColumn();
            unitTypeColumn = new DataGridViewTextBoxColumn();
            nameColumn = new DataGridViewTextBoxColumn();
            outlayColumn = new DataGridViewTextBoxColumn();
            tlpnlOutlayTable.SuspendLayout();
            pnlControls.SuspendLayout();
            pnlDgv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            SuspendLayout();
            // 
            // tlpnlOutlayTable
            // 
            tlpnlOutlayTable.ColumnCount = 1;
            tlpnlOutlayTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlOutlayTable.Controls.Add(pnlControls, 0, 0);
            tlpnlOutlayTable.Controls.Add(pnlDgv, 0, 1);
            tlpnlOutlayTable.Dock = DockStyle.Fill;
            tlpnlOutlayTable.Location = new Point(0, 0);
            tlpnlOutlayTable.Name = "tlpnlOutlayTable";
            tlpnlOutlayTable.Padding = new Padding(10, 5, 10, 5);
            tlpnlOutlayTable.RowCount = 2;
            tlpnlOutlayTable.RowStyles.Add(new RowStyle(SizeType.Percent, 12.2807016F));
            tlpnlOutlayTable.RowStyles.Add(new RowStyle(SizeType.Percent, 87.7193F));
            tlpnlOutlayTable.Size = new Size(364, 450);
            tlpnlOutlayTable.TabIndex = 0;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(btnCalculateOutlay);
            pnlControls.Dock = DockStyle.Fill;
            pnlControls.Location = new Point(13, 8);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(338, 48);
            pnlControls.TabIndex = 0;
            // 
            // btnCalculateOutlay
            // 
            btnCalculateOutlay.Location = new Point(12, 13);
            btnCalculateOutlay.Name = "btnCalculateOutlay";
            btnCalculateOutlay.Size = new Size(144, 23);
            btnCalculateOutlay.TabIndex = 0;
            btnCalculateOutlay.Text = "Пересчитать затраты";
            btnCalculateOutlay.UseVisualStyleBackColor = true;
            btnCalculateOutlay.Click += btnCalculateOutlay_Click_1;
            // 
            // pnlDgv
            // 
            pnlDgv.Controls.Add(dgvMain);
            pnlDgv.Dock = DockStyle.Fill;
            pnlDgv.Location = new Point(13, 62);
            pnlDgv.Name = "pnlDgv";
            pnlDgv.Size = new Size(338, 380);
            pnlDgv.TabIndex = 1;
            // 
            // dgvMain
            // 
            dgvMain.AllowUserToAddRows = false;
            dgvMain.AllowUserToDeleteRows = false;
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Columns.AddRange(new DataGridViewColumn[] { idColumn, tcIdColumn, typeColumn, unitTypeColumn, nameColumn, outlayColumn });
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 0);
            dgvMain.Name = "dgvMain";
            dgvMain.ReadOnly = true;
            dgvMain.Size = new Size(338, 380);
            dgvMain.TabIndex = 0;
            // 
            // idColumn
            // 
            idColumn.HeaderText = "Id";
            idColumn.Name = "idColumn";
            idColumn.ReadOnly = true;
            idColumn.Visible = false;
            // 
            // tcIdColumn
            // 
            tcIdColumn.HeaderText = "Id Тех. карты";
            tcIdColumn.Name = "tcIdColumn";
            tcIdColumn.ReadOnly = true;
            tcIdColumn.Visible = false;
            // 
            // typeColumn
            // 
            typeColumn.HeaderText = "Вид";
            typeColumn.Name = "typeColumn";
            typeColumn.ReadOnly = true;
            // 
            // unitTypeColumn
            // 
            unitTypeColumn.HeaderText = "Ед. Измерения";
            unitTypeColumn.Name = "unitTypeColumn";
            unitTypeColumn.ReadOnly = true;
            // 
            // nameColumn
            // 
            nameColumn.HeaderText = "Наименование";
            nameColumn.Name = "nameColumn";
            nameColumn.ReadOnly = true;
            nameColumn.Visible = false;
            // 
            // outlayColumn
            // 
            outlayColumn.HeaderText = "Затраты";
            outlayColumn.Name = "outlayColumn";
            outlayColumn.ReadOnly = true;
            // 
            // Win6_OutlayTable
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(364, 450);
            Controls.Add(tlpnlOutlayTable);
            Name = "Win6_OutlayTable";
            Text = "Таблица затрат";
            Load += Win6_OutlayTable_Load;
            tlpnlOutlayTable.ResumeLayout(false);
            pnlControls.ResumeLayout(false);
            pnlDgv.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tlpnlOutlayTable;
        private Panel pnlControls;
        private Panel pnlDgv;
        private Button btnCalculateOutlay;
        private DataGridView dgvMain;
        private DataGridViewTextBoxColumn idColumn;
        private DataGridViewTextBoxColumn tcIdColumn;
        private DataGridViewTextBoxColumn typeColumn;
        private DataGridViewTextBoxColumn unitTypeColumn;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn outlayColumn;
    }
}