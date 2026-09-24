# Comandantes e veículos — implementação de 2026-09-23

## Resultado

Sete comandantes bicolores MED-225–231 habilitados no projeto principal C:\Users\gil\TCG. Regras confirmadas em [[Decisoes Confirmadas - Comandantes Bicolores 2026-09-23]]. Compilação/build, 710 verificações e diagnóstico integrado passaram. Capturas da interface inspecionadas. Ver [[Validacao dos Comandantes e Veiculos 2026-09-23]].

## Como experimentar

Na tela Jogar, selecionar decks de demonstração e alternar a opção até **Testar comandantes**. Cada assento escolhe seu comandante; o botão Ver abre a ficha. Os decks temporários usam cartas e terrenos compatíveis com a identidade de cores e aceitam repetições para testes. Não acrescentam cartas, moedas ou decks ao perfil. Custos, fases e zona de comando continuam normais.

A zona do comandante exibe a mecânica atual: devoção de Elo, contadores de Lissandra, passiva do Receptáculo, destino do Pescador e lembretes dos demais.

## Veículos e Valeria

- Embarque/desembarque: 1 PA da tropa, mesmo tile, comandados pelo painel da peça. Só embarcam tropas do mesmo controlador; vagas são conferidas no motor.
- Veículos precisam do número de tripulantes declarado nos dados para mover/atacar. Passageiros comuns podem atacar/ativar habilidades; não podem caminhar separadamente sem desembarcar. Meka impede ações de ataque/habilidade dos passageiros, preservando alvos e a ação de desembarque.
- Transporte desloca os passageiros e seus equipamentos sem consumir movimento/PA adicional deles. A interface os identifica e a apresentação os reduz e posiciona junto do veículo.
- Destruir o veículo mata seus passageiros. Respostas à morte e regras de comandante continuam passando pelos fluxos existentes.
- Cada Valeria sua em campo adiciona uma vaga aos seus veículos. Os veículos herdam a união das palavras-chave explícitas dos passageiros. Ao sair o passageiro ou a Valeria, a herança é recalculada. Perder a vaga extra bloqueia novos embarques quando cheio; não remove automaticamente passageiros já embarcados.
- Palavras-chave são dados explícitos em `keywords`; habilidades de texto e subtipos não são copiadas como palavras-chave. Todas as chaves declaradas e suportadas pelo motor entram na herança, sem lista especial da Valeria.

## Conteúdo provisório

Pacote `test-vehicles`: Carroça de Treino (2 vagas, tripulação 1), Meka de Treino (1 vaga, tripulação 1) e Batedora Alada de Treino (Voar). Atributos e custos são fixtures de teste, não novas cartas medievais aprovadas. IDs `test-vehicle-*` preservam os IDs editoriais MED. Modelos procedurais provisórios de carroça e Meka.

## Arquitetura e extensão

- `Core/Vehicles.cs`: vínculo CarrierId, validação de comandos, transporte, capacidade e palavras-chave dinâmicas.
- `Core/Content.cs`: vehicleSeats, vehicleCrew, keywords; validação do catálogo. Veículos são Artefatos, Meka é subtipo explícito.
- `Core/BicolorCommanders.cs`, Match.cs e MedievalRules.cs: sete regras e integração de movimento, dano e morte. Ataque no instante de morte em combate é preservado antes de retirar vítimas simultâneas.
- `Runtime/ContentLoader.cs`, CommanderDemoUI.cs e HubUI.cs: geração/seleção da demonstração.
- `Runtime/CommanderUI.cs`, TableView.cs: estado da mecânica e comandos de embarque.
- `Runtime/AuthorModels.cs`, TableWorld.cs: modelos provisórios e posição dos passageiros.
- `Editor/BicolorChecks.cs`, `Runtime/CommanderDiagnostics.cs`: regras e cenário integrado `-tcg-commanders-verify`.

Caminhos relativos a Assets/TCG/Foundation. Expansões em Assets/StreamingAssets/Expansions. Perfil continua schemaVersion 2; não há migração da coleção. Rede permanece ausente.

