using System;
using System.Linq;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;

public static class QualityOfLifeChecks
{
    static int checks;
    static void Assert(bool value,string label){checks++;if(!value)throw new Exception("QOL: "+label);}
    static void Do(Match m,ActionKind action,int target=-1,string card=null)
    {Assert(m.Try(new Command{player=m.Controller,revision=m.Revision,kind=action,target=target,card=card,pile=0},out var error),action+" "+error);}
    public static void Run()
    {
        checks=0;var registry=new EffectRegistry();var catalog=ContentLoader.Load(registry);
        foreach(int count in new[]{2,3,4})
        {
            var mains=Enumerable.Range(0,count).Select(_=>Enumerable.Repeat("MED-085",100).ToArray()).ToArray();
            var lands=Enumerable.Range(0,count).Select(_=>Enumerable.Repeat("test-land-0",50).ToArray()).ToArray();
            var m=new Match(catalog,registry,count,false,5,mains,lands){AutomaticResponses=true};
            int active=m.Active;Do(m,ActionKind.DrawMain);Do(m,ActionKind.Place,Match.Capital(active,count));Do(m,ActionKind.NextPhase);
            Assert(m.Phase==Stage.Main&&m.Active==active,"não pula fase principal");
            m.Seats[active].mana[0]=100;
            Do(m,ActionKind.Play,Match.Capital(active,count),"MED-085");
            Assert(m.Stack.Count==0&&m.Controller==active&&m.Board[Match.Capital(active,count)].Pieces.Count==1,"resolve sem respostas em "+count+" lugares");
            int other=(active+1)%count;
            m.Seats[other].hand.Add(catalog.Get("MED-123"));m.Seats[other].mana[3]=10;
            Do(m,ActionKind.Play,Match.Capital(active,count),"MED-085");
            Assert(m.Stack.Count==0,"ignora truque sem alvo aliado");
            var ally=new Piece(900+count,other,catalog.Get("MED-085")){Game=m};m.Board[Match.Capital(other,count)].pieces.Add(ally);
            Do(m,ActionKind.Play,Match.Capital(active,count),"MED-085");
            Assert(m.Stack.Count==1&&m.Controller==other&&m.HasResponse(),"mantém resposta válida");
            Do(m,ActionKind.Play,card:"MED-123");
            Assert(m.Choice!=null&&m.Controller==other,"preserva escolha do alvo");
            Do(m,ActionKind.Choose,ally.Id);
            Assert(m.Stack.Count==0&&m.Controller==active,"retoma após resolver toda a pilha");
            Do(m,ActionKind.NextPhase);
            Assert(m.Phase==Stage.Draw&&m.Turn==2&&m.Active!=active,"fim de turno sem passes vazios");
        }
        Debug.Log("QOL CHECKS PASSED: "+checks);
    }
}
