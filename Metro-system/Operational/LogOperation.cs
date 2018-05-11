/*----------------------------------------------------------------
// Author: ipepha@aliyun.com
// MIT lisence
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

using Newtonsoft.Json;
using Metro_system.Conf;

namespace RO.Tools
{
    public class Log
    {
        private static readonly string logFolder = Configurations.logFolder;
        private const int recordLogThreadSleepSeconds = 3;
        private static object recordLogLocker = new object();
        private static List<LogModel> processQueue;
        private static WarningErrorLogEventHandler warningErrorHandler;
        private static AllLogEventHandler allLogHandler;
        private static Thread logRecordThread;
        private static readonly Object processQueueLocker = new Object();

        private Type logType;

        static Log()
        {
            processQueue = new List<LogModel>();
            warningErrorHandler = null;
            allLogHandler = null;

            logRecordThread = new Thread(new ThreadStart(RecordLogThread));
            logRecordThread.Start();

            if (!Directory.Exists(logFolder))
            {
                try
                {
                    Directory.CreateDirectory(logFolder);
                }
                catch (Exception e)
                {
                    processQueue.Add(new LogModel(LogLevel.WARN, typeof(Log), "初始化日志工具失败. 无法保存日志文件.", e));
                }
            }
        }

        public Log(Type logType)
        {
            this.logType = logType;
        }

        public static string SerializeLog(List<LogModel> inputLogList)
        {
            if (inputLogList == null)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = inputLogList.Count - 1; i >= 0; i--)
            {
                var log = inputLogList[i];
                sb.AppendLine(log.ToBriefString());
            }

            return sb.ToString();
        }

        public void Debug(string log, object data = null)
        {
            var model = new LogModel(LogLevel.DEBUG, this.logType, log, data);
            processQueue.Add(model);
        }

        public void Error(string log, object data = null)
        {
            var model = new LogModel(LogLevel.ERR, this.logType, log, data);
            processQueue.Add(model);
        }

        public void Warning(string log, object data = null)
        {
            var model = new LogModel(LogLevel.WARN, this.logType, log, data);
            processQueue.Add(model);
        }

        public void Info(string log, object data = null)
        {
            var model = new LogModel(LogLevel.INFO, this.logType, log, data);
            processQueue.Add(model);
        }

        public static void Flush()
        {
            var missionList = GetItemFromProcessQueue(processQueue, limit: -1);
            if (missionList.Count != 0)
            {
                RecordLog(missionList);
            }
        }

        private static void RecordLogThread()
        {
            while (true)
            {
                try
                {
                    var missionList = GetItemFromProcessQueue(processQueue);

                    if (missionList.Count == 0)
                    {
                        Thread.Sleep(recordLogThreadSleepSeconds * 1000);
                        continue;
                    }

                    RecordLog(missionList);
                }
                catch (Exception e)
                {
                    var model = new LogModel(LogLevel.WARN, typeof(Log), "日志处理进程遇到错误. 正在重启. " + e.Message, e);
                    processQueue.Add(model);
                    Thread.Sleep(recordLogThreadSleepSeconds * 1000);
                }
            }
        }

        private static void RecordLog(List<LogModel> missionList)
        {
            lock (recordLogLocker)
            {

                StringBuilder logToFileString = new StringBuilder();
                foreach (var log in missionList)
                {
                    if (log.logLevel == LogLevel.ERR || log.logLevel == LogLevel.WARN)
                    {
                        try
                        {
                            warningErrorHandler?.Invoke(log);
                        }
                        catch (Exception e)
                        {
                            processQueue.Add(new LogModel(LogLevel.WARN, typeof(Log), "触发错误日志handler失败. " + e.Message, e));
                        }
                    }

                    logToFileString.AppendLine(log.ToDetailString());

                }

                try
                {
                    allLogHandler?.Invoke(missionList);
                }
                catch (Exception e)
                {
                    processQueue.Add(new LogModel(LogLevel.WARN, typeof(Log), "触发一般日志handler失败. " + e.Message, e));
                }

                try
                {
                    string fileName = DateTime.Now.ToString("yyyyMMdd") + ".log";
                    string path = logFolder + fileName;
                    FileStream fs = new FileStream(path, FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(logToFileString.ToString());
                    sw.Close();
                    fs.Close();
                }
                catch (Exception e)
                {
                    processQueue.Add(new LogModel(LogLevel.WARN, typeof(Log), "日志写入文件失败. " + e.Message, e));
                }
            }
        }

        private static string SerializeLogList(List<LogModel> input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var log in input)
            {
                sb.AppendLine(log.ToBriefString());
            }

            return sb.ToString();
        }

        public static List<T> GetItemFromProcessQueue<T>(List<T> input, int limit = 20)
        {
            lock (processQueueLocker)
            {
                List<T> result = new List<T>();

                if (input == null || input.Count == 0)
                {
                    return result;
                }

                //use range to increase speed
                if (input.Count > limit && limit >= 0)
                {
                    result = input.GetRange(0, limit);
                    input.RemoveRange(0, limit);
                }
                else
                {
                    result = input.GetRange(0, input.Count);
                    input.Clear();
                }

                return result;
            }
        }
    }

    public class LogModel
    {
        public LogLevel logLevel;
        public DateTime createdDate;
        public string log;
        public string sourceModule;
        public object data;

        public string ToBriefString()
        {
            return createdDate.ToString("yyyy-MM-dd HH:mm:ss:ffff") + " " + logLevel.ToString() + " [" + sourceModule + "] " + log;
        }

        public string ToDetailString()
        {
            string result = ToBriefString();
            if (data != null)
            {
                result += "\n" + JsonConvert.SerializeObject(data);
            }

            return result;
        }

        public LogModel()
        { }

        public LogModel(LogLevel level, Type sourceModule, string log, object data = null)
        {
            this.logLevel = level;
            this.log = log;
            this.sourceModule = sourceModule.ToString();
            this.data = data;
            this.createdDate = DateTime.Now;
        }


    }

    public delegate void WarningErrorLogEventHandler(LogModel log);

    public delegate void AllLogEventHandler(List<LogModel> log);

    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARN,
        ERR,
    }




}
