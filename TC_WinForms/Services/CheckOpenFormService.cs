using TC_WinForms.Interfaces;

namespace TC_WinForms.Services
{
	public static class CheckOpenFormService
    {
        public static Form? FindOpenedForm(int objectId, string FormType)
        {
            FormCollection fc = Application.OpenForms;

            foreach (Form frm in fc)
            {
                //iterate through
                if (frm.GetType().Name == FormType && frm is IFormWithObjectId openFormCheck && openFormCheck.GetObjectId() == objectId)
                    return frm;
            }
            return null;
        }

        /// <summary>
        /// Ищет открытую форму типа <typeparamref name="T"/> с заданным идентификатором объекта.
        /// </summary>
        /// <typeparam name="T">
        /// Тип формы, которую нужно найти. Должен наследовать <see cref="Form"/> 
        /// и реализовывать <see cref="IFormWithObjectId"/>.
        /// </typeparam>
        /// <param name="objectId">Идентификатор объекта для поиска формы.</param>
        /// <returns>
        /// Найденная форма типа <typeparamref name="T"/> или <c>null</c>, если форма не найдена.
        /// </returns>
        public static T? FindOpenedForm<T>(int objectId) where T : Form, IFormWithObjectId
        {
            FormCollection fc = Application.OpenForms;

            foreach (Form frm in fc)
            {
                // Проверяем, что форма имеет нужный тип и реализует интерфейс IOpenFormCheck
                if (frm is T matchingForm && matchingForm.GetObjectId() == objectId)
                    return matchingForm;
            }
            return null;
        }
    }

}
