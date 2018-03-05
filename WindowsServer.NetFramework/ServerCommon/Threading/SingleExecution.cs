using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsServer.Log;

namespace WindowsServer.Threading
{
    public class SingleExecution
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private object _syncObject = new object();
#pragma warning disable 0414
        private volatile bool _isExecuting = false;
#pragma warning restore 0414
        private volatile Action _currentExecution = null;
        private volatile Action _nextExecution = null;

        /// <summary>
        /// Execute the specified action.
        /// </summary>
        /// <param name="execution">The execution which will be run immediately if there is no other execution is running.
        /// Otherwise, it will be run after the current execution completes.</param>
        /// <param name="overridePreviousExecutions">The flag indicates whether the previous executions, which are waiting for being run,
        /// will be ignored. If true, the given 'execution' will be run. If false, the given 'execution' will be ignored.</param>
        /// <returns>If true, the 'execution' will be run immediately. Otherwise, it is waiting for the completion of current execution.</returns>
        public bool Execute(Action execution, bool overridePreviousExecutions)
        {
            _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- Execute() entered.");
            lock(_syncObject)
            {
                // BUG!!!
                // There is a bug here. _currentExecution is always null. So far, cannot find the root cause.
                _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- Execute() entered lock. _currentExecution is " + (_currentExecution==null?"null":"not null"));

                if (_currentExecution != null)
                {
                    if ((_nextExecution == null) || overridePreviousExecutions)
                    {
                        _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- Execute() setting _nextExecution.");
                        _nextExecution = execution;
                    }
                    _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- Execute() returning false.");
                    return false;
                }
                else
                {
                    _currentExecution = execution;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(StartExecute));
                    _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- Execute() returning true.");
                    return true;
                }
            }

        }

        private void StartExecute(object unused)
        {
            _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- StartExecute() entered.");
            Action currentExecution;
            lock(_syncObject)
            {
                _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- StartExecute() entered lock1.");
                _isExecuting = true;
                currentExecution = _currentExecution;
            }
            _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- StartExecute() exited lock1.");

            try
            {
                currentExecution();
            }
            catch(Exception ex)
            {
                _logger.ErrorException("Failed to run execution. Exception occurs.", ex);
            }
            finally
            {
                _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- StartExecute() entering lock2.");
                lock (_syncObject)
                {
                    _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- StartExecute() entered lock2.");
                    _isExecuting = false;
                    _currentExecution = null;

                    if (_nextExecution != null)
                    {
                        _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- StartExecute() starting _nextExecution.");
                        _currentExecution = _nextExecution;
                        _nextExecution = null;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(StartExecute));
                    }
                }
                _logger.Info("---[" + Thread.CurrentThread.ManagedThreadId + "]---------- StartExecute() exited lock2.");
            }

        }
    }
}
