using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Graphs.GUI.Execution
{
    public class GameThreadWorkerData
    {
        private readonly GameThreadWorker _worker;



        public GameThreadWorkerData(GameThreadWorker worker, object[] args)
        {
            _worker = worker;
            Args = args;
        }



        public Object[] Args
        {
            get;
            private set;
        }



        public bool Cancel
        {
            get;
            internal set;
        }



        public void ReportProgress(Object progressData)
        {
            _worker.ReportProgress(progressData);
        }
    }

    public class WorkerProgressData : EventArgs
    {
        public WorkerProgressData(object data)
        {
            Data = data;
        }



        public Object Data
        {
            get;
            private set;
        }
    }

    public class WorkerResultData : EventArgs
    {
        public Object Data
        {
            get;
            set;
        }



        public TimeSpan ElapsedTime
        {
            get;
            set;
        }



        public Exception Exception
        {
            get;
            set;
        }



        public bool Cancelled
        {
            get;
            set;
        }
    }


    public class GameThreadWorker
    {
        //The synchronizer will allow a synchronouse comunication with the game
        private readonly Synchronizer _synchronizer = new Synchronizer();

        //function to be called
        private readonly Func<GameThreadWorkerData, Object> _workFunction;

        //Data that will be passed to the function.
        private GameThreadWorkerData _data;

        /// <summary>
        /// Name, to be associate with the Thread.
        /// </summary>
        private string _name;

        /// <summary>
        /// Thread where the function will be executed.
        /// </summary>
        private Thread _thread;



        public GameThreadWorker(String name, Func<GameThreadWorkerData, object> workFunction)
        {
            _name = name;
            _workFunction = workFunction;
        }



        public bool IsRunning
        {
            get { return _thread != null; }
        }



        /// <summary>
        /// Name of the inside thread. 
        /// Should be set before calling Run().
        /// </summary>
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }



        /// <summary>
        /// Event called when the function calls "ReportProgress".
        /// </summary>
        public event EventHandler<WorkerProgressData> ProgressChanged = delegate { };



        /// <summary>
        /// Event called when the function finishes or is cancelled.
        /// It is not called if the function is aborted.
        /// </summary>
        public event EventHandler<WorkerResultData> Completed = delegate { };



        /// <summary>
        /// Executes the function in a background thread.
        /// </summary>
        /// <param name="args"></param>
        public void Run(params Object[] args)
        {
            _data = new GameThreadWorkerData(this, args);

            _thread = new Thread(ThreadFunction)
            {
                Name = _name,
                CurrentCulture = CultureInfo.InvariantCulture,
                CurrentUICulture = CultureInfo.InvariantCulture,
                IsBackground = true
            };
            _thread.Start();
        }



        private void ThreadFunction(Object obj)
        {
            WorkerResultData workerResultData = new WorkerResultData();

            //starts counting the time to deliver later in the statistics
            Stopwatch watch = Stopwatch.StartNew();

            //call the subfunction now, but protect it inside a try catch so that
            //we can deliver the exception in the result, in a clean way
            try
            {
                workerResultData.Data = _workFunction.Invoke(_data);
            }
            catch (Exception ex)
            {
                workerResultData.Exception = ex;
            }

            //close up and deliver the results
            workerResultData.ElapsedTime = watch.Elapsed;
            DeliverResult(workerResultData);
            _thread = null;
            _data = null;
        }



        /// <summary>
        /// This is called in the subthread, when the functions calls the ReportProgress from the GameThreadWorkerData.
        /// We need to invoke the event in the main thread.
        /// </summary>
        /// <param name="progressData"></param>
        internal void ReportProgress(Object progressData)
        {
            _synchronizer.Enqueue(() => ProgressChanged.Invoke(this, new WorkerProgressData(progressData)));
        }



        /// <summary>
        /// Invokes the proper finish event on the main thread.
        /// </summary>
        /// <param name="workerResultData"></param>
        internal void DeliverResult(WorkerResultData workerResultData)
        {
            _synchronizer.Enqueue(() => Completed.Invoke(this, workerResultData));
        }



        /// <summary>
        /// Updates the worker, essencial for synchronous response.
        /// </summary>
        /// <param name="span"></param>
        public void Update(TimeSpan span)
        {
            _synchronizer.Update();
        }



        /// <summary>
        /// Sends a signal to the function to cancel the process gracefully.
        /// </summary>
        public void Cancel()
        {
            if (_thread != null && _data != null)
            {
                lock (_data)
                {
                    _data.Cancel = true;
                }
            }
        }



        /// <summary>
        /// Aborts the thread.
        /// </summary>
        public void Abort()
        {
            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }
        }
    }
}