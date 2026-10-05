# Comandante obrigatório e escolha de decks — 2026-10-05

## Decisão do autor
Todo deck precisa de comandante. Ao criar a partida, deve haver a opção de escolher o deck antes de começar. Esta regra vale também para decks experimentais e substitui a exceção anterior.

## Implementação
Jogar → Criar mesa local abre a preparação dos decks, sem iniciar outra partida. Escolha entre meus decks salvos e decks planejados de teste, por jogador, e confirme em Iniciar com estes decks. Os planejados têm comandante separado, 100 cartas principais e 50 terrenos. As antigas demonstrações sem comandante deixaram de ser oferecidas nessa tela. Salas online/rede local permanecem acessíveis durante a preparação.

Na coleção, é necessário escolher comandante antes de salvar; Trocar mostra o filtro de comandantes. Rascunhos antigos sem comandante são preservados, mas não podem jogar. Ainda é possível salvar a montagem incompleta das demais cartas para terminar depois. CollectionLibrary.Validate rejeita comandante ausente em todo deck, inclusive experimental; NetworkRoom usa a mesma validação no host.

Na sala online, o jogador escolhe o deck, confirma o envio e depois marca Estou pronto. Trocar a escolha desfaz o estado pronto e exige confirmação novamente. O preset online de teste agora inclui Comandante das Cem Lanças. Não concede cartas nem altera a coleção pessoal.

## Código e compatibilidade
CollectionLibrary, CollectionUI, HubUI, CommanderDemoUI e NetworkUI implementam a regra e o fluxo. NetworkDiagnostics seleciona explicitamente seu preset de teste. CollectionChecks/NetworkChecks cobrem rejeição e prontidão; CommanderDiagnostics cobre abrir a preparação sem iniciar, decks planejados com comandante e bloqueio ao salvar sem comandante.

Fingerprint de rede: tcg-network-6/20261005-required-commander. Todos os participantes precisam atualizar o jogo. Nenhuma migração de inventário/decks ou mudança de economia.

## Validação
Build aprovado no Unity principal 6000.3.6f1 e 2.929 verificações aprovadas, incluindo rejeição no host e prontidão com 2/4 assentos. Diagnóstico -tcg-commander-hud-verify passou: preparação sem iniciar, comandante em todos os decks planejados locais, salvar sem comandante bloqueado, escolha online explícita e HUD de conjuração; zero erros. LAN com dois processos independentes passou até revisão 26, mãos privadas, movimento e reconexão; zero erros nos dois. Evidências em 09 - Testes/Evidencias/Escolha-Decks-2026-10-05. Avaliação visual por cliques físicos e internet/Relay não foram repetidas; o teste do fluxo chama as funções e comandos do HUD.

[[04 - Pendencias de Implementacao]] · [[Comandante no HUD - 2026-10-05]]
