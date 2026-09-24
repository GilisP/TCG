using System;
using System.IO;
using System.Linq;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        bool progressPage; MissionCatalog missionCatalog; Vector2 progressScroll; float nextProgressSave;
        bool DiagnosticSession=>Environment.GetCommandLineArgs().Any(a=>a.StartsWith("-tcg-"));
        Match cameraMatch; int cameraSeat=-1;
        void UpdateSeatCamera(){if(match==null||world==null)return;int seat=Viewer;if(cameraMatch==match&&cameraSeat==seat)return;cameraMatch=match;cameraSeat=seat;world.Yaw=TableWorld.CapitalYaw(seat,match.Seats.Count);}
        void Update(){UpdateSeatCamera();if(Time.unscaledTime<nextProgressSave)return;nextProgressSave=Time.unscaledTime+3;SaveFinishedProgress();}
        void SaveFinishedProgress()
        {
            if(DiagnosticSession||!sessionAvailable||match==null||!match.Over||library==null||!collectionStore.CanWrite)return;
            var result=match.NumbersFor(match.IsRemoteView?Viewer:0);if(result==null||library.Data.progress.recorded.Contains(result.id))return;
            ProgressTransaction(()=>library.RecordMatch(result));
        }
        bool ProgressTransaction(Action action)
        {
            if(!collectionStore.CanWrite){hubNotice=collectionStore.Warning;return false;}
            var before=JsonUtility.ToJson(library.Data);
            try{action();collectionStore.Save(library);hubNotice="Progresso salvo.";return true;}
            catch(Exception e){library=new CollectionLibrary(catalog,JsonUtility.FromJson<CollectionData>(before));hubNotice="Não foi possível salvar: "+e.Message;return false;}
        }
        void DrawProgressPage()
        {
            SaveFinishedProgress();
            if(missionCatalog==null)missionCatalog=JsonUtility.FromJson<MissionCatalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"Economy","missions.json")));
            var p=library.Data.progress;
            Text(45,140,1200,45,"CRÔNICAS DO JOGADOR",heading,Gold);
            if(Button(1280,136,270,48,"Voltar ao menu"))progressPage=false;
            Text(45,200,1470,65,"Perfil local de teste · mesa local: assento Âmbar · online: seu assento. Apenas partidas concluídas contam. Missões únicas; recompensas provisórias.",body,Muted);
            Text(45,280,1480,45,$"Partidas: {p.completed}   Vitórias: {p.wins}   Cartas jogadas: {p.cards}   Terrenos: {p.terrains}",heading,Ink);
            Text(45,336,1470,42,$"Recordes por partida: {p.recordCards} cartas · {p.recordTerrains} terrenos · {p.recordTurns} turnos globais",body,Gold);
            progressScroll=GUI.BeginScrollView(new Rect(45,395,1510,490),progressScroll,new Rect(0,0,1470,missionCatalog.missions.Length*90+100+p.history.Count*45));
            int y=0;foreach(var m in missionCatalog.missions)
            {
                int value=library.MissionValue(m);bool claimed=p.claimed.Contains(m.id);Fill(new Rect(0,y,1450,80),Panel);
                Text(20,y+8,970,30,m.title+" · "+Math.Min(value,m.target)+" / "+m.target,body,Ink);
                Text(20,y+43,970,25,$"Recompensa: {m.coins} moedas + {m.boosters} booster(s)",small,Gold);
                if(Button(1100,y+15,325,50,claimed?"Resgatada":"Resgatar",!claimed&&value>=m.target))ProgressTransaction(()=>library.ClaimMission(m));y+=90;
            }
            Text(20,y+10,1400,40,"ÚLTIMAS 50 PARTIDAS · mais recente primeiro",heading,Gold);y+=65;
            foreach(var h in p.history){Text(20,y,1400,40,$"{(h.won?"Vitória":"Derrota / empate")} · {h.cards} cartas · {h.terrains} terrenos · {h.turns} turnos · {h.id.Substring(0,Math.Min(8,h.id.Length))}",body,Ink);y+=45;}
            GUI.EndScrollView();Text(45,915,1480,45,hubNotice,body,Gold);
        }
    }
}
