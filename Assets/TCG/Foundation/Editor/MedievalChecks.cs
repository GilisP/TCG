using System;
using System.Linq;
using System.Collections.Generic;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;
public static class MedievalChecks
{
    static int checks, id=1000;
    static ContentCatalog catalog;
    static EffectRegistry registry=new EffectRegistry();
    static void Assert(bool condition,string title){checks++;if(!condition)throw new Exception(title);}
    static void Do(Match m,ActionKind kind,int target=-1,int unit=-1,string card=null,int[] units=null,int[] blockers=null)
    {if(!m.Try(new Command{player=m.Controller,revision=m.Revision,kind=kind,target=target,unit=unit,card=card,pile=0,units=units??Array.Empty<int>(),blockers=blockers??Array.Empty<int>()},out var error))throw new Exception(kind+": "+error);}
    static void Settle(Match m)
    {
        int guard=0;while((m.Stack.Count>0||m.Choice!=null)&&!m.Over){
            if(++guard>400)throw new Exception("Loop de escolhas/pilha");
            if(m.Choice!=null)Do(m,ActionKind.Choose,m.Choice.Options.First(o=>o.Key>=0).Key);
            else if(m.Defense!=null){var b=m.Board[m.Defense.Target].Pieces.First(p=>p.Owner==m.Defense.Defender&&p.Card.Kind!=CardType.Equipment);Do(m,ActionKind.Defend,blockers:Enumerable.Repeat(b.Id,m.Defense.Attackers.Length).ToArray());}
            else Do(m,ActionKind.Pass);
        }
    }
    static Match Game()
    {
        var main=new[]{Enumerable.Repeat("MED-085",100).ToArray(),Enumerable.Repeat("MED-085",100).ToArray()};
        var lands=new[]{Enumerable.Repeat("test-land-0",50).ToArray(),Enumerable.Repeat("test-land-0",50).ToArray()};
        var m=new Match(catalog,registry,2,false,5,main,lands);
        Do(m,ActionKind.DrawMain);Do(m,ActionKind.Place,Match.Capital(m.Active,2));Do(m,ActionKind.NextPhase);
        for(int c=0;c<121;c++){m.Board[c].Terrain=catalog.Get("test-land-0");m.Board[c].Owner=m.Active;}
        for(int c=0;c<7;c++)m.Seats[m.Active].mana[c]=100;
        return m;
    }
    static Piece Add(Match m,string card,int pos,int owner=-1)
    {var p=new Piece(id++,owner<0?m.Active:owner,catalog.Get(card)){Game=m};m.Board[pos].pieces.Add(p);return p;}
    static void Cast(Match m,string id,int cell=60)
    {m.Seats[m.Controller].hand.Add(catalog.Get(id));Do(m,ActionKind.Play,cell,card:id);Settle(m);}
    [UnityEditor.MenuItem("TCG/Medieval/Validar cartas")]
    public static void Run(){ if(Main()!=0)throw new Exception("Medieval checks failed"); }
    public static int Main()
    {
        try{ checks=0;
            catalog=ContentLoader.Load(registry);
            Assert(catalog.Cards.Count(c=>c.Expansion=="EXP-001-base")==72,"72 cartas medievais");
            foreach(var c in catalog.Cards.Where(c=>c.Expansion=="EXP-001-base")){
                var m=Game();Add(m,"MED-085",59);Add(m,"MED-085",61);Add(m,"MED-085",62,1-m.Active);Add(m,"MED-117",71);Add(m,"MED-085",71);
                m.Seats[m.Active].grave.Add(catalog.Get("MED-097"));m.Seats[m.Active].grave.Add(catalog.Get("MED-098"));m.Seats[m.Active].grave.Add(catalog.Get("MED-085"));
                if(!c.Playable){Assert(!m.CanPlay(c,60),"carta pendente indisponível");continue;}
                Cast(m,c.Id);
                Assert(m.Choice==null&&m.Stack.Count==0,"resolve "+c.Id);
                if(c.Permanent)Assert(m.Board[60].Pieces.Any(p=>p.Card.Id==c.Id),"permanente "+c.Id);
            }
            {var m=Game();var a=Add(m,"MED-085",60);Cast(m,"MED-123");Assert(a.Attack==4,"bônus ataque aplicado");
             Do(m,ActionKind.NextPhase);Do(m,ActionKind.Pass);Do(m,ActionKind.Pass);Assert(a.Attack==2,"bônus expira");}
            {var m=Game();var a=Add(m,"MED-085",60);var b=Add(m,"MED-085",61);var e=Add(m,"MED-090",60);
             Do(m,ActionKind.Equip,a.Id,e.Id);Assert(a.Attack==4,"lança em formação");Do(m,ActionKind.Equip,b.Id,e.Id);Assert(a.Attack==2&&b.Attack==4,"transferência sem duplicar bônus");}
            {var m=Game();var a=Add(m,"MED-085",60);var e=Add(m,"MED-138",60);Do(m,ActionKind.Equip,a.Id,e.Id);Assert(a.Actions==2,"bota concede PA");
             m.Deal(a,99);Assert(m.Find(e.Id)!=null&&e.AttachedTo==-1&&m.Position(e.Id)==60,"equipamento fica após morte");}
            {var m=Game();var b=Add(m,"MED-112",60);Assert(b.Health==4&&!m.CanMove(b.Id,61)&&!m.CanAttack(b.Id,61),"construção imóvel");
             var guard=Add(m,"MED-085",60);var a=Add(m,"MED-085",59);Assert(!m.CanMove(a.Id,60),"barricada bloqueia");}
            {var m=Game();var a=Add(m,"MED-086",60);Add(m,"MED-085",61);Assert(a.Defense==4,"adjacência ortogonal");}
            {var m=Game();var a=Add(m,"MED-134",60);Add(m,"MED-085",62,1-m.Active);Assert(m.CanAttack(a.Id,62),"alcance 2");m.Board[61].Terrain=null;Assert(!m.CanAttack(a.Id,62),"alcance não atravessa vazio");}
            {var m=Game();var a=Add(m,"MED-085",60);Cast(m,"MED-150");m.Deal(a,0);m.Deal(a,2);Assert(a.Damage==0,"prevenção parcial; dano zero não consome");m.Deal(a,2);Assert(a.Damage==2,"prevenção vale só para próximo dano");}
            {var m=Game();var a=Add(m,"MED-125",60);m.Deal(a,1);Settle(m);Assert(a.Attack==6,"berserker sobrevivente");}
            {var m=Game();var source=Add(m,"MED-130",60);var enemy=Add(m,"MED-097",61,1-m.Active);
             Do(m,ActionKind.Attack,61,units:new[]{source.Id});Do(m,ActionKind.Defend,blockers:new[]{enemy.Id});Settle(m);Assert(source.Attack==8,"dragão cresce após matar");}
            {var m=Game();var a=Add(m,"MED-085",60);var e=Add(m,"MED-126",60);int rev=m.Revision;int mana=m.Seats[m.Active].Mana.Sum();
             Assert(!m.Try(new Command{player=m.Active,revision=rev-1,kind=ActionKind.Equip,unit=e.Id,target=a.Id},out _),"revisão inválida");
             Assert(e.AttachedTo==-1&&m.Seats[m.Active].Mana.Sum()==mana,"rejeição não altera estado");}
            {var m=Game();var a=Add(m,"MED-085",60);int castAt=-1,impactAt=-1;
             m.Visual+=e=>{if(e.Kind=="cast")castAt=e.From;if(e.Kind=="impact")impactAt=e.To;};
             m.Seats[m.Active].hand.Add(catalog.Get("MED-123"));Do(m,ActionKind.Play,card:"MED-123");
             Assert(castAt==Match.Capital(m.Active,2),"anúncio pertence à capital do conjurador");
             Assert(impactAt==-1&&a.Attack==2,"pilha não aplica impacto nem bônus antecipado");
             Settle(m);Assert(impactAt==60&&impactAt!=castAt&&a.Attack==4,"resolução impacta alvo fora da capital");}
            {var m=Game();var a=Add(m,"MED-098",60);var b=Add(m,"MED-116",61,1-m.Active);b.Damage=1;b.LastDamagedTurn=m.Turn-1;
             Do(m,ActionKind.Attack,61,units:new[]{a.Id});Do(m,ActionKind.Defend,blockers:new[]{b.Id});Settle(m);
             Assert(b.Damage==3,"Carrasco não ganha bônus por dano de turno anterior");}
            {var m=Game();var a=Add(m,"MED-085",60);int impact=-1;m.Visual+=e=>{if(e.Kind=="impact")impact=e.To;};
             Cast(m,"MED-135");Assert(impact==m.Position(a.Id)&&impact!=60&&impact!=Match.Capital(m.Active,2),"magia de movimento emite VFX no destino escolhido");}
            {var m=Game();var a=Add(m,"MED-085",60);var e=Add(m,"MED-102",60);Do(m,ActionKind.Equip,a.Id,e.Id);
             int before=m.Seats[m.Active].HandCount;m.Deal(a,99);
             Assert(m.Seats[m.Active].HandCount==before&&m.Stack.Count>0,"Adaga aguarda pilha após morte");
             Settle(m);Assert(m.Seats[m.Active].HandCount==before+1&&m.Seats[m.Active].Life==49,"Adaga compra e perde vida na resolução");}
            Debug.Log("MEDIEVAL CORE CHECKS PASSED: "+checks);
            return 0;
        }catch(Exception e){Debug.Log(e);return 1;}
    }
}

