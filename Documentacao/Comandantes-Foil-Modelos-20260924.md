# Comandantes, foil e acabamento visual — 2026-09-24

Projeto principal: `C:/Users/gil/TCG`. Fonte: [[Lote Autoral - Cartas 2026-09-24]] e [[Esclarecimentos - Lote Autoral 2026-09-24]].

## Escopo
MED-232–242 recebem programas próprios, subtipos explícitos e regras provisórias autorizadas pelo autor. O nome/atributos da comandante de Lua são proposta ajustável: Arauto da Lua Rubra, LL3, 2/4, 2 PA. Nenhuma alteração dos IDs anteriores.

Implementado para testes, com 813 verificações automatizadas e build aprovados. Evidências e limites em [[Validacao Comandantes Foil e Modelos 2026-09-24]]. Isso não comprova todas as combinações possíveis nem substitui a avaliação autoral.

## Sistemas
Core/NewCommanders.cs reúne os gatilhos/ações do lote, permissões individuais de conjuração do exílio, pagamento com PA, cura centralizada e reflexão. Uma carta exilada possui permissão própria e dono original; conjurar consome a permissão. O exílio pode sobreviver à saída do comandante. A origem da carta é preservada ao conjurar/controlar cartas alheias.

Encantamento é um tipo explícito novo. Anexos acompanham o hospedeiro, contribuem continuamente e vão para o cemitério quando ele sai. Colecionador pode tomar os encantamentos da criatura morta por seu dano. Equipamentos continuam no terreno quando o hospedeiro morre.

Apoios provisórios em test-new-commanders.json: Runa de Vigor, Runa de Fraqueza, Erosão da Memória (duas cartas de um grimório ao cemitério), Dissipar (anula a última magia comum da pilha) e terreno incolor. Servem para testar anexos, moagem, anulação e comandante incolor; não são cartas medievais autorais aprovadas e não entram nos boosters.

Ancestral oferece apenas magias com alvos disponíveis e respeita a restrição de Troca de Destino à morte iminente. Admirador pode conceder permissão para Troca de Destino: ela aparece automaticamente na resposta à morte, com pagamento normal, consumindo a permissão. A janela de exílio informa essa restrição.

## Uso
- Coleção: marcar Só comandantes junto à busca. O atalho e o painel de filtros compartilham o mesmo campo, sem duplicar filtro.
- Ficha de carta possuída: marcar Foil e usar na coleção ou no deck. Acabamento gratuito provisório; não altera chances/preços dos boosters.
- Jogar → Testar comandantes: catálogo inclui o lote e monta decks por identidade; incolor recebe terrenos incolores.
- Habilidades ativadas: selecionar peça e Ativar habilidade. Monge também aparece como resposta quando há pilha e ação disponível.
- Exílio: botão durante a partida lista as cartas com permissão e permite conjurá-las com custos/alvos normais.

## Foil
CardData.foil e Definition.Foil são o padrão de definição. CardLook.foil guarda preferência por identidade na coleção/deck, independente de styleId. Perfil antigo sem esse campo usa false; esquema 3, saldo e propriedade preservados. SelectStyle mantém o acabamento ao trocar arte.

Runtime/FoilPresentation resolve a aparência por jogador e emite brilho apenas em entrada de permanente/terreno ou resolução/impacto de magia. Movimento não repete o brilho. Invocação/cópia também usa o evento de entrada. A apresentação recebe um resolvedor de cosméticos, sem alterar estado de regras.

## Modelos e VFX
CraftedModels acrescenta juntas arredondadas, malha, punhos, fivelas, detalhes de capa, tomos, gemas, garras e penas; Aberração tem espinhos e olho próprio. Tudo ainda procedural/provisório; não é produção de modelos finais.

ArcaneBurst e Resources/ArcaneGlow.shader usam círculos rúnicos, centelhas com cauda, luz local e variação por elemento. Foil combina tons dourados e iridescentes. Anúncio continua na capital do conjurador; impacto somente na resolução. Efeitos têm duração curta, materiais liberados e limite de 32 simultâneos por mesa; reiniciar mesa limpa efeitos.

## Extensão
Novas cartas entram em StreamingAssets/Expansions. Novas artes seguem o manifest existente. Foil não cria uma nova identidade mecânica nem permite cópias extras no deck. As regras de obtenção definitiva de foil continuam para decisão futura; execução central em [[04 - Pendencias de Implementacao]].
