"""Inject Google's desktop registration from a build secret; never print it."""
from pathlib import Path
import os,json
root=Path(__file__).resolve().parents[1]
value=os.environ.get('GOOGLE_DESKTOP_CLIENT_SECRET','')
target=root/'.artifacts'/'GoogleDesktopClient.g.cs'
if not value:
    raise SystemExit('Google desktop registration missing from build environment')
if len(value)>500 or any(c in value for c in '\r\n'):
    raise SystemExit('Google desktop registration has an invalid format')
target.parent.mkdir(exist_ok=True)
target.write_text('namespace StickyNotes; internal static partial class GoogleDesktopClient { static partial void Configure(ref string value) { value = '+json.dumps(value)+'; } }',encoding='utf-8')
print('Google desktop registration injected; value withheld')
