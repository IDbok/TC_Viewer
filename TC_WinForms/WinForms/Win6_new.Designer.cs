namespace TC_WinForms.WinForms
{
    partial class Win6_new
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Win6_new));
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            btnShowStaffs = new Button();
            btnShowComponents = new Button();
            btnShowMachines = new Button();
            btnShowProtections = new Button();
            btnShowTools = new Button();
            btnShowWorkSteps = new Button();
            pnlControls = new Panel();
            pnlDataViewer = new Panel();
            toolStrip1.SuspendLayout();
            pnlControls.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3, toolStripButton4 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1363, 27);
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
            // toolStripButton4
            // 
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(87, 24);
            toolStripButton4.Text = "Сохранить";
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // btnShowStaffs
            // 
            btnShowStaffs.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnShowStaffs.Location = new Point(5, 4);
            btnShowStaffs.Name = "btnShowStaffs";
            btnShowStaffs.Size = new Size(224, 50);
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
            btnShowComponents.Size = new Size(224, 50);
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
            btnShowMachines.Size = new Size(224, 50);
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
            btnShowProtections.Size = new Size(224, 50);
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
            btnShowTools.Size = new Size(224, 50);
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
            btnShowWorkSteps.Size = new Size(224, 50);
            btnShowWorkSteps.TabIndex = 32;
            btnShowWorkSteps.Text = "Ход работ";
            btnShowWorkSteps.UseVisualStyleBackColor = true;
            btnShowWorkSteps.Click += btnShowWorkSteps_Click;
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
            pnlControls.Location = new Point(0, 27);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(235, 426);
            pnlControls.TabIndex = 34;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(235, 27);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(1128, 426);
            pnlDataViewer.TabIndex = 35;
            // 
            // Win6_new
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1363, 453);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Controls.Add(toolStrip1);
            MinimumSize = new Size(1381, 489);
            Name = "Win6_new";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Win6_new";
            FormClosing += Win6_new_FormClosing;
            Load += Win6_new_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            pnlControls.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private Button btnShowStaffs;
        private Button btnShowComponents;
        private Button btnShowMachines;
        private Button btnShowProtections;
        private Button btnShowTools;
        private Button btnShowWorkSteps;
        private Panel pnlControls;
        private Panel pnlDataViewer;
        private ToolStripButton toolStripButton4;
    }
}