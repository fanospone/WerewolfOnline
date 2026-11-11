using Microsoft.Data.Sqlite;
namespace Werewolf.Server.Services;
public sealed class GameHistoryStore(IConfiguration config) {
 private readonly string _cs=config.GetConnectionString("Werewolf")??"Data Source=werewolf.db";
 public async Task InitializeAsync(){await using var c=new SqliteConnection(_cs);await c.OpenAsync();var q=c.CreateCommand();q.CommandText="CREATE TABLE IF NOT EXISTS GameHistory(Id INTEGER PRIMARY KEY AUTOINCREMENT,RoomCode TEXT NOT NULL,Winner TEXT NOT NULL,PlayerCount INTEGER NOT NULL,CompletedAt TEXT NOT NULL);";await q.ExecuteNonQueryAsync();}
 public async Task SaveResultAsync(string room,string winner,int count){await using var c=new SqliteConnection(_cs);await c.OpenAsync();var q=c.CreateCommand();q.CommandText="INSERT INTO GameHistory(RoomCode,Winner,PlayerCount,CompletedAt) VALUES($r,$w,$c,$t)";q.Parameters.AddWithValue("$r",room);q.Parameters.AddWithValue("$w",winner);q.Parameters.AddWithValue("$c",count);q.Parameters.AddWithValue("$t",DateTimeOffset.UtcNow.ToString("O"));await q.ExecuteNonQueryAsync();}
}
