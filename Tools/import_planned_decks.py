"""Compile the approved-for-playtest editorial lists into explicit engine data.
No text is interpreted during matches. Unknown source identities abort import.
"""
from pathlib import Path
import json,re
ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'Documentacao/Design/Decks-2026-10-01'
cards=json.loads((SOURCE/'catalogo-editorial.json').read_text(encoding='utf-8'))['cards']
decks=json.loads((SOURCE/'decks-editoriais.json').read_text(encoding='utf-8'))['decks']
used={e['id'] for d in decks for z in ['main','terrains'] for e in d[z]}|{d['commander'] for d in decks}
symbols='SLGFAT'
def op(kind,a=0,b=0,c=0,value='',condition=''):return dict(kind=kind,a=a,b=b,c=c,value=value,condition=condition)
def buff(a=0,b=0,c=0,value='',condition=''):return op('buff',a,b,c,value,condition)
def ability(*ops,trigger='cast',target='owner',scope='any',condition='',gate='',subtype='',mana=0,pa=0,manaColor=-1,colored=0,maxCost=999,count=1,once=False,optional=False):
 return dict(ops=list(ops),trigger=trigger,target=target,scope=scope,condition=condition,gate=gate,subtype=subtype,mana=mana,pa=pa,manaColor=manaColor,colored=colored,maxCost=maxCost,count=count,once=once,optional=optional)
def aura(*ops,target='self',condition='',scope='any',subtype=''):return ability(*ops,trigger='aura',target=target,condition=condition,scope=scope,subtype=subtype)
def bcast(a=0,b=0,c=0,**kw):return ability(buff(a,b,c),target='own',**kw)
def grave(cost=999,kind='creature',dest='hand'):return op('grave',cost,value=kind+':'+dest)

def colored_creature(col,n):
 group=(n-1)//3
 table={
 'S':[
  [aura(buff(b=2),condition='adjAlly')],
  [ability(op('gainLife',2),trigger='enter')],
  [ability(buff(b=2),trigger='enter',target='other',scope='here')],
  [ability(op('gainLife',1),trigger='allyHurtHere',once=True)],
  [ability(op('marker',1),trigger='combatDeath',target='other',scope='here')],
  [ability(buff(1,value='combat'),trigger='attack',target='self',condition='targetHasAllyAdjacent')],
  [ability(op('shield',1),trigger='defend',target='self',once=True)],
  [ability(op('pull'),trigger='enter',target='other',scope='adjacent',optional=True)]],
 'L':[
  [ability(op('loseLife',2),op('draw',1),trigger='enter',optional=True)],
  [ability(op('gainLife',2),trigger='death')],
  [ability(op('gainLife',1),trigger='combatHit')],
  [ability(op('sacrifice'),op('buffSource',2),trigger='activate',target='other',scope='here',pa=1,once=True)],
  [ability(grave(2,dest='top'),trigger='enter')],
  [ability(buff(2,value='combat'),trigger='attack',target='self',condition='attackWounded')],
  [ability(buff(1),trigger='allyDeathHere',target='self',gate='ownTurn',once=True)],
  [ability(op('mill',1),trigger='capitalHit',target='self')]],
 'G':[
  [ability(op('scry'),trigger='enter')],
  [ability(buff(b=2),trigger='enter',target='other',scope='here')],
  [ability(grave(kind='instant',dest='bottom'),trigger='death',optional=True)],
  [ability(buff(b=1),trigger='instant',target='self',once=True)],
  [ability(op('pull'),trigger='enter',target='other',scope='adjacent',optional=True)],
  [ability(buff(-1),trigger='activate',target='enemy',scope='adjacent',pa=1,once=True)],
  [ability(op('loot'),trigger='enter',optional=True)],
  [ability(op('orderTop',2),trigger='capitalHit')]],
 'F':[
  [ability(op('damage',1),trigger='enter',target='enemy',scope='adjacent')],
  [ability(op('damage',1),trigger='death',target='enemy',scope='here')],
  [ability(buff(1,value='combat'),trigger='attack',target='self')],
  [ability(op('damageSelf',1),trigger='capitalHit',target='self')],
  [ability(buff(1),trigger='enter',target='other',scope='here',subtype='Goblin')],
  [ability(buff(1),trigger='hurt',target='self',once=True)],
  [ability(op('sacrificeSource'),op('damage',2),trigger='activate',target='enemyBuilding',scope='adjacent',pa=1)],
  [ability(buff(2,value='combat'),trigger='attack',target='self',condition='attackBuilding')]],
 'A':[
  [ability(op('move',1),trigger='afterAttack',target='self',once=True,optional=True)],
  [aura(buff(c=1),condition='noEnemyAdjacent')],
  [ability(buff(value='move:1'),trigger='enter',target='other',scope='here')],
  [aura(buff(1),condition='alone')],
  [ability(buff(1,value='combat'),trigger='attack',target='self',condition='moved')],
  [ability(buff(c=1),trigger='activate',target='self',pa=1,once=True)],
  [ability(op('scry'),trigger='enter')],
  [ability(op('move',1),trigger='capitalHit',target='self',optional=True)]],
 'T':[
  [aura(buff(b=2),condition='still')],
  [ability(op('heal',2),trigger='enter',target='ownBuilding',scope='adjacent')],
  [aura(buff(b=2),condition='nearBuilding')],
  [aura(op('immobile',value='enemy'))],
  [ability(op('gainLife',2),trigger='combatDeath')],
  [ability(buff(b=2,value='next'),trigger='activate',target='self',pa=1,once=True)],
  [ability(buff(b=3),trigger='enter',target='other',scope='here')],
  [aura(buff(b=3),condition='capital')]]}
 return table[col][group]

def colored_support(col,n):
 table={
 'S':[[bcast(b=4,condition='adjAlly')],[ability(op('gainLife',4))],[ability(op('move',1),target='own',count=2)],[aura(buff(b=3),target='host')],[aura(buff(b=2),target='host'),aura(buff(1),target='host',subtype='Cavaleiro')],[aura(buff(1),target='host'),aura(buff(b=2),target='host',condition='adjAlly')]],
 'L':[[ability(op('sacrifice'),op('draw',2),op('loseLife',2),target='own')],[ability(grave(4))],[ability(buff(2),op('gainLife',2),target='own')],[aura(buff(b=-1),target='host'),ability(op('gainLife',2),trigger='hostDeath')],[aura(buff(1),target='host'),ability(op('mill',2),trigger='capitalHit',once=True)],[ability(op('mill',3),op('loseLife',1),target='player')]],
 'G':[[ability(op('move',1),target='own')],[ability(op('selectTop',3))],[ability(buff(-3),target='creature')],[aura(buff(b=3),target='host')],[aura(buff(b=2),target='host'),ability(op('move',1),trigger='combatHit',target='subject',optional=True)],[ability(grave(kind='instant'))]],
 'F':[[ability(op('damage',2),target='enemyBody')],[ability(op('damage',2),buff(1),target='enemy')],[bcast(a=2,count=2)],[aura(buff(2,-1),target='host')],[aura(buff(2),target='host'),ability(op('damage',1),trigger='hostAfterAttack',target='host')],[ability(op('damage',5),target='enemyBuilding')]],
 'A':[[ability(op('move',1),buff(1),target='own')],[ability(op('orderTop',3),op('draw',1))],[bcast(c=2,condition='range2')],[aura(op('keyword',value='flying'),target='host')],[aura(buff(value='move:1'),target='host')],[ability(op('keyword',value='flying'),target='own')]],
 'T':[[bcast(b=4)],[aura(buff(b=3,value='move:-1'),target='host')],[ability(op('heal',1000),target='ownBuilding')],[aura(buff(2,1,value='move:-1'),target='host')],[ability(grave(3,dest='bottom'),op('draw',2))],[ability(buff(b=3),target='groupOwn',count=2)]]}
 return table[col][n-25]

def neutral(n):
 if n<=30:
  return [[],[aura(buff(b=2),condition='capital')],[ability(op('gainLife',1),trigger='enter')],[ability(op('scry'),trigger='death')],[ability(op('move',1),trigger='enter',target='other',scope='here',optional=True)],[aura(buff(b=1),condition='nearBuilding')],[ability(buff(b=1),trigger='enter',target='own',scope='here')],[aura(op('immobile',value='own'))],[ability(buff(1,value='combat'),trigger='attack',target='self'),ability(op('damageSelf',1),trigger='afterAttack',target='self')],[ability(op('scry'),trigger='activate',pa=1,once=True)]][(n-1)//3]
 if n<=50:
  table=[
   [aura(buff(1),target='host')],[aura(buff(2),target='host')],[aura(buff(b=2),target='host')],[aura(buff(b=4,value='move:-1'),target='host')],
   [aura(buff(1,2),target='host')],[aura(buff(b=5,value='pa:-1'),target='host')],[aura(buff(1,c=1),target='host')],[aura(buff(value='move:1'),target='host')],
   [aura(buff(1,1),target='host')],[aura(buff(b=3),target='host')],[aura(buff(3,-1),target='host')],[aura(buff(2,2,value='move:-1'),target='host')],
   [aura(buff(b=1),target='host'),aura(buff(b=2),target='host',condition='capital')],[aura(buff(1),target='host'),aura(buff(1),target='host',condition='moved')],
   [aura(buff(b=1),target='host'),aura(buff(c=1),target='host',condition='still')],[aura(buff(1),target='host'),ability(op('gainLife',1),trigger='enter')],
   [aura(buff(b=1,value='move:1'),target='host')],[aura(buff(4,value='move:-1'),target='host')],[aura(buff(b=1),target='host'),aura(buff(1),target='host',condition='adjAlly')],[aura(buff(1,3),target='host')]]
  return table[n-31]
 if n<=65:
  return [
   [aura(buff(b=1),target='own',scope='here')],
   [ability(op('gainLife',1),trigger='turn',gate='terrainCreature')],
   [ability(op('heal',2),trigger='activate',target='ownBuilding',scope='here',mana=2,once=True)],
   [ability(op('gainLife',3),trigger='enter')],
   [aura(buff(b=2),target='own',scope='here')],
   [ability(buff(b=1),trigger='allyEnterHere',target='subject',once=True)],
   [aura(buff(c=1),target='own',scope='here',condition='range2')],
   [ability(op('scry'),trigger='turn')],
   [ability(op('loot'),trigger='activate',mana=3,once=True)],
   [aura(op('equipDiscount',1))],
   [aura(buff(value='move:-1'),target='own',scope='here')],
   [ability(buff(value='move:1'),trigger='activate',target='own',scope='here',mana=2,once=True)],
   [ability(op('mana',2,6),trigger='activate',mana=3,once=True)],
   [ability(op('draw',1),trigger='death')],
   [aura(buff(b=3),target='own',scope='here',condition='capital')]][n-51]
 if n<=75:return [] # Explicit vehicle fields below.
 if n<=90:
  return [
   [ability(op('draw',1),op('gainLife',1))],[ability(op('orderTop',3))],[ability(op('heal',4),target='ownBuilding')],[bcast(b=2)],
   [ability(buff(1,1),op('draw',1),target='own')],[ability(op('loot'))],[ability(op('gainLife',4))],[ability(op('move',1),target='own')],
   [ability(buff(b=2),target='groupOwn',count=2)],[bcast(b=4,condition='capital')],[ability(op('draw',2),op('bottomHand'))],
   [ability(op('mana',2,6))],[ability(grave(2,'equipment'))],[ability(op('heal',2),target='groupOwn',count=2)],[bcast(c=1)]][n-76]
 return [
  [aura(buff(b=2),target='host')],[aura(buff(b=3),target='host',condition='still')],[aura(buff(value='move:1'),target='host')],
  [aura(buff(b=2),target='host',condition='adjAlly')],[aura(buff(b=2,value='move:-1'),target='host')],[aura(buff(1,2),target='host')],
  [ability(op('gainLife',2),trigger='hostDeath')],[aura(buff(2,value='pa:-1'),target='host')],[aura(buff(1,3),target='host',condition='capital')],
  [ability(grave(2,'equipment','bottom'),trigger='hostDeath',optional=True)]][n-91]

def land(col,n):
 i=(n-1)%10;gate=['terrainCreature','terrainBuilding','terrainCapital','terrainTwo','terrainEntered'][(n-1)//10]
 def local(*ops,target='own',subtype='',condition=''):return ability(*ops,trigger='activate',target=target,scope='here' if target!='owner' else 'any',subtype=subtype,condition=condition,gate=gate,mana=2,once=True)
 data={
 'S':[local(buff(b=2)),local(op('gainLife',1),target='owner'),local(buff(1,1),subtype='Cavaleiro'),local(buff(1),condition='adjAlly'),local(op('shield',1)),local(buff(b=3),op('noMove')),local(op('orderTop',1),target='owner'),local(op('heal',2)),local(buff(1)),local(op('heal',1),target='ownBuilding')],
 'L':[local(op('loseLife',1),buff(2)),local(op('millSelf',1),target='owner'),local(op('gainLife',1),target='owner'),local(buff(1)),local(op('scry',value='mill'),target='owner'),local(grave(1,dest='bottom'),target='owner'),local(buff(2,-1)),local(buff(-1),target='enemy',condition='wounded'),local(grave(kind='instant',dest='bottom'),target='owner'),local(op('heal',1))],
 'G':[local(op('scry'),target='owner'),local(buff(-1),target='creature'),local(op('heal',1)),local(buff(b=2)),local(op('orderTop',2),target='owner'),local(op('shield',1)),local(buff(b=3),subtype='Mago'),local(buff(b=2),target='ownVehicle'),local(grave(kind='spell',dest='bottom'),target='owner'),local(buff(value='move:1'))],
 'F':[local(buff(1)),local(op('damage',1),target='enemyBuilding'),local(buff(2),subtype='Goblin'),local(buff(2),op('damage',1)),local(op('damage',1),target='enemy',condition='wounded'),local(buff(3,-2)),local(op('orderTop',1),target='owner'),local(buff(2,condition='attacked')),local(op('damage',1),op('loseLife',1),target='enemy'),local(buff(value='move:1'))],
 'A':[local(buff(c=1)),local(buff(value='move:1')),local(op('scry'),target='owner'),local(buff(1),subtype='Arqueiro'),local(buff(1,condition='moved')),local(buff(b=1)),local(op('orderTop',2),target='owner'),local(buff(-1,c=2)),local(buff(b=-1,value='move:2')),local(buff(1,1),subtype='Pirata')],
 'T':[local(buff(b=2)),local(op('heal',2),target='ownBuilding'),local(buff(b=3,value='move:-1')),local(op('gainLife',1),target='owner'),local(buff(1,1,condition='still')),local(op('immune')),local(op('orderTop',1),target='owner'),local(op('heal',2)),local(buff(b=4),op('noAttack')),local(grave(dest='bottom'),target='owner')],
 'N':[local(op('heal',1)),local(op('heal',1),target='ownBuilding'),local(buff(b=1)),local(op('orderTop',1),target='owner'),local(op('gainLife',1),target='owner'),local(buff(1)),local(buff(b=1),target='ownVehicle'),local(buff(b=1),condition='equipped'),local(buff(-1,value='move:1')),local(grave(kind='equipment',dest='bottom'),target='owner')]}
 return [data[col][i]]

def old(n):
 vanilla=[1,2,12,13,23,24,34,35,45,46,49,66,67,68,70,71,72,83]
 if n in vanilla or n in [3,14,25,36,47,58,69]:return []
 data={4:[bcast(b=2)],5:[aura(buff(b=1),condition='sameOther')],6:[ability(op('shield',3),target='own',condition='sameOther')],7:[ability(op('move',1,value='own'),target='own')],8:[aura(buff(1),condition='sameOther')],9:[ability(buff(b=2),target='groupOwn',count=121)],10:[ability(grave(2),trigger='enter')],11:[aura(buff(b=3),condition='sameOther')],15:[ability(grave(3))],16:[ability(op('sacrifice'),op('draw',1),trigger='activate',target='other',scope='here',pa=1,mana=1,manaColor=1,colored=1)],17:[ability(buff(-2),target='enemy')],18:[ability(op('sacrifice'),op('draw',2),target='own')],19:[aura(buff(1),condition='grave3')],20:[ability(grave(),grave(),optional=True)],21:[ability(grave(4),trigger='enter')],22:[aura(buff(b=3),condition='grave5')],26:[ability(op('bounce'),target='enemy')],27:[ability(op('scry'),trigger='enter')],28:[ability(buff(-1),target='enemy')],29:[ability(op('draw',2))],30:[ability(op('bounce'),trigger='activate',target='enemy',scope='adjacent',pa=1,mana=2,manaColor=2,colored=1)],31:[ability(op('move',1,value='own'),target='own')],32:[ability(op('draw',1),trigger='enter')],33:[ability(op('bounce'),trigger='enter',target='enemy',maxCost=3)],37:[ability(op('damage',2),target='enemy')],38:[ability(buff(1,value='combat'),trigger='attack',target='self')],39:[bcast(a=2)],40:[ability(op('damage',2),target='enemy')],41:[ability(buff(1,value='combat'),trigger='attack',target='self',condition='attackCapital')],42:[ability(op('damage',2),target='groupAny',count=121)],43:[ability(buff(2,value='combat'),trigger='attack',target='self')],44:[ability(op('damage',2),trigger='enter',target='enemy')],48:[ability(op('move',1,value='own'),target='own')],50:[ability(op('move',1,value='own'),target='own')],51:[ability(buff(value='move:2'),target='own')],53:[ability(op('groupMove',1),target='groupOwn',count=2)],59:[bcast(b=2)],61:[ability(op('drawTerrain'))],62:[ability(op('shield',3),target='own')],64:[ability(buff(b=3),target='groupOwn',count=121)],76:[ability(buff(2,value='move:1'),target='own')],78:[ability(grave(),op('scry'))],81:[aura(buff(b=1),condition='grave3')],82:[ability(grave(),op('drawTerrain'))],84:[ability(op('move',1,value='own'),op('scry'),target='own')]}
 assert n in data,('old not mapped',n)
 return data[n]

existing={}
for p in (ROOT/'Assets/StreamingAssets/Expansions').glob('*.json'):
 if p.name=='planned-decks-20261001.json':continue
 for c in json.loads(p.read_text(encoding='utf-8-sig'))['cards']:existing[c['id']]=c
out=[]
for c in cards:
 id=c['id']
 if id in existing or (id not in used and not id.startswith('PD26-')):continue
 kind={'Criatura':'Creature','Terreno':'Terrain','Feitiço':'Spell','Truque':'Instant','Construção':'Construction','Equipamento':'Equipment','Artefato':'Artifact','Encantamento':'Enchantment'}[c['kind']]
 cost=c['cost'];colored=[cost.count(s) for s in symbols]+[0]
 d=dict(id=id,name=c['name'],kind=kind,text=c['text'],rarity='prototype',art='soldier',rule='',traits=[],keywords=[],subtypes=c['subtypes'],commander=c['commander'],identityColors=c['colors'],maxCopies=1,cost=sum(int(n) for n in re.findall(r'\d+',cost)),coloredCost=colored,color=c['colors'][0] if c['colors'] else 6,attack=c['attack'] or 0,defense=c['defense'] or 1,actions=c['pa'] if c['pa'] is not None else 0,movement=c['movement'] if c['movement'] is not None else 0,range=c['range'] or 1,equipCost=1,abilities=[],effects=[])
 if id=='IDEIA-004':
  d['text']='Suas criaturas que causam dano a capitais fazem o dono da capital colocar cartas do topo do grimório no cemitério em quantidade igual ao dano. 2 PA, uma vez no seu turno: escolha uma carta de qualquer cemitério que tenha vindo diretamente do grimório e coloque-a na sua mão.'
  d['abilities']=[ability(op('pirateRecover'),trigger='activate',pa=2,once=True),ability(op('millDamage'),trigger='armyCapitalHit')]
 elif id.startswith('MED-'):
  n=int(id[4:]);d['abilities']=old(n)
  # Basic terrain exception is preserved even though these lists use one copy.
  if kind=='Terrain':d['maxCopies']=50
  if kind=='Creature' and not d['subtypes']:
   # Explicit editorial additions for this playtest, never runtime name inference.
   proposed_types={1:['Humano','Soldado'],2:['Humano','Soldado'],5:['Humano','Soldado'],8:['Humano','Cavaleiro'],10:['Humano','Cavaleiro'],11:['Espírito','Guardião'],12:['Humano','Soldado'],13:['Humano','Ladino'],16:['Humano','Clérigo'],19:['Espírito','Guerreiro'],21:['Humano','Mago'],22:['Esqueleto','Soldado'],23:['Humano','Soldado'],24:['Humano','Soldado'],27:['Humano','Mago'],30:['Humano','Mago'],32:['Humano','Mago'],33:['Serpente'],34:['Humano','Soldado'],35:['Humano','Guerreiro'],38:['Humano','Guerreiro'],41:['Construto'],43:['Humano','Nobre'],44:['Dragão'],45:['Humano','Batedor'],46:['Humano','Batedor'],49:['Humano','Arqueiro'],66:['Gigante'],67:['Humano','Mercenário'],68:['Construto'],70:['Humano','Batedor'],71:['Autômato','Construto'],72:['Humano','Cavaleiro'],81:['Esqueleto','Guardião'],83:['Humano','Batedor']}
   d['subtypes']=proposed_types[n]
 elif id.startswith('PD26-L') and len(id.split('-')[1])==2:
  tag=id.split('-')[1][1];d['abilities']=land(tag,int(id.split('-')[2]))
 else:
  _,col,num=id.split('-');n=int(num)
  d['abilities']=neutral(n) if col=='N' else colored_creature(col,n) if n<=24 else colored_support(col,n)
  if col=='A' and n<=24 and (n-1)//3 in [4,7]:d['keywords']=['flying']
  if col=='N' and 66<=n<=75:
   attrs=re.search(r'Veículo; (\d+)/(\d+), (\d+) PA, movimento (\d+), alcance (\d+). Capacidade (\d+); tripulação mínima (\d+)',c['text']);assert attrs
   d.update(dict(zip(['attack','defense','actions','movement','range','vehicleSeats','vehicleCrew'],map(int,attrs.groups()))))
 if id=='MED-018':d['abilities']=[ability(op('draw',2))];d['abilities'][0]['sacrifice']='own'
 if id=='MED-016':d['abilities']=[ability(op('draw',1),trigger='activate',pa=1,mana=1,manaColor=1,colored=1)];d['abilities'][0]['sacrifice']='other'
 if id in ['PD26-L-010','PD26-L-011','PD26-L-012']:d['abilities']=[ability(buff(2),trigger='activate',target='self',pa=1,once=True)];d['abilities'][0]['sacrifice']='other'
 if id in ['PD26-F-019','PD26-F-020','PD26-F-021']:d['abilities']=[ability(op('damage',2),trigger='activate',target='enemyBuilding',scope='adjacent',pa=1)];d['abilities'][0]['sacrifice']='self'
 if d['abilities']:d['rule']='abilities'
 if kind=='Spell' and id=='PD26-G-030':d['traits']=['exile-on-resolve']
 if kind=='Terrain':d['art']='terrain'
 elif 'Goblin' in d['subtypes']:d['art']='goblin'
 elif 'Mago' in d['subtypes']:d['art']='mage'
 elif 'Arqueiro' in d['subtypes']:d['art']='archer'
 elif kind=='Construction':d['art']='building'
 elif kind in ['Equipment','Artifact']:d['art']='artifact'
 elif kind in ['Spell','Instant','Enchantment']:d['art']='spell'
 out.append(d)

allcards={**existing,**{c['id']:c for c in out}}
assert len({c['name'].casefold() for c in allcards.values()})==len(allcards),'Duplicate names in combined catalog'
dest=ROOT/'Assets/StreamingAssets/Expansions/planned-decks-20261001.json'
dest.write_text(json.dumps(dict(schemaVersion=1,id='planned-decks-20261001',title='Decks planejados · protótipos',version='0.1.0',status='playtest',cards=out,printings=[]),ensure_ascii=False,indent=2),encoding='utf-8')
templates=[]
for d in decks:
 for z in ['main','terrains']:
  assert len(d[z])==(100 if z=='main' else 50)
  assert len({e['id'] for e in d[z]})==len(d[z])
  assert all(e['id'] in allcards for e in d[z])
 templates.append(dict(id=d['id'],name=d['name'],commander=d['commander'],description=d['plan'],experimental=False,main=[e['id'] for e in d['main']],terrains=[e['id'] for e in d['terrains']]))
deckdir=ROOT/'Assets/StreamingAssets/Decks';deckdir.mkdir(exist_ok=True)
(deckdir/'planned-decks.json').write_text(json.dumps(dict(version=1,decks=templates),ensure_ascii=False,indent=2),encoding='utf-8')
print(f'Compiled {len(out)} cards and {len(templates)} singleton decks; preserved {len(existing)} existing definitions.')
