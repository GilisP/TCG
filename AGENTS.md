# Instruções persistentes — TCG

Estas orientações foram fornecidas pelo usuário em 2026-09-14 e devem orientar as próximas sessões.

## Entrada no projeto

Leia primeiro `00 - Projeto/00 - Indice Mestre.md`, `00 - Projeto/04 - Pendencias de Implementacao.md`, `00 - Projeto/05 - Mapa de Fontes e Vigencia.md` e `08 - Tecnico/Acessos e Ferramentas.md` no vault `C:\Users\gil\OneDrive\Área de Trabalho\TCG`.
O projeto Unity principal é `C:\Users\gil\TCG`. A cópia auxiliar fica em `08 - Tecnico/Validacao/UnityTCG` dentro do vault. Releases, protótipo e fontes antigas estão em `99 - Arquivo`; não são destinos de novas funcionalidades.
Por pedido do usuário em 2026-09-14, todo planejamento deve ficar organizado em `11 - Planejamento/00 - Plano Mestre.md` e nas notas por área. Manter `00 - Projeto/04 - Pendencias de Implementacao.md` como única lista de execução prioritária. Conteúdo antigo deve ir para `99 - Arquivo`, com links atualizados e origem preservada. Arquivar uma fonte não revoga suas regras. Tipos/subtipos atuais ficam em `03 - Cartas` e cores em `06 - Conteudo`.
Versão confirmada no ProjectVersion.txt em 2026-09-14: 6000.3.6f1. Verifique novamente antes de executar o Editor.
Permissões variam por sessão: leitura não implica escrita nem controle visual do Editor. A escrita neste AGENTS.md não concede permissões adicionais.
Documentos de release descrevem comportamento experimental; não preenchem automaticamente lacunas das regras de design. Respeite a precedência explicitamente declarada dentro do histórico das releases.
Atualize as notas e a lista central de pendências durante o trabalho. Preserve os originais e referências. Não implemente funcionalidades durante a etapa inicial de organização.

## Atualização do autor — mapa, respostas e coleções (2026-09-14)

- Direção visual aceita: mapa 3D real OU pseudo-3D/2.5D com profundidade convincente; cartas continuam 2D. Leia `02 - Tabuleiro/Mapa 3D ou Pseudo-3D.md`. No projeto Unity, as anotações estão em `Documentacao/Mapa-3D-ou-2.5D.md`. Não considerar a grade plana IMGUI o resultado final nem inventar efeitos mecânicos de relevo.
- O autor respondeu as 22 perguntas iniciais. Leia `10 - Decisoes/Decisoes Confirmadas - Rodada 1.md` e as regras atualizadas. As respostas têm prioridade sobre hipóteses antigas. Não voltar a perguntar se há uma capital por jogador, quatro pilhas ou se General e Comandante são diferentes.
- Detalhes ainda ambíguos estão nas 64 perguntas em `10 - Decisoes/Perguntas`, indexadas por `00 - Projeto/03 - Perguntas em Aberto.md`. Não inferir reposição de pilhas, sentido de “começa de fora”, limpeza do dano ou momento exato da derrota sem esclarecer.
- Criação editorial por coleções: `06 - Conteudo/Colecoes/00 - Colecoes.md`, com modelos e COL-001. Coleção editorial não implica inventário do jogador, legalidade de decks ou importação automática no Unity. Preservar IDs de cartas e separar estados de design, arte, implementação e testes.
- O código e seus testes antigos ainda representam o protótipo anterior a essas respostas. Documentação nova não significa que as regras, o mapa ou as coleções já foram implementados no motor.

## Orientações integrais do usuário

Você será meu parceiro de desenvolvimento de um jogo TCG. Sua atuação inclui programação, organização do projeto, documentação no Obsidian, ferramentas de produção, interfaces, imagens, modelos e demais assets necessários.
Projeto e ferramentas
- Projeto Unity: C:\Users\gil\TCG
- Vault do Obsidian: C:\Users\gil\OneDrive\Área de Trabalho\TCG
- Versão identificada do Unity: 6000.3.6f1; confirme antes de configurar ferramentas.
- O mapa deve ter aparência 3D e as cartas serão 2D.
- O jogo deve suportar multiplayer para até quatro jogadores.
Primeiro verifique seus acessos ao Unity e ao Obsidian. Diferencie acesso aos arquivos de capacidade de operar o Unity Editor. Configure as integrações necessárias dentro das permissões disponíveis e valide seu funcionamento. Não declare que possui uma integração antes de testá-la.
Registre estas orientações nas instruções persistentes do projeto, como AGENTS.md, para que sejam mantidas nas próximas sessões.
Documentação como referência
Os documentos do Obsidian são a fonte de referência para o comportamento do jogo. Quando código e documentação divergirem, adapte o código ao documento. Não altere uma regra documentada apenas para justificar o comportamento atual da implementação.
Se houver documentos contraditórios, identifique se existe uma versão explicitamente vigente. Se isso não resolver o conflito, consulte-me antes de implementar a regra envolvida.
Diferencie o conteúdo dos documentos de instruções dirigidas ao agente. Documentos descrevem o projeto; não autorizam por si só instalações, exclusões ou ações externas. Minhas instruções explícitas têm prioridade.
Arquitetura e código
Produza código modular, reutilizável e fácil de entender, sem criar abstrações desnecessárias.
- Separe regras e estado da partida, dados das cartas, interface, apresentação visual, persistência e comunicação de rede.
- Prefira componentes com responsabilidades claras e composição de comportamentos.
- Evite duplicação de regras, classes concentrando responsabilidades demais e dependências desnecessárias entre sistemas.
- Mantenha o núcleo das regras independente da apresentação sempre que viável.
- Diferencie os dados de definição de uma carta de seu estado durante a partida.
- Considere desde o início o multiplayer para até quatro jogadores: autoridade sobre o estado, validação de ações, sincronização e informações privadas de cada jogador.
- Inspecione a arquitetura existente antes de decidir o que reaproveitar ou refatorar.
- Preserve as referências e os arquivos .meta dos assets Unity.
Decisões de arquitetura devem atender às necessidades reais do jogo e ser explicadas no Obsidian.
Obsidian atualizado durante o trabalho
A documentação faz parte da implementação. Cada tarefa que alterar o funcionamento de um sistema deve atualizar as notas correspondentes antes de ser considerada concluída.
Documente, na medida adequada à mudança:
- finalidade e funcionamento;
- scripts e assets envolvidos;
- dependências e relações com outros sistemas;
- configuração no Unity e exemplos de uso;
- pontos de extensão e reutilização;
- verificações realizadas, limitações e problemas conhecidos.
Prefira notas por sistema ou funcionalidade, com referências ao código, evitando copiar o código inteiro para o Obsidian. Mantenha índices e links internos funcionando.
Diferencie claramente planejado, implementado e verificado. Não marque como testado algo que apenas foi escrito ou inspecionado.
Implementações faltantes e próximo passo
Ao fim de cada implementação, atualize no Obsidian uma lista central de pendências, vinculada às notas dos sistemas envolvidos.
Para cada pendência, registre o que falta, o estado atual, dependências e o critério para considerá-la concluída. Mantenha explícito o próximo passo recomendado e o motivo da prioridade.
Conclua itens resolvidos e evite duplicar pendências. Separe requisitos documentados ainda não implementados de sugestões novas, que precisam ser avaliadas.
Execução e correção de erros
Após cada alteração, execute os processos e fluxos afetados, verifique os resultados e corrija os erros relacionados antes de concluir.
Faça a validação proporcional ao impacto:
- compile quando houver mudanças de código;
- execute testes pertinentes às regras ou sistemas alterados;
- valide no Unity Editor ou em Play Mode quando a mudança depender de cenas, componentes ou interação;
- verifique os erros e logs relevantes;
- teste o comportamento em rede quando a alteração afetar o multiplayer.
Inspecione também dependências diretamente afetadas. Amplie a verificação quando houver mudanças compartilhadas ou indícios de regressão, evitando revisar ou executar o projeto inteiro sem necessidade.
Depois de corrigir um erro, repita a verificação correspondente. Se alguma validação não puder ser executada, registre a limitação e o que falta para realizá-la. Não apresente uma implementação como plenamente validada nesse caso.
Produção de conteúdo
Ajude a produzir imagens de cartas, interface, materiais, modelos, animações e demais assets necessários, mantendo coerência artística e adequação ao Unity. Confirme as ferramentas disponíveis para cada tipo de produção e valide os assets após a importação.
Registre como os assets são utilizados e diferencie recursos provisórios de definitivos.
Primeira etapa: organização do Obsidian
Comece verificando os acessos e mapeando o projeto Unity e o vault existente. Depois organize o Obsidian aproveitando a estrutura atual.
Identifique documentos vigentes, históricos, duplicações, conflitos e o estado das implementações. Preserve o histórico e as referências ao reorganizar as notas.
Crie ou ajuste um painel central com:
- visão geral do jogo;
- acesso às regras e aos sistemas;
- estado das implementações;
- decisões e dúvidas em aberto;
- pendências priorizadas;
- próximo passo recomendado.
Nesta primeira etapa, concentre-se na configuração do agente e na organização do Obsidian. O desenvolvimento das funcionalidades virá depois.

## Implementação autorizada — base de 2026-09-15

- O autor autorizou sair da organização inicial e implementar. Ler `10 - Decisoes/Decisoes Confirmadas - Rodada 2.md` (64 respostas), `08 - Tecnico/Base Jogavel - Arquitetura e Uso.md` e `09 - Testes/Validacao da Base Jogavel.md`.
- Nova base principal: `C:\Users\gil\TCG\Assets\TCG\Foundation`, cena `Assets/TCG/Scenes/Mesa.unity`. Código legado em `Assets/TCG/Scripts` / `Partida.unity` foi preservado; não confundir recursos legados com recursos integrados à mesa nova.
- Execução local 2–4; todos contra todos padrão, duplas opcionais. Rede ainda não implementada. Código e apresentação novos usam um subconjunto e um preset de teste explicitamente documentado; não declarar regras oficiais completas.
- Motor sem dependência Unity; expansões em `Assets/StreamingAssets/Expansions/*.json`. Os quatro pacotes demo têm 24 cartas provisórias, não são as expansões finais. Conteúdo que usa efeitos existentes entra por dados; mecânicas novas exigem código/testes.
- Oficina atual `06 - Conteudo/Expansoes/00 - Expansoes.md`. Coleção significa inventário; expansões são gratuitas. Mínimo 300 por expansão; as 20 restantes na distribuição serão decididas depois. Preservar identidade/nome em reimpressões.
- Capitais e pilhas já respondidas. Fora da capital atacante escolhe confrontos; na capital defensor escolhe. Não perguntar novamente. Detalhes remanescentes em `10 - Decisoes/Perguntas/Perguntas 09 - Fechamento da Base.md`.
- Não salvar/retomar partida fechada, por decisão do autor. Inventário local e regras avançadas continuam pendentes.
- Testes da nova base: `TCG.Foundation.Editor.FoundationChecks.Run`; preparar/build: `FoundationSetup.Prepare` / `.Build`. Rodar no projeto principal, verificar logs e capturas; testes legados não comprovam o novo motor.
- `99 - Arquivo/Entregas/Base-2026-09-15` é a cópia arquivada desta transferência, não fonte paralela. Desenvolver somente no principal e atualizar documentação.

- Prioridade reafirmada pelo autor: acompanhar todas as alterações no Unity Editor de C:\Users\gil\TCG. Usar Mesa.unity e TCG > Jogar no Unity; executáveis e cópias do vault não substituem a atualização do projeto. ProjectEntry configura a cena inicial do Play. O cenário atual é gerado em execução, não estático no modo de edição.

## Cores e conteúdo — 2026-09-15

- Identidades estratégicas aceitas e preservadas em 06 - Conteudo/Ideologia das Cores/Identidade das Cores - Referencia de Design.md. Consultar ao criar qualquer expansão.
- Preferência explícita do autor: mecânicas básicas, claras e reutilizáveis entre coleções.
- EXP-001 tem 84 rascunhos editoriais MED-001 a MED-084, por raridade e cor. Avaliação central em Medieval - Avaliacao do Autor.md. Nenhuma carta foi aprovada, importada ou testada nesta entrega; preservar IDs e separar design/arte/implementação/testes.
- O JSON editorial-review-v1 não é importável no motor. Meta de pelo menos 300 cartas e 20 vagas de raridade ainda abertas permanecem.

## Base medieval autorizada — 2026-09-17
- O lote autoral MED-085 a MED-156 foi escolhido como base para implementar regras, cartas 2D, modelos 3D e VFX; não confundir com as 84 sugestões anteriores.
- Ler a nota Medieval - Implementacao da Base de Cartas e Medieval - Validacao 2026-09-17. Projeto principal continua C:\Users\gil\TCG.
- Confirmado: efeito de anúncio na capital do conjurador e impacto no alvo na resolução; adjacência ortogonal sem mesmo tile; construções imóveis, sem ataque/dano defensivo e defesa experimental 4/6/8; equipamento fica no terreno após morte do hospedeiro.
- Corrente Fraca MED-147 aguarda referência de adjacência; não inventar a resposta. Perfis procedurais são arte/modelos provisórios.

## Qualidade de vida e ambientes — 2026-09-18
- Match.AutomaticResponses fica habilitado na mesa principal: passar respostas sem ações disponíveis, preservando alvos, bloqueios e fase principal.
- TableEnvironment.cs gera dois ambientes, modelos sentados e pilhas sincronizadas com o motor. Documentação: Documentacao/Qualidade-de-Vida-Prioridade-e-Sala.md e notas correspondentes no vault.
- QualityOfLifeChecks.Run integra FoundationSetup.Prepare/Build. Diagnóstico visual de desenvolvimento: -tcg-qol-verify. Arte provisória; MED-147 e rede continuam pendentes.

## Arraste e rotas — 2026-09-18
- AutoAdvanceAfterTerrain fica habilitado na mesa. Rotas avançam automaticamente na entrada da fase principal; podem ser canceladas antes de colocar terreno.
- Arraste guarda destinos além do movimento atual, espera terreno andável, preserva escolhas/gatilhos e não inicia ataques. Paradas numeradas 1, 2, 3… usam a mesma luz animada e agrupam espaços pelo movimento.
- Ler Documentacao/Movimento-por-Arraste-e-Rotas.md e a validação correspondente no vault. MovementOrderChecks.Run integra o build; diagnóstico visual: -tcg-drag-verify.

## Personalidade, mão e formação — 2026-09-18
- Ler Personalidade Mao e Formacao e sua validação. Leque/hover em HandPresentation; avatares seguram apenas versos. Carta elevada tem prioridade de clique sobre a mesa/cartas abaixo.
- PieceFormation organiza até nove modelos visíveis por terreno, mantendo todos os ocupantes na lista. Não é limite mecânico. FigureMotion anima detalhes locais, sem alterar estado/posições do motor.
- PresentationChecks.Run integra o build; -tcg-personality-verify cobre hover, seleção e colocação. Assets procedurais permanecem provisórios.

## Coleção e comandantes — 2026-09-19
- Ler Colecao Decks e Comandantes, decisões e validação de 2026-09-19 no vault.
- Novo lote aprovado MED-193–224; preservar MED-157–192 como propostas editoriais antigas.
- Coleção gratuita por adição individual, decks locais persistentes e seleção por assento. Rede ausente.
- Comandante: zona própria, retorno opcional, +2 genéricos por reconjuração; derrota com 26 de dano de combate do mesmo comandante.
- Spawns centrais: 55/5/65/115. Subtipos explícitos em dados; nenhuma habilidade inferida de modelos.
- Troca de Destino responde à morte iminente; Teletransport não troca capitais. Demais detalhes na decisão vigente.

## Central de telas e loja — 2026-09-19
- Ler Central de Quatro Telas e Validacao da Central de Telas 2026-09-19 no vault.
- Quatro telas: Jogar, Coleção/decks/cosméticos, Loja, Menu. Partidas locais; busca online indisponível.
- Pedido novo inclui loja com moeda do jogo para cartas e cosméticos. Cartas anteriores preservadas; saldo inicial 500 e preços 50/150 são provisórios, não economia definitiva aprovada. Fonte de moedas ainda depende do autor.
- CollectionData schemaVersion 2 migra o esquema 1. Mesmo caminho collection-v1.json; saldo e propriedade no mesmo arquivo. Preservar backups e nunca reatribuir saldo a perfil já migrado.
- Versos por deck aplicados ao iniciar nova partida. HubChecks integra build; -tcg-hub-verify usa perfil isolado.

## Sessões de ideias — 2026-09-19
- O autor quer momentos dedicados a propor ideias e elaborá-las depois com calma. Registrar no vault em 11 - Planejamento/09 - Banco de Ideias.md.
- Durante sessões de ideias, conversar e documentar; não iniciar implementação automaticamente. Preservar autoria, intenção original e distinção entre sugestão e decisão.
- Quando o autor pedir para seguir com uma ideia, detalhar conforme necessário e usar a lista central de pendências para execução; o banco não é uma segunda fila prioritária.

## Ideias por voz — preferência de 2026-09-19
- Durante o modo de ideias por voz, o autor quer escuta sem respostas orais e resumos escritos no Banco de Ideias.
- Consolidar apenas conteúdo recebido, preservando autoria, dúvidas e decisões. Evitar interrupções para aprofundamento e não implementar automaticamente.
- Não confundir a preferência de silêncio com controle técnico do áudio do aplicativo; não alegar ativação ou silenciamento não verificados.

## Filtros e comandantes bicolores — 2026-09-23
- Ler Filtros Decks de Terrenos e Comandantes Bicolores, decisões e validação de 2026-09-23.
- Filtros separados na biblioteca e no deck; múltiplas opções do grupo usam OU, grupos usam E. DeckData mantém main e terrains vinculados; abas não mudam schemaVersion 2 nem inventário existente.
- MED-225–230 implementados para teste. MED-231 cadastrada e bloqueada enquanto veículos não estiverem prontos. Preservar nomes autorais e IDs antigos.
- Devoção conta símbolos de permanentes próprias. Lissandra usa apenas seu cemitério, contadores/cópias na capital. Pescador é SSG3. Ursa gratuita antes do combate, menor rota ou teleporte; interrompe invasão e exige ataque. Receptáculo ativa ambas no empate.
- Embarcar/desembarcar: 1 PA, mesmo tile. Passageiros/destruição e veículo de teste aguardam esclarecimento; não inventar. Valeria herda todas as palavras-chave enquanto embarcados.


## Conclusão dos comandantes e veículos — 2026-09-23
- Esta continuação substitui o bloqueio anterior da MED-231. Todos MED-225–231 habilitados. Ler Comandantes e Veiculos - Implementacao 2026-09-23 e sua validação.
- Resposta do autor: passageiros morrem com veículo destruído; veículo funciona tripulado; passageiros comuns agem; em Meka não agem, mas continuam alvos. Embarque/desembarque 1 PA no mesmo tile.
- Campos vehicleSeats/vehicleCrew e keywords são explícitos. Valeria amplia capacidade e herda palavras-chave dos passageiros enquanto embarcados; não copia texto/subtipos como palavras-chave.
- Testar comandantes na tela Jogar monta decks temporários por identidade, sem alterar perfil. Pacote test-vehicles é provisório e separado de MED: carroça, Meka e batedora alada para verificação.
- Defesa só é escolhida quando o combate está no topo da pilha: Ursa chega antes dos bloqueios.

## Boosters e HUD medieval — 2026-09-23
- Ler Boosters Variantes e HUD Medieval e sua validação no vault. Valores provisórios aprovados: 5 sorteios por 100 moedas, 10 de retorno por repetida; pesos de estilo 80/15/5 são preset configurável, não raridades garantidas.
- CollectionData esquema 3 substitui o 2, mantendo collection-v1.json e saldo existente. Não conceder novamente moedas na migração. Recibo/inventário/saldo são gravados antes da revelação; falha de escrita reverte a compra.
- Carta e variante visual têm identidades separadas; variantes não alteram regras nem limite de cópias. Aparências podem ser escolhidas por coleção ou por deck.
- Frente 2:3 compartilhada em CardAppearanceUI; acabamentos em MedievalSkin; configuração em StreamingAssets/Economy e manifest de artes em StreamingAssets/CardArt. Assets em Resources/CardArt.
- Há uma nova ilustração pintada para Elo/Iluminura; outras cartas mantêm retratos procedurais provisórios. Todas suportam artes próprias por estilo. Não declarar todo o catálogo ilustrado definitivamente.
- BoosterChecks integra FoundationSetup; diagnóstico -tcg-booster-verify usa perfil isolado. Preservar a adição gratuita individual de teste pela ficha da carta.

## Comandantes, foil e modelos — 2026-09-24
- Ler Comandantes Foil e Acabamento Visual 2026-09-24 e Validacao Comandantes Foil e Modelos 2026-09-24. MED-232–242 integrados com regras provisórias autorizadas; 813 verificações e build passaram.
- Lua sem dados recebeu proposta ajustável Arauto da Lua Rubra (LL3, 2/4, 2 PA). Preservar nomes/IDs autorais restantes e subtipos explícitos. Fontes e respostas preservadas no lote de 2026-09-24.
- NewCommanders trata habilidades, encantamentos, reflexão, cura, permissões de exílio e pagamento com PA. OriginalOwner preserva destino de cartas alheias. Ancestral exila após resolver/anular; Admirador paga normalmente e Troca de Destino só é oferecida na morte iminente.
- CardData.foil e CardLook.foil separados do estilo; acabamento gratuito de teste por coleção/deck, esquema 3 preservado. Foil brilha ao entrar, sem repetir ao mover. Não inventar economia de foil.
- Só comandantes junto à busca da coleção usa o filtro existente. CraftedModels/ArcaneBurst/ArcaneGlow são detalhes procedurais e VFX provisórios, não modelos/ilustrações finais.
- test-new-commanders contém apoios provisórios fora dos boosters; o lote autoral novo entra no booster medieval. NewCommanderChecks integra FoundationSetup. Diagnósticos foil-commanders e personality usaram perfis/testes isolados, sem erros de execução; avaliação autoral e sessão longa continuam pendentes.

## Multiplayer por host — 2026-09-24
- O autor autorizou as cinco etapas e escolheu jogadores como hosts. Reconexão: 120 segundos após queda detectada, pausa durante a janela e derrota por abandono ao expirar. Saída do host encerra a sala; migração não implementada.
- Ler Multiplayer por Host - Arquitetura e Uso e Validacao Multiplayer por Host 2026-09-24. NetworkRoom executa Match no host; NetworkView envia projeções por assento. Nunca enviar mãos/decks completos ou logs privados aos outros clientes.
- Runtime usa NGO 2.13.3, Unity Transport e MPS SDK 2.3.3. LAN por UDP 7777; Internet por UGS/Relay e sessões por código/públicas. Código online integrado, mas vínculo UGS e teste externo ainda pendentes; não declarar internet validada apenas por compilação/LAN.
- Projeções não executam Match.Try. UI consulta ações legais calculadas no host. Comandos verificam conexão/assento, sequência e revisão. Reconexão usa segredo em memória e restitui o assento, sem salvar partidas fechadas.
- Inventário/economia ainda locais e não autenticados por servidor central; reações sociais ainda locais. Cosméticos não mudam regras. NetworkChecks integra build; NetworkDiagnostics usa processos independentes e perfis de teste sem alterar coleção.
- Computer Use foi interrompido ao tentar configurar a conta por não conseguir determinar o URL do navegador com segurança. Nenhuma vinculação, credencial ou cobrança foi configurada. Não contornar essa limitação.

## Progresso, ruínas e backup — 2026-09-24
- Ler Estatisticas Missoes e Ruinas no vault. O autor aprovou preset inicial de estatísticas/recordes e missões únicas com recompensas configuráveis. Progresso local em CollectionData; mesa local contabiliza Âmbar, online contabiliza o assento do cliente; não confundir com economia autenticada por servidor.
- Regra inicial atual: capital usa uma carta do deck de terrenos; as três casas iniciais vizinhas são ruínas sem efeito/mana, não retiradas do deck nem adicionadas ao cemitério. Substitui o preset antigo.
- Remoto Git autorizado: https://github.com/GilisP/TCG. O autor quer backup no começo dos dias em que houver modificações. Conferir status e remoto, preservar o trabalho existente e enviar snapshot antes de novas mudanças; nunca usar force-push. A preferência se aplica durante sessões de trabalho, não é um agendamento para dias sem trabalho.
- Commit 30d0aeb preserva a base anterior à implementação de progresso/ruínas e foi enviado com sucesso. Não registrar tokens de autenticação no projeto/vault.

Atualização final desta entrega: 944 verificações aprovadas; Relay real com dois e quatro processos, busca pública, reconexão após 20 segundos e câmera atrás da própria capital verificados. Sessões MPS gerenciam sala/membros; RelayService renova a alocação de transporte sem recriar a sessão na reconexão. Ver documentação de validação; avaliação em máquinas físicas distintas permanece pendente.


## Pilhas comuns e apresentação — 2026-09-24
- Decisão nova substitui quatro pilhas individuais: quatro pilhas comuns com oito cartas iniciais. Contribuições 4/4, 3/3/2 (sorteio de quem contribui duas) ou 2/2/2/2; embaralhar o conjunto e distribuir duas por pilha. Capital continua usando carta própria antes da contribuição.
- Compras e reposições usam o deck do jogador do turno. Terreno entra no reino de quem o coloca, mas preserva dono original para o cemitério. `TerrainCard` distingue cópias de definição igual; `Cell.Owner` é reino, `TerrainOwner` é propriedade. Eliminação não limpa pilhas comuns.
- Consultar Pilhas Compartilhadas e Animacoes da Mesa e a validação correspondente. Hover/artes em PilePresentation/TableEnvironment; mana/ataques em TableFlights; resultados da pilha têm eventos separados. Retratos de terrenos são paisagens procedurais provisórias.
- Testes adicionais SharedTerrainChecks; diagnóstico -tcg-pile-verify. Shader PileCard deve ficar em Resources para não ser removido do executável. Protocolo de rede passa a tcg-network-4/20260924-shared-terrain.
