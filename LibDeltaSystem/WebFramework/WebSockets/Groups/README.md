# WebSocket Groups
WebSocket groups are a way of distributing messages across websockets. There are a few key parts:

* ``WebSocketGroupQuery`` - This serves as an identifier for groups.
* ``WebSocketGroup`` - Holds a group of clients and has a sigle WebSocketGroupQuery identifier.
* ``WebSocketGroupHolder`` - Holds all groups on a single server.

When a new client attempts to connect, they will request a specific group using a WebSocketGroupQuery. If the group exists, they will be added to it. If it doesn't, a new group will be created.