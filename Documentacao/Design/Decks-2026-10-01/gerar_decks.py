"""Oficina editorial: não importa cartas nem altera o perfil do jogo."""
from pathlib import Path
import json, re, collections, html

ROOT = Path(__file__).resolve().parent
VAULT = ROOT.parents[3]
GAME = Path('C:/Users/gil/TCG')
COLORS = ['Sol', 'Lua', 'Água', 'Fogo', 'Ar', 'Terra', 'Incolor']
SYMBOLS = ['S', 'L', 'G', 'F', 'A', 'T', '']
KINDS = {'Creature':'Criatura','Terrain':'Terreno','Spell':'Feitiço','Instant':'Truque','Equipment':'Equipamento','Building':'Construção','Construction':'Construção','Artifact':'Artefato','Enchantment':'Encantamento'}
cards = []
def add(id, name, color, kind, cost, text, subtypes=(), attack=None, defense=None, pa=None, movement=None, range=None, tags=(), source='Proposta do assistente, 2026-10-01', state='Proposta nova — não aprovada nem implementada', commander=False):
    colors = [color] if isinstance(color,int) and color < 6 else ([] if isinstance(color,int) else color)
    mana = sum(int(n) for n in re.findall(r'\d+',cost)) + sum(cost.count(s) for s in SYMBOLS[:6]) if cost != '—' else 0
    cards.append(dict(id=id,name=name,colors=colors,kind=kind,cost=cost,mana=mana,text=text,subtypes=list(subtypes),attack=attack,defense=defense,pa=pa,movement=movement,range=range,tags=list(tags),source=source,state=state,commander=commander,quantityLimit=1))

# Dados autorais: nomes e textos copiados das definições existentes, sem reescrita.
for file in sorted((GAME/'Assets/StreamingAssets/Expansions').glob('*.json')):
    if file.name not in ['exp-001-basico.json','medieval-author-20260919.json','medieval-commanders-20260923.json','medieval-commanders-20260924.json']: continue
    for c in json.loads(file.read_text(encoding='utf-8-sig'))['cards']:
        if c.get('unavailableReason'): continue
        colored = c.get('coloredCost',[0]*7)
        identity = sorted(set([i for i,v in enumerate(colored[:6]) if v] + c.get('identityColors',[]) + ([c['color']] if c.get('color',6)<6 else [])))
        cost = ''.join(SYMBOLS[i]*v for i,v in enumerate(colored[:6])) + (str(c.get('cost',0)) if c.get('cost',0) else '')
        add(c['id'],c['name'],identity,KINDS[c['kind']], '—' if c['kind']=='Terrain' else cost or '0',c.get('text',''),c.get('subtypes',[]),c.get('attack') if c['kind']=='Creature' else None,c.get('defense') if c['kind'] in ['Creature','Building','Construction'] else None,c.get('actions',1) if c['kind']=='Creature' else None,c.get('movement',2) if c['kind']=='Creature' else None,c.get('range',1) if c['kind']=='Creature' else None,source='Assets/StreamingAssets/Expansions/'+file.name,state='Catálogo do jogo — balanceamento experimental',commander=c.get('commander',False))

# Rascunhos anteriores mantêm IDs e estado editorial original.
editorial = VAULT/'06 - Conteudo/Expansoes/EXP-001 - Medieval/Prototipo Medieval - Dados Editoriais.json'
for c in json.loads(editorial.read_text(encoding='utf-8-sig'))['cards']:
    cost = c['cost']
    for a,b in [('SOL','S'),('LUA','L'),('ÁGUA','G'),('AGUA','G'),('AGU','G'),('FOGO','F'),('FOG','F'),('AR','A'),('TERRA','T'),('TER','T')]: cost=cost.replace('{'+a+'}',b)
    cost=cost.replace('{','').replace('}','')
    add(c['id'],c['name'],[COLORS.index(x) for x in c['colors'] if x in COLORS[:6]],c['type'],cost,c['text'],attack=c.get('attack'),defense=c.get('defense'),pa=c.get('pa'),movement=c.get('movement'),range=c.get('range'),source='Prototipo Medieval - Dados Editoriais.json (2026-09-15)',state='Rascunho anterior — não aprovado/importado')

# Cada combinação modifica atributos e custo; não são reimpressões renomeadas.
themes = [
 ('Aurora', ['Humano','Cavaleiro'], ['formação','cavaleiro','proteção'], [
  'Enquanto outra criatura sua estiver em um terreno ortogonalmente adjacente, esta recebe +0/+2.',
  'Ao entrar, cure 2 da sua capital.',
  'Ao entrar, outra criatura sua no mesmo terreno recebe +0/+2 até o fim do turno.',
  'Uma vez por turno, quando outra criatura sua no mesmo terreno sofrer dano, cure 1 da sua capital.',
  'Ao morrer em combate, outra criatura sua no mesmo terreno recebe um marcador +1/+0.',
  'Ao atacar, se outra criatura sua estiver adjacente ao alvo, esta recebe +1/+0 até o fim do combate.',
  'A primeira vez em cada turno que esta defender, previna 1 do dano que seria causado a ela nesse combate.',
  'Ao entrar, você pode mover outra criatura sua em um terreno adjacente para este terreno, sem gastar PA, respeitando bloqueios.']),
 ('Ossadas',['Vampiro','Clérigo'],['sacrifício','cemitério','cura'],[
  'Ao entrar, você pode perder 2 de vida da capital. Se fizer isso, compre uma carta.',
  'Ao morrer, cure 2 da sua capital.',
  'Ao causar dano de combate a uma criatura, cure 1 da sua capital.',
  '1 PA, sacrifique outra criatura sua neste terreno: esta recebe +2/+0 até o fim do turno. Use apenas uma vez por turno.',
  'Ao entrar, coloque uma criatura de custo total até 2 do seu cemitério no topo do seu grimório.',
  'Ao atacar uma criatura ferida, esta recebe +2/+0 até o fim do combate.',
  'Uma vez no seu turno, quando outra criatura sua morrer neste terreno, esta recebe +1/+0 até o fim do turno.',
  'Ao causar dano de combate à capital inimiga, coloque a carta do topo do grimório daquele jogador no cemitério dele.']),
 ('Estuário',['Humano','Mago'],['magia','água','reposicionamento'],[
  'Ao entrar, olhe a carta do topo do seu grimório. Você pode colocá-la no fundo.',
  'Ao entrar, outra criatura sua neste terreno recebe +0/+2 até o fim do turno.',
  'Ao morrer, você pode colocar um Truque do seu cemitério no fundo do seu grimório.',
  'Uma vez por turno, quando você conjurar um Truque, esta recebe +0/+1 até o fim do turno.',
  'Ao entrar, você pode mover outra criatura sua adjacente para este terreno sem gastar PA, respeitando bloqueios.',
  '1 PA: uma criatura inimiga adjacente recebe -1 de ataque até o fim do turno. Use apenas uma vez por turno.',
  'Ao entrar, você pode colocar uma carta da mão no fundo do grimório. Se fizer isso, compre uma carta.',
  'Ao causar dano de combate à capital inimiga, olhe as duas cartas do topo do seu grimório e devolva-as na ordem escolhida.']),
 ('Escória',['Goblin','Guerreiro'],['goblin','dano','pressão'],[
  'Ao entrar, cause 1 de dano a uma criatura inimiga adjacente.',
  'Ao morrer, cause 1 de dano a uma criatura inimiga no mesmo terreno.',
  'Ao atacar, esta recebe +1/+0 até o fim do combate.',
  'Ao causar dano de combate a uma capital, esta sofre 1 de dano.',
  'Ao entrar, outro Goblin seu neste terreno recebe +1/+0 até o fim do turno.',
  'Uma vez por turno, quando sobreviver a dano, esta recebe +1/+0 até o fim do turno.',
  '1 PA, sacrifique esta criatura: cause 2 de dano a uma construção adjacente.',
  'Ao atacar uma construção, esta recebe +2/+0 até o fim do combate.']),
 ('Vendaval',['Humano','Arqueiro'],['alcance','movimento','evasão'],[
  'Após seu primeiro ataque em cada turno, você pode mover esta criatura 1 terreno, sem gastar PA, respeitando bloqueios.',
  'Se não houver inimigos adjacentes, esta recebe +1 de alcance.',
  'Ao entrar, outra criatura sua neste terreno recebe +1 de movimento até o fim do turno.',
  'Enquanto não houver outra criatura no mesmo terreno, esta recebe +1/+0.',
  'Ao atacar depois de se mover neste turno, esta recebe +1/+0 até o fim do combate.',
  '1 PA: esta recebe +1 de alcance até o fim do turno. Use apenas uma vez por turno.',
  'Ao entrar, olhe a carta do topo do seu grimório. Você pode colocá-la no fundo.',
  'Ao causar dano de combate a uma capital, você pode mover esta criatura 1 terreno, sem gastar PA, respeitando bloqueios.']),
 ('Raiz Antiga',['Elemental','Guardião'],['defesa','território','criatura grande'],[
  'Enquanto não tiver se movido neste turno, esta recebe +0/+2.',
  'Ao entrar, cure 2 de dano de uma construção sua adjacente.',
  'Enquanto estiver adjacente a uma construção sua, esta recebe +0/+2.',
  'Esta não pode ser movida por efeitos de cartas inimigas.',
  'Ao morrer em combate, cure 2 da sua capital.',
  '1 PA: esta recebe +0/+2 até o início do seu próximo turno. Use apenas uma vez por turno.',
  'Ao entrar, outra criatura sua neste terreno recebe +0/+3 até o fim do turno.',
  'Enquanto estiver no terreno da sua capital, esta recebe +0/+3.'])]
prof = [('Aprendiz',1,1,2,1),('Veterano',2,2,4,1),('Guardião',4,4,6,2)]
for col,(theme,subs,tags,effects) in enumerate(themes):
    for e,txt in enumerate(effects):
        for p,(rank,generic,atk,defense,pa) in enumerate(prof):
            # Arqueiros têm alcance 2; voo explicitamente presente em dois perfis de Ar.
            st=list(subs)
            if col==4 and e in [4,7]: st=['Humano','Pirata']
            if col==5 and p==2: atk,defense=5,8
            flight=col==4 and e in [4,7]
            add(f'PD26-{SYMBOLS[col]}-{e*3+p+1:03}',f'{rank} {['da Vigília','da Ponte','do Pórtico','da Escolta','do Juramento','da Patrulha','da Torre','da Marcha'][e]} de {theme}',col,'Criatura',SYMBOLS[col]*(2 if p==2 else 1)+str(generic),('Palavra-chave: Voar. ' if flight else '')+txt,st,atk,defense,pa,2,2 if col==4 else 1,tags+(['voar'] if flight else []))

support = [
 [
 ('Juramento da Escolta','Truque','S1','Uma criatura sua adjacente a outra criatura sua recebe +0/+4 até o fim do turno.','proteção'),
 ('Alvorada Restauradora','Feitiço','S2','Cure 4 da sua capital.','cura'),
 ('Marcha dos Escudeiros','Feitiço','SS1','Mova até duas criaturas suas 1 terreno cada, respeitando bloqueios, sem gastar PA.','formação'),
 ('Escudo do Juramento','Equipamento','S2','A criatura equipada recebe +0/+3. Equipar: 1 mana genérica, no mesmo terreno.','equipamento'),
 ('Estandarte dos Cavaleiros','Encantamento','SS1','Encante uma criatura sua. Ela recebe +0/+2; se for Cavaleiro, recebe também +1/+0.','cavaleiro'),
 ('Lança da Companhia','Equipamento','S2','A criatura equipada recebe +1/+0; enquanto adjacente a outra criatura sua, recebe também +0/+2. Equipar: 1 mana genérica, no mesmo terreno.','equipamento')],
 [
 ('Dízimo de Sangue','Feitiço','L1','Sacrifique uma criatura sua. Se sacrificou, compre duas cartas e perca 2 de vida da capital.','sacrifício'),
 ('Lembrança do Túmulo','Feitiço','LL2','Devolva uma criatura de custo total até 4 do seu cemitério à sua mão.','cemitério'),
 ('Sangria da Vigília','Truque','L2','Uma criatura sua recebe +2/+0 até o fim do turno. Cure 2 da sua capital.','cura'),
 ('Maldição da Fome','Encantamento','L1','Encante uma criatura. Ela recebe -1 de defesa. Ao morrer a criatura encantada, cure 2 da sua capital.','encantamento'),
 ('Pá do Saqueador','Equipamento','L2','A criatura equipada recebe +1/+0. Uma vez por turno, quando causar dano de combate a uma capital, o dono dela coloca duas cartas do topo do grimório no cemitério. Equipar: 1 mana genérica, no mesmo terreno.','mill'),
 ('Memórias Afundadas','Feitiço','L2','Escolha um jogador. Ele coloca três cartas do topo do grimório no cemitério. Você perde 1 de vida da capital.','mill')],
 [
 ('Maré Curta','Truque','G1','Mova uma criatura sua até 1 terreno, respeitando bloqueios, sem gastar PA.','reposicionamento'),
 ('Ler as Correntes','Feitiço','G1','Olhe três cartas do topo do seu grimório. Ponha uma na mão e as demais no fundo, na ordem escolhida.','magia'),
 ('Nevoeiro do Estuário','Truque','G2','Uma criatura recebe -3 de ataque até o fim do turno.','proteção'),
 ('Casco Envernizado','Encantamento','G1','Encante um veículo seu. Ele recebe +0/+3.','veículo'),
 ('Rede do Pescador','Equipamento','G2','A criatura equipada recebe +0/+2. Após causar dano de combate a uma criatura, pode movê-la 1 terreno, respeitando bloqueios. Equipar: 1 mana genérica, no mesmo terreno.','equipamento'),
 ('Recuperar Pergaminho','Feitiço','GG2','Devolva um Truque do seu cemitério à sua mão. Exile esta carta após resolver.','magia')],
 [
 ('Faísca de Cerco','Feitiço','F1','Cause 2 de dano a uma construção ou criatura inimiga.','dano'),
 ('Brasa Persistente','Truque','F2','Cause 2 de dano a uma criatura inimiga. Ela recebe +1 de ataque até o fim do turno.','dano'),
 ('Investida de Escória','Feitiço','FF1','Até duas criaturas suas recebem +2/+0 até o fim do turno.','pressão'),
 ('Maldição da Fornalha','Encantamento','F1','Encante uma criatura. Ela recebe +2/+0 e -1 de defesa.','encantamento'),
 ('Clava do Braseiro','Equipamento','F2','A criatura equipada recebe +2/+0. Após atacar, sofre 1 de dano. Equipar: 1 mana genérica, no mesmo terreno.','equipamento'),
 ('Estilhaçar Portões','Feitiço','F3','Cause 5 de dano a uma construção inimiga.','dano')],
 [
 ('Giro de Flanco','Truque','A1','Mova uma criatura sua 1 terreno, respeitando bloqueios; ela recebe +1/+0 até o fim do turno.','movimento'),
 ('Vigiar do Alto','Feitiço','A1','Olhe as três cartas do topo do seu grimório e devolva-as na ordem escolhida. Compre uma carta.','magia'),
 ('Flecha do Horizonte','Truque','A2','Uma criatura sua com alcance pelo menos 2 recebe +2 de alcance até o fim do turno.','alcance'),
 ('Asas de Tecido','Equipamento','AA2','A criatura equipada ganha a palavra-chave Voar. Equipar: 1 mana genérica, no mesmo terreno.','voar'),
 ('Marca do Batedor','Encantamento','A1','Encante uma criatura sua. Ela recebe +1 de movimento.','movimento'),
 ('Passagem Aérea','Feitiço','AA2','Uma criatura sua ganha Voar até o fim do turno.','voar')],
 [
 ('Pele de Casca','Truque','T1','Uma criatura sua recebe +0/+4 até o fim do turno.','defesa'),
 ('Raízes Protetoras','Encantamento','T1','Encante uma criatura sua. Ela recebe +0/+3 e -1 de movimento, mínimo 0.','encantamento'),
 ('Erguer o Bastião','Feitiço','T2','Cure todo o dano de uma construção sua.','defesa'),
 ('Clava de Pedra','Equipamento','T2','A criatura equipada recebe +2/+1 e -1 de movimento, mínimo 0. Equipar: 1 mana genérica, no mesmo terreno.','equipamento'),
 ('Recuperar Sementes','Feitiço','TT2','Devolva uma criatura de custo total até 3 do seu cemitério ao fundo do grimório. Compre duas cartas.','cemitério'),
 ('Abraço das Raízes','Truque','TT1','Até duas criaturas suas no mesmo terreno recebem +0/+3 até o fim do turno.','defesa')]]
for col,entries in enumerate(support):
    for i,(name,kind,cost,txt,tag) in enumerate(entries):
        add(f'PD26-{SYMBOLS[col]}-{25+i:03}',name,col,kind,cost,txt,tags=[tag])

# Núcleo incolor: custos maiores e ferramentas locais, sem remoção universal barata.
body_names=['Sentinela de Bronze','Golem de Argila','Vigia de Porcelana','Carregador de Ferro','Batedor de Cobre','Guardião de Granito','Escudeiro de Estanho','Colosso de Basalto','Autômato de Vidro','Peregrino Mecânico']
body_effects=[
 'Sem habilidades.',
 'Enquanto no terreno da sua capital, esta recebe +0/+2.',
 'Ao entrar, cure 1 da sua capital.',
 'Ao morrer, olhe a carta do topo do seu grimório. Você pode colocá-la no fundo.',
 'Ao entrar, você pode mover outra criatura sua no mesmo terreno 1 terreno, respeitando bloqueios, sem gastar PA.',
 'Enquanto estiver adjacente a uma construção sua, esta recebe +0/+1.',
 'Ao entrar, uma criatura sua no mesmo terreno recebe +0/+1 até o fim do turno.',
 'Esta não pode ser movida por efeitos de cartas suas.',
 'Ao atacar, esta recebe +1/+0 até o fim do combate e sofre 1 de dano após o combate.',
 '1 PA: olhe a carta do topo do seu grimório. Você pode colocá-la no fundo. Use apenas uma vez por turno.']
for i,name in enumerate(body_names):
    for p,(suffix,cost,atk,df,pa) in enumerate([('Leve',3,2,2,1),('de Patrulha',5,3,5,2),('de Cerco',7,5,7,1)]):
        add(f'PD26-N-{i*3+p+1:03}',name+' '+suffix,6,'Criatura',str(cost),body_effects[i],['Autômato','Cavaleiro' if i==6 else 'Construto'],atk,df,pa,1 if p==2 else 2,1,['criatura','defesa']+(['cavaleiro'] if i==6 else []))
equipment=[
 ('Adaga de Viagem',2,'+1/+0'),('Espada de Ferro',3,'+2/+0'),('Broquel de Madeira',2,'+0/+2'),('Escudo de Torre',4,'+0/+4 e -1 de movimento, mínimo 0'),
 ('Armadura de Malha',4,'+1/+2'),('Armadura de Placas',5,'+0/+5 e -1 PA disponível por turno, mínimo 0'),('Lança de Caravana',4,'+1/+0 e +1 de alcance'),('Botas de Marcha',4,'+1 de movimento'),
 ('Manoplas de Trabalho',3,'+1/+1'),('Elmo de Guarda',3,'+0/+3'),('Machado de Lenhador',4,'+3/+0 e -1 de defesa'),('Martelo de Pedreiro',5,'+2/+2 e -1 de movimento, mínimo 0'),
 ('Capa do Vigia',3,'+0/+1; enquanto no terreno da sua capital, recebe também +0/+2'),('Espora de Cobre',4,'+1/+0; após se mover neste turno, recebe também +1/+0'),
 ('Pique da Sentinela',4,'+0/+1; enquanto não se moveu neste turno, recebe também +1 de alcance'),('Faca de Talhar',3,'+1/+0; ao entrar em campo este equipamento, cure 1 da sua capital'),
 ('Sela de Couro',3,'+0/+1 e +1 de movimento'),('Mangual Pesado',6,'+4/+0 e -1 de movimento, mínimo 0'),('Braçadeira da Escolta',3,'+0/+1; enquanto adjacente a outra criatura sua, recebe também +1/+0'),('Couraça do Peregrino',5,'+1/+3')]
for i,(name,cost,bonus) in enumerate(equipment):
    add(f'PD26-N-{31+i:03}',name,6,'Equipamento',str(cost),'A criatura equipada recebe '+bonus+'. Equipar: 1 mana genérica, no mesmo terreno.',tags=['equipamento'])
building_effects=[
 ('Posto de Vigia',3,4,'Criaturas suas neste terreno recebem +0/+1.'),
 ('Enfermaria de Campanha',4,4,'No início do seu turno, cure 1 da sua capital se você controlar uma criatura neste terreno.'),
 ('Oficina de Remendos',3,5,'Uma vez no seu turno, pague 2 manas genéricas: cure 2 de dano de uma construção sua neste terreno.'),
 ('Depósito de Provisões',4,5,'Ao entrar, cure 3 da sua capital.'),
 ('Guarita de Pedra',5,7,'Criaturas suas neste terreno recebem +0/+2.'),
 ('Abrigo dos Viajantes',3,4,'A primeira criatura sua que entrar neste terreno em cada turno recebe +0/+1 até o fim do turno.'),
 ('Torre de Observação',5,5,'Criaturas suas neste terreno com alcance pelo menos 2 recebem +1 de alcance.'),
 ('Casa de Mapas',4,4,'No início do seu turno, olhe a carta do topo do seu grimório. Você pode colocá-la no fundo.'),
 ('Celeiro de Reserva',5,6,'Uma vez no seu turno, pague 3 manas genéricas e coloque uma carta da mão no fundo do grimório: compre uma carta.'),
 ('Forja de Campanha',4,5,'O primeiro custo de equipar que você pagar neste terreno em cada turno custa 1 mana genérica a menos, mínimo 0.'),
 ('Muralha de Entulho',2,6,'Criaturas suas neste terreno têm -1 de movimento, mínimo 0.'),
 ('Pátio de Manobras',5,5,'Uma vez no seu turno, pague 2 manas genéricas: uma criatura sua neste terreno recebe +1 de movimento até o fim do turno.'),
 ('Silo de Mana',6,4,'Uma vez no seu turno, pague 3 manas genéricas: gere 2 manas incolores.'),
 ('Arquivo de Viagem',5,4,'Ao ser destruída, compre uma carta.'),
 ('Bastião de Retaguarda',6,8,'Criaturas suas neste terreno recebem +0/+3 enquanto este for o terreno da sua capital.')]
for i,(name,cost,df,txt) in enumerate(building_effects):
    add(f'PD26-N-{51+i:03}',name,6,'Construção',str(cost),txt+' Não se move nem causa dano ao defender.',defense=df,tags=['construção','defesa'])
for i,(name,cost,atk,df,seats,crew,mov) in enumerate([
 ('Carro de Boi',4,1,6,2,1,1),('Carroça de Batedores',4,2,4,2,1,3),('Caravana Blindada',6,2,8,3,2,1),('Vagão de Milícia',5,3,5,3,1,2),('Torre Sobre Rodas',7,3,10,4,2,1),
 ('Trenó de Carga',3,1,4,1,1,2),('Barca de Cerco',6,4,6,2,1,2),('Carruagem de Comando',6,2,7,4,2,2),('Aríete Móvel',5,5,4,1,1,1),('Comboio de Socorro',7,2,9,5,2,1)]):
    add(f'PD26-N-{66+i:03}',name,6,'Artefato',str(cost),f'Veículo; {atk}/{df}, 2 PA, movimento {mov}, alcance 1. Capacidade {seats}; tripulação mínima {crew}. Só age tripulado. Passageiros podem agir enquanto tripulado. Embarcar/desembarcar: 1 PA do passageiro, mesmo terreno. Destruído o veículo, passageiros morrem.', ['Veículo'],tags=['veículo'])
neutral_spells=[
 ('Recontar Provisões','Feitiço',4,'Compre uma carta. Cure 1 da sua capital.','compra'),
 ('Consultar o Mapa','Feitiço',3,'Olhe as três cartas do topo do seu grimório e devolva-as na ordem escolhida.','seleção'),
 ('Conserto Demorado','Feitiço',4,'Cure 4 de dano de uma construção sua.','construção'),
 ('Abrigo Improvisado','Truque',3,'Uma criatura sua recebe +0/+2 até o fim do turno.','proteção'),
 ('Armar a Escolta','Feitiço',4,'Uma criatura sua recebe +1/+1 até o fim do turno. Compre uma carta.','equipamento'),
 ('Trocar a Bagagem','Feitiço',3,'Ponha uma carta da mão no fundo do grimório. Se colocou, compre uma carta.','seleção'),
 ('Reparos na Capital','Feitiço',5,'Cure 4 da sua capital.','cura'),
 ('Recuo Cauteloso','Feitiço',4,'Mova uma criatura sua 1 terreno, respeitando bloqueios, sem gastar PA.','movimento'),
 ('Distribuir Escudos','Feitiço',5,'Até duas criaturas suas no mesmo terreno recebem +0/+2 até o fim do turno.','proteção'),
 ('Vigília Longa','Truque',4,'Uma criatura sua no terreno da sua capital recebe +0/+4 até o fim do turno.','proteção'),
 ('Inventário da Caravana','Feitiço',6,'Compre duas cartas; ponha uma carta da mão no fundo do grimório.','compra'),
 ('Rito da Pedra Vazia','Feitiço',3,'Gere 2 manas incolores.','mana'),
 ('Ferramentas de Reserva','Feitiço',5,'Devolva um equipamento de custo total até 2 do seu cemitério à sua mão.','equipamento'),
 ('Reunir os Feridos','Feitiço',5,'Cure 2 de dano de até duas criaturas suas no mesmo terreno.','defesa'),
 ('Medir a Distância','Truque',4,'Uma criatura sua recebe +1 de alcance até o fim do turno.','alcance')]
for i,(name,kind,cost,txt,tag) in enumerate(neutral_spells): add(f'PD26-N-{76+i:03}',name,6,kind,str(cost),txt,tags=[tag])
for i,(name,cost,txt) in enumerate([
 ('Voto da Pedra',3,'Ela recebe +0/+2.'),('Voto da Vigília',4,'Enquanto não tiver se movido neste turno, ela recebe +0/+3.'),('Voto do Caminhante',4,'Ela recebe +1 de movimento.'),
 ('Voto da Escolta',3,'Enquanto adjacente a outra criatura sua, ela recebe +0/+2.'),('Voto da Ferrugem',2,'Ela recebe -1 de movimento, mínimo 0, e +0/+2.'),('Voto do Peregrino',5,'Ela recebe +1/+2.'),
 ('Voto do Regresso',4,'Ao morrer a criatura encantada, cure 2 da sua capital.'),('Voto do Fardo',3,'Ela recebe +2/+0 e -1 PA disponível por turno, mínimo 0.'),
 ('Voto da Guarda',4,'Enquanto no terreno da sua capital, ela recebe +1/+3.'),('Voto do Sacrifício',5,'Ao morrer a criatura encantada, você pode colocar um equipamento seu de custo total até 2 do cemitério no fundo do grimório.')]):
    add(f'PD26-N-{91+i:03}',name,6,'Encantamento',str(cost),'Encante uma criatura sua. '+txt,tags=['encantamento'])

# Terrenos distintos: efeitos locais com custo, condição e limite explícitos.
land_nouns=['Pátio','Travessia','Torreão','Refúgio','Clareira','Terraço','Posto','Santuário','Acampamento','Fortim']
conditions=[('da Guarda','se uma criatura sua estiver neste terreno'),('da Fronteira','se uma construção sua estiver neste terreno'),('da Retaguarda','se este for o terreno da sua capital'),('da Vigília','se você controlar ao menos duas criaturas neste terreno'),('dos Viajantes','se uma criatura sua entrou neste terreno neste turno')]
land_actions=[
 ['uma criatura sua neste terreno recebe +0/+2 até o fim do turno','cure 1 da sua capital','um Cavaleiro seu neste terreno recebe +1/+1 até o fim do turno','uma criatura sua neste terreno adjacente a outra criatura sua recebe +1/+0 até o fim do turno','previna o próximo 1 de dano que seria causado neste turno a uma criatura sua neste terreno','uma criatura sua neste terreno recebe +0/+3 até o fim do turno e não pode se mover neste turno','olhe a carta do topo do seu grimório e devolva-a ao topo','cure 2 de dano de uma criatura sua neste terreno','uma criatura sua neste terreno recebe +1/+0 até o fim do turno','cure 1 de dano de uma construção sua neste terreno'],
 ['você pode perder 1 de vida da capital para uma criatura sua neste terreno receber +2/+0 até o fim do turno','coloque uma carta do topo do seu grimório no seu cemitério','cure 1 da sua capital','uma criatura sua neste terreno recebe +1/+0 até o fim do turno','olhe a carta do topo do seu grimório; você pode colocá-la no seu cemitério','coloque uma criatura de custo total até 1 do seu cemitério no fundo do grimório','uma criatura sua neste terreno recebe +2/+0 e -1 de defesa até o fim do turno','uma criatura inimiga ferida neste terreno recebe -1 de ataque até o fim do turno','coloque um Truque do seu cemitério no fundo do grimório','cure 1 de dano de uma criatura sua neste terreno'],
 ['olhe a carta do topo do seu grimório; você pode colocá-la no fundo','uma criatura neste terreno recebe -1 de ataque até o fim do turno','cure 1 de dano de uma criatura sua neste terreno','uma criatura sua neste terreno recebe +0/+2 até o fim do turno','olhe as duas cartas do topo do seu grimório e devolva-as na ordem escolhida','previna o próximo 1 de dano que seria causado neste turno a uma criatura sua neste terreno','um Mago seu neste terreno recebe +0/+3 até o fim do turno','um veículo seu neste terreno recebe +0/+2 até o fim do turno','coloque um Feitiço do seu cemitério no fundo do grimório','uma criatura sua neste terreno recebe +1 de movimento até o fim do turno'],
 ['uma criatura sua neste terreno recebe +1/+0 até o fim do turno','cause 1 de dano a uma construção inimiga neste terreno','um Goblin seu neste terreno recebe +2/+0 até o fim do turno','uma criatura sua neste terreno recebe +2/+0 até o fim do turno e sofre 1 de dano','cause 1 de dano a uma criatura inimiga ferida neste terreno','uma criatura sua neste terreno recebe +3/+0 e -2 de defesa até o fim do turno','olhe a carta do topo do seu grimório e devolva-a ao topo','uma criatura sua neste terreno recebe +2/+0 até o fim do turno se atacou neste turno','cause 1 de dano a uma criatura inimiga neste terreno e 1 à sua capital','uma criatura sua neste terreno recebe +1 de movimento até o fim do turno'],
 ['uma criatura sua neste terreno recebe +1 de alcance até o fim do turno','uma criatura sua neste terreno recebe +1 de movimento até o fim do turno','olhe a carta do topo do seu grimório; você pode colocá-la no fundo','um Arqueiro seu neste terreno recebe +1/+0 até o fim do turno','uma criatura sua neste terreno recebe +1/+0 até o fim do turno se moveu neste turno','uma criatura sua neste terreno recebe +0/+1 até o fim do turno','olhe as duas cartas do topo do seu grimório e devolva-as na ordem escolhida','uma criatura sua neste terreno recebe +2 de alcance e -1 de ataque até o fim do turno','uma criatura sua neste terreno recebe +2 de movimento e -1 de defesa até o fim do turno','um Pirata seu neste terreno recebe +1/+1 até o fim do turno'],
 ['uma criatura sua neste terreno recebe +0/+2 até o fim do turno','cure 2 de dano de uma construção sua neste terreno','uma criatura sua neste terreno recebe +0/+3 e -1 de movimento até o fim do turno, mínimo 0','cure 1 da sua capital','uma criatura sua neste terreno que não se moveu neste turno recebe +1/+1 até o fim do turno','uma criatura sua neste terreno não pode ser movida por efeitos inimigos até o fim do turno','olhe a carta do topo do seu grimório e devolva-a ao topo','cure 2 de dano de uma criatura sua neste terreno','uma criatura sua neste terreno recebe +0/+4 e não pode atacar neste turno','coloque uma criatura do seu cemitério no fundo do grimório'],
 ['cure 1 de dano de uma criatura sua neste terreno','cure 1 de dano de uma construção sua neste terreno','uma criatura sua neste terreno recebe +0/+1 até o fim do turno','olhe a carta do topo do seu grimório e devolva-a ao topo','cure 1 da sua capital','uma criatura sua neste terreno recebe +1/+0 até o fim do turno','um veículo seu neste terreno recebe +0/+1 até o fim do turno','uma criatura equipada sua neste terreno recebe +0/+1 até o fim do turno','uma criatura sua neste terreno recebe +1 de movimento e -1 de ataque até o fim do turno','coloque um equipamento do seu cemitério no fundo do grimório']]
for col in range(7):
    for j,(suffix,condition) in enumerate(conditions[:4] if col<6 else conditions):
        for i,action in enumerate(land_actions[col]):
            idx=j*10+i+1
            mana='1 mana '+('incolor' if col==6 else 'de '+COLORS[col])
            txt=f'Gera {mana} nas regras normais de geração de terrenos. Uma vez no seu turno, {condition}, pague 2 manas genéricas: {action}. O controlador do terreno usa esta habilidade.'
            add(f'PD26-L{SYMBOLS[col] or "N"}-{idx:03}',f'{land_nouns[i]} {suffix} de {themes[col][0] if col<6 else "Pedra Cinzenta"}',col,'Terreno','—',txt,tags=['terreno',COLORS[col]])

add('IDEIA-004','Comandante da frota afundada',[1,4],'Criatura','AAL3','Criaturas suas que causam dano a capitais fazem o dono descartar cartas do topo do grimório igual ao dano. Escolha uma carta de um cemitério que veio direto do grimório e ponha em sua mão. Segunda habilidade: momento, custo e frequência ainda não definidos pelo autor.', ['Pirata'],3,4,2,2,1,['mill','cemitério','movimento'],source='Texto do autor nesta conversa, 2026-10-01',state='Ideia autoral — detalhes pendentes; não implementada',commander=True)

strategies={
193:('Tomos recuperados',['magia','truque','feitiço','mago','cemitério'],'Desenvolver magos e defesa; trocar magias por tempo; recuperar uma magia a cada início de turno.','Exílio do cemitério e pressão antes do comandante. Não conte com conjuração gratuita.'),
194:('Enxame de escória',['goblin','pressão','dano'],'Reunir Goblins; reservar 3 PA do comandante; criar a onda na posição dele e escolher a capital.','Limpeza de área e caminhos sem terrenos. Quantidade de Goblins importa mais que criaturas genéricas.'),
195:('O reino resistente',['defesa','guardião','construção','criatura grande'],'Construir defesa espalhada no reino; atacar com a Aberração protegida; usar sua morte como reorganização dos terrenos.','A soma aumenta defesa, não ataque; precisa de atacantes de apoio para terminar a partida.'),
196:('Cerco por várias frentes',['formação','soldado','movimento'],'Abrir duas frentes; manter aliados adjacentes aos alvos; coordenar ataques vindos de terrenos diferentes.','Concentrar tudo num terreno perde o plano. Remoção de apoios rompe a formação.'),
197:('Oficina de ossos',['sacrifício','cemitério','ao morrer','compra'],'Baixar corpos baratos; sacrificar com valor de morte; converter cartas compradas em presença e um comandante grande.','Não sacrificar a última defesa da capital. Exílio e falta de PA reduzem o motor.'),
198:('Caçada aos feridos',['ferida','ferido','cemitério','alcance'],'Ferir inimigos, aproximar a comandante e finalizar dentro de dois terrenos para reciclar ameaças.','Depende de posição e morte de inimigo ferido; devolver ao topo não é comprar imediatamente.'),
199:('Reino imóvel',['defesa','construção','não tiver se movido','guardião'],'Fixar o rei, cercá-lo de aliados e acumular defesa permanente; atacar com uma ala enquanto a base cresce.','Movimentos forçados e ataques fora da área. Defesa acumulada não substitui condição de vitória.'),
200:('Linhas de tiro',['alcance','arqueiro','movimento'],'Criar corredores para arqueiros; afastar inimigos adjacentes; atacar de longe com cobertura.','Adjacência inimiga desliga o bônus; proteger os atiradores com corpos e reposicionamento.'),
201:('Cavalgada em chamas',['movimento','pressão','equipamento'],'Montar rotas andáveis e mover antes do primeiro ataque; aproveitar bônus de deslocamento em uma ofensiva decisiva.','Falta de terreno, bloqueios e defesa da capital. Movimento programado não inicia ataque.'),
202:('Herança da guarda',['ao morrer','sacrifício','formação','ataque'],'Manter pares adjacentes; trocar criaturas e concentrar marcadores de ataque num sobrevivente protegido.','Só ataque é herdado; evitar perder todos os possíveis herdeiros na mesma área.'),
225:('Coroas do eclipse',['encantamento','cavaleiro','defesa','cemitério'],'Priorizar permanentes com símbolos Sol e Lua; equilibrar ataque e defesa por devoção; proteger o Elo.','Remoção de permanentes derruba atributos. Magias no cemitério não contam devoção.'),
226:('Memórias de Lissandra',['ao morrer','cemitério','defesa','ao entrar'],'Trocar criaturas em combate; juntar contadores na capital; recriar criaturas próprias como 1/1 pelas habilidades.','Sacrifício fora de combate não dá contador. Corpos grandes sem habilidades são cópias ruins.'),
227:('Última explosão',['sacrifício','ataque','ao morrer','dano'],'Usar corpos ofensivos e sacrifícios em posições avançadas; converter mortes em dano de área.','A explosão atinge aliados e capital: afastar o motor da sua retaguarda.'),
228:('Pescaria no estuário',['água','mago','criatura','ao entrar'],'Criar um terreno seguro, acumular PA no pescador e selecionar criaturas de Água para entrar naquele terreno.','Sem terreno criado a habilidade fica bloqueada; criaturas Sol e incolores não são acertos.'),
229:('Guardiã das fronteiras',['proteção','defesa','equipamento','cura'],'Equipar a Ursa e sustentar sua defesa; usar a chegada antes do combate para cobrir incursões.','Ataques sucessivos e efeitos que removem a Ursa. Teleporte só quando não existir rota.'),
230:('Duas faces da reserva',['dano','defesa','construção','mana'],'Manter mana Terra/Fogo empatada quando possível; usar PA adicional e fontes de dano não criatura.','Conta mana na reserva, não terrenos. Gastar mana pode desligar uma passiva.'),
231:('Comboio de Valeria',['veículo','voar','alcance','proteção'],'Tripular veículos e adicionar passageiros com palavras-chave explícitas; proteger o comboio e transportar tropas.','Veículo destruído mata passageiros. Texto de alcance numérico não é automaticamente palavra-chave.'),
232:('Fome de mana',['mana','defesa','equipamento','criatura'],'Desenvolver proteção e terrenos incolores; guardar mana para converter em atributos no turno de ataque.','Mana gasta em outras cartas não alimenta a habilidade; bônus acaba no fim do turno. Curva incolor é mais cara.'),
233:('O último esforço',['criatura grande','criatura','ao entrar','defesa'],'Gastar os PA do comandante com ações úteis; ao zerar, colocar uma criatura grande da mão em terreno seguro do reino.','Só uma ativação por turno; não supor que zerar PA de qualquer outra peça ativa o comandante.'),
234:('Cinzas que conjuram',['dano','feitiço','truque','equipamento'],'Garantir dano do próprio Ancestral; preparar mágicas no cemitério e reconjurar gratuitamente.','Dano de outras criaturas não ativa. A magia usada será exilada, mesmo anulada.'),
235:('Disciplina do retorno',['proteção','defesa','alcance','movimento'],'Manter PA para reflexão, escolher a peça que vai absorver dano e aproveitar alcance para forçar trocas ruins.','Reflete metade arredondada para baixo; múltiplos danos pequenos reduzem valor.'),
236:('Arquivo lunar',['magia','feitiço','truque','mago'],'Encher o cemitério com mágicas úteis e transformar cada uma em uma permissão de conjuração pelo exílio.','Ainda paga mana; proteger a capital enquanto acumula recursos. Não contar a mesma carta duas vezes.'),
237:('Pulso da lua rubra',['cura','vampiro','clérigo','sacrifício'],'Criar fontes distintas de cura e posicionar o Arauto a até três terrenos dos alvos; finalizar com disparos repetidos.','Cada evento de cura dá um disparo, não um por ponto curado. Sem cura a comandante tem pouco impacto.'),
238:('Ordem do resplendor',['cavaleiro','equipamento','proteção','formação'],'Reunir Cavaleiros com subtipo explícito nos terrenos escolhidos e proteger a concentração.','Nome de cavaleiro não basta. Área e remoção coletiva punem o agrupamento.'),
239:('Relicário de maldições',['encantamento','proteção','defesa','dano'],'Anexar encantamentos baratos e deixar 1 PA para roubar os anexos de uma vítima encantada.','A morte precisa ser causada pelo Colecionador; equipamentos não são encantamentos.'),
240:('Arsenal das feras',['equipamento','proteção','movimento','defesa'],'Conjurar equipamentos depois do comandante, gerar uma fera e equipá-la; usar Panda na defesa e outras fichas na pressão.','Equipar algo já em campo não é entrada de equipamento; remoção do comandante interrompe o motor.'),
241:('Passagens secretas',['ao entrar','proteção','equipamento','cemitério'],'Usar criaturas com boas entradas e peças de pressão; deslocar para terreno andável sem criaturas e ameaçar rotas alternativas.','Mover não é entrar em campo. Terreno ocupado impede destino e Ursa pode interceptar.'),
242:('Saque das criptas',['mill','cemitério','magia','criatura'],'Produzir descarte do topo, distribuir PA entre tropas e exilar cartas úteis para conjurar depois.','Exige carta não terreno vinda diretamente do grimório e pagamento em PA; conjuração ainda custa mana.'),
4:('A frota e os destroços',['mill','pirata','movimento','alcance'],'Abrir passagem, causar dano de criaturas às capitais e enviar cartas do topo ao cemitério; explorar a recuperação após definir sua janela.','A segunda habilidade ainda não tem frequência/custo. Não assumir recuperação ilimitada nem chamar o deck jogável.')}

def inferred(c):
    return (' '.join(c['tags']+c['subtypes'])+' '+c['text']+' '+c['kind']).lower()
def weak_reason(c,num):
    if 'Veículo' in c['subtypes'] and num!=231: return 'Exige tripulação e arrisca passageiros; compete com o motor do comandante.'
    if c['kind']=='Construção': return 'Imóvel e dependente de posição; comparar com uma tropa que avança ou gera PA.'
    if c['kind']=='Equipamento' and num!=240: return 'Custa conjuração e equipar; avaliar se compensa atrasar o comandante.'
    if c['kind']=='Encantamento' and num!=239: return 'Concentra recursos numa peça; perder o hospedeiro pode desperdiçar duas cartas.'
    if c['mana']>=5: return 'Custo alto para a contribuição ao plano; comparar com duas jogadas baratas.'
    return 'Efeito útil em poucos cenários desta estratégia; medir quantos turnos fica parado na mão.'

def score(c,keys,num):
    t=inferred(c)
    s=sum(7 for k in keys if k.lower() in t)
    if c['kind']=='Criatura': s+=4
    if c['mana']<=3: s+=4
    if c['state'].startswith('Catálogo'): s+=3
    if num==228 and c['kind']=='Criatura' and 2 in c['colors']: s+=30
    if num==225 and c['kind'] in ['Criatura','Construção','Equipamento','Encantamento','Artefato']: s+=sum(c['cost'].count(x) for x in ['S','L'])*5
    if num==233 and c['kind']=='Criatura': s+=c['mana']*2
    if num==238 and 'Cavaleiro' in c['subtypes']: s+=35
    if num==231 and 'Veículo' in c['subtypes']: s+=35
    if num==240 and c['kind']=='Equipamento': s+=35
    if num==239 and c['kind']=='Encantamento': s+=35
    if num==194 and 'Goblin' in c['subtypes']: s+=35
    return s

commanders=[c for c in cards if c['commander']]
decks=[]
for commander in commanders:
    num=int(commander['id'].split('-')[-1]); title,keys,plan,weak=strategies[num]
    identity=set(commander['colors'])
    eligible=[c for c in cards if not c['commander'] and c['kind']!='Terreno' and set(c['colors'])<=identity]
    ranked=sorted(eligible,key=lambda c:(-score(c,keys,num),c['id']))
    # Estrutura de mesa: tropas para movimento/PA, suporte e cartas situacionais.
    selected=[]
    def take(items,n):
        for c in items:
            if len([x for x in items if x in selected])>=n: break
            if c not in selected: selected.append(c)
    troops=[c for c in ranked if c['kind']=='Criatura']
    creature_target=60 if num==233 else 50 if num in [194,225,228,238] else 34 if num in [193,234,236] else 44
    take(troops,creature_target)
    if num in [193,234,236]: take([c for c in ranked if c['kind'] in ['Truque','Feitiço']],24)
    if num==231: take([c for c in ranked if 'Veículo' in c['subtypes']],10)
    if num==239: take([c for c in ranked if c['kind']=='Encantamento'],12)
    if num==240: take([c for c in ranked if c['kind']=='Equipamento'],20)
    # As últimas dez vagas dão comparação explícita com opções menos eficientes.
    for c in ranked:
        if len(selected)>=90: break
        if c['kind']=='Criatura' and sum(x['kind']=='Criatura' for x in selected)>=creature_target: continue
        if c not in selected: selected.append(c)
    for c in ranked:
        if len(selected)>=90: break
        if c not in selected: selected.append(c)
    alternatives=[c for c in ranked if c not in selected]
    weakcards=sorted(alternatives,key=lambda c:(score(c,keys,num),-c['mana'],c['id']))[:10]
    selected+=weakcards
    assert len(selected)==100,(commander['id'],len(selected))
    terrain=[]
    land_candidates=[c for c in cards if c['kind']=='Terreno' and set(c['colors'])<=identity]
    if identity:
        # 40 fontes coloridas distribuídas pelos símbolos das cartas, mais dez incolores.
        pip={i:sum(c['cost'].count(SYMBOLS[i]) for c in selected)+commander['cost'].count(SYMBOLS[i]) for i in identity}
        quotas={i:8 for i in identity}
        for _ in range(40-sum(quotas.values())):
            i=max(identity,key=lambda x:(pip[x]/(quotas[x]+1),-x)); quotas[i]+=1
        for col,n in quotas.items():
            candidates=[c for c in land_candidates if c['colors']==[col]]
            terrain+=sorted(candidates,key=lambda c:(not c['state'].startswith('Catálogo'),c['id']))[:n]
        neutral=[c for c in land_candidates if not c['colors']]
        terrain+=sorted(neutral,key=lambda c:(not c['state'].startswith('Catálogo'),c['id']))[:50-len(terrain)]
    else:
        terrain=sorted(land_candidates,key=lambda c:(not c['state'].startswith('Catálogo'),c['id']))[:50]
    ordered=sorted(selected,key=lambda c:(c in weakcards,-score(c,keys,num),c['mana'],c['id']))
    main=[dict(id=c['id'],quantity=1,assessment='Situacional / comparação' if c in weakcards else ('Núcleo de sinergia' if idx<20 else 'Apoio'),reason=weak_reason(c,num) if c in weakcards else next((f'Contribui para {k}.' for k in keys if k in inferred(c)), 'Corpo para presença/PA.' if c['kind']=='Criatura' else 'Suporte geral; avaliar custo e oportunidade.')) for idx,c in enumerate(ordered)]
    decks.append(dict(id='DECK-'+commander['id'],commander=commander['id'],name=title,plan=plan,weakness=weak,main=main,terrains=[dict(id=c['id'],quantity=1) for c in terrain],status='Teórico para avaliação; contém propostas não implementadas',alternatives=[c['id'] for c in ranked if c not in selected][:8]))

byid={c['id']:c for c in cards}
def attrs(c):
    if c['kind']=='Criatura': return f"{c['attack']}/{c['defense']}; {c['pa']} PA; mov. {c['movement']}; alcance {c['range']}"
    if c['kind']=='Construção': return f"defesa {c['defense']}; imóvel; sem dano defensivo"
    return '—'
def esc(s): return str(s).replace('|','/').replace('\n',' ')
def write(name,text): (ROOT/name).write_text(text,encoding='utf-8')
def table(lines): return '\n'.join(lines)+'\n'

write('catalogo-editorial.json',json.dumps(dict(schema='tcg-editorial-decks-v1',date='2026-10-01',notice='Não importável no motor. Aprovação, implementação e testes de jogo pendentes.',cards=cards),ensure_ascii=False,indent=2))
write('decks-editoriais.json',json.dumps(dict(schema='tcg-editorial-decklists-v1',decks=decks),ensure_ascii=False,indent=2))

for col in range(7):
    group=[c for c in cards if c['id'].startswith('PD26-') and c['colors']==([] if col==6 else [col])]
    out=[f'# Propostas 2026-10-01 - {COLORS[col]}','', 'Autoria: assistente. **Rascunhos não aprovados, não implementados e sem partidas de balanceamento.** IDs próprios; não substituem MED. Raridades e artes ainda não atribuídas. Custos/atributos são propostas ajustáveis.','', '[[Decks completos - Avaliacao 2026-10-01]]','']
    for c in group:
        out += [f"## {c['id']} — {c['name']}",'',f"{c['kind']} · {', '.join(c['subtypes']) or 'sem subtipo proposto'} · custo {c['cost']} · {attrs(c)}",'',c['text'],'']
    write(f'Propostas 2026-10-01 - {COLORS[col]}.md','\n'.join(out))

index=['# Decks completos - Avaliacao 2026-10-01','', '**Escolha do autor:** “Sem repetições, propondo cartas novas”. Cada lista contém 100 cartas principais diferentes + 50 terrenos diferentes + comandante separado. Cartas podem aparecer em decks diferentes.','', '**Estado:** montagem editorial, sem importação no Unity. A validação de listas não comprova equilíbrio, implementação ou partidas jogáveis. Os textos originais permanecem em suas fontes. Os protótipos anteriores não foram substituídos.','', '## Como avaliar','', 'Comece por um deck, leia seu núcleo e compare as dez opções situacionais. “Mais forte” aqui significa maior sinergia prevista, não raridade nem resultado medido. Registre aprovar/ajustar/rejeitar pelo ID; aprovar uma carta compartilhada vale para todos os decks que a usam.','', 'Os 100 principais incluem deliberadamente dez opções menos eficientes para comparação. Não são listas otimizadas. A seleção inicial usa afinidade de efeitos e composição por tipos; refinamento manual e partidas continuam necessários.','', '## Convenções das propostas','', '- S Sol, L Lua, G Água, F Fogo, A Ar, T Terra; número é custo genérico. Incolor não significa poder pagar símbolos de outras cores.\n- Adjacentemente = ortogonal, sem o próprio terreno. Movimentos respeitam terrenos andáveis e bloqueios; não iniciam ataques.\n- Equipamentos se anexam no mesmo terreno, ficam nele se o hospedeiro morrer. Encantamentos “encante” ficam anexados; destino após morte segue regras a validar antes de importar.\n- Bônus temporários expiram como escrito. Limites “uma vez no seu turno” são por cópia; trocar controle não renova uso no mesmo turno.\n- Terrenos propostos geram uma mana pela rotina normal. Sua habilidade adicional custa mana e exige condição; não é geração adicional automática. O controlador usa a habilidade; o dono original conserva o destino no cemitério.\n- Os subtipos dos rascunhos antigos não foram inventados. Não conte um nome como Cavaleiro/Goblin sem subtipo explícito.\n- Raridade, arte e balanceamento das propostas aguardam revisão; nenhuma proposta concede propriedade na coleção.','', '## Terrenos e pilhas compartilhadas','', 'A carta inicial da capital vem do próprio deck; os vizinhos iniciais são ruínas. As quatro pilhas centrais misturam contribuições e a reposição vem do jogador do turno. Portanto a distribuição abaixo é oferta do deck, não garantia de acesso às suas cores ou terrenos especiais. Priorize fontes coloridas no topo durante o teste; registre mana travada. Portões aparecem no máximo uma vez por deck e dependem de outro Portão disponível na mesa.','', 'Nos decks coloridos, aproximadamente 40 fontes coloridas e dez terrenos neutros; divisão acompanha os símbolos das listas. Os terrenos novos têm utilidades locais para respeitar singleton sem renomear terrenos básicos. É um volume alto de efeitos para revisar: avaliar primeiro a simplicidade e a carga de leitura.','', '## Listas','', '| Comandante | Deck | Principais / terrenos | Propostas novas na lista |','|---|---|---|---|']
for d in decks:
    cmd=byid[d['commander']]; maincards=[byid[x['id']] for x in d['main']]; lands=[byid[x['id']] for x in d['terrains']]
    filename=f"{d['id']} - {d['name']}.md"; d['file']=filename
    curve=collections.Counter('7+' if c['mana']>=7 else str(c['mana']) for c in maincards)
    kinds=collections.Counter(c['kind'] for c in maincards)
    states=collections.Counter(c['state'] for c in maincards+lands)
    out=[f"# {d['name']}",'',f"**Comandante:** {cmd['name']} ({cmd['id']}) · {' / '.join(COLORS[i] for i in cmd['colors']) or 'Incolor'} · {cmd['cost']} · {attrs(cmd)}",'',cmd['text'],'', '**Estado:** '+d['status']+'. 100 principais + 50 terrenos + comandante; uma cópia de cada identidade.','', '[[Decks completos - Avaliacao 2026-10-01]] · [[Validacao editorial dos decks 2026-10-01]]','', '## Plano e pontos fracos','',d['plan'],'',d['weakness'],'', 'A abertura deve buscar fontes de mana e uma criatura barata; o desenvolvimento estabelece a sinergia acima; o encerramento exige pressão real sobre capitais. Terrenos oferecidos às pilhas podem ser usados por adversários. Não há garantia de curva de abertura.','', '## Composição','', 'Tipos: '+', '.join(f'{k}: {v}' for k,v in sorted(kinds.items()))+'.','', 'Curva (custo total → quantidade): '+', '.join(f'{k}: {curve[k]}' for k in ['0','1','2','3','4','5','6','7+'])+'.','', 'Estado das 150 cartas: '+ '; '.join(f'{k}: {v}' for k,v in states.items())+'.','', '## Lista principal','', '| ID | Carta | Custo | Tipo / subtipos | Avaliação | Função prevista |','|---|---|---|---|---|---|']
    for entry in d['main']:
        c=byid[entry['id']]
        out.append('| '+' | '.join(map(esc,[c['id'],c['name'],c['cost'],c['kind']+(' / '+', '.join(c['subtypes']) if c['subtypes'] else ''),entry['assessment'],entry['reason']]))+' |')
    out += ['', '## Deck de terrenos vinculado','', '| ID | Terreno | Identidade |','|---|---|---|']
    for c in lands: out.append(f"| {c['id']} | {c['name']} | {' / '.join(COLORS[i] for i in c['colors']) or 'Incolor'} |")
    out += ['', '## Comparação antes do teste','', 'As dez últimas vagas são o grupo situacional. Sua menor afinidade não torna a carta universalmente ruim. Corte primeiro as que não contribuírem em partidas reais; mantenha a proporção de tropas e fontes de mana.','']
    for entry in d['main'][-5:]:
        c=byid[entry['id']]
        out.append(f"- {c['name']} ({c['id']}): custo {c['cost']}; {entry['reason']}")
    if d['alternatives']: out += ['', 'Reservas candidatas, fora dos 100: '+', '.join(f"{byid[i]['name']} ({i})" for i in d['alternatives'])+'.']
    out += ['', '## Textos e avaliação','', 'Textos completos, atributos, estado e fonte estão em `catalogo-editorial.json` e na consulta `avaliar-decks.html`. As propostas também estão nas notas Propostas 2026-10-01 por cor.','', '- Aprovar / ajustar / rejeitar núcleo: pendente.\n- Cartas que ficaram sem uso, mana insuficiente e turnos para conjurar comandante: não medidos.\n- Interações, equilíbrio entre decks e multiplayer: não testados.\n- Integração: somente após aprovação dos textos e implementação das mecânicas ausentes.','']
    write(filename,'\n'.join(out))
    index.append(f"| {cmd['name']} | [[{filename[:-3]}|{d['name']}]] | 100 / 50 | {sum(c['id'].startswith('PD26-') for c in maincards+lands)} |")
index += ['', '## Catálogo e fontes','',f"Catálogo compartilhado: {len(cards)} definições; {sum(c['id'].startswith('PD26-') for c in cards)} propostas novas do assistente. Nem toda carta do catálogo está selecionada: as restantes são opções de troca.",'', '[[Ideias autorais - Frota e Apoios 2026-10-01]] · [[Validacao editorial dos decks 2026-10-01]]','']
index += [f'- [[Propostas 2026-10-01 - {c}]]' for c in COLORS]
index += ['', 'Arquivos: `catalogo-editorial.json`, `decks-editoriais.json`, `avaliar-decks.html`. `gerar_decks.py` reconstrói as listas a partir das fontes e propostas; antes de regenerar após revisão, preserve a versão anterior. Dados editoriais não têm o contrato de importação do Unity.','']
write('Decks completos - Avaliacao 2026-10-01.md','\n'.join(index))
write('decks-editoriais.json',json.dumps(dict(schema='tcg-editorial-decklists-v1',decks=decks),ensure_ascii=False,indent=2))

# Consulta offline: textos completos ao selecionar um deck, busca e situação.
payload=json.dumps(dict(cards=cards,decks=decks),ensure_ascii=False).replace('</','<\\/')
page='''<!doctype html><html lang="pt-BR"><meta charset="utf-8"><title>Oficina de decks — TCG</title><style>body{margin:2rem;background:#171c20;color:#eee;font:16px system-ui}h1{color:#e3c48b}select,input{padding:.7rem;margin:.4rem;background:#283239;color:white;border:1px solid #87908c;max-width:95%}article{border:1px solid #6f6550;border-radius:8px;padding:1rem;background:#222a2d}#cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(300px,1fr));gap:12px}small{color:#bfc9c6}.tag{color:#edc57d}#intro{max-width:1000px;line-height:1.5}button{padding:.5rem}</style><h1>Oficina de decks</h1><p>29 listas teóricas • 100 cartas + 50 terrenos • sem repetições</p><p>Propostas aguardam aprovação e implementação. Esta consulta não modifica sua coleção.</p><select id="deck"></select><select id="zone"><option value="main">Principal</option><option value="terrains">Terrenos</option></select><input id="search" placeholder="Buscar nome, subtipo, texto ou ID" aria-label="Buscar cartas"><div id="intro"></div><p id="count"></p><div id="cards"></div><script>const DATA=PAYLOAD;const byId=Object.fromEntries(DATA.cards.map(c=>[c.id,c]));const sel=document.querySelector('#deck');for(const d of DATA.decks){const o=document.createElement('option');o.value=d.id;o.textContent=byId[d.commander].name+' — '+d.name;sel.append(o)}const esc=x=>String(x??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));function draw(){const d=DATA.decks.find(x=>x.id===sel.value);const cmd=byId[d.commander];document.querySelector('#intro').innerHTML='<h2>'+esc(d.name)+'</h2><p>'+esc(cmd.text)+'</p><p>'+esc(d.plan)+'</p><p><strong>Ponto fraco:</strong> '+esc(d.weakness)+'</p>';const q=document.querySelector('#search').value.toLocaleLowerCase();const list=d[document.querySelector('#zone').value].filter(e=>JSON.stringify(byId[e.id]).toLocaleLowerCase().includes(q));document.querySelector('#count').textContent=list.length+' cartas exibidas';document.querySelector('#cards').innerHTML=list.map(e=>{const c=byId[e.id];return '<article><small>'+esc(c.id)+' • '+esc(e.assessment||'Terreno')+'</small><h3>'+esc(c.name)+'</h3><p class="tag">'+esc(c.kind+' — '+c.subtypes.join(', ')+' • Custo '+c.cost)+'</p>'+(c.attack!=null?'<p>'+esc(c.attack+'/'+c.defense+' • '+c.pa+' PA • Movimento '+c.movement+' • Alcance '+c.range)+'</p>':c.defense!=null?'<p>Defesa '+esc(c.defense)+'</p>':'')+'<p>'+esc(c.text)+'</p><small>'+esc(c.state)+'<br>Fonte: '+esc(c.source)+'</small></article>'}).join('')}sel.onchange=draw;document.querySelector('#zone').onchange=draw;document.querySelector('#search').oninput=draw;draw();</script></html>'''.replace('PAYLOAD',payload)
write('avaliar-decks.html',page)
print(json.dumps(dict(cards=len(cards),proposals=sum(c['id'].startswith('PD26-') for c in cards),decks=len(decks)),ensure_ascii=False))
