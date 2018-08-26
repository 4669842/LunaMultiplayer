<p align="center">
    <img src="../master/External/logo.png" alt="Luna multiplayer logo"/>
    <a href="https://www.youtube.com/watch?v=gf6xyLnpnoM"><img src="https://img.youtube.com/vi/gf6xyLnpnoM/0.jpg" alt="Video" height="187" width="250"/></a>
</p>

<p align="center">
    <a href="https://paypal.me/gavazquez"><img src="https://img.shields.io/badge/paypal-donate-yellow.svg" alt="PayPal"/></a>
    <a href="https://discord.gg/wKVMhWQ"><img src="https://img.shields.io/discord/378456662392045571.svg" alt="Chat on discord"/></a>
    <a href="../../releases"><img src="https://img.shields.io/github/release/lunamultiplayer/lunamultiplayer.svg" alt="Latest release" /></a>
    <a href="../../releases"><img src="https://img.shields.io/github/downloads/lunamultiplayer/lunamultiplayer/total.svg" alt="Total downloads" /></a>
</p>

<p align="center">
    <a href="https://forum.kerbalspaceprogram.com/index.php?/topic/168271-131-luna-multiplayer-lmp-alpha"><img src="https://img.shields.io/badge/KSP%20Forum-Post-4265f4.svg" alt="KSP forum post"/></a>
    <a href="https://github.com/LunaMultiplayer/LunaMultiplayerUpdater"><img src="https://img.shields.io/badge/Automatic-Updater-4265f4.svg" alt="Latest build updater"/></a>
</p>

---

<p align="center">
  <a href="../../releases/latest"><img src="../master/External/downloadIcon.png" alt="Download" height="85" width="300"/></a>
  <a href="../../wiki"><img src="../master/External/documentationIcon.png" alt="Documentation" height="85" width="353"/></a>
</p>

---
[![Codefresh build status]( https://g.codefresh.io/api/badges/pipeline/4669842/4669842%2FLunaMultiplayer%2FLunaMultiplayer?branch=master&key=eyJhbGciOiJIUzI1NiJ9.NWI4MjEzMTJkZGQ1NGEwMDAxMWYxYWQ5.dV3ijq5Ia7d9R2ppVk3l64Hrsz9AQkeOXidBh8UoUDE&type=cf-2)]( https://g.codefresh.io/repositories/4669842/LunaMultiplayer/builds?filter=trigger:build;branch:master;service:5b82134baedcd0cf6eccbf19~LunaMultiplayer)

# Luna Multiplayer Mod (LMP)

*Multiplayer mod for [Kerbal Space Program (KSP)](https://kerbalspaceprogram.com)*

### Main features:

- [x] Clean and optimized code, based on systems and windows which makes it easier to read and modify.
- [x] Multi threaded.
- [x] [NTP](https://en.wikipedia.org/wiki/Network_Time_Protocol) protocol to sync the time between clients and the server.
- [x] [UDP](https://en.wikipedia.org/wiki/User_Datagram_Protocol) based using the [Lidgren](https://github.com/lidgren/lidgren-network-gen3) library for reliable UDP message handling.
- [x] [Interpolation](http://www.gabrielgambetta.com/entity-interpolation.html) so the vessels won't jump when there are bad network conditions.
- [x] Multilanguage.
- [x] [Nat-punchtrough](../../wiki/Master-server) feature so a server doesn't need to open ports on it's router.
- [x] Servers displayed within the mod.
- [x] Settings saved as XML.
- [x] [UPnP](https://en.wikipedia.org/wiki/Universal_Plug_and_Play) support for servers and [master servers](../../wiki/Master-server)
- [x] Better creation of network messages so they are easier to modify and serialize.
- [x] Every network message is cached in order to reduce the garbage collector spikes.
- [x] Based on tasks instead of threads.
- [x] Supports career and science modes.
- [ ] [QuickLZ](http://www.quicklz.com) for fast compression.
- [ ] Support for groups/companies inside career and science modes.

Please check the [wiki](../../wiki) to see how to [install](../../wiki/How-to-install-LMP), [run](../../wiki/How-to-play-with-LMP), [build](../../wiki/How-to-compile-LMP) or [debug](../../wiki/Debugging-in-Visual-studio) LMP among other things

---
### Servers:

Check how many servers are up and running either in [Release](../../wiki/How-to-get-the-latest-version-of-LMP) or in [Nightly](../../wiki/How-to-get-nightly-builds) versions

| [Release servers](http://37.187.108.37:8701) | [Nightly servers](http://dagger.ole32.com:8751) |
| ------------------------------------------------------  | ------------------------------------------------------- |
---

### Status:

|   Branch   |   Build  |   Tests  |  Last commit  |   Activity    |    Commits    |
| ---------- | -------- | -------- | ------------- | ------------- | ------------- |
| **master** |[![AppVeyor](https://img.shields.io/appveyor/ci/gavazquez/lunamultiplayer/master.svg?logo=appveyor)](https://ci.appveyor.com/project/gavazquez/lunamultiplayer/branch/master) | [![AppVeyor Tests](https://img.shields.io/appveyor/tests/gavazquez/lunamultiplayer/master.svg?logo=appveyor)](https://ci.appveyor.com/project/gavazquez/lunamultiplayer/branch/master/tests) | [![Last commit](https://img.shields.io/github/last-commit/lunamultiplayer/lunamultiplayer/master.svg)](../../commits/master) | [![Commit activity](https://img.shields.io/github/commit-activity/y/lunamultiplayer/lunamultiplayer.svg)](../../commits/master) | [![Commits since release](https://img.shields.io/github/commits-since/lunamultiplayer/lunamultiplayer/latest.svg)](../../commits/master)

---

### Contributing:

Consider [donating trough paypal](https://paypal.me/gavazquez) if you like this project. 
It will encourage us to do future releases, fix bugs and add new features :star:

Please write the code as you were going to leave it, return after 1 year and you'd have to understand what you wrote.  
It's **very** important that the code is clean and documented so in case someone leaves, another programmer could take and maintain it. Bear in mind that **nobody** likes to take a project where it's code looks like a dumpster.

There's also a test project in case you want to add tests to your code.

---

### Troubleshooting:

Please visit [this page](../../wiki/Troubleshooting) in the wiki to solve the most common issues with LMP 
[![Analytics](https://ga-beacon.appspot.com/UA-118326748-1/Home?pixel&useReferer)](https://github.com/igrigorik/ga-beacon)

---

<p align="center">
  <a href="mailto:gavazquez@gmail.com"><img src="https://img.shields.io/badge/email-gavazquez@gmail.com-blue.svg?style=flat" alt="Email: gavazquez@gmail.com" /></a>
  <a href="./LICENSE"><img src="https://img.shields.io/github/license/lunamultiplayer/LunaMultiPlayer.svg" alt="License" /></a>
</p>
