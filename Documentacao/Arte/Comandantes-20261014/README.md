# Ilustrações individuais dos comandantes — 2026-10-14

29 ilustrações geradas com a ferramenta integrada `image_gen`, uma chamada por carta, em grupos de até três chamadas independentes. Nenhum mosaico ou recorte manual foi usado como asset. Os PNG completos versionados ficam em `Assets/TCG/Resources/CardArt/Commanders/<ID>-portrait-v1.png`.

Cada JSON por ID preserva o prompt completo, origem retornada pela ferramenta, caminho final e inspeção. `assets-verification.json` contém dimensões, bytes e SHA-256. A fonte dos 29 IDs é `Documentacao/Desenvolvimento/commander-art-source-20261014.json`; os perfis de identidade estão em `commander-visual-profiles-20261014.json`.

O manifest tem 29 entradas `standard`; a entrada anterior MED-225 `illuminated` e seu asset foram preservados. Outros estilos usam o fallback standard já existente em CardAppearanceUI. A arte não altera definição, raridade, economia, subtipos ou regras.

Todas as imagens foram vistas individualmente: personagem/forma reconhecível, pintura detalhada sem facetas low-poly, composição central landscape apropriada à janela de arte. Sem texto de carta, moldura, logo ou marca-d'água. A direção é uma proposta visual, ainda sujeita à avaliação do autor. Diferenças de detalhe dos primeiros perfis: MED-193 não destaca cajado; MED-195 olhos verdes; MED-229 animal sem tecido/medalhão, com filhote. Não são mudanças mecânicas.

Verificado neste pacote: 29 IDs cobertos, 29 PNG distintos por SHA-256, 30 entradas no manifest contando a iluminura preservada. Importação, mipmaps/memória, exibição nos estilos e recorte final no Unity ficam com a integração do coordenador; não foram validados pelo agente de arte.
