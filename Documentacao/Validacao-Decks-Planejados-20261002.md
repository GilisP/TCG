# Validacao dos Decks Planejados 2026-10-02

[[Decks Planejados - Integracao 2026-10-02]] · [[04 - Pendencias de Implementacao]]

## Ambiente e escopo

Projeto principal `C:/Users/gil/TCG`, Unity **6000.3.6f1**. Build Windows em `Builds/BaseJogavel/Fronteiras.exe`; catálogo do executável comparado byte a byte com a fonte. Testes de coleção usam perfis isolados; não importaram listas no inventário pessoal do autor.

## Compilação e regras — aprovado

`TCG.Foundation.Editor.FoundationSetup.Build` terminou com **2.915 verificações aprovadas**, incluindo **1.695 PlannedDeckChecks** e build concluído. Evidência original: `Logs/planned-release-build.log`, saída 0.

Cobertura específica:
- 29 listas válidas de 100 principais + 50 terrenos, sem identidades repetidas e compatíveis com o comandante.
- Importação, reinício do perfil, preservação de moedas/decks antigos e reimportação sem sobrescrever edições.
- Execução básica das 641 definições novas; serialização de suas habilidades. É teste de execução, não prova exaustiva de todas as combinações.
- Auras por formação, equipamento/movimento, alcance sem recursão, imunidade apenas a movimento, prevenção consumida entre golpes, custo de sacrifício antes da resolução, gatilho de ataque antes do combate, terrenos com custo/limite/expiração e Valeria com veículo novo.
- Pirata: dano a capital envia cartas do grimório ao cemitério; recuperação paga 2 PA, distingue entradas iguais e preserva dono original ao conjurar/morrer.
- Projeções de salas com 2/3/4 jogadores: listas completas aceitas, 95 cartas restantes após mão inicial e mãos adversárias ausentes.

## Fluxo de coleção/partida — aprovado; revisão visual pendente

`-tcg-planned-decks-verify` concluiu importação de 29 modelos, persistência, abas principal/terrenos, escolha de decks, início de partida e ativação de terreno. Resultado: **0 erros de runtime**. Evidência: `Builds/BaseJogavel/PlannedDeckVerification/result.txt`.

As sete capturas produzidas com a janela oculta foram inspecionadas e estavam pretas. **Não são evidência de legibilidade nem de acabamento visual.** O teste acionou as funções da interface; não validou cliques físicos nos botões. A conferência visual/interativa pelo autor no Unity continua pendente (P-05/P-09).

## Rede em processos independentes

LAN/UDP, mesmo computador, decks completos escolhidos pelos modelos novos (`-tcg-planned-net`). Porta 7797, dois processos: **aprovado**, revisão 140 nos dois, 0 erros de runtime; host transmitiu 6 movimentos e cliente 10; ambos receberam 132 eventos de mana e 28 de resolução. Host observou pausa e cliente reconectou. Mãos privadas e pilhas comuns conferidas durante a execução.

A primeira rodada (7796), com o limite antigo de revisão 26, terminou antes de o host movimentar uma criatura e falhou nessa asserção. O cliente havia passado. O diagnóstico dos decks completos foi ampliado para revisão 140 (dois) / 240 (quatro), sem retirar a exigência de movimento. A repetição de dois passou.

A rodada de quatro em 7798 não atingiu a meta em 300 segundos. A repetição instrumentada (7799) identificou o jogador automático parado na revisão 98: tentava comprar na fase de compra enquanto havia uma habilidade de início de turno na pilha. O motor recusava corretamente; a interface normal já verifica a pilha antes de oferecer compra. O diagnóstico foi corrigido para resolver a pilha antes de comprar ou colocar terreno e recebeu limite de 30 FPS. Na rodada 7800, os quatro continuaram avançando, mas chegaram à revisão 222 ao esgotar 300 segundos. O diagnóstico passou a desligar VSync para efetivar 30 FPS e ter orçamento de 600 segundos; nenhuma asserção foi removida.

**Rodada final 7801: aprovada**, quatro processos em duplas, todos na revisão 240. Host/guest1/guest2/guest3 transmitiram respectivamente 6/8/9/8 movimentos. Cada processo recebeu 190 eventos de mana e 49 de resolução; todos registraram **0 erros de runtime**. Host observou pausa; guest1 reconectou. Mãos privadas, pilhas compartilhadas e câmera atrás da própria capital aprovadas. Os quatro relatórios finais têm data de 2026-10-02; arquivos antigos de 2026-09-24 que já estavam nessa pasta não foram usados como resultado desta rodada.

Os logs de 7798 registraram erros nativos Curl/TLS. Não houve desativação da verificação de certificados; esses registros não comprovam falha do transporte LAN nem sucesso de acesso à internet.

## Limites

Não houve novo teste Relay nesta entrega; a validação de internet de 2026-09-24 permanece histórica. Partidas entre máquinas/redes físicas distintas, sessões longas, equilíbrio das 29 listas, revisão dos subtipos provisórios e avaliação visual seguem pendentes. Arte é provisória. Os diagnósticos não representam validação de economia autenticada por servidor.

Nos encerramentos do executável há avisos de descarte de ComputeBuffer/memória do Unity; não se confundem com exceções de execução contadas pelos diagnósticos. A avaliação de recursos em sessões longas permanece em P-09.


## Evidências preservadas

Logs de compilação e diagnósticos, resultados de coleção e os relatórios finais LAN 2/4 estão em `09 - Testes/Evidencias/Decks-2026-10-02`. Capturas pretas não foram promovidas a evidência visual. No repositório, esta síntese está em `Documentacao/Validacao-Decks-Planejados-20261002.md`.
