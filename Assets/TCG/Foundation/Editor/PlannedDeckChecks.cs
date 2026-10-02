using System;
using System.Linq;
using System.Reflection;
using System.IO;
using TCG.Table;
using UnityEngine;
namespace TCG.Foundation.Editor
{
    public static class PlannedDeckChecks
    {
        static ContentCatalog catalog;static EffectRegistry effects;static int checks,next=800000;
        static void Check(bool ok,string message){checks++;if(!ok)throw new Exception("PLANNED DECKS: "+message);}
        static void Invoke(Match game,string method,params object[] args){try{typeof(Match).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,args);}catch(TargetInvocationException e){throw e.InnerException;}}
        static void Do(Match game,ActionKind kind,int target=-1,int unit=-1,string card=null,int[] attackers=null,int[] blockers=null){if(!game.Try(new Command{player=game.Controller,revision=game.Revision,kind=kind,target=target,unit=unit,card=card,pile=0,units=attackers??Array.Empty<int>(),blockers=blockers??Array.Empty<int>()},out string error))throw new Exception("PLANNED ACTION "+kind+": "+error);}
        static void Settle(Match game){for(int i=0;i<250&&(game.Stack.Count>0||game.Choice!=null);i++){if(game.Choice!=null)Do(game,ActionKind.Choose,game.Choice.Options.First(x=>x.Key!=-1).Key);else if(game.Defense!=null)Do(game,ActionKind.Defend,blockers:Enumerable.Repeat(-1,game.Defense.Attackers.Length).ToArray());else Do(game,ActionKind.Pass);}Check(game.Stack.Count==0&&game.Choice==null,"effect queue terminates");}
        static Match Game(){var game=ContentLoader.TestTable(catalog,effects,2,false,67);Do(game,ActionKind.DrawMain);Do(game,ActionKind.Place,Match.Capital(game.Active,2));Do(game,ActionKind.NextPhase);foreach(var cell in game.Board){cell.Terrain=catalog.Get("test-land-6");cell.Owner=game.Active;cell.TerrainOwner=game.Active;}foreach(var seat in game.Seats){seat.hand.Clear();for(int i=0;i<7;i++)seat.mana[i]=100;}return game;}
        static Piece Add(Match game,string id,int at=60,int owner=-1){var p=new Piece(next++,owner<0?game.Active:owner,catalog.Get(id)){Game=game};game.Board[at].pieces.Add(p);return p;}
        public static void Run()
        {
            checks=0;effects=new EffectRegistry();catalog=ContentLoader.Load(effects);var plans=PlannedDeckLoader.Load(catalog);
            Check(plans.Decks.Count==29,"29 templates");
            var data=new CollectionData{coins=317};var library=new CollectionLibrary(catalog,data);var custom=new DeckData{name="Não sobrescrever"};library.SaveDeck(custom);
            foreach(var plan in plans.Decks){var deck=library.ImportPlannedDeck(plan);Check(deck.main.Count==100&&deck.terrains.Count==50,"100 + 50 "+plan.id);Check(library.Validate(deck,true).Count==0,"legal "+plan.id);}
            Check(data.coins==317&&data.decks.Count==30,"imports preserve economy and unrelated decks");
            var edited=data.decks[1];edited.name="Nome editado";edited.main.RemoveAt(0);var again=library.ImportPlannedDeck(plans.Decks[0]);Check(again.name=="Nome editado"&&again.main.Count==99&&data.decks.Count==30,"reimport does not overwrite edits");
            var dir=Path.Combine(Path.GetTempPath(),"tcg-planned-"+Guid.NewGuid().ToString("N"));var store=new CollectionStore(dir);store.Save(library);var loaded=store.Load(catalog);Check(loaded.Data.coins==317&&loaded.Data.decks.Count==30,"restart preserves imports");

            {var game=Game();var host=Add(game,"MED-085");var eq=Add(game,"PD26-N-047");Do(game,ActionKind.Equip,host.Id,eq.Id);Check(host.Defense==4&&host.Movement==3,"equipment dynamic stats");var remote=Match.FromView(game.ViewFor(game.Active),catalog,effects);Check(remote.Find(host.Id).Defense==4&&remote.Find(host.Id).Movement==3,"equipment stats transmitted");Invoke(game,"ReturnHand",eq);Check(host.Defense==3&&host.Movement==2,"detaching removes movement bonus");}
            {var game=Game();var p=Add(game,"PD26-S-001");Check(p.Defense==2,"conditional aura initially inactive");Add(game,"MED-085",59);Check(p.Defense==4,"orthogonal formation aura");}
            {var game=Game();var p=Add(game,"PD26-T-016");Do(game,ActionKind.Activate,unit:p.Id);Settle(game);Check(p.Defense==4&&!game.CanActivate(p.Id),"once per turn and next-turn defense");}
            {var game=Game();var p=Add(game,"MED-085");game.Board[60].Terrain=catalog.Get("PD26-LS-001");int mana=game.Seats[game.Active].Mana.Sum();Check(game.CanActivateTerrain(60),"terrain activation legal");var remote=Match.FromView(game.ViewFor(game.Active),catalog,effects);Check(remote.CanActivateTerrain(60),"terrain action projected");Do(game,ActionKind.ActivateTerrain,60);Settle(game);Check(p.Defense==5&&game.Seats[game.Active].Mana.Sum()==mana-2&&!game.CanActivateTerrain(60),"terrain pays and once limit");Invoke(game,"EndModifiers");Check(p.Defense==3,"terrain bonus expires");}
            {var game=Game();var p=Add(game,"PD26-F-007");game.Board[61].CapitalOwner=1-game.Active;int life=game.Seats[1-game.Active].Life;Do(game,ActionKind.Attack,61,attackers:new[]{p.Id});Settle(game);Check(game.Seats[1-game.Active].Life==life-2,"attack trigger resolves before combat");Check(p.Attack==1,"combat bonus removed");}
            {var game=Game();var p=Add(game,"PD26-L-010");var fodder=Add(game,"MED-085");Do(game,ActionKind.Activate,unit:p.Id);Check(game.Choice!=null,"sacrifice cost choice before ability");Do(game,ActionKind.Choose,fodder.Id);Check(game.Find(fodder.Id)==null,"sacrifice is paid before resolution");Settle(game);Check(p.Attack==3&&p.Actions==0,"sacrifice buff applies to source");}
            {var grave=new Graveyard();var card=catalog.Get("MED-085");grave.Add(card);grave.AddMilled(card);var entry=grave.Milled.Single();Check(grave.RemoveEntry(entry.Id)&&grave.Count==1&&!grave.Milled.Any(),"equal card copies retain distinct provenance");grave.Add(card);Check(!grave.Milled.Any(),"ordinary re-entry cannot reuse mill status");}
            {var game=Game();var pirate=Add(game,"IDEIA-004");int enemy=1-game.Active;var card=catalog.Get("MED-085");game.Seats[enemy].grave.Add(card);game.Seats[enemy].grave.AddMilled(card);Do(game,ActionKind.Activate,unit:pirate.Id);Settle(game);Check(pirate.Actions==0&&game.Seats[enemy].grave.Count==1&&!game.Seats[enemy].grave.Milled.Any()&&game.Seats[game.Active].hand.Count==1,"pirate recovers exact milled entry");var stolen=game.Seats[game.Active].hand.Single();Do(game,ActionKind.Play,60,card:stolen.Id);Settle(game);var piece=game.Board[60].Pieces.Single(p=>p.Card.Id==stolen.Id);Check(piece.OriginalOwner==enemy,"pirate hand remembers original owner");game.Deal(piece,100);Settle(game);Check(game.Seats[enemy].grave.Count==2&&!game.Seats[enemy].grave.Milled.Any(),"stolen creature returns without stale provenance");}
            {var game=Game();Add(game,"IDEIA-004");var attacker=Add(game,"MED-085");int enemy=1-game.Active;game.Board[61].CapitalOwner=enemy;int count=game.Seats[enemy].MainCount;Do(game,ActionKind.Attack,61,attackers:new[]{attacker.Id});Settle(game);Check(game.Seats[enemy].MainCount==count-2&&game.Seats[enemy].grave.Milled.Count()==2,"pirate capital damage mills actual amount");}
            {var game=Game();var p=Add(game,"MED-085");game.Seats[game.Active].hand.Add(catalog.Get("MED-018"));int cards=game.Seats[game.Active].HandCount;Do(game,ActionKind.Play,card:"MED-018");Check(game.Choice!=null,"additional cast cost waits for sacrifice");Do(game,ActionKind.Choose,p.Id);Check(game.Find(p.Id)==null,"cast sacrifice paid");Settle(game);Check(game.Seats[game.Active].HandCount==cards+1,"draw two after sacrifice");}
            {var game=Game();var boat=Add(game,"PD26-N-066");Add(game,"MED-231");var flyer=Add(game,"PD26-A-013");Do(game,ActionKind.BoardVehicle,boat.Id,flyer.Id);Check(game.EffectiveKeywords(boat.Id).Contains("flying")&&game.VehicleCapacity(boat.Id)==3,"new vehicle and explicit flight with Valeria");}
            {var game=Game();var p=Add(game,"MED-085");game.Seats[game.Active].hand.Add(catalog.Get("MED-062"));Do(game,ActionKind.Play,card:"MED-062");Settle(game);game.Deal(p,1);game.Deal(p,1);game.Deal(p,2);Check(p.Damage==1,"three-point prevention is consumed across hits");}
            {var game=Game();var p=Add(game,"MED-085");var tower=Add(game,"PD26-N-057");Check(p.Range==1,"range tower does not enable itself");var lance=Add(game,"PD26-N-037");Do(game,ActionKind.Equip,p.Id,lance.Id);Check(p.Range==3,"equipment range enables tower without recursion");}
            {var game=Game();var p=Add(game,"PD26-N-022");Check(!(bool)typeof(Match).GetMethod("Immune",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{p,p.Owner}),"movement immunity does not block unrelated effects");Check((bool)typeof(Match).GetMethod("AbilityMoveBlocked",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{p,p.Owner}),"own movement effect blocked");}
            SmokeCards();
            foreach(int count in new[]{2,3,4}){
                var room=new NetworkRoom(catalog,effects,"planned-check",count,false,()=>0);for(int i=0;i<count;i++){Check(room.Receive((ulong)i,new RoomRequest{type="hello",content="planned-check"}).error=="","join planned room");Check(room.Receive((ulong)i,new RoomRequest{type="deck",sequence=1,deck=plans.Decks[i].Create()}).error=="","submit full singleton deck");Check(room.Receive((ulong)i,new RoomRequest{type="ready",sequence=2,ready=true}).error=="","ready");}Check(room.Receive(0,new RoomRequest{type="start",sequence=3}).error=="","start full decks");
                for(int i=0;i<count;i++){var view=room.Reply(i).view;var remote=Match.FromView(JsonUtility.FromJson<MatchView>(JsonUtility.ToJson(view)),catalog,effects);Check(remote.HandFor(i).Count==5&&Enumerable.Range(0,count).Where(s=>s!=i).All(s=>remote.HandFor(s).Count==0),"new decks preserve hand privacy");Check(remote.Seats[i].MainCount==95,"100-card principal initialized");}
            }
            Debug.Log("PLANNED DECK CHECKS PASSED: "+checks+"; 641 imported cards, 29 decks, isolated persistence and 2/3/4-player projections");
        }
        static void SmokeCards()
        {
            foreach(var card in catalog.Cards.Where(c=>c.Expansion=="planned-decks-20261001")){
                try{
                    var game=Game();game.Board[60].CapitalOwner=game.Active;var ally=Add(game,"MED-085");Add(game,"MED-134");Add(game,"PD26-F-001");Add(game,"MED-085",59);Add(game,"MED-112");var vehicle=Add(game,"PD26-N-066");Add(game,"MED-229",61,1-game.Active);Add(game,"MED-112",61,1-game.Active);
                    game.Seats[game.Active].grave.Add(catalog.Get("MED-085"));game.Seats[game.Active].grave.Add(catalog.Get("MED-123"));game.Seats[game.Active].grave.Add(catalog.Get("MED-090"));
                    game.Seats[1-game.Active].grave.AddMilled(catalog.Get("MED-085"));
                    if(card.Kind==CardType.Terrain){game.Board[60].Terrain=card;game.Board[60].CapitalOwner=game.Active;Invoke(game,"AbilitiesEntered",ally);Settle(game);if(game.CanActivateTerrain(60)){Do(game,ActionKind.ActivateTerrain,60);Settle(game);}}
                    else {game.Seats[game.Active].hand.Add(card);int host=card.Id=="PD26-G-028"?vehicle.Id:ally.Id;Check(game.CanPlay(card,60,host),"can play imported "+card.Id);Do(game,ActionKind.Play,60,host,card.Id);Settle(game);var permanent=game.Board.SelectMany(c=>c.Pieces).FirstOrDefault(p=>p.Card==card);if(permanent!=null&&game.CanActivate(permanent.Id)){Do(game,ActionKind.Activate,unit:permanent.Id);Settle(game);}if(permanent!=null){var packed=NetworkCards.Unpack(NetworkCards.Pack(card));Check(packed.Abilities.Count==card.Abilities.Count,"ability serialization "+card.Id);}}
                }catch(Exception e){throw new Exception("IMPORTED CARD "+card.Id+" "+card.Name+": "+e.Message,e);}
            }
        }
    }
}
