# SMT Sticky Notes 1.6.1

- Reinicia o tempo de inatividade do widget ao fechar as configurações, evitando que ele se oculte imediatamente depois de uma permanência longa no painel.
- Preserva o recolhimento por clique externo e o comportamento de fixação.
- Acrescenta regressões do retorno das configurações após 25 segundos, sem aguardar tempo real nem alterar notas pessoais.

# SMT Sticky Notes 1.6.0 — Fluent 2

- Configurações reorganizadas em Geral, Notas e Google; atalhos e versão na ajuda contextual.
- Texto em tamanho natural, sem diminuir a página inteira. O painel usa espaço acima ou ao lado do widget e respeita a área útil; telas excepcionalmente pequenas mantêm rolagem acessível.
- Busca com Ctrl+F, limpar e Escape; estado vazio específico para resultados ausentes.
- Interruptor de início automático, restauração de opacidade100%, nova área com Enter e feedback junto ao campo.
- Indicadores de negrito/itálico/lista acompanham a seleção. Paleta50cores adaptada ao monitor, com teclado e retorno de foco.
- Confirmações Google consistentes com Fluent, Cancelar como padrão seguro; callback claro/escuro e nome SMT.
- Identidade Store/bandeja, notas, formatação humana, conta Google e sincronização existentes preservados. Nenhuma migração de dados necessária.
- Testes pertencem à candidata; publicação e instalação dependem do fluxo oficial, não implícitas nesta nota.

# SMT Sticky Notes 1.5.8

- Organiza as notas automaticamente na primeira exibição de cada área a cada execução do app, inclusive ao abrir pela bandeja após iniciar com o Windows.
- Começa no canto inferior direito, empilha de baixo para cima e abre novas colunas à esquerda, mantendo o espaçamento compacto e a área útil de cada monitor.
- Usa a mesma direção no botão Auto-organizar, sem inverter a ordem ao clicar novamente.
- Mantém os ajustes manuais ao ocultar e mostrar as notas durante a mesma execução.

## SMT Sticky Notes 1.5.7

- Mantém a cor da nota no título e no editor, inclusive ao passar o mouse ou digitar, sem preenchimento escuro adicional.
- Remove fundos trazidos por textos colados ou documentos existentes, preservando texto, negrito, itálico, listas e imagens.
- Enter confirma e salva o título e leva o foco para o conteúdo da nota, sem inserir uma quebra de linha.

## SMT Sticky Notes 1.5.6

- Corrige a abertura inicial do widget e das configurações fora da tela em monitores com escala de 150%.
- Corrige a inserção de imagens dentro de listas com marcadores, seções e células de tabelas, preservando texto e formatação.
- Usa o DPI nativo da janela antes da primeira exibição, preservando a área útil e a barra de tarefas.
- Acrescenta regressões de primeira abertura em coordenadas físicas e conversão de escala, incluindo origens negativas.

## SMT Sticky Notes 1.5.5

- Nome SMT Sticky Notes no pacote do Windows, menu Iniciar, inicialização automática, bandeja, widget e configurações.
- Identidade do pacote, GUID da bandeja e armazenamento permanecem estáveis para preservar notas e atualizações.
- O empacotamento não restaura mais o nome antigo quando uma variável de compilação está desatualizada; regressão verifica os manifestos x64 e ARM64.

## Windows Sticky Notes 1.5.4

- Cada nota nova exibe sua cor original automaticamente, diferente das notas existentes, inclusive em outras áreas e após reiniciar o app.
- Notas novas usam cores reais; notas existentes preservam a aparência e o estilo salvos.
- Regressões cobrem 64 novas notas, temas claro e escuro, cores manuais e persistência.

## Versão 1.5.3

- Preparação da primeira publicação Store como Showmetech Sticky Notes, com nome reservado e identidade oficial do produto.
- Nome de exibição da Store configurável no empacotamento, sem vincular o aplicativo à conta pessoal do desenvolvedor.
- A certificação e a disponibilidade pública dependem da Microsoft; cadastro e pacote não equivalem a publicação.

## Versão 1.5.2

- Instalação preparada e verificada antes de substituir a versão ativa, com backup e reversão em falhas. Atualizações preservam a escolha de início automático.
- Atalhos e registros de terceiros são preservados; a desinstalação retira a configuração pública Google e somente o próprio histórico de bandeja, com backup.
- Reconexão Google disponível após sessão inválida ou copiada de outro usuário do Windows, sem apagar a sessão anterior nem as notas.
- Índices locais inválidos recuperam backup preservando o original; sequência de cores inválida não bloqueia a criação de notas.
- Verificações de privacidade dos pacotes e testes de instalação isolada integrados ao CI. Scripts compatíveis com perfis com espaços e acentos.

## Versão 1.5.1

- Notas sem Fixar se recolhem quando o foco sai do app para outra janela ou para a área de trabalho.
- Notas fixadas continuam visíveis. Editar outra nota, abrir menus, paletas e diálogos do app não recolhe as notas.
- Clique na bandeja continua alternando entre ocultar todas e mostrar as notas da área ativa. Documentos e posições preservados.

## Versão 1.5.0

- Estilo Fluent 2 nas notas, widget, configurações, paleta de cores e confirmação de exclusão.
- Superfícies sólidas, tipografia Segoe UI Variable, cantos de 4/8/12 DIP e estados de interação consistentes.
- Seleções usam a cor de destaque do Windows; foco de teclado visível sem reduzir o contraste do texto no hover.
- Fundos 100% opacos por padrão; preservadas as 50 cores, posições, sincronização e alternância pela bandeja.

## Versão 1.4.9

- Clique no ícone da bandeja alterna entre ocultar todas as notas visíveis e mostrar todas as notas da área ativa.
- Cliques rápidos respeitam o estado solicitado mesmo durante as animações. Conteúdo, tamanho e posição são preservados.
- Widget e configurações continuam disponíveis pelo menu do ícone, com o botão direito.

## Versão 1.4.8

- Painel de notas com ícones, busca com indicação visual, abas com seleção destacada e ações alinhadas em uma única faixa.
- Preservada a lista de cartões coloridos com uma linha de título.
- Auto-organização com intervalo visual de 10 DIP entre cartões, reduzindo também as margens transparentes das notas e ajustando suas sombras.

## Versão 1.4.7

- Botão Auto-organizar com ícone no widget, menu da bandeja e atalho Ctrl+Shift+O.
- Notas alinhadas à direita em colunas, com largura uniforme e espaçamento de 12 DIP; alturas preservadas quando couberem.
- Organização respeita cada monitor e sua área útil, preserva o conteúdo e salva a posição. Se não houver espaço, mantém o arranjo anterior sem sobrepor notas.

## Versão 1.4.6

- Atualizações guardam o backup fora da pasta de programas e retiram ícones de versões antigas somente após confirmar sua cópia de segurança.
- Instalação, atalhos e inicialização continuam usando um único caminho fixo.

## Versão 1.4.5

- Testes isolados removem somente o próprio registro de bandeja ao encerrar, evitando entradas antigas nas configurações do Windows.
- Preservado o identificador fixo da instalação principal e os registros dos demais aplicativos.

## Versão 1.4.4

- Configurações organizadas em cinco seções com ícones: Aparência, Áreas, Google, Dados e Atalhos.
- Informações essenciais, ações com ícones e páginas que se ajustam ao espaço disponível sem barra de rolagem.
- Barra de opacidade com padrão 100%, aplicação imediata e preferência salva por computador.

## Versão 1.4.3

- Barra de opacidade do fundo nas configurações de aparência, de 0% a 100%, com valor visível, ajuste imediato e padrão 100%.
- Preferência salva neste computador; instalações existentes começam em 100%. Textos e botões permanecem visíveis ao reduzir a opacidade.

## Versão 1.4.2

- Widget, configurações e superfícies do app 100% opacos por padrão nos temas claro, escuro e do sistema.
- Preservados os controles de aparência e as animações de abertura, recolhimento e destaque das notas.

## Versão 1.4.1

- Lista do widget simplificada: retângulos inteiramente na cor da nota e somente uma linha de título, com reticências para títulos longos.
- Clique em qualquer ponto do retângulo abre e destaca a nota na área de trabalho com um contorno pulsante temporário. Cliques repetidos reiniciam o destaque.
- Destaque respeita a opção de animações e o alto contraste do Windows, sem alterar posição, tamanho, conteúdo ou fixação da nota.

## Versão 1.4.0

- Sincronização opcional de notas, formatação, imagens, cores, áreas e lixeira pela mesma conta Google no desktop e notebook.
- Acesso somente à pasta privada do app no Google Drive, com login no navegador do sistema e confirmação da conta antes do primeiro envio.
- Histórico de revisões, continuidade offline e cópias de conflito para preservar edições simultâneas. Posições e tamanhos permanecem independentes por computador.
- Sessões protegidas pelo Windows; backups exportados não incluem credenciais ou cache de sincronização.
- Português corrigido na tela de retorno do login e em avisos existentes. Verificação de UTF-8 incluída no fluxo de publicação.

## Versão 1.3.2


- Notas com quatro ações visíveis no topo: configurações, nova nota, fixar e minimizar/ocultar. O símbolo de minimizar substitui o X, sem excluir a nota nem criar janela na barra de tarefas.
- Base com exatamente seis ações alinhadas: cor, imagem, negrito, itálico, lista com marcadores e lixeira com confirmação.
- Removidos o menu de três pontos, o menu da faixa superior e os textos passivos da nota. Falhas de salvamento continuam visíveis.
- Redimensionamento nas quatro laterais e nos quatro cantos, com cursor correspondente ao passar o mouse. A borda oposta permanece fixa, os tamanhos mínimos são respeitados e a nova geometria é salva.
- Troca de área disponível nos cartões do widget quando há mais de uma área; notas, conteúdo, imagens, cores e posições preservados.

## Versão 1.3.1

- Novo ícone Fluent com três notas adesivas: amarela, azul e rosa, com gradientes, profundidade e fundo transparente.
- Ícone atualizado no executável, bandeja, atalhos e assets de empacotamento da Store. ICO com nove tamanhos de 16 a 256 px.
- Atalhos usam um caminho de ícone versionado para evitar a imagem antiga no cache do Explorer. Instalação oferece `-DesktopShortcut` e mantém atualizado um atalho já existente do próprio app.

## Versão 1.3.0

- Widget compacto de 360 px e configurações separadas acima dele, com mesma largura e intervalo de 12 px, seguindo a base de Calendário/Áudio.
- Tema Fluent, cabeçalho de 64 px, controles de 32 px, materiais translúcidos, grupos expansíveis e rolagem com botão de rodapé.
- Animação de 240 ms na abertura e recolhimento, respeitando animações, transparência e alto contraste do Windows.
- Bandeja nativa com suporte a teclado e recuperação após reinício do Explorer. Widget fixável, fechamento por Esc e integração de foco com os demais widgets da suíte.
- Atalho Ctrl+, e botão de configurações nas notas. Notas continuam independentes, com posição livre, 50 cores, imagens e exclusão confirmada.
- Regressões isoladas e verificações nativas para alinhamento, acoplamento e preservação de notas. Nenhuma nota do usuário é usada nos testes.

## Versão 1.2.0

- 50 cores em cinco linhas de dez: Normal, Pastel, Vivas, Neon e Terrosas, com cor forte real e contraste automático de texto nos temas claro e escuro.
- Faixa superior ampliada para arraste livre; a posição permanece salva e não é encaixada em grade automaticamente.
- Botão de lixeira à direita, alinhado com os demais botões da base de cada nota. Botão e menu usam a mesma confirmação, com Cancelar como padrão.
- Excluir a última nota não recria uma nota de boas-vindas ao reiniciar.
- Exclusão recuperável e reversão do estado se a gravação falhar. Notas existentes preservam cores suaves, textos, imagens e posições.

A publicação na Microsoft Store continua sujeita à aprovação do cadastro empresarial, identidade oficial, primeira submissão e certificação. Esta release no GitHub não comprova publicação na Store.

