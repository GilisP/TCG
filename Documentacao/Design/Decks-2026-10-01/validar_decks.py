"""Validação estrutural editorial, não executa regras/partidas Unity."""
from pathlib import Path
import json, re, collections, hashlib

root=Path(__file__).resolve().parent
cards=json.loads((root/'catalogo-editorial.json').read_text(encoding='utf-8'))['cards']
decks=json.loads((root/'decks-editoriais.json').read_text(encoding='utf-8'))['decks']
byid={c['id']:c for c in cards}
checks=0
def check(ok,why):
    global checks
    checks+=1
    if not ok: raise AssertionError(why)
check(len(byid)==len(cards),'IDs duplicados')
check(len({c['name'].casefold() for c in cards})==len(cards),'Nomes duplicados: verificar reimpressões')
check(len(decks)==29,'Esperados 29 decks')
check(len({d['commander'] for d in decks})==29,'Comandantes repetidos')
check({d['commander'] for d in decks}=={c['id'] for c in cards if c['commander']},'Cobertura de comandantes')
signatures={}
for c in cards:
    check(bool(re.fullmatch(r'[SLGFAT0-9]+|—',c['cost'])),f"Custo inválido {c['id']}: {c['cost']}")
    check(bool(c['name'] and c['text'] and c['source'] and c['state']),f"Campos vazios {c['id']}")
    if c['kind']=='Criatura':
        check(all(c[k] is not None for k in ['attack','defense','pa','movement','range']),f"Atributos ausentes {c['id']}")
    if c['id'].startswith('PD26-'):
        sig=json.dumps([c[k] for k in ['colors','kind','cost','text','subtypes','attack','defense','pa','movement','range']],ensure_ascii=False,sort_keys=True)
        check(sig not in signatures,f"Propostas funcionalmente idênticas: {c['id']} / {signatures.get(sig)}")
        signatures[sig]=c['id']
usage=collections.Counter()
for d in decks:
    cmd=byid[d['commander']]
    for zone,size in [('main',100),('terrains',50)]:
        entries=d[zone]
        check(len(entries)==size,f"Tamanho {d['id']} {zone}")
        check(len({e['id'] for e in entries})==size,f"Repetição {d['id']} {zone}")
        for e in entries:
            check(e['id'] in byid,f"Referência inexistente {e['id']}")
            c=byid[e['id']];usage[c['id']]+=1
            check(e['quantity']==1,f"Mais de uma cópia: {d['id']} {c['id']}")
            check(not c['commander'],f"Comandante dentro do deck {c['id']}")
            check((c['kind']=='Terreno')==(zone=='terrains'),f"Zona inválida {c['id']}")
            check(set(c['colors'])<=set(cmd['colors']),f"Identidade incompatível {d['id']} {c['id']}")
            check(c['id']!='MED-147', 'Carta bloqueada incluída')
    check(sum(e['assessment']=='Situacional / comparação' for e in d['main'])==10,f"Comparações {d['id']}")
    check(sum(e['assessment']=='Núcleo de sinergia' for e in d['main'])==20,f"Núcleo {d['id']}")
    check((root/d['file']).exists(),f"Nota ausente {d['file']}")

# Fidelidade às fontes importadas: textos e nomes não foram reinventados.
game=Path('C:/Users/gil/TCG')
for c in cards:
    if c['source'].startswith('Assets/'):
        source=json.loads((game/c['source']).read_text(encoding='utf-8-sig'))
        original=next(x for x in source['cards'] if x['id']==c['id'])
        check((original['name'],original.get('text',''))==(c['name'],c['text']),f"Fonte alterada {c['id']}")

# Referências das listas no índice e IDs na consulta offline.
index=(root/'Decks completos - Avaliacao 2026-10-01.md').read_text(encoding='utf-8')
page=(root/'avaliar-decks.html').read_text(encoding='utf-8')
for d in decks:
    check(d['file'][:-3] in index,f"Link de deck ausente {d['id']}")
    check(d['id'] in page,f"Deck ausente da consulta {d['id']}")

report={'checksPassed':checks,'decks':len(decks),'mainSlots':2900,'terrainSlots':1450,'newProposals':sum(c['id'].startswith('PD26-') for c in cards),'uniqueSelectedCards':len(usage),'runtimeChanged':False,'gameplayTested':False,'filesSha256':{name:hashlib.sha256((root/name).read_bytes()).hexdigest() for name in ['catalogo-editorial.json','decks-editoriais.json','avaliar-decks.html']}}
(root/'validacao-editorial.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
(root/'Validacao editorial dos decks 2026-10-01.md').write_text(f'''# Validacao editorial dos decks 2026-10-01

**{checks} verificações estruturais aprovadas.** 29 listas; 2.900 vagas principais e 1.450 vagas de terrenos. Cada deck: 100 + 50, sem repetição de identidade; comandante separado. Não houve execução do motor ou partidas para validar estes decks.

[[Decks completos - Avaliacao 2026-10-01]]

Verificado por `validar_decks.py`: referências, contagens, quantidades, identidade de cor, separação por tipo, ausência da MED-147 bloqueada, cobertura dos comandantes, custos legíveis, atributos de criaturas, origem/estado, fidelidade de nomes e textos do catálogo do jogo, dez escolhas situacionais e vinte cartas de núcleo por lista, links internos das 29 listas e presença na consulta offline.

Propostas novas: {report['newProposals']}. A verificação também rejeita propostas com a mesma combinação de custo, cor, tipo, texto, subtipos e atributos. Isso evita duplicações exatas, mas não substitui análise de semelhança de design, variedade ou equilíbrio.

Arquivos e hashes em `validacao-editorial.json`. Consulta HTML autocontida, sem dependências de rede; interação visual não foi certificada por este validador.

## Limitações e próximos testes

- Classificação forte/situacional é estimativa editorial. A seleção inicial usa afinidade e metas por tipo, não um simulador de partidas.
- As propostas novas e os rascunhos antigos ainda exigem aprovação e implementação. O comandante pirata tem uma habilidade incompleta; três apoios autorais aguardam esclarecimentos e não preenchem vagas das listas.
- Não há comprovação de equilíbrio entre comandantes, curva de abertura, economia, desempate de gatilhos ou compatibilidade online dessas propostas.
- Parte considerável dos decks monocolores é incolor. Avaliar se essa base compartilhada atende à identidade desejada; substituir apoios genéricos por propostas específicas após os primeiros testes.
- Terrenos são compartilhados durante a partida. A proporção de cores do deck não garante acesso à mana; testar distribuição com dois, três e quatro jogadores após a integração.
- Próximo passo de conteúdo: revisar um deck de cada estilo e os terrenos; registrar decisões por ID antes de importar qualquer lote. Prioridades gerais continuam em [[04 - Pendencias de Implementacao]].

Nenhum script C#, cena, asset, perfil de coleção ou deck salvo do jogador foi alterado nesta entrega. Compilação Unity não se aplica a estes documentos editoriais.
''',encoding='utf-8')
print(json.dumps(report,ensure_ascii=False))
