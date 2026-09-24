# Validação de boosters e HUD medieval — 2026-09-23
Escopo: [[Boosters Variantes e HUD Medieval]], no projeto principal C:\Users\gil\TCG, Unity 6000.3.6f1.

## Verificações
FoundationSetup executou 726 verificações: Foundation 69, Medieval 143, QOL 48, Movement 79, Reactions 13, Presentation 174, Collection 17, Author 38, Hub 19, Bicolor 110 e Booster 16.
O conjunto cobre economia 5/100/10, pool elegível, repetidas dentro da abertura, propriedade de variantes, concessão da base, cópia independente de deck, persistência, migração sem reaplicar saldo inicial e rejeição atômica de fundos insuficientes/configuração inválida/falha do sorteador.

Um teste de recarga detectou que JsonUtility serializa recibo nulo como objeto vazio. A leitura agora normaliza somente esse objeto vazio, preservando a validação dos demais recibos. O conjunto passou novamente após a correção.

Build final: Evidencias/base-2026-09-23-123100.log. Executável em C:\Users\gil\TCG\Builds\BaseJogavel\Fronteiras.exe.

## Execução visual
O diagnóstico -tcg-booster-verify usa perfil temporário isolado. Exercita compra, recibo persistido antes da revelação, restauração após falha de escrita, escolha de aparência da coleção/deck, recarga e importação da ilustração. Captura menu, loja, abertura, coleção, ficha ilustrada e mesa.

O diagnóstico -tcg-personality-verify passou com zero erros de execução: hover, prioridade da carta elevada, seleção, colocação por eventos sintéticos de mouse e ocupação múltipla. O leque e as capturas foram inspecionados. Esses testes não equivalem a uma partida manual prolongada.

A inspeção visual detectou e corrigiu corte dos rostos na ilustração, ornamentos deslocados por transformações da GUI e cartas ocultas pela faixa inferior. Retratos verticais agora usam corte superior, a gema usa textura procedural sem alterar a matriz e a mão cabe acima do rodapé. Granulação reduzida e contraste aumentado.

Evidências finais: pasta Evidencias/boosters-hud-20260923; logs booster-final-20260923.log e personality-final-20260923.log. Os relatórios result.txt registram os resultados dos percursos.

## Limitações
- Apenas Elo/Iluminura tem a nova arte pintada; retratos restantes e economia são provisórios.
- Foi testada a interface a 1600×1000; escalas extremas/resoluções menores e avaliação manual de legibilidade permanecem pendentes.
- Um aviso de descarte de ComputeBuffer no encerramento do Unity continua presente, já observado antes desta alteração; sem exceções de jogo nos percursos executados.
- Rede, fonte recorrente de moedas, balanceamento e aprovação estética não foram concluídos por estes testes.

Editor principal reaberto em Play: editor-boosters-20260923.log confirmou Assets/TCG/Scenes/Mesa.unity e table=True, sem erros de compilação ou exceções no trecho de inicialização. Diagnóstico final de boosters: saída 0, zero erros; escolha de aparência por coleção/deck, persistência e rollback também passaram.
