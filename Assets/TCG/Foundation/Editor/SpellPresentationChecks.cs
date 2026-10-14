using System;
using System.Linq;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;
public static class SpellPresentationChecks
{
    public static void Run()
    {
        int checks=0;void Check(bool value,string reason){checks++;if(!value)throw new Exception("SPELL PRESENTATION: "+reason);}
        Definition Card(CardType kind,int color=6,int[] identity=null,string operation=null)=>NetworkCards.Unpack(new CardData{id="vfx-fixture",name="VFX",kind=kind.ToString(),color=color,identityColors=identity??Array.Empty<int>(),effects=operation==null?Array.Empty<EffectData>():new[]{new EffectData{operation=operation,amount=1}}});
        foreach(CardType kind in Enum.GetValues(typeof(CardType)))
        {
            bool expected=kind==CardType.Instant||kind==CardType.Spell;
            Check(SpellPresentation.TryPlan(new MatchEvent("cast",55,65,card:Card(kind)),out _,out _)==expected,"only nonpermanent spell kinds "+kind);
        }
        var spell=Card(CardType.Instant);
        foreach(string kind in new[]{"cast","trigger"}){Check(SpellPresentation.TryPlan(new MatchEvent(kind,55,65,card:spell),out var stage,out int tile)&&stage==SpellVisualStage.Announcement&&tile==55,"announcement at origin "+kind);}
        foreach(string kind in new[]{"resolve","impact"}){Check(SpellPresentation.TryPlan(new MatchEvent(kind,55,65,card:spell),out var stage,out int tile)&&stage==SpellVisualStage.Resolution&&tile==65,"target only on resolution "+kind);}
        Check(SpellPresentation.TryPlan(new MatchEvent("stack-canceled",55,65,card:spell),out var canceled,out int source)&&canceled==SpellVisualStage.Cancellation&&source==55,"cancel never hits target");
        foreach(string kind in new[]{"stack-resolved","summon","attack","move","death"})Check(!SpellPresentation.TryPlan(new MatchEvent(kind,55,65,card:spell),out _,out _),"no duplicate/irrelevant visuals "+kind);
        Check(!SpellPresentation.TryPlan(null,out _,out _),"null event");
        Check(!SpellPresentation.TryPlan(new MatchEvent("cast",-1,65,card:spell),out _,out _),"invalid origin");
        Check(!SpellPresentation.TryPlan(new MatchEvent("impact",55,121,card:spell),out _,out _),"invalid target");
        Check(SpellPresentation.Colors(Card(CardType.Spell,2,new[]{2,4})).SequenceEqual(new[]{2,4}),"bicolor retained in individual filaments");
        Check(SpellPresentation.Colors(spell).SequenceEqual(new[]{6}),"colorless palette");
        var printed=NetworkCards.Unpack(new CardData{id="mana-palette",name="Palette",kind="Instant",color=6,identityColors=new[]{6},coloredCost=new[]{2,1,0,0,0,0,0},cost=3});
        Check(SpellPresentation.Colors(printed).SequenceEqual(new[]{0,1}),"printed mana colors take precedence over generic portrait color");
        for(int color=0;color<7;color++)Check(SpellPresentation.Family(Card(CardType.Spell,color))==(SpellVisualFamily)color,"distinct color motif "+color);
        Check(SpellPresentation.Family(Card(CardType.Spell,2,operation:"damage"))==SpellVisualFamily.Flame,"damage motif");
        Check(SpellPresentation.Family(Card(CardType.Spell,3,operation:"heal"))==SpellVisualFamily.Growth,"heal motif");
        Check(SpellPresentation.Family(Card(CardType.Spell,3,operation:"draw"))==SpellVisualFamily.Arcane,"draw motif");
        var catalog=ContentLoader.Load(new EffectRegistry());
        var precon=PreconstructedDeckLoader.Load(catalog).Decks.First(d=>d.id=="COL001-D02");
        var cards=precon.main.Select(catalog.Get).Where(c=>c.Kind==CardType.Spell||c.Kind==CardType.Instant).ToArray();
        Check(cards.Select(SpellPresentation.Family).Distinct().Count()>=4,"D02 abilities produce varied motifs despite one mana color");
        foreach(var card in cards)Check(SpellPresentation.TryPlan(new MatchEvent("impact",55,65,card:card),out _,out int tile)&&tile==65,"D02 nonpermanent supported "+card.Id);
        foreach(SpellVisualStage stage in Enum.GetValues(typeof(SpellVisualStage)))Check(SpellPresentation.Duration(stage)>0&&SpellPresentation.Duration(stage)<=1.6f,"finite lifetime "+stage);
        Check(SpellPresentation.MaximumInstances==24,"bounded simultaneous instances");
        Debug.Log("SPELL PRESENTATION CHECKS PASSED: "+checks);
    }
}
