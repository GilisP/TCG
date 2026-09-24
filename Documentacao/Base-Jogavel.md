# Base jogável — Unity principal

**Acompanhar no Editor:** este é o projeto principal. Use TCG → Jogar no Unity. ProjectEntry configura Mesa.unity como cena inicial do Play. A geometria é criada ao entrar em Play; o modo de edição ainda não contém uma mesa estática. As atualizações são feitas em Assets deste projeto, não no executável.

Nova cena: `Assets/TCG/Scenes/Mesa.unity`. Unity confirmado: **6000.3.6f1**.

## Jogar

Executar `Builds/BaseJogavel/Fronteiras.exe` ou abrir a cena Mesa e entrar em Play. Mesa local para 2–4 pessoas, todos contra todos ou duplas em quatro. Mãos ficam ocultas ao trocar de jogador. O mapa é 3D real low-poly; as cartas são 2D.

## Documentação vigente

- [Arquitetura, uso e limitações](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/08 - Tecnico/Base Jogavel - Arquitetura e Uso.md>)
- [Decisões do autor — Rodada 2](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/10 - Decisoes/Decisoes Confirmadas - Rodada 2.md>)
- [Oficina das quatro expansões](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Expansoes/00 - Expansoes.md>)
- [Contrato de dados e autoria](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Expansoes - Contrato de Dados e Autoria.md>)
- [Pendências prioritárias](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/00 - Projeto/04 - Pendencias de Implementacao.md>)
- [Evidências de validação](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/09 - Testes/Validacao da Base Jogavel.md>)

## Organização do código

`Assets/TCG/Foundation/Core`: domínio sem referência Unity. `Runtime`: conteúdo, UI, cena 3D e diagnóstico explícito de build. `Editor`: testes e preparação/build. Conteúdo em `Assets/StreamingAssets/Expansions/*.json`, com importação atômica em duas etapas (definições/reimpressões), independente da ordem dos arquivos.

Use `TCG.Foundation.Editor.FoundationChecks.Run` para testar e `TCG.Foundation.Editor.FoundationSetup.Build` para gerar o executável. A opção de linha de comando `-tcg-verify` em build de desenvolvimento percorre o fluxo, gera capturas em `Builds/BaseJogavel/Verification` e encerra; não é um modo normal de jogo. Verifique também o código de saída e o log, pois o relatório de cenas é escrito antes do encerramento.

## Limites

24 cartas provisórias, não quatro expansões completas. Preset de teste: 40 cartas principais com repetições, 50 terrenos, mão de cinco. Sem rede e sem todos os tipos/regras avançadas; não é formato oficial. General, Comandante, anexos, missões, efeitos avançados, identidade individual de todas as cartas entre zonas e inventário ainda estão pendentes.

O protótipo anterior em `Assets/TCG/Scripts` / cena `Partida` foi preservado. Não confundir seus sistemas com os integrados à nova mesa. O bootstrap antigo não abre sobre Mesa. O renderizador 2D legado não deve ser usado para desenhar a nova geometria 3D.