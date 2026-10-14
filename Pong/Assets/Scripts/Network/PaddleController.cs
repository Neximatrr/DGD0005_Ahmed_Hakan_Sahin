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

    private void Update()
    {
        if (!IsOwner) return;
        if (Side.Value == 2) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        float direction = 0;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) direction += 1;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) direction -= 1;
        transform.position += Vector3.forward * direction * speed * Time.deltaTime;
        Court.ClampPaddle(transform);
    }

    public static string SideName(int side)
    {
        return side == 0 ? "LEFT" : side == 1 ? "RIGHT" : "SPECTATOR";
    }
}
