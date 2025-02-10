using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebhookServer.Demo2.Models;

namespace WebhookServer.Demo2.Windows
{
    public interface IWindow
    {
        Task RunAsync();
        Task<bool> Update();
        Task Show();
        Task<bool> Listen();
    }

    public class BaseWindow : IWindow
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly AppConfiguration _appConfiguration;
        protected bool _isRunning = false;
        protected IDictionary<ConsoleKey, Func<Task<bool>>> _inputHandler;

        public BaseWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _appConfiguration = serviceProvider.GetService<AppConfiguration>();
        }

        public virtual async Task<bool> Listen()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (_inputHandler != null)
                {
                    if (_inputHandler.TryGetValue(key.Key, out var fnc))
                    {
                        return await fnc();
                    }
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    return false;
                }
            }
            return true;
        }

        public virtual async Task RunAsync()
        {
            _isRunning = true;
            await Show();
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();

            while (_isRunning)
            {
                try
                {
                    int elapsedTime = (int)stopwatch.ElapsedMilliseconds;
                    int sleepTime = _appConfiguration.TargetFrameRate - elapsedTime;
                    if (sleepTime > 0)
                    {
                        Thread.Sleep(sleepTime);
                    }

                    stopwatch.Restart();
                    _isRunning = await Update();
                    _isRunning = _isRunning &&  await Listen();
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                }
            }
            _isRunning = false;
        }

        public virtual async Task Show()
        {
            Console.Clear();
        }

        public virtual async Task<bool> Update()
        {
            return true;
        }
    }
}
