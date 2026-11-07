# Windows Sticky Notes — fonte canônica

Este repositório corresponde ao app desktop de Bruno e a https://github.com/showmetech/windows-sticky-notes.

## Atualizações autorizadas

Bruno determinou em 07/11/2026 que futuras atualizações deste app sejam entregues no GitHub e na Microsoft Store, além do desktop. Toda atualização deve:

1. Reler estado local e remoto; preservar notas e alterações humanas. Aumentar a versão no csproj.
2. Alterar esta fonte, executar regressões isoladas e construir x64/ARM64. Nunca incluir notas, arquivos da conta, credenciais, logs locais ou diretórios ignorados no GitHub.
3. Versionar e publicar a fonte pelo conector GitHub autorizado; confirmar o SHA no destino e aguardar CI. Quando Git/CLI já estiver autenticado, também pode ser usado para operações não cobertas pelo MCP. Não extrair tokens do conector nem sessões do navegador.
4. Criar tag/release para o mesmo commit. O workflow gera ZIPs, valida MSIX, executa testes nativos x64/ARM64 e tenta a submissão à Store quando a identidade real e as credenciais estiverem configuradas.
5. Verificar a certificação e a versão pública na Store. Enquanto o cadastro, a primeira publicação ou a certificação estiverem pendentes, não chamar os canais de sincronizados nem instalar silenciosamente uma nova versão no desktop como se a distribuição estivesse concluída. Preservar a versão instalada e registrar a pendência.
6. O comando `python scripts/release.py deploy --tag vX.Y.Z` só instala um artefato de release cujo recibo Store esteja marcado como publicado. O recibo deve ter evidência real, nunca ser forjado para liberar instalação.

Não implantar diretamente um build de trabalho com Install.ps1. Esse script é para usuários de pacotes distribuídos e instalação inicial, não para contornar o fluxo de manutenção. Artefatos de identidade fictícia são testes de empacotamento, não instaladores nem pacotes de submissão.

Preferir o navegador do Codex para Partner Center. Não mexer nos outros apps, no cadastro compartilhado da empresa ou nas rotinas de outros responsáveis como efeito colateral. Não criar cron ou monitor de arquivos para publicar alterações não revisadas. Não afirmar compatibilidade universal: a matriz suportada é Windows 11 em versão suportada, x64/ARM64; testes de hardware/DPI e MSIX devem ser reportados separadamente.
