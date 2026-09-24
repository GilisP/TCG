# TCG — versão 0.6: Confluências

Esta etapa acrescenta **43 definições** ao projeto `C:\Users\gil\TCG`: **17 terrenos multicoloridos, 12 criaturas, 12 magias e 2 fichas**. O catálogo original passa a **257 definições**, além das cartas personalizadas. As definições antigas incluem variantes de mecânicas repetidas; o total não representa 257 mecânicas únicas.

Foram criadas **seis ilustrações originais** de fantasia medieval, incluindo uma imagem de referência usada para manter o mundo visual consistente. As artes são compartilhadas por tema entre as cartas novas. O menu também usa a imagem de referência como fundo. As molduras e textos continuam sendo desenhados pelo Unity e permanecem editáveis.

## Experimentar

1. Pare e reinicie Play para carregar a versão e abrir o menu.
2. Vá a **Montar deck** e crie um deck sugerido. Os modelos novos priorizam Confluências; seus slots antigos não recebem cartas automaticamente.
3. Salve e selecione o slot para um jogador. Use **Treino** para experimentar rapidamente os custos e efeitos.
4. Para um terreno de mais de uma cor, abra **Domínios**, clique na casa e alterne **Próxima geração**. A mudança vale para o próximo início do próprio turno.
5. No Unity, abra **TCG → Oficina de cartas e efeitos**. Escolha a ilustração, as cores de geração e um **Modelo pronto** para inserir uma combinação reutilizável.

## Terrenos multicoloridos

Cada terreno oferece uma lista de cores possíveis. Na fase Principal, com sua prioridade e pilha vazia, você escolhe qual delas produzirá no próximo início do seu turno. Escolher não custa mana, não entra na pilha e **não converte a reserva existente**. A primeira cor é usada até que você faça outra escolha. Substituir um terreno remove a escolha anterior.

Os novos terrenos geram **1 mana de uma das cores**, e não 1 de cada cor. Nos terrenos personalizados, a geração extra também usa a cor escolhida. Terrenos antigos continuam funcionando com seu elemento original.

Há todas as **15 combinações de duas cores** entre Sol, Lua, Água, Fogo, Ar e Terra. Esses terrenos têm capacidade de habitação 1. Os dois terrenos de três cores não comportam construções — uma compensação experimental pela flexibilidade.

| Terreno | Cores disponíveis |
|---|---|
| Claustro do Eclipse | Sol / Lua |
| Estuário da Aurora | Sol / Água |
| Forja do Meio-dia | Sol / Fogo |
| Ponte dos Alísios | Sol / Ar |
| Bosque da Coroa | Sol / Terra |
| Lago das Duas Luas | Lua / Água |
| Caldeira do Crepúsculo | Lua / Fogo |
| Torre da Névoa | Lua / Ar |
| Raízes do Anoitecer | Lua / Terra |
| Fontes de Obsidiana | Água / Fogo |
| Arquipélago Suspenso | Água / Ar |
| Jardim das Marés | Água / Terra |
| Desfiladeiro do Trovão | Fogo / Ar |
| Vulcão das Raízes | Fogo / Terra |
| Planície dos Sussurros | Ar / Terra |
| Encontro da Aurora | Sol / Água / Terra |
| Fenda da Tempestade | Lua / Fogo / Ar |

As cartas e as casas do tabuleiro mostram faixas das cores. O filtro de elemento do Arsenal encontra terrenos por qualquer uma das cores disponíveis, independentemente da escolha de geração durante uma partida.

## Criaturas novas

Na tabela, a parte numérica isolada do custo é genérica. Todas as habilidades listadas são de entrada, exceto Armadura.

| Criatura | Custo | Ataque/vida | Habilidade |
|---|---|---|---|
| Guardiã dos Juramentos | 1 + Sol | 2/4 | Escudo próprio 1 permanente; ganha 1 popularidade |
| Vidente do Lago Lunar | 2 + Lua | 1/4 | Compra 1; oponente descarta 1 |
| Curadora das Fontes | 1 + Água | 1/4 | Limpa estados negativos dos aliados; cura 2 de cada |
| Dragão da Caldeira Antiga | 3 + Fogo + Fogo | 5/5 | Dano 1 a todas as unidades inimigas |
| Capitã dos Alísios | 2 + Ar | 2/3 | Recebe 1 ação e 1 movimento extras |
| Sentinela do Carvalho | 2 + Terra | 3/5 | Armadura 1; cura 2 do jogador |
| Cavaleira do Bosque Dourado | 1 + Sol + Terra | 3/4 | Cura 1 dos aliados; escudo próprio 2 permanente |
| Oráculo das Duas Margens | 1 + Lua + Água | 2/3 | Compra 1; recupera 1 carta recente do cemitério |
| Draco do Trovão Rubro | 2 + Fogo + Ar | 4/4 | Dano 2 direto ao jogador inimigo |
| Arauto do Eclipse Ardente | 1 + Lua + Fogo | 3/3 | Oponente descarta 1; ganha 1 popularidade |
| Sábia das Raízes Submersas | 1 + Água + Terra | 2/4 | Recupera 1 do cemitério; cura 2 do jogador |
| Porta-estandarte da Aurora | 1 + Sol + Ar | 2/3 | Aliados recebem +1 ataque e escudo 1 até o final |

## Magias novas

| Magia | Tipo e custo | Efeito |
|---|---|---|
| Chuva de Brasas | Feitiço; 2 + Fogo + Fogo | Dano 2 em todas as unidades inimigas |
| Fenda da Memória | Feitiço; 1 + Lua | Oponente descarta até 2 cartas |
| Memória das Raízes | Feitiço; 1 + Terra | Recupera até 2 cartas recentes do seu cemitério |
| Chamado dos Estandartes | Feitiço; 1 + Sol | Ganha 2 popularidade e compra 1 |
| Segundo Fôlego | Truque; Ar | Aliada não congelada recebe 1 ação e 1 movimento extras |
| Peso do Crepúsculo | Truque; Lua | Inimiga recebe -2 ataque até o final |
| Águas da Libertação | Truque; Água | Limpa estados negativos de uma aliada e cura 3 |
| Tributo do Eclipse | Feitiço; 1 + Lua + Fogo | Dano 3 no jogador inimigo; cura 3 do seu jogador |
| Muralha dos Juramentos | Truque; 1 + Sol + Água | Escudo 2 nos aliados até o final; compra 1 |
| Inverno sem Voz | Feitiço; 2 + Lua + Água | Congela todas as unidades inimigas |
| Juramento Materializado | Feitiço; 1 + Sol | Cria Escudeiro Solar 2/3 em casa sua vazia |
| Forma da Névoa | Feitiço; Lua + Água | Cria Aparição das Marés 1/2, movimento 3; compra 1 |

As duas fichas desaparecem ao sair do campo e não podem ser colocadas diretamente em decks.

## Efeitos prontos para reutilizar

O motor ganhou **seis operações** e **dois alvos**:

- **Descartar:** envia cartas do final da mão do jogador indicado para o cemitério. Não há escolha ou revelação interativa nesta versão; descarta até o número disponível.
- **Recuperar:** devolve as cartas mais recentes do próprio cemitério para a mão. Uma magia só vai ao cemitério depois de resolver sua sequência, portanto não recupera a si mesma.
- **Ganhar popularidade:** adiciona o valor configurado ao jogador.
- **Ação e movimento extras:** concede ambos a uma unidade não congelada; não remove congelamento.
- **Enfraquecer:** aplica redução de ataque até o final, com mínimo zero. A redução é armazenada separadamente dos bônus.
- **Limpar estados:** remove congelamento e redução de ataque, preservando bônus positivos. Não devolve ações ou movimento gastos; combine com ação extra quando desejar esse comportamento.
- **Todos os inimigos:** seleciona as unidades inimigas presentes no tabuleiro. Passageiros embarcados continuam fora desses alvos.
- **Jogador inimigo:** permite dano direto e descarte. Não exige clicar numa casa do tabuleiro.

Há **25 modelos prontos** na Oficina: dano, cura, compra, anulação, devolução, exílio, destruição, fortalecimento, escudo, congelamento, mana, ficha, descarte, recuperação, popularidade, ação extra, enfraquecimento, limpeza, dano em área, dano direto, dano direto + cura, dano + compra, fortalecimento do exército, limpeza + cura e entrada com escudo + compra.

Cada clique em **Adicionar combinação pronta** cria etapas independentes que podem ser ajustadas e reordenadas. O limite continua em oito efeitos por sequência. Modelos com alvos incompatíveis com o tipo escolhido são apontados pela validação antes de salvar; por exemplo, uma entrada de criatura deve usar a fonte, o jogador ou uma área em vez de exigir outra escolha de alvo.

## Artes

As seis imagens foram geradas pela ferramenta integrada de imagens e inspecionadas na conversa:

| Arquivo | Uso |
|---|---|
| `realm-reference.png` | Referência do mundo, fundo do menu e terreno tricolor |
| `guardian.png` | Guardiãs, cavaleiras e escudeiro |
| `moon-mage.png` | Magos, curadores e aparição |
| `dragon.png` | Criaturas ligadas a fogo e tempestade |
| `arcane-clash.png` | Magias desta coleção |
| `confluence.png` | Terrenos de duas cores e Fenda da Tempestade |

Destino: `Assets/TCG/Resources/TCGArt`. São seis pinturas compartilhadas, não uma pintura exclusiva para cada uma das 43 cartas. As cartas anteriores mantêm sua arte procedural quando não possuem uma ilustração selecionada.

A Oficina permite escolher essas imagens; o identificador é salvo na carta e preservado na exportação do deck. Os PNGs já acompanham a versão 0.6 do projeto. A moldura, os custos e as regras são elementos separados da imagem para manter a legibilidade. `CardArtImport` preserva proporções, desliga mipmaps e aplica importação adequada às texturas da interface. Os prompts completos estão em `ART-PROMPTS.md`, ao lado deste README.

## Implementação e testes

- `Confluence.cs`: 43 definições, novas operações, escolha de mana e 25 modelos.
- `Catalog.cs`: cores de produção, identificador de arte e novos alvos/operações.
- `Game.cs`: geração por escolha, alvos em área e redução separada de ataque.
- `GeneratedArt.cs` e `BoardArt.cs`: carregamento e reutilização das texturas.
- `CardPresentation.cs`, `DomainView.cs`, `GameView.cs` e `MainMenuView.cs`: artes, faixas de cores e escolha da geração.
- `CardWorkshop.cs`: seletor de arte, cores e modelos de efeitos.
- `CardAuthoring.cs`: validação compartilhada entre catálogo e editor.
- `ConfluenceChecks.cs`: testes da expansão, serialização e recursos de imagem.

**290 verificações aprovadas** no Unity 6000.3.6f1 em cópia isolada do projeto: 231 anteriores e 59 desta expansão. Incluem todas as definições novas, 25 modelos, geração de apenas uma cor, troca sem conversão da reserva, efeitos em área, congelamento/limpeza, bônus positivos preservados, recuperação do cemitério e carregamento das seis texturas.

As pinturas foram inspecionadas e o carregamento das imagens no Unity foi testado. A composição final da interface em Play ainda não foi validada por captura de tela automatizada. Custos e atributos são experimentais e precisam de balanceamento em partidas. Slots salvos, cenas, configurações e notas originais do Obsidian foram preservados.

## Próximas implementações possíveis

1. Artes exclusivas para as cartas mais importantes.
2. Efeitos de veneno, queimadura e dano periódico.
3. Escolha interativa de cartas ao descartar e recuperar.
4. Busca de cartas no deck.
5. Habilidades ao morrer e ao causar dano.
6. Decks temáticos de duas e três cores.
7. Animações próprias por escola de magia.
8. Balanceamento por testes contra IA.


---

# Histórico das versões anteriores

A atualização 0.6 acima prevalece nas regras alteradas.

# TCG — atualização experimental 0.5

Os seis itens desta etapa foram integrados ao projeto Unity `C:\Users\gil\TCG`. O catálogo original passa de 210 para **214 definições**, com dois veículos e dois terrenos automáticos adicionais. Os IDs anteriores e os slots salvos foram preservados.

## Menu inicial, modos e slots

Ao iniciar Play, o jogo abre no menu de fantasia medieval com **Jogar** e **Montar deck**. Quando existe uma partida iniciada nesta execução, aparece **Continuar partida em andamento**. O botão **Menu** no topo da partida permite voltar sem apagar o estado atual.

**Jogar** abre a seleção do modo e dos decks dos dois jogadores. Clique no seletor de cada jogador para alternar entre os slots completos. Listas incompletas continuam salvas no Arsenal, mas não aparecem como opção para iniciar. A tela avisa que iniciar outra partida substitui a atual.

| Modo | Regras efetivas |
|---|---|
| Duelo local | Dois jogadores no mesmo computador; 50 de vida; mana normal; tela de troca de prioridade para preservar a mão |
| Partida rápida | 30 de vida; +1 mana incolor em cada início do próprio turno; os mesmos decks completos e regras de combate |
| Treino | Controle manual dos dois lados; 50 de vida; +10 mana de cada elemento por início de turno; sem a tela de troca de jogador |

Nenhum modo inclui IA ou jogo online nesta versão. Treino mantém a pilha e a troca de prioridade, apenas elimina a tela de privacidade e oferece mais recursos. A cura máxima acompanha a vida inicial do modo.

**Montar deck** abre o Arsenal. Cada lista tem um identificador independente e aparece como um slot. É possível criar, duplicar, renomear, salvar e excluir várias listas, sem limite fixo de slots nesta versão. **Salvar lista** persiste o slot; sair da tela mantém rascunhos apenas enquanto o jogo estiver aberto. A biblioteca continua no arquivo `decks/library-v1.json` da pasta de dados do jogo. Os decks equipados são cópias das versões salvas.

## Transferir e desequipar

Na aba **Criaturas**, selecione uma criatura equipada. O painel mostra os equipamentos e dois comandos:

- **Desequipar · 1 mana:** devolve o equipamento à mão.
- **Transferir · 1 mana:** escolha uma criatura aliada adjacente marcada ALVO, com espaço disponível.

Ambas as ações exigem sua fase Principal e pilha vazia. O custo é genérico e pago ao anunciar; a ação entra na pilha e pode ser anulada. Se fonte, alvo, adjacência ou espaço não forem mais válidos na resolução, a mudança não ocorre e o custo não volta.

Continuam existindo dois espaços por criatura/comandante. Ataque e armadura acompanham a troca. Retirar botas limita o movimento restante ao novo máximo; equipar ou transferir botas não concede movimento imediato. Veículos não recebem equipamentos de criatura nesta versão. Ao sair do campo, uma criatura continua enviando seus equipamentos anexados ao cemitério.

## Vida e reparo de construções

Construções têm **5 de vida por padrão**, configuráveis na Oficina. Agora um ataque causa o ataque da unidade como dano à primeira construção da casa, em vez de demoli-la automaticamente. Ela só é destruída ao chegar a zero. Havendo criatura ou veículo defendendo a casa, o ataque é dirigido a essa unidade.

Na aba **Domínios**, cada construção mostra vida atual/máxima e o comando **Reparar 2 de vida · 1 mana**. O reparo:

- Exige construção sua ferida e uma criatura sua habitando a casa.
- Custa 1 mana genérica e pode ser usado uma vez por construção por turno.
- Entra na pilha; o habitante escolhido deve continuar ali na resolução.
- Recupera até 2, sem superar a vida máxima; anulação não devolve o custo nem o uso.

Um veículo, mesmo transportando criaturas, não conta como habitante para ativar construções ou reparar: desembarque uma criatura. Construções autônomas continuam podendo ativar seus próprios efeitos sem habitante, mas reparos exigem um.

A conquista de um terreno inimigo desocupado continua demolindo as construções daquele dono. A escolha individual entre várias construções, reparos por feitiços e resistências de construção ainda não fazem parte desta etapa.

## Habilidades automáticas de terrenos

Terrenos podem ter duas listas distintas: efeitos **ativados**, já existentes, e efeitos **automáticos**, novos. Na Oficina, selecione um gatilho e marque **Editar efeitos automáticos** para editar a segunda lista.

- **Início do seu turno:** dispara depois da renovação de mana e das unidades.
- **Entrada de unidade sua:** dispara quando uma criatura ou veículo entra por invocação, criação de ficha, movimento, desembarque ou evacuação.

Esses efeitos não exigem botão nem mana de ativação. Entram na pilha, aceitam truques como resposta e precisam ser resolvidos antes de avançar a fase. A fase Início agora permite passar prioridade quando há habilidades pendentes. Gatilhos simultâneos de início são organizados pela ordem das casas; os de menor índice resolvem primeiro.

A fonte deve continuar sendo o mesmo terreno sob controle do jogador. Efeitos no ocupante não atingem uma unidade que o substituiu. Entrar novamente pode disparar novamente; esta versão não impõe limite adicional por turno. Efeitos automáticos em uma entrada resolvem depois da habilidade de entrada da criatura, quando ambas existem.

Exemplos:

| Carta | Efeito |
|---|---|
| Santuário da Primeira Luz | Gera 1 Sol; no início do seu turno, cura 2 do jogador pela pilha |
| Portão dos Ventos | Gera 1 Ar; unidade sua que entra recebe escudo 1 até o final do turno, pela pilha |

## Veículos e tripulação

A nota `Sub-Tipos/Veiculo.md` define veículos como transporte de múltiplas criaturas com quantidades próprias de assentos. A implementação usa uma unidade de veículo no tabuleiro e uma lista de passageiros, preservando vida, equipamentos e estados de cada criatura embarcada.

Para embarcar, selecione uma criatura na aba **Criaturas** e clique num veículo aliado adjacente marcado **EMBARCAR**. O embarque exige um assento livre, consome uma ação da criatura e zera seu movimento. Uma criatura não pode carregar outro veículo.

Selecione o veículo para consultar assentos e tripulação e para mover/atacar normalmente. Passageiros congelados não contam para a tripulação mínima. Sem tripulação suficiente, o veículo não move nem ataca. Passageiros não agem e não são alvos de efeitos do tabuleiro enquanto estão embarcados; seus estados temporários ainda expiram normalmente.

Para desembarcar, escolha um passageiro no painel e clique numa casa adjacente vazia, sua ou neutra. Ele desembarca com zero ações e movimento até a próxima renovação. Embarque e desembarque são ações imediatas da fase Principal, com pilha vazia, assim como o movimento.

Quando um veículo sai por morte, devolução ou exílio, os passageiros tentam evacuar para a própria casa e depois casas adjacentes seguras. Cada um ocupa uma casa livre, sua ou neutra. Quem não consegue sair é destruído; comandantes retornam à zona de comando com a taxa de morte. Passageiros evacuados ficam sem ações/movimento nesse turno. Essa consequência é uma regra provisória do protótipo.

| Veículo | Custo | Atributos | Transporte |
|---|---|---|---|
| Carruagem da Fronteira | 2 genéricas + 1 Terra | Ataque 1, vida 6, movimento 3 | 3 assentos; exige 1 tripulante |
| Carro de Cerco Solar | 3 genéricas + 1 Sol | Ataque 4, vida 8, movimento 1, alcance 2 | 2 assentos; exige 2 tripulantes |

Na Oficina, o novo tipo **Veículo** oferece atributos, assentos, tripulação mínima e efeitos de entrada. **Construção** ganhou vida máxima, e **Terreno** ganhou o gatilho/lista automática. Os arquivos de cartas da versão 0.4 continuam válidos e recebem os valores padrão dos campos novos.

## Exportar um deck completo com suas cartas

No Arsenal, **Exportar pacote**:

1. Inclui a lista de deck e as definições de todas as cartas personalizadas referenciadas.
2. Inclui também fichas personalizadas usadas pelos efeitos dessas cartas.
3. Salva um arquivo JSON em `decks/exports` dentro da pasta de dados do jogo.
4. Copia o pacote para a área de transferência e informa o caminho no rodapé.

**Importar JSON** aceita pacotes novos e listas antigas. É possível colar o conteúdo ou digitar o caminho de um arquivo no campo abaixo da área de texto e clicar em **Ler arquivo**. Depois, importe como novo deck e use **Salvar lista** para guardar o slot.

A importação valida o pacote inteiro antes de alterar o catálogo ou gravar a biblioteca. Definições com o mesmo ID e conteúdo são reutilizadas. Se o conteúdo for diferente, o importador cria um novo ID e reescreve as referências do deck e das fichas, preservando a carta local. Arquivos inválidos são rejeitados sem instalação parcial.

As cartas importadas ficam em `custom-cards/installed-v1.json`, dentro da pasta de dados do jogo, e são carregadas antes dos decks. Alterar uma carta importada na Oficina atualiza essa biblioteca local, para que a edição sobreviva à reabertura. Novas cartas criadas no projeto continuam em `Assets/TCG/Resources/TCGCards`. Um pacote é a forma de compartilhar cartas instaladas apenas na biblioteca local; elas não são automaticamente incorporadas como assets da build.

## Implementação e verificação

- `Journey.cs`: transferência, desequipar, reparo, embarque/desembarque, evacuação e gatilhos.
- `Game.cs` e `Kingdom.cs`: integração com pilha, combate, renovação, estados e regras dos modos.
- `MainMenuView.cs`: menu inicial, seleção dos modos e slots.
- `UnitActionsView.cs` e `DomainView.cs`: comandos de unidades, equipamentos, passageiros e construções.
- `DeckPackages.cs`: pacotes portáteis, dependências, resolução de conflitos e persistência.
- `CardWorkshop.cs`, `Catalog.cs` e `CardAuthoring.cs`: campos novos e edição/validação das cartas.
- `JourneyChecks.cs`: testes dos seis itens; integra a opção **TCG → Validar regras**.

**231 verificações aprovadas** no Unity 6000.3.6f1 em uma cópia isolada: 54 regras anteriores, 15 controles de interface, 47 efeitos/biblioteca, 54 sistemas da versão 0.4 e 61 desta etapa. Os testes abrangem ações anuladas, alvos perdidos, reparos, disparos automáticos, passageiros, evacuação, conflitos de IDs, gravação, compatibilidade de cartas antigas e regras efetivas dos modos.

Os testes de interface verificam o controlador e a navegação. A renderização visual do menu e dos painéis ainda precisa de conferência numa sessão interativa; não foi validada por captura de tela automatizada. Os custos, capacidades, modos e regras de evacuação permanecem experimentais. Cenas, pacotes/configurações do projeto e notas originais do Obsidian foram preservados.

## Próximas implementações possíveis

1. Adversário por IA.
2. Salvar e retomar partidas após fechar o jogo.
3. Tutorial guiado dos modos e da pilha.
4. Selecionar individualmente construções como alvo.
5. Habilidades próprias de veículos e passageiros.
6. Animações de equipamentos, reparo e embarque.
7. Balanceamento dos decks sugeridos e dos modos.


---

# Histórico — documentação das versões anteriores

As seções abaixo descrevem versões anteriores; a atualização 0.5 acima prevalece nas regras alteradas.

# TCG — atualização experimental 0.4

Implementados os cinco sistemas solicitados no projeto Unity `C:\Users\gil\TCG`.

## Como usar

1. Aguarde a recompilação do Unity. Se estiver em Play, pare e inicie novamente.
2. Em **Arsenal**, crie um deck sugerido para incluir os novos exemplos. Listas antigas salvas são preservadas; também é possível adicionar as cartas manualmente, mantendo 100 principais, 50 terrenos e um comandante.
3. Salve e equipe a lista no Sol ou Lua. Inicie outra partida para usar essa seleção.
4. Na **Mão**, selecione um equipamento e clique numa criatura sua marcada ALVO. Para uma construção, clique num terreno seu com capacidade disponível. Ambos entram na pilha e podem ser anulados.
5. Abra **Domínios** e clique numa casa para consultar habitação e ativar habilidades de terrenos/construções. Ativações exigem sua fase Principal, pilha vazia e mana disponível; resolvem depois dos passes de prioridade.
6. No menu superior do **Unity**, abra **TCG → Oficina de cartas e efeitos**. Esse editor funciona dentro do Unity; não é uma tela da versão executável do jogo.

## Equipamentos

Uma criatura ou comandante pode carregar até dois equipamentos. Ataque e armadura passam a valer na resolução; movimento adicional é recebido na renovação dos próximos turnos. Equipamentos acompanham o movimento e não ocupam habitação. Quando a criatura sai do campo por morte, devolução ou exílio, seus equipamentos vão ao cemitério do controlador. Se o alvo desaparece ou não tem mais espaço ao resolver, o equipamento vai ao cemitério.

Os bônus ficam em `Unit.Equipment`; ataque, armadura e movimento máximo são calculados a partir das cartas anexadas. A redução de armadura ocorre antes dos escudos e da vida. Não há transferência, desequipar ou alvo direto de magia sobre equipamento nesta versão.

## Construções

Ficam numa lista separada em cada casa e não ocupam o espaço da criatura. Exigem terreno próprio e capacidade suficiente. Terrenos antigos têm capacidade padrão 2; as novas cartas possuem valores próprios. Substituir um terreno por outro com capacidade inferior à já utilizada é recusado sem remover cartas ou pagar custos.

Seguindo as notas `Sub-Tipos/Construção.md` e `Tipo de cartas/Permanente/Terreno.md`, construções normalmente exigem uma criatura sua no terreno. A opção **Funciona sem habitante** permite criar uma construção autônoma; efeitos no ocupante ainda precisam dele. Cada construção pode ativar sua habilidade uma vez por turno. A presença da fonte, seu controle e a condição de habitação são verificados novamente na resolução.

Regra provisória: um ataque a uma construção sem criatura defensora demole uma construção, pela pilha, consumindo uma ação e respeitando alcance. Não há pontos de vida de construção ainda. Se houver várias, o ataque escolhe a primeira da lista. Uma criatura defensora recebe o ataque em vez da construção. Conquistar um terreno desocupado pelo inimigo demole suas construções e envia as cartas ao cemitério do dono.

## Terrenos

Cada terreno gera 1 mana de seu elemento mais seu valor de **Mana extra no início**. A capital continua gerando 1 incolor. A reserva é renovada no início do próprio turno, como antes.

Terrenos podem ter uma sequência de efeitos ativada uma vez por turno, com custo genérico configurável. **Fonte / ocupante** significa a criatura escolhida naquela casa na ativação: se sair, esse efeito não atinge uma substituta. Os escudos temporários de terrenos expiram no final do turno. As habilidades podem ser anuladas na pilha, sem devolver o custo nem o uso da ativação.

## Mana por elemento

`CardDefinition.Cost` representa a parte genérica, e `ColoredCost` guarda seis exigências: Sol, Lua, Água, Fogo, Ar e Terra. `TotalCost` soma as duas partes; filtros e curva de mana usam esse total. A carta ampliada e o painel da carta selecionada mostram os elementos exigidos.

O jogo verifica o pagamento completo antes de modificar a reserva ou a mão. Reserva primeiro os valores dos elementos exigidos; paga a parte genérica usando incolor e, se necessário, sobras dos outros elementos. Exemplo: **1 genérica + 1 Sol** exige pelo menos 1 Sol; duas manas incolores não bastam. A taxa de morte de comandante continua sendo genérica.

Os custos das 202 definições anteriores foram preservados como genéricos. Os cinco novos equipamentos/construções demonstram custos elementais; a Oficina permite configurá-los em novas criaturas, magias e comandantes. Não há restrição de identidade de cor do comandante.

## Oito cartas de exemplo

| Carta | Tipo e custo | Regra |
|---|---|---|
| Lâmina da Forja Solar | Equipamento; 1 genérica + 1 Sol | +2 ataque |
| Cota das Montanhas | Equipamento; 1 genérica + 1 Terra | +1 armadura |
| Botas do Peregrino | Equipamento; 1 Ar | +1 movimento nos próximos turnos |
| Enfermaria da Abadia | Construção; 1 genérica + 1 Água | Ocupa 1; habitada, pague 1 para curar 3 do ocupante |
| Observatório Lunar | Construção; 1 genérica + 1 Lua | Ocupa 2; habitado, pague 2 para comprar 1 |
| Poço da Aurora | Terreno; capacidade 2 | Gera 2 Sol no início |
| Bosque dos Juramentos | Terreno; capacidade 1 | Gera 1 Terra; pague 1 para dar escudo 2 ao ocupante até o final |
| Fonte das Marés | Terreno; capacidade 3 | Gera 1 Água; pague 1 para curar 2 do jogador |

Catálogo original agora: **210 definições**. Esse total inclui variantes anteriores com mecânicas repetidas. Cartas personalizadas são adicionais.

## Oficina visual

Há busca de modelos, edição de nome/descrição/tipo/elemento, custos genéricos e coloridos, atributos de criatura, bônus de equipamento, habitação, autonomia e ativação. A prévia usa moldura dourada e área de pergaminho, seguindo a direção de fantasia medieval.

Adicione até oito efeitos, escolha operação, alvo, quantidade, duração, elemento da mana ou ficha e reordene com a seta. A validação impede combinações sem implementação, como bônus de ataque permanente, efeitos com diferentes escolhas de alvo, efeitos extras em equipamentos e habilidades de entrada em fichas. Terrenos e construções usam jogador, ocupante ou todos os aliados; não fazem uma segunda escolha de alvo no campo.

**Salvar carta no catálogo** cria uma definição com ID próprio ao partir de um modelo original. Ao editar uma personalizada, preserva seu ID. **Salvar como nova cópia** cria outro ID. Mudar o tipo de uma personalizada salva exige uma cópia para preservar a validade dos decks. A descrição é texto autoral; os campos de atributos e efeitos determinam o funcionamento real, portanto mantenha o texto coerente com eles.

Cada carta é gravada em `Assets/TCG/Resources/TCGCards/custom-<id>.json`, com versão, validação e substituição de arquivo temporário. O carregamento ocorre antes da biblioteca de decks; os recursos também são incluídos na build. As definições novas substituem a consulta do catálogo sem alterar as referências das cartas em partidas já iniciadas. Para distribuir um deck que usa cartas personalizadas, envie também seus arquivos de carta: o JSON de deck contém IDs, não incorpora as definições.

## Arquivos e validação

- `Scripts/Catalog.cs`: tipos, custos, dados de equipamento, capacidade e registro de cartas.
- `Scripts/Kingdom.cs`: pagamento elemental, equipamentos, construções, habilidades e oito exemplos.
- `Scripts/Game.cs`: integração com ações, resolução, combate, movimento e zonas.
- `Scripts/DomainView.cs`: inspeção e ativação de permanentes no tabuleiro.
- `Scripts/DeckEditorView.cs` e `CardPresentation.cs`: filtros de artefatos, custo total e apresentação.
- `Scripts/CardAuthoring.cs`: validação, cópia, arquivos de cartas e carregamento.
- `Editor/CardWorkshop.cs`: editor visual do Unity.
- `Editor/KingdomChecks.cs`: testes de regras novas e persistência de cartas.

Validação: **170 verificações** no Unity 6000.3.6f1, em cópia isolada do projeto: 54 regras anteriores, 15 controles de interface, 47 efeitos/biblioteca e 54 novos sistemas. Inclui pagamento recusado sem perdas, equipamento anulado/alvo perdido, limite de espaços, habitação, retirada do habitante em resposta, demolição, conquista, geração/ativação de terreno, arquivo inválido, gravação e preservação de instâncias antigas.

Os testes de interface verificam o controlador, não a renderização em pixels. A aparência final da Oficina e da aba Domínios ainda precisa ser observada em uma sessão interativa de Play; não foi possível obter captura visual útil pelo ambiente automatizado.

As regras de dois equipamentos, custos/valores, demolição, capacidades e ativações são decisões provisórias para tornar o protótipo jogável. As notas originais do Obsidian foram preservadas.

## Próximas implementações possíveis

1. Transferir e desequipar equipamentos.
2. Vida, reparo e efeitos que escolham construções como alvo.
3. Terrenos com habilidades automáticas e efeitos de movimento.
4. Veículos e tripulação.
5. Encantamentos contínuos.
6. Exportar decks junto das cartas personalizadas.
7. Salvamento e retomada de partidas.
8. Adversário por IA.


---

# Histórico — documentação da versão 0.3

As seções abaixo descrevem a versão anterior; a atualização 0.4 acima prevalece nas regras alteradas.

# TCG — versão experimental 0.3

## Atualização 0.3 — cartas, efeitos e Arsenal

Esta atualização acrescenta **15 cartas novas com designs próprios** e **1 definição de ficha**, mantendo todos os IDs antigos e a compatibilidade das listas salvas. O catálogo tem 202 definições: 99 criaturas, 44 magias, 56 terrenos, 2 comandantes e 1 ficha. As 186 definições anteriores continuam incluindo variantes com as mesmas mecânicas; a quantidade total não significa 202 mecânicas diferentes.

### Efeitos por composição

`EffectStep` descreve uma operação, alvo, valor e duração. `CardDefinition.Effects` permite combinar várias operações em uma única carta. Criaturas executam sua sequência numa habilidade ao entrar; magias executam ao resolver. A sequência é resolvida em ordem, sem uma nova janela de prioridade entre suas etapas. As cartas antigas usam uma adaptação do formato anterior.

Novas operações: devolver à mão, exilar, destruir, aumentar ataque temporariamente, dar escudo, congelar, gerar mana e criar ficha. As operações anteriores de dano, cura, compra e anulação continuam disponíveis. Há alvos de unidade inimiga, aliada, qualquer unidade, própria fonte, todas as aliadas, jogador controlador e casa própria vazia.

| Carta nova | Custo | Tipo | Efeito |
|---|---:|---|---|
| Fagulha Sagaz | 2 | Feitiço | 2 de dano a inimigo, depois compra 1 |
| Retorno das Marés | 2 | Truque | Devolve inimigo à mão do dono |
| Eclipse do Destino | 4 | Feitiço | Exila inimigo |
| Ruptura das Raízes | 3 | Feitiço | Destrói inimigo, ignorando armadura/escudo |
| Brado da Aurora | 1 | Truque | +2 ataque para um aliado até o fim do turno |
| Escudo Solar | 1 | Truque | 3 de escudo para um aliado até o fim do turno |
| Prisão de Geada | 2 | Truque | Congela inimigo até o fim do próximo turno dele |
| Reserva dos Ventos | 1 | Feitiço | Gera 3 mana incolor após pagar o custo |
| Vigia de Argila | 1 | Feitiço | Cria uma ficha 1/2 com 1 movimento em casa própria vazia |
| Clarão do Refúgio | 3 | Feitiço | Cura 2 de cada aliado, depois compra 1 |
| Chama Voraz | 3 | Feitiço | 4 de dano a inimigo, depois cura 2 do jogador |
| Estilhaço Instantâneo | 1 | Truque | 1 de dano a inimigo; pode responder na pilha |
| Sentinela da Aurora | 2 | Criatura 1/4 | Ao entrar: recebe 2 de escudo permanente |
| Semeadora da Vida | 3 | Criatura 2/4 | Ao entrar: cura 3 do jogador e compra 1 |
| Cartógrafo dos Ventos | 2 | Criatura 1/3 | Ao entrar: gera 1 mana incolor; tem 3 movimentos |

Regras provisórias desta expansão:

- Se o alvo escolhido de uma magia não for mais válido no começo da resolução, a sequência inteira é cancelada. Se a própria primeira etapa matar o alvo, as etapas seguintes ainda acontecem.
- Dano passa por armadura, depois escudo temporário, depois escudo permanente e finalmente vida. Escudo é consumido; escudo permanente não expira, mas pode acabar. Destruição e exílio não são dano.
- Brado e escudos temporários acabam no Final do turno corrente. Congelamento zera movimento/ações imediatamente e impede a renovação no próximo turno do controlador; expira ao final desse turno. Não cancela um ataque que já estava na pilha.
- Devolver um comandante o coloca na zona de comando sem imposto de morte. Destruir continua aumentando seu custo em 2. **Exilar comandante o torna indisponível pelo restante desta versão da partida**; não há carta de recuperação de exílio ainda.
- Fichas desaparecem ao deixar o campo, inclusive por devolução ou exílio; não entram na mão ou em decks.
- Custos continuam genéricos e não há restrição de identidade de cor. Todas estas cartas e custos são experimentais e precisam de balanceamento.

### Arsenal e biblioteca de decks

Abra **Arsenal** no topo da partida. Agora há várias listas salvas, em vez de apenas dois arquivos editáveis:

1. Crie um deck vazio, duplique uma lista existente ou crie um dos modelos sugeridos (equilibrado, agressivo, controle).
2. Edite o nome, inclua/remova cartas e escolha um comandante. Use busca, filtros de tipo, elemento, custo e nova expansão.
3. Consulte a carta ampliada, a lista atual, a curva de mana e a distribuição entre criaturas, feitiços e truques.
4. Use **Salvar lista**. Listas incompletas podem ser salvas como rascunho; listas com cartas desconhecidas, repetidas ou de tipo inválido não podem.
5. Com a lista completa e salva, use **Equipar no Sol** ou **Equipar na Lua**.
6. **Nova partida** usa as versões equipadas. Editar ou excluir uma lista não modifica a cópia já equipada nem uma partida em andamento.

O asterisco marca mudanças não salvas. Alternar entre listas ou voltar à partida mantém os rascunhos na memória; sair do Play/aplicativo ou recarregar os scripts perde alterações não salvas. Exclusão pede confirmação dentro da interface. Copiar JSON exporta para a área de transferência; Importar JSON cria um novo rascunho e valida os dados antes de aceitá-los.

Persistência: `Application.persistentDataPath/decks/library-v1.json`. Os arquivos antigos `player-1.json` e `player-2.json` são lidos quando não há biblioteca nova e permanecem intactos. A biblioteca passa a ser gravada quando o usuário salva/equipa uma lista. Gravação usa arquivo temporário e substituição; falha de gravação não descarta o rascunho. Uma biblioteca inválida é preservada no carregamento e a interface informa o uso de listas padrão.

Os modelos novos e os decks padrão recém-criados incluem as cartas novas. Os decks antigos mantêm suas escolhas; para experimentar a expansão, crie um modelo sugerido ou adicione as cartas manualmente.

### Aparência medieval

Molduras com cantos ornamentados, bordas metálicas, áreas de regras em pergaminho, fundos de paisagem com fortaleza e símbolos dos elementos. As imagens são procedurais e originais, compartilhadas entre cartas por família visual. Não são ilustrações exclusivas de cada carta. A mão usa miniaturas; **Ampliar carta** mostra o texto completo. Unidades exibem escudo e congelamento no campo e no painel de atributos.

### Código da atualização

Validação 0.3: 113 verificações (54 da base, 12 da interface e 47 da expansão/biblioteca). Incluem operações reais de arquivo em pasta temporária, migração, importação/exportação, preservação da versão equipada e falhas de validação. A validação de lógica não substitui a conferência visual da interface no Play; a captura automática em execução oculta não está disponível nesta sessão.

- `Expansion.cs`: as 15 novas cartas, ficha e modelos sugeridos.
- `Catalog.cs`: passos de efeito, validação de metadados e validação completa/rascunho de decks.
- `Game.cs`: resolução das sequências, zonas, escudos, congelamento e fichas.
- `DeckLibrary.cs`: biblioteca, versões equipadas, importação/exportação e persistência com migração.
- `DeckEditorView.cs`: Arsenal, galeria, filtros, curva e gerenciamento de listas.
- `CardPresentation.cs` e `BoardArt.cs`: molduras, pergaminho, paisagens e símbolos.
- `ExpansionChecks.cs`: testes dos novos efeitos e operações reais de arquivo numa pasta temporária exclusiva.

Os dados desta versão são definidos em C#; ainda não há editor visual de criação de cartas ou suporte genérico a qualquer combinação imaginável de efeitos. `Catalog.ValidateEffectDefinitions()` identifica configurações de alvo/duração não suportadas. A seleção de alvos adicionais em habilidades ao entrar ainda não está implementada; as habilidades ao entrar desta coleção usam alvos automáticos.

As seções abaixo registram a base 0.2. Quando houver conflito, as regras 0.3 acima prevalecem, especialmente persistência e exílio de comandantes.

Projeto Unity 6000.3.6f1 em `C:\Users\gil\TCG`. Abra `Assets/TCG/Scenes/Partida.unity` e pressione Play. A interface foi dimensionada para 1440×960 e se ajusta proporcionalmente à aba Game.

## Os sete itens implementados

1. **Cartas e habilidades:** catálogo de 186 definições com IDs únicos: 96 criaturas (8 arquétipos em 12 variantes de nome), 32 magias (4 efeitos em 8 variantes), 56 terrenos e 2 comandantes. As variantes do mesmo arquétipo compartilham atributos e habilidade; não são 186 designs independentes. O conteúdo serve a testes, não a uma coleção balanceada.
2. **Comandante jogável:** Aurel (3 ataque, 6 vida, alcance 1) ou Selene (2 ataque, 7 vida, alcance 2). Custo inicial 3, zona de comando, movimento e ataque normais. Popularidade e duas habilidades ativadas usam a pilha.
3. **Pilha e prioridade:** cartas, ataques, habilidades de comando e habilidades ao entrar resolvem em LIFO. Truques podem responder. Dois passes consecutivos resolvem só o topo; uma nova ação reinicia a contagem. Alvos são revalidados na resolução.
4. **Fases:** Início → Compra → Terreno → Principal → Final. Cada fase é visível e tem um botão de avanço; o Final precisa de dois passes com pilha vazia para encerrar.
5. **Aparência:** símbolos originais procedurais para sete elementos, silhuetas de unidades (arqueiro, guardião, conjurador, comandante etc.), cores de dono, miniaturas nas cartas, custo e atributos. Não há arte ilustrada externa.
6. **Animações:** interpolação de movimento, crescimento na invocação, traço de ataque, brilho de terreno e números de dano/cura. Os eventos visuais são emitidos pelo motor; a interface os exibe em sequência de 0,45 s. As ações ficam indisponíveis durante a animação.
7. **Editor e validador:** decks separados de Sol e Lua; busca por nome/tipo/texto, filtros por zona, lista das cartas incluídas, adição/remoção, escolha de comandante, nome, restauração do padrão e salvar/carregar JSON. Os decks são aplicados ao iniciar uma nova partida, não à partida em andamento.

## Como jogar

1. Clique em **Comprar 2 cartas**, depois **Ir para Terrenos**.
2. Na aba Terrenos, selecione uma carta e uma casa COLOCAR. Avance à Principal.
3. Use a aba Mão: selecione uma carta e clique no alvo do campo. Estudo não precisa de alvo; use seu botão de lançamento. Para Negação, selecione uma ação da pilha e clique em Anular.
4. A ação está agora na pilha. O jogador com prioridade pode lançar um truque ou passar. Troque de jogador pela tela de passagem. Após dois passes, o topo resolve.
5. Use Criaturas para selecionar, mover ou atacar. Use Comandante para invocar na sua área ou ativar uma habilidade se ele estiver no campo.
6. Vá ao Final e passe prioridade entre ambos os jogadores para encerrar. A pilha precisa estar vazia. O início seguinte renova os recursos.

As mãos acompanham o jogador com prioridade, que pode ser diferente do jogador ativo. A tela de troca oculta a mão até clicar em continuar. Este é um modo de confiança entre dois jogadores no mesmo computador; não é segurança multiplayer.

## Cartas disponíveis

| Arquétipo | Custo | Ataque / vida | Movimento / alcance | Habilidade |
|---|---:|---|---|---|
| Batedor | 1 | 2 / 3 | 2 / 1 | Sem habilidade adicional |
| Explorador | 1 | 1 / 2 | 3 / 1 | Mais movimento |
| Guardião | 2 | 2 / 5 | 2 / 1 | Armadura reduz cada dano recebido em 1 |
| Arqueiro | 2 | 2 / 2 | 2 / 2 | Ataque à distância |
| Curandeira | 2 | 1 / 3 | 2 / 1 | Ao entrar: recupera 2 de vida do jogador |
| Sábio | 3 | 1 / 3 | 2 / 1 | Ao entrar: compra 1 carta |
| Colosso | 3 | 4 / 5 | 1 / 1 | Alto ataque, movimento reduzido |
| Ceifador | 3 | 3 / 3 | 2 / 1 | Jogador recupera o dano efetivamente causado por seus ataques |

- **Chama:** feitiço, custo 2, causa 3 de dano a uma criatura inimiga.
- **Amparo:** truque, custo 1, recupera até 3 de vida de uma criatura sua ferida.
- **Estudo:** feitiço, custo 2, compra 2 cartas.
- **Negação:** truque, custo 1, anula uma ação escolhida na pilha, inclusive ataque ou habilidade.

## Comando

Cada jogador começa com 2 popularidade. Uma ativação por turno do controlador; exige comandante no campo, fase Principal, pilha vazia e 1 ação disponível. Compartilha a ação com o ataque.

- **Reunir:** na resolução, ganha 1 popularidade e recupera 2 de vida do jogador.
- **Inspirar:** paga 2 popularidade ao anunciar; na resolução, unidades aliadas atualmente no campo ganham +1 ataque até o fim do turno. Unidades que entrarem depois não recebem o bônus.

Ao morrer, o comandante retorna à zona de comando e seu custo aumenta em 2. Se a invocação for anulada, retorna sem aumento. Custos pagos não são devolvidos. A habilidade anunciada continua existindo se o comandante morrer antes da resolução.

## Regras provisórias explícitas

O Obsidian continua sendo a referência de design. As notas em `01 - Regras`, `Tipo de cartas`, `04 - Sistemas` e `08 - Tecnico` ainda deixam várias decisões abertas. As escolhas abaixo são da versão experimental:

- Dois jogadores, uma capital por jogador, Sol começa. Mão inicial 5; ambos compram 2 no primeiro turno. Sem mulligan.
- Deck principal exatamente 100, terrenos exatamente 50, um comandante; singleton por ID, incluindo terrenos. Identidade de cor, banimentos e rituais ainda não restringem decks.
- Os custos atuais são **genéricos**. A cor do elemento identifica a carta e a mana, mas qualquer cor pode pagar. Consumo: incolor, terra, ar, fogo, água, lua, sol.
- Cada território, incluindo capital, gera 1 mana no início. Mana não zera ao passar o turno: só no próximo início do próprio jogador; a reserva permite respostas fora do turno.
- Um terreno obrigatório na fase Terreno, exceto sem oferta ou sem posição válida. Reposição imediata, substituição própria, sem substituir capitais. Terreno substituído vai ao cemitério de terrenos. Ocupantes permanecem.
- Movimento e terrenos são ações imediatas. Movimento exige Principal, prioridade do jogador ativo e pilha vazia. Uma criatura por casa. Casas sem terreno são transitáveis; entrar em terreno conquista seu controle. Capital inimiga não pode ser ocupada; capital própria vazia pode.
- Distância Manhattan, sem linha de visão. Ataques têm o alcance da unidade; não há contra-ataque. Cada criatura tem uma ação por turno. Criaturas recém-invocadas podem agir. Dano persiste, cura respeita a vida máxima; vida máxima do jogador 50.
- Cartas de criatura, comandante e feitiços só são anunciados na própria Principal com pilha vazia. Truques podem ser anunciados com prioridade na Principal e no Final, inclusive com pilha vazia.
- Após anunciar, o autor mantém prioridade; dois passes resolvem uma ação. Após resolver, a prioridade volta ao jogador ativo. Habilidades ao entrar recebem um novo item na pilha e podem ser anuladas.
- Dois passes com pilha vazia na Principal avançam ao Final. O jogador ativo também pode usar o botão Ir para Final; a janela de respostas no Final continua obrigatória. Dois passes no Final encerram o turno e removem bônus temporários.
- O ataque usa a identidade da criatura alvo e exige fonte viva e alcance válido ao resolver. Um alvo removido não é substituído por outra criatura na mesma casa. A invocação revalida casa vazia e controle. Efeitos inválidos não devolvem custos.
- Ataque à capital afeta a vida do jogador; uma criatura defendendo a capital é alvo primeiro. Derrota com zero de vida ou tentativa de compra sem cartas. A partida bloqueia novas ações após vitória.
- A detecção automática de loops, simultaneidade e camadas de efeitos ainda não foi implementada.

## Arquitetura

- `Scripts/Catalog.cs`: definições, arquétipos, decks padrão e validação, sem Unity.
- `Scripts/Game.cs`: estado, fases, comandos, custos, pilha, prioridade, alvos e efeitos, sem Unity.
- `Scripts/GameView.cs`: interface IMGUI, passagem de jogador e animações por eventos.
- `Scripts/BoardArt.cs`: imagens procedurais reutilizadas e liberadas ao fechar a tela.
- `Scripts/DeckEditorView.cs`: parte da interface dedicada ao editor.
- `Scripts/DeckStorage.cs`: JSON, validação ao carregar/salvar e substituição por arquivo temporário.
- `Editor/RuleChecks.cs`: verificações de regras e serialização.
- `Editor/ProjectSetup.cs`: menu TCG para preparar a cena e validar.

Decks ficam em `Application.persistentDataPath/decks/player-1.json` e `player-2.json`. O diretório depende da identidade do projeto Unity. Um arquivo inválido não é sobrescrito no carregamento: a interface usa o deck padrão e informa o problema no editor. O usuário pode salvar um deck válido depois. A mesma lista salva é preservada entre execuções; o estado da partida não é salvo.

## Fora desta versão

Salvamento de partidas, IA, multiplayer online, três/quatro jogadores, rituais, equipamentos, construções, efeitos contínuos genéricos, loop detection, arte ilustrada, animações de personagens esqueléticos, sons e tutorial interativo.

## Validação

O menu TCG > Validar regras executa 54 verificações de motor e serialização e 12 verificações de integração da interface (cliques, alvos destacados, troca de prioridade, fila de animações e edição de listas). Também foi gerado um executável de teste Windows e executados cenários de partida. As capturas de tela no modo oculto ficaram pretas porque a interface não recebeu eventos de desenho nesse modo; elas não servem como aprovação visual. O acabamento visual deve ser conferido no Play da Unity.
