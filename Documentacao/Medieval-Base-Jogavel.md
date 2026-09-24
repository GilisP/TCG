# Medieval — Implementação da base de cartas

Atualização: 2026-09-17. [[Medieval - Lote do Autor 2026-09-17]] · [[04 - Pendencias de Implementacao]].

## Escopo autorizado

O autor escolheu as 72 cartas recebidas como base para implementação, com design das cartas, modelos 3D e efeitos de magias/truques. Não se trata das 84 sugestões anteriores, que permanecem preservadas.

Confirmado nesta conversa:
- Anúncio da magia/truque na capital de quem jogou; efeito no alvo apenas na resolução.
- Construções comuns/raras/lendárias com defesa provisória 4/6/8, imóveis, sem ataque e sem dano ao defender.
- Equipamentos permanecem no terreno quando o hospedeiro morre.
- Adjacência entre tiles ortogonais, sem incluir o mesmo tile.

## Arquivos no projeto principal

Projeto: `C:\Users\gil\TCG`. Cena: `Assets/TCG/Scenes/Mesa.unity`.

- `Assets/StreamingAssets/Expansions/exp-001-basico.json`: 72 definições MED-085 a MED-156, cores/custos, PA, alcance, texto, raridade, perfil visual e comportamentos.
- `medieval-terrenos-teste.json`: seis terrenos auxiliares, separados das 72 cartas.
- `Foundation/Core/Content.cs`: dados imutáveis; tipos Construção/Equipamento; PA, alcance, custo de equipar, programas de efeito e traços validados.
- `Foundation/Core/MedievalRules.cs`: modificadores, prevenção, auras, gatilhos, recuperação, equipamentos, movimento por efeito e escolhas. Não depende de Unity.
- `Foundation/Core/Match.cs`: ações validadas, pilha, integração com combate/turnos e eventos para apresentação.
- `Foundation/Runtime/ContentLoader.cs`: mesa medieval; seleção da primeira cor no menu.
- `Foundation/Runtime/MedievalUI.cs`: ficha ampliada, escolhas durante resolução e transferência de equipamentos.
- `Foundation/Runtime/MedievalVisuals.cs`: miniaturas e retratos procedurais, animações por cor na capital/alvo.
- `Foundation/Runtime/TableView.cs` / `TableWorld.cs`: mesa e integração de interface.
- `Foundation/Editor/MedievalChecks.cs`: verificação do conjunto e casos de comportamento.
- `Foundation/Runtime/MedievalDiagnostics.cs`: diagnóstico explícito `-tcg-medieval-verify`; não roda numa partida normal.

## Como experimentar

No Unity, abra o projeto principal e use **TCG → Jogar no Unity**. No menu, selecione **Medieval · 72 cartas** e escolha a primeira cor; os outros lugares recebem as cores seguintes. Mesa local de dois a quatro jogadores, com duplas opcionais em quatro.

Os decks de teste contêm 48 cartas principais com repetições e 50 terrenos, sem Comandante. Não são decks oficiais. Cartas indisponíveis por dúvida de regra são excluídas do baralho, mantendo presença no catálogo.

Clique na carta para jogar; use botão direito para abrir a ficha completa. Na biblioteca, clique na carta para inspecioná-la. Efeitos que exigem escolha abrem um painel na resolução. Selecione um equipamento no terreno para equipar/transferir. O Altar apresenta botão de ativação quando disponível.

## Mecânicas incluídas

Corpos simples, PA impresso, alcance ortogonal por terrenos existentes, bônus de ataque/defesa/alcance, prevenção do próximo dano, auras de proximidade, dano e dano em área, perda de vida da capital, recuperação e fundo do deck, devolução à mão, deslocamento, troca de posições, proteção contra deslocamento, construções defensivas e equipamentos.

Gatilhos de entrada/morte/turno/dano/ataque/movimento entram na pilha e pedem escolhas quando aplicável. Equipar segue a regra existente: custo separado, no próprio turno, fora da pilha, múltiplos anexos e transferência paga.

Retratos e miniaturas usam famílias visuais: soldado, arqueiro, mago, fera, criatura alada, gigante, serpente marinha, realeza, construção, equipamento e sigilo. São recursos procedurais provisórios, não 72 ilustrações ou esculturas finais exclusivas. Cor identifica elemento; borda identifica raridade; ficha ampliada mostra custo colorido e texto integral.

## Convenções do ambiente de teste

Estas convenções não fecham perguntas antigas automaticamente:
- Movimento padrão permanece 2, valor já existente no motor; PA é separado. Mantidos os textos originais dos quatro equipamentos recebidos, com custo experimental de equipar de 1 mana genérica.
- Efeitos medievais com escolha de alvo abrem seleção na resolução. A especificação final do momento dos alvos continua aberta em [[Sistema de Pilha e Prioridade]].
- Distâncias “até N” usam distância ortogonal. Alcance ataca em linha reta, exige terrenos no percurso e não atravessa vazio; ocupantes intermediários não bloqueiam o disparo neste teste.
- Deslocamento por efeito não conquista terreno nem declara ataque. Destinos exigem percurso de terrenos; imunidades e bloqueios são consultados.
- Construções precisam de presença aliada no tile para suas habilidades, conforme a regra histórica não revogada; essa condição deve ser avaliada nas partidas.
- Ordem de gatilhos simultâneos e do dano de retorno preserva a ordem determinística do motor; não é uma resolução final das dúvidas de prioridade.
- Morte Oportuna usa o último registro de morte de criatura sua causada por inimigo no turno atual. A janela reativa exata ainda precisa ser especificada; avaliar esse comportamento antes de tratar como regra final.
- Criaturas e equipamentos têm instância no tabuleiro; mão/cemitério ainda usam definições. Cenários com múltiplas cópias idênticas e efeitos retardados precisam da evolução de identidade em todas as zonas (P-11).

## Pendência localizada

**MED-147 — Corrente Fraca:** indisponível para jogar até o autor dizer a que ela se refere por “criatura adjacente”: uma criatura sua escolhida ou a capital. O catálogo expõe a justificativa e a mesa não a inclui no baralho. Não considerar as 72 cartas integralmente concluídas enquanto essa resposta faltar.

## Validação

Compilação isolada de Core/Runtime/Editor passou; a regressão anterior no Unity passou com 69 checks. O conjunto medieval possui 143 checks automatizados, cobrindo importação, resolução básica e casos de bônus, equipamentos, construções, alcance e dano. Conferir a evidência final em [[Medieval - Validacao 2026-09-17]].

Uma primeira captura em janela oculta ficou preta apesar de não haver exceções. Foi descartada como evidência visual. Não usar o FPS dessa execução oculta como medida de desempenho.

O trabalho preserva arquivos `.meta`, cena existente e código legado. Backup anterior: `99 - Arquivo/Auditorias/unity-antes-medieval-2026-09-17.zip`. A revisão automática rejeitou a primeira transferência ampla; após compilação isolada e testes, uma aplicação restrita aos arquivos alterados foi aceita.

