using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.RoadMap;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms.Win6.RoadMap
{
    public partial class Win6_RoadMap : Form
    {
        private ElementHost elementHost;
        private TcViewState _tcviewState;
        private RoadMapControl _roadMapControl;
        private ILogger _logger;
        public Win6_RoadMap(TcViewState tcViewState)
        {
            _tcviewState = tcViewState;

            InitializeComponent();
            Load += Win6_RoadMap_Load;
        }

        private void Win6_RoadMap_Load(object? sender, EventArgs e)
        {
            _logger = Log.ForContext<Win6_RoadMap>(); // Устанавливаем контекст класса
            _logger.Information("Инициализация окна Win6_RoadMap");

            if (!_tcviewState.IsViewMode)
            {
                _logger.Information("Окно открывается в режиме редактирования, инициализируется конструктор с List<TechOperationWorks>");
                _roadMapControl = new RoadMapControl(_tcviewState.TechOperationWorksList, _tcviewState);
                InitializeWpfControl();
                return;
            }

            List<int> towIds = _tcviewState.TechOperationWorksList.Select(s => s.Id).ToList();
            List<RoadMapItem> roadMapItems;

            using (var context = new MyDbContext())
            {
                _logger.Information("Происходит запрос данных RoadMapItem из БД");
                roadMapItems = context.Set<RoadMapItem>().Where(r => towIds.Contains(r.TowId)).ToList();
            }

            bool isDataInvalid = !roadMapItems.Any() || roadMapItems.Count != towIds.Count;

            if (isDataInvalid)
            {
                _logger.Error($"Данные не валидны, количество TO равно {towIds.Count}, количество записей roadMapItems равно {roadMapItems.Count}");
                HandleInvalidData(roadMapItems);
                return;
            }

            _logger.Information("Окно открывается в режиме просмотра, инициализируется конструктор с List<RoadMapItem>");
            _roadMapControl = new RoadMapControl(roadMapItems, _tcviewState);
            _tcviewState.RoadmapItemList = (false, roadMapItems);
            InitializeWpfControl();
        }

        private void HandleInvalidData(List<RoadMapItem> roadMapItems)
        {
            string message = "В ходе запроса данных возникла ошибка:\nЗаписи дорожной карты, связанные с ТК либо не существуют, либо не актуальны.";

            if (_tcviewState.UserRole == User.Role.Lead)
            {
                var result = MessageBox.Show( $"{message}\nОбновить записи и перезаписать в базе?", "Внимание", MessageBoxButtons.YesNo,MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _logger.Information("Окно открывается в режиме просмотра, но инициализируется конструктор с List<TechOperationWorks>, пользователь с ролью {_tcviewState.UserRole} инициализировал запрос");
                    _roadMapControl = new RoadMapControl(_tcviewState.TechOperationWorksList, _tcviewState);
                    InitializeWpfControl();
                }
            }
            else
            {
                _logger.Information("Инициализация окна прервана, данные не валидны.");
                MessageBox.Show( $"{message}\nОбратитесь для обновления данных ТК к технологу руководителю если карта выпущена, либо переведите карту в режим редактирования и сохраните изменения.",
                    "Внимание", MessageBoxButtons.OK,MessageBoxIcon.Warning
                );
            }
        }

        private void InitializeWpfControl()
        {
            InitializeElementHost();

            // Помещаем WPF-контрол в ElementHost
            elementHost.Child = _roadMapControl;
        }

        private void InitializeElementHost()
        {
            elementHost = new ElementHost();
            elementHost.Dock = DockStyle.Fill;

            this.Controls.Add(elementHost);
        }
    }
}
