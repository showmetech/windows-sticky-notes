# Compatibilidade, instalação e privacidade

## Matriz de suporte

- Windows 11 em versão com suporte, desktop x64 (Intel/AMD) ou ARM64. Não há pacote x86, macOS ou Linux.
- Runtime .NET incluído em cada arquitetura. O usuário não precisa instalar o SDK ou o .NET separadamente.
- Executa como usuário comum, sem elevação. Instalação, atalhos e início automático são por usuário.
- Dados em `%LOCALAPPDATA%\Bruno\WindowsStickyNotes` na distribuição portátil; MSIX usa o LocalFolder do pacote. O caminho não depende do nome do desenvolvedor.
- Acesso à pasta privada do Google Drive é opcional e exige login/consentimento da própria conta. Sem internet, as notas locais continuam disponíveis.

## Instalação e atualização

Extraia o ZIP completo da arquitetura correta. O executável portátil funciona diretamente. Os scripts de instalação/desinstalação requerem uma política Windows que permita scripts; não desative proteções para executá-los. A distribuição Store gerenciará instalação/atualização pelo Windows após a certificação.

O instalador verifica arquivos, arquitetura, atalhos e registros antes de substituir o app. Prepara uma pasta temporária, verifica as cópias e guarda a versão anterior fora da pasta ativa de programas. Em falha de ativação restaura a instalação e os atalhos anteriores. Preserva a opção de início automático durante atualização. Backups intencionais ficam em `%LOCALAPPDATA%\Bruno\WindowsStickyNotes-backups`.

A desinstalação preserva as notas. Remove somente seus arquivos e atalhos comprovadamente próprios e seu registro de bandeja (GUID e caminho do executável), com backup do registro. Não limpa globalmente a bandeja, o registro ou arquivos de outros programas. Backups preservados e o cache de extração do runtime não são instalações ativas.

## Contas e dados

Uma sessão Google copiada de outro usuário/máquina pode não ser decifrada pelo Windows. O app mantém as notas e permite conectar novamente; a sessão anterior não é apagada silenciosamente. Sincronizar não transfere o perfil Windows ou a sessão do desenvolvedor para outras pessoas.

Títulos, textos e imagens locais não recebem criptografia própria. A proteção depende do usuário, das permissões e do disco do Windows. A sessão Google usa DPAPI CurrentUser. Backups exportados contêm conteúdo legível e excluem `.sync`; devem ser guardados como arquivos privados. Consulte a política de privacidade.

## Controles de distribuição

O CI rejeita arquivos de notas, sessões, chaves privadas, logs e símbolos de depuração e procura padrões conhecidos de tokens nos pacotes preparados. A checagem não imprime valores detectados. O client_id e o registro de um cliente OAuth desktop identificam publicamente o aplicativo; não são access/refresh tokens pessoais e não dão acesso às notas sem consentimento.

As verificações automatizadas são uma barreira adicional, não uma garantia matemática contra todo dado sensível possível. Código, documentos, imagens e conteúdo dos artefatos precisam continuar sendo revisados antes de cada publicação.

## Testes e limites

O CI compila x64/ARM64, valida semanticamente MSIX de teste e executa regressões em Windows ARM64 nativo. O instalador é testado com dados, registro e pastas de perfil isolados, incluindo espaços e acentos, preferência de startup desativada, falha de ativação/reversão e remoção seletiva de atalhos e bandeja.

Isso não comprova funcionamento em todos os fabricantes, drivers, níveis de DPI ou políticas corporativas. Windows S Mode, antivírus, permissões negadas, restrições de scripts e rede podem bloquear recursos ou instalação. Não contorne esses controles; trate o diagnóstico concreto. Alto contraste, leitores de tela, alteração física de monitores, reinicialização e instalação do MSIX oficial precisam de evidência própria.

A publicação, a instalação e a migração dos dados para o MSIX oficial permanecem pendentes até haver identidade real e certificação. Não instale uma versão Store em paralelo com a portátil sem planejar a continuidade dos dados e confirmar uma única instância/identidade ativa.
