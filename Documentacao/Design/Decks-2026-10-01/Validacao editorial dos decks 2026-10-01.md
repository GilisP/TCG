# Validacao editorial dos decks 2026-10-01

**28896 verificações estruturais aprovadas.** 29 listas; 2.900 vagas principais e 1.450 vagas de terrenos. Cada deck: 100 + 50, sem repetição de identidade; comandante separado. Não houve execução do motor ou partidas para validar estes decks.

[[Decks completos - Avaliacao 2026-10-01]]

Verificado por `validar_decks.py`: referências, contagens, quantidades, identidade de cor, separação por tipo, ausência da MED-147 bloqueada, cobertura dos comandantes, custos legíveis, atributos de criaturas, origem/estado, fidelidade de nomes e textos do catálogo do jogo, dez escolhas situacionais e vinte cartas de núcleo por lista, links internos das 29 listas e presença na consulta offline.

Propostas novas: 570. A verificação também rejeita propostas com a mesma combinação de custo, cor, tipo, texto, subtipos e atributos. Isso evita duplicações exatas, mas não substitui análise de semelhança de design, variedade ou equilíbrio.

Arquivos e hashes em `validacao-editorial.json`. Consulta HTML autocontida, sem dependências de rede; interação visual não foi certificada por este validador.

## Limitações e próximos testes

- Classificação forte/situacional é estimativa editorial. A seleção inicial usa afinidade e metas por tipo, não um simulador de partidas.
- As propostas novas e os rascunhos antigos ainda exigem aprovação e implementação. O comandante pirata tem uma habilidade incompleta; três apoios autorais aguardam esclarecimentos e não preenchem vagas das listas.
- Não há comprovação de equilíbrio entre comandantes, curva de abertura, economia, desempate de gatilhos ou compatibilidade online dessas propostas.
- Parte considerável dos decks monocolores é incolor. Avaliar se essa base compartilhada atende à identidade desejada; substituir apoios genéricos por propostas específicas após os primeiros testes.
- Terrenos são compartilhados durante a partida. A proporção de cores do deck não garante acesso à mana; testar distribuição com dois, três e quatro jogadores após a integração.
- Próximo passo de conteúdo: revisar um deck de cada estilo e os terrenos; registrar decisões por ID antes de importar qualquer lote. Prioridades gerais continuam em [[04 - Pendencias de Implementacao]].

Nenhum script C#, cena, asset, perfil de coleção ou deck salvo do jogador foi alterado nesta entrega. Compilação Unity não se aplica a estes documentos editoriais.

## Consulta offline

`validar_consulta.cjs` executado com Node: 29 opções de comandante, 58 combinações de lista principal/terrenos, contagem de fichas e busca com e sem resultados passaram em DOM simulado. Isso verifica a lógica JavaScript, não aparência ou cliques num navegador real.
