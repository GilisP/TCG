# Medieval — Validação de 2026-09-17

[[Medieval - Implementacao da Base de Cartas]]

## Resultado

- Unity 6000.3.6f1, projeto principal `C:\Users\gil\TCG`.
- Compilação e build Windows concluídos com código de saída 0.
- **69 checks de regressão + 143 checks medievais: 212 verificações passaram.**
- Catálogo: 72 definições do autor, 71 habilitadas e MED-147 indisponível aguardando referência de adjacência.
- Execução do player final: código 0; zero erros/exceções registrados durante o roteiro; aproximadamente 60 FPS no trecho de medição de três segundos.
- 20 arquivos `.meta` originais da pasta Foundation/expansões comparados com o backup, sem mudanças.
- Links do vault conferidos: nenhum destino ausente ou ambíguo.

## Evidências

Build/testes finais: `09 - Testes/Evidencias/base-2026-09-17-142908.log`.

Player: `09 - Testes/Evidencias/medieval-player-entrega-final-2026-09-17.log`.

Capturas e resultado: `09 - Testes/Evidencias/medieval-capturas-2026-09-17/`.

Manifesto da entrega: `09 - Testes/Evidencias/medieval-entrega-2026-09-17.json`.

## Cobertura

Verificações de importação/resolução das cartas habilitadas e rejeição da carta pendente; bônus temporários; adjacência; equipamentos e transferência; equipamento no terreno após morte; habilidade da Adaga aguardando pilha; construção imóvel/bloqueio; alcance sem atravessar vazio; prevenção do próximo evento de dano; bônus do Carrasco limitado ao turno; gatilhos de dano/morte.

Eventos visuais verificados no núcleo: anúncio na capital do conjurador, ausência de impacto antecipado, resolução em alvo fora da capital e movimento mágico com impacto no destino escolhido.

Roteiro visual no player: seis páginas do catálogo, ficha ampliada, invocação, anúncio de truque, escolha de alvo, resolução e mostra das famílias de modelos. Capturas inspecionadas; corrigidos retratos sobrepostos, texto cortado na mão e formatos das cartas. A instrumentação chama os mesmos comandos da interface; não equivale a uma sessão manual completa de cliques.

## Limitações

A medição curta de FPS não comprova desempenho em mesa cheia nem em sessões longas. Não houve teste de rede; o jogo continua local. Não foi comprovado balanceamento nem cobertura de todas as combinações entre 72 cartas.

A referência de adjacência da Corrente Fraca permanece aberta. As convenções de alvo na resolução, reação de Morte Oportuna, identidade de instância fora do tabuleiro e outras regras provisórias estão detalhadas na nota técnica.

A primeira execução oculta gerou imagens pretas e FPS não representativo; foi descartada como validação visual e repetida com janela visível.

Persistem avisos de encerramento de ComputeBuffer/FontEngine já observados anteriormente. Não houve crash nem exceção no roteiro final; a investigação de memória em sessões longas continua em P-09.

