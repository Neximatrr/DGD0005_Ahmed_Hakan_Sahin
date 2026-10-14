using System;
using System.Collections.Generic;
using System.Reflection;
using FishNet;
using FishNet.Editing.NetworkProfiler;
using FishNet.Managing;
using FishNet.Managing.Statistic;
using FishNet.Transporting;
using UnityEngine;

public class ConnectionHud : MonoBehaviour
{
    public static ConnectionHud Instance;
    public static readonly List<Func<string>> Panels = new List<Func<string>>();
    public double BytesIn { get; private set; }
    public double BytesOut { get; private set; }
    private NetworkManager manager;
    private NetworkTrafficStatistics statistics;
    private string address = "127.0.0.1";
    private string state = "Stopped";
    private string message = "";
    private float messageUntil;
    private double received;
    private double sent;
    private float lastSample;

    private void Awake()
    {
        Instance = this;
        Panels.Clear();
    }

    private void Start()
    {
        manager = InstanceFinder.NetworkManager;
        manager.ClientManager.OnClientConnectionState += ClientState;
        if (manager.StatisticsManager.TryGetNetworkTrafficStatistics(out statistics))
            statistics.OnNetworkTraffic += Traffic;
        lastSample = Time.realtimeSinceStartup;
    }

    private void ClientState(ClientConnectionStateArgs args)
    {
        state = args.ConnectionState.ToString();
    }

    private void Traffic(uint tick, BidirectionalNetworkTraffic server, BidirectionalNetworkTraffic client)
    {
        object traffic = manager.IsServerStarted ? server : client;
        received += ReadBytes(traffic, "InboundTraffic");
        sent += ReadBytes(traffic, "OutboundTraffic");
    }

    private ulong ReadBytes(object traffic, string direction)
    {
        object data = traffic.GetType().GetField(direction, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(traffic);
        return data == null ? 0 : (ulong)data.GetType().GetField("Bytes").GetValue(data);
    }

    private void Update()
    {
        float now = Time.realtimeSinceStartup;
        if (now - lastSample < 1f) return;
        BytesIn = received / (now - lastSample);
        BytesOut = sent / (now - lastSample);
        received = sent = 0;
        lastSample = now;
    }

    public static void Flash(string text)
    {
        ConnectionHud hud = Instance;
        hud.message = text;
        hud.messageUntil = Time.unscaledTime + 2f;
    }

    private void OnGUI()
    {
        if (manager == null) return;
        float scale = Mathf.Min(Screen.width / 960f, Screen.height / 640f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
        GUI.skin.label.fontSize = 16;
        GUI.skin.button.fontSize = 16;
        GUI.skin.textField.fontSize = 16;
        GUILayout.BeginArea(new Rect(18, 12, 924, 85));
        GUILayout.BeginHorizontal();
        if (!manager.IsClientStarted)
        {
            address = GUILayout.TextField(address, GUILayout.Width(180));
            GUI.enabled = state == "Stopped";
            if (GUILayout.Button("Start Client", GUILayout.Width(125))) manager.ClientManager.StartConnection(address, 7770);
            if (!manager.IsServerStarted && GUILayout.Button("Start Server", GUILayout.Width(125))) manager.ServerManager.StartConnection();
            GUI.enabled = true;
        }
        if ((manager.IsServerStarted || manager.IsClientStarted) && GUILayout.Button(manager.IsServerStarted ? "Stop Server" : "Disconnect", GUILayout.Width(125)))
        {
            if (manager.IsServerStarted) manager.ServerManager.StopConnection(true);
            else manager.ClientManager.StopConnection();
        }
        GUILayout.FlexibleSpace();
        GUI.enabled = !manager.IsClientStarted && !manager.IsServerStarted;
        GUILayout.Label("Tick rate", GUILayout.Width(70));
        foreach (ushort rate in new ushort[] { 10, 30, 60 })
        {
            GUI.color = manager.TimeManager.TickRate == rate ? Color.cyan : Color.white;
            if (GUILayout.Button(rate.ToString(), GUILayout.Width(42))) manager.TimeManager.SetTickRate(rate);
        }
        GUI.color = Color.white;
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        string role = manager.IsServerStarted ? "SERVER" : PaddleController.Local != null ? PaddleController.SideName(PaddleController.Local.Side.Value) : state.ToUpper();
        int clients = manager.IsServerStarted ? manager.ServerManager.Clients.Count : manager.ClientManager.Clients.Count;
        GUILayout.Label($"{role}   |   Clients: {clients}   |   RTT: {manager.TimeManager.RoundTripTime} ms   |   Tick: {manager.TimeManager.Tick} / {manager.TimeManager.TickRate} Hz");
        GUILayout.Label($"In: {BytesIn:0} B/s   |   Out: {BytesOut:0} B/s");
        GUILayout.EndArea();
        if (manager.IsServerStarted || manager.IsClientStarted)
        {
            if (Panels.Count > 0)
                GUI.Label(new Rect(145, 91, 455, 64), Panels[0](), new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter });
            if (Panels.Count > 1)
                GUI.Label(new Rect(750, 185, 190, 280), Panels[1]());
        }
        if (Time.unscaledTime < messageUntil)
            GUI.Label(new Rect(195, 300, 360, 40), message, new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter });
    }

    private void OnDestroy()
    {
        if (statistics != null) statistics.OnNetworkTraffic -= Traffic;
        if (manager != null) manager.ClientManager.OnClientConnectionState -= ClientState;
        if (Instance == this) Instance = null;
    }
}
