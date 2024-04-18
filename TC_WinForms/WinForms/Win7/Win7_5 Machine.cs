using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win7_5_Machine : Form, ISaveEventForm, ILoadDataAsyncForm
    {
        private DbConnector dbCon = new DbConnector();

        private BindingList<DisplayedMachine> _bindingList;
        private List<Machine> _objects = new List<Machine>();

        private List<DisplayedMachine> _changedObjects = new List<DisplayedMachine>();
        private List<DisplayedMachine> _newObjects = new List<DisplayedMachine>();
        private List<DisplayedMachine> _deletedObjects = new List<DisplayedMachine>();

        private DisplayedMachine _newObject;

        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;

        public bool _isDataLoaded = false;

        public bool CloseFormsNoSave { get; set; } = false;

        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }

        public Win7_5_Machine(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }
        public Win7_5_Machine()
        {
            InitializeComponent();
        }

        private async void Win7_5_Machine_Load(object sender, EventArgs e)
        {
            progressBar.Visible = true;

            if (!_isDataLoaded)
                await LoadDataAsync();

            SetDGVColumnsSettings();
            DisplayedEntityHelper.SetupDataGridView<DisplayedMachine>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            if (isAddingForm)
            {
                //isAddingFormSetControls();
                WinProcessing.SetAddingFormControls(pnlControlBtns, dgvMain,
                    out btnAddSelected, out btnCancel);
                SetAddingFormEvents();
            }

            progressBar.Visible = false;
        }
        public async Task LoadDataAsync()
        {
            var objList = await Task.Run(() => dbCon.GetObjectList<Machine>(includeLinks: true)
                .Select(obj => new DisplayedMachine(obj)).ToList());

            _bindingList = new BindingList<DisplayedMachine>(objList);

            dgvMain.DataSource = null; // cancel update of dgv while data is loading
            _bindingList.ListChanged += BindingList_ListChanged;

            dgvMain.DataSource = _bindingList;
            //SetLinkColumn();
            dgvMain.CellContentClick += dgvMain_CellContentClick;

            _isDataLoaded = true;
        }
        private async void Win7_5_Machine_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseFormsNoSave)
            {
                return;
            }

            if (_newObjects.Count + _changedObjects.Count + _deletedObjects.Count != 0)
            {
                //e.Cancel = true;
                var result = MessageBox.Show("Сохранить изменения перед закрытием?", "Сохранение", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    await SaveChanges();
                }
                //  e.Cancel = false;
                // Close();
            }
        }

        public bool GetDontSaveData()
        {
            if (_newObjects.Count + _changedObjects.Count + _deletedObjects.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void AccessInitialization(int accessLevel)
        {
        }

        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            //DisplayedEntityHelper.AddNewObjectToDGV(ref _newObject,
            //                _bindingList,
            //                _newObjects,
            //                dgvMain);
            var objEditor = new Win7_LinkObjectEditor(new Machine(), isNewObject: true);

            objEditor.AfterSave = async (createdObj) => AddNewObjectInDataGridView<Machine, DisplayedMachine>(createdObj as Machine);

            objEditor.ShowDialog();
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if(dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return;
            }

            var selectedObj = dgvMain.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            var obj = selectedObj?.DataBoundItem as DisplayedMachine;

            if (obj != null)
            {
                var machine = dbCon.GetObjectWithLinks<Machine>(obj.Id);

                if (machine != null)
                {
                    var objEditor = new Win7_LinkObjectEditor(machine);

                    objEditor.AfterSave = async (updatedObj) => UpdateObjectInDataGridView<Machine, DisplayedMachine>(updatedObj as Machine);

                    objEditor.ShowDialog();
                }
            }
        }

        private async void btnDeleteObj_Click(object sender, EventArgs e)
        {
            await DisplayedEntityHelper.DeleteSelectedObjectWithLinks<DisplayedMachine,Machine>(dgvMain,
                _bindingList);

            //await DisplayedEntityHelper.DeleteSelectedObjectAsync<DisplayedMachine,Machine>(dgvMain,
            //    _bindingList);

            //if (dgvMain.SelectedRows.Count > 0)
            //{
            //    string message = "Вы действительно хотите удалить выбранные объекты?\n";
            //    DialogResult result = MessageBox.Show(message, "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            //    if (result == DialogResult.Yes)
            //    {

            //        var selectedRows = dgvMain.SelectedRows.Cast<DataGridViewRow>()
            //            .Select(row => row.DataBoundItem as DisplayedMachine)
            //            .Where(obj => obj != null)
            //                .ToList();

            //        List<LinkEntety> linksToDelete = new();
            //        foreach (var obj in selectedRows)
            //        {
            //            linksToDelete.AddRange(obj!.Links);
            //        }

            //        var linkIds = linksToDelete.Select(l => l.Id).ToList();

            //        bool isLinksDeleted = await dbCon.DeleteObjectAsync<LinkEntety>(linkIds);

            //        if (!isLinksDeleted)
            //        {
            //            MessageBox.Show("Ошибка удаления ссылок");
            //            return;
            //        }
            //        var selectedRowIds = selectedRows.Select(obj => obj!.Id).ToList();

            //        bool isObjDeleted = await dbCon.DeleteObjectAsync<Machine>(selectedRowIds);

            //        if (isObjDeleted)
            //        {
            //            // Удаляем объекты из BindingList
            //            foreach (var obj in selectedRowIds)
            //            {
            //                var objToDelete = _bindingList.FirstOrDefault(o => o.Id == obj);
            //                if (objToDelete != null)
            //                {
            //                    _bindingList.Remove(objToDelete);
            //                }
            //            }
            //        }
            //    }

            //    dgvMain.Refresh();
            //}


            //var selectedRowIds = dgvMain.SelectedRows.Cast<DataGridViewRow>()
            //    .Select(row => row.DataBoundItem as DisplayedMachine)
            //    .Where(obj => obj != null)
            //    .Select(obj => obj!.Id)
            //    .ToList();

            //await dbCon.DeleteObjectAsync<Machine>(selectedRowIds);

            //// Удаляем объекты из BindingList
            //foreach (var obj in selectedRowIds)
            //{
            //    var objToDelete = _bindingList.FirstOrDefault(o => o.Id == obj);
            //    if (objToDelete != null)
            //    {
            //        _bindingList.Remove(objToDelete);
            //    }
            //}
        }
        /////////////////////////////////////////////// * SaveChanges * ///////////////////////////////////////////
        public bool HasChanges => _changedObjects.Count + _newObjects.Count + _deletedObjects.Count != 0;
        public async Task SaveChanges()
        {
            // stop editing cell
            dgvMain.EndEdit();
            if (!HasChanges)
            {
                return;
            }
            if (_newObjects.Count > 0)
            {
                await SaveNewObjects();
            }
            if (_changedObjects.Count > 0)
            {
                await SaveChangedObjects();
            }
            if (_deletedObjects.Count > 0)
            {
                await DeleteDeletedObjects();
            }
            // todo - change id in all new cards 
            dgvMain.Refresh();
        }


        private async Task SaveNewObjects()
        {
            var newObjects = _newObjects.Select(dtc => CreateNewObject(dtc)).ToList();

            await dbCon.AddObjectAsync(newObjects);

            // set new ids to new objects matched them by all params
            foreach (var newObj in _newObjects)
            {
                var newId = newObjects.Where(s =>
                s.Name == newObj.Name
                && s.Type == newObj.Type
                && s.Unit == newObj.Unit
                && s.Price == newObj.Price
                && s.Description == newObj.Description
                && s.Manufacturer == newObj.Manufacturer
                && s.Links == newObj.Links
                && s.ClassifierCode == newObj.ClassifierCode
                ).FirstOrDefault().Id;
                newObj.Id = newId;
            }


            _newObjects.Clear();
        }
        private async Task SaveChangedObjects()
        {
            var changedTcs = _changedObjects.Select(dtc => CreateNewObject(dtc)).ToList();

            await dbCon.UpdateObjectsListAsync(changedTcs);

            _changedObjects.Clear();
        }
        private async Task DeleteDeletedObjects()
        {
            var deletedTcIds = _deletedObjects.Select(dtc => dtc.Id).ToList();

            await dbCon.DeleteObjectAsync<Machine>(deletedTcIds);
            _deletedObjects.Clear();
        }
        private Machine CreateNewObject(DisplayedMachine dObj)
        {
            return new Machine
            {
                Id = dObj.Id,
                Name = dObj.Name,
                Type = dObj.Type,
                Unit = dObj.Unit,
                Price = dObj.Price,
                Description = dObj.Description,
                Manufacturer = dObj.Manufacturer,
                Links = dObj.Links,
                ClassifierCode = dObj.ClassifierCode,
            };
        }


        ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////

        void SetDGVColumnsSettings()
        {
            // автоподбор ширины столбцов под ширину таблицы
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.RowHeadersWidth = 25;

            //// автоперенос в ячейках
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            //    // ширина столбцов по содержанию
            var autosizeColumn = new List<string>
            {
                nameof(DisplayedMachine.Id),
                nameof(DisplayedMachine.Unit),
                nameof(DisplayedMachine.Type),
                //nameof(DisplayedMachine.ClassifierCode),
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            dgvMain.Columns[nameof(DisplayedMachine.Price)].Width = 120;
            dgvMain.Columns[nameof(DisplayedMachine.ClassifierCode)].Width = 150;
            dgvMain.Columns[nameof(DisplayedMachine.ClassifierCode)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvMain.Columns[nameof(DisplayedMachine.LinkNames)].Width = 100;
            dgvMain.Columns[nameof(DisplayedMachine.LinkNames)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        }

        private void SetLinkColumn()
        {
            // Добавление столбца ссылок, если он ещё не добавлен
            if (!dgvMain.Columns.Contains("DefaultLink"))
            {
                var linkColumn = new DataGridViewLinkColumn
                {
                    Name = "DefaultLink",
                    HeaderText = "Ссылки",
                    DataPropertyName = "DefaultLink",
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                    Width = 100,
                };
                linkColumn.UseColumnTextForLinkValue = true;
                dgvMain.Columns.Add(linkColumn);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////// * isAddingFormMethods and Events * ///////////////////////////////////////////

        void SetAddingFormEvents()
        {
            btnAddSelected.Click += BtnAddSelected_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        void BtnAddSelected_Click(object sender, EventArgs e)
        {
            // get selected rows
            var selectedRows = dgvMain.Rows.Cast<DataGridViewRow>().Where(r => Convert.ToBoolean(r.Cells["Selected"].Value) == true).ToList();
            if (selectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строки для добавления");
                return;
            }
            // get selected objects
            var selectedObjs = selectedRows.Select(r => r.DataBoundItem as DisplayedMachine).ToList();
            var newItems = new List<Machine>();
            foreach (var obj in selectedObjs)
            {
                newItems.Add(CreateNewObject(obj));
            }
            // find opened form
            var tcEditor = Application.OpenForms.OfType<Win6_Machine>().FirstOrDefault();

            tcEditor.AddNewObjects(newItems);

            // close form
            this.Close();
        }
        void BtnCancel_Click(object sender, EventArgs e)
        {
            // close form
            this.Close();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                if (_newObject != null && e.NewIndex == 0) // if changed _newCard check if all required fields are filled
                {
                    if (!DisplayedEntityHelper.IsValidNewCard(_newObject))
                    {
                        return;
                    }
                    _newObject = null;
                }

                if (_newObjects.Contains(_bindingList[e.NewIndex])) // if changed new Objects don't add it to changed list
                {
                    return;
                }

                var changedItem = _bindingList[e.NewIndex];
                if (!_changedObjects.Contains(changedItem))
                {
                    _changedObjects.Add(changedItem);
                }
            }
        }


        private class DisplayedMachine : INotifyPropertyChanged, IDisplayedEntity, IModelStructure
        {
            public Dictionary<string, string> GetPropertiesNames()
            {
                return new Dictionary<string, string>
            {
                { nameof(Id), "ID" },
                { nameof(Name), "Наименование" },
                { nameof(Type), "Тип (исполнение)" },
                { nameof(Unit), "Ед.изм." },
                { nameof(ClassifierCode), "Код в classifier" },
                //{ nameof(Price), "Стоимость, руб. без НДС" },
                { nameof(Description), "Описание" },
                { nameof(Manufacturer), "Производитель" },
                //{ nameof(Links), "Ссылки" },
                { nameof(LinkNames), "Ссылка" },
            };
            }
            public List<string> GetPropertiesOrder()
            {
                return new List<string>
                {
                    nameof(Id),
                    nameof(Name),
                    nameof(Type),
                    nameof(Unit),
                    nameof(ClassifierCode),
                    //nameof(Price),
                    nameof(Description),
                    nameof(Manufacturer),
                    nameof(LinkNames),
                };
            }
            public List<string> GetRequiredFields()
            {
                return new List<string>
                {
                    nameof(Name) ,
                    nameof(Type) ,
                    nameof(Unit) ,
                    nameof(ClassifierCode) ,
                };
            }

            private int id;
            private string name;
            private string? type;
            private string unit;
            private float? price;
            private string? description;
            private string? manufacturer;
            private List<LinkEntety> links = new();
            private string classifierCode;

            private string GetDefaultLinkOrFirst() 
            {
                if(links.Count > 0)
                {
                    // Все названия существующих ссылок с новой строки
                    //var linksNames = string.Join("\n", links.Select(l => l.Name).ToList());
                    var defLink = links.Where(l => l.IsDefault).FirstOrDefault();
                    if(defLink == null)
                    {
                        return links[0].Name ?? links[0].Link;
                    }
                    //defLink.Name = linksNames;
                    return defLink.Name ?? defLink.Link;
                }
                return string.Empty;
            }
            public DisplayedMachine()
            {

            }
            public DisplayedMachine(Machine obj)
            {
                Id = obj.Id;
                Name = obj.Name;
                Type = obj.Type;
                Unit = obj.Unit;
                Price = obj.Price;
                Description = obj.Description;
                Manufacturer = obj.Manufacturer;
                Links = obj.Links;
                ClassifierCode = obj.ClassifierCode;
            }


            public int Id { get; set; }

            public string Name
            {
                get => name;
                set
                {
                    if (name != value)
                    {
                        name = value;
                        OnPropertyChanged(nameof(Name));
                    }
                }
            }

            public string Type
            {
                get => type;
                set
                {
                    if (type != value)
                    {
                        type = value;
                        OnPropertyChanged(nameof(Type));
                    }
                }
            }
            public string Unit
            {
                get => unit;
                set
                {
                    if (unit != value)
                    {
                        unit = value;
                        OnPropertyChanged(nameof(unit));
                    }
                }
            }
            public float? Price
            {
                get => price;
                set
                {
                    if (price != value)
                    {
                        price = value;
                        OnPropertyChanged(nameof(Price));
                    }
                }
            }
            public string Description
            {
                get => description;
                set
                {
                    if (description != value)
                    {
                        description = value;
                        OnPropertyChanged(nameof(Description));
                    }
                }
            }
            public string Manufacturer
            {
                get => manufacturer ?? string.Empty;
                set
                {
                    if (manufacturer != value)
                    {
                        manufacturer = value;
                        OnPropertyChanged(nameof(Manufacturer));
                    }
                }
            }
            public string LinkNames
            {
                get => GetDefaultLinkOrFirst();
            }

            public List<LinkEntety> Links
            {
                get => links;
                set
                {
                    if (links != value)
                    {
                        links = value;
                        OnPropertyChanged(nameof(Links));
                    }
                }
            }
            public string ClassifierCode
            {
                get => classifierCode;
                set
                {
                    if (classifierCode != value)
                    {
                        classifierCode = value;
                        OnPropertyChanged(nameof(ClassifierCode));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterTechnologicalCards();
        }
        private void FilterTechnologicalCards()
        {
            try
            {
                var searchText = txtSearch.Text == "Поиск" ? "" : txtSearch.Text;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    dgvMain.DataSource = _bindingList; // Возвращаем исходный список, если строка поиска пуста
                }
                else
                {
                    var filteredList = _bindingList.Where(obj =>
                            (obj.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.Type?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.Unit?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (obj.ClassifierCode?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                        ).ToList();

                    dgvMain.DataSource = new BindingList<DisplayedMachine>(filteredList);
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }

        }

        private void dgvMain_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            UrlClick(sender, e);
        }
        private void UrlClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvMain.Columns[e.ColumnIndex].Name == nameof(DisplayedMachine.LinkNames) && e.RowIndex >= 0)
            {
                var displayedMachine = dgvMain.Rows[e.RowIndex].DataBoundItem as DisplayedMachine;
                if (displayedMachine != null)
                {
                    var link = displayedMachine.Links.FirstOrDefault(l => l.IsDefault) ?? displayedMachine.Links.FirstOrDefault();
                    if (link != null && Uri.TryCreate(link.Link, UriKind.Absolute, out var uri))
                    {
                        var result = MessageBox.Show($"Открыть ссылку в браузере?\n{link}", "Ссылка", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
                        }
                    }
                }
            }
        }
        public void UpdateObjectInDataGridView<TModel,TDisplayed>(TModel modelObject)
            where TModel : IModelStructure
            where TDisplayed : class, IModelStructure
        {
            // Обновляем объект в DataGridView
            var displayedObject = _bindingList.OfType<TDisplayed>().FirstOrDefault(obj => obj.Id == modelObject.Id);
            if (displayedObject != null)
            {
                displayedObject.Name = modelObject.Name;
                displayedObject.Type = modelObject.Type;
                displayedObject.Unit = modelObject.Unit;
                displayedObject.Price = modelObject.Price;
                displayedObject.Description = modelObject.Description;
                displayedObject.Manufacturer = modelObject.Manufacturer;
                displayedObject.Links = modelObject.Links;
                displayedObject.ClassifierCode = modelObject.ClassifierCode;

                dgvMain.Refresh();
            }
        }

        public void AddNewObjectInDataGridView<TModel, TDisplayed>(TModel modelObject)
            where TModel : IModelStructure
            where TDisplayed : class, IModelStructure
        {
            var newDisplayedObject = Activator.CreateInstance<TDisplayed>();
            if (newDisplayedObject is DisplayedMachine displayedObject)
            {
                displayedObject.Id = modelObject.Id;
                displayedObject.Name = modelObject.Name;
                displayedObject.Type = modelObject.Type;
                displayedObject.Unit = modelObject.Unit;
                displayedObject.Price = modelObject.Price;
                displayedObject.Description = modelObject.Description;
                displayedObject.Manufacturer = modelObject.Manufacturer;
                displayedObject.Links = modelObject.Links;
                displayedObject.ClassifierCode = modelObject.ClassifierCode;
                if (displayedObject is ICategoryable objectWithCategory && modelObject is ICategoryable modelWithCategory)
                {
                    objectWithCategory.Categoty = modelWithCategory.Categoty;
                }

                _bindingList.Insert(0, displayedObject);
            }
        }
    }
}
