# Qualidade de vida — prioridade e sala

Pedido do autor de 2026-09-18: pedir respostas somente quando houver algo a fazer, adicionar modelos dos jogadores, dois ambientes e posicionar as pilhas sobre a mesa. Implementação no projeto principal `C:\Users\gil\TCG`, cena `Assets/TCG/Scenes/Mesa.unity`.

## Respostas automáticas

A mesa habilita `Match.AutomaticResponses`. Depois de cada comando, o motor passa a prioridade dos jogadores sem truque disponível e pagável. Verifica os alvos dos programas medievais conhecidos, inclusive movimento, troca e retaliação. Reavalia após cada resolução. Não escolhe alvos nem bloqueadores pelo jogador e não encerra automaticamente sua fase principal. A compra e o início de um novo turno continuam exigindo ação. O encerramento sem respostas não exige passes vazios.

O protocolo explícito de passes permanece disponível para os testes e outros consumidores do motor. A interface principal sempre habilita a automação. Programas futuros desconhecidos mantêm a oportunidade de resposta por precaução; ao adicionar um novo truque, estender a avaliação de disponibilidade e seus testes. As habilidades ativadas atuais exigem fase principal e não são respostas.

## Ambientes e jogadores

Dois ambientes procedurais: **Salão dos Reinos**, com pedra e estandartes, e **Taverna do Carvalho**, com vigas, prateleiras e lareira. O botão com o nome da sala, acima da mesa, alterna o ambiente sem reiniciar a partida. Modelos sentados usam a cor do respectivo jogador; em duelos ficam em lados opostos, acompanhando as capitais. Lugares não ocupados são ocultados.

São modelos estilizados provisórios, gerados em execução, sem rig ou animações de personagem. O cenário não aplica regras de relevo, bônus ou iluminação ao motor.

## Pilhas na mesa

Cada jogador tem quatro pilhas de terrenos, o deck principal e o deck de terrenos. Altura e contagem acompanham o estado real; as faces privadas não são exibidas. Na compra ou fase de terreno, clicar em uma das quatro pilhas do jogador ativo seleciona a pilha usada pela interface. A bandeja selecionada recebe destaque dourado. Os botões laterais continuam disponíveis. D identifica o deck principal e T o deck de terrenos. A representação usa altura limitada para preservar a leitura de decks grandes.

## Organização do código

- `Core/AutomaticResponses.cs`: disponibilidade e avanço automático da prioridade.
- `Core/Match.cs`: executa o avanço após os comandos sem mudar a regra de conjuração e resolução das cartas.
- `Runtime/TableEnvironment.cs`: ambientes, personagens e apresentação das pilhas.
- `Runtime/TableWorld.cs`: construção e sincronização da cena.
- `Runtime/TableView.cs`: habilitação, troca de sala e seleção das pilhas.
- `Editor/QualityOfLifeChecks.cs`: regressões do fluxo de respostas em 2–4 lugares.
- `Runtime/QualityOfLifeDiagnostics.cs`: verificação de apresentação, raycasts e capturas por `-tcg-qol-verify` em build de desenvolvimento.

Todos os caminhos acima são relativos a `Assets/TCG/Foundation`. Não foram substituídos arquivos `.meta` existentes. Backup anterior em `99 - Arquivo/Auditorias/antes-qol-2026-09-18.zip`.

## Verificação e continuidade

Estado: implementado e verificado no build. As 260 verificações de lógica passaram; a execução final apresentou zero erros e 60 FPS médios na amostra de três segundos. Capturas inspecionadas. Resultados em [[Validacao da Sala e Prioridade 2026-09-18]]. Pendências de execução somente em [[04 - Pendencias de Implementacao]]. Arte definitiva e avaliação manual do autor continuam necessárias; rede permanece ausente.

## Luz dos movimentos disponíveis — 2026-09-18
Ao selecionar uma criatura, feixes azulados saem do seu tile e chegam aos destinos aceitos por `Match.CanMove`. Uma luz percorre cada feixe em direção ao destino, que recebe um círculo pulsante. Os arcos indicam destinos disponíveis, não desenham a rota percorrida no tabuleiro. Ataques não recebem esses feixes.

`Runtime/MovementLight.cs` mantém um conjunto reutilizável de LineRenderers e um material aditivo compartilhado com `Assets/TCG/Resources/MovementGlow.shader` (independente da iluminação e sem exigir bloom); anima em `TableWorld.LateUpdate`. Recalcula destinos quando seleção ou revisão da partida mudam. `TableView` oculta o efeito em menus, transferência de jogador, escolhas e demais modais, ou ao selecionar uma carta. Movimento concluído limpa a seleção e os feixes. A troca de partida também limpa o efeito. Usa o motor existente, sem mudar custos, alcance ou regras de movimento.

Verificação de desenvolvimento: `-tcg-movement-verify`, em `MovementLightDiagnostics.cs`, confere igualdade com os destinos legais, ocultação por privacidade, restauração e limpeza após mover; gera capturas em `Builds/BaseJogavel/MovementVerification`. Compilação, build, 260 verificações anteriores e diagnóstico visual concluídos. Captura final inspecionada com feixes luminosos nos três destinos legais; zero erros de runtime. Backup anterior: `99 - Arquivo/Auditorias/antes-luz-movimento-2026-09-18.zip`.
