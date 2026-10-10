# Coordenação de desenvolvimento — piloto AG-001
Data: 2026-10-10. Coordenador: agente raiz desta conversa. Projeto principal: C:/Users/gil/TCG. Fonte de regras: vault C:/Users/gil/OneDrive/Área de Trabalho/TCG, decisões confirmadas e mapa de vigência.

## Contrato antes da execução paralela
Piloto: verificar a escolha de deck com comandante já implementada, incluindo recusa de decks inválidos e consistência online. Nenhuma regra nova. Dois especialistas com contexto separado; testes/integração ficam com coordenador. PvC não integra este piloto.
- AG-001-R, regras/cartas: somente Assets/TCG/Foundation/Editor/CoordinationRulesChecks.cs e seu .meta; relatório Documentacao/Desenvolvimento/AG-001-R-result.json. Validar contrato CollectionLibrary.Validate, rascunhos e identidade/cópia dos decks. Criar public static void Run() na namespace TCG.Foundation.Editor; registrar COORDINATION RULES CHECKS PASSED: N. Não executar Unity.
- AG-001-N, multiplayer: somente Assets/TCG/Foundation/Editor/CoordinationNetworkChecks.cs e seu .meta; relatório Documentacao/Desenvolvimento/AG-001-N-result.json. Validar NetworkRoom.Receive no fluxo de deck/prontidão, duplicação, reconexão, autoridade e privacidade. Criar public static void Run(); registrar COORDINATION NETWORK CHECKS PASSED: N. Não executar Unity.
- Coordenador: FoundationSetup.cs, runner compartilhado, documentação/vault/AGENTS, controle de Unity/Git, validação conjunta. Qualquer correção em Core/Runtime exige que o especialista envie proposta ao coordenador; não sair do escopo.

Formato de retorno JSON: taskId, status (completed/blocked/needs-integration), summary, filesChanged[], checksPerformed[], blockers[], assumptions[]. Completed no especialista significa artefato entregue; conclusão da tarefa raiz depende da validação integrada.

Git: backup inicial 7d5f87d enviado a origin/main. Trabalho anterior preservado. Os dois escopos de escrita são disjuntos, sem alteração compartilhada: checkout principal compartilhado é permitido neste piloto. Se o escopo exigir arquivos compartilhados, parar e solicitar repartição ao coordenador; usar worktrees/branches isolados em tarefas com conflito real. Nenhum especialista faz commit/push ou abre Editor. Não usar force push.

Contratos existentes a preservar: Command(player,revision,kind,...); Match.Try é autoridade; projeções Match.FromView não executam regras; RoomRequest(version,sequence,type,command/deck/token); NetworkRoom valida assento/sequência/revisão; MatchView por assento esconde mão/deck/seed alheios. Testes de Match usam semente fixa e NetworkRoom relógio injetável. Seed da criação online continua privada e aleatória.

Retomada: ler checkpoint.json, depois taskId/contrato e relatório específico; não transmitir todo histórico. Em caso de compactação, registrar resultados, arquivos em progresso, próximo comando e limitações antes de continuar.
