

using TcModels.Models;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms
{
    public class TcViewState
    {
        private bool _isViewMode = true;
        private bool _isCommentViewMode = false;
        public User.Role UserRole { get; }

        public TechnologicalCard? TechnologicalCard; // todo: make it readonly

        public TcViewState(User.Role userRole)
        {
            UserRole = userRole;
        }
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
    }
}
