namespace TC_WinForms.WinForms
{
	partial class Win7_Dictionary
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
			listBox1 = new ListBox();
			dataGridView1 = new DataGridView();
			Column1 = new DataGridViewTextBoxColumn();
			Column2 = new DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			SuspendLayout();
			// 
			// listBox1
			// 
			listBox1.FormattingEnabled = true;
			listBox1.ItemHeight = 25;
			listBox1.Location = new Point(77, 51);
			listBox1.Name = "listBox1";
			listBox1.Size = new Size(297, 529);
			listBox1.TabIndex = 0;
			// 
			// dataGridView1
			// 
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2 });
			dataGridView1.Location = new Point(616, 51);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.RowHeadersWidth = 62;
			dataGridView1.Size = new Size(598, 517);
			dataGridView1.TabIndex = 1;
			// 
			// Column1
			// 
			Column1.HeaderText = "Наименование";
			Column1.MinimumWidth = 8;
			Column1.Name = "Column1";
			Column1.ReadOnly = true;
			Column1.Width = 150;
			// 
			// Column2
			// 
			Column2.HeaderText = "Описание";
			Column2.MinimumWidth = 8;
			Column2.Name = "Column2";
			Column2.ReadOnly = true;
			Column2.Width = 150;
			// 
			// Win7_Dictionary
			// 
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1247, 626);
			Controls.Add(dataGridView1);
			Controls.Add(listBox1);
			Name = "Win7_Dictionary";
			Text = "Win7_Dictionary";
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private ListBox listBox1;
		private DataGridView dataGridView1;
		private DataGridViewTextBoxColumn Column1;
		private DataGridViewTextBoxColumn Column2;
	}
}