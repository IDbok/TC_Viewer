using System.Collections.ObjectModel;
using System.Windows;
using TcModels.Models.TcContent;
using UserControl = System.Windows.Controls.UserControl;

namespace TC_WinForms.WinForms
{
	/// <summary>
	/// Interaction logic for CoefficientEditor.xaml
	/// </summary>
	public partial class CoefficientEditor : UserControl
	{
		public ObservableCollection<Coefficient> Coefficients { get; set; }

		public CoefficientEditor()
		{
			InitializeComponent();

			// Initialize collection
			Coefficients = new ObservableCollection<Coefficient>();
			CoefficientDataGrid.ItemsSource = Coefficients;
		}

		private void AddRowButton_Click(object sender, RoutedEventArgs e)
		{
			Coefficients.Add(new Coefficient());
		}
	}
}
