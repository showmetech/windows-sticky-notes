using System;
using System.Linq;
namespace StickyNotes;
internal static class GoogleLoginPage {
 internal static string Html(bool success){string message=success?"Volte ao Sticky Notes para confirmar a conta e ativar a sincronização.":"Não foi possível concluir o login. Volte ao aplicativo para tentar novamente.";string encoded=string.Concat(message.Select(c=>c>127?"&#"+(int)c+";":c.ToString()));return "<!doctype html><html lang=pt-BR><meta charset=utf-8><meta name=viewport content='width=device-width, initial-scale=1'><title>Sticky Notes</title><style>body{margin:0;background:#191c23;color:#f5f5f7;font:16px 'Segoe UI',sans-serif;display:grid;place-items:center;min-height:100vh}main{max-width:440px;margin:24px;padding:32px;background:#292c35;border-radius:16px;box-shadow:0 16px 48px #0004}h1{font-size:24px;margin:0 0 16px}p{line-height:1.6;margin:0}</style><main><h1>Sticky Notes</h1><p>"+encoded+"</p></main><script>history.replaceState(null,'','/connected')</script></html>";}
}
