# Comandante no HUD - 2026-10-05

Pedido do autor: um lugar no HUD para usar o comandante, mostrando sua imagem.

## Implementado

Painel à direita da mesa, com frente da carta, arte/variante e foil existentes, localização (zona, pilha, campo ou fora da zona), custo total e adicional de reconjuração. Mouse sobre a carta amplia; botão direito abre a ficha. Clique na carta ou em Conjurar seleciona o comandante; depois clique em um terreno destacado do seu reino. Cancelar seleção ou Esc cancela. Em campo, o botão seleciona a peça para os controles normais de movimento/ataque/habilidade.

A disponibilidade consulta CanSummonCommander, incluindo mana, fase, prioridade e destinos legais; online, usa as ações legais recebidas do host. Não muda custos ou regras. Sem comandante no deck, há orientação explícita para escolher um deck com comandante ou Experimentar comandantes.

O painel bloqueia entrada no tabuleiro por baixo, inclusive soltar um arraste sobre ele. Exílio foi deslocado para baixo para não sobrepor a carta.

## Arquivos e extensão

CommanderUI.cs usa RenderCardFace/CardStyle, compartilhados com coleção/mão. Novas artes cadastradas no manifest existente aparecem aqui também. TableView integra a ampliação; DragMovement protege a região e trata Esc; ExileUI reposiciona o botão. Sem mudança de dados, perfil, protocolo ou motor de regras.

CommanderDiagnostics.cs oferece -tcg-commander-hud-verify com perfil isolado: bloqueio por fase/mana, selecionar/cancelar, região de entrada, conjuração e projeção de rede. Não modifica a coleção pessoal.

## Validação

Compilação e build aprovados em Logs/commander-hud-build.log, com 2.915 verificações existentes. Diagnóstico de runtime aprovado: fase/mana, selecionar/cancelar, região de entrada do painel, conjuração, presença em campo e projeção de rede; 0 erros. Evidência em Builds/BaseJogavel/CommanderHudVerification/result.txt. Teste executa as funções do HUD e o comando de conjuração; não equivale a entrada física de mouse. Não houve nova sessão LAN/Relay, pois motor/protocolo permaneceram iguais. A ferramenta nativa identificou o Unity e leu sua árvore de controles, mas captura falhou com FrameArrived timed out e a ativação com timeout; clique por acessibilidade também não tinha geometria disponível. Fechamento normal solicitado, sem forçar descarte. Revisão visual/manual permanece pendente; não confundir teste de funções com cliques físicos ou aprovação de legibilidade.

[[04 - Pendencias de Implementacao]] · [[Decks Planejados - Integracao 2026-10-02]]

Unity principal reaberto em Play: log editor-commander-hud.log confirmou Mesa.unity e table=True. As capturas do executável oculto ficaram pretas e não comprovam a aparência do painel.
