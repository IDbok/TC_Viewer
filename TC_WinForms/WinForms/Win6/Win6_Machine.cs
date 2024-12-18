using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.DGVProcessing;

namespace TC_WinForms.WinForms
{
	public partial class Win6_Machine : Form, IViewModeable
    {
        private readonly TcViewState _tcViewState;

        private bool _isViewMode;

        private int _tcId;
        private MyDbContext context;

        private BindingList<DisplayedMachine_TC> _bindingList;
        private List<DisplayedMachine_TC> _changedObjects = new ();
        private List<DisplayedMachine_TC> _newObjects = new ();
        private List<DisplayedMachine_TC> _deletedObjects = new ();

        private Dictionary<DisplayedMachine_TC, DisplayedMachine_TC> _replacedObjects = new();// add to UpdateMode

        public Win6_Machine(int tcId, TcViewState tcViewState, MyDbContext context)// bool viewerMode = false)
        {
            _tcViewState = tcViewState;
            this.context = context;

            _tcId = tcId;

            InitializeComponent();

            var dgvEventService = new DGVEvents(dgvMain);
            dgvEventService.SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);

            dgvMain.CellFormatting += dgvEventService.dgvMain_CellFormatting;
            dgvMain.CellValidating += dgvEventService.dgvMain_CellValidating;

            this.FormClosed += (sender, e) => this.Dispose();
        }

        public void SetViewMode(bool? isViewMode = null)
        {
            pnlControls.Visible = !_tcViewState.IsViewMode;

            // make columns editable
            dgvMain.Columns[nameof(DisplayedMachine_TC.Order)].ReadOnly = _tcViewState.IsViewMode;
            dgvMain.Columns[nameof(DisplayedMachine_TC.Quantity)].ReadOnly = _tcViewState.IsViewMode;


            dgvMain.Columns[nameof(DisplayedMachine_TC.Order)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
            dgvMain.Columns[nameof(DisplayedMachine_TC.Quantity)].DefaultCellStyle.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;

            // update form
            dgvMain.Refresh();

        }

        private void Win6_Machine_Load(object sender, EventArgs e)
        {
            LoadObjects();
            DisplayedEntityHelper.SetupDataGridView<DisplayedMachine_TC>(dgvMain);

            dgvMain.AllowUserToDeleteRows = false;

            SetViewMode();
        }
        private void LoadObjects()
        {
            var tcList = _tcViewState.TechnologicalCard.Machine_TCs.Select(obj => new DisplayedMachine_TC(obj)).ToList();
            _bindingList = new BindingList<DisplayedMachine_TC>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

            SetDGVColumnsSettings();
        }
        public void AddNewObjects(List<Machine> newObjs)
        {
            foreach (var obj in newObjs)
            {
                var newObj_TC = CreateNewObject(obj, _bindingList.Select(o => o.Order).Max() + 1);
                var machine = context.Machines.Where(s => s.Id == newObj_TC.ChildId).First();

                context.Machines.Attach(machine);
                _tcViewState.TechnologicalCard.Machine_TCs.Add(newObj_TC);

                newObj_TC.Child = machine;
                newObj_TC.ChildId = machine.Id;

                var displayedObj_TC = new DisplayedMachine_TC(newObj_TC);
                _bindingList.Add(displayedObj_TC);

            }
            dgvMain.Refresh();
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

            //// ширина столбцов по содержанию
            //var autosizeColumn = new List<string>
            //{
            //    nameof(DisplayedMachine_TC.Order),
            //    nameof(DisplayedMachine_TC.Name),
            //    nameof(DisplayedMachine_TC.Type),
            //    nameof(DisplayedMachine_TC.Unit),
            //    nameof(DisplayedMachine_TC.Quantity),

            //    nameof(DisplayedMachine_TC.ChildId),

            //};
            //foreach (var column in autosizeColumn)
            //{
            //    dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //}

            int pixels = 35;
            // Минимальные ширины столбцов
            Dictionary<string, int> fixColumnWidths = new Dictionary<string, int>
            {
                { nameof(DisplayedMachine_TC.Order), 1*pixels },
                { nameof(DisplayedMachine_TC.Type), 4*pixels },
                { nameof(DisplayedMachine_TC.Unit), 2*pixels },
                { nameof(DisplayedMachine_TC.Quantity), 3*pixels },
                { nameof(DisplayedMachine_TC.ChildId), 2*pixels },

            };
            foreach (var column in fixColumnWidths)
            {
                dgvMain.Columns[column.Key].Width = column.Value;
                dgvMain.Columns[column.Key].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvMain.Columns[column.Key].Resizable = DataGridViewTriState.False;
            }

            // make columns readonly
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                column.ReadOnly = true;
            }
            var changeableColumn = new List<string>
            {
                nameof(DisplayedMachine_TC.Order),
                nameof(DisplayedMachine_TC.Quantity),
            };
            foreach (var column in changeableColumn)
            {
                dgvMain.Columns[column].ReadOnly = false;
                dgvMain.Columns[column].DefaultCellStyle.BackColor = Color.LightGray;
            }
        }
        private void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Проверяем, что это не заголовок столбца и не новая строка
            if (e.RowIndex >= 0 && e.RowIndex < dgvMain.Rows.Count)
            {
                var row = dgvMain.Rows[e.RowIndex];
                var displayedStaff = row.DataBoundItem as DisplayedMachine_TC;
                if (displayedStaff != null)
                {
                    // Меняем цвет строки в зависимости от значения свойства IsReleased
                    if (!displayedStaff.IsReleased)
                    {
                        row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#d1c6c2"); // Цвет для строк, где IsReleased = false
                    }
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void SaveReplacedObjects() // add to UpdateMode
        {
            if (_replacedObjects.Count == 0)
                return;

            var oldObject = _replacedObjects.Select(dtc => CreateNewObject(dtc.Key)).ToList();
            var newObject = _replacedObjects.Select(dtc => CreateNewObject(dtc.Value)).ToList();

            var obj_TCsIds = oldObject.Select(t => t.ChildId).ToList();

            List<ExecutionWork> executionWorks = new List<ExecutionWork>();

            //Получаем список ExecutionWorks где будет происходить замена Machine
            foreach (var techOperationWork in _tcViewState.TechOperationWorksList)
            {
                var executionWorksToReplace = techOperationWork.executionWorks
                                                               .Where(ew => ew.techOperationWork.TechnologicalCardId == _tcId
                                                                         && ew.Machines.Any(m => obj_TCsIds.Contains(m.ChildId)))
                                                                .ToList();

                if(executionWorksToReplace != null && executionWorksToReplace.Count != 0)
                    executionWorks.AddRange(executionWorksToReplace);
            }

            //Присваиваем Machine новым объектам и удаляем старые объекты Machine_TCs под замену
            for (int i = 0; i < oldObject.Count; i++)
            {
                var mach = context.Machines.Where(m => m.Id == newObject[i].ChildId).First();
                newObject[i].Child = mach;

                var oldMach = _tcViewState.TechnologicalCard.Machine_TCs.Where(m => m.ChildId == oldObject[i].ChildId).FirstOrDefault();
               
                if (oldMach != null)
                {
                    context.Machine_TCs.Attach(oldMach);
                    _tcViewState.TechnologicalCard.Machine_TCs.Remove(oldMach);
                }
            }

            _tcViewState.TechnologicalCard.Machine_TCs.AddRange(newObject);

            foreach (var newTc in newObject)
            {
                executionWorks.ForEach(ew => ew.Machines.Add(newTc));
            }

            _replacedObjects.Clear();
        }
        private Machine_TC CreateNewObject(DisplayedMachine_TC dObj)
        {
            return new Machine_TC
            {
                ParentId = dObj.ParentId,
                ChildId = dObj.ChildId,
                Order = dObj.Order,
                Quantity = dObj.Quantity ?? 0,
                Note = dObj.Note,
            };
        }
        private Machine_TC CreateNewObject(Machine obj, int oreder)
        {
            return new Machine_TC
            {
                ParentId = _tcId,
                ChildId = obj.Id,
                Child = obj,
                Order = oreder,
                Quantity = 0,
                Note = "",
            };
        }

        ///////////////////////////////////////////////////// * Events handlers * /////////////////////////////////////////////////////////////////////////////////
        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            var newForm = new Win7_5_Machine(activateNewItemCreate: true, createdTCId: _tcId);
            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();
        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
                _bindingList, _newObjects, _deletedObjects);

            if (_deletedObjects.Count != 0)
            {
                var deletedObjects = _deletedObjects.Select(dtc => CreateNewObject(dtc)).ToList();

                foreach (var obj in deletedObjects)
                {
                    var deletedObj = _tcViewState.TechnologicalCard.Machine_TCs.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    _tcViewState.TechnologicalCard.Machine_TCs.Remove(deletedObj);
                }

                _deletedObjects.Clear();

            }

        }

        private void dgvMain_CellEndEdit(object sender, DataGridViewCellEventArgs e) // todo - fix problem with selection replacing row (error while remove it)
        {
            ReorderRows(dgvMain, e, _bindingList);
        }
        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            DisplayedEntityHelper.ListChangedEventHandlerIntermediate
                (e, _bindingList, _newObjects, _changedObjects, _deletedObjects);

            if (_changedObjects.Count != 0)
            {
                foreach (var obj in _changedObjects)
                {
                    var changedObject = _tcViewState.TechnologicalCard.Machine_TCs.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    if (changedObject != null)
                        changedObject.ApplyUpdates(CreateNewObject(obj));
                }
                _changedObjects.Clear();
            }
        }

        

        private void btnReplace_Click(object sender, EventArgs e)
        {
            // Выделение объекта выбранной строки
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return;
            }

            // load new form Win7_3_Component as dictonary
            var newForm = new Win7_5_Machine(activateNewItemCreate: true, createdTCId: _tcId, isUpdateMode: true);

            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();

        }// add to UpdateMode
        public bool UpdateSelectedObject(Machine updatedObject)
        {
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return false;
            }

            var selectedRow = dgvMain.SelectedRows[0];
            //var displayedComponent = selectedRow.DataBoundItem as DisplayedTool_TC;

            if (selectedRow.DataBoundItem is DisplayedMachine_TC dObj)
            {

                if (dObj.ChildId == updatedObject.Id)
                {
                    MessageBox.Show("Ошибка обновления объекта: ID объекта совпадает");
                    return false;
                }
                // проверка на наличие объекта в списке существующих объектов
                if (_bindingList.Any(obj => obj.ChildId == updatedObject.Id))
                {
                    MessageBox.Show("Ошибка обновления объекта: объект с таким ID уже существует");
                    return false;
                }
                var newItem = CreateNewObject(updatedObject, dObj.Order);
                newItem.Quantity = dObj.Quantity ?? 0;
                newItem.Note = dObj.Note;

                var newDisplayedComponent = new DisplayedMachine_TC(newItem);


                // замена displayedComponent в dgvMain на newDisplayedComponent
                var index = _bindingList.IndexOf(dObj);
                _bindingList[index] = newDisplayedComponent;

                // проверяем наличие объекта в списке измененных объектов в значениях replacedObjects
                if (_replacedObjects.ContainsKey(dObj))
                {
                    _replacedObjects[dObj] = newDisplayedComponent;
                }
                else
                {
                    _replacedObjects.Add(dObj, newDisplayedComponent);
                }

                SaveReplacedObjects();

                return true;
            }

            return false;
        }// add to UpdateMode
    }

}
