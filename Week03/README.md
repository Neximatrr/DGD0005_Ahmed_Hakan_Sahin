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
