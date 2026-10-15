# Week 3

## Checkpoint D

1. NetworkObject gives the paddle its ObjectId.
2. The server chooses LEFT and writes Side. The client receives the SyncVar, and Place sets its position and colour.
3. `if (!IsOwner) return;` stops other clients reading the keyboard for this paddle.

## Predictions

1. The owner must send its paddle position to the server, then the server must send it to the other clients.
2. The server should control the one real ball. Clients need its spawn message and position updates to draw it.
3. A client could cheat by reporting a false position and teleporting its paddle to the ball.

## LAN connection card

Date: 15 October 2026. Partner: Berkay Şahin. Network: PongTest phone hotspot. Transport: Tugboat / UDP 7770. Tick rate: 30 Hz. The hosting computer started the server and connected its own client before the guest.

| Field | I host, Berkay joins | Berkay hosts, I join |
|---|---|---|
| Server IPv4 | 192.168.43.101 | 192.168.43.102 |
| Guest IPv4 | 192.168.43.102 | 192.168.43.101 |
| My ClientId / side | 0 / LEFT | 1 / RIGHT |
| Partner ClientId / side | 1 / RIGHT | 0 / LEFT |
| My client RTT | 12 ms | 14 ms |
| Worked first try? | Yes | Yes |
| Result | Both paddles moved on both screens. | Both paddles moved on both screens. |

Each keyboard controlled only its own paddle. The host's player was blue LEFT and the guest was orange RIGHT. We swapped server roles and got the same result.

## Design sheet

| Thing | State or event? | FishNet piece | On which object? | Who writes / calls? |
|---|---|---|---|---|
| Ball position | State | NetworkTransform | Ball | Server |
| Ball in play? | State | SyncVar | Ball | Server |
| Two scores | State | SyncVar | GameState | Server |
| Whose serve | State | SyncVar | GameState | Server |
| Let me serve | Event | ServerRpc | Paddle | Owner |
| LEFT scored! to all | Event | ObserversRpc | Scorer's paddle | Server |
| YOU SERVE to one | Event | TargetRpc | Conceder's paddle | Server |
| Event feed | State | SyncList | GameState | Server |

BallSpawner on Court creates the ball in OnStartServer at (0, Court.BallY, 0) and spawns it on the network. NetworkBall.Update starts with `if (!IsServerInitialized) return;`, so only the server moves the ball.

## Step 8

1. A hit sound for everyone uses ObserversRpc. A "not your serve" message for one player uses TargetRpc.
2. LeftScore and RightScore are SyncVars on GameState. Only the server writes them.
3. The server uses its received paddle copy. Network delay and interpolation make that position different from the owner's local position, so a hit on the player's screen can still be a miss on the server.
