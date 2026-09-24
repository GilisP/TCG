> Anotação do projeto Unity, registrada em 2026-09-14. Fonte de design: [nota atual no Obsidian](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Colecoes/00 - Colecoes.md>). Este espelho não é implementação; em caso de atualização, consultar a fonte e sincronizar a nota.

**Atualização 2026-09-15:** há uma nova [base jogável local 2–4](Base-Jogavel.md), com mesa 3D real e expansões por dados. As orientações anteriores abaixo são contexto histórico; consulte a base e a Rodada 2 para o estado atual.

# Coleções — Oficina de Cartas

[Painel](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/00 - Projeto/00 - Indice Mestre.md>) · [Conteúdo](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Indice de Conteudo.md>) · [Tipos](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/03 - Cartas/Indice de Cartas.md>) · [Decisões sobre coleções](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/10 - Decisoes/Perguntas/Perguntas 06 - Colecoes e Cartas.md>).

Aqui você cria e organiza cartas por coleção/expansão. Esta organização editorial não define posse de cartas pelo jogador nem quais coleções são legais nos decks.

## Começar a criar

1. Abra [Colecao COL-001 - Primeira Colecao](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Colecoes/COL-001 - Primeira Colecao/Colecao COL-001 - Primeira Colecao.md>) e preencha nome, tema e objetivos.
2. Abra [COL-001-001 - Carta em Criacao](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Colecoes/COL-001 - Primeira Colecao/Cartas/COL-001-001 - Carta em Criacao.md>) e escreva sua primeira carta. Ela começa como rascunho, sem mecânica ou atributos inventados.
3. Para adicionar outra carta, duplique [Modelo - Carta de Colecao](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Colecoes/Modelos/Modelo - Carta de Colecao.md>) para a pasta `Cartas` da coleção, dê um ID novo e inclua o link na lista da coleção.
4. Para outra coleção, crie uma pasta `COL-002 - Nome`, duplique [Modelo - Colecao](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Colecoes/Modelos/Modelo - Colecao.md>) e adicione-a ao registro abaixo.

## Coleções em criação

| ID | Coleção | Estado | Observação |
|---|---|---|---|
| COL-001 | [Colecao COL-001 - Primeira Colecao](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Colecoes/COL-001 - Primeira Colecao/Colecao COL-001 - Primeira Colecao.md>) | Em definição | Nome e tema aguardando você; uma ficha vazia disponível |

## Modelos

[Modelo - Colecao](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Colecoes/Modelos/Modelo - Colecao.md>) · [Modelo - Carta de Colecao](<C:/Users/gil/OneDrive/Área de Trabalho/TCG/06 - Conteudo/Colecoes/Modelos/Modelo - Carta de Colecao.md>). São Markdown comuns: não exigem plugin adicional. Duplicar o modelo preserva os campos; depois trocar título, ID e vínculo da coleção.

## Organização de arquivos

Cada coleção contém sua nota principal e uma pasta `Cartas`. Futuras imagens podem ficar em `Artes` dentro da coleção, sempre ligadas à carta que as usa. Modelos ficam separados em `Modelos` para não se misturarem às cartas reais.

Uma carta deve ter ID editorial único no vault. Ao renomear a carta, preservar esse ID. Não assumir que uma reimpressão permite ignorar singleton; isso será decidido em K-04. O ID editorial não é automaticamente o ID técnico `custom-...` usado pelo catálogo Unity atual.

## Estados de trabalho

| Estado | Significado |
|---|---|
| Rascunho | Ideia em preenchimento; não aprovada |
| Em revisão | Texto, custo, interações e missão sendo avaliados |
| Design aprovado | Autor aprovou a carta; implementação pode ainda faltar |
| Implementada | Dados/efeitos existentes no jogo, com referência ao asset |
| Verificada | Critérios e testes registrados no Unity/partida |

Arte tem status separado: ausente, provisória, em revisão ou definitiva. Uma pintura pronta não torna a carta implementada ou balanceada.

## Ligação com o Unity

O motor atual tem catálogo, oficina e pacotes de decks, mas `CardDefinition` não possui coleção nos campos inspecionados. Esta pasta não é importada automaticamente. Quando a integração for implementada, preservar IDs, definir mapeamento/versionamento e registrar o asset na ficha. Não copiar os Markdown para Resources como se fossem JSON de cartas.

Antes de reutilizar as 257 definições declaradas pela versão experimental, responder K-07. Nenhuma carta antiga foi importada ou reclassificada automaticamente como coleção aprovada.

