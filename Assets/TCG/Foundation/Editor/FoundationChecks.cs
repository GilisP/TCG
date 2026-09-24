using System;
using System.Linq;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;

namespace TCG.Foundation.Editor
{
    public static class FoundationChecks
    {
        static int count;
        static void Assert(bool ok,string label) { count++; if(!ok) throw new Exception("BASE: "+label); }
        static Command C(Match m,ActionKind kind,int target=-1,int unit=-1,string card=null,int[] ids=null,int[] blockers=null)=>new Command{player=m.Controller,revision=m.Revision,kind=kind,target=target,unit=unit,card=card,pile=0,units=ids??Array.Empty<int>(),blockers=blockers??Array.Empty<int>()};
        static void Do(Match m,Command c) { if(!m.Try(c,out var error)) throw new Exception("Ação falhou: "+c.kind+" / "+error); }
        static void PassAll(Match m) { int guard=20; while(m.Stack.Count>0&&guard-->0) Do(m,C(m,ActionKind.Pass)); Assert(guard>0,"pilha termina"); }
        static void Main(Match m) { Do(m,C(m,ActionKind.DrawMain)); Do(m,C(m,ActionKind.Place,Match.Capital(m.Active,m.Seats.Count))); Do(m,C(m,ActionKind.NextPhase)); }
        [UnityEditor.MenuItem("TCG/Base/Validar nova base")]
        public static void Run()
        {
            count=0; var registry=new EffectRegistry(); var catalog=ContentLoader.Load(registry);
            Assert(catalog.Expansions.Count>=4&&catalog.Cards.Count>=24,"quatro pacotes de teste carregados");
            foreach(int n in new[]{2,3,4})
            {
                var m=ContentLoader.TestTable(catalog,registry,n,false,37);
                Assert(m.Seats.Count==n,"quantidade dinâmica"); Assert(m.Board.Count(c=>c.CapitalOwner>=0)==n,"capital por jogador");
                Assert(m.Board.Count(c=>c.Terrain!=null)==n*4,"capital + três terrenos iniciais");
                Assert(m.Seats.All(s=>Enumerable.Range(0,4).All(p=>s.PileCount(p)==2)),"quatro pilhas de duas");
                Assert(m.Seats.All(s=>s.HandCount==5),"mão de teste explícita"); Assert(m.Seats[m.Active].Mana.Sum()==1,"somente capital produz mana inicial");
                int before=m.Revision; var wrong=C(m,ActionKind.DrawMain); wrong.player=(m.Controller+1)%n;
                Assert(!m.Try(wrong,out _)&&m.Revision==before&&m.Phase==Stage.Draw,"jogador errado não altera estado");
                var old=C(m,ActionKind.DrawMain); old.revision--;
                Assert(!m.Try(old,out _)&&m.Revision==before,"revisão antiga rejeitada");
                int hand=m.Seats[m.Active].HandCount,lands=m.Seats[m.Active].TerrainCount;
                Do(m,C(m,ActionKind.DrawMain)); Assert(m.Seats[m.Active].HandCount==hand+1&&m.Seats[m.Active].TerrainCount==lands,"primeiro compra somente principal");
                Assert(!m.Try(C(m,ActionKind.NextPhase),out _),"terreno obrigatório");
                int capital=Match.Capital(m.Active,n); int pile=m.Seats[m.Active].PileCount(0);
                Do(m,C(m,ActionKind.Place,capital)); Assert(m.Seats[m.Active].PileCount(0)==pile-1,"usa só topo da pilha");
                Assert(m.Board[capital].CapitalOwner==m.Active,"substituir capital preserva capital");
                Do(m,C(m,ActionKind.NextPhase)); int active=m.Active;
                Do(m,C(m,ActionKind.NextPhase)); for(int p=0;p<n;p++) Do(m,C(m,ActionKind.Pass));
                Assert(m.Active!=active&&m.Turn==2,"turno percorre jogadores");
                hand=m.Seats[m.Active].HandCount; lands=m.Seats[m.Active].TerrainCount;
                Do(m,C(m,ActionKind.DrawMain)); Assert(m.Seats[m.Active].HandCount==hand+1&&m.Seats[m.Active].TerrainCount==lands-1,"compras normais separadas");
            }
            var game=ContentLoader.TestTable(catalog,registry,4,false,11); Main(game);
            var creature=catalog.Cards.First(c=>c.Kind==CardType.Creature); int home=Match.Capital(game.Active,4);
            var activeSeat=game.Seats[game.Active]; activeSeat.hand.Add(creature); activeSeat.mana[6]=30;
            Do(game,C(game,ActionKind.Play,home,card:creature.Id)); Assert(game.Board[home].Pieces.Count==0,"invocação aguarda prioridade");
            for(int n=0;n<3;n++) Do(game,C(game,ActionKind.Pass)); Assert(game.Stack.Count==1,"três passes não resolvem mesa de quatro");
            Do(game,C(game,ActionKind.Pass)); Assert(game.Board[home].Pieces.Count==1,"quarto passe resolve");
            activeSeat.hand.Add(creature); Do(game,C(game,ActionKind.Play,home,card:creature.Id)); PassAll(game);
            Assert(game.Board[home].Pieces.Count==2,"mais de uma criatura na casa");
            var piece=game.Board[home].Pieces[0]; int free=Match.Neighbors(home).First(); game.Board[free].Terrain=null;
            Assert(!game.CanMove(piece.Id,free),"vazio bloqueia movimento");
            game.Board[free].Terrain=catalog.Cards.First(c=>c.Kind==CardType.Terrain); game.Board[free].Owner=game.Active;
            Do(game,C(game,ActionKind.Move,free,piece.Id)); Assert(game.Position(piece.Id)==free&&piece.Movement==creature.Movement-1,"movimento ortogonal");
            // Four 1/1 against one 3/3: all damage is assigned before any death.
            var fixture=new ContentCatalog();
            fixture.Add(new ExpansionData{id="fixture",title="Teste",cards=new[]{new CardData{id="one",name="Um",kind="Creature",attack=1,defense=1},new CardData{id="three",name="Três",kind="Creature",attack=3,defense=3}}},registry.Keys);
            int source=60,target=61,owner=game.Active,enemy=(owner+1)%4;
            game.Board[source].Terrain=game.Board[free].Terrain; game.Board[source].Owner=owner; game.Board[target].Terrain=game.Board[free].Terrain; game.Board[target].Owner=enemy;
            var attackers=Enumerable.Range(100,4).Select(id=>new Piece(id,owner,fixture.Get("one"))).ToArray(); game.Board[source].pieces.AddRange(attackers);
            var defender=new Piece(200,enemy,fixture.Get("three")); game.Board[target].pieces.Add(defender);
            Do(game,C(game,ActionKind.Attack,target,ids:attackers.Select(p=>p.Id).ToArray()));
            Assert(game.Controller==owner&&game.Defense!=null,"atacante escolhe confrontos fora da capital");
            Do(game,C(game,ActionKind.Defend,blockers:Enumerable.Repeat(defender.Id,4).ToArray())); PassAll(game);
            Assert(game.Find(200)==null&&attackers.Count(a=>game.Find(a.Id)==null)==3,"3/3 vs quatro 1/1");
            // Invalid pack must be atomic; reprints resolve to the canonical deck identity.
            int cardsBefore=catalog.Cards.Count;
            try { catalog.Add(new ExpansionData{id="bad",title="Ruim",cards=new[]{new CardData{id="bad-a",name="Ruim",kind="Spell",effects=new[]{new EffectData{operation="unsupported",amount=1}}}}},registry.Keys); Assert(false,"aceitou efeito desconhecido"); } catch(InvalidOperationException) { Assert(catalog.Cards.Count==cardsBefore&&!catalog.Expansions.ContainsKey("bad"),"expansão inválida não aplica parcialmente"); }
            catalog.Add(new ExpansionData{id="reprint",title="Reimpressão",printings=new[]{new PrintingData{id="art-2",cardId=creature.Id}}},registry.Keys);
            Assert(catalog.Identity("art-2")==creature.Id,"reimpressão mantém identidade");
            var unordered=new ContentCatalog();
            unordered.AddAll(new[]{new ExpansionData{id="b",title="B",printings=new[]{new PrintingData{id="b-art",cardId="a-card"}}},new ExpansionData{id="a",title="A",cards=new[]{new CardData{id="a-card",name="Original A",kind="Creature"}}}},registry.Keys);
            Assert(unordered.Identity("b-art")=="a-card","reimpressão independe da ordem dos arquivos");
            try { unordered.AddAll(new[]{new ExpansionData{id="c",title="C",cards=new[]{new CardData{id="c-card",name="Original C",kind="Creature"}}},new ExpansionData{id="d",title="D",printings=new[]{new PrintingData{id="d-art",cardId="missing"}}}},registry.Keys); Assert(false,"aceitou lote inválido"); }
            catch(InvalidOperationException) { Assert(unordered.Cards.Count==1&&!unordered.Expansions.ContainsKey("c"),"lote inteiro é atômico"); }
            var teams=ContentLoader.TestTable(catalogWithoutReprint(),registry,4,true,9); int ally=(teams.Active+2)%4;
            Assert(!teams.Enemies(teams.Active,ally),"duplas opostas não se atacam");
            var deckout=ContentLoader.TestTable(catalogWithoutReprint(),registry,3,false,9); int loser=deckout.Active; deckout.Seats[loser].main.Clear();
            Assert(!deckout.Seats[loser].Eliminated,"deck vazio não elimina antes da compra");
            Do(deckout,C(deckout,ActionKind.DrawMain)); Assert(deckout.Seats[loser].Eliminated&&!deckout.Over&&deckout.Active!=loser,"deckout elimina e segue com dois");
            Assert(deckout.Board[Match.Capital(loser,3)].Terrain!=null,"eliminação preserva terrenos");
            var refill=ContentLoader.TestTable(catalogWithoutReprint(),registry,2,false,55); int refillOwner=refill.Active;
            Do(refill,C(refill,ActionKind.DrawTerrain)); Assert(refill.Seats[refillOwner].HandCount==5&&refill.Seats[refillOwner].PileCount(0)==3,"primeiro compra somente terreno");
            var refillSeat=refill.Seats[refillOwner]; refillSeat.piles[0].RemoveRange(0,2); int terrainCount=refillSeat.TerrainCount;
            int replaced=Match.Capital(refillOwner,2); var casualty=new Piece(600,refillOwner,creature); refill.Board[replaced].pieces.Add(casualty);
            Do(refill,C(refill,ActionKind.Place,replaced)); Assert(refillSeat.PileCount(0)==1&&refillSeat.TerrainCount==terrainCount-1,"pilha vazia repõe imediatamente");
            Assert(refill.Find(600)==null&&refillSeat.Graveyard.Contains(creature),"substituir destrói ocupantes");
            Do(refill,C(refill,ActionKind.NextPhase)); var hurt=new Piece(601,refillOwner,fixture.Get("three")){Damage=1}; refill.Board[replaced].pieces.Add(hurt);
            Do(refill,C(refill,ActionKind.NextPhase)); Do(refill,C(refill,ActionKind.Pass)); Do(refill,C(refill,ActionKind.Pass)); Assert(hurt.Damage==0,"limpeza no fim do próprio turno");
            hurt.Damage=1; Main(refill); Do(refill,C(refill,ActionKind.NextPhase)); Do(refill,C(refill,ActionKind.Pass)); Do(refill,C(refill,ActionKind.Pass)); Assert(hurt.Damage==0,"limpeza no início do próprio turno");
            var lethal=ContentLoader.TestTable(catalogWithoutReprint(),registry,2,false,5); Main(lethal); int victim=(lethal.Active+1)%2,capitalVictim=Match.Capital(victim,2),origin=Match.Neighbors(capitalVictim).First();
            lethal.Seats[victim].Life=1; lethal.Board[origin].Terrain=catalog.Cards.First(c=>c.Kind==CardType.Terrain); lethal.Board[origin].Owner=lethal.Active;
            lethal.Board[origin].pieces.Add(new Piece(700,lethal.Active,fixture.Get("one")));
            Do(lethal,C(lethal,ActionKind.Attack,capitalVictim,ids:new[]{700})); Do(lethal,C(lethal,ActionKind.Defend,blockers:new[]{-1})); PassAll(lethal);
            Assert(lethal.Over&&lethal.Seats[victim].Eliminated&&lethal.WinningTeam!=lethal.Seats[victim].Team,"capital sem bloqueio permite vitória");
            // Catalog growth does not alter starter decks or require source changes.
            var large=new ContentCatalog(); var timer=System.Diagnostics.Stopwatch.StartNew();
            for(int set=0;set<40;set++) large.Add(new ExpansionData{id="scale-"+set,title="Expansão "+set,cards=Enumerable.Range(0,300).Select(i=>new CardData{id="scale-"+set+"-"+i,name="Carta "+set+" / "+i,kind="Creature",attack=1,defense=1}).ToArray()},registry.Keys);
            Assert(large.Cards.Count==12000&&large.Expansions.Count==40,"40 expansões / 12.000 definições");
            Debug.Log("FOUNDATION CATALOG SCALE: 12000 cards / "+timer.ElapsedMilliseconds+" ms (editor, data only)");
            Debug.Log("FOUNDATION CHECKS PASSED: "+count);
            ContentCatalog catalogWithoutReprint()=>ContentLoader.Load(registry);
        }
    }
}
