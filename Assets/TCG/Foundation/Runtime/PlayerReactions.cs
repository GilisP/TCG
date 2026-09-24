using System;
using System.Linq;
using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
    public sealed class PlayerAvatarHit:MonoBehaviour { public int Player; }
    public sealed partial class TableWorld
    {
        public Vector3 AvatarPosition(int player)=>places[player].TransformPoint(new Vector3(0,.7f,8.7f));
        public Vector3 AvatarBubblePosition(int player)=>places[player].TransformPoint(new Vector3(0,2.2f,8.7f));
    }
    public sealed partial class TableView
    {
        ReactionChannel reactions=new ReactionChannel();
        int reactionMenu=-1;ReactionKind? reactionTargetPicker;
        readonly Texture2D[] reactionIcons=new Texture2D[5];
        static readonly string[] ReactionNames={"Joia","Bravo","Feliz","Triste","Comemorar"};
        bool ReactionInputAllowed=>!menu&&!handoff&&!collection&&!help&&!confirmQuit&&!match.Over&&inspected==null&&match.Choice==null&&equipmentToAttach<0;
        bool HandleAvatarReaction(Event e)
        {
            if(e.type!=EventType.MouseDown||e.button!=0||!BoardArea.Contains(e.mousePosition))return false;
            var ray=world.View.ScreenPointToRay(new Vector3(e.mousePosition.x*scale,Screen.height-e.mousePosition.y*scale,0));
            if(!Physics.Raycast(ray,out var hit,70)||!hit.collider.TryGetComponent<PlayerAvatarHit>(out var avatar))return false;
            ResetDrag();
            if(ReactionInputAllowed&&avatar.Player==Viewer)
            {reactionMenu=avatar.Player;reactionTargetPicker=null;selectedCard=null;}
            else notice="Clique no seu avatar: "+match.Seats[Viewer].Name+".";
            e.Use();return true;
        }
        void CloseReactionMenu(){reactionMenu=-1;reactionTargetPicker=null;}
        bool SendReaction(ReactionKind kind,int target)
        {
            if(!reactions.Publish(match,reactionMenu,kind,target,Time.unscaledTimeAsDouble,out var error)){notice=error;return false;}
            CloseReactionMenu();return true;
        }
        Vector2 ReactionPoint(int player)
        {
            var p=world.View.WorldToScreenPoint(world.AvatarBubblePosition(player));
            return new Vector2(Mathf.Clamp(p.x/scale,90,1100),Mathf.Clamp((Screen.height-p.y)/scale,190,680));
        }
        Texture2D ReactionIcon(ReactionKind kind)
        {int i=(int)kind;return reactionIcons[i]!=null?reactionIcons[i]:reactionIcons[i]=ReactionIconArt.Create(kind);}
        Rect ReactionMenuRect()
        {var point=ReactionPoint(reactionMenu);return new Rect(Mathf.Clamp(point.x-250,18,670),Mathf.Clamp(point.y-80,165,510),500,reactionTargetPicker.HasValue?235:175);}
        void DrawSocialReactions()
        {
            if(menu||collection||help||confirmQuit)return;
            foreach(var reaction in reactions.Visible(Time.unscaledTimeAsDouble))
            {
                var point=ReactionPoint(reaction.Sender);double age=Time.unscaledTimeAsDouble-reaction.StartedAt;
                var old=GUI.color;GUI.color=new Color(1,1,1,(float)Math.Min(1,(SocialReaction.Duration-age)*2));
                var box=new Rect(point.x-65,point.y-110,130,105);
                Fill(new Rect(box.x-3,box.y-3,box.width+6,box.height+6),TableWorld.Seats[reaction.Sender]);Fill(box,Panel);
                GUI.DrawTexture(new Rect(point.x-31,box.y+4,62,62),ReactionIcon(reaction.Kind));
                Text(box.x+6,box.y+68,118,28,reaction.Target<0?"Para a mesa":"Para "+match.Seats[reaction.Target].Name,small,reaction.Target<0?Ink:TableWorld.Seats[reaction.Target]);
                Fill(new Rect(point.x-5,box.yMax,10,12),TableWorld.Seats[reaction.Sender]);GUI.color=old;
            }
            if(reactionMenu<0)return;
            if(!ReactionInputAllowed||reactionMenu!=Viewer){CloseReactionMenu();return;}
            var rect=ReactionMenuRect();Fill(new Rect(rect.x-3,rect.y-3,rect.width+6,rect.height+6),TableWorld.Seats[reactionMenu]);Fill(rect,Dark);
            Text(rect.x+15,rect.y+10,410,28,"REAÇÕES · "+match.Seats[reactionMenu].Name,cardName,Ink);
            if(Button(rect.xMax-42,rect.y+8,32,28,"×")){CloseReactionMenu();return;}
            for(int i=0;i<5;i++)
            {
                var button=new Rect(rect.x+12+i*96,rect.y+46,90,94);Fill(button,Panel);
                GUI.DrawTexture(new Rect(button.x+17,button.y+5,56,56),ReactionIcon((ReactionKind)i));
                Text(button.x+4,button.y+64,84,22,ReactionNames[i],small,Ink);
                if(GUI.Button(button,GUIContent.none,GUIStyle.none))
                {
                    var kind=(ReactionKind)i;
                    if(kind==ReactionKind.ThumbsUp||kind==ReactionKind.Angry||kind==ReactionKind.Celebrate)reactionTargetPicker=kind;
                    else{SendReaction(kind,-1);return;}
                }
            }
            if(reactionTargetPicker.HasValue)
            {
                Text(rect.x+14,rect.y+145,470,23,"Para quem? Escolha uma cor ou toda a mesa.",small,Muted);
                if(Button(rect.x+12,rect.y+181,110,38,"Toda a mesa")){SendReaction(reactionTargetPicker.Value,-1);return;}
                int at=0;for(int p=0;p<match.Seats.Count;p++)if(p!=reactionMenu&&!match.Seats[p].Eliminated)
                {
                    float x=rect.x+132+at++*119;Fill(new Rect(x,rect.y+178,111,4),TableWorld.Seats[p]);
                    if(Button(x,rect.y+181,111,38,match.Seats[p].Name)){SendReaction(reactionTargetPicker.Value,p);return;}
                }
            }
            else Text(rect.x+14,rect.y+145,475,23,"Joia, bravo e comemorar permitem escolher um jogador.",small,Muted);
            var e=Event.current;
            if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Escape){CloseReactionMenu();e.Use();}
            if(e.type==EventType.MouseDown&&!rect.Contains(e.mousePosition)){CloseReactionMenu();e.Use();}
        }
        void DisposeReactionIcons(){foreach(var icon in reactionIcons)if(icon!=null)Destroy(icon);}
    }
}
