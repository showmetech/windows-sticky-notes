"""Reject broken UTF-8 Portuguese before distributing the app."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
BROKEN=('\u00c3\u00a7','\u00c3\u00a3','\u00c3\u00a9','\u00c3\u00ad','\u00c3\u00b5','\u00c3\u00a1','\u00c2\u00b7','\u00e2\u20ac','\ufffd')
targets=[*ROOT.joinpath('src').glob('*.cs'),*ROOT.joinpath('docs').glob('*.md'),ROOT/'README.md',ROOT/'PRIVACY.md']
for path in targets:
    text=path.read_text(encoding='utf-8-sig')
    if any(fragment in text for fragment in BROKEN):
        raise SystemExit('Invalid Portuguese text encoding: '+str(path.relative_to(ROOT)))
print('UTF-8 text validated:',len(targets),'files')
