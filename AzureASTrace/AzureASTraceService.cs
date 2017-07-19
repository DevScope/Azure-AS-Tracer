using DevScope.Framework.Common.Logging;
using DevScope.Framework.Common.Utils;
using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Xml.Linq;
using DevScope.Framework.Common.Extensions;
using System.Xml;
using Microsoft.SqlServer.XEvent.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureASTrace
{
    public partial class AzureASTraceService : ServiceBase
    {
        private string currentFolder;
        private string traceId;
        private AdomdConnection conn;
        private Task workerTask;
        private CancellationTokenSource tokenSource;

        public AzureASTraceService()
        {
            InitializeComponent();
        }

        public void StartConsole(string[] args)
        {
            this.OnStart(args);
        }

        public void StopConsole()
        {
            this.OnStop();            
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.CurrentLogger = new NLogLogger();

                Logger.Log("Service Started");

                this.currentFolder = System.AppDomain.CurrentDomain.BaseDirectory;

                Logger.Log($"Execution Folder: '{currentFolder}'");

                // Setup AS Connection

                this.conn = GetASConnection();                

                this.traceId = SetupTrace();

                SubscribeTrace(this.traceId);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw;
            }
        }

        private string ResolveSettingsPath(string path)
        {
            if (path.StartsWith(".\\"))
            {
                path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, path.Remove(0, 2));
            }
            
            return path;
        }

        private void SubscribeTrace(string traceId)
        {
            this.tokenSource = new CancellationTokenSource();
                        
            this.workerTask = new Task(AsyncTraceReader, this.tokenSource.Token);

            this.workerTask.Start();             
        }

        private void AsyncTraceReader()
        {
            var outputFolder = ResolveSettingsPath(AppSettingsHelper.GetAppSetting("OutputFolder"));

            while (true)
            {
                try
                {
                    if (this.tokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    Logger.Log($"Subscribing Trace: {traceId}");                    

                    if(conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText =
                        $@"<Subscribe xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine"">
                                <Object xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine""> 
                                    <TraceID>{traceId}</TraceID>
                                 </Object>
                            </Subscribe>";

                        using (var inputReader = XmlReader.Create(cmd.ExecuteXmlReader(), new XmlReaderSettings() { Async = true }))
                        {
                            using (var data = new QueryableXEventData(inputReader, EventStreamSourceOptions.EventStream, EventStreamCacheOptions.CacheToDisk))
                            {
                                foreach (PublishedEvent evt in data)
                                {
                                    try
                                    {
                                        if (this.tokenSource.IsCancellationRequested)
                                        {
                                            break;
                                        }

                                        Logger.Debug($"{evt.Name} event received");                                                                 

                                        var eventId = Guid.NewGuid().ToString("N");

                                        var json = ConvertXEventToJSON(evt);

                                        var outputFile = Path.Combine(outputFolder, evt.Name, string.Concat(eventId, ".json"));

                                        var jsonStr = json.ToString(Newtonsoft.Json.Formatting.None);
                                        
                                        FileHelper.WriteAllTextAndEnsureFolder(outputFile, jsonStr);
                                    }
                                    catch(Exception ex)
                                    {
                                        Logger.Error(ex, "Error on QueryableXEventData");
                                    }                                                                                                                                
                                }

                                Logger.Log("Trace stopped");
                            }
                        }
                    }                    
                }   
                catch(Exception ex)
                {
                    Logger.Error(ex);
                }

                if (this.tokenSource.IsCancellationRequested)
                {
                    break;
                }

                Logger.Debug("Waiting 10s for server connection...");

                Thread.Sleep(10000);
            }
        }

        private JObject ConvertXEventToJSON(PublishedEvent evt)
        {
            var json = new JObject();

            json.Add("UUID", evt.UUID);
            json.Add("Timestamp", evt.Timestamp);
            json.Add("Name", evt.Name);

            var fields = new JObject();

            foreach(PublishedEventField field in evt.Fields)
            {
                fields.Add(field.Name, JToken.FromObject(field.Value));
            }
            
            json.Add("Fields", fields);            

            return json;
        }

        private List<dynamic> GetActiveTraces()
        {
            var list = DBHelper.ExecuteCommand<List<dynamic>>(this.conn, "select TraceId, CreationTime, StopTime, [Type] from $system.discover_traces");            

            return list;
        }

        private string SetupTrace()
        {
            var templateFilePath = ResolveSettingsPath(AppSettingsHelper.GetAppSetting("XEventTemplateFilePath"));

            var template = File.ReadAllText(templateFilePath);

            var ns = (XNamespace)"http://schemas.microsoft.com/analysisservices/2003/engine";

            var xml = XElement.Parse(template);

            var traceId = xml.TryGetElementValue(ns + "ObjectDefinition", ns + "Trace", ns + "ID");

            // Delete the Trace if one already exists with the same name

            var activeTraces = GetActiveTraces();
                        
            if (activeTraces.Any(s => traceId.EqualsCI((string)s.TraceId))) 
            {                
                DeleteTrace(traceId, this.conn);
            }

            // Create the trace

            DBHelper.ExecuteCommand<int>(this.conn, template);

            return traceId;
        }

        private void DeleteTrace(string traceId, AdomdConnection conn = null)
        {
            if (string.IsNullOrEmpty(traceId))
                throw new ArgumentNullException("traceId");

            var startedConnection = false;
                 
            try
            {

                Logger.Log($"Deleting Trace: {traceId}");

                // When Deleting Trace on stop its better to be a new connection the other could be blocked by the reader

                if (conn == null)
                {
                    conn = GetASConnection();
                    startedConnection = true;
                }                

                var deleteTraceCmd = $@"<Delete xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine"">
                              <Object>
                                <TraceID>{traceId}</TraceID>
                              </Object>
                            </Delete>";
                
                DBHelper.ExecuteCommand<int>(conn, deleteTraceCmd);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                if (startedConnection)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        private AdomdConnection GetASConnection()
        {
            var connStr = AppSettingsHelper.GetConnection("AnalysisServices").ConnectionString;

            var conn = new AdomdConnection(connStr);

            conn.Open();

            return conn;
        }

        protected override void OnStop()
        {
            try
            {
                Logger.Log("Stopping...");

                if (this.workerTask != null)
                {
                    this.tokenSource.Cancel();

                    Logger.Log("Waiting 30s for the Trace Reader to cancel...");
                    // Wait for the task for 30s
                    this.workerTask.Wait(30000);
                }
                
                if (!string.IsNullOrEmpty(this.traceId))
                {
                    this.DeleteTrace(this.traceId);
                }

                if (this.conn != null)
                {
                    this.conn.Close();
                    this.conn.Dispose();
                }

                Logger.Log("Service Stopped");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }
    }
}
