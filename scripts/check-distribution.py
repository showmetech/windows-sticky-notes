"""Fail closed on private files and known user-credential signatures; never print values."""
import argparse, json, pathlib, re, zipfile

FORBIDDEN={'.dat','.pfx','.p12','.pem','.key','.dpapi','.note','.bak','.log','.pdb'}
PATTERNS={
 'private-key':re.compile(rb'-----BEGIN (?:RSA |EC |OPENSSH |ENCRYPTED )?PRIVATE KEY-----'),
 'google-user-access-token':re.compile(rb'ya29\.[A-Za-z0-9_-]{35,}'),
 'google-user-refresh-token':re.compile(rb'(?<![A-Za-z0-9_+/-])1//[A-Za-z0-9_-]{40,}'),
 'github-token':re.compile(rb'(?:gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{60,})'),
}
def inspect(name,data):
 p=pathlib.PurePosixPath(name.replace('\\','/'))
 issues=[]
 if p.is_absolute() or '..' in p.parts:issues.append('unsafe-package-path')
 if p.suffix.lower() in FORBIDDEN or any(part.lower() in ('.sync','verification','work') for part in p.parts) or p.name.lower() in ('notebook.json','accounts.json','settings.json','.env') or p.name.lower().startswith('.env.') or 'client_secret' in p.name.lower():issues.append('private-file')
 # OAuth desktop client registration and client_id are public app identity, not user grants.
 for label,pattern in PATTERNS.items():
  if pattern.search(data):issues.append(label)
 # Match real UTF-16 strings without joining unrelated binary regions across NULs.
 for label,prefix,minimum in [('google-user-access-token','ya29.',35),('google-user-refresh-token','1//',40),('github-token','ghp_',30),('github-token','github_pat_',60)]:
  expression=rb'(?<![A-Za-z0-9_+/-]\x00)'+re.escape(prefix.encode('utf-16le'))+rb'(?:[A-Za-z0-9_-]\x00){'+str(minimum).encode()+rb',}'
  if re.search(expression,data):issues.append(label)
 if any(marker.encode('utf-16le') in data for marker in ('-----BEGIN PRIVATE KEY-----','-----BEGIN RSA PRIVATE KEY-----','-----BEGIN OPENSSH PRIVATE KEY-----')):issues.append('private-key')
 return [{'path':name,'reason':reason} for reason in sorted(set(issues))]
def check(path):
 findings=[];count=0
 if path.is_dir():
  for item in path.rglob('*'):
   if item.is_file():count+=1;findings.extend(inspect(item.relative_to(path).as_posix(),item.read_bytes()))
 else:
  with zipfile.ZipFile(path) as archive:
   for item in archive.infolist():
    if not item.is_dir():count+=1;findings.extend(inspect(item.filename,archive.read(item)))
 return {'artifact':path.name,'files':count,'findings':findings,'passed':not findings}
if __name__=='__main__':
 parser=argparse.ArgumentParser();parser.add_argument('path',type=pathlib.Path);parser.add_argument('--report',type=pathlib.Path);args=parser.parse_args();result=check(args.path)
 if args.report:args.report.write_text(json.dumps(result,indent=2),encoding='utf-8')
 print(json.dumps(result));raise SystemExit(0 if result['passed'] else 1)
