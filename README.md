# Tower Defense 3D

Projeto de tower defense em Unity com campanha de 5 niveis, selecao de dificuldade, economia, construcao de torres, progressao por estrelas e UI criada em runtime.

## Visao Geral

O jogo combina um menu principal de campanha com um Level 1 personalizado em `SampleScene` e quatro niveis adicionais. O objetivo e defender a base, sobreviver as waves, gerir moedas, colocar torres nos slots certos e adaptar a estrategia ao tipo de inimigo e a dificuldade escolhida.

Depois da reorganizacao do projeto, todo o conteudo jogavel ficou centralizado em `Assets/Game`, o que torna mais simples perceber onde estao as cenas, os scripts principais, os prefabs, os modelos e os dados da campanha.

## O Que Da Para Fazer

### Menu principal e campanha

- iniciar o jogo a partir do `MainMenu`
- escolher entre `Easy`, `Medium`, `Hard` e `Epic`
- selecionar qualquer nivel desbloqueado entre `SampleScene`, `Level2`, `Level3`, `Level4` e `Level5`
- ajustar o volume geral no menu
- sair do jogo a partir do menu
- desbloquear o nivel seguinte ao obter pelo menos 1 estrela no nivel anterior
- repetir niveis para melhorar a classificacao final
- guardar dificuldade, volume e estrelas entre sessoes com `PlayerPrefs`

### Durante a partida

- defender a base contra waves de inimigos
- enfrentar inimigos terrestres e aereos
- receber moedas por cada inimigo destruido
- acompanhar a moeda atual num HUD no ecra
- arrastar torres da barra inferior para slots validos
- cancelar a colocacao da torre com `Esc`
- impedir construcao quando o slot esta ocupado ou quando nao ha moedas suficientes
- usar targeting por tipo de inimigo com `GroundOnly`, `AirOnly` e `All`
- terminar o nivel com ecra de vitoria ou derrota
- reiniciar o nivel, voltar ao menu ou avancar para o proximo nivel no fim da partida

### Torres e gestao

- construir torres com custos diferentes
- usar tres arquetipos base no `SampleScene`: `MachineGun_All`, `Laser_AirOnly` e `Rocket_GroundOnly`
- combinar torres para cobrir alvos terrestres, aereos ou ambos
- gerir a economia para decidir entre construir cedo, guardar moedas ou reforcar a defesa
- nos niveis da campanha com HUD de torre, selecionar torres colocadas para ver informacao, fazer upgrade e vender

### Waves e progressao

- jogar um Level 1 com 2 waves configuradas por codigo
- jogar os niveis seguintes com as waves definidas nas proprias cenas
- ganhar entre 1 e 3 estrelas com base na vida restante da base
- manter sempre a melhor pontuacao por nivel
- avancar na campanha nivel a nivel

## Dificuldades

As dificuldades alteram diretamente a economia e o comportamento do jogo:

- `Easy`: mais moedas iniciais, mais recompensas, mais vida na base e inimigos menos exigentes
- `Medium`: equilibrio base do projeto
- `Hard`: menos margem de erro, menos recursos e inimigos mais fortes
- `Epic`: menos moedas, mais pressao, waves mais agressivas e selecao de torres mais limitada

O sistema de dificuldade afeta:

- vida dos inimigos
- velocidade dos inimigos
- quantidade de inimigos por wave
- intervalo entre spawns
- moedas iniciais
- recompensa por eliminacao
- vida da base
- disponibilidade de torres em certos niveis

## Como Jogar

1. Abrir o projeto na Unity e entrar em `Play` a partir do `MainMenu`.
2. Escolher a dificuldade.
3. Selecionar um nivel desbloqueado.
4. Arrastar uma torre da barra inferior para um slot valido.
5. Defender a base e ganhar moedas ao eliminar inimigos.
6. Reforcar a defesa com novas torres e, quando disponivel, fazer upgrade ou vender.
7. Terminar o nivel e usar o ecra final para repetir, regressar ao menu ou avancar.

## Controlos

- `Mouse esquerdo`: interagir com menus, escolher nivel, arrastar torres e selecionar elementos da HUD
- `Esc`: cancelar a colocacao de uma torre em drag

## Requisitos

- Unity `6000.3.10f1`
- Universal Render Pipeline
- Input System

Versao do Unity usada no projeto:

```text
6000.3.10f1
```

## Como Abrir

1. Abrir o projeto na Unity Hub com a versao `6000.3.10f1`.
2. Esperar pelo import inicial de packages e assets.
3. O projeto esta organizado dentro de `Assets/Game`.
4. O script `Assets/Game/ProjectScripts/Editor/ProjectStartupScene.cs` tenta abrir automaticamente o menu principal uma vez por sessao no editor.

Se for preciso abrir manualmente:

- menu principal: `Assets/Game/Scenes/MainMenu.unity`
- Level 1: `Assets/Game/ProjectScenes/SampleScene.unity`
- restantes niveis: `Assets/Game/Scenes/Levels/`

## Estrutura Principal

### Organizacao geral

- `Assets/Game/`
  contem todo o conteudo principal do jogo num unico bloco

- `Assets/Game/ProjectScripts/`
  contem a logica personalizada deste projeto, incluindo bootstrap do Level 1, dificuldade, progressao, HUD simples e overlay de campanha

- `Assets/Game/Scripts/`
  contem os sistemas de suporte usados pelos restantes niveis e pela infraestrutura do jogo, como UI, waves, torres, agentes, camera, input e dados base

- `Assets/Game/ProjectScenes/`
  contem a cena personalizada usada como primeiro nivel da campanha

- `Assets/Game/Scenes/`
  contem o menu principal e os niveis seguintes da campanha

- `Assets/Game/Data/`
  contem configuracoes de agentes, torres, bibliotecas de torres, alinhamentos e listas de niveis

- `Assets/Game/Prefabs/`, `Assets/Game/Models/`, `Assets/Game/Materials/`, `Assets/Game/Audio/` e `Assets/Game/UI/`
  concentram os assets visuais, audio e prefabs usados pelo jogo

### Scripts mais importantes

- `Assets/Game/ProjectScripts/SampleSceneBootstrap.cs`  
  Configura o `SampleScene`, cria o fluxo do Level 1, prepara inimigos, waves, moeda, construcao e fim de nivel.

- `Assets/Game/ProjectScripts/SimpleBuildManager.cs`  
  Gere slots, barra inferior, drag-and-drop, ghost de colocacao e validacao da compra.

- `Assets/Game/ProjectScripts/SimpleCurrencyManager.cs`  
  Gere a moeda do jogador e os eventos de atualizacao.

- `Assets/Game/ProjectScripts/SimpleCurrencyHUD.cs`  
  Desenha o HUD de moedas em runtime.

- `Assets/Game/ProjectScripts/Level1DifficultySettings.cs`  
  Define as dificuldades, as regras de campanha e as cenas associadas a cada nivel.

- `Assets/Game/ProjectScripts/CampaignLevelBootstrap.cs`  
  Aplica a dificuldade aos niveis seguintes da campanha, incluindo currency, waves, base e biblioteca de torres.

- `Assets/Game/ProjectScripts/CampaignProgression.cs`  
  Gere estrelas, desbloqueios e persistencia da campanha.

- `Assets/Game/ProjectScripts/SimpleCampaignEndGameUI.cs`  
  Mostra o ecra final de vitoria ou derrota com estrelas e navegacao.

- `Assets/Game/ProjectScripts/Level1MainMenuOverlay.cs`  
  Implementa o overlay de campanha no menu com dificuldade, selecao de nivel e volume.

### Cenas e conteudo

- `Assets/Game/ProjectScenes/`  
  Contem a cena personalizada usada como Level 1.

- `Assets/Game/Scenes/`  
  Contem o menu principal e os restantes niveis da campanha.

- `Assets/Game/Data/`  
  Contem dados configuraveis da campanha, inimigos e torres.

- `Assets/Game/Prefabs/`  
  Contem prefabs de torres, inimigos, UI, audio e outros elementos reutilizaveis.

- `Assets/Game/Models/` e `Assets/Game/Materials/`  
  Contem os modelos 3D, texturas e materiais usados nas cenas e nos prefabs.

- `Assets/Game/Scripts/`  
  Contem componentes adicionais do jogo, incluindo HUD de torre, waves e menus usados nos restantes niveis.

## Persistencia

O projeto guarda automaticamente:

- dificuldade selecionada
- volume geral
- estrelas obtidas por nivel
- melhor resultado ja alcancado em cada nivel

## Estado Atual

- campanha com 5 niveis
- menu com selecao de dificuldade, niveis e volume
- Level 1 personalizado com sistema proprio de construcao
- progressao por estrelas e desbloqueio de niveis
- estrutura do projeto centralizada em `Assets/Game`
- README atualizado com funcionalidades principais e organizacao atual
