# Coleção Oficial 01 — integração de 2026-10-09

Estado: design dos dez decks aprovado pelo autor; integração em andamento. Não confundir aprovação editorial com cartas executáveis ou verificadas.

## Fontes e identidades
Fonte aprovada: Projetos decks/Revisao tematica - 2026-10-08, na Área de Trabalho. Snapshot preservado em C:/Users/gil/TCG/Documentacao/Design/COL-001-20261009. Dez listas: 100 cartas principais + 50 terrenos + comandante separado. Principais: 50 comuns, 20 incomuns, 15 raras, 15 míticas; terrenos: 25/10/8/7.

O autor autorizou criar nomes novos para versões revisadas e para terrenos temáticos com nomes iguais e efeitos diferentes, preservando todos os efeitos. identity-resolutions.json registra origem por arquivo/posição e identidade proposta. Cartas antigas permanecem preservadas. Não substituir globalmente definições anteriores.

## Regras confirmadas pelo autor
- Refinar o plano: troca as posições das duas criaturas próprias e substitui a participante do combate; cada uma mantém seu dano.
- Vendedor de mapas: compra ao colocar sob seu controle o terreno marcado.
- Persoadir guerreiros: controle permanente; move ao terreno andável mais próximo do reino sem atravessar bloqueios; fica no lugar se não houver caminho.
- Jonas: PA são a soma do maior custo de mana de criatura em cada cemitério.
- Indestrutível impede morte por dano e destruição, mas não sacrifício, exílio nem defesa zero.
- Fabrica dos Horrores conta mortes de qualquer criatura; ficha com 1 PA/movimento 2 e capital escolhida pelo controlador.
- Reino dos mortos devolve a criatura ao cemitério no fim do turno.
- Documento falso sorteia ataque ou defesa no início de cada turno do controlador, dobrando o atributo atual até o próximo sorteio.
- Ondas gigantes: causa 3 de dano às criaturas, construções e capital do tile selecionado; depois empurra peças nos tiles ortogonalmente adjacentes até 2 tiles para longe do centro, parando antes de buracos ou bloqueios. Terrenos/capitais não se movem; equipamentos anexados acompanham a peça.

## Implementação e validação
Tools/import_official_collection.py é o importador em desenvolvimento. A primeira conversão identifica 728 definições novas; 685 ainda exigem tradução e verificação de mecânicas. Não liberar esses dez decks como jogáveis antes de concluir o suporte. A compilação Python da conversão não comprova funcionamento no Unity.

Próximo passo: implementar famílias de efeitos, conferir dados completos e integrar dez listas ao fluxo de escolha/importação, com testes do motor e rede. Não substituir os 29 decks planejados antigos nem perfis pessoais. Execução prioritária em [[04 - Pendencias de Implementacao]].

## Decisão confirmada — 2026-10-10

Senhor das Catacumbas Despertas: a criatura reanimada aparece no terreno do Senhor, conforme resposta explícita do autor. Os dez precons foram integrados; a tradução inicial incompleta descrita acima é histórica. Ver [[Precons Oficiais - Implementacao 2026-10-10]].
