# Estatísticas, missões e ruínas — 2026-09-24

## Regras autorizadas
O autor confirmou estatísticas acumuladas, por partida e recordes; preset inicial aprovado: partidas concluídas, vitórias, cartas jogadas e terrenos colocados, missões únicas com moedas e boosters configuráveis. Capital começa sobre uma carta retirada do deck de terrenos. As três casas iniciais adjacentes são ruínas sem efeito e sem mana; não retiram cartas do deck.

## Implementação
`Core/PlayerProgress.cs`: contadores autoritativos por assento, resumo com ID único, histórico das últimas 50 partidas, totais, recordes e recibos permanentes para impedir contabilização/resgate duplicado. Contam comandos válidos de jogar carta, conjurar comandante e conjurar do exílio; cópias automáticas e fichas não contam como cartas jogadas. Turnos são o contador global da partida. Somente partidas concluídas são contabilizadas; fechar o jogo ou sair antes do resultado não concede progresso. Duplas contabilizam vitória por equipe.

Perfil local: assento Âmbar; rede: assento do cliente, recebido do host. Não há conta central nem proteção contra edição local ou host desonesto. Diagnósticos automáticos não concedem progresso.

`Runtime/ProgressUI.cs`: Menu → Estatísticas e missões. Arquivo `StreamingAssets/Economy/missions.json` define IDs estáveis, métrica, meta, moedas e quantidade de boosters. Preset: concluir uma partida (50 moedas + 1 booster), jogar 10 cartas (100 moedas), colocar 10 terrenos (50 moedas + 1 booster), vencer uma partida (100 moedas + 1 booster). São missões únicas, sem reset diário.

Progresso integrado ao mesmo `collection-v1.json`, com campos opcionais compatíveis com esquema 3. Migração não reatribui saldo. Transações guardam cópia anterior em memória e persistem saldo, recibo e prêmio juntos; erro reverte. Boosters recebidos são abertos pela Loja → Boosters → Abrir recompensa, sem pagar moedas, com as mesmas regras de sorteio/repetidas do booster medieval.

Ruínas são definição interna `system-ruins`, fora da coleção e boosters, ocupáveis e substituíveis. Ao serem substituídas/destruídas não entram no cemitério de cartas. A rede reconhece essa definição e a revisão de compatibilidade foi alterada: builds antigas não entram na mesma sala. Modelos de ruínas mostram muro e coluna partidos.

## Git e internet
Remoto configurado: https://github.com/GilisP/TCG. Histórico inicial preservado. Commit `30d0aeb` contém a base anterior às alterações. Após autorização por código do Git Credential Manager, push concluído e HEAD remoto conferido: `30d0aeb`. Preferência do autor: backup antes das modificações do dia.

Vínculo UGS salvo confirmado. Teste real de duas instâncias via Relay aprovado, usando perfis anônimos separados, busca pública e reconexão depois de 20 segundos. Rodada final com quatro jogadores em duplas aprovada: revisão 60, movimento e câmera de todos, reconexão e zero erros. Não confundir esse tráfego real via Relay com computadores fisicamente distintos, ainda não verificados.

## Validação
Build `base-2026-09-24-182837.log`: 944 verificações aprovadas (918 anteriores + 26 novas, incluindo câmera). Diagnóstico `-tcg-progress-verify` passou em gravação/releitura, resgate repetido recusado, reversão de resgate/abertura ao falhar a escrita, abertura gratuita e inspeção de ruínas, sem erros. Capturas inspecionadas em `09 - Testes/Evidencias/progresso-relay-20260924/progresso`. Perfil isolado; nenhum prêmio concedido ao perfil real. Casos específicos em `Editor/ProgressChecks.cs`, incluídos no processo de build. Execução prioritária permanece em [[04 - Pendencias de Implementacao]].

## Câmera por assento
Pedido adicional do autor: câmera atrás da própria capital. A orientação usa a posição real da capital, para 2, 3 ou 4 jogadores. Online acompanha o assento local, independentemente de quem tem prioridade; mesa local muda com a passagem de controle. Os botões de rotação continuam disponíveis; a orientação automática é aplicada ao entrar na partida ou trocar de assento, sem reiniciar a cada pacote. `TableWorld.CapitalYaw` calcula a orientação; `UpdateSeatCamera` aplica na apresentação. Testes verificam os nove assentos das configurações 2/3/4 e a orientação de cada cliente no diagnóstico de rede.
