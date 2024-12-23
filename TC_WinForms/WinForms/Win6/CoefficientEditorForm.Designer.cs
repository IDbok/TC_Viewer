namespace TC_WinForms.WinForms
{
	partial class CoefficientEditorForm
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
			components = new System.ComponentModel.Container();
			DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
			dgvCoefficients = new DataGridView();
			idDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			technologicalCardIdDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			technologicalCardDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			codeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			valueDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			shortNameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			descriptionDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			coefficientBindingSource = new BindingSource(components);
			btnAddCoefficient = new Button();
			((System.ComponentModel.ISupportInitialize)dgvCoefficients).BeginInit();
			((System.ComponentModel.ISupportInitialize)coefficientBindingSource).BeginInit();
			SuspendLayout();
			// 
			// dgvCoefficients
			// 
			dgvCoefficients.AllowUserToAddRows = false;
			dgvCoefficients.AutoGenerateColumns = false;
			dgvCoefficients.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
			dgvCoefficients.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dgvCoefficients.Columns.AddRange(new DataGridViewColumn[] { idDataGridViewTextBoxColumn, technologicalCardIdDataGridViewTextBoxColumn, technologicalCardDataGridViewTextBoxColumn, codeDataGridViewTextBoxColumn, valueDataGridViewTextBoxColumn, shortNameDataGridViewTextBoxColumn, descriptionDataGridViewTextBoxColumn });
			dgvCoefficients.DataSource = coefficientBindingSource;
			dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = SystemColors.Window;
			dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
			dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
			dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
			dgvCoefficients.DefaultCellStyle = dataGridViewCellStyle1;
			dgvCoefficients.Location = new Point(12, 12);
			dgvCoefficients.Name = "dgvCoefficients";
			dgvCoefficients.RowHeadersWidth = 62;
			dgvCoefficients.Size = new Size(1214, 386);
			dgvCoefficients.TabIndex = 0;
			dgvCoefficients.CellBeginEdit += dgvCoefficients_CellBeginEdit;
			dgvCoefficients.CellEndEdit += dgvCoefficients_CellEndEdit;
			dgvCoefficients.CellValidating += dgvCoefficients_CellValidating;
			// 
			// idDataGridViewTextBoxColumn
			// 
			idDataGridViewTextBoxColumn.DataPropertyName = "Id";
			idDataGridViewTextBoxColumn.HeaderText = "Id";
			idDataGridViewTextBoxColumn.MinimumWidth = 8;
			idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
			idDataGridViewTextBoxColumn.Visible = false;
			idDataGridViewTextBoxColumn.Width = 150;
			// 
			// technologicalCardIdDataGridViewTextBoxColumn
			// 
			technologicalCardIdDataGridViewTextBoxColumn.DataPropertyName = "TechnologicalCardId";
			technologicalCardIdDataGridViewTextBoxColumn.HeaderText = "TechnologicalCardId";
			technologicalCardIdDataGridViewTextBoxColumn.MinimumWidth = 8;
			technologicalCardIdDataGridViewTextBoxColumn.Name = "technologicalCardIdDataGridViewTextBoxColumn";
			technologicalCardIdDataGridViewTextBoxColumn.Visible = false;
			technologicalCardIdDataGridViewTextBoxColumn.Width = 150;
			// 
			// technologicalCardDataGridViewTextBoxColumn
			// 
			technologicalCardDataGridViewTextBoxColumn.DataPropertyName = "TechnologicalCard";
			technologicalCardDataGridViewTextBoxColumn.HeaderText = "TechnologicalCard";
			technologicalCardDataGridViewTextBoxColumn.MinimumWidth = 8;
			technologicalCardDataGridViewTextBoxColumn.Name = "technologicalCardDataGridViewTextBoxColumn";
			technologicalCardDataGridViewTextBoxColumn.Visible = false;
			technologicalCardDataGridViewTextBoxColumn.Width = 150;
			// 
			// codeDataGridViewTextBoxColumn
			// 
			codeDataGridViewTextBoxColumn.DataPropertyName = "Code";
			codeDataGridViewTextBoxColumn.HeaderText = "Код";
			codeDataGridViewTextBoxColumn.MinimumWidth = 8;
			codeDataGridViewTextBoxColumn.Name = "codeDataGridViewTextBoxColumn";
			codeDataGridViewTextBoxColumn.Width = 150;
			// 
			// valueDataGridViewTextBoxColumn
			// 
			valueDataGridViewTextBoxColumn.DataPropertyName = "Value";
			valueDataGridViewTextBoxColumn.HeaderText = "Значение";
			valueDataGridViewTextBoxColumn.MinimumWidth = 8;
			valueDataGridViewTextBoxColumn.Name = "valueDataGridViewTextBoxColumn";
			valueDataGridViewTextBoxColumn.Width = 150;
			// 
			// shortNameDataGridViewTextBoxColumn
			// 
			shortNameDataGridViewTextBoxColumn.DataPropertyName = "ShortName";
			shortNameDataGridViewTextBoxColumn.HeaderText = "Наименование";
			shortNameDataGridViewTextBoxColumn.MinimumWidth = 8;
			shortNameDataGridViewTextBoxColumn.Name = "shortNameDataGridViewTextBoxColumn";
			shortNameDataGridViewTextBoxColumn.Width = 300;
			// 
			// descriptionDataGridViewTextBoxColumn
			// 
			descriptionDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			descriptionDataGridViewTextBoxColumn.DataPropertyName = "Description";
			descriptionDataGridViewTextBoxColumn.HeaderText = "Описание";
			descriptionDataGridViewTextBoxColumn.MinimumWidth = 8;
			descriptionDataGridViewTextBoxColumn.Name = "descriptionDataGridViewTextBoxColumn";
			// 
			// coefficientBindingSource
			// 
			coefficientBindingSource.DataSource = typeof(TcModels.Models.TcContent.Coefficient);
			// 
			// btnAddCoefficient
			// 
			btnAddCoefficient.Location = new Point(12, 404);
			btnAddCoefficient.Name = "btnAddCoefficient";
			btnAddCoefficient.Size = new Size(218, 34);
			btnAddCoefficient.TabIndex = 1;
			btnAddCoefficient.Text = "Добавить";
			btnAddCoefficient.UseVisualStyleBackColor = true;
			btnAddCoefficient.Click += btnAddCoefficient_Click;
			// 
			// CoefficientEditorForm
			// 
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1236, 450);
			Controls.Add(btnAddCoefficient);
			Controls.Add(dgvCoefficients);
			Name = "CoefficientEditorForm";
			Text = "CoefficientEditorForm";
			((System.ComponentModel.ISupportInitialize)dgvCoefficients).EndInit();
			((System.ComponentModel.ISupportInitialize)coefficientBindingSource).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private DataGridView dgvCoefficients;
		private Button btnAddCoefficient;
		private BindingSource coefficientBindingSource;
		private DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn technologicalCardIdDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn technologicalCardDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn codeDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn valueDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn shortNameDataGridViewTextBoxColumn;
		private DataGridViewTextBoxColumn descriptionDataGridViewTextBoxColumn;
	}
}