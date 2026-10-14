# Custos multicoloridos e consulta de terrenos — 2026-10-14

TILE-MANA-014. O autor relatou aparente ausência de cobrança de várias cores e pediu consulta das cartas presentes em um tile.

## Custos
Match.CanPay/Pay já verificam e consomem cada uma das sete posições de ColoredCost e depois o custo genérico. Não foi encontrado erro nessa cobrança. A frente da carta mostrava apenas total em uma gema da cor principal: CardAppearanceUI passa a mostrar também a composição por cor (S Sol, L Lua, G Água, F Fogo, A Ar, T Terra, N incolor) e genérico. Total e identidades existentes preservados.

MulticolorCostChecks: 346 verificações sobre 57 definições multicoloridas do catálogo ativo; disponibilidade exata, ausência de cada cor mesmo com mana excedente de outra cor, consumo integral, texto da frente e transporte da definição pela rede. Sem alteração de protocolo ou regras. Permissões explicitamente “qualquer cor” continuam seguindo suas regras.

## Consulta
TileContentsUI lista o próprio terreno e todos Cell.Pieces públicos, sem limite de nove miniaturas. Inclui anexos e embarcados, dono/controlador e hospedeiro. Ficha abre definição; Selecionar prepara a peça. Clique direito parado consulta sem Submit, enquanto arraste direito mantém câmera. Botão Ver cartas neste terreno disponível no painel da casa selecionada. Janela bloqueia ações da mesa enquanto aberta; consulta preserva seleção de ação e estado Match. Não acessa mãos ou decks privados.

Implementação compartilhada: TableView.OnGUI, DrawSelectedCell, DragMovement.HandleTileInspectionInput; apresentação CardAppearanceUI. Coordenador cuida custo/integração e um especialista do painel/entrada; contrato Documentacao/Desenvolvimento/TILE-MANA-014-contract.json.

Verificação: auditoria multicolor aprovada; Editor Play da janela aprovado: terreno +12 ocupantes, ficha renderizada, seleção de ação preservada e revisão da partida intacta; zero exceções. Logs/TileManaVerification/result.txt e tile-mana-play-20261014.log. Critério: janela com mais de nove ocupantes, ficha renderizada e revisão/seleção intactas. Teste de conexão externa e revisão manual ainda não realizados nesta mudança.

Compilação do Editor aprovada. Não reconstruído o executável anterior nesta entrega; atualização destinada ao projeto principal no Unity. Teste Play usa seleção por método/fixture projetado, não equivale a clique manual ou partida completa online.
