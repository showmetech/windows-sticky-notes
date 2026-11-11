# Política de privacidade — Windows Sticky Notes

O Windows Sticky Notes mantém títulos, textos, imagens, cores e posições das notas no computador do usuário. Funciona sem conta, anúncios, telemetria ou análise de uso. A sincronização Google é opcional e só começa depois do login no navegador e da confirmação da conta no aplicativo.

Ao conectar, títulos, textos formatados, imagens incorporadas, cores, áreas, alfinetes e lixeira são enviados à pasta privada de dados do aplicativo no Google Drive (`appDataFolder`). O app solicita `drive.appdata`, além de identidade e e-mail para identificar a conta; não solicita acesso aos demais arquivos do Drive. O Google processa esses dados conforme suas políticas. A transmissão usa HTTPS. Não há criptografia de ponta a ponta própria do aplicativo.

Posições, dimensões, tema e área ativa permanecem independentes em cada computador. O app guarda revisões para preservar alterações offline e cria cópias quando há conflito. Notas excluídas e revisões anteriores continuam recuperáveis no histórico privado; ocupam armazenamento até os dados do aplicativo serem removidos pelo usuário nas configurações do Google Drive. A interface atual não oferece exclusão permanente desse histórico.

O executável inclui o registro do cliente Google para aplicativos desktop, que identifica o app e não concede acesso às notas por si só. O identificador da conta e o token de renovação são protegidos pelo Windows DPAPI, vinculados ao usuário do Windows. Tokens nunca são enviados ao GitHub nem incluídos no backup exportado. Desconectar remove a sessão deste computador, preservando as notas locais e os dados da nuvem; não desconecta outros dispositivos. Revogar o acesso pelo Google pode exigir novo login nos dispositivos.

O usuário pode importar imagens locais, colar imagens da área de transferência, exportar backups ZIP e abrir a pasta de dados. Backups contêm o texto e as imagens; quem tiver acesso a eles poderá ler esse conteúdo. O aplicativo não adiciona criptografia própria.

Dados são gravados na pasta de dados do perfil do Windows. Na versão MSIX, o Windows pode gerenciar a pasta no contêiner do pacote. Instalações só sincronizam quando conectadas à mesma conta e ao mesmo cadastro Google do aplicativo. Sem conexão, as notas continuam salvas localmente e aguardam nova tentativa. A lixeira conserva notas recuperáveis. A desinstalação portátil preserva os dados; antes de remover/redefinir uma instalação da Store, exporte seu backup.

Atualizações pela Microsoft Store seguem as configurações e políticas da Microsoft. Downloads pelo GitHub seguem as políticas do GitHub. Esses serviços podem coletar dados próprios, independentemente do aplicativo. O app não consulta a rede para atualização automática; o Windows/Store gerencia a instalação da versão Store.

Contato: bruno@showmetech.com.br.
