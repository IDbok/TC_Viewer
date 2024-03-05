
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Xml;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using static TC_WinForms.WinForms.Win6_Staff;

namespace TC_WinForms.DataProcessing.Utilities
{
    public class DisplayedEntityHelper
    {
        public static bool IsValidNewCard<T>(T obj) where T : IDisplayedEntity
        {
            List<string> requiredFields = obj.GetRequiredFields();
            foreach (var field in requiredFields)
            {
                try
                {
                    var propertyInfo = obj.GetType().GetProperty(field);
                    if (propertyInfo == null)
                    {
                        //  log an error or handle the case when the property is not found
                    }

                    var value = propertyInfo.GetValue(obj);
                    if (value == null || IsDefaultValue(value))
                    {
                        return false; // If any required field is empty, the object is not valid.
                    }
                }
                catch (Exception ex)
                {
                    // Handling an exception that occurred when trying to access a property or its value
                    // Console.WriteLine($"Error accessing property {field} on {card.GetType().Name}: {ex.Message}");
                }
            }
            return true;
        }
        private static bool IsDefaultValue(object value)
        {
            if (value is string stringValue)
            {
                return string.IsNullOrEmpty(stringValue);
            }
            else
            {
                var type = value.GetType();
                return value.Equals(Activator.CreateInstance(type));
            }
        }

        public static void AddNewObjectToDGV<T>(
            ref T newObject,
            BindingList<T> bindingList,
            List<T> newObjectsList,
            DataGridView dgv) where T : class, IDisplayedEntity, new()
        {
            if (newObject != null && !IsValidNewCard(newObject))
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля для уже созданного объекта.");
                return;
            }

            newObject = new T(); 
            newObjectsList.Add(newObject); 
            bindingList.Insert(0, newObject); 
           
            dgv.Refresh();
            // TODO: - ? highlight new row and all its required fields
        }

        public static void DeleteSelectedObject<T>(
            DataGridView dgvMain,
            BindingList<T> bindingList,
            List<T> newObjects,
            List<T> deletedObjects) where T : class, IDisplayedEntity, new()
        {
            if (dgvMain.SelectedRows.Count > 0)
            {
                string message = "Вы действительно хотите удалить выбранные объекты?\n";
                DialogResult result = MessageBox.Show(message, "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var selectedDTCs = dgvMain.SelectedRows.Cast<DataGridViewRow>()
                    .Select(row => row.DataBoundItem as T)
                    .Where(dtc => dtc != null)
                    .ToList();

                    foreach (var dtc in selectedDTCs)
                    {
                        bindingList.Remove(dtc);
                        deletedObjects.Add(dtc);

                        if (newObjects.Contains(dtc)) // if new card was deleted, remove it from new cards list
                        {
                            newObjects.Remove(dtc);
                        }
                    }
                }

                dgvMain.Refresh();
            }
        }
        

        public static void SetupDataGridView<T>(DataGridView dgv) where T : IDisplayedEntity, new()
        {
            var displayedobj = new T();

            WinProcessing.SetTableHeadersNames(displayedobj.GetPropertiesNames(), dgv);
            WinProcessing.SetTableColumnsOrder(displayedobj.GetPropertiesOrder(), dgv);

            //var propertiesOrder = displayedobj.GetPropertiesOrder();
            //var propertiesNames = displayedobj.GetPropertiesNames();

            //foreach (var propertyName in propertiesOrder)
            //{
            //    var propertyType = typeof(T).GetProperty(propertyName)?.PropertyType;

            //    if (propertyType == typeof(bool) || propertyType == typeof(bool?))
            //    {
            //        var column = new DataGridViewCheckBoxColumn
            //        {
            //            Name = propertyName,
            //            HeaderText = propertiesNames[propertyName],
            //            DataPropertyName = propertyName,
            //            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            //            TrueValue = true,
            //            FalseValue = false,
            //            IndeterminateValue = null // Adjust if necessary
            //        };
            //        dgv.Columns.Add(column);
            //    }
            //    else
            //    {
            //        dgv.Columns.Add(propertyName, propertiesNames[propertyName]);
            //    }
            //}
        }
        public static void ListChangedEventHandler<T>(ListChangedEventArgs e, BindingList<T> bindingList, List<T> newObjects, List<T> changedObjects, ref T newObject) where T : class, IDisplayedEntity, new()
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                if (newObject != null && e.NewIndex == 0) // if changed _newCard check if all required fields are filled
                {
                    if (!IsValidNewCard(newObject))
                    {
                        return;
                    }
                    newObject = null;
                }

                if (newObjects.Contains(bindingList[e.NewIndex])) // if changed new Objects don't add it to changed list
                {
                    return;
                }

                var changedItem = bindingList[e.NewIndex];
                if (!changedObjects.Contains(changedItem))
                {
                    changedObjects.Add(changedItem);
                }
            }
        }
        public static void ListChangedEventHandlerIntermediate(ListChangedEventArgs e, BindingList<DisplayedStaff_TC> bindingList, 
            List<DisplayedStaff_TC> newObjects, List<DisplayedStaff_TC> changedObjects, List<DisplayedStaff_TC> deletedObjects) 
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                if (newObjects.Contains(bindingList[e.NewIndex])) // if changed new Objects don't add it to changed list
                {
                    return;
                }

                var changedItem = bindingList[e.NewIndex];
                // check if changed Symbol property adn get old value

                if (e.PropertyDescriptor != null && e.PropertyDescriptor.Name == nameof(Staff_TC.Symbol))
                {
                    var oldValue = changedItem.GetOldValue(nameof(DisplayedStaff_TC.Symbol));
                    if (oldValue != null)
                    {
                        var deletedItem = new DisplayedStaff_TC
                        {
                            ChildId = changedItem.ChildId,
                            ParentId = changedItem.ParentId,
                            Symbol = oldValue.ToString(),
                        };

                        deletedObjects.Add(deletedItem);
                        newObjects.Add(changedItem);
                    }
                    return;
                }

                if (!changedObjects.Contains(changedItem))
                {
                    changedObjects.Add(changedItem);
                }
            }
        }
        public static void ListChangedEventHandlerIntermediate<T>(ListChangedEventArgs e, 
            BindingList<T> bindingList, List<T> newObjects, List<T> changedObjects, List<T> deletedObjects) 
            where T : class, IDisplayedEntity, new()
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                var changedItem = bindingList[e.NewIndex];

                //todo: ReorderRows remove from DGVprocessing to DisplayedEntityHelper in ListChangedEventHandlerIntermediate

                if (newObjects.Contains(changedItem)) // if changed new Objects don't add it to changed list
                {
                    return;
                }
                
                if (!changedObjects.Contains(changedItem))
                {
                    changedObjects.Add(changedItem);
                }
            }
        }

        //private static void OrderChangedEventHandler<T>(ListChangedEventArgs e, BindingList<T> bindingList, List<T> changedObjects) where T : class, IDisplayedEntity, IOrderable new()
        //{
        //    if (e.ListChangedType == ListChangedType.ItemChanged)
        //    {
        //        var changedItem = bindingList[e.NewIndex];

        //        // check if changed Order property and get old value
        //        if (e.PropertyDescriptor != null && e.PropertyDescriptor.Name == nameof(IOrderable.Order))
        //        {
        //            if (orderValue == e.RowIndex + 1) { return; }
        //            MoveRowAndUpdateOrder(_bindingList, e.RowIndex, orderValue - 1);

        //        }
        //        if (!changedObjects.Contains(changedItem))
        //        {
        //            changedObjects.Add(changedItem);
        //        }
        //    }
        //}

    }
}
