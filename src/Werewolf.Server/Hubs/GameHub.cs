
using Microsoft.AspNetCore.SignalR;
using Werewolf.Server.Services;

namespace Werewolf.Server.Hubs;

public sealed class GameHub(RoomManager rooms) : Hub
{
    public async Task<string> CreateRoom(string playerName)
    {
        var r = rooms.Create(Context.ConnectionId, playerName);

        await Groups.AddToGroupAsync(
            Context.ConnectionId, r.Code);

        await Clients.Caller.SendAsync("JoinedRoom", new
        {
            code = r.Code,
            playerId = r.Players[0].Id,
            isHost = true
        });

        await rooms.Broadcast(r);

        return r.Code;
    }

    public async Task JoinRoom(string code, string playerName)
    {
        var r = rooms.Join(
            code, Context.ConnectionId, playerName);

        var p = r.Players.Last();

        await Groups.AddToGroupAsync(
            Context.ConnectionId, r.Code);

        await Clients.Caller.SendAsync("JoinedRoom", new
        {
            code = r.Code,
            playerId = p.Id,
            isHost = false
        });

        await rooms.Broadcast(r);
    }


    public async Task StartGame(string code, string playerId)
    {
        var r = rooms.Start(code, playerId);

        foreach (var p in r.Players)
        {
            await Clients.Client(p.ConnectionId)
                .SendAsync("RoleAssigned", p.Role?.ToString());

            if (p.Role == Werewolf.Server.Models.RoleType.Werewolf)
            {
                var pack = string.Join(", ",
                    r.Players
                        .Where(x => x.Role == Werewolf.Server.Models.RoleType.Werewolf
                                 && x.Id != p.Id)
                        .Select(x => x.Name));

                await Clients.Client(p.ConnectionId).SendAsync(
                    "PrivateNotice",
                    string.IsNullOrEmpty(pack)
                        ? "Kamu Werewolf tunggal."
                        : $"Rekan Werewolf: {pack}"
                );
            }
        }

        await Clients.Group(code).SendAsync(
            "GameNotice",
            "Permainan dimulai. Malam pertama tiba."
        );

        await rooms.Broadcast(r);

        // Jalankan moderator otomatis
        _ = rooms.RunClock(code);
    }

    public Task NightAction(
        string code, string playerId, string targetId)
    {
        rooms.NightAction(code, playerId, targetId);
        return Task.CompletedTask;
    }

    public Task Vote(
        string code, string playerId, string targetId)
    {
        rooms.Vote(code, playerId, targetId);
        return Task.CompletedTask;
    }

    public Task SendChat(
        string code, string playerId, string message)
    {
        return rooms.Chat(code, playerId, message);
    }
}
