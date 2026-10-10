# Coordenação — Piloto e Retomada 2026-10-10

[[Agentes de Desenvolvimento - Arquitetura e Contratos]] · [[10 - Coordenacao de Desenvolvimento]] · [[04 - Pendencias de Implementacao]].

## Pedido e decisão
O autor autorizou trabalho coordenado, especialistas com contextos separados e execução paralela. Adaptamos ao motor modular existente: dois especialistas e integração/testes sob o coordenador; interface ou PvC são papéis acionados quando houver tarefas próprias. A organização não cria camadas novas no runtime.

## Piloto AG-001
Funcionalidade existente escolhida: seleção de deck com comandante, já aprovada em [[Comandante Obrigatorio e Escolha de Decks - 2026-10-05]]. Escopo pequeno e regras definidas permitem demonstrar coordenação sem misturar o lote amplo da coleção oficial.

| ID | Objetivo / dono | Escrita atribuída | Dependências / critério |
|---|---|---|---|
| AG-001-R | Verificar deck/comandante, regras/cartas | CoordinationRulesChecks.cs/.meta e relatório R | Sem dependência N; teste conjunto aprovado e perfil pessoal preservado |
| AG-001-N | Verificar seleção/prontidão, replay/reconexão e consistência final, multiplayer | CoordinationNetworkChecks.cs/.meta e relatório N | Sem dependência R; host rejeita inválidos, protege dados e projeta resultado consistente |
| AG-001-I | Integrar, executar e registrar, coordenador | FoundationSetup, CoordinationPilotChecks, documentação, checkpoint e ferramenta de retomada | Depende R/N; compilação + quatro suítes aprovadas e registro recuperável |

Escopos definidos antes dos dispatches. Os dois especialistas foram iniciados sem aguardar a entrega do outro, com fork_turns=none e briefings focados. Nenhum recebeu o histórico completo. Ambos produziram arquivos disjuntos em paralelo; somente o coordenador escreveu o runner compartilhado. Relatórios vieram como needs-integration, sem alegar execução Unity pelo especialista.

Git: snapshot do trabalho editorial anterior 7d5f87d enviado antes das modificações do dia. Checkout compartilhado escolhido porque as listas de escrita são disjuntas; conflito de escopo exigiria reatribuição ou worktrees/branches. Nenhuma edição de Core/Runtime/StreamingAssets neste piloto. Metas antigos preservados.

## Contrato integrado
Coordenador ligou ambas as suites em FoundationSetup.Prepare para builds futuros e criou TCG → Base → Validar piloto de coordenacao. Entry point batch: TCG.Foundation.Editor.CoordinationPilotChecks.Run. Chama CollectionChecks e NetworkChecks existentes, depois os dois novos testes. Nenhuma nova assinatura de Command/RoomRequest/MatchView, protocolo ou economia.

## Execução
Primeira execução: Unity principal 6000.3.6f1 compilou e aprovou 430 verificações (Collection 19, Network 117, novas regras 36, nova rede 258). Encerramento batch com código 0. Depois acrescentamos fechamento da partida por concessões autenticadas e consistência do resultado final em todos os assentos; rodada final concluída com 445 verificações aprovadas (19 + 117 + 36 + 273), incluindo 309 novas; código de saída 0. Evidência Logs/coordination-pilot-final-20261010.log. Integração AG-001 concluída; nenhum teste falhou nas duas rodadas Unity.

## Retomada demonstrada
Contratos, tarefas, fontes e próximo comando foram gravados em Documentacao/Desenvolvimento/checkpoint.json e tasks.json. Tools/Resume-Development.ps1 foi executado por novo powershell.exe -NoProfile: recuperou três tarefas, responsáveis, estados, limites e o próximo passo só pelos registros persistidos. A correção da leitura de arrays no PowerShell 5 foi repetida com sucesso. Saída em Logs/coordination-resume-20261010.txt. Esta é demonstração de recuperação por arquivos em processo novo, não alegação de que houve compactação real do chat.

Para retomar, executar a ferramenta, ler contrato e somente relatório/notas da tarefa escolhida. Antes de compactar ou pausar, atualizar checkpoint com evidências, modificações em progresso, bloqueios e próximo passo. Mensagens antigas não devem ser a única fonte.

## Limites e próximo uso
Testes em memória com relógio injetado, catálogo de produção nas regras e projeções serializadas por assento. Não houve mudança de transporte nem novo teste Relay/LAN em processos físicos; avaliação visual/Play Mode e redes distintas continuam nas pendências históricas. O piloto não completa a coleção oficial nem implementa adversário PvC.

Próximo uso recomendado: aplicar o contrato às famílias de habilidades da coleção oficial, escolhendo grupos pequenos com interfaces definidas e validando cada combinação; prioridade final continua na fila central. Sem aumentar equipe antes de existir independência real.
