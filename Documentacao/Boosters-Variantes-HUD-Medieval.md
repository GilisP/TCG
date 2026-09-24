# Boosters, variantes e HUD medieval
Atualização de 2026-09-23. Projeto principal: C:\Users\gil\TCG.

## Funcionamento
Loja → Boosters vende Crônicas dos Reinos. Valores provisórios aprovados pelo autor: cinco sorteios por 100 moedas, retorno de 10 por repetida. A tela mostra as chances. A adição gratuita individual de cartas continua disponível.

A configuração fica em Assets/StreamingAssets/Economy/booster-medieval.json. Neste preset, cada carta elegível tem chance igual, independentemente da raridade. Cada espaço sorteia separadamente uma carta e o estilo: Clássica 80%, Iluminura 15%, Nocturna 5%. Esses pesos são parâmetros de teste, não economia definitiva aprovada. Só entram as três expansões medievais configuradas e cartas jogáveis; conteúdo bloqueado e veículos de demonstração ficam fora.

Uma variante nova libera também a carta-base. Repetição significa já possuir aquela combinação carta/estilo; uma arte diferente de carta possuída não é repetida. Repetidas dentro da mesma abertura também devolvem moedas. Abertura pode repetir cartas. Não existe garantia de raridade neste preset.

O sorteio, saldo, inventário, variantes e recibo são salvos juntos antes da revelação. Falha de escrita desfaz a compra em memória. Última abertura apenas mostra o recibo persistido, sem novo sorteio. O inventário é local; não constitui uma economia protegida contra edição de arquivos ou serviço multiplayer.

## Cartas e apresentação
Um renderer compartilhado desenha frente 2:3 com moldura, nome, custo, ilustração, tipo/subtipos, regras, atributos e identificação. Coleção, loja, mão, inspeção e abertura usam essa apresentação. Texto longo pode ser reduzido na miniatura; a ficha mantém o texto completo.

O HUD usa couro escuro, pergaminho, contornos de metal envelhecido, ornamentos e títulos serifados. As texturas leves de acabamento são procedurais. A mão mantém leque, destaque e prioridade de clique da carta elevada.

Abra uma carta para pré-visualizar os três estilos. Uma variante possuída pode ser aplicada como padrão da carta na coleção ou ao deck em edição. O deck precisa conter a carta para equipá-la. Versos continuam independentes.

Todas as cartas aceitam imagens por estilo. **Somente Elo Proibido da Realeza recebeu uma nova ilustração pintada nesta entrega**, na variante Iluminura; as demais usam retratos procedurais provisórios e molduras alternativas. Infraestrutura não significa arte final produzida para todo o catálogo.

## Acrescentar artes
Coloque a imagem em Assets/TCG/Resources/CardArt e acrescente uma entrada em Assets/StreamingAssets/CardArt/manifest.json:
~~~json
{"cardId":"MED-225","styleId":"illuminated","resourcePath":"CardArt/elo-iluminura-v1"}
~~~
O caminho Resources não leva extensão. Cada combinação carta/estilo pode usar imagem própria; sem entrada utiliza o retrato procedural. Preserve o ID da carta e os arquivos .meta. A arte não altera custo, habilidades, limites de deck nem identidade.

## Arquitetura e persistência
Core/Boosters.cs contém configuração, sorteio com Random injetável, recibo e propriedade de variantes, sem Unity. CollectionLibrary.cs separa definições das aparências da coleção e dos decks. Runtime/CollectionStore.cs mantém gravação atômica e backup. BoosterUI.cs coordena compra/revelação; CardAppearanceUI.cs resolve artes, seleciona estilo e desenha cartas; MedievalSkin.cs fornece acabamentos compartilhados.

CollectionData agora usa schemaVersion 3 no mesmo arquivo collection-v1.json. Migrações preservam cartas, decks, versos e saldo do esquema 2, sem conceder novamente as 500 moedas. Schema 1 conserva sua migração anterior. variantes e aparências são listas separadas; recibo da última abertura acompanha o perfil. IDs ausentes temporariamente não devem ser descartados.

## Arte gerada
Ferramenta nativa de geração de imagens, sem CLI. Asset final:
C:\Users\gil\TCG\Assets\TCG\Resources\CardArt\elo-iluminura-v1.png

Prompt exato:
> Create a premium medieval fantasy trading card illustration, portrait 2:3 composition, NO text NO letters NO borders NO card frame. Subject: the Forbidden Royal Bond, two adult royal sovereigns standing together in a gothic castle, one wearing ivory and muted golden solar heraldry and one wearing dark amethyst and silver lunar heraldry, hands touching at a delicate eclipse-shaped jewel, dignified and emotionally compelling. Rich painterly oil and illuminated manuscript influence, realistic faces, elegantly detailed embroidered costumes, dramatic candlelight, stone arches and distant dusk. Centered waist-up composition with safe margins for trading card crop. Beautiful professional collectible card art, nuanced brushwork and deep burgundy, aged gold, midnight violet palette. Intended as an interchangeable illustration asset in a Unity medieval TCG.

## Verificação e limites
Ver [[Validacao de Boosters e HUD Medieval 2026-09-23]]. Balanceamento, aprovação estética, fonte recorrente de moedas, artes restantes e sessão manual prolongada permanecem em [[04 - Pendencias de Implementacao]].
