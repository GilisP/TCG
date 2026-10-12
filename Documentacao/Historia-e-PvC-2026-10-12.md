# História e adversários — implementação 2026-10-12

## Uso

No Unity principal C:/Users/gil/TCG, cena Mesa, use Jogar → Jogar História. Escolha um dos dez precons oficiais; abre o mapa da campanha. Clique em um encontro e em Enfrentar. Vitórias liberam a próxima parada; encontros vencidos podem ser repetidos. O marcador de avanço percorre o caminho após o progresso. Voltar ao mapa pausa a batalha; ela pode ser retomada enquanto o jogo continua aberto. Não há salvamento de partidas fechadas, conforme decisão anterior do autor.

O roteiro inicial provisório tem nove duelos 1x1 contra os outros precons e três encontros especiais: 2x2 (parceiro IA), três participantes todos contra todos e quatro participantes todos contra todos. Os duelos são a maioria. Os especiais estão no final do percurso inicial. A escolha utiliza a lista canônica aprovada do precon, não alterações pessoais na lista salva; não exige importar cartas nem concede inventário. PvP local/online e seus seletores continuam disponíveis.

## Recompensas e regras da campanha

Na vitória da sua equipe/assento, cada deck inimigo presente no encontro concede um booster na primeira vitória contra ele naquele dia. O mesmo inimigo reaparecendo em outro encontro ou contra outro precon seu compartilha o limite diário. Perder/empatar não concede booster nem avanço. Repetir o mesmo resultado não paga novamente. O aliado do 2x2 não é inimigo nem gera recompensa.

O dia é o calendário local do computador, trocando à meia-noite, passado explicitamente ao núcleo para testes. O perfil é local, assim como a economia existente; isto não implementa economia autenticada por servidor. Boosters ficam em progress.boosters para abrir gratuitamente na Loja, com o pacote/chances já existentes. Não altera moedas, preços ou probabilidades. Estatísticas e missões existentes contabilizam apenas a partida concluída do assento humano Âmbar; em duplas, vitória utiliza a equipe.

Roteiro, parceiro IA e interpretação de inimigo como identidade do deck são a base provisória usada enquanto as perguntas opcionais aguardam resposta. Não registrar como respostas explícitas do autor. São ajustes de campanha; não alteram efeitos, movimento, combate ou decisões anteriores das cartas.

## Arquitetura e fontes

Core/StoryCampaign.cs mantém encontros, requisitos, progresso por precon, recibos por MatchId e claims por inimigo/dia. CollectionData.story é aditivo no esquema3; NormalizeStoryProgress inicializa os campos ausentes sem reiniciar saldo, cartas, variantes, decks, estatísticas ou boosters. CollectionStore grava progresso/estatísticas/recompensa numa transação; falha restaura o JSON anterior. Apenas progresso concluído é persistido.

Core/ComputerDecisions.cs produz candidatos de Command no Match oficial reutilizando queries e projeções existentes. Core/ComputerOpponent.cs recebe somente MatchView do assento, catálogo público e candidatos legais; não recebe Match autoritativo, mãos inimigas, ordem dos decks ou RNG da partida. Decide por heurísticas de mana, expansão, criaturas/comandante, equipamentos, embarque, movimento, ataque e defesa. Ativações repetidas são limitadas pela estratégia para evitar loops; isso não muda a regra do jogador. Ataques/bloqueios usam combinações representativas, não enumeração exponencial. O bot não concede para encerrar jogos nem recebe vida, mana ou cartas adicionais.

Runtime/StoryUI.cs desenha mapa e seleção; StorySession.cs prepara os formatos e agenda bots, aplica Match.Try e grava resultados. Humanos continuam usando Submit → Match.Try. Viewer permanece0, câmera/mão na capital humana; outras mãos não são exibidas, e a UI bloqueia ações durante a vez da IA. Bots aguardam durante menus/inspeção. Mesa e VFX são os existentes, com tabuleiro3D e cartas2D; o mapa de campanha é um atlas de interface separado. Não há nova arte definitiva.

Hooks compartilhados: Core/CollectionLibrary.cs; Runtime/HubUI.cs, DeckSelectionUI.cs, NetworkUI.cs (Viewer/controle), TableView.cs, ProgressUI.cs, TableDiagnostics.cs; Editor/FoundationSetup.cs registra StoryCampaignChecks e ComputerOpponentChecks. StoryDiagnostics usa -tcg-story-verify e perfil isolado. Nenhum pacote, protocolo ou regra do PvP foi alterado.

Especialistas em paralelo tiveram arquivos novos disjuntos de IA e campanha; coordenador integrou arquivos compartilhados, Editor, Git e documentação. Contratos/relatórios/checkpoint em Documentacao/Desenvolvimento/HIST-001-*.json/.md.

## Estado e limites

Implementado e verificado: build final em Logs/Story-20261012-final-build.log passou39.569 verificações; runtime passou127checks, zero erros. O último ajuste traduziu o feedback de ações da IA; não mudou regras. Detalhes em Validacao da Historia 2026-10-12 no vault. Captura oculta do mapa foi inspecionada e está preta; não comprova legibilidade/aparência. Avaliação visual manual, dificuldade, comportamento estratégico e campanhas completas com os precons permanecem pendentes. Os testes não demonstram todas as interações entre cartas. Limitações preexistentes de combate com múltiplos donos/equipes (P-03) também se aplicam ao PvC; não foram redefinidas para a IA.
