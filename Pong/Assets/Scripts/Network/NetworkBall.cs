using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class NetworkBall : NetworkBehaviour
{
    public static NetworkBall Instance;
    public readonly SyncVar<bool> InPlay = new SyncVar<bool>();
    [SerializeField] private float speed = 7f;
    private Vector3 velocity;

    private void Awake()
    {
        Instance = this;
        InPlay.OnChange += (previous, next, asServer) => SetColour();
    }

    public override void OnStartServer()
    {
        ResetBall();
    }

    public override void OnStartClient()
    {
        SetColour();
    }

    public void SetColour()
    {
        Court.Paint(gameObject, InPlay.Value ? Color.white : Color.gray);
    }

    public void Serve(int towardSide)
    {
        if (!IsServerInitialized) return;
        float direction = towardSide == 0 ? -1f : 1f;
        velocity = new Vector3(direction, 0, Random.Range(-0.6f, 0.6f)).normalized * speed;
        InPlay.Value = true;
    }

    public void ResetBall()
    {
        if (!IsServerInitialized) return;
        InPlay.Value = false;
        velocity = Vector3.zero;
        transform.position = new Vector3(0, Court.BallY, 0);
        SetColour();
    }

    private void Update()
    {
        if (!IsServerInitialized) return;
        if (!InPlay.Value) return;
        Vector3 oldPosition = transform.position;
        Vector3 position = oldPosition + velocity * Mathf.Min(Time.deltaTime, 0.1f);
        float limit = Court.HalfHeight - Court.BallRadius;
        if (Mathf.Abs(position.z) > limit)
        {
            position.z = Mathf.Sign(position.z) * (2f * limit - Mathf.Abs(position.z));
            velocity.z = -velocity.z;
        }
        foreach (PaddleController paddle in PaddleController.All)
        {
            if (paddle.Side.Value == 2) continue;
            Vector3 paddlePosition = paddle.transform.position;
            float edge = paddlePosition.x + (paddle.Side.Value == 0 ? 0.5f : -0.5f);
            bool crossing = velocity.x < 0 ? oldPosition.x >= edge && position.x <= edge : oldPosition.x <= edge && position.x >= edge;
            bool facing = paddle.Side.Value == 0 ? velocity.x < 0 : velocity.x > 0;
            bool touching = Mathf.Abs(oldPosition.x - paddlePosition.x) <= 0.5f;
            if (!facing || (!crossing && !touching)) continue;
            float fraction = crossing ? (edge - oldPosition.x) / (position.x - oldPosition.x) : 0f;
            float hit = Mathf.Lerp(oldPosition.z, position.z, fraction) - paddlePosition.z;
            if (Mathf.Abs(hit) > Court.PaddleHalfLength + Court.BallRadius) continue;
            velocity = new Vector3(-Mathf.Sign(velocity.x), 0, hit).normalized * speed;
            position.x = crossing ? 2f * edge - position.x : edge;
        }
        transform.position = position;
        if (position.x < -Court.HalfWidth - 0.5f) GameState.Instance.Goal(1);
        else if (position.x > Court.HalfWidth + 0.5f) GameState.Instance.Goal(0);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
