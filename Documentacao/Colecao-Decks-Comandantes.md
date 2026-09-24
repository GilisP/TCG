# Coleção, decks e comandantes
Atualização: 2026-09-19. Fonte: [[Decisoes Confirmadas - Comandantes e Colecao 2026-09-19]].

## Uso no Unity principal
Projeto C:\Users\gil\TCG, cena Mesa.unity, menu TCG > Jogar no Unity.
Abra Coleção e decks, encontre a carta e adicione-a gratuitamente à coleção. Crie um deck, adicione cartas e escolha o comandante. Salve o rascunho; no menu da mesa, selecione decks salvos e atribua um a cada jogador.
Deck padrão: 100 cartas principais, 50 terrenos, comandante separado; identidade de cores e limite de cópias são validados. Portões admitem duas cópias. Deck experimental: mínimo de 5 principais e 12 terrenos, com repetições, para testar o catálogo incompleto. Rascunhos inválidos podem ser salvos, mas não iniciam partida.
Todos os lugares usam a coleção local. Não há autenticação, comércio, pacotes pagos ou sincronização em rede. Partidas não são salvas.

## Organização técnica
- Core/Content.cs: definições, subtipos explícitos, identidade e limites de cópias.
- Core/CollectionLibrary.cs: inventário por identidade de reimpressão, rascunhos e validação.
- Runtime/CollectionStore.cs: JSON em Application.persistentDataPath/Library/collection-v1.json; gravação temporária, substituição e backup. Arquivo inválido bloqueia escrita e tenta recuperar backup. IDs de expansões ausentes são preservados.
- Runtime/CollectionUI.cs: biblioteca, coleção, edição e seleção por assento.
- Core/Commanders.cs e Runtime/CommanderUI.cs: zona própria, custo adicional de 2 genéricos por reconjuração e opção de retorno após morte. Dano de combate acumulado por comandante derrota ao atingir 26.
- Core/AuthorRules.cs, AuthorMovement.cs e DeathResponses.cs: efeitos do lote, missões, deslocamentos, redirecionamento e resposta à morte.
- Runtime/AuthorModels.cs e FigureDetails.cs: modelos procedurais e detalhes adicionais dos comandantes. Arte provisória.
Todos os caminhos de scripts partem de C:\Users\gil\TCG\Assets\TCG\Foundation.

## Conteúdo
32 cartas aprovadas em Assets/StreamingAssets/Expansions/medieval-author-20260919.json: MED-193–224, incluindo dez comandantes. A faixa MED-157–192 continua reservada às propostas editoriais anteriores. [[Lote Aprovado - Comandantes e Apoio 2026-09-19]] preserva o texto original e fichas individuais.
Subtipos são dados explícitos; a classificação editorial deve ser revista pelo autor. O motor não deduz subtipo pelo modelo visual nem concede Voar pelo nome.
Troca de Destino abre uma oportunidade quando a criatura morreria. Ao substituir dano letal, restaura o dano anterior ao pacote fatal, preservando ferimentos anteriores; não deixa proteção permanente.
Teletransport exclui capitais. Fichas de missão avançam automaticamente, aguardam terreno e atacam bloqueadores. Portões usam um passo de movimento. Ponte não encadeia outra Ponte no mesmo deslocamento.
Spawns agora ficam nos centros das quatro paredes: índices 55, 5, 65 e 115.

## Extensão e limites
Novas cartas com efeitos existentes entram por JSON. Mecânicas novas exigem código e testes; nenhuma arquitetura elimina essa necessidade.
Persistência possui versão de esquema 1; migrações futuras ainda precisam ser implementadas. Arte, balanceamento, partida manual prolongada e rede permanecem pendentes. Ver [[Validacao de Colecao e Comandantes 2026-09-19]] e a lista central [[04 - Pendencias de Implementacao]].
