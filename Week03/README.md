# Week 3

## Checkpoint D

1. NetworkObject gives the paddle its ObjectId.
2. The server chooses LEFT and writes Side. The client receives the SyncVar, and Place sets its position and colour.
3. `if (!IsOwner) return;` stops other clients reading the keyboard for this paddle.

## Predictions

1. The owner must send its paddle position to the server, then the server must send it to the other clients.
2. The server should control the one real ball. Clients need its spawn message and position updates to draw it.
3. A client could cheat by reporting a false position and teleporting its paddle to the ball.
