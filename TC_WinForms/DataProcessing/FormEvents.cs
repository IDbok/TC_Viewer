using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.DataProcessing
{
    public class FormEvents
    {
        public static void Form_KeyDown_Save(object sender, KeyEventArgs e)
        {
            // Проверяем, была ли нажата комбинация клавиш Ctrl + S
            if (e.Control && e.KeyCode == Keys.S)
            {
                //Save(); // Вызываем метод сохранения
            }
        }

        private void Save()
        {
            // Код сохранения
            MessageBox.Show("Сохранено!");
        }
    }
}
