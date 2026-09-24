using System;
using System.Linq;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;

public static class ReactionChecks
{
    public static void Run()
    {
        int checks=0;void Assert(bool ok,string label){checks++;if(!ok)throw new Exception("REACTION: "+label);}
        var registry=new EffectRegistry();var catalog=ContentLoader.Load(registry);var m=ContentLoader.TestTable(catalog,registry,4,false,5);var channel=new ReactionChannel();int sender=m.Controller,target=(sender+1)%4,revision=m.Revision;
        foreach(ReactionKind kind in Enum.GetValues(typeof(ReactionKind)))Assert(channel.Publish(m,sender,kind,-1,10+(int)kind*2,out _),"reação "+kind);
        Assert(channel.Publish(m,sender,ReactionKind.ThumbsUp,target,30,out _),"direcionada");
        Assert(channel.Visible(30).Single().Target==target,"cor alvo preservada");
        Assert(!channel.Publish(m,target,ReactionKind.Angry,sender,32,out _),"não usa avatar alheio");
        Assert(!channel.Publish(m,sender,ReactionKind.Angry,sender,32,out _),"não dirige a si");
        Assert(!channel.Publish(m,sender,ReactionKind.Angry,4,32,out _),"alvo fora da mesa");
        Assert(!channel.Publish(m,sender,ReactionKind.Angry,-1,30.5,out _),"intervalo mínimo");
        Assert(!channel.Visible(35).Any(),"balão expira");
        Assert(m.Revision==revision,"reação não altera partida");
        Debug.Log("REACTION CHECKS PASSED: "+checks);
    }
}
