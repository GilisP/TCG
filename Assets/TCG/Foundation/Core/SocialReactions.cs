using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG.Foundation
{
    public enum ReactionKind { ThumbsUp, Angry, Happy, Sad, Celebrate }
    public sealed class SocialReaction
    {
        public int Sender {get;} public int Target {get;} public ReactionKind Kind {get;} public double StartedAt {get;}
        public const double Duration=5;
        internal SocialReaction(int sender,int target,ReactionKind kind,double time){Sender=sender;Target=target;Kind=kind;StartedAt=time;}
    }
    // Ephemeral social presentation. Never changes match state, commands or priority.
    public sealed class ReactionChannel
    {
        readonly Dictionary<int,SocialReaction> latest=new Dictionary<int,SocialReaction>();
        public event Action<SocialReaction> Published;
        public IEnumerable<SocialReaction> Visible(double time)=>latest.Values.Where(r=>time>=r.StartedAt&&time-r.StartedAt<SocialReaction.Duration);
        public bool Publish(Match match,int sender,ReactionKind kind,int target,double time,out string error)
        {
            error="";
            if(match==null||match.Over||sender<0||sender>=match.Seats.Count||sender!=match.Controller||match.Seats[sender].Eliminated){error="Use o avatar do jogador que controla a tela.";return false;}
            if(!Enum.IsDefined(typeof(ReactionKind),kind)||double.IsNaN(time)||double.IsInfinity(time)||time<0){error="Reação inválida.";return false;}
            if(target < -1||target>=match.Seats.Count||target==sender||target>=0&&match.Seats[target].Eliminated){error="Escolha outro jogador ou toda a mesa.";return false;}
            if(latest.TryGetValue(sender,out var last)&&time-last.StartedAt<1){error="Aguarde um instante entre reações.";return false;}
            var reaction=new SocialReaction(sender,target,kind,time);latest[sender]=reaction;Published?.Invoke(reaction);return true;
        }
    }
}
