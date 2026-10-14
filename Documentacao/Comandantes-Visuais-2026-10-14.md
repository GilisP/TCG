# Comandantes — ilustrações, modelos e animações 2026-10-14

[[13 - Comandantes Arte Modelos e Animacoes]] · [[Validacao dos Comandantes Visuais 2026-10-14]] · [[04 - Pendencias de Implementacao]].

## Resultado e uso
29 ilustrações individuais e29 prefabs dos comandantes integrados no projeto principal. Modelo suave e detalhado, de miniatura estilizada, com roupas, armas, rostos e acessórios próprios. Anatomias dedicadas:árvore,Ursa,abismo eKhan montado. Proposta visual em avaliação pelo autor; não declarar arte definitiva. As demais tropas e terrenos não foram redesenhados nesta tarefa.

**TCG → Arte → Visualizar comandantes no Play** abre a galeria. Selecione personagem e os estadosEsperar,Caminhar,Atacar,Morrer,Comemorar. **TCG → Jogar no Unity** restauraMesa como entrada dePlay. Galeria somente apresenta, sem coleção ou partida.

## Arquivos e dependências
- Resources/CardArt/Commanders:29 PNGversionados, gerados uma chamadaimage_gen por comandante. StreamingAssets/CardArt/manifest.json:29standard + iluminuraElo anterior. CardAppearanceUI usa standard como fallback para estilos sem arte própria; não alterafoil,raridade ou regras.
- Resources/CommanderModels:29 prefabs, cinco malhas compartilhadas, materiais comGPUinstancing e145 arquivos.anim. Preservar .meta e IDs. Malhas de revolução32 × 20 e placas de seis faces com bordas suaves. Materiais de cor, sem pacote de texturasPBR; miniaturas articuladas porTransform, sem malha deformávelSkinnedMesh/FBX.
- CommanderModels constrói geometria de fallback; CommanderPresentation carrega prefab por ID e reaplicaSeatEmblem na cor do jogador. MedievalVisuals.Figure encaminha apenas comandantes. Demais tipos mantidos.
- FigureMotion emPiecePersonality:cinco estados, poses absolutas/determinísticas e hierarquiaBody/Head/Arms/Legs/Cape/Tail, comRider e pernas quadrúpedes. Runtime usa este controlador; clipes.anim exportados são alternativas para edição/reutilização, não executados simultaneamente.
- TableWorld usa eventosmove/attack/death eOver/WinningTeam. Caminhada acompanha a rota e orientação; golpes avançam para frente sem saltar vários tiles. Morte conserva uma figura temporária1,15s, com collider desligado; retorno/exílio não dispara morte. Morte antes deSync produz ghost semPicking/ocupação. Vencedores vivos comemoram uma vez; corpos não comemoram. Quadrúpedes tombam para o lado e levantam patas dianteiras ao comemorar.
- CommanderAssetSetup gera prefabs/clipes/previews eGaleriaComandantes.unity. ProjectEntry aceita a galeria somente quando solicitada explicitamente na sessão doEditor; entrada normal continuaMesa. Nenhuma alteração deprotocolo,Match,PA,vida,comandos ou economia.

## Extensão e produção
Manter IDestável; adicionar modelo emResources/CommanderModels/<ID>-v1.prefab e arte na pastaCardArt/Commanders, com entrada por estilo no manifest. Bone names são contrato; substituir o modelo preservando hierarquia ou atualizar vínculos. Não executar oController procedural eAnimator ao mesmo tempo. TCG/Arte/Gerar modelos e animações regenerará assetsv1, preservandoGUIDs existentes; para arte final editada manualmente, criar uma versão nova antes de regenerar.

Prompts, fontes, SHA256/dimensões e prévias nativas emDocumentacao/Arte/Comandantes-20261014; índiceIndice.md. Geração embutidaimage_gen, nenhumaCLI/APIkey usada. As imagens estão no projeto e as originais do gerador foram mantidas.

## Verificação e limites
Build completo40.522checks passou. Após refinamentos somente de apresentação, BuildPresentation compilou e verificou969checks, entrada da galeria e novoexecutável. Runtime: 69 checks deimagens/modelos/eventos projetados e13dosmenus,zero erros. Galeria percorreu29×5estados no PlayModeEditor e restaurouentradaMesa. Capturas nativas de modelos eposes foram inspecionadas; não confundir com cliques manuais nem aprovação doautor. Perfil pessoal permaneceu comhash idêntico. Relay epartida completa natural não repetidos; performance/FPS em sessão longa ainda pendentes. Avisos antigos deComputeBuffer ao encerrar continuam registrados.
