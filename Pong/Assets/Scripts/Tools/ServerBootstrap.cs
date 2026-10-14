using System;
using System.Collections;
using FishNet;
using UnityEngine;
#if UNITY_EDITOR
using Unity.Multiplayer.PlayMode;
#endif

public class ServerBootstrap : MonoBehaviour
{
    private IEnumerator Start()
    {
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        yield return null;
        string[] args = Environment.GetCommandLineArgs();
        int client = Array.IndexOf(args, "-client");
        int tick = Array.IndexOf(args, "-tick");
        if (tick >= 0 && tick + 1 < args.Length && ushort.TryParse(args[tick + 1], out ushort rate))
            InstanceFinder.TimeManager.SetTickRate((ushort)Mathf.Clamp(rate, 1, 120));
        bool server = Array.IndexOf(args, "-server") >= 0 || (Application.isBatchMode && client < 0);
#if UNITY_EDITOR
        foreach (string tag in CurrentPlayer.ReadOnlyTags())
            if (tag == "Server") server = true;
#endif
        if (server) InstanceFinder.ServerManager.StartConnection();
        else if (client >= 0 && client + 1 < args.Length)
            InstanceFinder.ClientManager.StartConnection(args[client + 1], 7770);
    }
}

