# WerewolfOnline MVP

ASP.NET Core 8 + SignalR + C# Game Engine + SQLite.

## Prasyarat
- Visual Studio 2022 dengan workload ASP.NET and web development
- .NET 8 SDK

## Menjalankan
1. Extract ZIP dan buka `WerewolfOnline.sln`.
2. Pilih `Werewolf.Server` sebagai Startup Project.
3. Restore NuGet Packages, lalu tekan F5.
4. Buka aplikasi di beberapa tab browser/incognito untuk simulasi pemain.
5. Buat room dari satu tab; tab lain bergabung dengan kode room. Minimal 6 pemain untuk mulai.

Database `werewolf.db` dibuat otomatis. Timer: malam 25 detik, diskusi 35 detik, voting 20 detik. Room berada di memory server; riwayat hasil tersimpan di SQLite.

## Batasan MVP yang perlu diselesaikan sebelum production
Reconnect dan penggantian ConnectionId, pengelolaan disconnect/host migration, rate limiting, autentikasi, validasi aksi role yang lebih ketat, private team chat, spectator mode, serta deployment HTTPS. Untuk tahap ini gunakan untuk pengujian lokal.
