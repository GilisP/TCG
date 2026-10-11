"""Whole-text compiler for approved preconstructed library effects. No runtime text parsing."""
import re, unicodedata

def norm(s):
    return ''.join(x for x in unicodedata.normalize('NFD',s.lower()) if not unicodedata.combining(x))
def op(k,a=0,b=0,c=0,value=''):
    return dict(kind=k,a=a,b=b,c=c,value=value)
def ability(*ops,**kw):
    return dict(dict(trigger='cast',target='owner',scope='any',maxCost=999,count=1,manaColor=-1,ops=list(ops)),**kw)

def compile_card(d,c):
    t=norm(d.get('Efeito',c.get('text',''))).strip(); out=None; traits=[]
    m=re.fullmatch(r'(\d+) pa, uma vez no seu turno: o jogador alvo tritura (\d+) cartas\.',t)
    if m:out=[ability(op('mill',int(m[2])),trigger='activate',target='player',pa=int(m[1]),once=True,gate='ownTurn')]
    if t=='o oponente alvo tritura dez cartas e depois compra uma carta.':out=[ability(op('pcl-enemy-mill-draw',10))]
    if t=='uma vez no seu turno, quando voce conjurar uma carta que pertencia a um oponente, cada outro oponente tritura tres cartas.':out=[ability(op('pcl-other-enemies-mill',3),trigger='pcl-foreign-cast',once=True,gate='ownTurn')]
    if t=='a primeira vez em cada turno que voce conjurar um truque, esta criatura recebe +0/+2 ate o fim do turno.':out=[ability(op('buff',b=2),trigger='pcl-first-instant',target='self')]
    m=re.fullmatch(r'uma criatura de custo total ate (\d+) nao pode iniciar movimentos nem ataques ate o fim do turno\. nao desfaz acoes que ja estao na pilha\.',t)
    if m:out=[ability(op('noMove'),op('noAttack'),target='creature',maxCost=int(m[1]))]
    m=re.fullmatch(r'ate (\d+) criaturas alvo recebem -(\d+) de ataque ate o fim do turno, ate o minimo de zero\.',t)
    if m:out=[ability(op('buff',-int(m[2])),target='creature',count=int(m[1]),optional=True)]
    if t=='uma criatura alvo perde 2 de ataque ate o fim do turno, ate o minimo de zero.':out=[ability(op('buff',-2),target='creature')]
    # Every branch consumes the complete editorial effect, including explanatory clauses.
    m=re.fullmatch(r'o jogador alvo tritura (\d+) cartas\. tritura[r] coloca cartas do topo do grimorio no cemiterio\.',t)
    if m: out=[ability(op('mill',int(m[1])),target='player')]
    m=re.fullmatch(r'o oponente alvo tritura (\d+) cartas e depois compra uma carta\. se ele tentar comprar do grimorio vazio, aplica-se a regra de derrota por compra impossivel\.',t)
    if m: out=[ability(op('pcl-enemy-mill-draw',int(m[1])))]
    m=re.fullmatch(r'ao entrar, um oponente alvo tritura (\d+) cartas\. voce pode exilar uma carta de criatura do cemiterio dele; se fizer isso, sua capital ganha (\d+) de vida\.',t)
    if m:out=[ability(op('pcl-enemy-mill-exile',int(m[1]),int(m[2])),trigger='enter')]
    m=re.fullmatch(r'ao causar dano de combate a uma capital, olhe as (\d+) cartas do topo do grimorio do dono\. coloque uma no cemiterio dele e devolva as demais na ordem escolhida\.',t)
    if m:out=[ability(op('pcl-capital-top-mill',int(m[1])),trigger='capitalHit')]
    m=re.fullmatch(r'exile uma carta nao terreno de custo total ate (\d+) de um cemiterio inimigo\. ate o fim deste turno, voce pode conjura-la pagando seu custo com mana de qualquer cor\. preserve o dono original\.',t)
    if m:out=[ability(op('pcl-exile-grave',int(m[1]),1,0))]
    m=re.fullmatch(r'ao entrar, exile uma carta nao terreno de custo total ate (\d+) de um cemiterio inimigo\. ate o fim do seu proximo turno, voce pode conjura-la pagando seu custo com mana de qualquer cor\. ela preserva o dono original\.',t)
    if m:out=[ability(op('pcl-exile-grave',int(m[1]),1,1),trigger='enter')]
    if t=='exile ate tres cartas nao terreno de cemiterios inimigos. ate o fim do seu proximo turno, voce pode conjura-las pagando seus custos com mana de qualquer cor; preserve os donos originais.':out=[ability(op('pcl-exile-grave',999,3,1))]
    m=re.fullmatch(r'compre (\d+) cartas e coloque uma carta da mao no fundo do grimorio\.',t)
    if m:out=[ability(op('draw',int(m[1])),op('bottomHand'))]
    if t=='compre duas cartas. depois coloque uma carta da sua mao no fundo do grimorio.':out=[ability(op('draw',2),op('bottomHand'))]
    m=re.fullmatch(r'devolva um truque ou feitico de custo total ate (\d+) do seu cemiterio a mao\. exile esta carta apos a resolucao\.',t)
    if m:out=[ability(op('pcl-grave',int(m[1]),1,value='magic:hand'))];traits=['exile-on-resolve']
    m=re.fullmatch(r'devolva uma criatura de custo total ate (\d+) a mao do dono\. se ela for comandante, o dono pode aplicar a regra de retorno a zona de comando\.',t)
    if m:out=[ability(op('pcl-bounce',int(m[1]),1))]
    m=re.fullmatch(r' olhe as (\d+) cartas do topo do seu grimorio\. voce pode revelar um truque ou feitico entre elas e coloca-lo na mao\. coloque as demais no fundo, na ordem escolhida\.', ' '+t)
    if m:out=[ability(op('pcl-select-top',int(m[1]),value='magic'))]
    m=re.fullmatch(r'olhe as (\d+) cartas do topo do seu grimorio\. voce pode revelar um feitico de custo total 5 ou maior e coloca-lo na mao\. coloque as demais no fundo em qualquer ordem\.',t)
    if m:out=[ability(op('pcl-select-top',int(m[1]),5,value='spell'))]
    m=re.fullmatch(r'anule um truque ou feitico na pilha de custo total ate (\d+)\.',t)
    if m:out=[ability(op('pcl-counter',int(m[1]),1))]
    if t=='anule ate dois truques ou feiticos na pilha.':out=[ability(op('pcl-counter',999,2))]
    if t=='devolva ate quatro criaturas nao comandantes as maos dos donos.':out=[ability(op('pcl-bounce',999,4,value='noncommander'))]
    if t=='devolva uma criatura que nao seja comandante a mao do dono. ele pode comprar uma carta.':out=[ability(op('pcl-bounce',999,1,1,'noncommander'))]
    # Recoveries across green/moon/equipment decks.
    m=re.fullmatch(r'(ao morrer, |ao entrar, )?(voce pode )?(?:devolva|devolver|colocar|coloque) (?:uma criatura|um truque) de custo total ate (\d+) (?:do seu|da sua) cemiterio (?:a mao|no fundo do grimorio)\.( sua capital perde 2 de vida\.)?',t)
    if m:
        trigger='death' if t.startswith('ao morrer') else 'enter' if t.startswith('ao entrar') else 'cast'
        out=[ability(op('pcl-grave',int(m[3]),1,value=('instant' if 'um truque' in t else 'creature')+(':bottom' if 'no fundo' in t else ':hand')),trigger=trigger,optional=bool(m[2]))]
        if m[4]:out[0]['ops'].append(op('loseLife',2))
    if t=='devolva ate tres equipamentos do seu cemiterio a mao.':out=[ability(op('pcl-grave',999,3,value='equipment:hand'))]
    m=re.fullmatch(r'ao morrer, olhe a carta do topo do seu grimorio\. voce pode coloca-la na mao; se fizer isso, perca (\d+) de vida da capital\.',t)
    if m:out=[ability(op('pcl-top-hand-life',int(m[1])),trigger='death')]
    if t=='coloque a carta do topo do seu grimorio no seu cemiterio. depois compre duas cartas.':out=[ability(op('millSelf',1),op('draw',2))]
    if t=='ao entrar, coloque as duas cartas do topo do seu grimorio no seu cemiterio.':out=[ability(op('millSelf',2),trigger='enter')]
    if t=='coloque as quatro cartas do topo do seu grimorio no cemiterio. depois cure 2 de vida da sua capital.':out=[ability(op('millSelf',4),op('gainLife',2))]
    if t=='olhe as duas cartas do topo do grimorio do jogador alvo. coloque uma no cemiterio dele e devolva a outra ao topo.':out=[ability(op('pcl-top-mill',2),target='player')]
    if t=='olhe as duas cartas do topo do seu grimorio. coloque uma no fundo e a outra no topo.':out=[ability(op('pcl-top-bottom',2))]
    m=re.fullmatch(r'(\d+) pa, uma vez no seu turno: olhe a carta do topo do seu grimorio\. voce pode coloca-la no seu cemiterio\.',t)
    if m:out=[ability(op('scry',value='mill'),trigger='activate',pa=int(m[1]),once=True,gate='ownTurn')]
    if t=='2 pa: o jogador alvo coloca duas cartas do topo do grimorio no cemiterio. use uma vez no seu turno.':out=[ability(op('mill',2),trigger='activate',target='player',pa=2,once=True,gate='ownTurn')]
    m=re.fullmatch(r'uma criatura sua recebe \+0/\+(\d+) ate o fim do turno\. se voce ja conjurou outra magia neste turno, olhe a carta do topo do seu grimorio; voce pode coloca-la no fundo\.',t)
    if m:out=[ability(op('buff',b=int(m[1])),op('pcl-scry-other-spell'),target='own')]
    if t=='a primeira vez no seu turno que voce conjurar um feitico, pode devolver um truque de custo total ate 2 do seu cemiterio a mao.':out=[ability(op('pcl-grave',2,1,value='instant:hand'),trigger='pcl-first-spell',gate='ownTurn',optional=True)]
    if t=='enquanto houver uma carta exilada que voce ainda possa conjurar, esta criatura recebe +0/+2.':out=[ability(op('buff',b=2),trigger='aura',target='self',condition='pcl-exile-permission')]
    if t=='uma vez no seu turno, quando uma carta sair de um cemiterio por um efeito seu, olhe a carta do topo do seu grimorio. voce pode coloca-la no fundo.':out=[ability(op('scry',value='bottom'),trigger='pcl-grave-leave',gate='ownTurn',once=True)]
    if t=='ao entrar, voce pode colocar um feitico da sua mao no fundo do grimorio. se fizer isso, compre uma carta.':out=[ability(op('pcl-hand-spell-bottom-draw'),trigger='enter')]
    m=re.fullmatch(r'uma vez no seu turno, pague (\d+) mana(?:s)? generica(?:s)?:\s*(.*)',t)
    if m:
        if m[2]=='olhe a carta do topo do grimorio, voce pode colocar a carta do topo do seu grimorio no seu cemiterio.':out=[ability(op('scry',value='mill'),trigger='activate',mana=int(m[1]),once=True,gate='ownTurn')]
        if m[2]=='coloque um truque do seu cemiterio no fundo do grimorio e compre uma carta.':out=[ability(op('pcl-grave-bottom-draw',999),trigger='activate',mana=int(m[1]),once=True,gate='ownTurn')]
    m=re.fullmatch(r'equipar: (\d+) mana generica no mesmo tile\. a criatura equipada recebe \+0/\+(\d+)\. ao equipar um mago, olhe a carta do topo do seu grimorio; voce pode coloca-la no fundo\.',t)
    equip=None
    if m:equip=int(m[1]);out=[ability(op('buff',b=int(m[2])),trigger='aura',target='host'),ability(op('scry',value='bottom'),trigger='pcl-equip-mage')]
    if t=='equipar: 1 mana generica, no mesmo tile. a criatura equipada recebe +0/+1. uma vez no seu turno, quando ela causar dano de combate a uma capital, o dono dessa capital tritura duas cartas.':equip=1;out=[ability(op('buff',b=1),trigger='aura',target='host'),ability(op('pcl-capital-mill',2),trigger='capitalHit',once=True,gate='ownTurn')]
    # Explicit terrain generation remains the standard engine color generation.
    if c.get('kind')=='Terrain' and '. uma vez no seu turno, pague ' in t:
        _,effect=t.split('. uma vez no seu turno, pague ',1);m=re.fullmatch(r'(\d+) manas genericas: (.*)',effect)
        if m:
            effect=m[2]; ops=None
            if effect in ['olhe a carta do topo do seu grimorio; voce pode coloca-la no cemiterio.','coloque a carta do topo do seu grimorio no seu cemiterio.']:ops=[op('scry',value='mill')] if effect.startswith('olhe') else [op('millSelf',1)]
            if effect=='olhe as duas cartas do topo do seu grimorio e devolva-as na ordem escolhida.':ops=[op('orderTop',2)]
            if effect.startswith('coloque um truque do seu cemiterio no fundo do grimorio'):ops=[op('pcl-grave',999,1,value='instant:bottom')]+([op('pcl-look')] if 'e olhe' in effect else [])
            if effect=='coloque uma criatura de custo total ate 1 do seu cemiterio no fundo do grimorio.':ops=[op('pcl-grave',1,1,value='creature:bottom')]
            if effect=='o oponente alvo tritura duas cartas.':ops=[op('pcl-enemy-mill',2)]
            if effect=='o jogador alvo tritura uma carta.':out=[ability(op('mill',1),trigger='activate',target='player',mana=int(m[1]),once=True,gate='ownTurn')]
            if effect in ['uma criatura sua neste tile recebe +0/+2 ate o fim do turno.','um mago seu neste tile recebe +0/+2 ate o fim do turno.']:out=[ability(op('buff',b=2),trigger='activate',target='own',scope='here',subtype='Mago' if effect.startswith('um mago') else '',mana=int(m[1]),once=True,gate='ownTurn')]
            if ops:out=[ability(*ops,trigger='activate',mana=int(m[1]),once=True,gate='ownTurn')]
    if out is None:return False
    c['abilities']=out
    if equip is not None:c['equipCost']=equip
    if c.get('kind') in ['Spell','Instant']:c['rule']='abilities'
    c['traits']=list(dict.fromkeys(c.get('traits',[])+traits))
    if c.get('kind')=='Terrain':
        prefix=t.split('.',1)[0]
        if prefix=='gera uma mana de cor aleatoria':c['traits'].append('precon-random-mana')
        elif 'incolor' in prefix:c['color']=6
        else:
            for name,index in [('sol',0),('lua',1),('agua',2),('ar',4),('fogo',3),('terra',5)]:
                if 'mana de '+name in prefix:c['color']=index
    return True
