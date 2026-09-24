# Validação — Pilhas e animações — 2026-09-24

Sistema: [[Pilhas Compartilhadas e Animacoes da Mesa]]. Projeto principal `C:\Users\gil\TCG`, Unity 6000.3.6f1.

## Motor e build

Builds executadas no principal. Suíte de 1.217 verificações passou: 944 anteriores e 273 em SharedTerrainChecks. Cobertura nova: preparação equilibrada em 2/3/4 pessoas com múltiplas sementes, aleatoriedade reproduzível, conjunto realmente compartilhado, origem da compra/reposição, propriedade e cemitério, deck vazio, eliminação, projeção de rede, eventos de mana e resultados distintos por instância de carta na pilha. Build final: `Evidencias/base-2026-09-24-191102.log`, concluída com sucesso.

## Rede real — duas instâncias

`Builds/BaseJogavel/NetworkVerification/7810`: host e guest1 PASS, revisão 26, dois movimentos por participante, 15 eventos de mana e quatro de resolução recebidos por ambos, zero erros de execução. Reconexão do convidado após 20 segundos e pausa do host verificadas. Busca retornou uma sessão compatível.

## Apresentação e correções

O shader por Shader.Find foi removido do primeiro build; corrigido com PileCard.shader em Resources. Retratos de terrenos agora mostram paisagens procedurais. O clique sintético inicialmente falhou; separar a leitura de entrada da renderização com matrizes GUI aninhadas corrigiu a seleção.

`-tcg-pile-verify`: PASS, zero erros. Sete capturas inspecionadas: frentes/hover no painel, hover das bandejas físicas, hover da pilha, queima, brilho, mana nas sete cores e três formas de ataque. Verificados quatro topos físicos com textura, seleção sem alterar revisão do motor, recepção de eventos por MatchView e limpeza dos objetos após animação. Evidência `Builds/BaseJogavel/PileVerification`, log `Evidencias/pile-ui-20260924-d.log`.

As capturas de VFX usam eventos de rede controlados, sem mudar a coleção. Os testes do motor verificam emissões reais; a rodada Relay confirma envio/recepção durante ações reais. Não confundir a cena controlada de apresentação com uma partida completa.

## Limites

Testes Relay usam processos separados no mesmo computador e o serviço real de internet; não substituem validação em computadores/roteadores distintos. Artes, paisagens e VFX são provisórios. Modificações não alteram a coleção salva nem a composição dos decks do jogador. Avisos conhecidos de liberação gráfica no encerramento do Unity permanecem separados de erros durante a partida.


## Rodada de quatro clientes — concluída

Tentativa 7811: três clientes permaneceram na sala e guest3 não concluiu a conexão inicial ao host; a partida não começou antes do limite de 150 segundos. Processos encerrados normalmente. Não houve evidência de divergência do estado das pilhas, pois a partida nem iniciou. Nova tentativa 7812 usa entradas escalonadas e apenas os quatro clientes, sem a verificação visual concorrente.


7812: PASS nos quatro processos, revisão 60, duas ou três movimentações por participante; 30–31 eventos de mana e dez de resolução observados em cada cliente, zero erros durante a execução. Guest1 reconectou após 20 segundos; host pausou. Câmeras atrás das respectivas capitais e mãos privadas também verificadas. Capturas e relatórios preservados em `Evidencias/pilhas-20260924/relay-quatro`; rodada de dois em `relay-dois` e sete capturas da interface em `interface`.

A primeira falha de conexão inicial permanece registrada como intermitência observada. A segunda rodada bem-sucedida não substitui teste entre redes físicas diferentes. Próximo passo: avaliação manual do autor e partida completa em dois computadores, com duração e latência reais.
