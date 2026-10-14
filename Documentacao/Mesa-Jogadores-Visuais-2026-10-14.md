# Mesa e jogadores — estilo 3D suave

Pedido de 2026-10-14, VIS-MESA-014. Estende a linguagem dos comandantes ao tabuleiro, capitais, ruínas, avatares sentados e mobiliário. Não altera regras.

## Implementação
StyledTerrain reutiliza CmdPart/CmdLine e as malhas compartilhadas de CommanderModels. Superfícies e pedras têm bordas suaves; detalhes de floresta, nascente, campo e rochas ficam na periferia. Capitais têm torres, ameias, portão e estandarte. Ruínas mantêm a regra de não gerar mana. A classificação visual por cor permanece provisória: não representa novos efeitos de terreno.

StyledPlayers cria rostos, cabelo, roupas, mãos, botas e cadeiras estofadas. TableEnvironment mantém lugares, colliders de reação, pilhas e rotações. HeldCards continua mostrando apenas versos, sem revelar informações privadas. Salas recebem detalhes em colunas, estandartes e garrafas. StyledTable constrói madeira com acabamento em latão e pés torneados.

## Contratos preservados
121 casas nas posições originais, alturas da superfície e formação das peças; CellHit e PlayerAvatarHit; quatro pilhas comuns; câmera atrás da capital; cartas na mão e reações. Decoração suave não tem collider nem valida ações. Match continua responsável pelas regras.

## Verificação
Compilação/build e 1.041 verificações do novo diagnóstico aprovados; 969 verificações dos comandantes e 13 da UI atual aprovadas. Fixtures antigos de hover/arraste/pilhas falharam e reação não concluiu; interação manual e atualização desses testes pendentes. Novo diagnóstico -tcg-styled-world-verify usa perfil isolado, valida geometria/picking/assentos e gera renders nativos. Capturas não comprovam interação manual no Editor. Modelos procedurais articulados, sem escultura/FBX definitivo; avaliação autoral e desempenho prolongado pendentes.
