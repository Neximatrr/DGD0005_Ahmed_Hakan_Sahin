# Pong

Ahmed Hakan Şahin - 2400004772

Each player owns one paddle. The ball has no client owner and only the server moves it. Everyone shares the score, whose serve it is and the last six events. A third client joins as a spectator.

Unity 6000.3.2f1, FishNet 4.7.3R, Multiplayer Play Mode 2.0.2.

## Run

1. Open `Assets/Scenes/Pong.unity` in Unity 6000.3.2f1, use 30 Hz and press Play. Start Server on the hosting computer.
2. Start Client on the hosting computer using `127.0.0.1`.
3. On the other computer, use the server's IPv4 address and Start Client. Both computers must be on the same phone hotspot; Tugboat uses UDP 7770.

Move with W/S or the arrow keys. Press Space when it is your serve.

For Multiplayer Play Mode, activate Players 2, 3 and 4 and tag Player 2 `Server`. Start clients in the main editor, Player 3 and Player 4, in that order.

[Week 3 answers and tests](Week03/README.md)

[Week 4 answers and tests](Week04/README.md)
