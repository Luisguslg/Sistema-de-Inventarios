
namespace IITS.Services
{
    public enum ToastLevel
    {
        Info,
        Success,
        Warning,
        Error
    }
    public class ToastService
    {
        public event Action<string, ToastLevel> OnShow = delegate { };
        public event Action OnHide = delegate { };



        public void ShowToast(string message, ToastLevel level)
        {
            OnShow?.Invoke(message, level);
        }
        public void HideToast()
        {
            OnHide?.Invoke();
        }
    }
}
