"""Freeze approved TXT decks into COL-001, preserving identities and source hashes.
Unknown mechanics remain explicitly unavailable; never silently turn text into vanilla.
"""
from pathlib import Path
import json,re,hashlib,collections,unicodedata
ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/'Documentacao/Design/COL-001-20261009'
DEST=ROOT/'Assets/StreamingAssets'
OUTPUT=SRC/'Generated'
OUTPUT.mkdir(exist_ok=True)
symbols='SLGFAT'
def norm(s):return ''.join(c for c in unicodedata.normalize('NFD',s.casefold()) if not unicodedata.combining(c))
def parse(text,kind):
 out=[]
 for m in re.finditer(r'^'+kind+r' (\d+)\n(.*?)(?=^(?:CARTA|TERRENO) \d+|^50 TERRENOS|\Z)',text,re.M|re.S):
  d={k.strip():v.strip() for line in m.group(2).splitlines() if ':' in line for k,v in [line.split(':',1)]}
  if d.get('Nome'):out.append(d)
 return out
def op(kind,a=0,b=0,c=0,value=''):return dict(kind=kind,a=a,b=b,c=c,value=value)
def ability(*ops,**kw):return dict(dict(trigger='cast',target='owner',scope='any',maxCost=999,count=1,manaColor=-1,ops=list(ops)),**kw)
def aura(*ops,**kw):return ability(*ops,trigger='aura',**kw)

existing={}
for f in (DEST/'Expansions').glob('*.json'):
 if f.name=='official-collection-001.json':continue
 for c in json.loads(f.read_text(encoding='utf-8-sig'))['cards']:existing[c['id']]=c
byname={c['name'].casefold():c for c in existing.values()}
def compile_rules(d,c):
 text=d['Efeito'].strip();t=norm(text)
 if t in ['sem habilidade adicional.','sem habilidade adicional','—']:return [],True
 # Exact, bounded patterns only. More mechanics are compiled in named modules.
 if c['kind']=='Terrain' and re.fullmatch(r'Gera 1 mana de .+ na renovação do seu turno\. Seu deck de terrenos pode conter até 50 cópias desta carta\.',text):return [],True
 m=re.fullmatch(r'Uma criatura sua recebe \+0/\+(\d+) até o fim do turno\.',text)
 if m:return [ability(op('buff',b=int(m[1])),target='own')],True
 m=re.fullmatch(r'(?:Ao entrar,|Ao morrer,) cure (\d+) (?:de vida )?da sua capital\.',text)
 if m:return [ability(op('gainLife',int(m[1])),trigger='enter' if text.startswith('Ao entrar') else 'death')],True
 m=re.fullmatch(r'Ao entrar, coloque as (\w+) cartas do topo do seu grimório no seu cemitério\.',text)
 if m and m[1] in ['duas','três','quatro']:return [ability(op('millSelf',{'duas':2,'três':3,'quatro':4}[m[1]]),trigger='enter')],True
 m=re.fullmatch(r'Equipar: (\d+) mana(?:s)? genérica(?:s)?,? no mesmo tile\. A criatura equipada recebe \+(\d+)/\+(\d+)\.',text)
 if m:c['equipCost']=int(m[1]);return [aura(op('buff',int(m[2]),int(m[3])),target='host')],True
 m=re.fullmatch(r'Equipar: (\d+) mana(?:s)? genérica(?:s)?,? no mesmo tile\. A criatura equipada recebe \+(\d+) de ataque\.',text)
 if m:c['equipCost']=int(m[1]);return [aura(op('buff',int(m[2])),target='host')],True
 return [],False

def convert(d,id):
 typ=norm(d['Tipo']);sub=d.get('Subtipo','');txt=d['Efeito'];kind={'criatura':'Creature','truque':'Instant','feitico':'Spell','construcao':'Construction','encantamento':'Enchantment','terreno':'Terrain','artefato':'Artifact','artefato — equipamento':'Equipment'}.get(typ)
 if kind=='Artifact' and 'equipamento' in norm(sub):kind='Equipment'
 if kind=='Artifact' and any(x in norm(sub) for x in ['construcao','costrucao']):kind='Construction'
 if not kind:raise ValueError((id,typ))
 colored=[d.get('Custo','').count(s) for s in symbols]+[0] if kind!='Terrain' else [0]*7
 match=re.search(r'\d+',d.get('Custo',''));cost=int(match[0]) if match and kind!='Terrain' else 0
 colors=[i for i,s in enumerate(symbols) if s in d.get('Cor','')]
 stats=re.fullmatch(r'(\d+)/(\d+)',d.get('Ataque/Defesa','').strip())
 def num(key,default):m=re.match(r'\d+',d.get(key,''));return int(m[0]) if m else default
 c=dict(id=id,name=d['Nome'],kind=kind,text=txt,rarity=d['Raridade'],color=colors[0] if colors else 6,identityColors=colors,coloredCost=colored,cost=cost,attack=int(stats[1]) if stats else 0,defense=max(1,int(stats[2])) if stats else 1,actions=num('PA',1),movement=num('Movimento',2),range=num('Alcance',1),equipCost=1,subtypes=[x.strip() for x in sub.split(',') if x.strip() and x.strip()!='—'],effects=[],abilities=[],traits=[],keywords=[],rule='',maxCopies=1,commander=False,art='soldier')
 if not c['subtypes'] and kind=='Creature':c['subtypes']=['Criatura']
 if 'voar' in norm(d.get('Palavras-chave','')):c['keywords']=['flying']
 if 'veiculo' in norm(sub):
  seats=re.search(r'(\d+) vagas?',txt);crew=re.search(r'exige (\d+) tripulante',txt)
  if seats and crew:c.update(vehicleSeats=int(seats[1]),vehicleCrew=int(crew[1]))
 if kind=='Terrain':c.update(attack=0,actions=0,movement=0)
 c['abilities'],supported=compile_rules(d,c)
 if c['abilities'] or kind in ['Spell','Instant']:c['rule']='abilities'
 if not supported:c['unavailableReason']='COL-001: efeito aprovado; implementação e teste específicos pendentes.'
 return c,supported

source=json.loads((SRC/'origem-e-composicao.json').read_text(encoding='utf-8'))
resolutions=json.loads((SRC/'identity-resolutions.json').read_text(encoding='utf-8'))
assert resolutions['approved']
renames={(r['file'],r['slot'].split()[0],int(r['slot'].split()[1])):r for r in resolutions['entries']}
new={};decks=[];entries=[];audit=[];hashes={}
for di,sd in enumerate(source,1):
 path=SRC/sd['arquivo'];raw=path.read_text(encoding='utf-8-sig');hashes[path.name]=hashlib.sha256(path.read_bytes()).hexdigest()
 cmd=next(c['id'] for c in existing.values() if c.get('commander') and c['name']==re.search(r'^Comandante escolhido: (.*)$',raw,re.M)[1])
 deck=dict(id=f'COL001-D{di:02d}',name=re.search(r'^Nome do deck: (.*)$',raw,re.M)[1],commander=cmd,description=sd['tematica'],main=[],terrains=[])
 for zone,label,originkey in [('main','CARTA','cartas'),('terrains','TERRENO','terrenos_origem')]:
  rows=parse(raw,label);assert len(rows)==(100 if zone=='main' else 50)
  for position,(d,meta) in enumerate(zip(rows,sd[originkey]),1):
   change=renames.get((path.name,label,position))
   if change:
    assert d['Nome']==change['originalName']
    d=dict(d,Nome=change['name'])
    meta=dict(meta,id=change['id'] or meta['id'])
   known=byname.get(d['Nome'].casefold())
   if known:
    id=known['id'];supported=not bool(known.get('unavailableReason'))
    # Existing gameplay identities retain their code/data, not duplicated per deck.
    if norm(known.get('text',''))!=norm(d['Efeito']):raise ValueError(('Existing text conflict',id,d['Nome']))
   else:
    id=meta['id'];candidate,supported=convert(d,id)
    if id in new:assert new[id]==candidate,('Conflicting repeated identity',id)
    else:new[id]=candidate;byname[d['Nome'].casefold()]=candidate
   deck[zone].append(id)
   entries.append(dict(deckId=deck['id'],zone=zone,position=position,cardId=id,rarity=d['Raridade'],sourceId=meta['id']))
  assert len(set(deck[zone]))==len(deck[zone])
 decks.append(deck)
for c in new.values():audit.append(dict(id=c['id'],name=c['name'],implemented=not bool(c.get('unavailableReason')),text=c['text']))
pack=dict(schemaVersion=1,id='COL-001',title='Coleção Oficial 01',version='1.0.0',status='design-approved',cards=list(new.values()),printings=[])
(OUTPUT/'official-collection-001-expansion.json').write_text(json.dumps(pack,ensure_ascii=False,indent=2),encoding='utf-8')
manifest=dict(version=1,id='COL-001',title='Coleção Oficial 01',approvalDate='2026-10-09',decks=decks,entries=entries,cardIds=sorted({e['cardId'] for e in entries}|{d['commander'] for d in decks}))
(OUTPUT/'official-collection-001-decks.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
(SRC/'implementation-audit.json').write_text(json.dumps(audit,ensure_ascii=False,indent=2),encoding='utf-8')
(SRC/'source-hashes.json').write_text(json.dumps(hashes,indent=2),encoding='utf-8')
print(json.dumps(dict(decks=len(decks),newDefinitions=len(new),ready=sum(x['implemented'] for x in audit),pending=sum(not x['implemented'] for x in audit))))
