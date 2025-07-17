
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
            toolStripFile = new ToolStripSplitButton();
            SaveChangesToolStripMenuItem = new ToolStripMenuItem();
            printToolStripMenuItem = new ToolStripMenuItem();
            printBlockSchemeToolStripMenuItem = new ToolStripMenuItem();
            updateToolStripMenuItem = new ToolStripMenuItem();
            actionToolStripMenuItem = new ToolStripMenuItem();
            setDraftStatusToolStripMenuItem = new ToolStripMenuItem();
            setApprovedStatusToolStripMenuItem = new ToolStripMenuItem();
            ChangeIsDynamicToolStripMenuItem = new ToolStripMenuItem();
            setRemarksModeToolStripMenuItem = new ToolStripMenuItem();
            SetMachineCollumnModeToolStripMenuItem = new ToolStripMenuItem();
            действияToolStripMenuItem = new ToolStripMenuItem();
            toolStripExecutionScheme = new ToolStripButton();
            toolStripDiagrams = new ToolStripButton();
            toolStripShowCoefficients = new ToolStripButton();
            toolStripButton1 = new ToolStripButton();
            btnHideControlBtns = new ToolStripButton();
            btnShowStaffs = new Button();
            btnShowComponents = new Button();
            btnShowMachines = new Button();
            btnShowProtections = new Button();
            btnShowTools = new Button();
            btnRoadMap = new Button();
            btnShowWorkSteps = new Button();
            pnlControls = new Panel();
            btnShowCoefficients = new Button();
            buttonDiagram = new Button();
            btnOutlay = new Button();
            pnlDataViewer = new Panel();
            toolStripButton1 = new ToolStripButton();
            toolStrip1.SuspendLayout();
            pnlControls.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripFile, toolStripExecutionScheme, toolStripDiagrams, toolStripShowCoefficients, toolStripButton1, btnHideControlBtns });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
			toolStrip1.Padding = new Padding(0, 0, 3, 0);
			toolStrip1.Size = new Size(1704, 34);
            toolStrip1.TabIndex = 19;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripFile
            // 
            toolStripFile.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripFile.DropDownItems.AddRange(new ToolStripItem[] { SaveChangesToolStripMenuItem, printToolStripMenuItem, printBlockSchemeToolStripMenuItem, updateToolStripMenuItem, actionToolStripMenuItem, setRemarksModeToolStripMenuItem, SetMachineCollumnModeToolStripMenuItem, действияToolStripMenuItem });
            toolStripFile.Image = (Image)resources.GetObject("toolStripFile.Image");
            toolStripFile.ImageTransparentColor = Color.Magenta;
            toolStripFile.Name = "toolStripFile";
			toolStripFile.Size = new Size(97, 29);
            toolStripFile.Text = "Главная";
            // 
            // SaveChangesToolStripMenuItem
            // 
            SaveChangesToolStripMenuItem.Name = "SaveChangesToolStripMenuItem";
			SaveChangesToolStripMenuItem.Size = new Size(354, 34);
            SaveChangesToolStripMenuItem.Text = "Сохранить";
            SaveChangesToolStripMenuItem.Click += SaveChangesToolStripMenuItem_Click;
            // 
            // printToolStripMenuItem
            // 
            printToolStripMenuItem.Name = "printToolStripMenuItem";
			printToolStripMenuItem.Size = new Size(354, 34);
			printToolStripMenuItem.Text = "Печать";
			printToolStripMenuItem.Click += printToolStripMenuItem_Click;
			// 
			// printBlockSchemeToolStripMenuItem
			// 
			printBlockSchemeToolStripMenuItem.Name = "printBlockSchemeToolStripMenuItem";
            printBlockSchemeToolStripMenuItem.Visible = false;
            printBlockSchemeToolStripMenuItem.Size = new Size(354, 34);
			printBlockSchemeToolStripMenuItem.Text = "Печать блок схемы";
			printBlockSchemeToolStripMenuItem.Click += printDiagramToolStripMenuItem_Click;
			// 
			// updateToolStripMenuItem
			// 
			updateToolStripMenuItem.Name = "updateToolStripMenuItem";
			updateToolStripMenuItem.Size = new Size(354, 34);
            updateToolStripMenuItem.Text = "Редактировать";
            updateToolStripMenuItem.Click += updateToolStripMenuItem_Click;
            // 
            // actionToolStripMenuItem
            // 
            actionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { setDraftStatusToolStripMenuItem, setApprovedStatusToolStripMenuItem, ChangeIsDynamicToolStripMenuItem });
            actionToolStripMenuItem.Name = "actionToolStripMenuItem";
			actionToolStripMenuItem.Size = new Size(354, 34);
            actionToolStripMenuItem.Text = "Действия";
            // 
            // setDraftStatusToolStripMenuItem
            // 
            setDraftStatusToolStripMenuItem.Name = "setDraftStatusToolStripMenuItem";
			setDraftStatusToolStripMenuItem.Size = new Size(302, 34);
            setDraftStatusToolStripMenuItem.Text = "Выпустить ТК";
            setDraftStatusToolStripMenuItem.Click += setDraftStatusToolStripMenuItem_Click;
            // 
            // setApprovedStatusToolStripMenuItem
            // 
            setApprovedStatusToolStripMenuItem.Name = "setApprovedStatusToolStripMenuItem";
			setApprovedStatusToolStripMenuItem.Size = new Size(302, 34);
            setApprovedStatusToolStripMenuItem.Text = "Опубликовать ТК";
            setApprovedStatusToolStripMenuItem.Click += SetApprovedStatusToolStripMenuItem_Click;
            // 
            // ChangeIsDynamicToolStripMenuItem
            // 
            ChangeIsDynamicToolStripMenuItem.Name = "ChangeIsDynamicToolStripMenuItem";
			ChangeIsDynamicToolStripMenuItem.Size = new Size(302, 34);
            ChangeIsDynamicToolStripMenuItem.Text = "Сделать динамической";
            ChangeIsDynamicToolStripMenuItem.Click += ChangeIsDynamicToolStripMenuItem_Click;
            // 
            // setRemarksModeToolStripMenuItem
            // 
            setRemarksModeToolStripMenuItem.Name = "setRemarksModeToolStripMenuItem";
			setRemarksModeToolStripMenuItem.Size = new Size(354, 34);
            setRemarksModeToolStripMenuItem.Text = "Показать комментарии";
            setRemarksModeToolStripMenuItem.Click += setRemarksModeToolStripMenuItem_Click;
            // 
            // SetMachineCollumnModeToolStripMenuItem
            // 
            SetMachineCollumnModeToolStripMenuItem.Name = "SetMachineCollumnModeToolStripMenuItem";
			SetMachineCollumnModeToolStripMenuItem.Size = new Size(354, 34);
            SetMachineCollumnModeToolStripMenuItem.Text = "Скрыть столбцы механизмов";
            SetMachineCollumnModeToolStripMenuItem.Click += SetMachineCollumnModeToolStripMenuItem_Click;
            // 
            // действияToolStripMenuItem
            // 
            действияToolStripMenuItem.Name = "действияToolStripMenuItem";
			действияToolStripMenuItem.Size = new Size(354, 34);
            действияToolStripMenuItem.Text = "Действия";
            // 
            // toolStripExecutionScheme
            // 
            toolStripExecutionScheme.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripExecutionScheme.Image = (Image)resources.GetObject("toolStripExecutionScheme.Image");
            toolStripExecutionScheme.ImageTransparentColor = Color.Magenta;
            toolStripExecutionScheme.Name = "toolStripExecutionScheme";
			toolStripExecutionScheme.Size = new Size(167, 29);
            toolStripExecutionScheme.Text = "Обложка тех. карты";
            toolStripExecutionScheme.Click += toolStripExecutionScheme_Click;
            // 
            // toolStripDiagrams
            // 
            toolStripDiagrams.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDiagrams.Image = (Image)resources.GetObject("toolStripDiagrams.Image");
            toolStripDiagrams.ImageTransparentColor = Color.Magenta;
            toolStripDiagrams.Name = "toolStripDiagrams";
			toolStripDiagrams.Size = new Size(109, 29);
            toolStripDiagrams.Text = "Блок-схема";
            toolStripDiagrams.Click += toolStripDiagrams_Click;
            // 
            // toolStripShowCoefficients
            // 
            toolStripShowCoefficients.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripShowCoefficients.Image = (Image)resources.GetObject("toolStripShowCoefficients.Image");
            toolStripShowCoefficients.ImageTransparentColor = Color.Magenta;
            toolStripShowCoefficients.Name = "toolStripShowCoefficients";
			toolStripShowCoefficients.Size = new Size(139, 29);
            toolStripShowCoefficients.Text = "Коэффициенты";
            toolStripShowCoefficients.Visible = false;
            toolStripShowCoefficients.Click += toolStripShowCoefficients_Click;
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(87, 22);
            toolStripButton1.Text = "Изображения";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // btnHideControlBtns
            // 
            btnHideControlBtns.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnHideControlBtns.Image = (Image)resources.GetObject("btnHideControlBtns.Image");
            btnHideControlBtns.ImageTransparentColor = Color.Magenta;
            btnHideControlBtns.Name = "btnHideControlBtns";
            btnHideControlBtns.Size = new Size(95, 22);
            btnHideControlBtns.Text = "Скрыть кнопки";
            btnHideControlBtns.Click += btnHideControlBtns_Click;
            // 
            // btnShowStaffs
            // 
            btnShowStaffs.Font = new Font("Segoe UI", 9F);
			btnShowStaffs.Location = new Point(6, 3);
			btnShowStaffs.Margin = new Padding(4, 3, 4, 3);
            btnShowStaffs.Name = "btnShowStaffs";
			btnShowStaffs.Size = new Size(280, 70);
            btnShowStaffs.TabIndex = 27;
            btnShowStaffs.Text = "Требования к составу бригады и квалификации";
            btnShowStaffs.UseVisualStyleBackColor = true;
            btnShowStaffs.Click += btnShowStaffs_Click;
            // 
            // btnShowComponents
            // 
            btnShowComponents.Font = new Font("Segoe UI", 9F);
			btnShowComponents.Location = new Point(6, 79);
			btnShowComponents.Margin = new Padding(4, 3, 4, 3);
            btnShowComponents.Name = "btnShowComponents";
			btnShowComponents.Size = new Size(280, 70);
            btnShowComponents.TabIndex = 28;
            btnShowComponents.Text = " Требования к материалам и комплектующим";
            btnShowComponents.UseVisualStyleBackColor = true;
            btnShowComponents.Click += btnShowComponents_Click;
            // 
            // btnShowMachines
            // 
            btnShowMachines.Font = new Font("Segoe UI", 9F);
			btnShowMachines.Location = new Point(4, 155);
			btnShowMachines.Margin = new Padding(4, 3, 4, 3);
            btnShowMachines.Name = "btnShowMachines";
			btnShowMachines.Size = new Size(280, 70);
            btnShowMachines.TabIndex = 29;
            btnShowMachines.Text = "Требования к механизмам";
            btnShowMachines.UseVisualStyleBackColor = true;
            btnShowMachines.Click += btnShowMachines_Click;
            // 
            // btnShowProtections
            // 
            btnShowProtections.Font = new Font("Segoe UI", 9F);
			btnShowProtections.Location = new Point(6, 231);
			btnShowProtections.Margin = new Padding(4, 3, 4, 3);
            btnShowProtections.Name = "btnShowProtections";
			btnShowProtections.Size = new Size(280, 70);
            btnShowProtections.TabIndex = 30;
            btnShowProtections.Text = "Требования к средствам защиты";
            btnShowProtections.UseVisualStyleBackColor = true;
            btnShowProtections.Click += btnShowProtections_Click;
            // 
            // btnShowTools
            // 
            btnShowTools.Font = new Font("Segoe UI", 9F);
			btnShowTools.Location = new Point(6, 307);
			btnShowTools.Margin = new Padding(4, 3, 4, 3);
            btnShowTools.Name = "btnShowTools";
			btnShowTools.Size = new Size(280, 70);
            btnShowTools.TabIndex = 31;
            btnShowTools.Text = "Требования к инструментам и приспособлениям";
            btnShowTools.UseVisualStyleBackColor = true;
            btnShowTools.Click += btnShowTools_Click;
            // 
            // btnShowWorkSteps
            // 
            btnShowWorkSteps.Font = new Font("Segoe UI", 9F);
			btnShowWorkSteps.Location = new Point(6, 383);
			btnShowWorkSteps.Margin = new Padding(4, 3, 4, 3);
            btnShowWorkSteps.Name = "btnShowWorkSteps";
			btnShowWorkSteps.Size = new Size(280, 70);
            btnShowWorkSteps.TabIndex = 32;
            btnShowWorkSteps.Text = "Ход работ";
            btnShowWorkSteps.UseVisualStyleBackColor = true;
            btnShowWorkSteps.Click += btnShowWorkSteps_Click;
            // btnOutlay
            // 
            btnOutlay.Font = new Font("Segoe UI", 9F);
            btnOutlay.Location = new Point(5, 459);
            btnOutlay.Margin = new Padding(3, 2, 3, 2);
            btnOutlay.Name = "btnOutlay";
            btnOutlay.Size = new Size(280, 70);
            btnOutlay.TabIndex = 35;
            btnOutlay.Text = "Таблица затрат";
            btnOutlay.UseVisualStyleBackColor = true;
            btnOutlay.Click += button1_Click;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(btnShowCoefficients);
            pnlControls.Controls.Add(buttonDiagram);
            pnlControls.Controls.Add(btnRoadMap);
            pnlControls.Controls.Add(btnOutlay);
            pnlControls.Controls.Add(btnShowWorkSteps);
            pnlControls.Controls.Add(btnShowTools);
            pnlControls.Controls.Add(btnShowProtections);
            pnlControls.Controls.Add(btnShowMachines);
            pnlControls.Controls.Add(btnShowComponents);
            pnlControls.Controls.Add(btnShowStaffs);
            pnlControls.Dock = DockStyle.Left;
			pnlControls.Location = new Point(0, 34);
			pnlControls.Margin = new Padding(4, 3, 4, 3);
            pnlControls.Name = "pnlControls";
			pnlControls.Size = new Size(294, 742);
            pnlControls.TabIndex = 34;
            // 
            // btnShowCoefficients
            // 
            btnShowCoefficients.Font = new Font("Segoe UI", 9F);
			btnShowCoefficients.Location = new Point(6, 684);
			btnShowCoefficients.Margin = new Padding(4, 3, 4, 3);
            btnShowCoefficients.Name = "btnShowCoefficients";
			btnShowCoefficients.Size = new Size(280, 70);
            btnShowCoefficients.TabIndex = 34;
            btnShowCoefficients.Text = "Коэффициенты";
            btnShowCoefficients.UseVisualStyleBackColor = true;
            btnShowCoefficients.Visible = false;
            btnShowCoefficients.Click += btnShowCoefficients_Click;
            // 
            // buttonDiagram
            // 
            buttonDiagram.Font = new Font("Segoe UI", 9F);
			buttonDiagram.Location = new Point(6, 534);
			buttonDiagram.Margin = new Padding(4, 3, 4, 3);
            buttonDiagram.Name = "buttonDiagram";
			buttonDiagram.Size = new Size(280, 70);
            buttonDiagram.TabIndex = 33;
            buttonDiagram.Text = "Блок схема";
            buttonDiagram.UseVisualStyleBackColor = true;
            buttonDiagram.Click += buttonDiagram_Click;
            // 
            // btnRoadMap
            // 
            btnRoadMap.Font = new Font("Segoe UI", 9F);
            btnRoadMap.Location = new Point(6, 609);
            btnRoadMap.Margin = new Padding(4, 3, 4, 3);
            btnRoadMap.Name = "btnRoadMap";
            btnRoadMap.Size = new Size(280, 70);
            btnRoadMap.TabIndex = 36;
            btnRoadMap.Text = "Дорожная карта";
            btnRoadMap.UseVisualStyleBackColor = true;
            btnRoadMap.Click += btnRoadMap_Click;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Dock = DockStyle.Fill;
			pnlDataViewer.Location = new Point(294, 34);
			pnlDataViewer.Margin = new Padding(4, 3, 4, 3);
            pnlDataViewer.Name = "pnlDataViewer";
			pnlDataViewer.Size = new Size(1410, 642);
            pnlDataViewer.TabIndex = 35;
            //
            // Win6_new
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1704, 800);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Controls.Add(toolStrip1);
			Margin = new Padding(4, 3, 4, 3);
			//MinimumSize = new Size(1718, 584);
            Name = "Win6_new";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Win6_new";
            WindowState = FormWindowState.Maximized;
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
        private Button btnShowStaffs;
        private Button btnShowComponents;
        private Button btnShowMachines;
        private Button btnShowProtections;
        private Button btnShowTools;
        private Button btnRoadMap;
        private Button btnShowWorkSteps;
        private Panel pnlControls;
        private Panel pnlDataViewer;
        private ToolStripSplitButton toolStripFile;
        private ToolStripMenuItem printToolStripMenuItem;
        private ToolStripMenuItem updateToolStripMenuItem;
        private ToolStripMenuItem SaveChangesToolStripMenuItem;
        private ToolStripMenuItem actionToolStripMenuItem;
        private ToolStripMenuItem выпуститьПроектToolStripMenuItem;
        private ToolStripMenuItem setApprovedStatusToolStripMenuItem;
        private ToolStripMenuItem setRemarksModeToolStripMenuItem;
        private ToolStripMenuItem setDraftStatusToolStripMenuItem;
        private Button buttonDiagram;
        private ToolStripButton toolStripExecutionScheme;
        private ToolStripButton toolStripDiagrams;
        private ToolStripMenuItem SetMachineCollumnModeToolStripMenuItem;
        private ToolStripMenuItem printBlockSchemeToolStripMenuItem;
		private Button btnShowCoefficients;
        private Button btnOutlay;
        private ToolStripButton toolStripShowCoefficients;
		private ToolStripMenuItem ChangeIsDynamicToolStripMenuItem;
		private ToolStripMenuItem действияToolStripMenuItem;
        private ToolStripButton toolStripButton1;
        private ToolStripButton btnHideControlBtns;
	}
}
