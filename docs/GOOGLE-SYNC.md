# Sincronização Google · 1.4.0

No desktop, abra Configurações → Sincronização com Google → Conectar conta Google. Faça login no navegador do sistema e confira o e-mail na confirmação do app. No notebook, instale a mesma versão e conecte a mesma conta. As notas são combinadas; nenhuma biblioteca é substituída por uma biblioteca vazia.

A sincronização ocorre ao iniciar com uma sessão válida, a cada 30 segundos enquanto o app está aberto e pelo botão Sincronizar agora. Texto, formatação, listas, imagens, título, cor, área, alfinete e lixeira acompanham a conta. Posição e tamanho ficam por computador, adequados aos monitores. Fechar o app antes de concluir o envio deixa a alteração local pendente para a próxima execução.

Se uma nota mudar offline em dois computadores, o app conserva a versão escolhida de forma determinística e cria outra nota com “cópia em conflito”. Exclusão concorrente com edição mantém a original na lixeira e a edição numa cópia. Restaurar pela lixeira também sincroniza. O histórico é cumulativo; a versão atual não faz compactação nem exclusão automática na nuvem. Cada revisão admite até 48 MB, com limites adicionais para texto e imagens.

Desconectar este computador preserva notas e cópia na nuvem. Para trocar de conta, desconecte e conecte a outra; a confirmação deixa explícito que as notas locais serão combinadas com a conta escolhida. Exportar backup não inclui tokens, sessões nem cache de sincronização.

## Cadastro do desenvolvedor

É necessário um projeto Google Cloud exclusivo, Google Drive API habilitada e cliente OAuth do tipo Computador. Distribua somente o identificador público em `src/Assets/google-oauth.json`, formato `{"client_id":"IDENTIFICADOR.apps.googleusercontent.com"}`. O mesmo arquivo público deve acompanhar x64, ARM64 e Store. Nunca distribua tokens, sessões DPAPI ou credenciais de outros aplicativos. Não reutilize o projeto Hermes ou o projeto do calendário: isso acoplaria a identidade e a pasta privada de aplicações distintas.

Fluxo nativo: navegador do sistema, PKCE S256, estado aleatório, callback em IP de loopback com porta aleatória, acesso offline e escopo mínimo `openid email https://www.googleapis.com/auth/drive.appdata`. O identificador público não autentica usuários sozinho. A aprovação e o estado de publicação da tela de consentimento devem ser verificados no Google antes da distribuição pública; não prometer acesso universal com o projeto restrito a usuários de teste.

O índice local mantém geometria; a nuvem contém JSON tipado, nunca XAML executável. Revisões imutáveis são identificadas pelo SHA-256 de conteúdo e pais; duplicatas de upload são deduplicadas. Antes de aplicar uma mesclagem, alterações locais são salvas/capturadas novamente. O commit local tem cópia de recuperação de índice e documentos, revertida na próxima inicialização se houver interrupção. Credenciais ficam em `.sync`, protegidas por DPAPI. Um cache com histórico ausente ou inválido interrompe a sincronização e preserva os arquivos locais.

Referências oficiais: [dados privados no Drive](https://developers.google.com/workspace/drive/api/guides/appdata) e [OAuth para aplicativos nativos](https://developers.google.com/identity/protocols/oauth2/native-app).
