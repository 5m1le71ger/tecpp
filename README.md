TecPlusPlus

WPF client that connects to a Multi-User Dungeon(MUD) called <a href="http://www.skotos.net/games/eternal-city">The Eternal City</a> via Sockets. It's an exercise in handling sockets asynchronously (fun with multi-threading), and heavy string parsing (making sense of non-character data to trigger text colors, styles, spacing etc), as well as tooling additions that enhance the playability of the game.

Some functionality and connection magic achieved through reverse engineering the <a href="http://www.skotos.net/Zealotry/">Zealotry</a> client.

Next steps:
- Should probably comment/document this sometime...
- Move functionality out into separate code files.
- Build more of a library/framework hierarchy out of the components.
- Then, and only then... shall we add a custom macro/scripting framework for gameplay automation!

