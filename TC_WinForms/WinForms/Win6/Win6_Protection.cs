using Serilog;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
	// при работе с дизайнером раскоментировать
	//[DesignerCategory("Form")]
	//public partial class Win6_Protection : Win6_Protection_Design
	public partial class Win6_Protection : BaseContentForm<DisplayedProtection_TC, Protection_TC>, IFormWithObjectId
	{		protected override DataGridView DgvMain => dgvMain;
		protected override Panel PnlControls => pnlControls;
		protected override IList<Protection_TC> TargetTable
			=> _tcViewState.TechnologicalCard.Protection_TCs;

		private MyDbContext context;
        private int _tcId;

        public Win6_Protection(int tcId, TcViewState tcViewState, MyDbContext context)// bool viewerMode = false)
        {
			if (DesignMode)
				return;

			_tcViewState = tcViewState;
			this.context = context;

			_tcId = tcId;

			_logger = Log.Logger
				.ForContext<Win6_Protection>()
				.ForContext("TcId", _tcId);

			_logger.Information("Инициализация формы. TcId={TcId}");

            InitializeComponent();

            InitializeDataGridViewEvents();
            dgvMain.CellEndEdit += DgvMain_CellEndEdit;
            this.FormClosed += (sender, e) => {
				_logger.Information("Форма закрыта");
				this.Dispose();
			};
		}

        private bool autoAdvance = false;

        private void DgvMain_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            base.dgvMain_CellEndEdit(sender, e);

            if (!autoAdvance) return;

            int currentRow = dgvMain.CurrentCell.RowIndex;
            int currentCol = dgvMain.CurrentCell.ColumnIndex;
            int nextRow = currentRow + 1;

            if (nextRow < dgvMain.Rows.Count)
            {
                this.BeginInvoke(new Action(() =>
                {
                    dgvMain.EndEdit();
                    autoAdvance = true;
                    dgvMain.CurrentCell = dgvMain.Rows[nextRow].Cells[currentCol];
                    dgvMain.BeginEdit(true);
                }));
            }

            autoAdvance = false;
        }

        protected override void LoadObjects()
        {
            var tcList = TargetTable
                .OrderBy(o => o.Order)
                .Select(obj => new DisplayedProtection_TC(obj))
                .ToList();

            _bindingList = new BindingList<DisplayedProtection_TC>(tcList);
            _bindingList.ListChanged += BindingList_ListChanged;
            dgvMain.DataSource = _bindingList;

			InitializeDataGridViewColumns();
		}

        public void AddNewObjects(List<Protection> newObjs)
        {
            List<Protection> existedProtection = new List<Protection>();

            foreach (var obj in newObjs)
            {
                if (_bindingList.Select(c => c.ChildId).Contains(obj.Id))
                {
                    existedProtection.Add(obj);
                    continue;
                }

                var newObj_TC = CreateNewObject(obj, GetNewObjectOrder());
                var protection = context.Protections.Where(s => s.Id == newObj_TC.ChildId).First();

                context.Protections.Attach(protection);
                TargetTable.Add(newObj_TC);

                newObj_TC.Child = protection;
                newObj_TC.ChildId = protection.Id;

                var displayedObj_TC = new DisplayedProtection_TC(newObj_TC);
                _bindingList.Add(displayedObj_TC);
            }

            dgvMain.Refresh();

            if (existedProtection.Count > 0)
            {
                string elements = "";
                foreach (var protection in existedProtection)
                {
                    elements += "Средство защиты: " + protection.Name + " ID: " + protection.Id + ".\n";
                }

                MessageBox.Show("Часть объектов уже присутствовала в требованиях. Они не были внесены: \n" + elements, "Дублирование элементов", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

		////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		protected override void SaveReplacedObjects() // add to UpdateMode
		{
			if (_replacedObjects.Count == 0)
				return;

			var oldObjects = _replacedObjects.Select(dtc => CreateNewObject(dtc.Key)).ToList();
            var newObjects = _replacedObjects.Select(dtc => CreateNewObject(dtc.Value)).ToList();

            var obj_TCsIds = oldObjects.Select(t => t.ChildId).ToList();

            List<ExecutionWork> executionWorks = new List<ExecutionWork>();

            foreach (var techOperationWork in _tcViewState.TechOperationWorksList)
            {
                var executionWorksToReplace = techOperationWork.executionWorks
                                                               .Where(ew => ew.techOperationWork.TechnologicalCardId == _tcId
                                                                         && ew.Protections.Any(m => obj_TCsIds.Contains(m.ChildId)))
                                                                .ToList();

                if (executionWorksToReplace != null && executionWorksToReplace.Count != 0)
                    executionWorks.AddRange(executionWorksToReplace);
            }

            for (int i = 0; i < newObjects.Count; i++)
            {
                var protection = context.Protections.Where(m => m.Id == newObjects[i].ChildId).First();
                newObjects[i].Child = protection;

                var oldProtection = TargetTable.Where(m => m.ChildId == oldObjects[i].ChildId).FirstOrDefault();
                if(oldProtection != null)
                TargetTable.Remove(oldProtection);
            }

			foreach (var newObj in newObjects)
			{
				TargetTable
					.Add(newObj);
			}

			foreach (var newTc in newObjects)
            {
                executionWorks.ForEach(ew => ew.Protections.Add(newTc));
            }


            _replacedObjects.Clear();
        }
		protected override Protection_TC CreateNewObject(BaseDisplayedEntity dObj)
        {
            return new Protection_TC
            {
                ParentId = dObj.ParentId,
                ChildId = dObj.ChildId,
                Order = dObj.Order,
                Quantity = dObj.Quantity ?? 0,
                Note = dObj.Note,
                Formula = dObj.Formula,
			};
        }
        private Protection_TC CreateNewObject(Protection obj, int oreder)
        {
            return new Protection_TC
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
			LogUserAction("Добавление нового объекта");

			var newForm = new Win7_7_Protection(activateNewItemCreate: true, createdTCId: _tcId);
            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();
        }
        private void btnDeleteObj_Click(object sender, EventArgs e)
		{
			LogUserAction("Удаление выбранных объектов");

			DisplayedEntityHelper.DeleteSelectedObject(dgvMain,
                _bindingList, _newObjects, _deletedObjects);

            if (_deletedObjects.Count != 0)
            {
                foreach (var obj in _deletedObjects)
                {
                    foreach (var techOperation in _tcViewState.TechOperationWorksList)
                    {
                        foreach (var executionWork in techOperation.executionWorks)
                        {
                            var protectionToDelete = executionWork.Protections.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                            if (protectionToDelete != null)
                                executionWork.Protections.Remove(protectionToDelete);
                        }

                    }


                    var deletedObj = TargetTable.Where(s => s.ChildId == obj.ChildId).FirstOrDefault();
                    if(deletedObj != null)
                    {
						_logger.Debug("Удаление объектов({ObjectType}}: {ComponentName} (ID={Id})",
							deletedObj.GetType(), deletedObj.Child.Name, deletedObj.ChildId);

						TargetTable.Remove(deletedObj);
					}
                }

                _deletedObjects.Clear();
            }
            ReorderRows();
        }
        private void ReorderRows()
        {
            // Если у вас есть поле с порядком (например, Order или Sequence)
            int order = 1;
            foreach (var item in _bindingList)
            {
                // Предполагая, что у вашего объекта есть свойство Order
                // Если свойство называется иначе, измените соответственно
                item.Order = order++;

                // Если нужно обновить порядок и в TargetTable
                var targetItem = TargetTable.FirstOrDefault(t => t.ChildId == item.ChildId);
                if (targetItem != null)
                {
                    targetItem.Order = item.Order;
                }
            }

            // Обновляем отображение DataGridView
            _bindingList.ResetBindings();

            // Или можно использовать:
            // dgvMain.Refresh();
        }

        private void btnReplace_Click(object sender, EventArgs e)
		{
			LogUserAction("Замена выбранных объектов");
			// Выделение объекта выбранной строки
			if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return;
            }

            // load new form Win7_3_Component as dictonary
            var newForm = new Win7_7_Protection(activateNewItemCreate: true, createdTCId: _tcId, isUpdateMode: true);

            newForm.WindowState = FormWindowState.Maximized;
            newForm.ShowDialog();

        }// add to UpdateMode
        public bool UpdateSelectedObject(Protection updatedObject)
        {
            if (dgvMain.SelectedRows.Count != 1)
            {
                MessageBox.Show("Выберите одну строку для редактирования");
                return false;
            }

            var selectedRow = dgvMain.SelectedRows[0];
            //var displayedComponent = selectedRow.DataBoundItem as DisplayedTool_TC;

            if (selectedRow.DataBoundItem is DisplayedProtection_TC dObj)
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

                var newDisplayedComponent = new DisplayedProtection_TC(newItem);


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

		public int GetObjectId()
		{
			return _tcId;
		}
	}

}
