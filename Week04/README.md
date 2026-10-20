# Week 4

## Step 2

I tested one server and two clients on my PC at 30 Hz. The client RTT was 17 ms. Both paddles moved on both clients and stayed inside the court.

## Step 3

`axis = Mathf.Clamp(axis, -1f, 1f);` changes 50 to 1 and runs on the server, so the paddle moves at normal speed.
