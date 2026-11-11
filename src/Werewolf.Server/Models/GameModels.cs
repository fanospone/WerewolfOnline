namespace Werewolf.Server.Models;
public enum RoleType { Werewolf, Seer, Doctor, Villager }
public enum GamePhase { Lobby, Night, Discussion, Voting, Finished }
public sealed class Player { public required string Id {get;init;} public required string ConnectionId {get;set;} public required string Name {get;init;} public RoleType? Role {get;set;} public bool IsAlive {get;set;}=true; }
public sealed class GameRoom { public required string Code {get;init;} public required string HostPlayerId {get;set;} public List<Player> Players {get;}=[]; public GamePhase Phase {get;set;}=GamePhase.Lobby; public int Round {get;set;} public DateTimeOffset PhaseEndsAt {get;set;} public Dictionary<string,string> NightActions {get;}=[]; public Dictionary<string,string> Votes {get;}=[]; public string? Winner {get;set;} public object SyncRoot {get;}=new(); }
public sealed record PublicPlayer(string Id,string Name,bool IsAlive,bool IsYou);
public sealed record RoomSnapshot(string Code,string Phase,int Round,int PlayerCount,int MaxPlayers,DateTimeOffset? PhaseEndsAt,string? Winner,IReadOnlyList<PublicPlayer> Players);
public sealed record ChatMessage(string PlayerName,string Message,DateTimeOffset SentAt);
