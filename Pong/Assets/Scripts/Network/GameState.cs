using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GameState : NetworkBehaviour
{
    public static GameState Instance;
    public readonly SyncVar<int> LeftScore = new SyncVar<int>();
    public readonly SyncVar<int> RightScore = new SyncVar<int>();
    public readonly SyncVar<int> ServingSide = new SyncVar<int>(0);
    public readonly SyncList<string> Feed = new SyncList<string>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ConnectionHud.Panels.Add(ScorePanel);
        ConnectionHud.Panels.Add(FeedPanel);
    }

    private string ScorePanel()
    {
        return $"LEFT {LeftScore.Value} : {RightScore.Value} RIGHT\nServe: {PaddleController.SideName(ServingSide.Value)}";
    }

    private string FeedPanel()
    {
        return string.Join("\n", Feed);
    }

    public override void OnStartServer()
    {
        LeftScore.Value = 0;
        RightScore.Value = 0;
        ServingSide.Value = 0;
        Feed.Clear();
        Announce("Waiting for players");
    }

    public static void Announce(string line)
    {
        if (Instance == null || !Instance.IsServerInitialized) return;
        Instance.Feed.Add(line);
        if (Instance.Feed.Count > 6) Instance.Feed.RemoveAt(0);
    }

    public void Goal(int scorerSide)
    {
        if (!IsServerInitialized || NetworkBall.Instance == null || !NetworkBall.Instance.InPlay.Value) return;
        if (scorerSide == 0) LeftScore.Value++;
        else RightScore.Value++;
        ServingSide.Value = 1 - scorerSide;
        NetworkBall.Instance.ResetBall();
        Announce($"{PaddleController.SideName(scorerSide)} scored");
        foreach (PaddleController paddle in PaddleController.All)
            if (paddle.Side.Value == scorerSide) paddle.AnnounceGoal(LeftScore.Value, RightScore.Value);
        foreach (PaddleController paddle in PaddleController.All)
            if (paddle.Side.Value == ServingSide.Value) paddle.YouServe(paddle.Owner);
    }

    private void OnDestroy()
    {
        ConnectionHud.Panels.Remove(ScorePanel);
        ConnectionHud.Panels.Remove(FeedPanel);
        if (Instance == this) Instance = null;
    }
}
