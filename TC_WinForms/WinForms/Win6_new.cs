using Microsoft.EntityFrameworkCore;
using Nancy.Validation.Rules;
using Serilog;
using System.Windows.Forms.Integration;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.ExceptionHandler;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Diagram;
using TC_WinForms.WinForms.PrinterSettings;
using TC_WinForms.WinForms.Win6;
using TC_WinForms.WinForms.Win6.ImageEditor;
using TC_WinForms.WinForms.Win6.Models;
using TC_WinForms.WinForms.Win6.RoadMap;
using TC_WinForms.WinForms.Work;
using TcDbConnector;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TcModels.Models.TechnologicalCard;
using Timer = System.Timers.Timer;

namespace TC_WinForms.WinForms
{// todo: загрузить данные о переходах из других ТК
    public partial class Win6_new : Form, IViewModeable, IFormWithObjectId
    {
        private readonly ILogger _logger;
        private TcViewState tcViewState;
        private Timer? AutosaveTimer;
        private bool isFormAutosave = false;
        private int TimerInterval = 1000 * 60 * 25;//интервал работы таймера в милисекундах(25 минут)
        public readonly Guid FormGuid; // создан для проверки работы с одинаковым контекстом
        private ConcurrencyBlockService<TechnologicalCard> concurrencyBlockServise;
        private EModelType startOpenForm = EModelType.WorkStep;
        //private static bool _isViewMode = true;
        //private static bool _isCommentViewMode = false;
        private static bool _isMachineCollumnViewMode = true;
        public static bool isMachineViewMode
        {
            get => _isMachineCollumnViewMode;
            set
            {
                if (_isMachineCollumnViewMode != value)
                {
                    _isMachineCollumnViewMode = value;
                    OnCommentViewModeChanged();
                }
            }
        }

        public static event Action? CommentViewModeChanged;
        public static event Action? ViewModeChanged;
        public static event Action? TCStatusModedChanged;

        private static void OnCommentViewModeChanged()
        {
            CommentViewModeChanged?.Invoke();
        }
        private static void OnViewModeChanged()
        {
            ViewModeChanged?.Invoke();
        }

        private User.Role _accessLevel;

        private Dictionary<EModelType, Form> _formsCache = new Dictionary<EModelType, Form>();
        private EModelType? _activeModelType = null;
        private Form _activeForm = null;

        //TechOperationForm techOperationForm;

        EModelType? activeModelType = null;
        private TechnologicalCard _tc = new TechnologicalCard();
        private int _tcId;
        private DbConnector db = new DbConnector();
        private MyDbContext context = new MyDbContext();
        private OutlayService calculateOutlayService = new OutlayService();
        private bool TcWasBlocked = false;
        private bool OptimicticError = false;
        private int timerInterval = 1000 * 60 * 14;//миллисекунды * секунды * минуты
        

        public Win6_new(int tcId, User.Role role = User.Role.Lead, bool viewMode = false, EModelType startForm = EModelType.WorkStep)
        {
            FormGuid = Guid.NewGuid();

            _tcId = tcId;
            _accessLevel = role;
            startOpenForm = startForm;
            concurrencyBlockServise = new ConcurrencyBlockService<TechnologicalCard>(_tc.GetType().Name, _tcId, timerInterval);

            _logger = Log.Logger.ForContext<Win6_new>();
            _logger.Information("Инициализация Win6_new: TcId={TcId}, Role={Role}, ViewMode={ViewMode}", tcId, role, viewMode);

            tcViewState = new TcViewState(role, this);
            tcViewState.IsViewMode = viewMode;

            InitializeComponent();

            this.KeyDown += ControlSaveEvent;

            FormClosed += (s, e) => ThisFormClosed();
        }

        private void UpdateDynamicCardParametrs()
        {
            toolStripShowCoefficients.Visible = tcViewState.IsViewMode ? false : _tc.IsDynamic;

            UpdateIsDynamicButtonText();
        }

        private void TrySaveRoadMapDataInViewMode()
        {
            if (_accessLevel == User.Role.Lead && tcViewState.IsViewMode && tcViewState.RoadmapInfo.IsRoadMapUpdate)
            {
                SaveRoadMapData();
                context.SaveChanges();

                MessageBox.Show("Данные дорожной карты сохранены");
            }
        }

        private void ThisFormClosed()
        {
            _logger.Information("Закрытие формы. Очистка временных файлов для TcId={TcId}", _tc.Id);

            TrySaveRoadMapDataInViewMode();
            TempFileCleaner.CleanUpTempFiles(TempFileCleaner.GetTempFilePath(_tc.Id));
            concurrencyBlockServise.CleanBlockData();

            var a = CheckOpenFormService.FindOpenedForms<Win6_new>(_tc.Id);
            foreach (var f in a)
                f.Close();


            Dispose();


        }
        private void AccessInitialization()
        {
            var controlAccess = new Dictionary<User.Role, Action>
            {
                //[User.Role.Lead] = () => { },

                [User.Role.Implementer] = () =>
                {
                    setApprovedStatusToolStripMenuItem.Visible = false;
                    //setApprovedStatusToolStripMenuItem.Enabled = false;
                    if (_tc.Status != TechnologicalCardStatus.Remarked)
                    {
                        setRemarksModeToolStripMenuItem.Enabled = false;
                    }
                    ChangeIsDynamicToolStripMenuItem.Visible = false;

                },

                [User.Role.ProjectManager] = () =>
                {
                    SetOnlyViewModeRoleAccess();
                },

                [User.Role.User] = () =>
                {
                    SetOnlyViewModeRoleAccess();
                }
            };

            controlAccess.TryGetValue(_accessLevel, out var action);
            action?.Invoke();
        }
        private void SetOnlyViewModeRoleAccess()
        {
            updateToolStripMenuItem.Visible = false;
            if (!tcViewState.IsViewMode)
            {
                MessageBox.Show("Доступен только режим просмотра!");
                SetViewMode(true);
            }
            SaveChangesToolStripMenuItem.Visible = false;
            updateToolStripMenuItem.Visible = false;
            actionToolStripMenuItem.Visible = false;
            setRemarksModeToolStripMenuItem.Visible = false;
            ChangeIsDynamicToolStripMenuItem.Visible = false;
        }
        private void SetTCStatusAccess()
        {
            var controlAccess = new Dictionary<TechnologicalCardStatus, Action>
            {

                [TechnologicalCardStatus.Approved] = () =>
                {
                    actionToolStripMenuItem.Visible = false;
                    setRemarksModeToolStripMenuItem.Visible = false;
                    updateToolStripMenuItem.Visible = false;

                    tcViewState.IsViewMode = true;
                    // SaveChangesToolStripMenuItem.Enabled = false;
                },

                [TechnologicalCardStatus.Remarked] = () =>
                {
                    setDraftStatusToolStripMenuItem.Enabled = false;
                },

                [TechnologicalCardStatus.Draft] = () =>
                {
                    setDraftStatusToolStripMenuItem.Enabled = false;
                },

                [TechnologicalCardStatus.Created] = () =>
                {
                    //setApprovedStatusToolStripMenuItem.Enabled = false;
                    setRemarksModeToolStripMenuItem.Visible = false;
                }
            };

            controlAccess.TryGetValue(_tc.Status, out var action);
            action?.Invoke();

        }
        public int GetObjectId()
        {
            return _tcId;
        }
        public void SetViewMode(bool? isViewMode = null)
        {
            var usedStatus = concurrencyBlockServise.GetObjectUsedStatus();
            if (usedStatus)
            {
                tcViewState.IsViewMode = true;
                TcWasBlocked = true;
                MessageBox.Show("Сейчас карта используется другим пользователем. Она доступна только для просмотра.");
            }
            else if (isViewMode != null && tcViewState.IsViewMode != isViewMode)
            {
                tcViewState.IsViewMode = (bool)isViewMode;
                _logger.Information("Изменен режим просмотра: TcId={TcId}, IsViewMode={IsViewMode}", _tc.Id, isViewMode);
                if (!tcViewState.IsViewMode)
                {
                    if(TcWasBlocked)
                    {
                        var newForm = new Win6_new(_tcId, _accessLevel, tcViewState.IsViewMode, startOpenForm);
                        newForm.Show();
                        MessageBox.Show("Карта была  перезагружена, так как была обнаружена блокировка данных другим пользователем. Карта обновлена до последней версии.", "Данные обновлены", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                        concurrencyBlockServise.BlockObject();
                }
            }
            else if(isViewMode == null && !usedStatus)
                concurrencyBlockServise.BlockObject();

            SaveChangesToolStripMenuItem.Visible = !tcViewState.IsViewMode;

            updateToolStripMenuItem.Text = tcViewState.IsViewMode ? "Редактировать" : "Просмотр";
            actionToolStripMenuItem.Visible = !tcViewState.IsViewMode;

            setAutoSaveSettings.Visible = !tcViewState.IsViewMode;
            if(tcViewState.IsViewMode && isFormAutosave)
            {
                isFormAutosave = false;
                AutosaveTimer?.Dispose();
                setAutoSaveSettings.Text = "Включить автосохранение";
                MessageBox.Show("Вы перешли в режим просмотра, автосохранение выключено.", "Автосохранение",MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            foreach (var form in _formsCache.Values)
            {
                // is form is ISaveEventForm
                if (form is IViewModeable cashForms)
                {
                    cashForms.SetViewMode(tcViewState.IsViewMode);  // todo: добавить изменение режима отображения коэффициентов
                }
            }

            UpdateDynamicCardParametrs();
        }

        #region SetTcData

        private async Task<TechnologicalCard> GetTCDataAsync() // зачем остались данные методы, если они не используются?
        {
            _logger.Information("Загрузка данных технологической карты для TcId={TcId}", _tcId);
            // зафиксировать время начала загрузки
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var techCard = await context.TechnologicalCards
                    .FirstAsync(t => t.Id == _tcId);


                // 2. Загружаем все связанные данные отдельными запросами

                // Machine_TCs
                var machineTcs = await context.Machine_TCs
                    .Where(m => m.ParentId == _tcId).Include(m => m.Child)
                    .ToListAsync();

                // Protection_TCs
                var protectionTcs = await context.Protection_TCs
                    .Where(pt => pt.ParentId == _tcId).Include(m => m.Child)
                    .ToListAsync();

                // Tool_TCs
                var toolTcs = await context.Tool_TCs
                    .Where(tt => tt.ParentId == _tcId).Include(m => m.Child)
                    .ToListAsync();

                // Component_TCs
                var componentTcs = await context.Component_TCs
                    .Where(ct => ct.ParentId == _tcId).Include(m => m.Child)
                    .ToListAsync();

                // Staff_TCs
                var staffTcs = await context.Staff_TCs
                    .Where(st => st.ParentId == _tcId).Include(m => m.Child)
                    .ToListAsync();

                // Coefficient
                var coefficients = await context.Coefficients
                    .Where(c => c.TechnologicalCardId == _tcId)
                    .ToListAsync();

                _logger.Information("Загружены данные технологической карты: {TcName}(id: {TcId})", techCard.Name, _tcId);
                return techCard;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке данных технологической карты для TcId={TcId}", _tcId);
                throw;
            }
            finally
            {
                // остановить таймер и вывести время выполнения
                stopwatch.Stop();
                _logger.Information("Время загрузки данных технологической карты: {Time} мс", stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task<List<TechOperationWork>> GetTOWDataAsync()
        {
            _logger.Information("Загрузка данных технологических операций для TcId={TcId}", _tcId);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();


            try
            {
                var techOperationWorkList = await context.TechOperationWorks.Where(w => w.TechnologicalCardId == _tcId)
                       .Include(i => i.techOperation)
                       .ToListAsync();

                //список ID Технологических операций
                var towIds = techOperationWorkList.Select(t => t.Id).ToList();

                //Получаем список всех компонентов которые принадлежат карте
                var componentWorks = await context.ComponentWorks.Where(c => towIds.Any(o => o == c.techOperationWorkId))
                   .Include(t => t.component)
                   .ToListAsync();

                //Получаем список всех инструментов, которые принадлежат карте
                var toolWorks = await context.ToolWorks.Where(c => towIds.Any(o => o == c.techOperationWorkId))
                    .Include(t => t.tool)
                    .ToListAsync();

                //Получаем список всех ExecutionWorks для технологической карты
                var executionWorks = await
                   context.ExecutionWorks.Where(e => towIds.Any(o => o == e.techOperationWorkId))
                                         .Include(e => e.techTransition)
                                         .Include(e => e.Protections)
                                         .Include(e => e.Machines)
                                         .Include(e => e.Staffs)
                                         .Include(e => e.ExecutionWorkRepeats)
                                         .ToListAsync();

                _logger.Information("Загружены данные технологических операций для TcId={TcId}", _tcId);
                return techOperationWorkList;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке данных технологических операций для TcId={TcId}", _tcId);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.Information("Время загрузки данных технологических операций: {Time} мс", stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task<List<DiagamToWork>> GetDTWDataAsync()
        {
            _logger.Information("Загрузка данных диаграмм для TcId={TcId}", _tcId);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var diagramToWorkList = await context.DiagamToWork.Where(w => w.technologicalCardId == _tcId).ToListAsync();


                var listDiagramParalelno = await context.DiagramParalelno.Where(p => diagramToWorkList.Select(i => i.Id).Contains(p.DiagamToWorkId))
                                                                         .Include(ie => ie.techOperationWork)
                                                                         .ToListAsync();

                var listDiagramPosledov = await context.DiagramPosledov.Where(p => listDiagramParalelno.Select(i => i.Id).Contains(p.DiagramParalelnoId))
                    .ToListAsync();

                var listDiagramShag = await context.DiagramShag.Where(d => listDiagramPosledov.Select(i => i.Id).Contains(d.DiagramPosledovId))
                    .Include(q => q.ListDiagramShagToolsComponent)
                    .ToListAsync();

                _logger.Information("Загружены данные диаграмм для TcId={TcId}", _tcId);
                return diagramToWorkList;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке данных диаграмм для TcId={TcId}", _tcId);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.Information("Время загрузки данных диаграмм: {Time} мс", stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task SetTcViewStateData()
        {

            try
            {
                var rep = new TechnologicalCardRepository();

                tcViewState.TechnologicalCard = await rep.GetTCDataAsync(_tcId, context) ?? throw new Exception("Не получилось загрузить данные ТК.");
                tcViewState.TechOperationWorksList = tcViewState.TechnologicalCard.TechOperationWorks;
                tcViewState.DiagramToWorkList = await rep.GetDTWDataAsync(_tcId, context);

                _logger.Information("Данные для TcViewState успешно загружены для TcId={TcId}", _tcId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке данных для TcViewState для TcId={TcId}", _tcId);
                throw;
            }

        }
        #endregion

        private async void Win6_new_Load(object sender, EventArgs e)
        {
            _logger.Information("Загрузка формы Win6_new для TcId={TcId}", _tcId);
            try
            {
                // Блокировка формы при загрузки данных
                this.Enabled = false;

                await SetTcViewStateData();
                _tc = tcViewState.TechnologicalCard;

                SetTagsToButtons();
                AccessInitialization();
                SetTCStatusAccess();
                UpdateFormTitle();

                SetViewMode();
                UpdateDynamicCardParametrs();

                await ShowForm(startOpenForm);

                _logger.Information("Форма загружена успешно для TcId={TcId}", _tcId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке данных формы для TcId={TcId}", _tcId);
                MessageBox.Show(ex.Message);
                this.Close();
            }
            finally
            {
                if (this != null && !this.IsDisposed && this.IsHandleCreated) // Проверяем, что форма не уничтожена
                {
                    this.Enabled = true;
                }
            }
        }
        private void UpdateFormTitle()
        {
            this.Text = $"{_tc.Name} ({_tc.Article})";
            if (_tc.Status != TechnologicalCardStatus.Approved)
            {
                this.Text += $" - {_tc.Status.GetDescription()}";
            }
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            WinProcessing.BackFormBtn(this);
        }

        private async void Win6_new_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (concurrencyBlockServise != null && concurrencyBlockServise.GetObjectUsedStatus())
            {
                concurrencyBlockServise.CleanBlockData(); // Снимаем блокировку
            }

            if (!OptimicticError)
            {
                // проверка на наличие изменений во всех формах
                if (!CheckForChanges()) // если false, то отменяем переключение
                {
                    _logger.Warning("Закрытие формы без сохранения (CloseFormsNoSave=true).");
                    e.Cancel = true;
                    return;
                }
            }
            AutosaveTimer?.Dispose();
            //close all inner forms
            foreach (var form in _formsCache.Values)
            {
                // is form is ISaveEventForm
                if (!form.IsDisposed)
                {
                    form.Close();
                }
            }
        }

        private void cmbTechCardName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private async Task ShowForm(EModelType modelType)
        {
            _logger.Information("Загрузка формы ModelType={ModelType} для TcId={TcId}",
                modelType, _tcId);

            if (_activeModelType == modelType)
            {
                _logger.Debug("Форма ModelType={ModelType} уже активна. Пропуск переключения.", modelType);
                return;
            }

            try
            {
                // Блокировка формы при переключении
                this.Enabled = false;
                bool isSwitchingFromOrToWorkStep = _activeModelType == EModelType.WorkStep || modelType == EModelType.WorkStep
                                                || _activeModelType == EModelType.Diagram || modelType == EModelType.Diagram
                                                || _activeModelType == EModelType.Outlay || modelType == EModelType.Outlay;

                var diagramForm = CheckOpenFormService.FindOpenedForm<DiagramForm>(_tcId);
                var areDiagramInWindow = _activeModelType != EModelType.Diagram && modelType != EModelType.Diagram && isSwitchingFromOrToWorkStep && diagramForm != null;

                if (isSwitchingFromOrToWorkStep)
                {
                    // Удаляем формы из кеша для их обновления при следующем доступе
                    foreach (var formKey in _formsCache.Keys.ToList())
                    {
                        if (_formsCache[formKey] != null)
                        {
                            if (areDiagramInWindow && formKey == EModelType.Diagram)
                                continue;

                            _formsCache[formKey].Close();
                            _formsCache.Remove(formKey);
                            //_formsCache[formKey].Dispose();
                        }
                    }

                    //_formsCache.Clear(); // Очищаем кеш
                }

                if (!_formsCache.TryGetValue(modelType, out var form))
                {
                    form = CreateForm(modelType);
                    _formsCache[modelType] = form;
                }

                if (form is IOnActivationForm baseForm)
                {
                    baseForm.OnActivate();
                }


                SwitchActiveForm(form);
                _activeModelType = modelType;

                if (form is TechOperationForm techForm)
                {
                    //techForm.UpdateGrid(); // todo: кажется бесполезный участок, тк данные обновляются при загрузке формы
                }
                else if (form is DiagramForm digramForm)
                    digramForm.Update();

                UpdateButtonsState(modelType);

                _logger.Information("Форма ModelType={ModelType} успешно активирована для TcId={TcId}",
                    modelType, _tcId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке формы ModelType={ModelType} для TcId={TcId}", modelType, _tcId);
                MessageBox.Show("Ошибка загрузки формы: " + ex.Message);
                this.Close();
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private bool CheckForChanges()
        {
            _logger.Debug("Проверка наличия несохраненных изменений для TcId={TcId}", _tcId);

            if (tcViewState.IsViewMode) //если находимся в режиме просмотра - выходим из метода без сохранения
            {
                _logger.Debug("Изменений нет, режим просмотра включен для TcId={TcId}", _tcId);
                return true;
            }

            if (_accessLevel == User.Role.User || _accessLevel == User.Role.ProjectManager)
            // todo: заменить этот "костыль" на невозможность внесения изменений другими ролями
            {
                return true;
            }

            if (HasChanges())
            {
                _logger.Warning("Обнаружены несохраненные изменения для TcId={TcId}", _tcId);

                var result = MessageBox.Show("Вы хотите сохранить изменения?", "Сохранение изменений",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveAllChanges();

                    return true;
                }
                else if (result == DialogResult.Cancel)
                {
                    _logger.Information("Сохранение отменено пользователем для TcId={TcId}", _tcId);

                    return false;
                }
            }

            _logger.Debug("Изменений не обнаружено для TcId={TcId}", _tcId);
            return true;
        }

        private bool HasChanges()
        {

            //var hasUnsavedChanges = false;
            //foreach (var fm in _formsCache.Values)
            //{
            //    if (fm is ISaveEventForm saveForm && saveForm.HasChanges || context.ChangeTracker.HasChanges())
            //    {
            //        hasUnsavedChanges = true;
            //        break;
            //    }
            //}

            // проверка на наличие изменений
            return _formsCache.Values.OfType<ISaveEventForm>()
                                              .Any(f => f.HasChanges)
                                              || context.ChangeTracker.HasChanges()
                                              || tcViewState.RoadmapInfo.IsRoadMapUpdate;
        }

        private Form CreateForm(EModelType modelType)
        {
            switch (modelType)
            {
                case EModelType.Staff:
                    return new Win6_Staff(_tcId, tcViewState, context);// _isViewMode);
                case EModelType.Component:
                    return new Win6_Component(_tcId, tcViewState, context);// _isViewMode);
                case EModelType.Machine:
                    return new Win6_Machine(_tcId, tcViewState, context); //_isViewMode);
                case EModelType.Protection:
                    return new Win6_Protection(_tcId, tcViewState, context);// _isViewMode);
                case EModelType.Tool:
                    return new Win6_Tool(_tcId, tcViewState, context);// _isViewMode);
                case EModelType.WorkStep:
                    return new TechOperationForm(_tcId, tcViewState, context);// _isViewMode);
                case EModelType.Diagram:
                    return new DiagramForm(_tcId, tcViewState, context);// _isViewMode);
                case EModelType.ExecutionScheme:
                    return new Win6_ExecutionScheme(/*_tc,*/ tcViewState, context);// _isViewMode);
                                                                                   //case EModelType.TechnologicalCard:
                                                                                   //    return new Win7_1_TCs_Window(_tcId, win6Format: true);
                case EModelType.Coefficient:
                    return new CoefficientEditorForm(_tcId, tcViewState, context);
                case EModelType.Outlay:
                    return new Win6_OutlayTable(tcViewState, calculateOutlayService);// _isViewMode);
                case EModelType.RoadMap:
                    return new Win6_RoadMap(tcViewState);// _isViewMode);
                default:
                    throw new ArgumentOutOfRangeException(nameof(modelType), "Неизвестный тип модели");
            }
        }
        private void SwitchActiveForm(Form form) // todo - move to WinProcessing and add to win7
        {
            _logger.Debug("Переключение на новую форму: {FormName}, TcId={TcId}", form.Name, _tcId);

            if (_activeForm != null)
            {
                _activeForm.Hide();
            }
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            pnlDataViewer.Controls.Add(form);

            DataGridView dataGridView = form.Controls.OfType<DataGridView>().FirstOrDefault();
            if (dataGridView != null)
            {
                dataGridView.SuspendLayout();
            }
            if (dataGridView != null)
            {
                dataGridView.ResumeLayout();
            }

            _activeForm = form;
            _activeForm.Show();

            _logger.Information("Форма {FormName} успешно активирована для TcId={TcId}", form.Name, _tcId);
        }

        private async void btnShowStaffs_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение персонала");
            await ShowForm(EModelType.Staff);
        }
        private async void btnShowComponents_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение компонентов");
            await ShowForm(EModelType.Component);
        }
        private async void btnShowMachines_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение механизмов");
            await ShowForm(EModelType.Machine);
        }
        private async void btnShowProtections_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение СЗ");
            await ShowForm(EModelType.Protection);
        }
        private async void btnShowTools_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение инструментов");
            await ShowForm(EModelType.Tool);
        }
        private async void btnShowWorkSteps_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение хода работ");
            await ShowForm(EModelType.WorkStep);
        }

        public async void buttonDiagram_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение диаграммы");
            await ShowForm(EModelType.Diagram);
        }

        private void UpdateButtonsState(EModelType activeModelType)
        {
            foreach (Control control in pnlControls.Controls)
            {
                if (control is Button button)
                {
                    button.BackColor = SystemColors.Control;
                    button.ForeColor = SystemColors.ControlText;

                    if (button.Tag is EModelType buttonModelType && buttonModelType == activeModelType)
                    {
                        button.BackColor = Color.FromArgb(10, 107, 88);
                        button.ForeColor = Color.White;
                    }
                }
            }


        }

        private void SetTagsToButtons()
        {
            btnShowStaffs.Tag = EModelType.Staff;
            btnShowComponents.Tag = EModelType.Component;
            btnShowMachines.Tag = EModelType.Machine;
            btnShowProtections.Tag = EModelType.Protection;
            btnShowTools.Tag = EModelType.Tool;
            btnShowWorkSteps.Tag = EModelType.WorkStep;
            btnShowCoefficients.Tag = EModelType.Coefficient;
        }

        private async void SaveAllChanges()
        {
            using var transaction = context.Database.BeginTransaction();
            _logger.Information("Сохранение изменений для TcId={TcId}", _tcId);

            _logger.Information("Сохранение изменений для TcId={TcId}", _tcId);
            try
            {
                SaveTehCartaChanges();
                SaveRoadMapData();
                calculateOutlayService.UpdateOutlay(tcViewState);

                foreach (var form in _formsCache.Values)
                {
                    // is form is ISaveEventForm
                    if (form is ISaveEventForm saveForm)//todo: пересмотреть логику работы интерфейса сохранения
                    {
                        saveForm.SaveChanges();
                    }
                }

                context.SaveChanges();
                transaction.Commit();
                _logger.Information("Изменения успешно сохранены для TcId={TcId}", _tcId);
                MessageBox.Show("Изменения сохранены");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                OptimicticError = true;

                var handler = new ConcurrencyConflictHandler(_logger, context);
                bool retry = await handler.HandleConcurrencyExceptionAsync(ex, _tcId);

                if (retry)
                {
                    await transaction.RollbackAsync(); // Откатываем первую транзакцию
                    await using var retryTransaction = await context.Database.BeginTransactionAsync();
                    try
                    {
                        SaveTehCartaChanges();
                        SaveRoadMapData();
                        calculateOutlayService.UpdateOutlay(tcViewState);

                        foreach (var form in _formsCache.Values)
                        {
                            if (form is ISaveEventForm saveForm)
                            {
                                await saveForm.SaveChanges();
                            }
                        }

                        await context.SaveChangesAsync();
                        await retryTransaction.CommitAsync();
                        _logger.Information("Конфликт разрешён, данные сохранены для TcId={TcId}", _tcId);

                        MessageBox.Show("Данные обновлены из базы. Проверьте изменения. Рекомендуется повторное сохранение");
                    }
                    catch (DbUpdateConcurrencyException ex2)
                    {
                        await retryTransaction.RollbackAsync();
                        _logger.Error(ex2, "Повторная попытка сохранения не удалась для TcId={TcId}", _tcId);
                        MessageBox.Show("Не удалось сохранить данные из-за повторного конфликта. Рекомендуется повторное сохранение");
                    }
                }
                else
                {
                    await transaction.RollbackAsync();
                    // UI уже обновлено в HandleConcurrencyExceptionAsync при удалении объекта
                    MessageBox.Show("Сохранение отменено из-за конфликта.");
                }
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.Error(ex, "Ошибка FK при сохранении данных для TcId={TcId}", _tcId);
                MessageBox.Show($"Ошибка сохранения: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при сохранении изменений для TcId={TcId}", _tcId);
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                if (OptimicticError)
                {
                    var newForm = new Win6_new(_tcId, _accessLevel, tcViewState.IsViewMode, startOpenForm);
                    newForm.Show();
                    MessageBox.Show("Карта была  перезагружена, так как была обнаружена ошибка оптимистичного параллелизма. Карта обновлена до последней версии, данные сохранены.", "Данные обновлены", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }
        }
        private void ControlSaveEvent(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                LogUserAction("Сохранение изменений по нажатию Ctrl+S");

                SaveAllChanges();
            }
        }

        private void SaveRoadMapData()
        {
            if (tcViewState.RoadmapInfo.RoadMapItems == null)
                return;

            var roadMapItems = tcViewState.RoadmapInfo.RoadMapItems;

            try
            {
                var existedRoadMapItems = context.RoadMapItems.Where(r => roadMapItems.Select(s => s.TowId).Any(s => s == r.TowId)).ToList();
                foreach (var item in roadMapItems)
                {
                    var existedRoadMap = existedRoadMapItems.Where(r => r.TowId == item.TowId).FirstOrDefault();
                    if (existedRoadMap != null)
                        existedRoadMap.ApplyUpdates(item);
                    else
                        context.RoadMapItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка сохранения дорожной карты: {ex.Message}");
                MessageBox.Show($"Ошибка сохранения дорожной карты: {ex.Message}", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveTehCartaChanges()
        {
            DbConnector dbCon = new DbConnector();

            List<TechOperationWork> AllDele = tcViewState.TechOperationWorksList.Where(w => w.Delete == true).ToList();
            foreach (TechOperationWork techOperationWork in AllDele)
            {
                tcViewState.TechOperationWorksList.Remove(techOperationWork);
                if (techOperationWork.NewItem == false)
                {
                    context.TechOperationWorks.Remove(techOperationWork);
                }
            }

            foreach (TechOperationWork techOperationWork in tcViewState.TechOperationWorksList)
            {
                var allDel = techOperationWork.executionWorks.Where(w => w.Delete == true).ToList();
                foreach (ExecutionWork executionWork in allDel)
                {
                    techOperationWork.executionWorks.Remove(executionWork);
                }

                var to = context.TechOperationWorks.SingleOrDefault(s => techOperationWork.Id != 0 && s.Id == techOperationWork.Id);
                if (to == null)
                {
                    context.TechOperationWorks.Add(techOperationWork);
                }
                else
                {
                    to = techOperationWork;
                }

                var delTools = techOperationWork.ToolWorks.Where(w => w.IsDeleted == true).ToList();

                foreach (ToolWork delTool in delTools)
                {
                    dbCon.DeleteRelatedToolComponentDiagram(delTool.Id, true);
                    techOperationWork.ToolWorks.Remove(delTool);
                }



                foreach (ToolWork toolWork in techOperationWork.ToolWorks)
                {
                    if (tcViewState.TechnologicalCard.Tool_TCs.SingleOrDefault(s => s.Child == toolWork.tool) == null)
                    {
                        Tool_TC tool = new Tool_TC();
                        tool.Child = toolWork.tool;
                        tool.Order = tcViewState.TechnologicalCard.Tool_TCs.Count + 1;
                        tool.Quantity = toolWork.Quantity;
                        tcViewState.TechnologicalCard.Tool_TCs.Add(tool);
                    }
                }

                var delComponents = techOperationWork.ComponentWorks.Where(w => w.IsDeleted == true).ToList();

                foreach (var delComp in delComponents)
                {
                    dbCon.DeleteRelatedToolComponentDiagram(delComp.Id, false);
                    techOperationWork.ComponentWorks.Remove(delComp);
                }

                foreach (ComponentWork componentWork in techOperationWork.ComponentWorks)
                {
                    if (tcViewState.TechnologicalCard.Component_TCs.SingleOrDefault(s => s.Child == componentWork.component) == null)
                    {
                        Component_TC Comp = new Component_TC();
                        Comp.Child = componentWork.component;
                        Comp.Order = tcViewState.TechnologicalCard.Component_TCs.Count + 1;
                        Comp.Quantity = componentWork.Quantity;
                        tcViewState.TechnologicalCard.Component_TCs.Add(Comp);
                    }
                }
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogUserAction("Изменение режима редактирования");

            if (concurrencyBlockServise.GetObjectUsedStatus())
            {
                _logger.Information("Карта уже редактируется другим пользователем. Отмена переключения режима для TcId={TcId}", _tcId);
                MessageBox.Show("Сейчас карта используется другим пользователем. Она доступна только для просмотра.");
                TcWasBlocked = true;
                return;
            }

            if (tcViewState.IsViewMode == false)
            {
                if (!CheckForChanges())
                    return;
            }

            if (!tcViewState.IsViewMode && !concurrencyBlockServise.GetObjectUsedStatus())
                concurrencyBlockServise.BlockObject();
            
            SetViewMode(!tcViewState.IsViewMode);

        }

        private async void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogUserAction("Печать технологической карты");
            var printForm = new TcPrintForm(_tcId);
            printForm.Show();
        }

        private async void printDiagramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogUserAction("Печать диаграммы");

            try
            {
                var diagramExporter = new DiadramExcelExport();
                await diagramExporter.SaveDiagramToExelFile(_tc.Article, _tc.Id);

                _logger.Information("Печать диаграммы успешно завершена для TcId={TcId}", _tcId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при печати диаграммы для TcId={TcId}", _tcId);
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogUserAction("Сохранение изменений");

            SaveAllChanges();
        }
        private bool CheckChangesForTcDraftStatusChanging()
        {
            bool hasUnsavedChanges = false;
            foreach (var fm in _formsCache.Values)
            {
                if (fm is ISaveEventForm saveForm && saveForm.HasChanges)
                {
                    hasUnsavedChanges = true;
                    break;
                }
            }
            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show("Перед выпуском карты необходимо сохранить изменения.", "Сохранение изменений",
                                             MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveAllChanges();
                    return true;
                }
                else
                    return false;
            }
            return true;
        }
        private async void setDraftStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogUserAction("Выпуск технической карты");

            await db.UpdateStatusTc(_tc, TechnologicalCardStatus.Draft);
            setDraftStatusToolStripMenuItem.Enabled = false;
            MessageBox.Show("Статус успешно обновлён.");
        }



        private async void SetApprovedStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogUserAction("Опубликование технической карты");

            // если есть несохраненые изменённыя, необходимо их сохранить, либо отменить
            if (!CheckForChanges())
            {
                MessageBox.Show("Перед опубликованием карты необходимо сохранить изменения.");
                return;
            }


            if (!CheckChangesForTcDraftStatusChanging()) { return; }

            try
            {
                List<(string Type, string Name)> unpublishedElements = db.CanTCDraftStatusChange(_tcId);

                if (unpublishedElements.Count > 0)
                {
                    string elements = "";
                    foreach (var element in unpublishedElements)
                    {
                        elements += "Тип: " + element.Type + ". Название: " + element.Name + ".\n";
                    }

                    MessageBox.Show("Карта не может быть опубликована, если используются неопубликолванные элементы. \n" + elements, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                await db.UpdateStatusTc(_tc, TechnologicalCardStatus.Approved);
                setApprovedStatusToolStripMenuItem.Enabled = false;

                // становить viewmode
                SetTCStatusAccess();
                SetViewMode(true);
                SetCommentViewMode(false);

                _logger.Information("Техническая карта успешно опубликована для TcId={TcId}", _tcId);

                var editorForm = new Win6_new(_tcId, role: _accessLevel);
                this.Close();
                editorForm.Show();

                MessageBox.Show("Техническая карта опубликована.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при опубликовании технической карты TcId={TcId}", _tcId);
                MessageBox.Show(ex.Message);
            }
        }

        private async void setRemarksModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogUserAction("Открытие этапа замечаний");

            if (_tc.Status == TechnologicalCardStatus.Draft && _accessLevel == User.Role.Lead)
            {
                await db.UpdateStatusTc(_tc, TechnologicalCardStatus.Remarked);
            }
            else if (_tc.Status == TechnologicalCardStatus.Draft && _accessLevel != User.Role.Lead)
            {
                MessageBox.Show("Открыть этап замечаний может только Технолог-руководитель.");
                return;
            }

            setRemarksModeToolStripMenuItem.Text = tcViewState.IsCommentViewMode ?
                    "Показать комментарии" : "Скрыть комментарии";

            SetCommentViewMode();
        }

        private async void SetMachineCollumnModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogUserAction("Изменение режима отображения столбцов механизмов");

            if (_tc.Status == TechnologicalCardStatus.Draft && _accessLevel == User.Role.Lead)
            {
                await db.UpdateStatusTc(_tc, TechnologicalCardStatus.Remarked);
            }

            isMachineViewMode = !isMachineViewMode;

            SetMachineCollumnModeToolStripMenuItem.Text = isMachineViewMode ?
                 "Скрыть столбцы механизмов" : "Показать стобцы механизмов";

            SetMachineViewMode();
        }

        private void SetMachineViewMode()
        {
            if (_formsCache.TryGetValue(EModelType.WorkStep, out var cachedForm)
                && cachedForm is TechOperationForm techOperationForm)
            {
                techOperationForm.SetMachineViewMode(isMachineViewMode);
            }
        }


        private void SetCommentViewMode(bool? isComViewMode = null)
        {
            if (isComViewMode != null)
            {
                tcViewState.IsCommentViewMode = (bool)isComViewMode;
            }
            else
                tcViewState.IsCommentViewMode = !tcViewState.IsCommentViewMode;

            if (_formsCache.TryGetValue(EModelType.WorkStep, out var cachedForm)
                && cachedForm is TechOperationForm techOperationForm)
            {
                techOperationForm.SetCommentViewMode();
            }
        }

        private void toolStripExecutionScheme_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение схемы выполнения");

            if (!_formsCache.TryGetValue(EModelType.ExecutionScheme, out var win6_ExecutionScheme)
                || win6_ExecutionScheme.IsDisposed)
            {
                win6_ExecutionScheme = CreateForm(EModelType.ExecutionScheme);
                _formsCache[EModelType.ExecutionScheme] = win6_ExecutionScheme;
            }
            win6_ExecutionScheme.Show();

            // вывести на передний план
            win6_ExecutionScheme.BringToFront();
        }

        private void toolStripDiagrams_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение диаграммы в отдельном окне");

            if (!_formsCache.TryGetValue(EModelType.Diagram, out var diagramForm)
                || diagramForm.IsDisposed)
            {
                diagramForm = CreateForm(EModelType.Diagram);
                _formsCache[EModelType.Diagram] = diagramForm;
            }
            diagramForm.Show();
            diagramForm.BringToFront();
        }

        private void LogUserAction(string actionDescription)
        {
            _logger.LogUserAction(actionDescription, _tcId);
            //Information("Действие пользователя: {Action}, TcId={TcId}", 
            //actionDescription, _tcId);
        }

        private async void btnShowCoefficients_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение коэффициентов");
            await ShowForm(EModelType.Coefficient);
        }

        private void toolStripShowCoefficients_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение коэффициентов в отдельном окне");

            if (!_formsCache.TryGetValue(EModelType.Coefficient, out var diagramForm)
                || diagramForm.IsDisposed)
            {
                diagramForm = CreateForm(EModelType.Coefficient);
                _formsCache[EModelType.Coefficient] = diagramForm;
            }
            diagramForm.Show();
            diagramForm.BringToFront();
        }

        public void RecalculateValuesWithCoefficientsInOpenForms()
        {
            foreach (var form in _formsCache.Values)
            {
                if (form is IOnActivationForm baseForm)
                {
                    baseForm.OnActivate();
                }
            }
        }

        private void ChangeIsDynamicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Проверка существующего статуса
            if (_tc.IsDynamic)
            {
                // Если карта динамическая, то
                // 1. проверяем наличие коэфиициентов в карте
                var coefficients = _tc.Coefficients;
                // 1.1. если коэффициенты присутствуют, то
                if (coefficients != null && coefficients.Count > 0)
                {
                    var coefDict = coefficients.ToDictionary(c => c.Code, c => c.Value);

                    // 1.1.1 во всех сущностях, где они используются, заменяем на их значения
                    // Выделить в отдельный список все сущности, где используются коэффициенты
                    List<IDynamicValue> dynamicValues = new List<IDynamicValue>();
                    foreach (var obj in _tc.Component_TCs)//.Where(c => c.ParentId == _tc.Id))
                    {
                        if (obj != null && obj is IDynamicValue)
                            AddToDynamicList(dynamicValues, obj);
                    }
                    foreach (var obj in _tc.Tool_TCs)//.Where(c => c.ParentId == _tc.Id))
                    {
                        if (obj != null && obj is IDynamicValue)
                            AddToDynamicList(dynamicValues, obj);
                    }
                    foreach (var obj in _tc.Protection_TCs)//.Where(c => c.ParentId == _tc.Id))
                    {
                        if (obj != null && obj is IDynamicValue)
                            AddToDynamicList(dynamicValues, obj);
                    }
                    foreach (var obj in _tc.Machine_TCs)//.Where(c => c.ParentId == _tc.Id))
                    {
                        if (obj != null && obj is IDynamicValue)
                            AddToDynamicList(dynamicValues, obj);
                    }

                    var executionWorks = _tc.TechOperationWorks.SelectMany(tow => tow.executionWorks).ToList();
                    var executionWorksWithCoefficients = executionWorks.Where(ew => !string.IsNullOrEmpty(ew.Coefficient) && ew.Coefficient.Contains(Coefficient.FirstLetter)).ToList();

                    var executionWorksRepeats = executionWorks.SelectMany(ew => ew.ExecutionWorkRepeats).ToList();//context.ExecutionWorkRepeats.Where(ewr => ewr.ParentExecutionWork.techOperationWork.TechnologicalCardId == _tc.Id).ToList();
                    var executionWorkRepeatsWithCoefficients = executionWorksRepeats
                        .Where(ewr => !string.IsNullOrEmpty(ewr.NewCoefficient) && ewr.NewCoefficient.Contains(Coefficient.FirstLetter)).ToList();

                    // в объектах IDynamicValue удалить значение формулы
                    foreach (var obj in dynamicValues)
                    {
                        obj.Formula = null;
                    }

                    // в объектах ExecutionWork и ExecutionWorksRepeat заменить коэффициенты на их значения
                    foreach (var ew in executionWorksWithCoefficients)
                    {
                        ew.Coefficient = MathScript.ReplaceCoefficientsInFormula(ew.Coefficient!, coefDict);
                    }

                    foreach (var ewr in executionWorkRepeatsWithCoefficients)
                    {
                        ewr.NewCoefficient = MathScript.ReplaceCoefficientsInFormula(ewr.NewCoefficient!, coefDict);
                    }
                }
                // 1.2. если коэффициенты отсутствуют, то дополнительно ничего делать не надо

                // Удаляем все коэффициенты
                _tc.Coefficients.Clear();
                // 2. изменяем статус на не динамический
                _tc.IsDynamic = false;
                // 3. закрываем доступ к коэффициентам
            }
            else
            {
                // Если карта не динамическая, то изменяем данный статус на динамический
                _tc.IsDynamic = true;
                // и открываем доступ к коэффициентам

            }

            UpdateDynamicCardParametrs();

            foreach (var form in _formsCache.Values)
            {
                if (form is IDynamicForm dynamicForm)
                {
                    dynamicForm.UpdateDynamicCardParametrs();
                }
            }


            void AddToDynamicList(List<IDynamicValue> dynamicValues, IDynamicValue obj)
            {
                var formula = obj.Formula;
                if (!string.IsNullOrEmpty(formula) && formula.Contains(Coefficient.FirstLetter))
                {
                    dynamicValues.Add(obj);
                }
            }
        }

        private void UpdateIsDynamicButtonText()
        {
            ChangeIsDynamicToolStripMenuItem.Text = _tc.IsDynamic ? "Сделать не динамической" : "Сделать динамической";
        }


        private async void button1_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение таблицы затрат");
            await ShowForm(EModelType.Outlay);
        }

        private async void btnRoadMap_Click(object sender, EventArgs e)
        {
            LogUserAction("Отображение дорожной карты");
            await ShowForm(EModelType.RoadMap);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var editor = new Win6_ImageEditor(null, tcViewState, context, false);
            editor.Show();
            editor.AfterSave = async (savedObj) =>
            {
                var techOperationForm = CheckOpenFormService.FindOpenedForm<TechOperationForm>(_tcId);
                if (techOperationForm != null)
                    techOperationForm.RefreshPictureNameColumn();
            };

        }

        private void btnHideControlBtns_Click(object sender, EventArgs e)
        {
            pnlControls.Visible = !pnlControls.Visible;

            if (pnlControls.Visible)
                btnHideControlBtns.Text = "Скрыть кнопки";
            else
                btnHideControlBtns.Text = "Показать кнопки";
        }

        private void setAutoSaveSettings_Click(object sender, EventArgs e)
        {
            isFormAutosave = !isFormAutosave;
            setAutoSaveSettings.Text = isFormAutosave ? "Выключить автосохранение" : "Включить автосохранение";
            SetAutosaveTimerWork();
        }

        private void SetAutosaveTimerWork()
        {
            if(isFormAutosave)
            {
                // Создание таймера с отсчетом
                AutosaveTimer = new Timer();
                AutosaveTimer.Interval = TimerInterval;
                // Hook up the Elapsed event for the timer.
                AutosaveTimer.Elapsed += AutosaveTimer_Elapsed;
                AutosaveTimer.AutoReset = true;
                AutosaveTimer.Enabled = true;
            }
            else
            {
                AutosaveTimer?.Dispose();
            }
        }

        private void AutosaveTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            SaveAllChanges();
        }
    }

}

