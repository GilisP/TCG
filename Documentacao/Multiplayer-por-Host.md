# Multiplayer por host — arquitetura e uso

Implementação autorizada em 2026-09-24 para as cinco etapas: conexão entre computadores, autoridade/privacidade, 2–4 jogadores, internet/reconexão e busca pública. O autor escolheu **jogadores como hosts** e **2 minutos para reconexão**, seguidos de derrota por abandono. Não introduzir servidor dedicado pago como requisito para jogar.

## Estado
Código integrado no projeto principal `C:/Users/gil/TCG`, cena Mesa. Compilação e 944 verificações aprovadas. Vínculo UGS confirmado e testes reais via Relay com dois e quatro processos aprovados, incluindo busca pública e reconexão após 20 segundos. Ver [[Estatisticas Missoes e Ruinas]] e [[Validacao Multiplayer por Host 2026-09-24]]. Quatro jogadores em duplas também passaram, com movimento dos quatro e câmera atrás da própria capital. Os processos estão no mesmo computador, mas o tráfego passa pelo serviço Relay; ainda falta uma partida entre computadores/redes físicos distintos.

## Responsabilidades
- `Core/NetworkRoom.cs`: assentos vinculados à conexão, segredo de reconexão, validação do deck, prontidão, início pelo host, sequência e revisão dos comandos, pausa e abandono.
- `Core/NetworkView.cs`: projeção por destinatário, sem mãos adversárias, ordem dos decks, semente aleatória ou opções privadas de outros jogadores. Envia contagens, estado público, mão própria e possibilidades de ação calculadas pelo host.
- `Match` continua como motor autoritativo. A projeção recebida é somente leitura: `Try` recusa execução local. A apresentação não sorteia cartas nem resolve efeitos por conta própria.
- `Runtime/NetworkSession.cs`: Netcode for GameObjects e Unity Transport, mensagens limitadas e comprimidas, eventos e estado por assento. Liga a rede local ou sessões UGS com Relay.
- `Runtime/NetworkUI.cs`: criar sala, entrar por IP/código, buscar salas públicas compatíveis, escolher deck, ficar pronto e iniciar. A mesa existente apresenta a visão recebida e envia comandos ao host.
- `Editor/NetworkChecks.cs` e `Runtime/NetworkDiagnostics.cs`: testes de privacidade/autoridade e processos separados com desconexão.

## Uso
Na tela Jogar, abrir **Salas online / rede local**.

Rede local: o host escolhe 2, 3 ou 4 jogadores e cria sala. Convidados informam seu IP local; porta UDP 7777. Cada participante envia seu próprio deck salvo ou o preset explícito Sol de teste e marca pronto. O host inicia quando todos estiverem prontos. Duplas usa assentos 1+3 contra 2+4.

Internet: vincular o projeto em Edit → Project Settings → Services e habilitar os serviços necessários no painel da conta Unity. A criação produz código compartilhável; salas públicas aparecem na busca. Relay retransmite a conexão, mas o motor fica no computador do host. Não foram adicionados cartões, compromissos de pagamento ou credenciais permanentes pela automação.

Durante uma desconexão detectada, a partida aguarda até dois minutos. O convidado usa Reconectar na tela da sala; recebe o mesmo assento e um estado completo atual. Após o prazo, perde por abandono. O estado permanece apenas na memória do host, respeitando a decisão de não salvar partidas fechadas. **Saída do host encerra a sala; migração de host não está implementada.**

## Segurança e limites
O transporte identifica a conexão; o cliente não pode escolher outro assento nos comandos. Sequência evita reaplicação; revisão rejeita ações antigas. O host valida cartas/custos/alvos e mantém as zonas privadas. Um host modificado tem acesso ao estado completo — essa é uma limitação da hospedagem por jogador, não uma proteção competitiva equivalente a servidor confiável.

Inventário e moedas continuam locais, sem economia online confiável. A validação da sala verifica legalidade do deck, não comprova propriedade em uma conta central. Perfis de teste não concedem cartas ao usuário.

Catálogos divergentes são recusados por impressão digital e versão de protocolo. Novas regras de rede exigem atualizar o protocolo e testes. Não transmitir catálogo de decks ou logs do motor aos demais jogadores para tentar corrigir apresentação: logs e escolhas podem conter informações privadas.

## Dependências e fontes
Pacotes fixados: `com.unity.netcode.gameobjects` 2.13.3 e `com.unity.services.multiplayer` 2.3.3, compatíveis com Unity 6000.0+ segundo o registro oficial. Projeto permanece em 6000.3.6f1. Assets `.meta` e modo local preservados.

Referências oficiais consultadas: [Relay com hosts jogadores](https://docs.unity.com/en-us/mps-sdk/networking/relay-servers), [entrada por código, busca e reconexão](https://docs.unity.com/en-us/mps-sdk/join-session), [mensagens customizadas NGO](https://docs-multiplayer.unity3d.com/netcode/2.0.0/advanced-topics/message-system/custom-messages/). A API instalada foi inspecionada durante a implementação.

Execução prioritária e critérios restantes somente em [[04 - Pendencias de Implementacao]].

## Reconexão Relay corrigida nesta continuação
O teste real revelou erro de eventos de lobby ao recriar a sessão na reconexão. Agora as sessões MPS gerenciam diretório, membros, código e heartbeat; o transporte Relay é configurado explicitamente pelas APIs públicas RelayService e AllocationUtils, usando DTLS. O código Relay fica em propriedade visível apenas aos membros; a busca recebe somente metadados públicos. A sala começa bloqueada enquanto configura o transporte. Na reconexão, mantém-se a sessão existente, atualizam-se os dados e obtém-se uma nova alocação de cliente. Não depende da validade da alocação antiga. O segredo do protocolo recupera o assento no host. A revisão de compatibilidade foi alterada para impedir entrada de builds antigas.
