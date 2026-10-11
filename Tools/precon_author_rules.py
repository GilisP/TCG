"""Complete approved author/revision effects, with no runtime text parsing."""
import unicodedata

def norm(s):
    return ' '.join(''.join(x for x in unicodedata.normalize('NFD',s.casefold()) if not unicodedata.combining(x)).split()).rstrip('.')
def op(kind,a=0,b=0,c=0,value=''):
    return dict(kind=kind,a=a,b=b,c=c,value=value)
def ability(*ops,**kw):
    return dict(dict(trigger='cast',target='owner',scope='any',maxCost=999,count=1,manaColor=-1,ops=list(ops)),**kw)

def compile_card(d,c):
    t=norm(d.get('Efeito',c.get('text','')));out=None;traits=[];equip=None
    if t==norm('2pa: olhe a carta do topo do grimorio alvo.'):
        out=[ability(op('pca-look-player'),trigger='activate',target='player',pa=2)]
    elif t==norm('sempre que um terreno é colocado em 1 tile de distancia do caranquejo de estimação os oponentes trituram 1 carta.'):
        out=[ability(op('pca-mill-enemies',1),trigger='pca-near-terrain')]
    elif t==norm('troque a critura em combate alvo que voce controla por outra criatura alvo que voce controla'):
        out=[ability(op('pca-swap-combat'),target='pca-combat-own')]
    elif t==norm('1pa: marque um tile caso o conquiste este turno compre duas cartas'):
        out=[ability(op('pca-mark-tile'),trigger='activate',pa=1)]
    elif t==norm('o jogador alvo tritura cinco cartas.'):
        out=[ability(op('mill',5),target='player')]
    elif t==norm('escolha um tile. empurre dois tiles para longe tudo que esteja no caminho. caso no tile exato toma 3 de dano'):
        out=[ability(op('pca-wave'))]
    elif t==norm('tem x pas sendo x o maior valor dentre as cartas de cada cemiterio somado. 6pa: impossibilite o movimento sob um tile adjacente até seu proximo turno'):
        traits=['pca-jonas'];out=[ability(op('pca-block-tile'),trigger='activate',pa=6)]
    elif t==norm('1pa:exile a criatura alvo de um cemiterio. o jogador dono do cemiterio trirura 2 cartas.'):
        out=[ability(op('pca-exile-grave-mill',2),trigger='activate',pa=1,gate='pca-any-grave-creature')]
    elif t==norm('equip 2: a criatura equipada tem ou o dobro de defesa ou de ataque escolhido ao acaso todo turno'):
        equip=2;traits=['pca-document'];out=[ability(op('pca-document'),trigger='aura',target='host')]
    elif t==norm('ganha um marcardor toda vez que uma criatura vai para o cemitero. no começo de seu tuno gere em seu tile um horror x/x que vai atacar uma captal inimiga, e volte a 0 marcadores.'):
        traits=['pca-factory'];out=[ability(op('pca-horror'),trigger='turn')]
    elif t==norm('toda criatura em seu cemiterio tem "Exumar 1: pague uma mana incolor e exile esse card. compre um card"'):
        traits=['pca-exumar'];out=[ability(op('pca-exumar'),trigger='activate',manaColor=6,colored=1,gate='pca-own-grave-creature')]
    elif t==norm('o tile encantado da -x/-x para toda criatura nele sendo x o numero de criaturas no cemiterio do proprio jogador dono da criatura.'):
        traits=['pca-tile-enchantment'];out=[ability(op('pca-symphony'),trigger='aura')]
    elif t==norm('de alvo numa criatura inimiga até 3 tiles de sua capital. você ganha o controle dela e move ela para seu tile mais proximo'):
        out=[ability(op('pca-persuade'),target='pca-persuade')]
    elif t==norm('uma vez por turno caso esse tile seja atacado e não tenha criaturas nele volte uma critura de seu cemiterio até o final do turno para a defender'):
        traits=['pca-realm-dead'];out=[ability(op('pca-realm-return'),trigger='pca-tile-attacked',once=True)]
    elif t==norm('Ao causar dano de combate, olhe as duas cartas do topo do seu grimório e devolva-as na ordem escolhida.'):
        out=[ability(op('orderTop',2),trigger='combatHit'),ability(op('orderTop',2),trigger='capitalHit')]
    elif t==norm('Ao causar dano de combate à uma criatura inimiga, coloque 2 cartas do topo do grimório daquele jogador no cemitério dele.'):
        out=[ability(op('pca-mill-victim',2,1),trigger='combatHit')]
    elif t==norm('Ao morrer, o dono do efeito que a matou tritura 5.'):
        out=[ability(op('pca-mill-killer',5),trigger='pca-killed')]
    elif t==norm('Ao morrer, você pode colocar um Truque do seu cemitério no topo do seu grimório.'):
        out=[ability(op('pcl-grave',999,1,value='instant:top'),trigger='death',optional=True)]
    elif t==norm('Ao causar dano de combate a uma criatura, o dono daquela criatura tritura um card.'):
        out=[ability(op('pca-mill-victim',1),trigger='combatHit')]
    elif t==norm('quando você conjurar um Truque, seus oponentes trituram uma carta.'):
        out=[ability(op('pca-mill-enemies',1),trigger='instant')]
    elif t==norm('Ao causar dano de combate à capital inimiga, olhe as 5 cartas do topo do seu grimório e devolva 1 o restante jogue no cemiterio.'):
        out=[ability(op('pca-top-keep-mill',5),trigger='capitalHit')]
    elif t==norm('1 PA: o opnente alvo tritura uma carta.'):
        out=[ability(op('pca-enemy-mill',1),trigger='activate',pa=1)]
    elif t==norm('Ao entrar, outra criatura sua neste terreno recebe +3/+0 e todo dano em criaturas deve ser triturado do grimorio tambem, até o fim do turno.'):
        out=[ability(op('buff',3),op('pca-mill-damage'),trigger='enter',target='other',scope='here')]
    elif t==norm('Ao entrar, você pode colocar uma carta da mão no fundo do grimório. Se fizer isso, volte uma carta de cemiterio para sua mão.'):
        out=[ability(op('pca-bottom-recover'),trigger='enter')]
    elif t==norm('Ao entrar, olhe a carta do topo do grimório de seu oponente. você pode exilar ele sem o revelar e castar pagando seu custo em manas genericas.'):
        out=[ability(op('pca-secret-exile'),trigger='enter')]
    elif t==norm('Ao morrer, você pode colocar um Card do seu cemitério no topo do seu grimório.'):
        out=[ability(op('pcl-grave',999,1,value='any:top'),trigger='death',optional=True)]
    elif t==norm('1 PA, sacrifique outra criatura sua neste terreno: o jogador alvo tritura o valor da soma da defesa e ataque da criatura sacrificada.'):
        out=[ability(op('pca-sacrifice-mill'),trigger='activate',pa=1,sacrifice='other',target='player')]
    elif t==norm('Ao entrar, você pode perder 10 de vida da capital. Se fizer isso, cada oponente tritura 10.'):
        out=[ability(op('loseLife',10),op('pca-mill-enemies',10),trigger='enter',optional=True)]
    elif t==norm('Ao morrer, seus oponentes trituram 5.'):
        out=[ability(op('pca-mill-enemies',5),trigger='death')]
    elif t==norm('Uma vez por turno, quando outra criatura sua morrer, você pode perder 2 de vida da Capital. Se fizer isso, devolva uma criatura diferente de custo 2 ou menos do seu cemitério para o campo.'):
        # User decision: "No terreno do Senhor das Catacumbas".
        out=[ability(op('pca-catacombs-return'),trigger='pca-own-other-death',once=True)]
    elif t==norm('Quando entrar, escolha até duas outras criaturas no seu cemitério e devolva-as para sua mão.'):
        out=[ability(op('pca-grave-other',2),trigger='enter')]
    elif t==norm('Criaturas aliadas adjacentes recebem +1 de defesa.'):
        out=[ability(op('buff',b=1),trigger='aura',target='pca-allied',scope='adjacent')]
    elif t==norm('Criaturas aliadas a até 2 tiles recebem +1 de defesa.'):
        out=[ability(op('buff',b=1),trigger='aura',target='pca-allied',condition='pca-range-two')]
    elif t==norm('Pode atravessar criaturas durante seu movimento.'):
        traits=['pass-units'];out=[ability(op('pca-cross-pieces'),trigger='aura',target='self')]
    if out is None:return False
    c['abilities']=out;c['traits']=list(dict.fromkeys(c.get('traits',[])+traits))
    if equip is not None:c['equipCost']=equip
    if c.get('kind') in ['Spell','Instant']:c['rule']='abilities'
    return True
