> Anotação do projeto Unity, registrada em 2026-09-14. Fonte de design: [nota atual no Obsidian](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/02 - Tabuleiro/Mapa 3D ou Pseudo-3D.md>). Este espelho não é implementação; em caso de atualização, consultar a fonte e sincronizar a nota.

**Atualização 2026-09-15:** há uma nova [base jogável local 2–4](Base-Jogavel.md), com mesa 3D real e expansões por dados. As orientações anteriores abaixo são contexto histórico; consulte a base e a Rodada 2 para o estado atual.

# Mapa 3D ou Pseudo-3D

**Estado: direção visual confirmada; apresentação nova ainda planejada.** Pedido do autor em 2026-09-14: o mapa deve ser 3D ou, pelo menos, ter uma aparência de 3D falso. As cartas continuam 2D.

[Tabuleiro](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/02 - Tabuleiro/Tabuleiro.md>) · [04 - Visual e Experiencia](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/11 - Planejamento/04 - Visual e Experiencia.md>) · [Perguntas 05 - Mapa e Direcao Visual](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/10 - Decisoes/Perguntas/Perguntas 05 - Mapa e Direcao Visual.md>).

## Critérios obrigatórios

- O tabuleiro deve transmitir volume e profundidade; a grade plana IMGUI atual não é o resultado visual pretendido.
- Manter a grade lógica 11×11 e a quantidade de capitais correspondente a 2–4 jogadores, uma por jogador.
- A apresentação não muda distâncias, alcance, custos, propriedade, regras de alvo ou movimento. Relevo visual não cria automaticamente vantagem mecânica.
- Cartas, texto de regras, custos e atributos permanecem 2D e legíveis.
- Terrenos, unidades, capitais, construções, proprietários e seleções devem ser distinguíveis. O alvo selecionado precisa corresponder à casa lógica correta.
- Tiles sem terreno continuam sem dono, sem criaturas e sem alvos, conforme R1-12. A exibição do vazio e a ação especial de colocar terreno ainda serão detalhadas.

## Duas soluções aceitas em escopo

| Solução | Como produz profundidade | Pontos a decidir |
|---|---|---|
| 3D real | Malhas para tiles/terrenos e objetos, câmera inclinada, materiais, luz e sombra | Estilo, detalhe de modelos, câmera e orçamento visual |
| Pseudo-3D / 2.5D | Projeção isométrica ou inclinada, sprites/planos em camadas, laterais de tiles, sombras e ordenação coerente | Ângulo, camadas, oclusão e limites de rotação |

**Proposta técnica para avaliação:** começar com câmera inclinada de projeção ortográfica, tiles com espessura aparente e cartas 2D sobrepostas. Pode ser montado com malhas simples ou sprites. Esta proposta é um ponto de partida, não uma escolha já feita pelo autor.

## Anotações para implementação no Unity

Versão confirmada: 6000.3.6f1. Projeto principal: `C:\Users\gil\TCG`. Cena atual: `Assets/TCG/Scenes/Partida.unity`. Scripts de apresentação atuais: `Assets/TCG/Scripts/GameView.cs`, `BoardArt.cs`, `GeneratedArt.cs`, `CardPresentation.cs` e `DomainView.cs`.

1. Inspecionar a cena e o pipeline efetivamente usado antes de trocar renderer, materiais ou câmera. Pacotes 2D instalados não determinam sozinhos o resultado visual final.
2. Manter `Game` como fonte do estado. Criar uma camada de apresentação que leia coordenadas, terreno, controlador e ocupantes; não duplicar regras em objetos visuais.
3. Definir uma transformação única entre índice da grade e posição/projeção visual. Na solução 3D, usar seleção por raycast quando apropriado; na 2.5D, resolver a sobreposição sem selecionar uma casa encoberta errada.
4. Separar seleção de tile, apontamento para alvo válido e comando de jogo. A área visual de uma casa vazia não a torna alvo válido.
5. Usar pontos de ancoragem para capital, construções, unidades e indicadores; ocupação e agrupamento dependem das respostas de combate.
6. Definir ordem de desenho, sombras, materiais, camadas e tratamento de oclusão; preservar legibilidade das cartas e indicadores de controle.
7. Atualizar a apresentação pela resolução de ações/eventos do motor. Animação de ataque/retorno não deve transferir controle ou mover definitivamente a unidade por conta própria.
8. Preservar cenas, IDs de assets e `.meta` existentes. Registrar assets provisórios/definitivos e não substituir a cena jogável sem preservar referência recuperável.

Os itens acima são notas de implementação, não scripts/classes já criados. Uma cópia destas orientações fica na pasta `Documentacao` do projeto Unity e é referenciada pelo AGENTS.md de lá.

## Verificação exigida antes de concluir a apresentação

- Conferir visualmente profundidade, luz/sombra e leitura dos 121 tiles.
- Testar seleção no centro/bordas, sobreposição de objetos, casas vazias e mudança de terreno.
- Conferir capital/controle para dois, três e quatro jogadores quando o motor suportar esses modos.
- Validar compra, pilha, ataque, retorno e conquista sem mudar regras para acomodar a animação.
- Testar textos/cartas na resolução mínima a definir e manter informações privadas fora da visão de outros jogadores.
- Medir desempenho após definir plataforma e metas; registrar capturas e problemas encontrados em Play.

**Não verificado nesta tarefa:** nova câmera, novos modelos, nova projeção ou visual em Play. Esta entrega registra a direção e os critérios; não afirma ter convertido o mapa.

