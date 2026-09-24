# Reações dos Jogadores

Implementado em `C:\Users\gil\TCG`, na mesa local. Clique no avatar do jogador que está no controle para abrir Joia, Bravo, Feliz, Triste e Comemorar. Joia, Bravo e Comemorar permitem escolher toda a mesa ou a cor/nome de outro jogador. Feliz e Triste são públicos. Esc, X ou clique fora fecham o menu.

O balão fica acima do autor durante cinco segundos; uma nova reação substitui a anterior, com intervalo mínimo de um segundo. Reações não gastam recursos nem avançam turnos. Na mesa compartilhada, o avatar utilizável acompanha o controlador atual; os demais não abrem o menu. Escolhas obrigatórias e troca de jogador bloqueiam a interação.

## Estrutura
- `Core/SocialReactions.cs`: dados e validação do canal social, separado do estado/revisão da partida. Futuro transporte de rede deve autenticar o autor; rede ainda ausente.
- `Runtime/PlayerReactions.cs`: seleção do avatar, menu, destino por cor e balões.
- `Runtime/ReactionIconArt.cs`: ícones procedurais provisórios, independentes das fontes de emoji do Windows; texturas liberadas ao destruir a mesa.
- `Runtime/TableEnvironment.cs`: collider dos avatares, criado junto dos lugares da mesa; não exige configuração manual na cena.
- `Editor/ReactionChecks.cs` e `Runtime/ReactionDiagnostics.cs`: validação de canal e fluxo de apresentação.

A nova partida limpa o canal; nenhum histórico social é persistido. Extensão: acrescentar um tipo ao enum, ícone, texto e política de destino. Arte definitiva e comunicação entre máquinas permanecem fora desta implementação.

## Correção de colocação de cartas
A introdução do arraste havia feito `TableView.OnGUI` limpar o estado do botão do mouse sempre que havia uma carta selecionada. Assim, o clique iniciava no terreno, mas era perdido antes de soltar o botão. A correção preserva esse estado e oculta somente as luzes de movimento enquanto se joga uma carta. A seleção pode colocar cartas em terrenos vazios ou ocupados, inclusive quando o clique atinge uma miniatura.

Ver [[Validacao das Reacoes 2026-09-18]] e [[Movimento por Arraste e Rotas]]. Pendências centralizadas em [[04 - Pendencias de Implementacao]].
