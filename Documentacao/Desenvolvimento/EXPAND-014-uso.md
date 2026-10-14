# Artes, tropas e palavras-chave — 2026-10-14

EXPAND-014 integrado no projeto principal C:\Users\gil\TCG. Build e testes proporcionais concluídos; avaliação estética do autor e desempenho prolongado continuam pendentes.

## Artes e modelos

Dez pinturas exclusivas ED261007-03-C001–C010 vinculadas por ID/style standard em CardArt/manifest.json. Assets em Resources/CardArt/PreconD03; prompts, fontes, hashes e prévias em Documentacao/Arte/Precon-D03-20261014/Indice.md. É o início de D03, não o precon completo. D01/D02 e comandantes preservados.

TroopModels.cs reutiliza malhas suaves de CommanderModels, com pivôs de corpo, cabeça, braços, pernas, capa, mandíbula, olhos e bolsa. Perfis por subtipos explícitos Goblin/Esqueleto e art mage/archer/royal/giant; não concede regras/subtipos. Cinco goblins de D03 têm corneta, lenha/machado, cozinha/panela, capacete grande e acessórios de caravana. Integração única em MedievalVisuals.Figure, compartilhada por peças e retratos provisórios. Comandantes, veículos, beast/serpent/winged preservam apresentações anteriores. Prévia: Documentacao/Arte/Tropas-20261014/preview.png.

FigureMotion em PiecePersonality.cs mantém Idle/Walking/Attacking/Dying/Celebrating, com respiração, gestos, olhos, mandíbula, bolsa e ataque diferenciado das novas tropas. Poses de ataque direcionadas para -Z, respeitando a orientação do root; picking e estado do motor preservados. Articulação por Transform, não FBX/skinned rig. Modelos procedurais sujeitos à avaliação artística.

## Cinco palavras-chave no motor

Fonte de design: [[Palavras-chave - Base de Design]] e [[Convencoes das Palavras-chave - Testes 2026-10-14]]. Core independente de Unity; host valida efeitos/ações. Interface só apresenta nomes com KeywordPresentation/CardAppearanceUI.

| Campo keywords | Funcionamento |
|---|---|
| magic-resistance | Oponentes não escolhem a peça como alvo. Preserva efeitos próprios/aliados, combate, áreas e sujeitos automáticos sem seleção. |
| anchored | Impede deslocamentos inimigos, incluindo veículo com passageiro Ancorado e troca de terrenos que levaria a peça junto. Movimento próprio e devolução à mão preservados. |
| armor:N | Reduz N de cada dano de combate. |
| tenacity:N | Reduz N de cada dano, incluindo efeitos. |
| barrier:N | Consome os próximos N pontos restantes após reduções. Sem renovação automática no turno ou por perder/reherdar a mesma capacidade. |

N validado tecnicamente entre 1 e 1.000; prefixos exatos. Maior valor por nome; Armadura + Tenacidade somam em combate. Barreira usa maior capacidade efetiva menos gasto acumulado por instância; aumento de capacidade fornece somente diferença disponível. Aplica-se depois do bônus de dano/reduções, antes da reflexão/prevenção legadas, preservando ordem antiga nas cartas sem novas defesas. Instância nova começa com recurso novo; não é salvamento de partida.

PD26-T-010/011/012 agora nomeiam Ancorado, que já estava explícito no texto. Mantidos textos, custos, atributos, IDs e habilidades existentes; import_planned_decks.py preserva a marcação ao recompilar. Outras quatro palavras não foram distribuídas automaticamente às cartas: disponíveis para definições futuras/fixtures aprovados.

KeywordRules/NumericKeywordRules, MedievalRules/CardAbilities/Match e seletores autorais usam o estado oficial. BarrierSpent é local ao host; Piece.BarrierRemaining é projetado em NetPiece.barrierRemaining, reconstruído para consulta do cliente. Sem executar regras no cliente. Fingerprint tcg-network-9/20261014-keyword-defenses: atualizar todos clientes juntos.

## Validação e evidências

Logs em Logs/expand-014-*.log. Regressão ampla FoundationSetup.Build concluída, incluindo 9.756 verificações de IA. Refinamentos e correções finais revalidados por BuildExpanded: 68 verificações de palavras-chave, 969 de comandantes, 223 de tropas/12 perfis, 69 de artes/vínculos/rótulos, 346 de custos, 117 de rede e 70 de precons. Build do executável aprovado.

Editor Play isolado: 223 verificações de figuras e oito de transições reais de FigureMotion; assertiva garante ausência de TableView/perfil. Renders Unity inspecionados. LAN de dois processos com reconexão/pausa passou; evidências anteriores em NetworkVerification/7966 e execução final em 7967 dentro Builds/BaseJogavel. Roundtrip de Barreira e opções inválidas cobertos por fixtures; LAN não contém todas as combinações sintéticas de keywords. Relay e máquinas distintas não repetidos.

Falhas preservadas: tentativa de pose afetou ataque de comandante e foi corrigida; fixtures de escolha direta precisaram drenar fila; primeira chamada LAN usou label incorreto para o cenário obrigatório de reconexão. Revisão independente encontrou bypass de resistência no bounce, corrigido. Sujeitos automáticos e terreno transportado foram conferidos e corrigidos na integração. Reviews aceitos por escopos, sem alegar cobertura universal pelo revisor.

Limitação procedural: leituras de arte e uma revisão agregada excederam orçamento; relatórios parciais preservados. Revisões posteriores receberam somente pacotes delimitados/continuações não lidas. Não há fiscalização automática de leitura/leases.

Pendentes: avaliação do autor, FPS prolongado; completar pinturas de D03; demais 14 palavras-chave ainda exigem suas interações/implementação. Convenções aprovadas de Marcha/Entrincheirar/Ímpeto estão registradas, não implementadas neste lote. [[04 - Pendencias de Implementacao]] permanece fila única.

Validação final LAN 7967: host e guest1 PASS na revisão 26, ambos com zero erros; host pausou e cliente reconectou. Evidências: Logs/expand-014-final-lan-host.log e expand-014-final-lan-guest.log.

