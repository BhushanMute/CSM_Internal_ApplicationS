using Microsoft.JSInterop;

namespace CSMTutorial.Services
{
    public class JsInteropService : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

        public JsInteropService(IJSRuntime jsRuntime)
        {
            _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/mainLayout.js").AsTask());
        }

        public async ValueTask<bool> GetTheme()
        {
            try
            {
                var module = await _moduleTask.Value;
                // ✅ Call exported themeManager object
                return await module.InvokeAsync<bool>("themeManager.getTheme");
            }
            catch
            {
                return false;
            }
        }

        public async ValueTask SetTheme(bool isDark)
        {
            var module = await _moduleTask.Value;
            await module.InvokeAsync<Task>("themeManager.setTheme", isDark);
        }

        public async ValueTask<bool> CheckIfMobile()
        {
            try
            {
                var module = await _moduleTask.Value;
                // ✅ Call exported function
                return await module.InvokeAsync<bool>("checkIfMobile");
            }
            catch
            {
                return false;
            }
        }

        public async ValueTask InitializeMainLayout<T>(DotNetObjectReference<T> dotNetRef) where T : class
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("initializeMainLayout", dotNetRef);
        }

        public async ValueTask DisposeMainLayout()
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("disposeMainLayout");
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_moduleTask.IsValueCreated)
            {
                try
                {
                    var module = await _moduleTask.Value;
                    await module.DisposeAsync();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
        }
    }
}