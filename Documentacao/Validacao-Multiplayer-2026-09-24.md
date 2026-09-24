# Validação multiplayer por host — 2026-09-24

[[Multiplayer por Host - Arquitetura e Uso]] · [[04 - Pendencias de Implementacao]]. Principal: `C:/Users/gil/TCG`, Unity 6000.3.6f1, cena Mesa.

## Testes automatizados
Suite ampliada: **918 verificações** (813 anteriores + 105 de rede). `NetworkChecks.Run` integra `FoundationSetup.Prepare/Build`. Cobre assentos, capacidade, decks/prontidão, início pelo host, duplas, mão própria, ausência das mãos adversárias e ordem/seed, projeção não executável, comandos forjados/repetidos/antigos, colocação/fase, movimento, ataque/bloqueio, reconexão, expiração após 120 s, fechamento pelo host e escolha privada descartada no abandono sem travar a partida.

Primeira passagem com 105: `Evidencias/base-2026-09-24-151623.log`. Build final e capturas serão acrescentados abaixo após a rodada final.

## Execução multiprocesso
Primeiras execuções bem-sucedidas preservadas em `Evidencias/multiplayer-20260924/dois-processos-inicial` e `quatro-processos-inicial`:
- Dois processos: ambos chegaram à revisão 26; host detectou pausa, convidado recuperou assento/estado; zero erros.
- Quatro processos, duplas: todos chegaram à revisão 26; convidado 1 reconectou; os demais detectaram pausa; zero erros.
- Centenas de verificações por processo confirmaram que mãos adversárias não existiam na projeção recebida.

Falha corrigida: o diagnóstico tentava iniciar o Netcode na mesma fase de inicialização em que o SDK registra suas mensagens. Ele agora espera o primeiro frame. A primeira rodada visual também revelou mojibake em arquivos regravados; a codificação UTF-8 foi corrigida antes da rodada final. Não confundir as capturas iniciais com a aparência final.

## Limites e pendências de validação
Os processos separados rodam no mesmo computador e comunicam-se pelo transporte real em loopback. Isso não comprova conexão entre máquinas físicas, roteadores diferentes, latência alta ou perda de pacotes. Internet/Relay, código de sessão e descoberta pública têm implementação e compilação, mas ainda precisam de vínculo UGS e validação externa real. O painel não foi configurado: Computer Use interrompeu a operação por não conseguir identificar o URL do navegador com segurança.

Reconexão recupera a sessão em memória, não uma partida fechada. Encerrar o host termina a sala. Não há migração de host, classificação competitiva, economia online, nem propriedade de coleção validada por conta central. Reações sociais ainda são locais; não confundir isso com sincronização dos comandos mecânicos.

Snapshots têm limites de tamanho para evitar alocações arbitrárias; estresse com grande quantidade de fichas ainda deve ser medido. Avisos de encerramento de recursos gráficos do Unity, já existentes, não foram resolvidos como parte do transporte.

Próximo passo: vincular UGS e testar duas redes físicas, após conferir a interface final local. A lista prioritária permanece em [[04 - Pendencias de Implementacao]].

## Rodada final concluída
Build `Evidencias/base-2026-09-24-151841.log`: 918 verificações aprovadas e build concluída. Evidências finais em `Evidencias/multiplayer-20260924/dois-processos-final` e `quatro-processos-final`.

Dois processos (porta 7791) e quatro em duplas (7792): todos chegaram à revisão 26, enviaram movimento e registraram zero erros de execução. Em ambas as rodadas o convidado 1 reconectou e o host confirmou a pausa. Centenas de verificações por processo validaram a projeção privada. Capturas finais de lobby e partida inspecionadas: acentos corretos, mão própria preservada durante prioridade adversária e indicação de espera.

Regressão visual local: a primeira tentativa oculta falhou na asserção de hover; não foi considerada aprovada e o recibo antigo foi desconsiderado pela data. Repetição com janela visível 1600×1000 passou em hover, seleção, colocação de dois ocupantes e formação, com zero erros (personality-network-visible.log, recibo de 15:26:48). Não houve mudança de código entre tentativas; o diagnóstico depende da execução visual. Unity principal reaberto em Play: marcador em ditor-multiplayer-20260924.log confirma Assets/TCG/Scenes/Mesa.unity e table=True, sem exceções ou erros de compilação encontrados.

Vínculo UGS confirmado posteriormente em ProjectSettings: projeto TCG, organização gilis_p. Isso conclui a conferência do vínculo salvo, sem comprovar Relay ou busca pública em execução.


## Continuação: teste real pelo Relay
Build `base-2026-09-24-182233.log`: 935 verificações e build aprovadas. O diagnóstico 7801 falhou por timeout SSL antes de criar sala. A repetição 7802 autenticou, criou e encontrou sala, mas revelou erro 23000 de eventos de lobby na reconexão ao criar segunda sessão. Corrigido mantendo sessão MPS e renovando explicitamente a alocação Relay do cliente por API pública.

Rodada 7803: dois processos, ambos revisão 26, dois movimentos enviados por processo, convidado reconectado depois de 20 segundos, pausa confirmada pelo host, zero erros de execução. Busca pública retornou a sala compatível. Evidências em `Evidencias/progresso-relay-20260924/relay-dois` e logs `relay-7803-*.log`. A sala temporária foi encerrada ao concluir. Trata-se de conexão real via Relay a partir de um computador; não comprova comportamento em dois roteadores físicos diferentes.


## Rodada final com ruínas, progresso e câmera por assento
Build final `base-2026-09-24-182837.log`: **944 verificações** (918 anteriores + 26 de progresso/ruínas/câmera), build concluída.

O diagnóstico 7804 chegou a sincronizar os quatro clientes e reconectar, mas terminou na revisão 26 antes de um assento conseguir movimentar uma peça com a nova mana inicial. A asserção recusou corretamente esse resultado; o cenário de quatro jogadores foi prolongado até a revisão 60, sem mudar as regras para satisfazer o teste.

Rodada final **7805**, quatro processos e duplas via Relay real:
- Host: revisão 60, 1.843 verificações de privacidade, 2 movimentos, pausa observada, zero erros.
- Convidado 1: revisão 60, 1.225 verificações, 3 movimentos, reconectou após 20 segundos, zero erros.
- Convidado 2: revisão 60, 2.302 verificações, 2 movimentos, pausa observada, zero erros.
- Convidado 3: revisão 60, 2.321 verificações, 2 movimentos, pausa observada, zero erros.
- Busca pública retornou uma sala compatível. Todos os clientes passaram na asserção de câmera atrás da própria capital. Capturas host/convidado inspecionadas: capital própria no lado próximo da mesa e mão própria durante prioridade adversária.
- Encerramento aguardou `NetworkSession.Stop` e os quatro processos fecharam. As salas de diagnóstico são temporárias.

Evidências em `Evidencias/progresso-relay-20260924/relay-quatro`, logs `relay-7805-*.log`. A busca e a comunicação realmente passaram por UGS/Relay; as instâncias executaram no mesmo computador. Resta avaliação entre máquinas/redes físicas distintas e sessão longa/perda de pacotes. Os avisos gráficos de encerramento já conhecidos não equivalem a erros de execução da partida.
