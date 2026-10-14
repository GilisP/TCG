# D02 — Arquivo das marés

142 novas artes +8 compartilhadas já pintadas +comandante preservado. Produção concluída e verificada. Fila/prompts/versões em Documentacao/Arte/Precon-D02-20261014.

## Arquitetura dos efeitos implementados
SpellPresentation.TryPlan usa exclusivamente eventos públicos existentes: cast/trigger na origem, impact/resolve no alvo e stack-canceled na origem. Apenas Spell/Instant entram no novo efeito. Permanentes e foil mantêm a apresentação anterior; stack-resolved não repete o impacto.

Famílias visuais por cor e operações públicas (Snapshot NetworkCards.Pack): radiância, vazio, maré, chama, vento, crescimento e arcano. Compra/seleção de topo, controle, proteção, devolução e cura recebem movimentos próprios sem interpretar o texto nem aplicar regras. Cores múltiplas alternam nos filamentos e centelhas.

Cada efeito tem21linhas,24 centelhas billboard com shader radial e uma luz sem sombra; limite24instâncias, duração0,75–1,6 s, limpeza na troca de partida e destruição de materiais. Assets/code: Runtime/SpellPresentation.cs, MedievalVisuals.SpellVisual, Resources/SpellMote.shader; diagnósticos SpellPresentationChecks, SpellVisualPreview, SpellVisualDiagnostics (-tcg-spell-vfx-verify). Shader em Resources para ser incluído no build.

Verificações iniciais durante a produção: compilação e112checks (incluindo74magias D02); Play nativo8579checks (eventos projetados, origem/alvo, posições finitas, limite/expiração/limpeza), três capturas de fases inspecionadas. Fixture visual não representa uma partida completa em rede. Build final e diagnóstico do executável aguardam completar142artes. Desempenho prolongado/aprovação artística do autor ainda pendentes.

## Entrega integrada e verificada — 2026-10-14
142 artes novas individuais, oito pinturas compartilhadas do D01 e comandante MED-193 preservado:151 cartas com imagem no Arquivo das marés. Onze versões finais v2; originais em Revisoes. Auditoria SHA256/CRC passou, nenhum arquivo selecionado é duplicata do D01, nove bindings anteriores mantidos. Imagens originais1448×1086; importação sRGB/max1024/compressão/clamp configurada automaticamente para pastas PreconD. Manifest aplica standard porID; variantes sem ilustração própria usam esta base, sem modificar skins/foil/inventário. Outros oito precons ainda aguardam seus lotes exclusivos.

Frentes completas/mão/miniaturas conferidas em Editor Play. Custos mostram símbolos/quantidades e genéricos; painel do comandante agora mostra custo base por cor e adicional separado, confirmado com +4 genéricos em fixture de duas conjurações anteriores. Cobrança já estava correta e não foi alterada.

BuildCardArt no Unity 6000.3.6f1 principal terminou código0:754 checks D01+738 D02+346 custos(57 multicoloridas)+113 VFX(inclui74magiasD02)+969 comandantes=2.920. Play nativo:8.595verificações de eventos projetados, origem/alvo, geometria finita, limite/expiração/rebind, cinco motivos reaisD02, partículas/material/colliders; capturas inspecionadas. Executável novo:22 checksVFX(5 famílias,shaders,partículas,limite/limpeza) e13UI, zero erros. Testes usam fixtures/perfis isolados; hash do perfil pessoal permanece8303FA1A105818CD4E13AD0F7980FB9D980EE3670668454CA7E3BCF0238FC175.

Evidências: Logs/card-art-d02-build-20261014.log, spell-vfx-d02-final-20261014.log, spell-vfx-player-20261014.log, card-art-d02-ui-20261014.log; Documentacao/Arte/Precon-D02-20261014/assets-verification.json, Layouts e VFX; relatório ART-VFX-D02-014-result.json. Avaliação artística/partida longa/FPS no hardware do autor pendentes. LAN/Relay não repetidos: projeção de eventos foi exercitada em fixture, protocolo e regras permanecem intactos.

Falha de compilação durante refinamento foi corrigida usando snapshot público NetworkCards.Pack para operações de habilidades; repetição e build final passaram. Preview inicial do adicional foi corrigida para configurar os dois assentos; captura final mostra +4 genéricos. Nenhuma fonte/regra foi modificada para fazer testes passarem.
