# Atualizações do desktop, GitHub e Store

O pedido de Bruno é atendido por uma fonte canônica e um fluxo de release. Alterações arbitrárias em um executável local não viram código no GitHub. Futuras mudanças devem passar pelo repositório, CI e publicação; não por substituição direta do app instalado.

## Preparação e publicação

1. Atualizar a versão em `src/WindowsStickyNotes.csproj` e testar em dados isolados.
2. Publicar o commit na `main`. O workflow compila x64/ARM64, roda regressões x64, valida dois MSIX e executa o binário ARM64 em um runner Windows ARM64 nativo.
3. Após CI aprovado, criar a tag `vX.Y.Z` apontando exatamente para esse commit. O workflow cria a release no GitHub com ZIPs e checksums.
4. O job Store só submete atualizações de um produto já publicado. Ausência de cadastro, identidade, primeira publicação ou credenciais é uma falha explícita; não uma publicação bem-sucedida. A aprovação e a disponibilização dependem da Microsoft.
5. Conferir a versão pública no Partner Center/Store. Anexar à mesma release um `store-receipt.json` com `status: published`, `version`, `sourceCommit`, `productId`, `storeUrl` e data real da conferência. Não marcar submissão pendente como publicada.
6. `python scripts/release.py deploy --tag vX.Y.Z` baixa esse recibo e o ZIP correspondente, verifica SHA-256 e instala o artefato publicado. Sem recibo publicado, não atualiza o desktop.

Os comandos `publish`/`deploy` exigem Git/CLI autenticados pelo procedimento oficial. Nesta sessão o conector GitHub é a identidade autorizada; seus tokens não são extraídos nem transportados para CLI. Um agente pode publicar a fonte com as ferramentas GitHub existentes e criar a tag pela interface autorizada, aguardando o mesmo pipeline.

## Configuração da Store

- Concluir o cadastro empresarial existente; não criar inscrições duplicadas.
- Reservar o nome do produto e copiar a identidade **exata** do Partner Center. `store/identity.example.json` é somente uma estrutura de referência. Nunca usar identidade fictícia para submissão.
- Configurar variáveis de Actions: `STORE_IDENTITY_NAME`, `STORE_PUBLISHER`, `STORE_PUBLISHER_DISPLAY_NAME`, `STORE_PRODUCT_ID`.
- Concluir a primeira submissão com classificação, categoria, descrições, ícones, capturas de tela, política de privacidade e certificação.
- Só então definir `STORE_FIRST_PUBLICATION_COMPLETE=true`.
- Para automação posterior: usar a aplicação Entra aprovada no Partner Center e os segredos `AZURE_AD_TENANT_ID`, `SELLER_ID`, `AZURE_AD_APPLICATION_CLIENT_ID`, `AZURE_AD_APPLICATION_SECRET`. Criar novas credenciais ou conceder papéis exige a aprovação apropriada; nunca publicar segredos no código, chat ou logs.

Documentação oficial: [automação GitHub/Store](https://learn.microsoft.com/windows/apps/publish/msstore-dev-cli/github-actions), [empacotamento WPF](https://learn.microsoft.com/windows/apps/desktop/modernize/dotnet/package-app), [StartupTask](https://learn.microsoft.com/uwp/api/windows.applicationmodel.startuptask).

## Compatibilidade e limites

Matriz de distribuição: Windows 11 atualizado, em versão com suporte, Intel/AMD x64 ou ARM64. Runtime incluído. Instalação por usuário, sem administrador. Sem vínculo a conta, domínio, nome de usuário ou credenciais desta máquina. A UI acompanha tema, alto contraste e preferência de animações do Windows.

A versão MSIX usa a tarefa de inicialização do próprio Windows e reconhece a ativação por startup; não grava uma Run key apontando para uma pasta de pacote versionada. Dados do usuário ficam fora da pasta binária. Pacotes Store e portáteis podem ter armazenamento diferente por causa da virtualização do Windows; não se presume compartilhamento.

`msix-test` usa identidade de teste separada, valida semanticamente com MakeAppx e guarda os pacotes em `.artifacts/packaging-test`. Não instala certificados nem os distribui como oficiais. Ainda são necessárias instalação/atualização do MSIX com identidade real, verificação de continuidade dos dados e QA visual em monitores/DPI diversos. Runners ARM64 confirmam execução nativa, não todos os modelos físicos. “Qualquer computador” não inclui sistemas sem suporte, Windows S Mode impedindo Win32, políticas corporativas que bloqueiam apps ou sistemas não Windows.
