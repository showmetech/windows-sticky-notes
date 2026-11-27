# Fluent 2 no Sticky Notes

O app mantém WPF/.NET 10 e usa os dicionários oficiais `PresentationFramework.Fluent` em controles, notas e menus. Os botões compactos preservam a disposição aprovada das ações, com preenchimento de estado separado do texto e foco interno de 2 DIP.

- Material sólido, opaco por padrão. A opacidade salva pelo usuário continua válida. Não simulamos Mica/Acrylic com transparência simples.
- Segoe UI Variable: corpo 14/20, subtítulo 20 e ícones Segoe Fluent Icons de 16 DIP nas barras de ferramentas. Documentos existentes mantêm sua formatação.
- Cantos: controles 4, cartões 8, paleta e confirmação 12 DIP. Os intervalos de auto-organização permanecem em 10 DIP.
- Cores neutras por função em claro/escuro; seleções usam o destaque do Windows com contraste verificado. As 50 cores das notas e as escolhas existentes são preservadas.
- Hover e pressionamento mudam apenas uma camada de estado, mantendo a opacidade do conteúdo. Foco de teclado visível. Alto contraste usa cores do sistema; animações existentes respeitam a preferência de movimento do Windows.

Referências oficiais consultadas em 16/11/2026:

- https://fluent2.microsoft.design/design-tokens
- https://fluent2.microsoft.design/typography
- https://fluent2.microsoft.design/shapes
- https://fluent2.microsoft.design/material
- https://fluent2.microsoft.design/elevation
- https://fluent2.microsoft.design/motion
- https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net90

Validação: regressões de dados, janelas reais com perfil isolado, temas claro/escuro, contraste das seleções, anel de foco, layouts mínimo/compacto, controles de opacidade e alternância pela bandeja. As prévias são renderizações WPF de notas sintéticas. Alto contraste permanece implementado com cores do sistema, mas não foi ativado globalmente no desktop do usuário durante este trabalho.


## Revisão1.6.0 —27/11/2026

Três seções substituem cinco abas; não há Viewbox para diminuir texto. Planejamento do painel considera área útil e disponibiliza altura460DIP; acima quando possível, ao lado se houver espaço e sobreposto intencionalmente apenas em telas estreitas. Em altura extrema, rolagem mantém acesso sem reduzir fontes. Regras de opacidade e editor transparente continuam independentes dos templates Fluent.

Busca permite Ctrl+F/limpar/Escape. A criação de área valida no campo; início imediato aparece como switch. Barra de formatação informa estado, cores têm nomes e marca de seleção, ajuda agrupa atalhos. Confirmação da conta e desconexão têm Cancelar padrão e ações explícitas. A conexão real não é comprovada por layout sintético.

Leituras adicionais públicas: [layout](https://fluent2.microsoft.design/layout), [acessibilidade](https://fluent2.microsoft.design/accessibility), [conteúdo](https://fluent2.microsoft.design/content-design), [Windows](https://fluent2.microsoft.design/components/windows), [componentes](https://fluent2.microsoft.design/components/web/react). Aplicados critérios de button, toolbar, input/field/textarea, tablist, searchbox, slider, switch/checkbox, combobox/menu, card, popover/tooltip, dialog, messagebar, spinner/progressbar. Não foram importados componentes React para WPF.

Referências comparáveis: [Microsoft StickyNotes](https://support.microsoft.com/en-us/windows/apps/stickynotes/get-started-with-sticky-notes) para acesso rápido, busca e manipulação das notas; [Simplenote](https://simplenote.com/help/) para busca e atalhos. Não copiamos sua identidade ou alteramos o formato privado de sincronização.
