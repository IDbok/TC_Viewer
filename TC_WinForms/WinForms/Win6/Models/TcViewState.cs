using TcModels.Models;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.RoadMap;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms.Win6.Models
{
	public class TcViewState
	{
		private Win6_new _parentForm;

		private bool _isViewMode = true;
		private bool _isCommentViewMode = false;

		public double DiagramScale { get; set; } = 1;

		public User.Role UserRole { get; }

		public TechnologicalCard TechnologicalCard { get; set; } // todo: make it readonly
		public List<TechOperationWork> TechOperationWorksList { get; set; }
		public List<DiagamToWork> DiagramToWorkList { get; set; }
		public (bool IsRoadMapUpdate, List <RoadMapItem> RoadMapItems) RoadmapInfo { get; set; }//Хранит информацию о том, изменелалась/генерировалась ли дорожная карта и список элементов дорожной карты
        public TcViewState(User.Role userRole, Win6_new parentForm)
		{
			UserRole = userRole;
			_parentForm = parentForm;
		}
		public Guid FormGuid => _parentForm.FormGuid;
		public bool IsViewMode
		{
			get => _isViewMode;
			set
			{
				if (_isViewMode != value)
				{
					_isViewMode = value;
					OnViewModeChanged();
				}
			}
		}
		public bool IsCommentViewMode
		{
			get => _isCommentViewMode;
			set
			{
				if (_isCommentViewMode != value)
				{
					_isCommentViewMode = value;
					OnCommentViewModeChanged();
				}
			}
		}

		public event Action? CommentViewModeChanged;
		public event Action? ViewModeChanged;

		private void OnCommentViewModeChanged()
		{
			CommentViewModeChanged?.Invoke();
		}
		private void OnViewModeChanged()
		{
			ViewModeChanged?.Invoke();
		}

		public void RecalculateValuesWithCoefficients()
		{
			_parentForm.RecalculateValuesWithCoefficientsInOpenForms();
		}

        public List<ExecutionWork> GetAllExecutionWorks()
        {
            List<ExecutionWork> allExecutionWorks = new List<ExecutionWork>();
            foreach (var techOperationWork in TechOperationWorksList.OrderBy(o => o.Order))
            {
				if (techOperationWork.Delete) continue;

				foreach (var ew in techOperationWork.executionWorks.OrderBy(o => o.Order))
				{
					if (ew.Delete) continue;

					allExecutionWorks.Add(ew);
                }
            }
            return allExecutionWorks;
        }
    }
}
