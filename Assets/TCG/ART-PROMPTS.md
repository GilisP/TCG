# Artes da expansão Confluências

Modo utilizado: ferramenta integrada `image_gen`, sem CLI ou chave de API do usuário. Imagens originais de fantasia medieval, criadas para o projeto. Inspiração solicitada: atmosfera de ilustrações de TCG como MTG, com direção própria e mundo consistente. Não foram usados scans de cartas existentes.

Todas as seis imagens são PNGs de 1536 × 1024. A primeira é a imagem de referência e fundo do menu. As outras cinco foram geradas separadamente usando a primeira como referência de estilo e mundo, não como alvo de edição. Os textos, custos, molduras e atributos das cartas são desenhados pelo Unity.

Destino no projeto: `C:\Users\gil\TCG\Assets\TCG\Resources\TCGArt`.

## realm-reference.png

Use case: stylized-concept. Create an original panoramic painted fantasy illustration for a medieval trading card game named Fronteiras. Asset: visual reference and main menu background, landscape 1536x1024. Scene: an ancient ornate stone citadel above a confluence of a turquoise river and a glowing golden sunlit forest, distant purple lunar mountains, a cloaked knight in foreground looking across the realm. Painterly epic high fantasy trading-card illustration aesthetic, richly textured traditional oil painting, atmospheric depth, believable intricate medieval architecture, dramatic chiaroscuro, jewel tones and restrained gold. Strong central composition that can also be used as card art. No text, no card frame, no logos, no watermark. Original world and characters.

## guardian.png

Use case: stylized-concept. Reference image role: visual style and world reference, not an edit target. Create a NEW landscape 1536x1024 painted card illustration, same original medieval fantasy world and oil painting detail: a solemn female knight of the dawn in engraved antique gold and steel armor, emerald cloak, holding a luminous spear, standing beside an immense guardian stag with branching antlers of living oak. Sun rays through turquoise forest mist, ancient citadel arches in background. Clear central figures, readable at small card size, rich tactile paint, heroic epic fantasy trading card illustration. No text, card frame, logos or watermark.

## moon-mage.png

Use case: stylized-concept. The input is a STYLE/WORLD REFERENCE only. New original landscape 1536x1024 fantasy trading card illustration: an elderly lunar tide mage with dark silver embroidered robes stands on a stone bridge over a turquoise waterfall at night, drawing luminous blue runes into a swirling orb of water, full moon and violet ruined cathedral in distance. Close enough for expressive face and hand detail, balanced dramatic central subject, richly textured painterly oil illustration, believable anatomy, restrained gold details, same ornate medieval world as reference. No text, no border, no logo or watermark.

## dragon.png

Use case: stylized-concept. Input reference supplies original world and painted style only. NEW landscape 1536x1024 card illustration of an enormous copper-scaled dragon on a volcanic ridge, spread wings catching firelight and storm lightning, distant medieval citadel beneath red and purple clouds. Centered majestic dragon with readable silhouette, intricate scales and wing anatomy, lava illuminating rock, traditional detailed oil painting, dramatic fantasy trading card art, same sophisticated medieval atmosphere as reference. No humans needed. No text, no frame, no logos, no watermark.

## arcane-clash.png

Use case: stylized-concept. Reference is a style/world reference only. Create NEW landscape 1536x1024 art for a fantasy spell card: a spectacular collision of a golden radiant protective sigil and a spiraling torrent of violet-blue arcane energy above a medieval stone courtyard; tiny silhouettes of knights sheltered behind the golden barrier give scale. Focus on the beautiful magical collision, flowing ribbons of light, floating stone fragments, painterly details, high contrast central readable focal point, ancient citadel and mist beyond, rich traditional oil painting fantasy trading card aesthetic matching reference. No text, no legible symbols or letters, no border, logo or watermark.

## confluence.png

Use case: stylized-concept. Input is a style and world reference only. NEW landscape 1536x1024 fantasy terrain card painting: an ancient arched stone bridge over the meeting point of two rivers, one moonlit violet blue from a snowy mountain valley, the other warm emerald gold from an autumn forest. A ruined circular temple at the confluence, mystical dawn and moon visible, exquisite landscape detail, no large foreground character. Rich atmospheric oil painted medieval fantasy trading card illustration, ornate ruins, clear readable scenery. Original scene. No text, frame, logos, watermark.

## Integração

`GeneratedArt` carrega as imagens por identificador em `Resources/TCGArt`. `BoardArt` usa a ilustração quando há um identificador, mantendo a arte procedural como alternativa para cartas anteriores. A Oficina permite escolher as seis imagens disponíveis. As ilustrações são compartilhadas entre cartas por tema: não são 43 pinturas distintas. O pacote de deck preserva o identificador de arte; a instalação da versão 0.6 já contém esses seis recursos.

As imagens geradas foram inspecionadas nesta conversa. O carregamento das texturas é verificado nos testes Unity. Isso não equivale a uma conferência completa da interface renderizada em Play.
