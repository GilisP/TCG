"""Complete, bounded translations for approved COL-001 unit effect families.
The editorial family table is used only by the offline compiler; runtime consumes ops.
Unknown text fails atomically and is never converted to a vanilla card.
"""
import copy,json,re,unicodedata
from pathlib import Path
_FAMILIES=json.loads((Path(__file__).resolve().parents[1]/'Documentacao/Design/COL-001-20261009/pending-effect-families.json').read_text(encoding='utf-8-sig'))
def norm(s):return ''.join(c for c in unicodedata.normalize('NFD',s.casefold()) if not unicodedata.combining(c))
def family(s):return re.sub(r'\d+','#',norm(s)).strip()
_INDEX={family(x['text']):i for i,x in enumerate(_FAMILIES)}
def op(k,a=0,b=0,c=0,value='',condition=''):return dict(kind=k,a=a,b=b,c=c,value=value,condition=condition)
def ab(*ops,**kw):return dict(dict(trigger='cast',target='owner',scope='any',manaColor=-1,maxCost=999,count=1,ops=list(ops)),**kw)
def compile_card(d,c):
 t=norm(d.get('Efeito',c.get('text',''))).strip(); i=_INDEX.get(family(t)); out=copy.deepcopy(c); aa=[]
 if i is None:return False
 ns=[int(x) for x in re.findall(r'\d+',t)]
 def n(j,default=0):return ns[j] if len(ns)>j else default
 def add(*ops,**kw):aa.append(ab(*ops,**kw))
 def buff(a=0,b=0,m=0,r=0,value=''):return op('buff',a,b,r,value or ('move:'+str(m) if m else ''))
 def pair():
  m=re.search(r'([+-]\d+)/([+-]\d+)',t);return (int(m[1]),int(m[2])) if m else (0,0)
 def attack():
  m=re.search(r'([+-]\d+) de ataque',t);return int(m[1]) if m else 0
 def movement():
  m=re.search(r'([+-]\d+) de movimento',t);return int(m[1]) if m else 0
 def rangebonus():
  m=re.search(r'([+-]\d+) de alcance',t);return int(m[1]) if m else 0
 def activation():
  prefix=t.split(':',1)[0];p=re.search(r'(\d+) pa',prefix);m=re.search(r'pague (\d+) mana',t) or re.search(r'e (\d+) manas genericas',t)
  return dict(trigger='activate',once=True,gate='ownTurn',pa=int(p[1]) if p else 0,mana=int(m[1]) if m else 0)
 def token(count=1,where='here'):
  m=re.search(r'(goblin|soldado|esqueleto|planta|elementa(?:l|is)) (\d+)/(\d+)(?: sem habilidades)?, (\d+) pa, movimento (\d+) e alcance (\d+)',t)
  if not m:raise ValueError(('token profile',c['id'],t))
  typ={'goblin':'Goblin','soldado':'Soldado','esqueleto':'Esqueleto','planta':'Planta','elemental':'Elemental','elementais':'Elemental'}[m[1]]
  return op('pcu-token',count,int(m[2]),int(m[3]),'|'.join([typ,m[4],m[5],m[6],where]))
 def recipient():
  return 'ownVehicle' if 'veiculo seu' in t else 'pcu-own-body' if 'criatura ou construcao' in t or 'criaturas ou construcoes' in t else 'own'
 def condition():
  if 'embarcada em um veiculo' in t:return 'pcu-boarded'
  if 'ficha sua equipada' in t or 'fichas suas equipadas' in t:return 'pcu-token-equipped'
  if 'ficha' in t and 'base 1/1' in t:return 'pcu-token-1-1'
  if 'ficha' in t:return 'pcu-token'
  if 'defesa maior' in t:return 'pcu-defense-greater'
  if 'ataque base 0' in t:return 'pcu-base-zero'
  if 'ataque base 1' in t:return 'pcu-base-one'
  if 'ataque base 2 ou menor' in t:return 'pcu-base-at-most-2'
  if 'alcance 2 ou maior' in t:return 'range2'
  if 'nao houver inimigo adjacente' in t:return 'noEnemyAdjacent'
  if 'outra criatura sua adjacente' in t:return 'adjAlly'
  return ''
 def scope():return 'adjacent' if 'adjacente' in t and 'neste tile ou' not in t and 'mesmo tile ou' not in t else 'hereOrAdjacent' if 'tile ou' in t else 'here' if 'neste tile' in t or 'mesmo tile' in t else 'any'
 def subtype():return 'Goblin' if 'goblin seu' in t or 'goblins seus' in t else 'Arqueiro' if 'arqueiro seu' in t or 'arqueiros seus' in t else 'Mago' if 'mago seu' in t else ''
 def count():
  m=re.search(r'ate (\d+|duas|tres|quatro|seis)',t);return int(m[1]) if m and m[1].isdigit() else {'duas':2,'tres':3,'quatro':4,'seis':6}.get(m[1],1) if m else 1
 # Generic complete effects, never substring-only acceptance.
 simple={51,66,79,85,89,91,95,101,102,103,105,106,107,108,109,110,112,116,119,122,123,124,125,126,127,128,129,130,134,135,143,151,164,166,174,175,176,177,178,179,180,181,187,192,193,199,200,201,209,216,223,225,226,227,228,229,230,234,241,247,251,252,253,267,270,271,274,276,277,278,279,280,281,282,283}
 if i in simple:
  a,b=pair(); a=a or attack(); mb=movement(); target=recipient();cond=condition();kw=dict(target=target,scope=scope(),condition=cond,subtype=subtype(),count=count())
  if i in {89,91,95,110,112,119,192,216}:kw['target']='other'
  if i==116:kw['condition']='targetHasAllyAdjacent'
  if i==85:b=-n(1)
  if i==66:a=-n(0)
  if i==135:kw['target']='pcu-commander'
  if i in {134,143}:kw['sacrifice']='own';kw['target']='pcu-own-other-than-subject' if i==134 else 'enemy'
  if i in {51,79,95,102,103,105,106,107,108,109,119,123,124,125,126,127,128,129,130,151,174,175,176,177,178,179,180,181,192,199,200,201,223,225,226,227,228,229,230,241,247,251,252,253,271,276,277,278,279,280,281,282,283}:kw.update(activation())
  elif t.startswith('ao entrar'):kw['trigger']='enter'
  elif i==216:kw['trigger']='combatDeath'
  elif t.startswith('ao morrer'):kw['trigger']='death'
  elif i in {89,110}:kw['trigger']='attack' if i==89 else 'pcu-combat-enter'
  heal=re.search(r'cure (\d+) de dano',t)
  add(op('heal',int(heal[1])) if heal else buff(a,b,mb,value='next' if i in {177,178,179,180,181} else ''),**kw)
 elif i in {165,170}:
  add(buff(b=n(-1)),trigger='aura',target='pcu-allied-creature',scope='adjacent' if i==165 else 'any',condition='pcu-radius-2' if i==170 else '')
 elif i==24:add(buff(*pair()),trigger='aura',target='self',condition='pcl-exile-permission')
 elif i in {86,219}:
  add(buff(n(0),value='combat'),trigger='attack',target='self',gate='attackCapital' if i==86 else 'pcu-attack-stronger')
 elif i in {90,111,115,131,158,191,194,217,220,269}:
  a,b=pair();a=a or attack(); add(buff(a,b),trigger='aura',target='pcu-commander' if i==131 else 'self' if i in {90,111,115,194} else 'own',scope=scope(),condition='pcu-other-goblin-here' if i==90 else 'adjAlly' if i in {111,115} else condition())
 elif i in {87,92,104}:
  damage=re.search(r'cause (\d+) de dano',t);kw=dict(target='enemy' if i==104 else 'creature',scope='adjacent' if i!=92 else 'any',sacrifice='other' if i==87 else 'own')
  if i!=92:kw.update(activation())
  kw['gate']='pcu-sacrifice-goblin' if i in {87,92} else 'ownTurn';add(op('damage',int(damage[1])),**kw)
 elif i in {88,94}:
  add(op('damage',n(0)),trigger='death' if i==88 else 'combatDeath',target='enemy' if i==88 else 'subject',scope='hereOrAdjacent' if i==88 else 'any')
 elif i in {96,97,98,99,118,121,138,141,142,145,148,152,153,154,155,156,169,173,215}:
  num=4 if i in {99,121,148} else 2 if i in {97,138,152,153,154,155,156,173} else n(0) if i==169 else 1
  where='choose' if i in {97,99,121,148,169,173} else 'here';kw={}
  if i in {96,118}:kw['trigger']='enter'
  if i in {141,142,215}:kw['trigger']='death'
  if i in {98,138,145,152,153,154,155,156}:kw.update(activation())
  if i in {152,153,154,155,156}:kw['sacrifice']='own';kw['gate']='pcu-sacrifice-here'
  ops=[token(num,where)]
  if i==138:ops.insert(0,op('sacrificeSource'))
  add(*ops,**kw)
 elif i in {100,120,140,147,196}:
  if i==100:add(token(1,'capital'),trigger='pcu-goblin-army',once=True,gate='ownTurn')
  if i==120:add(buff(n(0)),trigger='pcu-lone-entry',target='subject',once=True,gate='ownTurn')
  if i==140:add(buff(*pair()),trigger='pcu-sacrifice-other',target='self',once=True)
  if i==147:add(token(1,'subject'),trigger='pcu-sacrifice-nontoken',once=True)
  if i==196:add(op('pcu-free-move'),trigger='pcu-archer-shot',target='subject',optional=True,once=True,gate='ownTurn')
 elif i in {113,114,212}:
  add(op('pcu-free-move'),target='other' if i==113 else 'own',scope='here' if i==113 else 'any',condition='pcu-token-capital' if i==212 else '',count=2 if i==212 else 1,**(activation() if i==113 else {}))
 elif i in {157,198,232,235}:
  if i==157:add(op('noAttack'),trigger='aura',target='self')
  elif i==198:out['traits']=list(set(out.get('traits',[])+['pass-units']))
  else:out['keywords']=list(set(out.get('keywords',[])+['flying']))
 elif i in {160,163}:
  add(op('heal',n(0) if i==160 else n(1)),target='pcu-own-body' if i==160 else 'ownBuilding',scope='hereOrAdjacent',**(dict(trigger='enter') if i==160 else activation()))
 elif i==161:add(op('pcu-heal-pool',n(0),2))
 elif i in {167,171,172}:
  if i==172:add(op('pcu-heal-all'),op('gainLife',n(0)),op('draw',2))
  elif i==167:add(op('heal',1000),buff(n(1)),target='own',count=n(0))
  else:add(buff(*pair()),op('pcu-allow-attack'),target='own',count=4)
 elif i==168:add(op('pcu-fight',n(0)))
 elif i==188:add(op('pcu-declared-volley',n(1),n(0)))
 elif i in {182,183,184,185,186,189,190,195,197,202,203,204,205,206}:
  if i in {182,183,184,185,189}:
   damage=re.search(r'cause (\d+) de dano',t);add(op('pcu-shot',int(damage[1]),2 if i==184 else 0,1 if i==183 else 0,'noAttack' if i==185 else ''),target='owner',**activation())
   if i==189:aa[-1]['gate']='pcu-own-still'
  else:
   damage=re.search(r'causa (\d+) de dano',t);add(op('pcu-shot',int(damage[1]),0,1 if i==195 else 0),target='own',subtype='Arqueiro' if i!=186 else '',condition='range2' if i==186 else '',count=count(),scope='here' if i in {202,203,204,205,206} else 'any',**(activation() if i in {202,203,204,205,206} else {}))
 elif i in {207,208,162,137}:
  add(buff(*pair()),trigger='aura',target='host')
  if i==207:add(buff(n(4),n(5)),trigger='aura',target='host',condition='pcu-token-1-1')
  if i==162:add(op('heal',n(2)),trigger='turn',target='host',condition='pcu-still-since-own')
  if i==137:add(op('gainLife',n(2)),trigger='hostDeath')
 elif i==211:add(op('gainLife',n(0)),sacrifice='own')
 elif i==146:add(op('pcu-sacrifice-reward',3,n(0)))
 elif i==149:
  add(op('gainLife',n(2)),sacrifice='own',**activation());aa[-1]['gate']='pcu-sacrifice-here'
 elif i==144:add(op('draw',1),sacrifice='own',**activation());aa[-1]['gate']='pcu-sacrifice-here'
 elif i==221:add(buff(*pair()),op('pcu-draw-on-combat-death'),target='own',count=3)
 elif i in {231,236,237,238,239,243,244,245,246,248}:
  if i==231:add(buff(*pair()),trigger='aura',target='pcu-carrier')
  elif i==248:add(buff(*pair()),trigger='aura',target='ownVehicle',condition='pcu-passengers-3')
  else:
   out['vehicleSeats']=n(0);out['vehicleCrew']=n(1)
   if i==246:out['keywords']=list(set(out.get('keywords',[])+['flying']))
   if i in {238,243,245}:add(op('pcu-passenger-bonus',n(2),n(3) if i==245 else 0,1 if i==245 else 0,'defense' if i==243 else 'pair' if i==245 else 'attack'),trigger='aura',target='self')
   if i in {236,239}:add(buff(r=n(2)) if i==236 else buff(m=n(2)),trigger='aura',target='self',condition='pcu-passengers-2')
   if i==244:add(op('pcu-heal-passengers',n(2)),trigger='turn',target='self')
 elif i==233:add(op('pcu-disembark',2),target='ownVehicle')
 elif i==242:add(buff(*pair()),trigger='pcu-unboard',target='other',scope='here')
 elif i==249:add(op('heal',1000),op('pcu-buff-passengers',*pair()),target='ownVehicle',count=2)
 elif i==250:add(op('pcu-passenger-temp',n(0),0,0),buff(m=n(1)),target='ownVehicle',count=4)
 elif i in {254,255,256,257,258}:add(op('pcu-passenger-temp',0,n(2)),target='ownVehicle',scope='here',**activation())
 elif i in {93,117,259,260,261,262,263,265}:
  out['equipCost']=n(0);add(buff(*pair(),m=movement(),r=rangebonus()) if pair()!=(0,0) else buff(attack(),m=movement(),r=rangebonus()),trigger='aura',target='host',condition='adjAlly' if i==117 else '')
  if i==93:
   aa[0]['ops']=[buff(attack())]
   add(buff(m=n(2)),trigger='aura',target='host',subtype='Goblin')
  if i==263:add(buff(n(3)),trigger='aura',target='host',condition='pcu-token')
  if i==265:add(buff(n(3),value='combat'),trigger='pcu-host-defend',target='host')
 elif i in {266,268}:add(op('pcu-return-equipment',n(0) if i==266 else 1))
 elif i in {35,67,132,133,136,139,150,159,210,213,214,218,222,224,240,264,272,273,275}:
  if i in {35,67,264,272}:
   out['equipCost']=n(0)
   if i!=264:add(buff(*pair()),trigger='aura',target='host')
   if i==35:add(op('mill',2),trigger='pcu-host-capital-hit',once=True,gate='ownTurn')
   elif i==67:add(op('scry',value='bottom'),trigger='pcu-equip-mage')
   else:add(op('scry',value='bottom'),trigger='pcu-host-capital-hit',once=True,gate='ownTurn')
  elif i==132:add(op('pcl-top-hand-life',n(0)),trigger='death')
  elif i in {133,136,139,150,210,214,218,275}:
   ops=[op('pcl-grave',n(0,999),3 if i==275 else 1,value='equipment:hand' if i==275 else 'creature:bottom' if i in {133,136,150,214} else 'creature:hand')]
   if i==210:ops.append(op('loseLife',n(1)))
   kw=dict(trigger='death' if i in {133,218} else 'enter' if i in {136,214} else 'cast',optional=i in {133,136})
   if i==150:kw.update(activation());ops[0]['a']=n(2)
   add(*ops,**kw)
  elif i==159:add(op('pcl-select-top',n(0),n(1),value='spell'),trigger='enter')
  elif i==213:add(op('millSelf',4),op('gainLife',n(0)))
  elif i==222:add(op('pcl-grave',999,2,value='creature:hand'),token(1,'capital'))
  elif i==224:add(op('millSelf',1),**activation())
  elif i==240:add(buff(m=movement()),op('scry',value='bottom'),target='ownVehicle')
  elif i==273:add(op('gainLife',n(0)),op('scry',value='bottom',condition='pcu-any-equipped-token'),trigger='pcu-equipment-enter',once=True,gate='ownTurn')
 else:return False
 if out.get('kind')=='Enchantment' and not t.startswith('encante '):out['traits']=list(set(out.get('traits',[])+['pcu-global']))
 if out.get('kind')=='Terrain':
  if t.startswith('gera uma mana de cor aleatoria'):out['traits']=list(set(out.get('traits',[])+['precon-random-mana']));out['color']=6
  elif re.match(r'gera \d+ (?:mana )?incolor',t):out['color']=6
  else:
   for name,color in [('sol',0),('lua',1),('terra',2),('fogo',3),('agua',4),('ar',5)]:
    if re.match(r'gera \d+ mana de '+name+r'\b',t):out['color']=color;break
 out['abilities']=aa
 if aa or out.get('kind') in {'Spell','Instant'}:out['rule']='abilities'
 out.pop('unavailableReason',None);c.clear();c.update(out);return True
