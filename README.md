# TCG — Fronteiras

Projeto Unity principal do jogo de cartas com mesa 3D, coleção, decks e multiplayer por host.

## Abrir

1. Instale Unity 6000.3.6f1 pelo Unity Hub.
2. Adicione esta pasta como projeto e aguarde a importação dos pacotes.
3. Use **TCG → Jogar no Unity**. Cena principal: `Assets/TCG/Scenes/Mesa.unity`.

Fontes atuais: `Assets/TCG/Foundation`. Conteúdo: `Assets/StreamingAssets`. Preserve todos os arquivos `.meta`. Código legado foi preservado e não representa a mesa atual.

## Multiplayer

Em **Jogar → Salas online / rede local**, um jogador hospeda a sala. Dois a quatro jogadores; reconexão por 120 segundos. A saída do host encerra a sala.

Vínculo UGS registrado em 2026-09-24 para o projeto TCG. Testes locais passaram; Relay e descoberta pública ainda precisam de validação real. O vínculo salvo não comprova disponibilidade de todos os serviços.

## Validação e documentação

`TCG.Foundation.Editor.FoundationSetup.Build` prepara, verifica e gera a build. A última rodada de código passou em 918 verificações, além dos testes multiprocesso locais.

Consulte `Documentacao/Multiplayer-por-Host.md` e `Documentacao/Validacao-Multiplayer-2026-09-24.md`. As regras e decisões autorais são mantidas no vault Obsidian indicado em `AGENTS.md`.

Caches, builds, logs, preferências pessoais e arquivos de credenciais são excluídos pelo `.gitignore`. Este repositório não contém o inventário local do jogador nem uma cópia integral do vault.
