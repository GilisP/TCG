# Personalidade, Mão e Formação

Implementação no projeto principal `C:\Users\gil\TCG`, cena Mesa, em 2026-09-18. Apresentação procedural low-poly, ainda provisória. Não modifica custos, ocupação ou movimento do motor.

## Mão de cartas
Cartas dispostas em leque, com inclinação gradual e sobreposição. Passar o mouse levanta e amplia uma carta, exibindo atributos e parte do texto; botão direito abre a ficha completa. Clique esquerdo seleciona a carta para jogar. A área elevada consome eventos de mouse para evitar colocar cartas acidentalmente no tabuleiro atrás dela. Mãos grandes usam páginas de nove cartas, com contagem e navegação; isso não limita a mão do motor. A troca de controlador reinicia a página e a tela de passagem oculta as cartas.

Os avatares sentados seguram cartas 3D com dedos/polegar e pequeno movimento de pulso. Apenas versos aparecem no mundo. Até cinco versos representam visualmente a mão; não revelam as definições privadas nem representam sua contagem exata quando maior que cinco.

## Modelos e animações
Guerreiros têm ombreiras, brasões, capas e plumas. Magos ganham barba e túnica; arqueiros, aljava e flechas; criaturas possuem olhos e detalhes próprios. Perfis existentes de arte continuam escolhendo o modelo, sem IDs ou cartas novos. Retratos são gerados com as mesmas figuras.

FigureMotion anima pernas/patas durante deslocamento, asas, respiração e oscilação de capa/arma. Variações de fase usam o ID da peça. Construções e equipamentos continuam imóveis; movimentação, ataque, morte e VFX existentes permanecem sob TableWorld. Animações dos detalhes não mudam a posição do collider raiz nem as regras.

## Ocupação compartilhada
Formação centralizada em linhas, com escala proporcional ao número de peças e colliders separados. Uma peça fica no centro; linhas incompletas também ficam centralizadas horizontalmente. Mantém o limite visual anterior de nove modelos por terreno. Acima disso, a contagem e a lista lateral permitem consultar/selecionar todos; não existe um novo limite mecânico. A formação se recompõe nas revisões da partida, preservando IDs e ordem dos ocupantes.

## Scripts e extensão
- Runtime/HandPresentation.cs: leque, paginação, hover e seleção.
- Runtime/HeldCards.cs: versos segurados pelos jogadores, detalhes de roupa e animação de pulso.
- Runtime/FigureDetails.cs e MedievalVisuals.cs: geometria compartilhada por modelos e retratos.
- Runtime/PiecePersonality.cs: PieceFormation e FigureMotion.
- Runtime/TableWorld.cs: aplica posições, escalas e colliders da formação.
- Editor/PresentationChecks.cs e Runtime/PersonalityDiagnostics.cs: verificação de formação e interação visual.

Tudo é construído durante Play, sem configuração adicional na cena. Para novos perfis, estender detalhes e movimentos locais; preservar o collider raiz, destinos de movimento e eventos existentes.

Validação em [[Validacao da Personalidade 2026-09-18]]. Avaliação artística do autor e sessões longas permanecem em [[04 - Pendencias de Implementacao]].

Na capital, a fortaleza usa um marco menor na borda traseira e as peças ficam ligeiramente à frente, evitando que o castelo encubra a formação. Isso é apenas posicionamento visual.
