
using System;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization;

namespace Logging
{
    enum LogLevel
    {
        Debug= 1,
        Info = 2,
        Warn = 3,
        Error = 4
    }
    //LogMessage(Value Object — SRP)
    class LogMessage
    {
        public LogLevel Level { get; }
        public DateTime TimeStamp { get; }
        public string Message { get; }
        public string LoggerName { get; }
        public LogMessage(LogLevel level, string message, string loggerName)
        {
            this.Level = level;
            this.TimeStamp = DateTime.Now;
            this.Message = message;
            this.LoggerName = loggerName;
        }
    }
    //Formatter Strategy(OCP + Strategy)
    interface IFormatter
    {
        string Format(LogMessage msg);
    }
    class SimpleFormatter : IFormatter
    {
        public string Format(LogMessage msg)
        {
            return $"{ msg.TimeStamp:HH:mm:ss} [{msg.Level}], {msg.Message}-{msg.LoggerName}";
        }
    }
    //Destination Strategy(Strategy Pattern)
    //Different behaviors → same interface → Strategy + LSP
    interface ILogDestination
    {
        void Write(string formattedMsg);
    }
    class ConsoleLogDestination : ILogDestination
    {
        public void Write(string formattedMsg)
        {
            Console.WriteLine(formattedMsg);
        }
    }
    class FileLogDestination : ILogDestination
    {
        private readonly string Path;
        public FileLogDestination(string path)
        {
            this.Path = path;
        }
        public void Write(string formattedMsg)
        {
            File.AppendAllText(Path, formattedMsg + Environment.NewLine);
        }
    }
    //LoggerConfig(SRP — config holder)
    class LoggerConfig
    {
        public LogLevel MinLevel { get; }
        public List<ILogDestination> Destinations { get; }
        public IFormatter Formatter { get; }
        public LoggerConfig(LogLevel minLevel, List<ILogDestination> destinations, IFormatter formatter)
        {
            MinLevel = minLevel;
            Destinations = destinations;
            Formatter = formatter;
        }
    }
    //Logger(Singleton + Facade + DIP)
    class Logger
    {
        private static Logger _instance;
        private static readonly object _lock = new object();

        private LoggerConfig _config;
        private string _name;

        private Logger(string name, LoggerConfig config)
        {
            _name = name;
            _config = config;
        }
        public static Logger GetInstance(string name, LoggerConfig config)
        {
            if(_instance == null)
            {
                lock(_lock)
                {
                    if(_instance == null)
                    {
                        _instance = new Logger(name, config);
                    }
                }
            }
            return _instance;
        }
        private bool ShouldLog(LogLevel level)
        {
            return level >= _config.MinLevel;
        }
        public void Log(LogLevel level, string msg)
        {
            if (!ShouldLog(level))
                return;
            var logMsg = new LogMessage(level, msg, _name);
            string formattedMsg = _config.Formatter.Format(logMsg);
            foreach(var dest in _config.Destinations)
            {
                dest.Write(formattedMsg);
            }
        }
        // Convenience methods (Facade API)
        public void Info(string msg) => Log(LogLevel.Info, msg);
        public void Warn(string msg) => Log(LogLevel.Warn, msg);
        public void Debug(string msg) => Log(LogLevel.Debug, msg);
        public void Error(string msg) => Log(LogLevel.Error, msg);
    }

    class Program
    {
        static void Main(string[] args)
        {
            var destinations = new List<ILogDestination> { new ConsoleLogDestination(), new FileLogDestination("app.log") };
            var config = new LoggerConfig(LogLevel.Debug, destinations, new SimpleFormatter());

            var logger1 = Logger.GetInstance("AppInfoLogger", config);
            logger1.Info("Application started");

            var logger2 = Logger.GetInstance("AppDebugLogger", config);
            logger2.Debug("This will be filtered");

            var logger3 = Logger.GetInstance("AppWarnLogger", config);
            logger3.Warn("Low memory");

            var logger4 = Logger.GetInstance("AppErrorLogger", config);
            logger4.Error("Something failed");

            Console.WriteLine("Done - press key");
            Console.ReadKey();
        }
    }
}
