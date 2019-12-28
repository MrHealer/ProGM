/// <summary>
/// Author: nnthuong
/// Create Date: 11/10/2019
/// Description: 
/// </summary>

using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProGM.Business.Extention
{
    public class Logger
    {
        //private static readonly object AppSetting;

        private static void Init()
        {
            var configuration = new LoggingConfiguration();
            var target = new FileTarget();
            configuration.AddTarget("file", target);
            target.FileName = string.Format(@"{0}{1}\TraceLog.txt", LogPath, "${date:format=yyyyMMdd}");
            target.Layout = "${date:format=HH\\:mm\\:ss}\t${message}\t${logger}\t${stacktrace}";
            var rule = new LoggingRule("*", target);
            rule.EnableLoggingForLevel(LogLevel.Trace);
            configuration.LoggingRules.Add(rule);
            var target2 = new FileTarget();
            configuration.AddTarget("file", target2);
            target2.FileName = string.Format(@"{0}{1}\DebugLog.txt", LogPath, "${date:format=yyyyMMdd}");
            target2.Layout = "${date:format=HH\\:mm\\:ss}\t${message}\t${logger}\t${stacktrace}";
            var rule2 = new LoggingRule("*", target2);
            rule2.EnableLoggingForLevel(LogLevel.Debug);
            configuration.LoggingRules.Add(rule2);
            var target3 = new FileTarget();
            configuration.AddTarget("file", target3);
            target3.FileName = string.Format(@"{0}{1}\InfoLog.txt", LogPath, "${date:format=yyyyMMdd}");
            target3.Layout = "${date:format=HH\\:mm\\:ss}\t${message}\t${logger}\t${stacktrace}";
            var rule3 = new LoggingRule("*", target3);
            rule3.EnableLoggingForLevel(LogLevel.Info);
            configuration.LoggingRules.Add(rule3);
            var target4 = new FileTarget();
            configuration.AddTarget("file", target4);
            target4.FileName = string.Format(@"{0}{1}\WarnLog.txt", LogPath, "${date:format=yyyyMMdd}");
            target4.Layout = "${date:format=HH\\:mm\\:ss}\t${message}\t${logger}\t${stacktrace}";
            var rule4 = new LoggingRule("*", target4);
            rule4.EnableLoggingForLevel(LogLevel.Warn);
            configuration.LoggingRules.Add(rule4);
            var target5 = new FileTarget();
            configuration.AddTarget("file", target5);
            target5.FileName = string.Format(@"{0}{1}\ErrorLog.txt", LogPath, "${date:format=yyyyMMdd}");
            target5.Layout = "${date:format=HH\\:mm\\:ss}\t${message}\t${logger}\t${stacktrace}";
            var rule5 = new LoggingRule("*", target5);
            rule5.EnableLoggingForLevel(LogLevel.Error);
            configuration.LoggingRules.Add(rule5);
            var target6 = new FileTarget();
            configuration.AddTarget("file", target6);
            target6.FileName = string.Format(@"{0}{1}\FatalLog.txt", LogPath, "${date:format=yyyyMMdd}");
            target6.Layout = "${date:format=HH\\:mm\\:ss}\t${message}\t${logger}\t${stacktrace}";
            var rule6 = new LoggingRule("*", target6);
            rule6.EnableLoggingForLevel(LogLevel.Fatal);
            configuration.LoggingRules.Add(rule6);
            LogManager.Configuration = configuration;
        }
        public static void WriteLog(string content)
        {
            if (LogManager.Configuration == null)
            {
                Init();
            }
            var logger = LogManager.GetLogger(new StackFrame(1).GetMethod().Name);
            logger.Error(content);
        }
        public static void WriteLog(LogType logType, string content)
        {
            if (LogManager.Configuration == null)
            {
                Init();
            }
            var logger = LogManager.GetLogger(new StackFrame(1).GetMethod().Name);
            switch (logType)
            {
                case LogType.Trace:
                    logger.Trace(content);
                    return;

                case LogType.Debug:
                    if (AppSetting.GetString("IsDebugMode") == "1") logger.Debug(content);
                    return;

                case LogType.Warning:
                    logger.Warn(content);
                    return;

                case LogType.Error:
                    logger.Error(content);
                    return;

                case LogType.Fatal:
                    logger.Fatal(content);
                    return;
            }
            logger.Info(content);
        }

        public static void WriteLog(LogType logType, string content, LogParam logParam)
        {
            if (LogManager.Configuration == null)
            {
                Init();
            }
            var logger = LogManager.GetLogger(new StackFrame(1).GetMethod().Name);
            switch (logType)
            {
                case LogType.Trace:
                    logger.Trace(content);
                    foreach (var pair in logParam.Attribute)
                    {
                        logger.Trace(string.Format("[{0}:{1}]", pair.Key, pair.Value));
                    }
                    return;

                case LogType.Debug:
                    logger.Debug(content);
                    foreach (var pair in logParam.Attribute)
                    {
                        logger.Debug(string.Format("[{0}:{1}]", pair.Key, pair.Value));
                    }
                    return;

                case LogType.Warning:
                    logger.Warn(content);
                    foreach (var pair in logParam.Attribute)
                    {
                        logger.Warn(string.Format("[{0}:{1}]", pair.Key, pair.Value));
                    }
                    return;

                case LogType.Error:
                    logger.Error(content);
                    foreach (var pair in logParam.Attribute)
                    {
                        logger.Error(string.Format("[{0}:{1}]", pair.Key, pair.Value));
                    }
                    return;

                case LogType.Fatal:
                    logger.Fatal(content);
                    foreach (var pair in logParam.Attribute)
                    {
                        logger.Fatal(string.Format("[{0}:{1}]", pair.Key, pair.Value));
                    }
                    return;
            }
            logger.Info(content);
            foreach (var pair in logParam.Attribute)
            {
                logger.Info(string.Format("[{0}:{1}]", pair.Key, pair.Value));
            }
        }

        private static string LogPath
        {
            get
            {
                string path = new Uri(System.Reflection.Assembly.GetExecutingAssembly().EscapedCodeBase).AbsolutePath;
                return (!string.IsNullOrEmpty(AppSetting.GetString("LogPath")) ? AppSetting.GetString("LogPath") : path.Substring(0, path.IndexOf("/bin") + 1) + "Logs/");
                // Compares:
                //new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath
                //Environment.CurrentDirectory
            }
        }

        [Serializable]
        public class LogParam
        {
            //Phương thức
            public LogParam()
            {
                Attribute = new Dictionary<string, object>();
            }
            private Dictionary<string, object> _attribute;

            //Thuộc tính
            public Dictionary<string, object> Attribute
            {
                get { return _attribute; }
                set { _attribute = value; }
            }

            public object this[string attribute]
            {
                get
                {
                    return (Attribute.ContainsKey(attribute) ? Attribute[attribute] : null);
                }
                set
                {
                    if (Attribute.ContainsKey(attribute))
                    {
                        Attribute[attribute] = value;
                    }
                    else
                    {
                        Attribute.Add(attribute, value);
                    }
                }
            }
        }

        public enum LogType
        {
            Trace,
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }
    }
}
