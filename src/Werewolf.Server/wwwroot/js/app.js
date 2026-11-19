const $=id=>document.getElementById(id);
let hub,roomCode=null,playerId=null,isHost=false,myRole=null,state=null,timer;

const roles={
  Werewolf:["🐺","Werewolf","Pilih satu korban setiap malam."],
  Seer:["🔮","Seer","Periksa satu pemain setiap malam."],
  Doctor:["🛡️","Doctor","Lindungi satu pemain setiap malam."],
  Villager:["🧑","Villager","Bantu desa menemukan Werewolf."]
};
const phases={
  Lobby:["Waiting Room","Menunggu pemain."],
  Night:["Malam","Aksi rahasia Werewolf, Doctor, dan Seer."],
  Discussion:["Diskusi","Diskusikan siapa yang dicurigai."],
  Voting:["Voting","Pilih pemain untuk dieliminasi."],
  Finished:["Selesai","Permainan berakhir."]
};

function safe(s){
  return String(s).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}
function appendChat(containerId,name,message){
  const container=$(containerId);
  const d=document.createElement('div');
  d.className='chat-line';
  d.innerHTML=`<strong>${safe(name)}:</strong> ${safe(message)}`;
  container.append(d);
  container.scrollTop=container.scrollHeight;
}
function chat(n,m){appendChat('chatMessages',n,m);}
function privateChat(n,m){appendChat('werewolfChatMessages',n,m);}

function updateWerewolfChat(){
  if(!state || !myRole) return;
  const me=state.players.find(x=>x.isYou);
  const visible=state.phase==='Night' && myRole==='Werewolf' && !!me?.isAlive;
  $('werewolfChatPanel').classList.toggle('hidden',!visible);
  if(!visible) $('werewolfChatInput').value='';
}

function timerStart(end){
  clearInterval(timer);
  const tick=()=> $('timer').textContent=Math.max(0,Math.ceil((new Date(end)-Date.now())/1000))+"s";
  tick();
  timer=setInterval(tick,1000);
}

function render(s){
  state=s;
  $('playerCount').textContent=`${s.playerCount} / ${s.maxPlayers}`;
  const p=phases[s.phase];
  $('phaseLabel').textContent=p[0];
  $('phaseHint').textContent=s.phase==='Finished'?`Pemenang: ${s.winner}`:p[1];
  $('aliveLabel').textContent=s.phase;
  $('hostControls').classList.toggle('hidden',!isHost||s.phase!=='Lobby');
  $('players').innerHTML=s.players.map(x=>`<div class="player ${x.isAlive?'':'dead'}"><span>${safe(x.name)} ${x.isYou?'(Kamu)':''}</span><span>${x.isAlive?'● Hidup':'† Mati'}</span></div>`).join('');

  const me=s.players.find(x=>x.isYou);
  const visible=['Night','Voting'].includes(s.phase)&&me?.isAlive;
  $('actions').classList.toggle('hidden',!visible);
  $('targetLabel').childNodes[0].textContent=s.phase==='Night'?'Target aksi malam':'Pilih target voting';
  $('targetSelect').innerHTML=s.players.filter(x=>x.isAlive&&!x.isYou).map(x=>`<option value="${x.id}">${safe(x.name)}</option>`).join('');
  $('actionBtn').textContent=s.phase==='Night'?'Kirim Aksi Malam':'Kirim Vote';
  $('actionBtn').disabled=s.phase==='Night'&&myRole==='Villager';
  if(s.phaseEndsAt) timerStart(s.phaseEndsAt);
  updateWerewolfChat();
}

async function start(){
  hub=new signalR.HubConnectionBuilder().withUrl('/gameHub').withAutomaticReconnect().build();
  hub.on('JoinedRoom',x=>{
    roomCode=x.code;playerId=x.playerId;isHost=x.isHost;
    $('roomLabel').textContent=roomCode;
    $('welcome').classList.add('hidden');
    $('game').classList.remove('hidden');
  });
  hub.on('RoomUpdated',render);
  hub.on('RoleAssigned',r=>{
    myRole=r;
    const x=roles[r];
    $('roleIcon').textContent=x[0];
    $('roleName').textContent=x[1];
    $('roleDescription').textContent=x[2];
    updateWerewolfChat();
  });
  hub.on('PrivateNotice',m=>{
    $('privateNotice').textContent=m;
    $('privateNotice').classList.remove('hidden');
  });
  hub.on('GameNotice',m=>chat('MODERATOR',m));
  hub.on('ChatReceived',m=>chat(m.playerName,m.message));
  hub.on('WerewolfChatReceived',m=>privateChat(m.playerName,m.message));
  await hub.start();
  $('connection').textContent='Connected';
}

$('createBtn').onclick=async()=>{
  try{await hub.invoke('CreateRoom',$('playerName').value);}
  catch(e){$('error').textContent=e.message;}
};
$('joinBtn').onclick=async()=>{
  try{await hub.invoke('JoinRoom',$('roomCode').value,$('playerName').value);}
  catch(e){$('error').textContent=e.message;}
};
$('startBtn').onclick=async()=>{
  try{await hub.invoke('StartGame',roomCode,playerId);}
  catch(e){$('gameError').textContent=e.message;}
};
$('actionBtn').onclick=async()=>{
  try{
    const t=$('targetSelect').value;
    if(state.phase==='Night') await hub.invoke('NightAction',roomCode,playerId,t);
    else await hub.invoke('Vote',roomCode,playerId,t);
    $('gameError').textContent='Pilihan terkirim.';
  }catch(e){$('gameError').textContent=e.message;}
};
$('sendChat').onclick=async()=>{
  try{
    await hub.invoke('SendChat',roomCode,playerId,$('chatInput').value);
    $('chatInput').value='';
  }catch(e){$('gameError').textContent=e.message;}
};
$('chatInput').onkeydown=e=>{if(e.key==='Enter')$('sendChat').click();};

async function sendWerewolfChat(){
  try{
    await hub.invoke('SendWerewolfMessage',roomCode,$('werewolfChatInput').value);
    $('werewolfChatInput').value='';
  }catch(e){$('werewolfChatError').textContent=e.message;}
}
$('sendWerewolfChat').onclick=sendWerewolfChat;
$('werewolfChatInput').onkeydown=e=>{if(e.key==='Enter')sendWerewolfChat();};

start().catch(e=>{
  $('connection').textContent='Offline';
  $('error').textContent='Koneksi gagal: '+e.message;
});