using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    public sealed partial class Match
    {
        readonly Dictionary<int,int> commanderDamage=new Dictionary<int,int>();
        public int CommanderDamage(int attacker,int defender)=>commanderDamage.TryGetValue(attacker*4+defender,out var value)?value:0;
        public bool CanSummonCommander(int target)
        {if(IsRemoteView)return NetCan("summon",target);
            var s=seats[Priority];return !Over&&Choice==null&&Defense==null&&Phase==Stage.Main&&Priority==Active&&stack.Count==0&&s.Commander!=null&&s.CommanderReady&&s.Commander.Playable&&CanPay(Priority,s.Commander)&&s.mana.Sum()-s.Commander.TotalCost>=s.CommanderCasts*2&&Valid(target)&&cells[target].Owner==Priority&&cells[target].Terrain!=null;
        }
        void SpendGeneric(int owner,int amount){for(int i=6;i>=0&&amount>0;i--){int paid=Math.Min(amount,seats[owner].mana[i]);seats[owner].mana[i]-=paid;amount-=paid;}}
        void SummonCommander(int target)
        {
            Check(CanSummonCommander(target),"Comandante indisponível: confira mana, zona e terreno próprio.");var seat=seats[Priority];Pay(Priority,seat.Commander);SpendGeneric(Priority,seat.CommanderCasts*2);seat.CommanderCasts++;seat.CommanderReady=false;
            stack.Add(new Pending{Owner=Priority,Card=seat.Commander,Target=target,CommandUnit=true});passes=0;Note(seat.Name+" conjurou "+seat.Commander.Name+".");
        }
        void CommanderDied(Piece p)
        {
            if(!p.CommandUnit||seats[p.Owner].Eliminated)return;
            Ask(p.Owner,"Devolver "+p.Card.Name+" à zona de comandante?",new[]{new ChoiceOption(1,"Voltar à zona de comandante"),new ChoiceOption(0,"Manter no cemitério")},key=>{if(key==1&&seats[p.Owner].grave.Remove(p.Card))seats[p.Owner].CommanderReady=true;});
        }
        void MarkCommanderDamage(Piece p,int victim,int amount)
        {
            if(!p.CommandUnit||amount<=0)return;int key=p.Owner*4+victim;commanderDamage[key]=CommanderDamage(p.Owner,victim)+amount;
            if(commanderDamage[key]>=26){Eliminate(victim);Note(seats[victim].Name+" recebeu 26 de dano do mesmo comandante.");}
        }
    }
}
