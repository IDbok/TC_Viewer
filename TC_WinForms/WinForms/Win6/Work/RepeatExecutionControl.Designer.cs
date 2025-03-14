namespace TC_WinForms.WinForms.Work;

partial class RepeatExecutionControl
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

	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		dataGridViewRepeats = new DataGridView();
		dgvRepeatsEwObject = new DataGridViewTextBoxColumn();
		dgvRepeatsSelected = new DataGridViewCheckBoxColumn();
		dgvRepeatsToName = new DataGridViewTextBoxColumn();
		dgvRepeatsTpName = new DataGridViewTextBoxColumn();
		dgvRepeatsOldCoefficient = new DataGridViewTextBoxColumn();
		dgvRepeatsCoefficient = new DataGridViewTextBoxColumn();
		dgvRepeatsEtap = new DataGridViewTextBoxColumn();
		dgvRepeatsPosled = new DataGridViewTextBoxColumn();
		((System.ComponentModel.ISupportInitialize)dataGridViewRepeats).BeginInit();
		SuspendLayout();
		// 
		// dataGridViewRepeats
		// 
		dataGridViewRepeats.AllowUserToAddRows = false;
		dataGridViewRepeats.AllowUserToDeleteRows = false;
		dataGridViewRepeats.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		dataGridViewRepeats.Columns.AddRange(new DataGridViewColumn[] { dgvRepeatsEwObject, dgvRepeatsSelected, dgvRepeatsToName, dgvRepeatsTpName, dgvRepeatsOldCoefficient, dgvRepeatsCoefficient, dgvRepeatsEtap, dgvRepeatsPosled });
		dataGridViewRepeats.Dock = DockStyle.Fill;
		dataGridViewRepeats.Location = new Point(0, 0);
		dataGridViewRepeats.Margin = new Padding(3, 2, 3, 2);
		dataGridViewRepeats.MultiSelect = false;
		dataGridViewRepeats.Name = "dataGridViewRepeats";
		dataGridViewRepeats.RowHeadersWidth = 51;
		dataGridViewRepeats.Size = new Size(1192, 619);
		dataGridViewRepeats.TabIndex = 3;
		// 
		// dgvRepeatsEwObject
		// 
		dgvRepeatsEwObject.HeaderText = "EwObject";
		dgvRepeatsEwObject.MinimumWidth = 6;
		dgvRepeatsEwObject.Name = "dgvRepeatsEwObject";
		dgvRepeatsEwObject.SortMode = DataGridViewColumnSortMode.NotSortable;
		dgvRepeatsEwObject.Visible = false;
		dgvRepeatsEwObject.Width = 125;
		// 
		// dgvRepeatsSelected
		// 
		dgvRepeatsSelected.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
		dgvRepeatsSelected.HeaderText = "";
		dgvRepeatsSelected.MinimumWidth = 6;
		dgvRepeatsSelected.Name = "dgvRepeatsSelected";
		dgvRepeatsSelected.Width = 50;
		// 
		// dgvRepeatsToName
		// 
		dgvRepeatsToName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		dgvRepeatsToName.HeaderText = "Технологические операции";
		dgvRepeatsToName.MinimumWidth = 6;
		dgvRepeatsToName.Name = "dgvRepeatsToName";
		dgvRepeatsToName.SortMode = DataGridViewColumnSortMode.NotSortable;
		// 
		// dgvRepeatsTpName
		// 
		dgvRepeatsTpName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		dgvRepeatsTpName.HeaderText = "Технологические переходы";
		dgvRepeatsTpName.MinimumWidth = 6;
		dgvRepeatsTpName.Name = "dgvRepeatsTpName";
		dgvRepeatsTpName.ReadOnly = true;
		dgvRepeatsTpName.SortMode = DataGridViewColumnSortMode.NotSortable;
		// 
		// dgvRepeatsOldCoefficient
		// 
		dgvRepeatsOldCoefficient.HeaderText = "Коэффициент";
		dgvRepeatsOldCoefficient.MinimumWidth = 8;
		dgvRepeatsOldCoefficient.Name = "dgvRepeatsOldCoefficient";
		dgvRepeatsOldCoefficient.ReadOnly = true;
		dgvRepeatsOldCoefficient.SortMode = DataGridViewColumnSortMode.NotSortable;
		dgvRepeatsOldCoefficient.Width = 150;
		// 
		// dgvRepeatsCoefficient
		// 
		dgvRepeatsCoefficient.HeaderText = "Коэффициент повтора";
		dgvRepeatsCoefficient.MinimumWidth = 8;
		dgvRepeatsCoefficient.Name = "dgvRepeatsCoefficient";
		dgvRepeatsCoefficient.SortMode = DataGridViewColumnSortMode.NotSortable;
		dgvRepeatsCoefficient.Width = 150;
		// 
		// dgvRepeatsEtap
		// 
		dgvRepeatsEtap.HeaderText = "Этап";
		dgvRepeatsEtap.MinimumWidth = 8;
		dgvRepeatsEtap.Name = "dgvRepeatsEtap";
		dgvRepeatsEtap.SortMode = DataGridViewColumnSortMode.NotSortable;
		dgvRepeatsEtap.Visible = false;
		dgvRepeatsEtap.Width = 150;
		// 
		// dgvRepeatsPosled
		// 
		dgvRepeatsPosled.HeaderText = "Последовательность";
		dgvRepeatsPosled.MinimumWidth = 8;
		dgvRepeatsPosled.Name = "dgvRepeatsPosled";
		dgvRepeatsPosled.SortMode = DataGridViewColumnSortMode.NotSortable;
		dgvRepeatsPosled.Visible = false;
		dgvRepeatsPosled.Width = 150;
		// 
		// RepeatExecutionControl
		// 
		AutoScaleDimensions = new SizeF(8F, 20F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(dataGridViewRepeats);
		Name = "RepeatExecutionControl";
		Size = new Size(1192, 619);
		((System.ComponentModel.ISupportInitialize)dataGridViewRepeats).EndInit();
		ResumeLayout(false);
	}

	#endregion

	private DataGridView dataGridViewRepeats;
	private DataGridViewTextBoxColumn dgvRepeatsEwObject;
	private DataGridViewCheckBoxColumn dgvRepeatsSelected;
	private DataGridViewTextBoxColumn dgvRepeatsToName;
	private DataGridViewTextBoxColumn dgvRepeatsTpName;
	private DataGridViewTextBoxColumn dgvRepeatsOldCoefficient;
	private DataGridViewTextBoxColumn dgvRepeatsCoefficient;
	private DataGridViewTextBoxColumn dgvRepeatsEtap;
	private DataGridViewTextBoxColumn dgvRepeatsPosled;
}
