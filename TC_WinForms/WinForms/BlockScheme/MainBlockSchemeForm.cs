
using System.Windows.Forms;

namespace TC_WinForms.WinForms.BlockScheme;

public partial class MainBlockSchemeForm : Form
{

    private FlowLayoutPanel flowLayoutPanel;
    public MainBlockSchemeForm()
    {
        InitializeComponent();
        InitializeFlowLayoutPanel();
        AddControlButton();
    }
    private void InitializeFlowLayoutPanel()
    {
        flowLayoutPanel = new FlowLayoutPanel();
        flowLayoutPanel.Dock = DockStyle.Fill;
        flowLayoutPanel.AutoScroll = true;
        flowLayoutPanel.WrapContents = false;
        flowLayoutPanel.FlowDirection = FlowDirection.TopDown;


        this.Controls.Add(flowLayoutPanel);
    }

    private void AddControlButton()
    {
        Button addButton = new Button();
        addButton.Text = "Добавить блок";
        addButton.Click += new EventHandler(AddButton_Click);
        this.Controls.Add(addButton);
        addButton.Dock = DockStyle.Bottom;
        addButton.Height = 50;


    }

    private void AddButton_Click(object sender, EventArgs e)
    {
        Panel panel = new Panel();

        panel.AutoSize = true; // Включение автоматического изменения размера
        panel.AutoSizeMode = AutoSizeMode.GrowAndShrink; // Панель растягивается и сжимается

        panel.Dock = DockStyle.Top;

        //panel.FlowDirection = FlowDirection.TopDown;
        panel.Width = flowLayoutPanel.ClientSize.Width; // Панель на всю ширину FlowLayoutPanel
        //panel.WrapContents = false; // Элементы не переносятся на следующую строку
        panel.Height = 200; // Высота панели
        panel.BackColor = Color.LightGray; // Пример оформления
                                           // Здесь можно добавить другие элементы в panel, например текст или картинки
                                           // Добавление кнопки для управления содержимым этой панели
        Button addContentButton = new Button();
        addContentButton.Width = panel.Width;
        addContentButton.Height = 50;
        addContentButton.Text = "Добавить элемент";
        addContentButton.Dock = DockStyle.Bottom;
        addContentButton.Click += AddContentButton_Click;

        panel.Controls.Add(addContentButton);

        flowLayoutPanel.Controls.Add(panel);
        flowLayoutPanel.ScrollControlIntoView(panel);


        // реализовать событие нажатия на кнопку addContentButton
        Panel newPanel = new Panel();
        newPanel.Height = 300;
        newPanel.Width = flowLayoutPanel.ClientSize.Width - 20; // немного меньше, чтобы учесть padding
        newPanel.Margin = new Padding(10);
        newPanel.BackColor = Color.White; // Пример оформления

        panel.Controls.Add(newPanel);
        panel.ScrollControlIntoView(newPanel); // Прокрутка к новому элементу
    }

    private void AddContentButton_Click(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        Panel parentPanel = btn.Parent as Panel;

        Panel newPanel = new Panel();
        newPanel.Height = 200;
        newPanel.Width = parentPanel.Width - 20; // немного меньше, чтобы учесть padding
        newPanel.Margin = new Padding(10);
        newPanel.BackColor = Color.White; // Пример оформления

        parentPanel.Controls.Add(newPanel);
        parentPanel.ScrollControlIntoView(newPanel); // Прокрутка к новому элементу
    }

}
