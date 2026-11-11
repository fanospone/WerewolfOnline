using Werewolf.Server.Models;
namespace Werewolf.Server.Engine;
public static class GameEngine
{
 public static IReadOnlyList<RoleType> CreateRoleSet(int n) { if(n<6||n>10) throw new InvalidOperationException("Jumlah pemain harus 6-10."); var wolves=n>=9?3:2; var roles=new List<RoleType>(); roles.AddRange(Enumerable.Repeat(RoleType.Werewolf,wolves)); roles.Add(RoleType.Seer); roles.Add(RoleType.Doctor); roles.AddRange(Enumerable.Repeat(RoleType.Villager,n-wolves-2)); return roles; }
 public static void AssignRoles(GameRoom room) { var roles=CreateRoleSet(room.Players.Count).OrderBy(_=>Random.Shared.Next()).ToArray(); for(int i=0;i<room.Players.Count;i++) room.Players[i].Role=roles[i]; }
 public static string? GetWinner(GameRoom room) { var alive=room.Players.Where(p=>p.IsAlive).ToArray(); int wolves=alive.Count(p=>p.Role==RoleType.Werewolf), others=alive.Length-wolves; if(wolves==0)return "Village"; if(wolves>=others)return "Werewolf"; return null; }
 public static (string? KilledId,string? SeerId,string? InspectedId,string? Inspection) ResolveNight(GameRoom room) {
  var alive=room.Players.Where(p=>p.IsAlive).ToArray(); var wolfIds=alive.Where(p=>p.Role==RoleType.Werewolf).Select(p=>p.Id).ToHashSet();
  var attack=room.NightActions.Where(a=>wolfIds.Contains(a.Key)).Select(a=>a.Value).GroupBy(x=>x).OrderByDescending(g=>g.Count()).FirstOrDefault()?.Key;
  var doctor=alive.FirstOrDefault(p=>p.Role==RoleType.Doctor); string? save=doctor!=null&&room.NightActions.TryGetValue(doctor.Id,out var s)?s:null;
  var seer=room.Players.FirstOrDefault(p=>p.Role==RoleType.Seer); string? inspect=seer!=null&&room.NightActions.TryGetValue(seer.Id,out var i)?i:null;
  string? result=null; if(seer!=null&&inspect!=null) result=room.Players.FirstOrDefault(p=>p.Id==inspect)?.Role==RoleType.Werewolf?"Werewolf":"NotWerewolf";
  string? killed=null; if(attack!=null&&attack!=save) { var victim=room.Players.FirstOrDefault(p=>p.Id==attack&&p.IsAlive); if(victim!=null){victim.IsAlive=false;killed=victim.Id;} }
  room.NightActions.Clear(); return(killed,seer?.Id,inspect,result);
 }
 public static string? ResolveVoting(GameRoom room) { var counts=room.Votes.Where(v=>room.Players.Any(p=>p.Id==v.Key&&p.IsAlive)&&room.Players.Any(p=>p.Id==v.Value&&p.IsAlive)).Select(v=>v.Value).GroupBy(x=>x).Select(g=>new{Id=g.Key,Count=g.Count()}).OrderByDescending(x=>x.Count).ToArray(); room.Votes.Clear(); if(counts.Length==0||(counts.Length>1&&counts[0].Count==counts[1].Count))return null; var p=room.Players.FirstOrDefault(x=>x.Id==counts[0].Id&&x.IsAlive); if(p==null)return null;p.IsAlive=false;return p.Id; }
}
