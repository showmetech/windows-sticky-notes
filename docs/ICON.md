# Identidade visual do aplicativo

Ícone original de três notas adesivas, em amarelo, azul e rosa, com fundo transparente, gradientes e camadas de papel. A referência de Bruno foi a família de ícones Fluent do Windows/Microsoft 365; não foram copiados logos da Microsoft.

Master: `src/Assets/Notes.png`. Arte criada com a ferramenta de geração de imagens integrada, em 10/11/2026. O master preserva a transparência original.

ICO: `src/Assets/Notes.ico`, com representações PNG de 16, 20, 24, 32, 40, 48, 64, 128 e 256 px. Conversão técnica por WPF, sem alterar a composição:

```powershell
dotnet run --project tools/StoreAssets -c Release -- src/Assets/Notes.png src/Assets --app-icons
```

O projeto usa o ICO como `ApplicationIcon`; a bandeja nativa carrega o mesmo arquivo. O pipeline de release inclui uma cópia `Assets/Notes-<versão>.ico`, usada pelos atalhos para evitar cache de ícones antigos. O empacotamento MSIX gera os logos do mesmo master PNG. Nenhuma limpeza global de cache ou reinicialização do Explorer é necessária para a atualização dos atalhos.

Prompt utilizado: criar um único ícone Windows 11 Fluent para um app de notas, com exatamente três papéis quadrados sobrepostos e claramente distintos — amarelo à frente, azul/ciano atrás à esquerda e rosa/magenta atrás à direita. Gradientes suaves, volume discreto, cantos arredondados, uma dobra sutil no papel frontal; sem letras, texto, marcas ou moldura. Fundo realmente transparente e silhueta compacta legível em tamanhos pequenos.
