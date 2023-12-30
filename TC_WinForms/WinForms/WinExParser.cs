using OfficeOpenXml;

using TC_WinForms.DataProcessing;

namespace TC_WinForms
{
    public partial class WinExParser : Form
    {
        // Путь к файлу с ТК
        static string filepath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\Пример\ТК_ТТ_v4.0_Уфа — копия.xlsx";//@"C:\Users\bokar\Documents\ТК.xlsx";
        static string jsonCatalog = @"Temp\Cards";

        List<string> sheetsTC = new();
        public WinExParser()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            InitializeComponent();
            // Задание начальных значений для элементов формы
            txtFilePath.Text = filepath;
        }

        private void btnFilePathCheck_Click(object sender, EventArgs e)
        {
            gbxTCSheets.Visible = false;

            // Очистка списка листов с ТК
            sheetsTC.Clear();
            clbxTCSheets.DataSource = null;

            string filePathCheck = txtFilePath.Text;

            sheetsTC = FileManager.GetSheetsNamesWithTC(filePathCheck);
            
            if (sheetsTC.Count == 0)
            {
                PrintMessage("В файле не найдено листов с ТК!");
                return;
            }
            gbxTCSheets.Visible = true;
            clbxTCSheets.DataSource = sheetsTC;
        }

        private void btnTCSheetsSelectAll_Click(object sender, EventArgs e)
        {
            if (gbxTCSheets.Visible && clbxTCSheets.Items.Count > 0)
            {
                if (btnTCSheetsSelectAll.Text == "Выделить всё")
                {
                    for (int i = 0; i < clbxTCSheets.Items.Count; i++)
                    {
                        clbxTCSheets.SetItemChecked(i, true);
                    }
                    btnTCSheetsSelectAll.Text = "Отменить\nвыделение";
                }
                else
                {
                    for (int i = 0; i < clbxTCSheets.Items.Count; i++)
                    {
                        clbxTCSheets.SetItemChecked(i, false);
                    }
                    btnTCSheetsSelectAll.Text = "Выделить всё";
                }
            }
        }

        private void btnParseFile_Click(object sender, EventArgs e)
        {
            // Проверить наличие в списке выбранных листов хотя бы одного
            if (clbxTCSheets.CheckedItems.Count == 0)
            {
                PrintMessage("Не выбран ни один лист с ТК!");
                return;
            }

            // Выделяем список в отдельный список
            List<string> sheetsToParse = new();
            foreach (var item in clbxTCSheets.CheckedItems)
            {
                sheetsToParse.Add(item.ToString());
            }

            // Проверить наличие папки с каталогом
            if (!FileManager.CheckDirectory(jsonCatalog))
            {
                PrintMessage("Нет разрешения на создание директории."); // todo - delete this message after adding a history form
                return;
            }

            // Регистрирую делигат для вывода сообщений
            ExParserTC.RegisterMessageHandler(PrintMessage);
            ExParserTC.RegisterFilePath(filepath);
            ExParserTC.RegisterJsonCatalog(jsonCatalog);
            ExParserTC.RegisterCardNameToParse(sheetsToParse);

            ExParserTC.DoMain();
            Directory.CreateDirectory(jsonCatalog); // ?? why it's here?
            PrintMessage("Hello World!");
        }
        // TODO - create a new methot to print event history in the form
        void PrintMessage(string message) => MessageBox.Show(message);
    }


}
