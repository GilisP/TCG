# Validação — comandantes, foil e modelos — 2026-09-24

Implementação: [[Comandantes Foil e Acabamento Visual 2026-09-24]]. Principal: `C:/Users/gil/TCG`, Unity 6000.3.6f1, cena Mesa. Nenhuma implementação feita na cópia arquivada.

## Motor e build
Build final aprovado em `Evidencias/base-2026-09-24-143258.log`. Compilação e 813 verificações passaram: Foundation 69, Medieval 143, QOL 48, Movement 79, Reaction 13, Presentation 174, Collection 17, Author 38, Hub 19, Bicolor 143, Booster 16 e NewCommander 54.

Casos novos: registro dos 11 comandantes; consumo de mana/bônus temporário; gatilho de PA zerado uma vez por turno; replay gratuito e exílio após resolver/anular; permissão paga de Admirador; Troca de Destino do exílio respondendo à morte; prevenção/reflexão de dano ímpar; cura como evento; aura de Cavaleiros por tile; roubo de encantamentos preservando dono; ficha/equipamento; infiltração; pagamento de moagem com PA e mana de qualquer cor; destino da carta roubada; foil por coleção/deck e persistência; filtro de comandantes.

## Execução e inspeção visual
Diagnóstico `-tcg-foil-commanders-verify` com perfil isolado: **zero erros de execução**. Confirmou filtro com 28 comandantes, foil salvo/recarregado, invocação real com um brilho de entrada, movimento sem novo brilho e limpeza dos efeitos. Capturas finais inspecionadas: filtro, ficha foil, entrada na mesa, seis modelos e três efeitos. A primeira captura da mesa estava coberta pela coleção; o diagnóstico foi corrigido e repetido antes da inspeção final.

Evidências finais em `Evidencias/comandantes-foil-20260924/`; log `Evidencias/foil-commanders-final-20260924.log`. Os perfis isolados não substituem o inventário do usuário.

## Limites
Modelos e retratos continuam procedurais/provisórios. O novo lote tem regras de teste autorizadas e subtipos propostos; Arauto da Lua Rubra é uma proposta ajustável. A inspeção das capturas não equivale a uma partida manual completa. Rede e avaliação de desempenho prolongado não foram feitas. Persistem avisos do Unity ao encerrar sobre ComputeBuffer/FontEngine; não foram classificados como erros pelo coletor, mas merecem acompanhamento em sessão longa.

Próximo passo e critérios de conclusão em [[04 - Pendencias de Implementacao]]: avaliação autoral das regras, modelos e efeitos, seguida de partida prolongada.

Verificação adicional `-tcg-personality-verify`: hover, seleção e colocação por eventos de mouse, dois ocupantes no mesmo tile e animação de respiração passaram, zero erros. Resultado preservado em Evidencias/comandantes-foil-20260924/personality-result.txt.


Editor principal reaberto e entrada em Play confirmada no log Evidencias/editor-comandantes-foil-20260924.log: TCG EDITOR PLAY, C:/Users/gil/TCG/Assets, scene=Assets/TCG/Scenes/Mesa.unity, table=True. Sem erros de compilação ou exceções encontrados nessa abertura.

