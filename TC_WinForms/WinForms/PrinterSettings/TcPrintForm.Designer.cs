namespace TC_WinForms.WinForms.PrinterSettings
{
    partial class TcPrintForm
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
            pnlMain = new Panel();
            lbxTc = new ListBox();
            btnPrint = new Button();
            ckbxRoadMap = new CheckBox();
            ckbxOutlay = new CheckBox();
            ckbxDiagram = new CheckBox();
            ckbxWorkStep = new CheckBox();
            pnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(lbxTc);
            pnlMain.Controls.Add(btnPrint);
            pnlMain.Controls.Add(ckbxRoadMap);
            pnlMain.Controls.Add(ckbxOutlay);
            pnlMain.Controls.Add(ckbxDiagram);
            pnlMain.Controls.Add(ckbxWorkStep);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(258, 297);
            pnlMain.TabIndex = 0;
            // 
            // lbxTc
            // 
            lbxTc.Dock = DockStyle.Top;
            lbxTc.FormattingEnabled = true;
            lbxTc.ItemHeight = 15;
            lbxTc.Location = new Point(0, 0);
            lbxTc.Name = "lbxTc";
            lbxTc.Size = new Size(258, 64);
            lbxTc.TabIndex = 10;
            lbxTc.SelectedIndexChanged += lbxTc_SelectedIndexChanged;
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(71, 238);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(114, 41);
            btnPrint.TabIndex = 9;
            btnPrint.Text = "Печать";
            btnPrint.UseVisualStyleBackColor = true;
            btnPrint.Click += btnPrint_Click;
            // 
            // ckbxRoadMap
            // 
            ckbxRoadMap.AutoSize = true;
            ckbxRoadMap.Location = new Point(12, 189);
            ckbxRoadMap.Name = "ckbxRoadMap";
            ckbxRoadMap.Size = new Size(173, 19);
            ckbxRoadMap.TabIndex = 8;
            ckbxRoadMap.Text = "Печатать Дорожную карту";
            ckbxRoadMap.UseVisualStyleBackColor = true;
            ckbxRoadMap.CheckedChanged += ckbxRoadMap_CheckedChanged;
            // 
            // ckbxOutlay
            // 
            ckbxOutlay.AutoSize = true;
            ckbxOutlay.Location = new Point(12, 155);
            ckbxOutlay.Name = "ckbxOutlay";
            ckbxOutlay.Size = new Size(124, 19);
            ckbxOutlay.TabIndex = 7;
            ckbxOutlay.Text = "Печатать Затраты";
            ckbxOutlay.UseVisualStyleBackColor = true;
            ckbxOutlay.CheckedChanged += ckbxOutlay_CheckedChanged;
            // 
            // ckbxDiagram
            // 
            ckbxDiagram.AutoSize = true;
            ckbxDiagram.Location = new Point(12, 118);
            ckbxDiagram.Name = "ckbxDiagram";
            ckbxDiagram.Size = new Size(144, 19);
            ckbxDiagram.TabIndex = 6;
            ckbxDiagram.Text = "Печатать Блок-схему";
            ckbxDiagram.UseVisualStyleBackColor = true;
            ckbxDiagram.CheckedChanged += ckbxDiagram_CheckedChanged;
            // 
            // ckbxWorkStep
            // 
            ckbxWorkStep.AutoSize = true;
            ckbxWorkStep.Location = new Point(12, 83);
            ckbxWorkStep.Name = "ckbxWorkStep";
            ckbxWorkStep.Size = new Size(134, 19);
            ckbxWorkStep.TabIndex = 5;
            ckbxWorkStep.Text = "Печатать Ход работ";
            ckbxWorkStep.UseVisualStyleBackColor = true;
            ckbxWorkStep.CheckedChanged += ckbxWorkStep_CheckedChanged;
            // 
            // TcPrintForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(258, 297);
            Controls.Add(pnlMain);
            Name = "TcPrintForm";
            Text = "PrinterSettings";
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlMain;
        private ComboBox cmbxTechCard;
        private Button btnPrint;
        private CheckBox ckbxRoadMap;
        private CheckBox ckbxOutlay;
        private CheckBox ckbxDiagram;
        private CheckBox ckbxWorkStep;
        private ListBox lbxTc;
    }
}