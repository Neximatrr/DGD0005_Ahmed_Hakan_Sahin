using FishNet.Object;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{
    public NetworkObject ballPrefab;

    public override void OnStartServer()
    {
        NetworkObject ball = Instantiate(ballPrefab, new Vector3(0, Court.BallY, 0), Quaternion.identity);
        Spawn(ball);
    }
}
