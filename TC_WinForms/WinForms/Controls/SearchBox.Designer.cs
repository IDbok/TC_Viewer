namespace TC_WinForms.WinForms.Controls;

partial class SearchBox<T>
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
		textBoxSearch = new TextBox();
		listBoxSuggestions = new ListBox();
		SuspendLayout();
		// 
		// textBoxSearch
		// 
		textBoxSearch.Location = new Point(0, 3);
		textBoxSearch.Name = "textBoxSearch";
		textBoxSearch.Size = new Size(292, 27);
		textBoxSearch.TabIndex = 0;
		// 
		// listBoxSuggestions
		// 
		listBoxSuggestions.FormattingEnabled = true;
		listBoxSuggestions.Location = new Point(3, 27);
		listBoxSuggestions.Name = "listBoxSuggestions";
		listBoxSuggestions.Size = new Size(289, 104);
		listBoxSuggestions.TabIndex = 1;
		// 
		// SearchBox
		// 
		AutoScaleDimensions = new SizeF(8F, 20F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(listBoxSuggestions);
		Controls.Add(textBoxSearch);
		Name = "SearchBox";
		Size = new Size(292, 133);
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private TextBox textBoxSearch;
	private ListBox listBoxSuggestions;
}
