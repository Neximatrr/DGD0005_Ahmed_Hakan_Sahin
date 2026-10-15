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

## Tick-rate test

Date: 15 October 2026. One server, two players and one spectator on the same computer at 127.0.0.1 / UDP 7770. Server out was measured after ten seconds of a rally. Run order: 10 Hz, 60 Hz, then back to 30 Hz in every copy.

| Tick rate | Server out B/s (rally) | Ball in a client window | Other paddle |
|---|---:|---|---|
| 10 | 1025 | Most visible steps and delay | Slowest updates |
| 30 | 1811 | Smoother, some delay at bounces | Smoother updates |
| 60 | 3911 | Smoothest, least delay | Quickest updates |

60 Hz used about 2.2 times the traffic of 30 Hz. It gave quicker ball and remote paddle updates. The ball stepped at low tick rates because it waited for server positions, while my own paddle moved locally each frame.

## Prediction grades

1. Right: NetworkTransform sent the owner's position through the server to the other clients.
2. Right: the server spawned and moved one ball; clients received its spawn and positions.
3. Right: the client-authoritative paddle position was trusted without a server speed check.

## Second partner meeting

Date: 15 October 2026. Partner: Berkay Şahin. Network: the same PongTest phone hotspot. My computer: 192.168.43.101; Berkay's computer: 192.168.43.102. Transport: Tugboat / UDP 7770. Tick rate: 30 Hz.

| Check | I host | Berkay hosts |
|---|---|---|
| My ClientId / side | 0 / LEFT | 1 / RIGHT |
| Partner ClientId / side | 1 / RIGHT | 0 / LEFT |
| My RTT / first try | 13 ms / Yes | 15 ms / Yes |
| Controls | Each player moved only their own paddle; movement reached both screens. | Same result after swapping roles. |
| Ball and serve | One grey ball. LEFT served and it turned white. Wrong-side and spectator Space did nothing. | Same result with Berkay as LEFT. |
| Rally | Wall and paddle bounces appeared on both screens. | Same result on the other host. |
| Goal | LEFT scored: 1 : 0 everywhere. Everyone received the goal announcement; only RIGHT received YOU SERVE. | Same score and RPC behaviour. |
| Late spectator | ClientId 2 / SPECTATOR joined at 2 : 1 and received the current score and six feed lines. No visible paddle. | Same result on the other host. |
| Feed limit | Join, leave, serve and goal appeared. A seventh line removed the oldest. | Same six-line limit on every client. |
| Disconnect | The guest left during a rally at 2 : 1. Their paddle disappeared, the ball stopped and the score stayed 2 : 1. | Same result when I disconnected. |
| Reconnect | Berkay returned as ClientId 3 / RIGHT with score 2 : 1 and the latest six events. | I returned as ClientId 3 / RIGHT with the same score and feed. |

## Section 14

1. Messages take time to travel, so machines see each other's past. The notes give Istanbul-Frankfurt a minimum of 9.5 ms one way, about 0.57 frames at 60 fps, and 19 ms minimum RTT. Measured RTT is 35-50 ms, about 2.1-3 frames for the round trip.

2. Pong uses a dedicated server in the four-window test. Player 2 is the server in Step 5. In Step 11, the hosting computer runs the server; both players connect as clients. If the server and local client run in one game process, that process is a host/listen server.

| | Dedicated server | Listen server | Peer-to-peer |
|---|---|---|---|
| Trust | Server | Host player | Peers' agreement |
| Cost | Server operation | Host CPU/upload | Peers' CPU/upload |
| Latency | Ping to server | Host local, guests delayed | Direct peer links |

3. UDP avoids waiting for a missing old packet before receiving newer updates. Ball positions can be unreliable because newer positions replace them. A goal announcement should be reliable.

4. NetworkObject gives an object its network identity, ownership and spawn handling. NetworkBehaviour gives a script network callbacks, SyncVars and RPCs. Hud has neither because it only displays local UI.

5. For client 0's paddle in a separate-server setup:

| Process | IsOwner | IsServerInitialized | Callback | Reads keyboard |
|---|---|---|---|---|
| Server | false | true | OnStartServer | No |
| Client 0 | true | false | OnStartClient | Yes |
| Client 1 | false | false | OnStartClient | No |

On a host, the same paddle can run both server and client callbacks. The local host's paddle can have both IsOwner and IsServerInitialized true.

6. NetworkTransform sends owner -> server -> other clients. Client Authoritative is on for paddles because their owners move them. It is off for the ball because the server moves it.

7. Court.Awake made one local ball in each running game copy. BallSpawner.OnStartServer creates one ball on the server and Spawn makes clients receive it. A client-only Spawn cannot create a shared network object.

8. Score, serve side and InPlay are state in SyncVars. LEFT scored is an ObserversRpc event; YOU SERVE is a TargetRpc event. A hit sound is an event sent to everyone with ObserversRpc. Ball position is state sent by NetworkTransform.

9. The server decides using its received paddle copies, so a client may see a hit but get a miss. A client writing InPlay directly does not make that write authoritative on the server. The owner must request a serve through ServerRpc so the server checks whose turn it is.

10. 8 x 30 x 40 = 9,600 B/s per client. Four clients need 38,400 B/s of server position traffic, excluding overhead. In the tick test, 10 Hz used less traffic but made the ball step and lag more. My own paddle still moved locally each frame.
