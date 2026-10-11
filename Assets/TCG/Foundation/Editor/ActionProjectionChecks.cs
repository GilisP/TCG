using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TCG.Foundation.Editor
{
    public static class ActionProjectionChecks
    {
        static int checks;
        static ContentCatalog catalog;
        static EffectRegistry effects;
        public static void Run()
        {
            checks=0;effects=new EffectRegistry();catalog=new ContentCatalog();
            catalog.Add(new ExpansionData {id="action-fixture",title="Action fixture",cards=new[] {
                new CardData{id="action-unit",name="Action unit",kind="Creature",attack=2,defense=5},
                new CardData{id="action-land",name="Action land",kind="Terrain"}
            }},Array.Empty<string>());
            foreach(int count in new[]{2,4})
            {
                var main=Enumerable.Range(0,count).Select(_=>Enumerable.Repeat("action-unit",20).ToArray()).ToArray();
                var terrains=Enumerable.Range(0,count).Select(_=>Enumerable.Repeat("action-land",20).ToArray()).ToArray();
                var m=new Match(catalog,effects,count,false,23,main,terrains){AutoAdvanceAfterTerrain=true};
                Check(m,"draw");
                Do(m,ActionKind.DrawMain);Check(m,"terrain");
                Do(m,ActionKind.Place,Match.Capital(m.Active,count));
                foreach(var cell in m.Board){cell.Terrain=null;cell.Owner=-1;cell.CapitalOwner=-1;cell.pieces.Clear();}
                foreach(int at in new[]{48,49,50,59,60,61,62,71,72,73}){m.Board[at].Terrain=catalog.Get("action-land");m.Board[at].Owner=m.Active;}
                int owner=m.Active,enemy=(owner+1)%count;
                var walker=Add(m,owner,60,Card("equipped-walker"));
                var diagonal=Add(m,owner,71,Card("diagonal",traits:new[]{"pca-diagonal-only"}));
                Add(m,enemy,62,Card("enemy"));m.Board[62].Owner=enemy;m.Board[73].CapitalOwner=enemy;
                var equipment=Add(m,owner,60,Card("equipment-aura","Equipment",abilities:new[] {
                    new CardAbility {trigger="aura",target="host",ops=new[] {
                        new AbilityOp {kind="buff",c=1,value="move:2"},
                        new AbilityOp {kind="buff",value="pa:1"}
                    }}
                }));equipment.AttachedTo=walker.Id;
                // Precon noAttack and keyword auras exercise derived eligibility, not only base stats.
                var restraint=Add(m,owner,71,Card("restraint","Enchantment",abilities:new[] {
                    new CardAbility {trigger="aura",target="host",ops=new[]{new AbilityOp{kind="noAttack"}}}
                }));restraint.AttachedTo=diagonal.Id;
                var activeHand=new[] {
                    Card("hand-creature"),Card("hand-equipment","Equipment"),Card("hand-enchantment","Enchantment"),
                    Card("hand-global","Enchantment",traits:new[]{"pcu-global"}),
                    Card("hand-instant","Instant",rule:"abilities",abilities:new[] {
                        new CardAbility {target="enemy",ops=new[]{new AbilityOp{kind="shield",a=1}}}
                    }),
                    Card("hand-response","Instant",rule:"test-counter"),
                    Card("hand-unpaid",cost:100)
                };
                m.Seats[owner].hand.Clear();m.Seats[owner].hand.AddRange(activeHand);
                m.Seats[enemy].hand.Clear();m.Seats[enemy].hand.Add(Card("PRIVATE-ACTION-OTHER"));
                Check(m,"main auras/equipment/instants");
                int oldMovement=walker.Movement,revision=m.Revision;
                m.Board[60].pieces.Remove(equipment);
                Check(m,"same revision after aura removal");
                Ok(m.Revision==revision&&walker.Movement<oldMovement,"no revision cache retains removed movement aura");
                m.Board[60].pieces.Add(equipment);
                m.Seats[owner].mana[6]=100;
                Check(m,"same revision after mana change");
                var pending=(List<Pending>)typeof(Match).GetField("stack",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(m);
                pending.Add(new Pending {Owner=enemy,Card=Card("stack-spell","Spell"),Target=60});
                Check(m,"stack responses");pending.Clear();
                SetChoice(m,new RuleChoice(owner,"Fixture choice",new[]{new ChoiceOption(1,"One")},_=>{}));
                Check(m,"choice");SetChoice(m,null);
                Do(m,ActionKind.NextPhase);Check(m,"end");
            }
            Debug.Log("ACTION PROJECTION CHECKS PASSED: "+checks);
        }
        static void Check(Match m,string label)
        {
            int viewer=m.Controller,revision=m.Revision;var positions=m.Board.SelectMany(c=>c.Pieces).Select(p=>(p.Id,at:m.Position(p.Id))).ToArray();
            var hand=m.HandFor(viewer).Select(c=>c.Id).ToArray();var view=m.ViewFor(viewer);var legal=new HashSet<string>(view.legal);
            var pieces=m.Board.SelectMany(c=>c.Pieces).Where(p=>p.Owner==viewer).ToArray();
            foreach(var p in pieces)for(int at=0;at<121;at++)
            {
                Ok(legal.Contains("move:"+p.Id+":"+at+":")==m.CanMove(p.Id,at)&&m.CanMove(p.Id,at)==PreviousMove(m,p,at),label+" move "+p.Id+"/"+at);
                Ok(legal.Contains("attack:"+p.Id+":"+at+":")==m.CanAttack(p.Id,at)&&m.CanAttack(p.Id,at)==PreviousAttack(m,p,at),label+" attack "+p.Id+"/"+at);
            }
            foreach(var card in m.HandFor(viewer).Distinct())
            {
                Ok(legal.Contains("play:-1:-1:"+card.Id)==m.CanPlay(card,-1)&&m.CanPlay(card,-1)==PreviousPlay(m,card,-1,-1),label+" untargeted "+card.Id);
                for(int at=0;at<121;at++)
                {
                    Ok(legal.Contains("play:"+at+":-1:"+card.Id)==m.CanPlay(card,at)&&m.CanPlay(card,at)==PreviousPlay(m,card,at,-1),label+" play "+card.Id+"/"+at);
                    foreach(var p in m.Board[at].Pieces)
                        Ok(legal.Contains("play:"+at+":"+p.Id+":"+card.Id)==m.CanPlay(card,at,p.Id)&&m.CanPlay(card,at,p.Id)==PreviousPlay(m,card,at,p.Id),label+" piece target "+card.Id+"/"+p.Id);
                }
            }
            var other=m.ViewFor((viewer+1)%m.Seats.Count);
            Ok(other.legal.Length==0&&!other.hand.Contains("hand-creature")&&view.hand.SequenceEqual(hand),label+" legal actions and private hand stay with viewer");
            Ok(m.Revision==revision&&positions.All(x=>m.Position(x.Id)==x.at)&&m.HandFor(viewer).Select(c=>c.Id).SequenceEqual(hand),label+" projection has no mutation");
        }
        // Captured predicates from before the refactor independently verify state-gate extraction.
        // Unchanged lower-level rule queries remain the oracle for targets and derived effects.
        static object Call(Match m,string name,params object[] args)=>typeof(Match).GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)
            .Single(method=>method.Name==name&&method.GetParameters().Length==args.Length).Invoke(m,args);
        static bool Query(Match m,string name,params object[] args)=>(bool)Call(m,name,args);
        static void SetChoice(Match m,RuleChoice choice)=>typeof(Match).GetProperty("Choice").SetValue(m,choice);
        static bool PreviousMove(Match m,Piece p,int target)=>!m.Over&&m.Phase==Stage.Main&&m.Priority==m.Active&&m.Stack.Count==0&&p!=null&&p.Owner==m.Active
            &&Query(m,"Mobile",p)&&!p.Modifiers.Any(mod=>mod.NoMove)&&p.Movement>0&&Query(m,"MovePath",p,target)&&m.Board[target].Terrain!=null
            &&!Query(m,"Blocked",p,target)&&!m.Board[target].Pieces.Any(u=>u.Card.Kind!=CardType.Equipment&&u.Card.Kind!=CardType.Enchantment&&m.Enemies(m.Active,u.Owner))
            &&!m.Enemies(m.Active,m.Board[target].CapitalOwner);
        static bool PreviousAttack(Match m,Piece p,int target)=>!m.Over&&m.Phase==Stage.Main&&m.Priority==m.Active&&m.Stack.Count==0&&p!=null&&p.Owner==m.Active
            &&(p.Card.Kind==CardType.Creature||p.Card.IsVehicle)&&Query(m,"CanAct",p)&&!p.Modifiers.Any(mod=>mod.NoAttack)&&!Query(m,"PreconNoAttack",p)
            &&p.Actions>0&&p.Movement>0&&Query(m,"InRange",p,target)&&m.Board[target].Terrain!=null
            &&(m.Board[target].Pieces.Any(u=>u.Card.Kind!=CardType.Equipment&&u.Card.Kind!=CardType.Enchantment&&m.Enemies(m.Active,u.Owner))||m.Enemies(m.Active,m.Board[target].CapitalOwner));
        static bool PreviousPlay(Match m,Definition card,int target,int unit)
        {
            if(card==null||card.Rule=="destiny"||!card.Playable||m.Over||m.Choice!=null||m.Defense!=null||card.Kind==CardType.Terrain||!Query(m,"CanPay",m.Priority,card))return false;
            if(card.Kind!=CardType.Instant&&(m.Phase!=Stage.Main||m.Priority!=m.Active||m.Stack.Count!=0))return false;
            if(card.Kind==CardType.Instant&&m.Phase!=Stage.Main&&m.Phase!=Stage.End&&m.Stack.Count==0)return false;
            if(card.Rule=="test-counter"&&!Query(m,"ResponseTargets",card,m.Priority))return false;
            if(card.Rule=="abilities"&&!card.Permanent&&!Query(m,"AbilityResponse",card,m.Priority))return false;
            if(card.Kind==CardType.Enchantment)
            {
                var placement=Call(m,"PreconEnchantmentPlacement",card,m.Priority,target,unit);if(placement!=null)return (bool)placement;
                var host=m.Find(unit);return host!=null&&(host.Card.Kind==CardType.Creature||host.Card.IsVehicle)&&Query(m,"AbilityEnchantmentTarget",card,host,m.Priority)
                    &&m.Position(unit)==target&&!Query(m,"Immune",host,m.Priority);
            }
            if(card.Permanent)return target>=0&&target<121&&m.Board[target].Terrain!=null&&m.Board[target].Owner==m.Priority;
            if(card.Effects.Any(e=>effects.Get(e.Operation).NeedsEnemy)){var piece=m.Find(unit);return piece!=null&&m.Enemies(m.Priority,piece.Owner)&&m.Position(unit)==target;}
            return true;
        }
        static Definition Card(string id,string kind="Creature",string[] traits=null,string rule="",CardAbility[] abilities=null,int cost=0)=>new Definition(new CardData {
            id=id,name=id,kind=kind,attack=2,defense=5,traits=traits??Array.Empty<string>(),rule=rule,cost=cost,abilities=abilities??Array.Empty<CardAbility>()
        },"action-fixture");
        static Piece Add(Match m,int owner,int at,Definition card){var p=new Piece(12000+m.Board.Sum(c=>c.Pieces.Count),owner,card){Game=m};m.Board[at].pieces.Add(p);return p;}
        static void Do(Match m,ActionKind kind,int target=-1){Ok(m.Try(new Command{player=m.Controller,revision=m.Revision,kind=kind,target=target,pile=0},out var error),kind+": "+error);}
        static void Ok(bool value,string message){if(!value)throw new Exception("ACTION PROJECTION CHECK: "+message);checks++;}
    }
}
