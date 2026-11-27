# SMT Sticky Notes 1.6.1

- Reinicia o tempo de inatividade do widget ao fechar as configura��es, evitando que ele se oculte imediatamente depois de uma perman�ncia longa no painel.
- Preserva o recolhimento por clique externo e o comportamento de fixa��o.
- Acrescenta regress�es do retorno das configura��es ap�s 25 segundos, sem aguardar tempo real nem alterar notas pessoais.

# SMT Sticky Notes1.6.0 � candidata Fluent2

- Configura��es reorganizadas em Geral, Notas e Google; atalhos e vers�o na ajuda contextual.
- Texto em tamanho natural, sem diminuir a p�gina inteira. O painel usa espa�o acima ou ao lado do widget e respeita a �rea �til; telas excepcionalmente pequenas mant�m rolagem acess�vel.
- Busca com Ctrl+F, limpar e Escape; estado vazio espec�fico para resultados ausentes.
- Interruptor de in�cio autom�tico, restaura��o de opacidade100%, nova �rea com Enter e feedback junto ao campo.
- Indicadores de negrito/it�lico/lista acompanham a sele��o. Paleta50cores adaptada ao monitor, com teclado e retorno de foco.
- Confirma��es Google consistentes com Fluent, Cancelar como padr�o seguro; callback claro/escuro e nome SMT.
- Identidade Store/bandeja, notas, formata��o humana, conta Google e sincroniza��o existentes preservados. Nenhuma migra��o de dados necess�ria.
- Testes pertencem � candidata; publica��o e instala��o dependem do fluxo oficial, n�o impl�citas nesta nota.

# SMT Sticky Notes 1.5.8

- Organiza as notas automaticamente na primeira exibi��o de cada �rea a cada execu��o do app, inclusive ao abrir pela bandeja ap�s iniciar com o Windows.
- Come�a no canto inferior direito, empilha de baixo para cima e abre novas colunas � esquerda, mantendo o espa�amento compacto e a �rea �til de cada monitor.
- Usa a mesma dire��o no bot�o Auto-organizar, sem inverter a ordem ao clicar novamente.
- Mant�m os ajustes manuais ao ocultar e mostrar as notas durante a mesma execu��o.

## SMT Sticky Notes 1.5.7

- Mant�m a cor da nota no t�tulo e no editor, inclusive ao passar o mouse ou digitar, sem preenchimento escuro adicional.
- Remove fundos trazidos por textos colados ou documentos existentes, preservando texto, negrito, it�lico, listas e imagens.
- Enter confirma e salva o t�tulo e leva o foco para o conte�do da nota, sem inserir uma quebra de linha.

## SMT Sticky Notes 1.5.6

- Corrige a abertura inicial do widget e das configura��es fora da tela em monitores com escala de 150%.
- Corrige a inser��o de imagens dentro de listas com marcadores, se��es e c�lulas de tabelas, preservando texto e formata��o.
- Usa o DPI nativo da janela antes da primeira exibi��o, preservando a �rea �til e a barra de tarefas.
- Acrescenta regress�es de primeira abertura em coordenadas f�sicas e convers�o de escala, incluindo origens negativas.

## SMT Sticky Notes 1.5.5

- Nome SMT Sticky Notes no pacote do Windows, menu Iniciar, inicializa��o autom�tica, bandeja, widget e configura��es.
- Identidade do pacote, GUID da bandeja e armazenamento permanecem est�veis para preservar notas e atualiza��es.
- O empacotamento n�o restaura mais o nome antigo quando uma vari�vel de compila��o est� desatualizada; regress�o verifica os manifestos x64 e ARM64.

## Windows Sticky Notes 1.5.4

- Cada nota nova exibe sua cor original automaticamente, diferente das notas existentes, inclusive em outras �reas e ap�s reiniciar o app.
- Notas novas usam cores reais; notas existentes preservam a apar�ncia e o estilo salvos.
- Regress�es cobrem 64 novas notas, temas claro e escuro, cores manuais e persist�ncia.

## Vers�o 1.5.3

- Prepara��o da primeira publica��o Store como Showmetech Sticky Notes, com nome reservado e identidade oficial do produto.
- Nome de exibi��o da Store configur�vel no empacotamento, sem vincular o aplicativo � conta pessoal do desenvolvedor.
- A certifica��o e a disponibilidade p�blica dependem da Microsoft; cadastro e pacote n�o equivalem a publica��o.

## Vers�o 1.5.2

- Instala��o preparada e verificada antes de substituir a vers�o ativa, com backup e revers�o em falhas. Atualiza��es preservam a escolha de in�cio autom�tico.
- Atalhos e registros de terceiros s�o preservados; a desinstala��o retira a configura��o p�blica Google e somente o pr�prio hist�rico de bandeja, com backup.
- Reconex�o Google dispon�vel ap�s sess�o inv�lida ou copiada de outro usu�rio do Windows, sem apagar a sess�o anterior nem as notas.
- �ndices locais inv�lidos recuperam backup preservando o original; sequ�ncia de cores inv�lida n�o bloqueia a cria��o de notas.
- Verifica��es de privacidade dos pacotes e testes de instala��o isolada integrados ao CI. Scripts compat�veis com perfis com espa�os e acentos.

## Vers�o 1.5.1

- Notas sem Fixar se recolhem quando o foco sai do app para outra janela ou para a �rea de trabalho.
- Notas fixadas continuam vis�veis. Editar outra nota, abrir menus, paletas e di�logos do app n�o recolhe as notas.
- Clique na bandeja continua alternando entre ocultar todas e mostrar as notas da �rea ativa. Documentos e posi��es preservados.

## Vers�o 1.5.0

- Estilo Fluent 2 nas notas, widget, configura��es, paleta de cores e confirma��o de exclus�o.
- Superf�cies s�lidas, tipografia Segoe UI Variable, cantos de 4/8/12 DIP e estados de intera��o consistentes.
- Sele��es usam a cor de destaque do Windows; foco de teclado vis�vel sem reduzir o contraste do texto no hover.
- Fundos 100% opacos por padr�o; preservadas as 50 cores, posi��es, sincroniza��o e altern�ncia pela bandeja.

## Vers�o 1.4.9

- Clique no �cone da bandeja alterna entre ocultar todas as notas vis�veis e mostrar todas as notas da �rea ativa.
- Cliques r�pidos respeitam o estado solicitado mesmo durante as anima��es. Conte�do, tamanho e posi��o s�o preservados.
- Widget e configura��es continuam dispon�veis pelo menu do �cone, com o bot�o direito.

## Vers�o 1.4.8

- Painel de notas com �cones, busca com indica��o visual, abas com sele��o destacada e a��es alinhadas em uma �nica faixa.
- Preservada a lista de cart�es coloridos com uma linha de t�tulo.
- Auto-organiza��o com intervalo visual de 10 DIP entre cart�es, reduzindo tamb�m as margens transparentes das notas e ajustando suas sombras.

## Vers�o 1.4.7

- Bot�o Auto-organizar com �cone no widget, menu da bandeja e atalho Ctrl+Shift+O.
- Notas alinhadas � direita em colunas, com largura uniforme e espa�amento de 12 DIP; alturas preservadas quando couberem.
- Organiza��o respeita cada monitor e sua �rea �til, preserva o conte�do e salva a posi��o. Se n�o houver espa�o, mant�m o arranjo anterior sem sobrepor notas.

## Vers�o 1.4.6

- Atualiza��es guardam o backup fora da pasta de programas e retiram �cones de vers�es antigas somente ap�s confirmar sua c�pia de seguran�a.
- Instala��o, atalhos e inicializa��o continuam usando um �nico caminho fixo.

## Vers�o 1.4.5

- Testes isolados removem somente o pr�prio registro de bandeja ao encerrar, evitando entradas antigas nas configura��es do Windows.
- Preservado o identificador fixo da instala��o principal e os registros dos demais aplicativos.

## Vers�o 1.4.4

- Configura��es organizadas em cinco se��es com �cones: Apar�ncia, �reas, Google, Dados e Atalhos.
- Informa��es essenciais, a��es com �cones e p�ginas que se ajustam ao espa�o dispon�vel sem barra de rolagem.
- Barra de opacidade com padr�o 100%, aplica��o imediata e prefer�ncia salva por computador.

## Vers�o 1.4.3

- Barra de opacidade do fundo nas configura��es de apar�ncia, de 0% a 100%, com valor vis�vel, ajuste imediato e padr�o 100%.
- Prefer�ncia salva neste computador; instala��es existentes come�am em 100%. Textos e bot�es permanecem vis�veis ao reduzir a opacidade.

## Vers�o 1.4.2

- Widget, configura��es e superf�cies do app 100% opacos por padr�o nos temas claro, escuro e do sistema.
- Preservados os controles de apar�ncia e as anima��es de abertura, recolhimento e destaque das notas.

## Vers�o 1.4.1

- Lista do widget simplificada: ret�ngulos inteiramente na cor da nota e somente uma linha de t�tulo, com retic�ncias para t�tulos longos.
- Clique em qualquer ponto do ret�ngulo abre e destaca a nota na �rea de trabalho com um contorno pulsante tempor�rio. Cliques repetidos reiniciam o destaque.
- Destaque respeita a op��o de anima��es e o alto contraste do Windows, sem alterar posi��o, tamanho, conte�do ou fixa��o da nota.

## Vers�o 1.4.0

- Sincroniza��o opcional de notas, formata��o, imagens, cores, �reas e lixeira pela mesma conta Google no desktop e notebook.
- Acesso somente � pasta privada do app no Google Drive, com login no navegador do sistema e confirma��o da conta antes do primeiro envio.
- Hist�rico de revis�es, continuidade offline e c�pias de conflito para preservar edi��es simult�neas. Posi��es e tamanhos permanecem independentes por computador.
- Sess�es protegidas pelo Windows; backups exportados n�o incluem credenciais ou cache de sincroniza��o.
- Portugu�s corrigido na tela de retorno do login e em avisos existentes. Verifica��o de UTF-8 inclu�da no fluxo de publica��o.

## Vers�o 1.3.2


- Notas com quatro a��es vis�veis no topo: configura��es, nova nota, fixar e minimizar/ocultar. O s�mbolo de minimizar substitui o X, sem excluir a nota nem criar janela na barra de tarefas.
- Base com exatamente seis a��es alinhadas: cor, imagem, negrito, it�lico, lista com marcadores e lixeira com confirma��o.
- Removidos o menu de tr�s pontos, o menu da faixa superior e os textos passivos da nota. Falhas de salvamento continuam vis�veis.
- Redimensionamento nas quatro laterais e nos quatro cantos, com cursor correspondente ao passar o mouse. A borda oposta permanece fixa, os tamanhos m�nimos s�o respeitados e a nova geometria � salva.
- Troca de �rea dispon�vel nos cart�es do widget quando h� mais de uma �rea; notas, conte�do, imagens, cores e posi��es preservados.

## Vers�o 1.3.1

- Novo �cone Fluent com tr�s notas adesivas: amarela, azul e rosa, com gradientes, profundidade e fundo transparente.
- �cone atualizado no execut�vel, bandeja, atalhos e assets de empacotamento da Store. ICO com nove tamanhos de 16 a 256 px.
- Atalhos usam um caminho de �cone versionado para evitar a imagem antiga no cache do Explorer. Instala��o oferece `-DesktopShortcut` e mant�m atualizado um atalho j� existente do pr�prio app.

## Vers�o 1.3.0

- Widget compacto de 360 px e configura��es separadas acima dele, com mesma largura e intervalo de 12 px, seguindo a base de Calend�rio/�udio.
- Tema Fluent, cabe�alho de 64 px, controles de 32 px, materiais transl�cidos, grupos expans�veis e rolagem com bot�o de rodap�.
- Anima��o de 240 ms na abertura e recolhimento, respeitando anima��es, transpar�ncia e alto contraste do Windows.
- Bandeja nativa com suporte a teclado e recupera��o ap�s rein�cio do Explorer. Widget fix�vel, fechamento por Esc e integra��o de foco com os demais widgets da su�te.
- Atalho Ctrl+, e bot�o de configura��es nas notas. Notas continuam independentes, com posi��o livre, 50 cores, imagens e exclus�o confirmada.
- Regress�es isoladas e verifica��es nativas para alinhamento, acoplamento e preserva��o de notas. Nenhuma nota do usu�rio � usada nos testes.

## Vers�o 1.2.0

- 50 cores em cinco linhas de dez: Normal, Pastel, Vivas, Neon e Terrosas, com cor forte real e contraste autom�tico de texto nos temas claro e escuro.
- Faixa superior ampliada para arraste livre; a posi��o permanece salva e n�o � encaixada em grade automaticamente.
- Bot�o de lixeira � direita, alinhado com os demais bot�es da base de cada nota. Bot�o e menu usam a mesma confirma��o, com Cancelar como padr�o.
- Excluir a �ltima nota n�o recria uma nota de boas-vindas ao reiniciar.
- Exclus�o recuper�vel e revers�o do estado se a grava��o falhar. Notas existentes preservam cores suaves, textos, imagens e posi��es.

A publica��o na Microsoft Store continua sujeita � aprova��o do cadastro empresarial, identidade oficial, primeira submiss�o e certifica��o. Esta release no GitHub n�o comprova publica��o na Store.

