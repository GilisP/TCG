# Agentes de Desenvolvimento — Arquitetura e Contratos

Atualizado em 2026-10-10. [[00 - Plano Mestre]] · [[04 - Pendencias de Implementacao]] · [[Coordenacao - Piloto e Retomada 2026-10-10]].

## Organização ajustada ao projeto
O coordenador é o agente da conversa principal. Especialistas são criados por tarefa, com contexto focado e encerrados ao entregar; não são serviços permanentes nem personagens adversários. Dois especialistas simultâneos são suficientes para o piloto. Testes/integração são responsabilidade do coordenador em tarefas pequenas. Não há novo framework de agentes dentro do jogo.

| Área / responsável quando acionado | Módulos existentes e fronteira de escrita |
|---|---|
| Regras e cartas | Foundation/Core: Match e seus partials, Content/EffectRegistry/CardAbilities, tipos, custos, zonas, vitória. Compiladores Tools/import_* e dados StreamingAssets/Expansions somente com atribuição explícita. |
| Tabuleiro e interface | Foundation/Runtime: TableView, TableWorld, DragMovement, HandPresentation, CommanderUI, CardAppearanceUI, TableFlights; cena Mesa.unity e assets somente com dono explícito. Apresenta ações e feedback. |
| Multiplayer | Core/NetworkRoom e NetworkView; Runtime/NetworkSession, NetworkUI, NetworkDiagnostics. Host executa regras, transporte entrega pedidos/projeções. Alterações em Match/Content continuam sob dono de regras. |
| Persistência / conteúdo | Core/CollectionLibrary, PlannedDecks, Boosters, PlayerProgress; Runtime/CollectionStore e telas. É escopo de uma tarefa, não exige agente permanente adicional. |
| Coordenador / integração | Contratos, FoundationSetup, assembly definitions, manifesto de pacotes, versão de protocolo, índices/vault, revisão Git, processos Unity e testes conjuntos. |
| PvC futuro, apenas quando pedido | Consumirá Command e consultas de ações legais do mesmo motor; conhecimento limitado ao assento. Nenhuma IA adversária foi criada por esta organização. |

## Arquitetura e tecnologias identificadas
Unity 6000.3.6f1; C#. Assembly TCG.Foundation não referencia Unity (noEngineReferences), TCG.Table referencia motor/Unity/rede e TCG.Foundation.Editor contém verificações. Mesa atual gerada em Play em Assets/TCG/Scenes/Mesa.unity. Assets/TCG/Scripts e Partida.unity são legado preservado.

Apresentação 3D procedural com cartas 2D e HUD IMGUI. Projeto inclui URP 17.3.0, Input System 1.18.0 e UGUI 2.0.0; presença do pacote não significa que toda interface os utiliza. FoundationSetup.Build configura perfil built-in para o executável da mesa. Catálogo/coleções por JSON em StreamingAssets; Python nos compiladores editoriais. Persistência local JSON esquema 3, sem economia autenticada central.

NGO 2.13.3, Unity Transport e MPS SDK 2.3.3; LAN UDP e internet UGS/Relay. NetworkRoom controla autoridade; NetworkSession adapta transporte/serviços. Fingerprint atual tcg-network-6/20261005-required-commander. Testes do piloto não mudam protocolo. Build/compilação via Editor batch; logs e diagnósticos próprios, além dos pacotes de testes instalados.

## Fonte única e contratos
Decisões e regras do vault são fonte de design, com precedência em [[05 - Mapa de Fontes e Vigencia]]. Match é a execução autoritativa dessas regras. Não copiar validação para UI/rede/PvC; requisitos de transporte (tamanho, conexão, sequência) não substituem validação de ação.

| Contrato | Conteúdo / consumidor |
|---|---|
| Command → Match.Try | player/revision/kind e parâmetros de alvo, carta, unidades. Sucesso altera estado/revisão; falha retorna motivo. UI local usa o motor; online envia ao host. |
| RoomRequest → NetworkRoom.Receive | version=1, type, sequence; deck/ready/command/token. Host verifica assento, ordem e regra. Uma mensagem repetida não deve repetir a ação. |
| MatchView por assento → Match.FromView | Estado público e mão/opções do próprio jogador; ações legais calculadas no host. Projeções não executam Try. Nunca transmitir decks/seed/mãos privadas a outros. |
| MatchEvent → visual / NetVisual | Apresentação consome eventos; animação não decide movimento/dano. |
| CardData / Definition / Piece | Dados editoriais, definição imutável e estado de cópia em partida separados. IDs/OriginalOwner preservados. |
| DeckData / CollectionStore | Principal/terrenos/comandante separados; inventário e cosméticos persistidos sem reinicializar saldo. |

Testes diretos de Match recebem semente fixa; NetworkRoom recebe relógio injetável para reconexão. A sala real continua gerando seed privada aleatória; não expor seed no cliente. PvC futuro precisa de fachada de observação/ações do assento usando os mesmos comandos; não acessar mão inimiga nem implementar regras próprias.

## Procedimento por tarefa
1. Coordenador lê checkpoint, fila central e notas necessárias; define ID, objetivo, critério, dependências, contratos e dono de cada caminho antes de delegar.
2. Envia só briefing, restrições, referências relevantes e contrato. Usar contexto separado; não encaminhar histórico completo. Agente consulta detalhes sob demanda.
3. Especialistas independentes executam simultaneamente. Se precisar de arquivo fora da lista, reportam ao coordenador; não editam por conta própria.
4. Entrega contém status, resumo, caminhos alterados, verificações realmente executadas, bloqueios e hipóteses. Código escrito não significa teste aprovado.
5. Coordenador revisa diff, integra runner/dados, compila e testa comportamento conjunto. Concluído somente após integrar/verificar. Resultados negativos geram correção e repetição proporcional.
6. Atualiza notas, fila central e checkpoint antes de pausa, compactação ou reinício. O checkpoint basta para recuperar objetivo/estado; detalhes ficam em arquivos por tarefa.

## Git, assets e recursos compartilhados
Backup no início de dias com modificações, conforme preferência vigente, sem force-push. Preservar trabalho anterior. Um único dono para FoundationSetup, protocolo, pacote, asmdef, catálogo compartilhado, cenas/metas e índices. Editor/build/processos Unity só pelo coordenador.

Piloto permite checkout compartilhado porque cada especialista escreve um arquivo de teste novo e relatório próprio; coordenador é único escritor dos compartilhados. Quando duas tarefas exigirem o mesmo recurso ou isolamento de mudanças relevantes, usar branches/worktrees e integrar um de cada vez com revisão e validação conjunta. Worktrees evitam conflito de arquivos, mas não dispensam testar a combinação. Preservar metas existentes; Unity cria metas dos arquivos novos.

## Registro persistente e retomada
Planejamento em [[10 - Coordenacao de Desenvolvimento]]. A fila prioritária continua sendo [[04 - Pendencias de Implementacao]]. Documentacao/Desenvolvimento guarda espelho de contratos, tasks.json, checkpoint.json e relatórios por ID para leitura curta; tasks.json representa pacotes desta execução e não uma fila paralela de prioridades.

Estado da coleção oficial: fontes e identidades preservadas em [[Colecao Oficial 01 - Integracao 2026-10-09]], geração ainda fora do catálogo ativo; este piloto não conclui os 685 efeitos então pendentes. Não extrapolar resultado do fluxo de decks para toda a coleção.
