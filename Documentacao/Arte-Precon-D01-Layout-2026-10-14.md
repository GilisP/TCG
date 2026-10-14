# Artes exclusivas do precon Roubo de tumbas e layout

ARTE-D01-014, pedido e escolha do autor em 2026-10-14. Começar artes pintadas exclusivas pelo precon COL001-D01: 100 cartas principais e 50 terrenos. Arte do comandante MED-242 preservada. As demais listas seguem com retratos provisórios; não declarar catálogo inteiro pintado.

## Produção
Skill imagegen, ferramenta embutida, uma geração por ID; sem atlas ou API/CLI. Três especialistas com escopos separados (50 itens cada) em Documentacao/Arte/Precon-D01-20261014/batch-1/2/3.json. Fila completa queue.json; prompts e resultados por carta; batch-result mantém retomada. Originais gerados copiados para Assets/TCG/Resources/CardArt/PreconD01/<ID>-painted-v1.png. Cada cartão é representado pelo próprio nome/tipo/subtipos/efeito aprovado; arte não muda regras.

## Integração
CardArt/manifest.json associa padrão por ID. Variantes próprias anteriores preservadas; estilo sem imagem específica usa arte padrão. PaintedCardImport configura textura sRGB, clamp, sem mipmaps, max1024, compressão alta. CardAppearanceUI é a única frente para mão, coleção, deck, comandante, booster e consulta de terrenos. Efeitos foil ficam dentro da área da ilustração.

## Layout de mana
A gema única com total foi substituída por faixa abaixo do nome com quantidade separada do símbolo de cada cor e genéricos explícitos. S Sol, L Lua, G Água, F Fogo, A Ar, T Terra, N incolor. Não alterar ColoredCost ou Match.CanPay/Pay: cobrança já auditada. Nome ganhou largura; ilustração não é coberta por mana; rodapé ficou livre da borda e não mostra ID técnico. Em miniaturas é necessário ampliar/abrir ficha para ler detalhes.

## Estado e verificação
Produção concluída e integrada. CardArtLayoutPreview gera captura real de Editor Play em tamanho completo, mão e miniatura, com exemplos de duas/três cores. Primeira revisão levou a separar números das gemas. PreconArtChecks verifica151 imagens (150 novas+comandante), bindings únicos/dimensões/import, preservação de Elo iluminado e auditagem de custos. Só concluir integração após completar e validar assets. Avaliação artística do autor fica aberta.

## Entrega verificada — 2026-10-14
150 artes exclusivas integradas (100 principais + 50 terrenos), com MED-242 preservado: 151 cartas com imagem no precon. Nove revisões selecionadas v2; originais arquivados em Revisoes. Arquivos selecionados têm hashes distintos, CRC PNG íntegro e associação única no manifest. Demais precons continuam com artes provisórias, salvo comandantes/artes anteriores.

Unity 6000.3.6f1, projeto principal: BuildCardArt terminou com código 0; 754 verificações de artes, 346 de custos em 57 cartas multicoloridas, 969 de apresentação de comandantes (2.069). Executável novo: diagnóstico de interface passou 13 verificações, catálogo ativo 1.250, zero erros de execução; perfil isolado. Captura nativa de Editor Play em Layouts/preview.png confirma frente, mão, miniatura, seis cores, genéricos, foil e variante nocturne. Não houve mudança de regras, protocolo ou economia; rede não foi repetida nesta entrega. Avaliação artística do autor e outros nove precons permanecem pendentes.

Código: CardAppearanceUI.cs, PaintedCardImport.cs, PreconArtChecks.cs, CardArtLayoutPreview.cs e FoundationSetup.BuildCardArt. Assets e prompts: Documentacao/Arte/Precon-D01-20261014/Indice.md. Evidências: Logs/card-art-build-20261014.log, Logs/card-art-ui-20261014.log, Logs/CardArtLayoutVerification/result.txt e assets-verification.json.
