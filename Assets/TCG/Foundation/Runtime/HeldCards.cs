using UnityEngine;
namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        readonly Transform[] heldHands=new Transform[4];
        void BuildHeldCards(int player,Transform place,Color skin)
        {
            var fan=heldHands[player]=Group("Cartas na mão",place);fan.localPosition=new Vector3(-.55f,-.15f,7.5f);fan.localRotation=Quaternion.Euler(-22,0,8);
            for(int i=0;i<5;i++)
            {
                var card=Group("Verso de carta "+i,fan);card.localPosition=new Vector3((i-2)*.11f,.14f+(.04f-Mathf.Abs(i-2)*.02f),i*.009f);card.localRotation=Quaternion.Euler(0,0,-(i-2)*12);
                Block("Borda",Vector3.zero,new Vector3(.29f,.43f,.018f),Hex("E5D4AD"),card);
                Block("Verso",new Vector3(0,0,-.014f),new Vector3(.25f,.385f,.012f),Seats[player]*.45f,card);
                var seal=Block("Selo",new Vector3(0,0,-.024f),new Vector3(.1f,.14f,.01f),Hex("D9B36A"),card);seal.localRotation=Quaternion.Euler(0,0,45);
            }
            Block("Polegar segurando cartas",new Vector3(-.06f,-.04f,-.055f),new Vector3(.1f,.18f,.08f),skin,fan);
            for(int i=0;i<3;i++)Block("Dedo",new Vector3(-.14f+i*.08f,-.12f,.08f),new Vector3(.06f,.12f,.12f),skin,fan);
            Block("Gola",new Vector3(0,.11f,8.39f),new Vector3(.65f,.23f,.12f),Hex("E5D4AD"),place);
            Block("Broche",new Vector3(.28f,.03f,8.3f),new Vector3(.15f,.17f,.06f),Hex("D9B36A"),place);
            if(player==1)Block("Barba",new Vector3(0,.43f,8.37f),new Vector3(.4f,.3f,.12f),Hex("654733"),place);
            if(player==2){Block("Faixa",new Vector3(0,.94f,8.385f),new Vector3(.67f,.12f,.05f),Seats[player],place);}
        }
        void TickHeldCards()
        {
            if(match==null)return;
            for(int i=0;i<match.Seats.Count;i++)if(heldHands[i]!=null)
            {
                heldHands[i].localRotation=Quaternion.Euler(-22+Mathf.Sin(Time.unscaledTime*1.4f+i)*2,0,8+Mathf.Sin(Time.unscaledTime*.8f+i));
                // Only card backs are shown in world space; private definitions stay in the owner's UI.
                int count=match.HandFor(i).Count;for(int n=0;n<5;n++)heldHands[i].GetChild(n).gameObject.SetActive(n<count);
                heldHands[i].gameObject.SetActive(count>0);
            }
        }
    }
}
