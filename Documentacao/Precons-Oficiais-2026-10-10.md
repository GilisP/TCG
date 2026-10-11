# Precons oficiais — COL-001

Implementação de 2026-10-10 no projeto principal C:/Users/gil/TCG. Dez listas aprovadas pelo autor: 100 cartas principais, 50 terrenos e comandante separado. Fontes originais e resolução de nomes permanecem em Documentacao/Design/COL-001-20261009; os 29 decks planejados anteriores não foram substituídos.

## Uso

Coleção → Precons mostra as dez listas. Importar uma lista ou todas é uma ação gratuita explícita: acrescenta as cartas e salva o deck. Reimportar preserva decks já editados, moedas, variantes e foil. A escolha de deck antes da mesa local e a preparação online também oferecem os precons temporariamente, sem conceder cartas à coleção. Cada assento escolhe seu deck; mudar a escolha online retira a prontidão.

Roubo de tumbas; Arquivo das marés; Banquete da horda; Cerco das cem lanças; Oficina de ossos; O trono que não recua; Flechas do horizonte; O jardim dos pequenos mortos; Caravana das correntes; Arsenal das mil feras.

## Dados e regras

Assets/StreamingAssets/Decks/official-precons.json contém as listas COL001-D01–D10 e seus registros editoriais de raridade. A expansão official-collection-001.json acrescenta 728 definições; definições existentes são reaproveitadas por identidade. As posições editoriais principais mantêm 50/20/15/15 comuns/incomuns/raras/míticas; terrenos usam 25/10/8/7. Nenhuma lista repete identidade canônica. IDs, textos e fontes aprovadas são preservados; versões revisadas e terrenos temáticos recebem os nomes distintos autorizados.

Tools/import_official_collection.py traduz os textos editoriais em operações explícitas, usando precon_library_rules.py, precon_unit_rules.py e precon_author_rules.py. O motor não interpreta texto durante a partida. implementation-audit.json registra 728 definições traduzidas, sem pendências de tradução. Isso não representa teste individual exaustivo de todas as combinações.

CardAbilities valida custos, alvos e condições. PreconLibraryEffects, PreconUnitEffects e PreconAuthorRules executam os efeitos no Match oficial; PreconIntegration conecta colocação, combate e proveniência. Interface apenas solicita ações. O host mantém autoridade e envia projeções privadas por assento; o futuro PvC deve consumir essas mesmas ações.

Exílio secreto permanece privado; cartas roubadas conservam dono original. Salvas declaram os pares antes de pagar/entrar na pilha. Indestrutível não impede sacrifício, exílio nem defesa zero. Encantamentos globais e de terreno têm colocação própria. Terrenos de mana aleatória não recebem transporte dos Portões.

Decisão do autor em 2026-10-10: a criatura reanimada por Senhor das Catacumbas Despertas (COL001-R016) aparece no terreno do Senhor. A habilidade exige a fonte em campo e terreno válido; mantém o dono original e os gatilhos de entrada.

## Integração e extensão

PreconstructedDecks centraliza integridade/importação; PreconstructedDeckUI usa as frentes, artes, variantes e foil existentes. ContentLoader, CollectionUI, HubUI e NetworkUI integram a seleção. Não houve alteração do esquema 3 do perfil nem concessão automática de saldo. Artes/modelos atuais continuam provisórios.

Protocolo: tcg-network-7/20261010-official-precons. Todos os jogadores devem usar a mesma versão. O transporte por host/Relay permanece existente; mudanças desta entrega são de conteúdo, ações e projeções.

PC-001 demonstra especialistas com contextos reduzidos e escopos de arquivos separados; contratos, responsáveis, relatórios e ponto de retomada ficam em Documentacao/Desenvolvimento. Arquivos compartilhados pertencem ao coordenador. Não foram necessários worktrees porque as edições paralelas foram separadas por arquivo.

## Verificação e limites

FoundationSetup.Build compila, valida catálogo/listas e executa verificações de regras, importação, persistência e rede. PreconIntegrationChecks verifica 2/4 assentos, comandos inválidos/duplicados, privacidade e reconexão. Os diagnósticos -tcg-precons-verify e -tcg-precons-net usam perfis isolados. Resultados finais ficam na nota de validação no vault.

Balanceamento, avaliação autoral de partidas longas, arte definitiva e testes entre computadores físicos continuam pendentes. Tradução completa não substitui essa avaliação. Relay não é considerado novamente validado apenas por LAN nesta entrega.

Resultado final: build e 29795 verificações aprovadas; diagnóstico de coleção 14 verificações; LAN 2/4 com reconexão e zero erros. Evidências em Documentacao/Desenvolvimento/PC-001-I-result.json. Capturas ocultas pretas; revisão visual manual e balanceamento pendentes.

## Projeções e retomada

MovementOrders calcula todos os destinos com duas buscas por peça; MovementRoute continua disponível como referência. NetworkView usa helpers compartilhados de elegibilidade/alvo em Match e MedievalRules: atributos de movimento/alcance e disponibilidade da carta são consultados uma vez por projeção, sem cache entre estados. MovementProjectionChecks compara 3960 caminhos; ActionProjectionChecks compara 22314 ações com APIs públicas e condições anteriores, incluindo alterações de aura/mana sem mudança da revisão. Não há uma segunda implementação das regras na interface.

NetworkSession limpa a espera por confirmação ao desconectar; a reconexão restaura a sequência do assento. Diagnósticos usam semente controlada 20261010 e perfis isolados; partidas normais usam semente aleatória. LAN final: 2 processos na porta 7926 e 4 na 7924, com duas quedas forçadas de guest1, uma após envio de ação. O host de quatro registrou máximo de 760 ms por pedido e 395 ms por transmissão conjunta; medições locais não substituem teste entre máquinas. Tentativas anteriores com quedas e falhas de captura permanecem descritas no relatório, sem serem apresentadas como validação final.
