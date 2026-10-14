# Week 3

## Checkpoint D

1. NetworkObject gives the paddle its ObjectId.
2. The server chooses LEFT and writes Side. The client receives the SyncVar, and Place sets its position and colour.
3. `if (!IsOwner) return;` stops other clients reading the keyboard for this paddle.
