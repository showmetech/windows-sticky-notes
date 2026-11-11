namespace StickyNotes;
// Desktop OAuth clients are public clients. PKCE and user consent authorize access.
// The registration value is injected at build time, never committed to source.
internal static partial class GoogleDesktopClient {
 static partial void Configure(ref string value);
 internal static string Registration {get{string value="";Configure(ref value);return value;}}
}
