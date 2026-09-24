using System;
using System.Linq;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;

public static class MovementOrderChecks
{
    static int checks,id=4000;static ContentCatalog catalog;static EffectRegistry registry;
    static void Assert(bool ok,string label){checks++;if(!ok)throw new Exception("MOVEMENT ORDER: "+label);}
    static void Do(Match m,ActionKind kind,int target=-1,int unit=-1)
    {Assert(m.Try(new Command{kind=kind,player=m.Controller,revision=m.Revision,target=target,unit=unit,pile=0},out var error),kind+" "+error);}
    static Match Game()
    {
        var decks=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("MED-085",100).ToArray()).ToArray();
        var lands=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("test-land-0",50).ToArray()).ToArray();
        var m=new Match(catalog,registry,2,false,5,decks,lands){AutomaticResponses=true,AutoAdvanceAfterTerrain=true};
        Do(m,ActionKind.DrawMain);Do(m,ActionKind.Place,Match.Capital(m.Active,2));
        Assert(m.Phase==Stage.Main,"terreno avança diretamente para principal");
        foreach(var cell in m.Board){cell.Terrain=null;cell.Owner=-1;cell.pieces.Clear();}
        return m;
    }
    static void Land(Match m,params int[] tiles){foreach(int tile in tiles){m.Board[tile].Terrain=catalog.Get("test-land-0");m.Board[tile].Owner=m.Active;}}
    static Piece Add(Match m,int tile,string card="MED-085",int owner=-1)
    {var p=new Piece(id++,owner<0?m.Active:owner,catalog.Get(card)){Game=m};m.Board[tile].pieces.Add(p);return p;}
    static void NextMain(Match m)
    {
        // Match fixture restores a legal capital for the mandatory terrain action.
        Do(m,ActionKind.NextPhase);Do(m,ActionKind.DrawMain);int capital=Match.Capital(m.Active,2);Land(m,capital);Do(m,ActionKind.Place,capital);
    }
    public static void Run()
    {
        checks=0;registry=new EffectRegistry();catalog=ContentLoader.Load(registry);
        {var m=Game();Land(m,60,61,62,63,64);var p=Add(m,60);Do(m,ActionKind.PlanMove,64,p.Id);
         Assert(m.Position(p.Id)==62&&p.Movement==0&&m.OrderFor(p.Id)?.Destination==64,"anda dois passos e preserva destino");
         NextMain(m);Assert(m.Position(p.Id)==62,"não anda no turno do adversário");NextMain(m);
         Assert(m.Position(p.Id)==64&&m.OrderFor(p.Id)==null,"retoma no próprio turno e conclui");}
        {var m=Game();Land(m,60,62);var p=Add(m,60);Do(m,ActionKind.PlanMove,62,p.Id);
         Assert(m.Position(p.Id)==60&&p.Movement==2&&m.OrderFor(p.Id)?.State==MovementOrderState.WaitingForTerrain,"espera no buraco sem gastar movimento");
         NextMain(m);Land(m,61);NextMain(m);Assert(m.Position(p.Id)==62&&m.OrderFor(p.Id)==null,"anda depois de preencher o buraco");}
        {var m=Game();Land(m,60,61,62,63);var p=Add(m,60);Do(m,ActionKind.PlanMove,63,p.Id);NextMain(m);
         Do(m,ActionKind.NextPhase);Do(m,ActionKind.DrawMain);Do(m,ActionKind.CancelMove,unit:p.Id);int home=Match.Capital(m.Active,2);Land(m,home);Do(m,ActionKind.Place,home);
         Assert(m.Position(p.Id)==62&&m.OrderFor(p.Id)==null,"cancelamento antes da colocação impede avanço");}
        {var m=Game();Land(m,60,61,62);var p=Add(m,60);var enemy=Add(m,61,owner:1-m.Active);Do(m,ActionKind.PlanMove,61,p.Id);
         Assert(m.Position(p.Id)==60&&m.Stack.Count==0&&m.OrderFor(p.Id)!=null,"não transforma rota em ataque");
         m.Board[61].pieces.Remove(enemy);NextMain(m);NextMain(m);Assert(m.Position(p.Id)==61,"aguarda adversário sair");}
        {var m=Game();Land(m,60,61,62);var p=Add(m,60);Add(m,61,"MED-112");Add(m,61);Do(m,ActionKind.PlanMove,61,p.Id);
         Assert(m.Position(p.Id)==60&&p.Movement==2,"construção bloqueadora impede rota");
         Do(m,ActionKind.CancelMove,unit:-1);Assert(m.OrdersFor(m.Active).Count==0,"cancelar todas as rotas");}
        {var m=Game();Land(m,60,61,62,63);var p=Add(m,60);Do(m,ActionKind.PlanMove,63,p.Id);m.Deal(p,99);Do(m,ActionKind.NextPhase);
         Assert(m.OrderFor(p.Id)==null,"morte limpa rota");}
        {var m=Game();Land(m,60,61,62,63);var p=Add(m,60);Add(m,71,"MED-144");Do(m,ActionKind.PlanMove,63,p.Id);
         Assert(m.Position(p.Id)==61&&m.Choice!=null,"gatilho interrompe avanço para escolha");
         Do(m,ActionKind.Choose,62);Assert(m.Position(p.Id)==63&&p.Movement==0&&m.Choice==null,"retoma após escolha sem exceder recursos");}
        {var m=Game();Land(m,60,61,62);var p=Add(m,60);Do(m,ActionKind.PlanMove,62,p.Id);
         Assert(m.Position(p.Id)==62&&m.OrderFor(p.Id)==null,"arraste dentro do orçamento conclui já");
         int revision=m.Revision;bool accepted=m.Try(new Command{kind=ActionKind.PlanMove,player=m.Controller,revision=revision-1,unit=p.Id,target=59},out _);
         Assert(!accepted&&m.Revision==revision,"comando obsoleto rejeitado");}
        {var m=Game();Land(m,60,61);var p=Add(m,60,"MED-112");bool accepted=m.Try(new Command{kind=ActionKind.PlanMove,player=m.Controller,revision=m.Revision,unit=p.Id,target=61},out _);
         Assert(!accepted&&m.Position(p.Id)==60,"construção imóvel não recebe rota");}
        Debug.Log("MOVEMENT ORDER CHECKS PASSED: "+checks);
    }
}
