# Movimento por arraste e rotas

Pedido do autor em 2026-09-18: arrastar uma peça move; destinos além do movimento atual ficam programados; buracos precisam receber terreno andável; a programação pode ser cancelada antes do próximo movimento. O autor confirmou avanço automático. Também pediu passagem automática de fase ao colocar terreno.

## Funcionamento

- Arrastar uma criatura sua com o botão esquerdo mostra uma prévia e um trajeto luminoso com paradas numeradas. Soltar sobre outro tile envia um comando de movimento programado.
- Na fase principal, a peça avança o que puder imediatamente. Cada passo usa a validação e o custo de movimento existentes. Se alcançar o destino, a programação termina.
- Se faltar movimento, a rota aguarda o próximo turno do dono. Se faltar terreno ou houver bloqueio, não atravessa o impedimento nem gasta movimento naquela tentativa.
- Na entrada da próxima fase principal, tenta continuar automaticamente. Isso acontece depois da compra e da colocação do terreno. Ao colocar terreno, a mesa principal entra nessa fase sem pedir mais um clique.
- **Cancelar esta rota / Cancelar todas as rotas** fica disponível antes de colocar o terreno. Depois da colocação, a execução automática já pode ocorrer. Pode selecionar a miniatura na fase de terreno para cancelar sua rota específica.
- A rota procura primeiro um caminho existente; quando não há caminho completo, pode indicar um trajeto por tiles ainda vazios, mas a peça para antes deles. O trajeto luminoso é planejamento, não autorização para atravessar buracos.
- Não inicia ataques automaticamente. Revalida ocupação, construções bloqueadoras e capitais inimigas. Habilidades de movimento continuam valendo; gatilhos e escolhas interrompem a execução até serem resolvidos.
- A morte/remoção da peça elimina sua ordem; nova partida limpa as ordens. Não há salvamento de partida. Um movimento manual comum substitui a programação daquela peça.
- Soltar fora do tabuleiro, voltar à origem ou pressionar Esc cancela o arraste sem executar movimento. O botão direito continua girando a câmera.

## Código e responsabilidades

No projeto principal `C:\Users\gil\TCG`, sob `Assets/TCG/Foundation`:

- `Core/MovementOrders.cs`: ordens por identidade da peça, procura de caminho, estados de espera, fila e cancelamento. Não depende de Unity.
- `Core/Match.cs`: comandos `PlanMove` e `CancelMove`, integração com a validação de jogador/revisão e entrada da fase principal. `AutoAdvanceAfterTerrain` fica habilitado na mesa principal; os testes explícitos do protocolo anterior podem manter a passagem manual.
- `Runtime/DragMovement.cs`: distingue clique de arraste, seleciona miniaturas, envia comandos e mostra cancelamento/status.
- `Runtime/MovementOrderVisuals.cs`: prévia da peça e paradas numeradas com a mesma luz animada de MovementLight.cs; não altera o estado da partida.
- `Runtime/TableWorld.cs`: colliders de seleção de miniaturas e fila de pontos de animação, evitando cortar visualmente as esquinas dos passos programados.
- `Runtime/TableView.cs`: habilita a fase automática, apresenta ordens e limpa o gesto quando abre um modal.
- `Editor/MovementOrderChecks.cs`: regressões de turno, terreno ausente, bloqueios, cancelamento, morte, gatilhos e comandos inválidos.
- `Runtime/DragMovementDiagnostics.cs`: eventos sintéticos de mouse pelo mesmo caminho de entrada do jogo, captura e conferência do estado por `-tcg-drag-verify`.

O material da linha usa `Assets/TCG/Resources/MovementGlow.shader`, já utilizado em [[Qualidade de Vida - Prioridade e Sala]]. Recursos visuais são reutilizados; rotas recalculam quando mudam seleção, destino ou revisão da partida. Ordens e comandos ficam no motor para futura integração de rede, que continua ausente.

## Estado e validação

Implementação compilada; 339 verificações de lógica passaram. O diagnóstico de arraste passou sem erros. A apresentação final das paradas numeradas também passou: duas paradas para um trajeto de três espaços com movimento 2, sem bolinha no intermediário, e remoção completa após cancelar. Capturas inspecionadas. Resultados em [[Validacao do Arraste e Rotas 2026-09-18]]. Backup em `99 - Arquivo/Auditorias/antes-arraste-rotas-2026-09-18.zip`. Prioridades somente em [[04 - Pendencias de Implementacao]].

## Paradas numeradas confirmadas
O autor escolheu **ordem das paradas: 1, 2, 3…**, e não distância acumulada. A rota usa a mesma animação de luz viajante e círculo pulsante do movimento normal. Para uma peça com movimento 2, os marcadores da rota ficam nos pontos de parada a cada dois espaços, incluindo o destino final quando o último trecho for menor. O primeiro trecho considera o movimento ainda disponível; se a ordem já espera pelo próximo turno ou por terreno, considera a renovação normal do movimento.

Enquanto uma rota está selecionada ou sendo arrastada, os indicadores de destinos alternativos ficam ocultos para não misturar caminhos. Os números acompanham a câmera e são removidos ao cancelar. A numeração identifica paradas previstas: espera por terreno, bloqueios e escolhas de habilidades podem adiar ou alterar a execução. Nos destinos alternativos imediatos, o número 1 identifica alternativas para a primeira parada.
## Correção de clique — 2026-09-18
O estado MouseDown é preservado quando há carta selecionada; somente a visualização de movimento é ocultada. Corrige a impossibilidade de colocar cartas nos terrenos após a introdução do arraste. Ver [[Reacoes dos Jogadores]] e [[Validacao das Reacoes 2026-09-18]].
