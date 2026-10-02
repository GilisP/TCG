# Decks Planejados - Integracao 2026-10-02

O autor pediu “implemente os decks planejados”. As listas de [[Decks completos - Avaliacao 2026-10-01]] foram autorizadas para integração como **protótipos de teste**; isso não torna o balanceamento definitivo. As fontes editoriais de 2026-10-01 foram preservadas.

## Uso no jogo

No projeto principal `C:/Users/gil/TCG`, cena `Assets/TCG/Scenes/Mesa.unity`:

1. Abra **Coleção → Decks planejados · 29**.
2. Importe uma lista com suas cartas gratuitas de teste, ou importe as 29 de uma vez.
3. A lista aparece em Meus decks; principais, terrenos vinculados, comandante e cosméticos continuam editáveis.
4. Em Jogar, escolha os decks salvos por assento. Na sala de rede, use o mesmo deck salvo.
5. **Experimentar comandantes** usa diretamente as listas completas em uma partida temporária, sem conceder inventário.

Cada modelo tem 100 principais diferentes + 50 terrenos diferentes + comandante separado. Reimportar abre o deck já existente e preserva suas edições; não reatribui saldo, não redefine cosméticos e não cria duplicatas do mesmo modelo. Alterações continuam dependendo de salvar o rascunho. Importação é explícita, nunca automática ao abrir o jogo. Cartas de teste são gratuitas nessa operação; os boosters não foram ampliados com este lote.

## Conteúdo e fontes

- `Assets/StreamingAssets/Decks/planned-decks.json`: 29 listas, IDs originais preservados.
- `Assets/StreamingAssets/Expansions/planned-decks-20261001.json`: 641 definições adicionais — 570 propostas PD26, 70 rascunhos MED usados nas listas e o comandante IDEIA-004. As 160 definições já existentes continuam em seus arquivos originais.
- `Tools/import_planned_decks.py`: compilação editorial explícita, sem interpretar texto durante a partida. Uma identidade sem mapeamento interrompe a geração. Não executar `gerar_decks.py` sobre listas já revisadas sem versionar a revisão.
- Os dados editoriais de 2026-10-01 continuam como fonte histórica. Raridade `prototype`, retratos/modelos existentes provisórios; não há ilustrações definitivas novas.

O catálogo adicional contém também propostas de reserva. O preset continua sem repetições mesmo quando um terreno básico tem a exceção impressa de até 50 cópias. Portões mantêm sua identidade original e uma única cópia na lista; outro Portão pode aparecer nas pilhas compartilhadas.

## Habilidades e arquitetura

`Core/CardAbilities.cs` implementa habilidades por dados: gatilhos, condições, alvos, custos em mana/PA, limites por turno e operações de bônus, prevenção, compra, seleção do topo, cemitério, dano, cura, movimento, sacrifício, anexos e terrenos. `Content.cs` valida o contrato ao carregar e copia definições; estado de uso pertence a `Match`, não à definição compartilhada.

Integração nos eventos existentes: entrada, morte, morte em combate, ataque/defesa, dano, começo de turno, Truque, adjacência, anexos e movimento. Ataques colocam os gatilhos acima do combate na pilha. Auras se recalculam por posição; bônus temporários expiram. Equipamentos PD26 exigem hospedeiro no mesmo terreno, conforme seus textos; as regras anteriores de outros equipamentos foram preservadas.

Terrenos de utilidade geram sua mana na renovação normal. A habilidade adicional aparece ao selecionar o terreno quando suas condições forem satisfeitas; custa 2 manas genéricas e pode ser usada uma vez no próprio turno. O controlador do reino ativa, sem mudar o dono original. As quatro pilhas compartilhadas e as ruínas iniciais continuam vigentes.

`Core/PlannedDecks.cs` valida modelos e importa cópias para o inventário. `Runtime/PlannedDeckUI.cs` carrega e apresenta os modelos. Persistência reutiliza `CollectionStore` e esquema 3, com recuperação pelo último arquivo válido se houver falha de gravação. `ContentLoader.CommanderTable` seleciona as listas completas por identidade do comandante.

## Comandante da Frota Afundada

Resposta do autor em 2026-10-02: **“Usar a sugestão provisória”** à pergunta de 2 PA, uma vez no seu turno, recuperando de qualquer cemitério uma carta que veio diretamente do grimório.

- IDEIA-004, Ar/Lua, AAL3, 3/4, 2 PA, Pirata.
- Dano de suas criaturas a capitais gera um gatilho na pilha para enviar ao cemitério do jogador atingido aquela quantidade de cartas do topo do grimório.
- Ativação paga 2 PA e tem limite por turno. A escolha lista somente entradas cuja origem direta foi o grimório.
- `Core/Graveyard.cs` mantém origem por **entrada**, distinguindo cópias iguais e removendo o vínculo ao sair do cemitério. Uma morte posterior não reutiliza a condição antiga.
- Cartas recuperadas de outro jogador preservam o dono original ao serem conjuradas e voltarem ao cemitério.

As outras três ideias — Saída de emergência, Raio em cadeia e Farol guia — não estavam nas listas. Continuam em [[Ideias autorais - Frota e Apoios 2026-10-01]], com detalhes pendentes; não foram silenciosamente incorporadas com regras inventadas.

## Subtipos dos rascunhos antigos

Os rascunhos MED anteriores não traziam subtipos. Esta integração fornece subtipos **provisórios explícitos nos dados**, listados abaixo, para que não haja inferência em execução. Subtipo não concede palavra-chave: por exemplo, Dragão não ganha Voar automaticamente. A avaliação do autor pode ajustar essas atribuições.

| Carta | Subtipos de teste |
|---|---|
| MED-001 — Recruta da Aurora | Humano, Soldado |
| MED-002 — Vigia do Pórtico | Humano, Soldado |
| MED-005 — Porta-Estandarte da Vila | Humano, Soldado |
| MED-008 — Cavaleiro do Juramento | Humano, Cavaleiro |
| MED-010 — Aldren, o Cavaleiro Redimido | Humano, Cavaleiro |
| MED-011 — Guardião do Primeiro Alvorecer | Espírito, Guardião |
| MED-012 — Escudeiro do Crepúsculo | Humano, Soldado |
| MED-013 — Salteador das Catacumbas | Humano, Ladino |
| MED-016 — Acólito do Último Sino | Humano, Clérigo |
| MED-019 — Ceifador da Vigília | Espírito, Guerreiro |
| MED-021 — Mirela, Guardiã dos Nomes | Humano, Mago |
| MED-022 — Sentinela do Túmulo Real | Esqueleto, Soldado |
| MED-023 — Guarda do Vau | Humano, Soldado |
| MED-024 — Piqueiro do Estuário | Humano, Soldado |
| MED-027 — Escriba da Ponte | Humano, Mago |
| MED-030 — Vigia das Marés | Humano, Mago |
| MED-032 — Nerina, Guardiã do Estuário | Humano, Mago |
| MED-033 — Serpente do Fosso Real | Serpente |
| MED-034 — Lanceiro da Fornalha | Humano, Soldado |
| MED-035 — Saqueador da Estrada | Humano, Guerreiro |
| MED-038 — Berserker do Cerco | Humano, Guerreiro |
| MED-041 — Aríete Vivo | Construto |
| MED-043 — Roderic, o Rei do Cerco | Humano, Nobre |
| MED-044 — Dragão da Torre Queimada | Dragão |
| MED-045 — Mensageiro das Ameias | Humano, Batedor |
| MED-046 — Batedor das Colinas | Humano, Batedor |
| MED-049 — Arqueira do Campanário | Humano, Arqueiro |
| MED-066 — Gigante da Montanha Antiga | Gigante |
| MED-067 — Mercenário da Estrada | Humano, Mercenário |
| MED-068 — Sentinela de Pedra | Construto |
| MED-070 — Batedor Contratado | Humano, Batedor |
| MED-071 — Autômato do Portão | Autômato, Construto |
| MED-072 — Orven, o Cavaleiro sem Brasão | Humano, Cavaleiro |
| MED-081 — Guardião das Sepulturas | Esqueleto, Guardião |
| MED-083 — Batedora da Ponte Suspensa | Humano, Batedor |

## Rede, testes e limites

O host continua executando as regras. A rede projeta atributos calculados e a ação legal de ativar terrenos; escolhas do topo e da mão são visíveis apenas ao jogador correspondente. Protocolo/fingerprint atualizado para `tcg-network-5/20261002-planned-decks`; todos os participantes precisam da mesma versão.

`Editor/PlannedDeckChecks.cs` verifica modelos, preservação de edições, persistência isolada, efeitos representativos, custo de sacrifício, proveniência do cemitério, pirata, execução das cartas importadas e projeções com 2/3/4 assentos. `FoundationSetup` inclui os testes. `-tcg-planned-decks-verify` usa perfil isolado para a interface; `-tcg-planned-net` seleciona os modelos novos no diagnóstico de rede existente.

Estado de validação e evidências em [[Validacao dos Decks Planejados 2026-10-02]]. Testes automatizados não comprovam equilíbrio das 29 listas, sessões longas ou experiência entre computadores físicos. Arte permanece provisória. Pendências gerais e critérios de conclusão somente em [[04 - Pendencias de Implementacao]].
