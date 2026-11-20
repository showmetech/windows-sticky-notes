# Canais oficiais e atualização

- Código e releases: https://github.com/showmetech/windows-sticky-notes
- Microsoft Store: https://apps.microsoft.com/detail/9PN00KMTMDS1
- Nome reservado: **Showmetech Sticky Notes**.
- Identidade: `Showmetech.ShowmetechStickyNotes`.
- Família: `Showmetech.ShowmetechStickyNotes_t8ppe5514qkcj`.

Uma entrega do app usa a mesma versão e o mesmo commit para os ZIPs x64/ARM64 e o MSIX enviado à Microsoft. O workflow de release publica somente depois dos testes. Certificação, publicação pública e instalação no computador são estados distintos e precisam de conferência própria.

## Desktop

A distribuição ZIP do GitHub não possui atualizador automático integrado. A instalação Microsoft Store é o canal previsto para o Windows gerenciar as atualizações. Antes de migrar, confirmar a disponibilidade pública do produto, fazer backup e preservar notas, imagens, posições, conta Google e preferência de inicialização. Ativar e validar a instalação Store antes de retirar a instalação portátil. Manter somente um app ativo e um ícone de bandeja.

Não instalar MSIX sem assinatura ou com identidade de teste como substituto do produto publicado. Não afirmar que atualizações automáticas estão ativas em uma instalação portátil. As atualizações Store dependem das configurações de atualização, rede e políticas do Windows.

## Publicação

O workflow `Build, test and publish` recebe somente commits revisados por tag ou acionamento explícito de release. As variáveis `STORE_*` contêm a identidade pública do produto. As credenciais de publicação ficam apenas nos segredos do ambiente/repositório, nunca nos executáveis, na fonte, nos artefatos ou nos dados das notas.

`Verify Microsoft Store publishing access` faz somente autenticação e leitura do produto configurado. Um teste bem-sucedido não comprova permissão de envio, certificação, publicação ou atualização local. Não ampliar permissões silenciosamente se o teste for recusado.

A API oficial de submissões exige o papel `Manager(Windows)` para a aplicação Microsoft Entra. Esse papel abrange os produtos Windows da conta; não é restrito a um único produto. Sua atribuição à integração exclusiva deste app requer autorização explícita do responsável. Os workflows deste repositório continuam direcionados ao `STORE_PRODUCT_ID` do Sticky Notes, e a chave de publicação não deve ser compartilhada com outros repositórios ou distribuída com o app.

Manter `STORE_FIRST_PUBLICATION_COMPLETE` desabilitado até comprovar a primeira publicação pública. Depois, confirmar autenticação, envio do pacote exato e resultado da Microsoft em cada release. Não criar recibo fictício de publicação para liberar a atualização local.

O envio usa a API oficial de submissões e preserva preços, descrições e opções da última publicação. `Deliver existing release to Microsoft Store` permite reenviar o pacote de uma release existente, após conferir seu SHA-256, sem reconstruir ou substituir os binários públicos. Se houver outra submissão pendente, o envio para antes de alterá-la. Submissões criadas pela API devem continuar sendo editadas pela API; a interface do Partner Center serve para conferir seu estado.

Referências oficiais:

- https://learn.microsoft.com/windows/uwp/monetize/create-and-manage-submissions-using-windows-store-services
- https://learn.microsoft.com/windows/apps/package-and-deploy/package-updates-from-store
