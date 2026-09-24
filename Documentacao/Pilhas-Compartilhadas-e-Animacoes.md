# Pilhas Compartilhadas e Animacoes da Mesa

## Decisão vigente — 2026-09-24

O autor pediu quatro pilhas de terrenos compartilhadas, com imagens e ampliação ao passar o mouse, cartas da pilha de ações com o mesmo tratamento, esferas de mana até a capital, ataques animados e queima/brilho para cartas anuladas/resolvidas.

Resposta autoral preservada: “o jogador do turno sempre poem a carta de reposição. primeiro pegue 8/players cartas de cada dai minte a pilha, arredonde no 3.sim todos podem usar do topo. pertence ao reino mas vai para cemiterio do dono”.

Interpretação operacional comunicada: retirar oito cartas após o terreno da capital, quatro de cada jogador em duas pessoas, duas de cada em quatro pessoas; em três, 3/3/2 com sorteio de quem contribui duas. Embaralhar essas oito e distribuir duas por pilha. Todos usam o topo em seu turno. Compras/reposições posteriores usam exclusivamente o deck do jogador ativo; deck vazio não toma carta emprestada de outro jogador. A capital continua usando carta própria; vizinhos iniciais continuam ruínas.

Esta decisão substitui as quatro pilhas individuais documentadas em versões anteriores. Separar controle do reino (`Cell.Owner`) da propriedade da carta (`Cell.TerrainOwner`). O cemitério recebe a carta pelo dono original, inclusive quando a carta foi colocada em outro reino. Eliminação não apaga pilhas compartilhadas.

## Implementado

- Motor: `Core/Match.cs`, `AuthorRules.cs`, `MedievalRules.cs`, `NewCommanders.cs`. `TerrainCard` conserva definição e dono por cópia, inclusive cópias da mesma carta de jogadores diferentes. Os assentos consultam o mesmo conjunto de quatro pilhas.
- Rede: `NetworkView.cs`/`NetworkRoom.cs` levam topo, dono, contagem e eventos visuais; somente o topo é projetado, sem revelar ordem de decks. Compatibilidade `tcg-network-4/20260924-shared-terrain`.
- Apresentação: `PilePresentation.cs`, `TableEnvironment.cs`, `TableFlights.cs`, `TerrainPortraits.cs`. Frentes reutilizam `CardAppearanceUI`; artes existentes e retratos procedurais continuam provisórios. As bandejas físicas são comuns, decks fechados continuam individuais.
- Animações são apresentação; não atrasam nem mudam mana, dano ou prioridade. Escolhas de efeitos continuam obrigatórias. A ampliação não depende de ser o jogador ativo.

## Verificação

Build final aprovada com 1.217 verificações, diagnóstico visual sem erros e capturas inspecionadas. Relay real aprovado com dois e quatro processos; reconexão de 20 segundos. Houve uma falha inicial de conexão na primeira tentativa de quatro, registrada na nota. Evidências e limitações em [[Validacao Pilhas e Animacoes 2026-09-24]]. Teste entre redes físicas diferentes continua pendente.

## Extensão

Artes por carta continuam no manifesto `StreamingAssets/CardArt` e `Resources/CardArt`. Eventos de mana incluem origem, cor e quantidade. Identidade visual de cada item da pilha distingue cópias iguais; anulação emite evento próprio. Trocar modelos/VFX não exige modificar regras.
