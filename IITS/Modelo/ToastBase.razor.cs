using IITS.Services;
using Microsoft.AspNetCore.Components;

namespace IITS.Modelo
{
    public class ToastBase : ComponentBase
    {
        [Inject] public ToastService ToastService { get; set; } = null!;

        protected string Heading { get; set; } = "";
        protected string Message { get; set; } = "";
        protected bool IsVisible { get; set; }
        protected string BackgroundCssClass { get; set; } = "";
        protected string IconCssClass { get; set; } = "";
        private CancellationTokenSource? _autoHideCts;

        protected override void OnInitialized()
        {
            ToastService.OnShow += ShowToast;
            ToastService.OnHide += HideToast;
        }

        private async void ShowToast(string message, ToastLevel level)
        {
            _autoHideCts?.Cancel();
            BuildToastSettings(level, message);
            IsVisible = true;
            await InvokeAsync(StateHasChanged);
            _autoHideCts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(3000, _autoHideCts!.Token);
                    await InvokeAsync(() => { ToastService.HideToast(); });
                }
                catch (TaskCanceledException) { }
            });
        }

        private async void HideToast()
        {
            _autoHideCts?.Cancel();
            _autoHideCts = null;
            IsVisible = false;
            await InvokeAsync(StateHasChanged);
        }

        private void BuildToastSettings(ToastLevel level, string message)
        {
            switch (level)
            {
                case ToastLevel.Info:
                    BackgroundCssClass = "has-background-info";
                    IconCssClass = "information-outline";
                    Heading = "Info";
                    break;
                case ToastLevel.Success:
                    BackgroundCssClass = "has-background-success";
                    IconCssClass = "check-circle-outline";
                    Heading = "Éxito";
                    break;
                case ToastLevel.Warning:
                    BackgroundCssClass = "has-background-warning";
                    IconCssClass = "alert-circle-outline";
                    Heading = "Advertencia";
                    break;
                case ToastLevel.Error:
                    BackgroundCssClass = "has-background-danger";
                    IconCssClass = "alpha-x-circle-outline";
                    Heading = "Error";
                    break;
            }

            Message = message;
        }

    }
}
