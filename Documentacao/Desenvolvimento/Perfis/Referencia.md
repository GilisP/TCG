# Contrato dos perfis — referência operacional do TCG

As instruções permanentes estão em AGENTS.md. Esta nota registra apenas formatos e infraestrutura. Fonte literal preservada: Documentacao/Desenvolvimento/Fontes/Contrato-perfis-20261014.txt; no vault, 99 - Arquivo/Fontes Originais/Coordenacao/Contrato dos perfis 2026-10-14.txt.

## Infraestrutura constatada

| Recurso | Estado |
|---|---|
| tasks.json | Registro legado de tarefas, sem migração de campos/tipos |
| checkpoint.json | Última retomada do jogo, conteúdo e schema preservados |
| Tools/Resume-Development.ps1 | Leitura/resumo dos registros; não é leitor com orçamento nem CLI de coordenação |
| Briefings/checkpoints por tarefa | Arquivos JSON e atribuição/aceite cooperativos |
| CLI, leitor especializado, reuso.py e leases | Ausentes, confirmado pelo autor; nenhuma automação instalada nesta entrega |

Não há comandos de ack ou renovação de lease a executar. Atribuição e recebimento registrados em arquivos são compromissos cooperativos; não oferecem bloqueio do computador, expiração ou renovação automática. Esta adaptação documental não altera configurações globais do Codex nem cria agentes permanentes. A IA adversária continua sendo uma funcionalidade do jogo separada desses perfis de desenvolvimento.

## Schemas de novos arquivos

Briefing: task_id, project_id, objetivo, resultado_esperado, contexto_essencial, restricoes[], fontes[], dependencias[], criterios_conclusao[], recursos[].

Fonte: path, start_line, end_line, sha256/revisão quando pertinente. Recurso: path, acesso, responsavel. Um nome de módulo não amplia a lista de arquivos concedidos. Dependência identifica tarefa/artefato e o aceite necessário.

Checkpoint: objective, current_state, decisions[], pending[], refs[]. Recebimento pode ser registrado em current_state. Checkpoints legados mantêm seu schema. Aceite referencia evidência e hash/revisão examinados; ack não é autorização humana.

## Papéis por tarefa

| Perfil | Recurso identificado no briefing |
|---|---|
| Coordenador | Compartilhados, contratos, integração, documentação e processos atribuídos |
| Especialista | Arquivos liberados da área de regras/cartas, apresentação, rede ou conteúdo |
| Revisor independente | Fontes/evidências em leitura; somente relatório/checkpoint próprios em escrita |
| Triagem | Inventário pequeno somente leitura; escolha de modelo conforme AGENTS |

## Entrega e conversão

Mensagem compacta: Status, Result e, quando pertinentes, Files, Validation, Pending. Não é payload de CLI.

Relatório novo de seis campos portugueses: status (concluido/parcial/bloqueado/falhou), resumo (texto), arquivos (lista), evidencias (lista), pendencias (lista), proximo_passo (texto). Ausências: [] nas listas e "" nos textos. A conversão conserva somente evidências efetivamente recebidas; aprovação documental não comprova infraestrutura operacional inexistente.

Os formatos anteriores dos registros não foram convertidos. Um futuro consumidor CLI precisará declarar seu schema/comandos antes de integração.

## Retomada e prioridade

AG-PROFILES-014-brief.json identifica esta tarefa; AG-PROFILES-014-checkpoint.json salva suas decisões e pendências. O checkpoint legado do jogo continua separado. A fila única permanece em 00 - Projeto/04 - Pendencias de Implementacao.md no vault.
