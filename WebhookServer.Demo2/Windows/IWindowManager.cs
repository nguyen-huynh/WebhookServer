using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookServer.Demo2.Windows
{
    public interface IWindowManager
    {
        void SetNextWindow(IWindow window);
        void SetNextWindow<T>() where T : IWindow;
        Task RunAsync();
    }

    public class WindowManager: IWindowManager
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<WindowManager> logger;
        private IWindow _currentWindow;
        private IWindow _nextWindow;

        public WindowManager(IServiceProvider serviceProvider, ILogger<WindowManager> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public void SetNextWindow(IWindow window)
        {
            _currentWindow = _nextWindow;
            _nextWindow = window;
        }
        public void SetNextWindow<T>() where T:IWindow
        {
            var window = serviceProvider.GetService<T>();
            SetNextWindow(window);
        }

        public async Task RunAsync()
        {
            while (_nextWindow != null)
            {
                _currentWindow = _nextWindow;
                _nextWindow = null;
                try
                {
                    await _currentWindow.RunAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    _nextWindow = null;
                }
            }
        }
    }
}
