using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Assets.Sceelix.Communication;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace Assets.Sceelix.Runtime
{
    public enum PackageLoadStatus
    {
        Pending,    //when we're still waiting for the server to be ready
        Loading,    //when we've sent the request, but are waiting for it to load
        Loaded      //when it is loaded
    }


    public class SceelixRuntimeManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Address of the Sceelix Runtime.")]
        public string Host = "127.0.0.1";

        [SerializeField]
        [Tooltip("Port to connect to the Sceelix Runtime.")]
        public int Port = 4000;
        
        [SerializeField]
        [Tooltip("Indicates if detailed connection logging should be performed.")]
        public bool LogConnection = false;

        [SerializeField]
        public int PingPeriod = 5000;

        [SerializeField]
        [Tooltip("Indicates if a local runtime server should be started on initialization.")]
        public bool StartLocalServer = true;

        [SerializeField]
        [Tooltip("Indicates if a local runtime server window should remain visible.")]
        public bool ShowLocalServer = false;

        private NetworkClient _networkClient;
        private Synchronizer _synchronizer = new Synchronizer();

        public event Action RuntimeManagerStatusChanged;

        private readonly Dictionary<TextAsset, PackageLoadStatus> _loadedPackages = new Dictionary<TextAsset, PackageLoadStatus>();
        
        

        public void Start()
        {
            Application.runInBackground = true;

            if (StartLocalServer)
                StartServerExecutable();

            ConnectToServer();
        }



        public void Awake()
        {
            //do not let this object be destroyed
            DontDestroyOnLoad(transform.gameObject);
        }



        private void StartServerExecutable()
        {
            var platform = Application.platform.ToString();
            String subfolder = "Windows";

            if (platform.Contains("OSX"))
                subfolder = "MacOS";
            else if (platform.Contains("Linux"))
                subfolder = "Linux";

            Process proc = new Process();
            proc.StartInfo.FileName = Path.Combine(Application.streamingAssetsPath, Path.Combine("Sceelix", Path.Combine(subfolder, "Sceelix.Runtime.exe")));
            proc.StartInfo.Arguments = " /port:" + Port + " /closeOnDisconnect + /serialization:Unity" ;

            if (!ShowLocalServer)
            {
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
            }
            
            proc.Start();
        }



        private void ConnectToServer()
        {
            new Thread(new ThreadStart(
                delegate {

                    while (true)
                    {
                        try
                        {
                            _networkClient = new NetworkClient(Host, Port, PingPeriod);
                            _networkClient.ClientDisconnected += NetworkClientOnClientDisconnected;
                            _networkClient.TokenMessageReceived += NetworkClientOnTokenMessageReceived;

                            if (LogConnection)
                                Debug.Log("Started Listening To Sceelix Runtime.");

                            _synchronizer.Enqueue(OnConnectedToServer);

                            break;
                        }
                        catch (Exception)
                        {
                            _networkClient = null;
                            Thread.Sleep(100);
                        }
                    }
                }
                ))
            { IsBackground = true }.Start();
        }



        private void NetworkClientOnClientDisconnected()
        {
            if (LogConnection)
                Debug.Log("Disconnected From Sceelix Designer.");

            _networkClient = null;
        }



        private void NetworkClientOnTokenMessageReceived(JToken token)
        {
            var subject = token.Value<String>("Subject");
            if (subject == "Project Loaded")
            {
                lock (_loadedPackages)
                {
                    var loadedProject = token.Value<String>("Data");
                    var textAsset = _loadedPackages.Keys.First(x => x.name == loadedProject);
                    _loadedPackages[textAsset] = PackageLoadStatus.Loaded;

                    if (RuntimeManagerStatusChanged != null)
                        RuntimeManagerStatusChanged.Invoke();
                }
            }
        }


        private void OnConnectedToServer()
        {
            lock (_loadedPackages)
            {
                //load all the pending projects
                foreach (TextAsset projectData in _loadedPackages.Keys.ToList())
                {
                    SendLoadProjectRequest(projectData);
                    _loadedPackages[projectData] = PackageLoadStatus.Loading;
                }
            }
        }

        

        public void LoadProject(TextAsset projectData)
        {
            PackageLoadStatus status;

            lock (_loadedPackages)
            {
                if (!_loadedPackages.TryGetValue(projectData, out status))
                {
                    _loadedPackages[projectData] = PackageLoadStatus.Pending;

                    //if we're connected, we can make the request now
                    if (IsConnected)
                    {
                        SendLoadProjectRequest(projectData);
                        _loadedPackages[projectData] = PackageLoadStatus.Loading;
                    }   
                }
            }
        }


        public void SendLoadProjectRequest(TextAsset projectData)
        {
            Dictionary<String, System.Object> loadPackageAssetMessage = new Dictionary<string, System.Object>();
            loadPackageAssetMessage["Name"] = projectData.name;
            loadPackageAssetMessage["Data"] = projectData.bytes;

            _networkClient.SendMessage("Load Project", loadPackageAssetMessage);
        }



        public void OnApplicationQuit()
        {
            Stop();
        }


        [ExecuteInEditMode]
        public void Update()
        {
            if (_networkClient != null)
                _networkClient.Synchronize();

            _synchronizer.Update();
        }





        public void Stop()
        {
            if (_networkClient != null)
            {
                _networkClient.Disconnect();
                _networkClient = null;

                if (LogConnection)
                    Debug.Log("Stopped Listening To Sceelix Designer.");
            }
        }





        public bool IsConnected
        {
            get { return _networkClient != null && _networkClient.Connected; }
        }

        


        public static SceelixRuntimeManager Instance
        {
            get
            {
                var gameObject = GameObject.Find("Sceelix Runtime Manager");
                if (gameObject != null)
                    return gameObject.GetComponent<SceelixRuntimeManager>();

                return null;
            }
        }
        
        

        public NetworkClient NetworkClient
        {
            get { return _networkClient; }
        }


        /// <summary>
        /// Indicates if a connection is active and if the server had its project loaded.
        /// </summary>
        public bool IsReady(TextAsset packageAsset)
        {
            lock (_loadedPackages)
            {
                return IsConnected && _loadedPackages.ContainsKey(packageAsset) && _loadedPackages[packageAsset] == PackageLoadStatus.Loaded;
            }
        }

        
    }
}
