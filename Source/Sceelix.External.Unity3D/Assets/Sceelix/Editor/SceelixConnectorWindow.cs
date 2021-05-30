using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Assets.Sceelix.Communication;
using Assets.Sceelix.Editor;
using Assets.Sceelix.Processors.Messages;
using Assets.Sceelix.Runtime;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;


[InitializeOnLoad]
[ExecuteInEditMode]
public class SceelixConnectorWindow : EditorWindow
{
    //private MessageServer _messageServer;
    private NetworkClient _messageClient;

    private static Dictionary<String, MessageProcessor> _messageProcessors;
    
    /// <summary>
    /// Indicates if the user has intentionally started the connection.
    /// </summary>
    [SerializeField]
    private bool _userConnected;

    [SerializeField]
    private bool _logConnection;

    [SerializeField]
    private string _host = "127.0.0.1";

    [SerializeField]
    private int _port = 3500;

    [SerializeField]
    private int _pingPeriod = 5000;

    [SerializeField]
    private bool _removeOnRegeneration = true;

    [SerializeField]
    private bool _frameResult = true;

    [SerializeField]
    private bool _storePhysicalAssets = false;

    [SerializeField]
    private bool _createPrefab = false;

    [SerializeField]
    private bool _addToScene = true;

    [SerializeField]
    private string _assetsFolder = "Assets/SceelixAssets";
    
    [SerializeField]
    private bool _autoCleanup = false;

    //use this to attempt reconnections to the server
    private Timer _pingTimer;

    //used to execute actions that must be performed in the main thread
    private readonly Synchronizer _synchronizer = new Synchronizer();
    
    private Vector2 _scrollPosition;




    // Add menu named "My Window" to the Window menu
    [MenuItem("Tools/Sceelix")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SceelixConnectorWindow window = (SceelixConnectorWindow)GetWindow(typeof(SceelixConnectorWindow));
        window.Show();
    }


    public void OnEnable()
    {
        titleContent = new GUIContent("Sceelix");

        if (_pingPeriod > 0)
        {
            _pingTimer = new Timer(PingTimerFunction, null, 0, _pingPeriod);
        }
        else
        {
            _pingTimer.Dispose();
        }
    }


    private void PingTimerFunction(object state)
    {
        //if we are not connected, but the user wants us to be, 
        //try to start the connection again
        if (_userConnected && !IsConnected)
        {
            Start();
        }
    }


    public void OnDestroy()
    {
        //close the connection
        Stop();
    }


    public void Update()
    {
        if(IsConnected)
            _messageClient.Synchronize();

        //It is imperative to close the socket on compilation, otherwise Unity will hang
        if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Stop();
        }
        if (_messageProcessors == null)
        {
            InitializeProcessors();
        }
        //restart the connection if we stopped previously
        /*else if (_userConnected && ! IsConnected)
        {
            Start();
        }*/
        _synchronizer.Update();
    }



    public void OnGUI()
    {
        EditorGUIUtility.labelWidth = 200;
        var labelStyle = new GUIStyle();
        labelStyle.fixedWidth = EditorGUIUtility.labelWidth;

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        GUILayout.BeginVertical(new GUIStyle {padding = new RectOffset(10, 10, 10, 10) });

        //connection section
        GUILayout.Label(new GUIContent("Connection","Enables or disables the connection between Sceelix and Unity."), new GUIStyle() { fontStyle = FontStyle.Bold });

        UserConnected = EditorGUILayout.Toggle(new GUIContent("On:","Enables or disables the connection to the Sceelix Designer."), _userConnected);

        
        //Indicator if we are connected or not
        GUILayout.BeginHorizontal();

        GUILayout.Label(new GUIContent("Status","Indicates the current status of the connection. The connection checkbox must be enabled and the Sceelix Designer must be running for a connection to be possible."), labelStyle);

        var connectionLabelStyle = new GUIStyle();
        connectionLabelStyle.fontStyle = FontStyle.Bold;

        if (IsConnected)
        {
            connectionLabelStyle.normal.textColor = new Color(0, 0.62f, 0);
            GUILayout.Label("CONNECTED", connectionLabelStyle);
        }
        else
        {
            connectionLabelStyle.normal.textColor = new Color(0.62f, 0, 0);
            GUILayout.Label("NOT CONNECTED", connectionLabelStyle);
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //options for configuring the connection
        GUILayout.Label("Connection Options", new GUIStyle { fontStyle = FontStyle.Bold });

        Host = EditorGUILayout.TextField(new GUIContent("Host:","The address of machine hosting Sceelix. Set as 127.0.0.1 if Sceelix is running on the same machine."), _host);
        Port = EditorGUILayout.IntField(new GUIContent("Port:", "The connection port to the Sceelix Designer. It should match the configuration set in the Sceelix Designer."), _port);
        _logConnection = EditorGUILayout.Toggle(new GUIContent("Log:","If enabled, events and statuses of the connection will be logged to the console."), _logConnection);
        PingPeriod = EditorGUILayout.IntField(new GUIContent("Ping period:", "The interval of time between server pings, so as to try to automatically connect."), PingPeriod);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //options for configuring generation details
        GUILayout.Label("Generation Options", new GUIStyle { fontStyle = FontStyle.Bold });
        _addToScene = EditorGUILayout.Toggle(new GUIContent("Add to Scene:", "Indicates if the result should be added to the current scene. Enabled by default, but can be disable in case only asset and/or prefab creation is intended."), _addToScene);
        _frameResult = EditorGUILayout.Toggle(new GUIContent("Frame Result:", "If enabled, the generated result will be zoomed in the 3D View."), _frameResult);
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //options for configuring how the scene objects should be generated
        GUILayout.Label("Default Scene Object Options", new GUIStyle { fontStyle = FontStyle.Bold });
        _removeOnRegeneration = EditorGUILayout.Toggle(new GUIContent("Remove on regeneration:","Indicates if future generated results will have this flag on or off. If enabled, they will be removed from the scene if new ones are generated."), _removeOnRegeneration);

        EditorGUILayout.Space();
        EditorGUILayout.Space();


        //options for clearing assets and stuff
        GUILayout.Label("Asset Generation", new GUIStyle {fontStyle = FontStyle.Bold});
        _storePhysicalAssets = EditorGUILayout.Toggle(new GUIContent("Store Physical Assets:","Indicates if, when transferring textures, materials and meshes, these should be persisted to disk (will be stored under 'SceelixAssets' by default). Doing this takes more time in the first run, but may be useful in some cases. If disabled, these assets will only be stored in memory and directly in the scene file."), _storePhysicalAssets);
        _createPrefab = EditorGUILayout.Toggle(new GUIContent("Create Prefab:", "Indicates if a prefab containing the whole scene should be created. Requires the 'Store Physical Assets' option to be enabled to work properly. Repeated prefabs with the same name will be overwritten."), _createPrefab);
        _assetsFolder = EditorGUILayout.TextField(new GUIContent("Assets Folder:", "Indicates where to store the physical assets (if the previous option is enabled)."), _assetsFolder);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.Label("Asset Cleanup", new GUIStyle { fontStyle = FontStyle.Bold });

        _autoCleanup = EditorGUILayout.Toggle(new GUIContent("Auto Cleanup:", "Automatically (after each generation) perform an analysis on project's scenes and prefabs to remove unused assets. This could introduce a significant overhead if there are many prefabs/scenes/assets. The alternative is to use the manual button below."), _autoCleanup);

        GUILayout.BeginHorizontal();
        
        GUILayout.Label(new GUIContent("Clean Unused","Removes all generated physical assets (located in the 'Assets Folder') that are not being referenced in any scene or prefab."),labelStyle);

        if (GUILayout.Button("Clean"))
            AssetReferenceManager.CleanupAndUpdate(_assetsFolder);
        
        GUILayout.EndHorizontal();



        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.Label("Runtime", new GUIStyle { fontStyle = FontStyle.Bold });

        GUILayout.BeginHorizontal();

        GUILayout.Label(new GUIContent("Add Runtime Manager", "Adds a Runtime Manager Object to the Scene, which is essential to use the Sceelix Runtime in-game."), labelStyle);

        if (GUILayout.Button("Add"))
        {
            GameObject gameObject = new GameObject("Sceelix Runtime Manager");
            gameObject.AddComponent<SceelixRuntimeManager>();
        }
        GUILayout.EndHorizontal();




        GUILayout.EndVertical();

        GUILayout.EndScrollView();
    }
    


    /// <summary>
    /// Starts the Sceelix Connector using the default hostname and port.
    /// </summary>
    public void Start()
    {
        Start(_host, _port, _pingPeriod);
    }



    /// <summary>
    /// Starts the Sceelix Connector using the indicated hostname and port.
    /// </summary>
    public void Start(String hostName, int port, int poolPeriod)
    {
        //make sure we close a previously existing connection
        if (IsConnected)
            _messageClient.Disconnect();

        try
        {
            _messageClient = new NetworkClient(hostName, port, poolPeriod);
            _messageClient.RawMessageReceived += OnRawMessageReceived;
            _messageClient.TokenMessageReceived += MessageClientOnTokenMessageReceived;
            _messageClient.ClientDisconnected += MessageClientOnClientDisconnected;

            if (_logConnection)
                Debug.Log("Started Listening To Sceelix Designer.");

            //update the status of the window in the main thread
            _synchronizer.Enqueue(() => { Repaint();});
        }
        catch (SocketException ex)
        {
            //do not nag us with a message every time this fails
            if (_logConnection)
                Debug.LogError(ex.ToString());
        }
    }




    private void MessageClientOnTokenMessageReceived(JToken token)
    {
        MessageProcessor messageProcessorAttribute;

        //by default, Sceelix returns a message with the subject first
        var subject = token.Value<String>("Subject");
        var data = token["Data"];

        //look for registered message processors and invoke them
        if (_messageProcessors.TryGetValue(subject, out messageProcessorAttribute))
        {
            var editorGenerationContext = new EditorGenerationContext();
            editorGenerationContext.RemoveOnRegeneration = _removeOnRegeneration;
            editorGenerationContext.StorePhysicalAssets = _storePhysicalAssets;
            editorGenerationContext.AssetsFolder = _assetsFolder;
            editorGenerationContext.FrameResult = _frameResult;
            editorGenerationContext.CreatePrefab = _createPrefab;
            editorGenerationContext.AddToScene = _addToScene;
            editorGenerationContext.AutoCleanup = _autoCleanup;

            messageProcessorAttribute.Process(editorGenerationContext, data);
        }
        else
        {
            Debug.LogWarning(String.Format("There is no defined processor for message subject {0}.", subject));
        }
    }






    

    private void MessageClientOnClientDisconnected()
    {
        if (_logConnection)
            Debug.Log("Disconnected From Sceelix Designer.");

        _messageClient = null;
        _synchronizer.Enqueue(() => Repaint());
    }



    private void OnRawMessageReceived(string message)
    {
        //not really in use, but sometimes it is useful to debug the connection
        if (_logConnection)
            Debug.Log("Raw message received.");
    }





    [DidReloadScripts]
    public static void InitializeProcessors()
    {
        _messageProcessors = ProcessorAttribute.GetClassesOfType<MessageProcessor>();

        //DefaultMessageManager.InitializeProcessors();
    }


    public void Stop()
    {
        if (_messageClient != null)
        {
            _messageClient.Disconnect();
            _messageClient = null;

            if (_logConnection)
                Debug.Log("Stopped Listening To Sceelix Designer.");
        }
    }





    public bool IsConnected
    {
        get { return _messageClient != null && _messageClient.Connected; }
    }


    
    public bool UserConnected
    {
        get { return _userConnected; }
        set
        {
            //act only if there was a change
            if (_userConnected != value)
            {
                //if it was turned on, start the listener
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }

                _userConnected = value;
            }
        }
    }



    public string Host
    {
        get { return _host;}
        set
        {
            if (_host != value)
            {
                _host = value;

                if (_userConnected)
                    Start();
            }
        }
    }



    public int Port
    {
        get { return _port; }
        set
        {
            if (_port != value)
            {
                _port = value;

                if(_userConnected)
                    Start();
            }
        }
    }



    public int PingPeriod
    {
        get { return _pingPeriod; }
        set
        {
            if (_pingPeriod != value)
            {
                _pingPeriod = value;

                if (_messageClient != null)
                    _messageClient.PingPeriod = _pingPeriod;

                //if the value of the ping period is 0 (or less), disable the polling
                if(_pingPeriod > 0)
                    _pingTimer.Change(0, _pingPeriod);
                else
                {
                    _pingTimer.Dispose();
                }
            }
        }
    }
}
