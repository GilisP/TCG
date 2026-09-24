# Central de quatro telas

Pedido do autor em 2026-09-19: separar procurar jogo, coleção/decks com cosméticos, loja com moeda do jogo e menu.

## Navegação
A barra superior permanece nas quatro telas:
1. Jogar: criar mesa local de 2–4 jogadores, todos contra todos/duplas, decks de demonstração ou decks salvos por assento. A busca online está explicitamente indisponível; rede ainda não foi implementada.
2. Coleção e decks: biblioteca, rascunhos e montagem; aba Cosméticos permite equipar um verso no deck escolhido e salva o rascunho. Alterações visuais de deck são aplicadas ao iniciar uma nova partida.
3. Loja: cartas individuais e versos, saldo, busca, confirmação com saldo restante, prevenção de compra repetida e de saldo insuficiente.
4. Menu: tela cheia, volume geral, ambiente, acesso à partida aberta e saída. Preferências de apresentação ficam em PlayerPrefs.

## Economia provisória
O pedido mais recente inclui comprar cartas e cosméticos com moeda do jogo. A interface encaminha novas aquisições à loja; as cartas previamente possuídas são preservadas.
Para viabilizar o teste, foram escolhidos provisoriamente 500 de saldo inicial, 50 por carta e 150 por verso colorido. Selo dos Reinos vem incluso. Não são preços nem recompensas aprovados como definitivos.
O saldo inicial é concedido uma vez por perfil/migração. Não há dinheiro real, pacotes aleatórios, botão de recarga, recompensa por partida ou servidor econômico. Definir a forma de ganhar moedas com o autor antes de tratar a progressão como concluída.
As perguntas sobre a divisão das telas e aquisição gratuita versus loja foram enviadas; na ausência de resposta durante a implementação, a loja segue o pedido mais recente. Uma resposta posterior tem precedência sobre esta configuração provisória.

## Implementação
No projeto principal C:\Users\gil\TCG, Assets/TCG/Foundation:
- Runtime/HubUI.cs: navegação, partida local, cosméticos e configurações.
- Runtime/ShopUI.cs: catálogo da loja, confirmação, gravação e reversão em memória quando salvar falha.
- Core/CollectionLibrary.cs: saldo, propriedade, compras, validação e migração do esquema 1 para 2.
- Core/Cosmetics.cs: catálogo de quatro versos; preços e cores provisórios.
- Runtime/DeckCosmetics.cs: aplica o verso às pilhas físicas e às cartas seguradas pelos avatares. Não altera as regras nem revela cartas privadas.
- Runtime/CollectionUI.cs: mantém o editor existente dentro da nova central.
- Editor/HubChecks.cs e Runtime/HubDiagnostics.cs: testes de economia, persistência e fluxo renderizado.

O arquivo local continua denominado collection-v1.json por compatibilidade de caminho, mas contém schemaVersion 2. O esquema antigo é migrado preservando IDs, cartas e decks. Saldo e aquisição são gravados no mesmo arquivo, com backup; falha de escrita reverte a compra na sessão. Perfis corrompidos permanecem protegidos contra gravação.
Novos cosméticos podem ser acrescentados ao catálogo; tipos visuais diferentes de verso precisam de apresentação própria. Rede exige autoridade externa sobre compras e inventário; o perfil local é exclusivamente o protótipo.

## Estado
Implementado no projeto principal. Validação em [[Validacao da Central de Telas 2026-09-19]].
[[Colecao Decks e Comandantes]] · [[04 - Pendencias de Implementacao]].
