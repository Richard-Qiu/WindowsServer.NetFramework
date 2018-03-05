using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Log
{
    public class Logger
    {
        private NLog.Logger _nlogger = null;

        internal Logger(NLog.Logger nlogger)
        {
            _nlogger = nlogger;
        }


        public void DebugException(string message, Exception exception)
        {
            _nlogger.DebugException(message, exception);
        }

        public void Debug(string message)
        { 
            _nlogger.Debug(message);
        }

        public void Debug(string message, params object[] args)
        { 
            _nlogger.Debug(message, args);
        }

        public void Debug<TArgument>(string message, TArgument argument)
        {
            _nlogger.Debug<TArgument>(message, argument);
        }

        public void Debug<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        { 
            _nlogger.Debug<TArgument1, TArgument2>(message, argument1, argument2);
        }

        public void Debug<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        { 
            _nlogger.Debug<TArgument1, TArgument2, TArgument3>(message, argument1, argument2, argument3);
        }



        public void InfoException(string message, Exception exception)
        {
            _nlogger.InfoException(message, exception);
        }

        public void Info(string message)
        {
            _nlogger.Info(message);
        }

        public void Info(string message, params object[] args)
        {
            _nlogger.Info(message, args);
        }

        public void Info<TArgument>(string message, TArgument argument)
        {
            _nlogger.Info<TArgument>(message, argument);
        }

        public void Info<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        {
            _nlogger.Info<TArgument1, TArgument2>(message, argument1, argument2);
        }

        public void Info<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            _nlogger.Info<TArgument1, TArgument2, TArgument3>(message, argument1, argument2, argument3);
        }



        public void WarnException(string message, Exception exception)
        {
            _nlogger.WarnException(message, exception);
        }

        public void Warn(string message)
        {
            _nlogger.Warn(message);
        }

        public void Warn(string message, params object[] args)
        {
            _nlogger.Warn(message, args);
        }

        public void Warn<TArgument>(string message, TArgument argument)
        {
            _nlogger.Warn<TArgument>(message, argument);
        }

        public void Warn<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        {
            _nlogger.Warn<TArgument1, TArgument2>(message, argument1, argument2);
        }

        public void Warn<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            _nlogger.Warn<TArgument1, TArgument2, TArgument3>(message, argument1, argument2, argument3);
        }


        public void ErrorException(string message, Exception exception)
        {
            _nlogger.ErrorException(message, exception);
        }

        public void Error(string message)
        {
            _nlogger.Error(message);
        }

        public void Error(string message, params object[] args)
        {
            _nlogger.Error(message, args);
        }

        public void Error<TArgument>(string message, TArgument argument)
        {
            _nlogger.Error<TArgument>(message, argument);
        }

        public void Error<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        {
            _nlogger.Error<TArgument1, TArgument2>(message, argument1, argument2);
        }

        public void Error<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            _nlogger.Error<TArgument1, TArgument2, TArgument3>(message, argument1, argument2, argument3);
        }


        public void FatalException(string message, Exception exception)
        {
            _nlogger.FatalException(message, exception);
        }

        public void Fatal(string message)
        {
            _nlogger.Fatal(message);
        }

        public void Fatal(string message, params object[] args)
        {
            _nlogger.Fatal(message, args);
        }

        public void Fatal<TArgument>(string message, TArgument argument)
        {
            _nlogger.Fatal<TArgument>(message, argument);
        }

        public void Fatal<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        {
            _nlogger.Fatal<TArgument1, TArgument2>(message, argument1, argument2);
        }

        public void Fatal<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            _nlogger.Fatal<TArgument1, TArgument2, TArgument3>(message, argument1, argument2, argument3);
        }


    }
}
