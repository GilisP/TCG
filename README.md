# TCG — Fronteiras

Projeto Unity principal do jogo de cartas com mesa 3D, coleção, decks e multiplayer por host.

## Abrir

1. Instale Unity 6000.3.6f1 pelo Unity Hub.
2. Adicione esta pasta como projeto e aguarde a importação dos pacotes.
3. Use **TCG → Jogar no Unity**. Cena principal: `Assets/TCG/Scenes/Mesa.unity`.

Fontes atuais: `Assets/TCG/Foundation`. Conteúdo: `Assets/StreamingAssets`. Preserve todos os arquivos `.meta`. Código legado foi preservado e não representa a mesa atual.

## Multiplayer

Em **Jogar → Salas online / rede local**, um jogador hospeda a sala. Dois a quatro jogadores; reconexão por 120 segundos. A saída do host encerra a sala.

Vínculo UGS registrado em 2026-09-24 para o projeto TCG. Criação, descoberta pública, entrada e reconexão foram testadas usando o serviço Relay real com instâncias neste computador. Uma partida entre computadores e redes físicos distintos ainda precisa de avaliação.

## Validação e documentação

`TCG.Foundation.Editor.FoundationSetup.Build` prepara, verifica e gera a build. A última rodada de código passou em 1.217 verificações, além dos testes multiprocesso locais.

Consulte `Documentacao/Multiplayer-por-Host.md` e `Documentacao/Validacao-Multiplayer-2026-09-24.md`. As regras e decisões autorais são mantidas no vault Obsidian indicado em `AGENTS.md`.

Caches, builds, logs, preferências pessoais e arquivos de credenciais são excluídos pelo `.gitignore`. Este repositório não contém o inventário local do jogador nem uma cópia integral do vault.

## Estatísticas, missões e ruínas

**Menu → Estatísticas e missões** mostra totais, recordes e últimas 50 partidas concluídas. Perfil local: Âmbar nas mesas locais; próprio assento nas salas online. Resgate moedas e boosters das missões únicas de teste. Boosters ganhos são abertos em **Loja → Boosters → Abrir recompensa**. Valores em `Assets/StreamingAssets/Economy/missions.json`.

A capital começa sobre uma carta do deck de terrenos; as três casas iniciais adjacentes são ruínas sem mana ou efeitos. A câmera online fica atrás da própria capital; na mesa local acompanha a passagem de controle. Rotação manual continua disponível.


## Pilhas compartilhadas e animações

Quatro pilhas de terrenos são usadas por todos. As oito cartas iniciais vêm dos decks em partes equilibradas (4/4, 3/3/2 sorteado ou 2/2/2/2). Reposição e compra usam o deck do jogador do turno. Terrenos pertencem ao reino onde foram colocados, preservando o dono original para o cemitério.

Passe o mouse pelos topos das bandejas ou pelas cartas do painel para ampliar. A pilha de ações também mostra frentes e hover. Ganhos de mana, ataques, anulação e resolução têm efeitos visuais. Consulte `Documentacao/Pilhas-Compartilhadas-e-Animacoes.md`; `-tcg-pile-verify` executa a verificação visual em build de desenvolvimento.
