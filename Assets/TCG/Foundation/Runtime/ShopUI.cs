using System;
using System.Linq;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        bool shopCosmetics;
        string shopSearch="",pendingPurchase;
        bool purchaseIsCosmetic;
        int shopPage;
        void DrawShopPage()
        {
            Text(45,140,1060,60,"O MERCADO DOS REINOS",menuTitle,Ink);
            Fill(new Rect(1230,133,330,78),Panel);
            Text(1250,148,290,42,library.Data.coins+"  MOEDAS",heading,Gold);
            Text(48,219,1500,55,"Cartas avulsas, versos e boosters selados para ampliar seu arsenal.",body,Muted);
            if(Button(45,290,240,45,"Cartas",true,!shopCosmetics&&!shopBoosters)){shopCosmetics=false;shopBoosters=false;shopPage=0;}
            if(Button(302,290,240,45,"Cosméticos",true,shopCosmetics&&!shopBoosters)){shopCosmetics=true;shopBoosters=false;shopPage=0;}
            if(Button(559,290,240,45,"Boosters",true,shopBoosters)){shopBoosters=true;shopCosmetics=false;}
            Text(45,936,1515,50,"ECONOMIA DE TESTE · saldo inicial 500 · cartas 50 · versos 150 · forma de ganhar moedas ainda em definição",small,Muted);
            Text(45,882,1515,45,hubNotice,body,Gold);
            if(shopBoosters){DrawBoosterShop();return;}
            if(shopCosmetics)
            {
                for(int i=0;i<CosmeticCatalog.All.Length;i++)
                {
                    var c=CosmeticCatalog.All[i];float x=45+i*380;
                    Fill(new Rect(x,363,355,490),Panel);DrawBackPreview(new Rect(x+97,385,160,222),c);
                    Text(x+20,641,315,36,c.Name,heading,Gold);
                    Text(x+20,691,315,35,c.Price==0?"Incluso na coleção":c.Price+" moedas",body,Ink);
                    bool owned=library.OwnsCosmetic(c.Id);
                    if(Button(x+20,767,315,53,owned?"Na coleção":"Comprar verso",!owned&&collectionStore.CanWrite,true))
                    {pendingPurchase=c.Id;purchaseIsCosmetic=true;}
                }
                return;
            }
            Text(835,297,85,35,"Buscar",small,Muted);
            shopSearch=GUI.TextField(new Rect(920,291,640,42),shopSearch);
            var cards=catalog.Cards.Where(c=>c.Playable&&(c.Name+" "+c.Id+" "+c.Expansion).IndexOf(shopSearch,StringComparison.OrdinalIgnoreCase)>=0).OrderBy(c=>c.Name).ToArray();
            int pages=Math.Max(1,(cards.Length+3)/4);shopPage=Mathf.Clamp(shopPage,0,pages-1);
            for(int n=0;n<4&&shopPage*4+n<cards.Length;n++)
            {
                var c=cards[shopPage*4+n];float x=45+n*380;bool owned=library.Owns(c.Id);Fill(new Rect(x,350,355,457),Panel);
                if(DrawCard(new Rect(x+67,369,220,330),c))inspected=c;
                if(Button(x+20,738,315,50,owned?"Na coleção":"Comprar · 50 moedas",!owned&&collectionStore.CanWrite,true)){pendingPurchase=c.Id;purchaseIsCosmetic=false;}
            }
            if(Button(45,824,75,39,"‹",shopPage>0))shopPage--;
            Text(140,830,950,35,(shopPage+1)+" / "+pages+" · "+cards.Length+" cartas disponíveis",small,Muted);
            if(Button(1485,824,75,39,"›",shopPage<pages-1))shopPage++;
        }
        void DrawPurchaseConfirmation()
        {
            string name=purchaseIsCosmetic?CosmeticCatalog.Get(pendingPurchase).Name:catalog.Get(pendingPurchase).Name;
            int price=purchaseIsCosmetic?CosmeticCatalog.Get(pendingPurchase).Price:CollectionLibrary.TestCardPrice;
            Fill(new Rect(0,0,1600,1000),new Color(0,0,0,.86f));
            Fill(new Rect(410,255,780,490),Panel);
            Text(450,289,700,48,"CONFIRMAR COMPRA",heading,Gold);
            Text(450,369,700,65,name,heading,Ink);
            Text(450,460,700,96,price+" moedas · saldo atual "+library.Data.coins+"\n"+(library.Data.coins>=price?"Saldo após a compra: "+(library.Data.coins-price):"Você não tem moedas suficientes."),body,Muted);
            if(Button(450,631,330,59,"Cancelar"))pendingPurchase=null;
            if(Button(805,631,330,59,"Comprar",library.Data.coins>=price&&collectionStore.CanWrite,true))CompletePurchase();
        }
        bool CompletePurchase()
        {
            if(pendingPurchase==null)return false;
            string before=JsonUtility.ToJson(library.Data);string id=pendingPurchase;
            try
            {
                if(purchaseIsCosmetic)library.BuyCosmetic(id);else library.BuyCard(id);
                collectionStore.Save(library);
                hubNotice="Compra concluída. O item está na sua coleção.";pendingPurchase=null;return true;
            }
            catch(Exception e)
            {
                library=new CollectionLibrary(catalog,JsonUtility.FromJson<CollectionData>(before));
                hubNotice="Compra não concluída: "+e.Message;pendingPurchase=null;return false;
            }
        }
    }
}

