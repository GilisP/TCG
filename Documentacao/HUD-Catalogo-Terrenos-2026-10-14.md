# HUD e catálogo oficial — implementação 2026-10-14

[[12 - HUD Catalogo e Terrenos]] · [[04 - Pendencias de Implementacao]]

Implementado e integrado; build/runtime/LAN verificados, avaliação visual pendente: menus/HUD com paleta grafite, azul petróleo e latão, botões com hover e pressionamento, relevo discreto, títulos serifados e texto Segoe UI. Frentes de cartas recebem molduras suaves, título ajustável e rodapé legível; foil mantém identidade/efeito e marcador fora das estatísticas. Seleção de decks separa nome, comandante e quantidades. Não houve reforma do tabuleiro nesta etapa.

## Catálogo vigente
Decisão do autor em 2026-10-14: considerar somente os dez precons oficiais e manter todos os comandantes. União ativa: **1.250 definições**, sendo **29 comandantes**. **279 definições** fora desse conjunto deixam de ser oferecidas em biblioteca, loja, boosters e decks de jogo. Corpus histórico: 1.529 definições. Não apagar perfil, saldos, variantes, cosméticos, rascunhos ou fontes editoriais. Os registros de propriedade de cartas retiradas são preservados, sem habilitar seu uso.

ContentCatalog.Cards enumera conjunto ativo. Get/Identity preservam leitura das definições históricas para perfil e referência interna; IsActive/ValidateDeck impedem aquisição e decks com inativas. Match valida listas ao construir partida e NetworkRoom valida no host. LoadFixtures é uma entrada explícita para testes históricos, não o catálogo normal. As 29 listas planejadas antigas permanecem em arquivo e ficam fora das ofertas atuais. Os dez precons oficiais continuam com 100 principais, 50 terrenos e comandante separado.

Boosters preservam preço100, cinco cartas e retorno10; preset expansions=[] utiliza o catálogo ativo. O método Pool continua respeitando filtros de expansão configurados. Taxas de acabamento preservadas. Nenhuma moeda adicional foi concedida nesta entrega.

## Scripts e extensão
Runtime: MedievalSkin, TableView, HubUI, CardAppearanceUI, CommanderUI, DeckSelectionUI, CollectionUI, PreconstructedDeckUI, PlannedDeckUI, ContentLoader, NetworkUI. Core: ContentCatalog e CollectionLibrary. Preset booster em StreamingAssets/Economy/booster-medieval.json. Recursos de frente e skin comuns também refinam coleção, loja, História e mesa sem duplicar regras.

Rede: novo fingerprint **tcg-network-8/20261014-official-catalog**. Todos os jogadores precisam desta versão. O preset online índice0 passa a ser o precon oficial Cerco das Cem Lanças. Validação autoritativa e informações privadas preservadas.

## Verificações e limites
FoundationSetup.Build inclui OfficialCatalogChecks; suítes de regras antigas usam LoadFixtures quando dependem de conteúdo retirado. Suites de precons, integração, História e boosters usam catálogo ativo. OfficialInterfaceDiagnostics usa perfil isolado e percorre menus, seleção, partida, booster e mapa. Revisão visual, build e LAN desta entrega serão registrados em [[Validacao do HUD e Catalogo 2026-10-14]]. Acabamento procedural e ilustrações anteriores continuam provisórios; não declarar arte definitiva ou terrenos reformados.

# HUD, catálogo oficial e terrenos — 2026-10-14

## Escopo autorizado
Primeiro melhorar HUD, menus e apresentação das cartas; depois reformar o tabuleiro. O autor permite abandonar a direção low-poly. Catálogo de jogo limitado às cartas dos dez precons oficiais e a todos os comandantes, confirmado pelo autor. Os 29 decks antigos não definem o catálogo ativo desta etapa. Fontes históricas e perfil do jogador devem ser preservados.

## Direção visual
Medieval refinado: grafite e azul petróleo, latão, tipografia serifada nos títulos e texto de leitura simples. Superfícies discretas com profundidade, destaque consistente para ações e menos ruído ornamental. Cartas permanecem 2D e preservam variantes, foil e regras. Reutilizar componentes comuns para coleção, decks, loja, História e HUD.

## Terrenos — planejamento, sem novas regras
A próxima etapa altera a aparência das definições existentes, sem inferir efeitos pela arte. Usar um perfil visual separado da identidade, mana, dono e estado do terreno. O perfil não muda movimento, alcance, geração de mana ou gatilhos.

| Família visual | Elementos de arte propostos | Leitura no tabuleiro |
|---|---|---|
| Ruínas | Pedra gasta, lajes quebradas, vegetação discreta | Diferenciar claramente das cartas que geram mana |
| Floresta e jardim | Copa arredondada, raízes, grama e caminhos | Vegetação nas bordas, área central livre para peças |
| Rios e portos | Água com reflexo suave, cais, cordas e pontes | Superfície e bordas continuam legíveis; água não inventa bloqueio |
| Montanhas e pedreiras | Rochas detalhadas, neve ou minério, trilhas | Alturas moderadas; nada oculta peça, rota ou seleção |
| Fortalezas e cidades | Muralhas, bandeiras, pisos e torres | Edifícios menores nas bordas e centro para ocupantes |
| Cemitérios e criptas | Lápides, lanternas e pedra escura | Silhueta própria sem confundir com buraco |
| Vulcões e forjas | Basalto, metal e luz quente | Brilho localizado sem competir com mana/ataques |
| Santuários e academias | Runas, mosaicos, vitrais e livros | Cor de afinidade como acento, sem representar regras extras |
| Portões e terrenos especiais | Arcos, mecanismos e sinais próprios | Variações por carta, preservando posição e interações |

## Ordem da próxima etapa do tabuleiro
1. Inventariar terrenos ativos e atribuir perfis visuais sem depender apenas da cor.
2. Produzir uma amostra de ruína, floresta, rio e fortaleza, incluindo versões de arte das cartas.
3. Validar seleção, rotas numeradas, peças no mesmo terreno, capital e quatro câmeras.
4. Aplicar materiais e iluminação coerentes; medir desempenho no hardware de referência antes de ampliar.
5. Avaliar com o autor; então substituir modelos restantes. Assets procedurais atuais continuam provisórios.

## Contratos e verificação
Coordenador integra catálogo, compatibilidade de rede, testes e notas; especialistas editam escopos disjuntos definidos em Documentacao/Desenvolvimento/UI-CAT-014-contract.json. Engine Match continua fonte de validação; apresentação não cria regras. A catalogação deve bloquear compra/importação/deck de cartas retiradas, preservando IDs históricos no perfil. Testes históricos usam catálogo de fixtures explícito.

Estado: HUD e catálogo implementados e integrados. Build39.581, runtime13 e LAN16/16 passaram; avaliação estética manual pendente. Terrenos somente planejados. [[HUD e Catalogo Oficial - 2026-10-14]] · [[Validacao do HUD e Catalogo 2026-10-14]]. A fila única está em [[04 - Pendencias de Implementacao]].

# Validação do HUD e Catálogo — 2026-10-14

[[HUD e Catalogo Oficial - 2026-10-14]] · [[12 - HUD Catalogo e Terrenos]]

Unity6000.3.6f1, projeto principal C:\Users\gil\TCG. FoundationSetup.Build terminou com código0; **39.581 verificações aprovadas**, incluindo12 específicas do catálogo oficial. Catálogo ativo1.250, comandantes29, histórico1.529. Dez precons prontos; terreno da capital e demais regras preservados.

Runtime OfficialInterfaceDiagnostics:13checks, zeroerros. Renderização dos quatro menus, escolha de precon, partida com comandante correto, compra e animação de booster com recompensas ativas, mapa da História. Teste verifica execução e estado, não aprovação estética. Perfis isolados.

LAN em dois processos:16checks no host e16 no cliente; seleção local, busca/cancelamento, confirmação autoritativa e prontidão após troca. Ambos zeroerros. Relay/reconexão não foram repetidos nesta entrega; cobertura anterior não se transforma em nova validação.

Perfil pessoal SHA256 antes/depois idêntico:8303FA1A105818CD4E13AD0F7980FB9D980EE3670668454CA7E3BCF0238FC175. Nenhum saldo, deck ou inventário pessoal modificado pelos testes.

Tentativas iniciais do build revelaram duas expectativas antigas de teste: booster obrigatório em expansões antigas e manutenção das29 listas na oferta ativa. Foram atualizadas à decisão do autor; filtros de expansão e preservação do corpus histórico continuam verificados. Terceira execução completa aprovada.

Revisão visual via skill computer-use tentou ativar a janela real do executável: failed to activate captured window. Recuperação com seleção atualizada e captura: FrameArrived timed out. Nenhum clique foi feito sem observação válida. Aprovação de legibilidade/beleza, desempenho prolongado e reforma dos terrenos seguem pendentes. Avisos antigos do legado não foram tratados como validação da interface nova.

Evidências:Logs/ui-catalog-20261014-build-r3.log; OfficialInterfaceVerification/result.txt; DeckSelectionVerification/host.txt eguest.txt; Documentacao/Desenvolvimento/UI-CAT-014-result.json. Arquivos de runtime sob Builds/BaseJogavel e perfis de testes independentes.
