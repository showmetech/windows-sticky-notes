# Windows Sticky Notes 1.4.1

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
