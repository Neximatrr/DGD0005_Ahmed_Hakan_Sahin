using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : NetworkBehaviour
{
    public static readonly List<PaddleController> All = new List<PaddleController>();
    public static PaddleController Local;
    public readonly SyncVar<int> Side = new SyncVar<int>(2);
    [SerializeField] private float speed = 8f;
    private float _serverAxis;

    private void Awake()
    {
        Side.OnChange += (previous, next, asServer) => Place();
    }

    public override void OnStartServer()
    {
        bool left = false;
        bool right = false;
        foreach (PaddleController paddle in All)
        {
            if (paddle.Side.Value == 0) left = true;
            if (paddle.Side.Value == 1) right = true;
        }
        Side.Value = !left ? 0 : !right ? 1 : 2;
        All.Add(this);
        Place();
        Debug.Log($"[Server] Client {OwnerId}: {SideName(Side.Value)}, Object {ObjectId}");
        GameState.Announce($"{SideName(Side.Value)} joined");
    }

    public override void OnStartClient()
    {
        if (!All.Contains(this)) All.Add(this);
        if (IsOwner)
        {
            Local = this;
            Debug.Log($"[Client] I am client {OwnerId}, side {SideName(Side.Value)}");
        }
        Place();
    }

    public override void OnStopServer()
    {
        All.Remove(this);
        GameState.Announce($"{SideName(Side.Value)} left");
        if (Side.Value < 2 && NetworkBall.Instance != null) NetworkBall.Instance.ResetBall();
    }

    public override void OnStopClient()
    {
        All.Remove(this);
        if (Local == this) Local = null;
    }

    private void Place()
    {
        GetComponent<Renderer>().enabled = Side.Value != 2;
        if (Side.Value == 2) return;
        Vector3 position = transform.position;
        position.x = Side.Value == 0 ? -Court.PaddleX : Court.PaddleX;
        position.y = 0.5f;
        transform.position = position;
        Court.Paint(gameObject, Side.Value == 0 ? new Color(0.35f, 0.65f, 1f) : new Color(1f, 0.60f, 0.25f));
    }

    public override void OnStartNetwork()
    {
        TimeManager.OnTick += OnTick;
    }

    public override void OnStopNetwork()
    {
        TimeManager.OnTick -= OnTick;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Side.Value == 2) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.spaceKey.wasPressedThisFrame) RequestServe();
    }

    private void OnTick()
    {
        if (IsOwner && Side.Value != 2)
        {
            float axis = 0;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) axis += 1;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) axis -= 1;
            }
            ServerSetAxis(axis);
        }
        if (IsServerInitialized)
        {
            transform.position += Vector3.forward * _serverAxis * speed * (float)TimeManager.TickDelta;
            Court.ClampPaddle(transform);
        }
    }

    [ServerRpc]
    private void ServerSetAxis(float axis)
    {
        _serverAxis = axis;
    }

    [ServerRpc]
    public void RequestServe()
    {
        NetworkBall ball = NetworkBall.Instance;
        if (Side.Value == 2 || Side.Value != GameState.Instance.ServingSide.Value || ball == null || ball.InPlay.Value) return;
        int players = 0;
        foreach (PaddleController paddle in All)
            if (paddle.Side.Value < 2) players++;
        if (players != 2) return;
        ball.Serve(1 - Side.Value);
        GameState.Announce($"{SideName(Side.Value)} served");
    }

    [ObserversRpc]
    public void AnnounceGoal(int left, int right)
    {
        ConnectionHud.Flash($"{SideName(Side.Value)} scored! {left} : {right}");
    }

    [TargetRpc]
    public void YouServe(NetworkConnection target)
    {
        ConnectionHud.Flash("YOU SERVE");
    }

    public static string SideName(int side)
    {
        return side == 0 ? "LEFT" : side == 1 ? "RIGHT" : "SPECTATOR";
    }
}
