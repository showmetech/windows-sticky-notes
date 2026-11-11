"""Reproducible Windows builds. No credentials or user data are included."""
import argparse, hashlib, json, os, pathlib, re, shutil, subprocess, sys, xml.etree.ElementTree as ET, zipfile
ROOT=pathlib.Path(__file__).resolve().parents[1]
PROJECT=ROOT/'src'/'WindowsStickyNotes.csproj'
ARTIFACTS=ROOT/'artifacts'
STAGE=ROOT/'.artifacts'

def run(*args,timeout=None):
    subprocess.run([str(a) for a in args],cwd=ROOT,check=True,timeout=timeout)

def version():
    value=ET.parse(PROJECT).findtext('.//Version')
    if not re.fullmatch(r'\d+\.\d+\.\d+',value or ''): raise ValueError('Project Version must be major.minor.patch')
    return value

def sdk_tool(name):
    override=os.environ.get(name.upper().replace('.','_'))
    if override and pathlib.Path(override).is_file(): return override
    base=pathlib.Path(os.environ.get('ProgramFiles(x86)',r'C:\Program Files (x86)'))/'Windows Kits'/'10'/'bin'
    found=sorted(base.glob('*/x64/'+name),reverse=True)
    if not found:
        cache=pathlib.Path(os.environ.get('NUGET_PACKAGES',str(pathlib.Path.home()/'.nuget'/'packages')))
        found=sorted((cache/'microsoft.windows.sdk.buildtools'/'10.0.26100.4654'/'bin').glob('*/x64/'+name),reverse=True)
    if not found: raise RuntimeError('Install the official Windows SDK; missing '+name)
    return str(found[0])

def reject_private_files(folder):
    forbidden={'.dat','.pfx','.p12','.dpapi','.note','.bak'}
    for path in folder.rglob('*'):
        if path.is_file() and (path.suffix.lower() in forbidden or 'client_secret' in path.name.lower() or path.name in ('notebook.json','settings.json','accounts.json') or path.name.startswith('calendar-')):
            raise RuntimeError('Private file in distribution: '+str(path.relative_to(folder)))

def clean_stage(path):
    resolved=path.resolve()
    if not resolved.is_relative_to(STAGE.resolve()) or resolved==STAGE.resolve(): raise RuntimeError('Unsafe staging cleanup path')
    if path.exists(): shutil.rmtree(path)

def build():
    ARTIFACTS.mkdir(exist_ok=True); STAGE.mkdir(exist_ok=True)
    for arch in ('x64','arm64'):
        output=STAGE/arch
        clean_stage(output)
        run('dotnet','publish',PROJECT,'-c','Release','-r','win-'+arch,'--self-contained','true','-p:PublishSingleFile=true','-p:IncludeNativeLibrariesForSelfExtract=true','-p:DebugType=None','-o',output,'--nologo')
        commit=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip()
        (output/'build-info.json').write_text(json.dumps({'version':version(),'architecture':arch,'sourceCommit':commit,'googleOAuthConfigured':(STAGE/'GoogleDesktopClient.g.cs').is_file()},indent=2),encoding='utf-8')
        # A new icon path prevents Explorer retaining the previous shortcut icon.
        shutil.copy2(output/'Assets'/'Notes.ico',output/'Assets'/f'Notes-{version()}.ico')
        reject_private_files(output)
        if arch=='x64':
            tests=STAGE/'tests';tests.mkdir(exist_ok=True)
            report_path=tests/'self-tests.json'
            run(output/'WindowsStickyNotes.exe','--self-test','--report',report_path,timeout=120)
            report=json.loads(report_path.read_text(encoding='utf-8-sig'))
            if report.get('failed')!=0 or report.get('processArchitecture')!='X64': raise RuntimeError('Regression tests failed')
            shutil.copy2(report_path,ARTIFACTS/'verification.json')
        dist=STAGE/('distribution-'+arch);clean_stage(dist);dist.mkdir(exist_ok=True)
        shutil.copytree(output,dist/'app',dirs_exist_ok=True)
        for name in ('Install.ps1','Uninstall.ps1','README.md','PRIVACY.md'):
            shutil.copy2(ROOT/name,dist/name)
        shutil.make_archive(str(ARTIFACTS/f'Windows-Sticky-Notes-{version()}-{arch}'),'zip',dist)
    write_hashes()

def identity():
    path=ROOT/'store'/'identity.json'
    value=json.loads(path.read_text(encoding='utf-8-sig')) if path.exists() else {
        'identityName':os.environ.get('STORE_IDENTITY_NAME',''), 'publisher':os.environ.get('STORE_PUBLISHER',''),
        'publisherDisplayName':os.environ.get('STORE_PUBLISHER_DISPLAY_NAME',''), 'productId':os.environ.get('STORE_PRODUCT_ID',''),
        'displayName':'Windows Sticky Notes'}
    missing=[key for key in ('identityName','publisher','publisherDisplayName','productId') if not value.get(key)]
    if missing: raise RuntimeError('Partner Center configuration missing: '+', '.join(missing))
    return value

def manifest(config,arch,destination):
    ns='http://schemas.microsoft.com/appx/manifest/foundation/windows10'
    uap='http://schemas.microsoft.com/appx/manifest/uap/windows10'
    uap5='http://schemas.microsoft.com/appx/manifest/uap/windows10/5'
    rescap='http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities'
    for prefix,url in (('',ns),('uap',uap),('uap5',uap5),('rescap',rescap)): ET.register_namespace(prefix,url)
    def add(parent,name,attributes=None,text=None):
        element=ET.SubElement(parent,'{'+ns+'}'+name,attributes or {})
        if text is not None: element.text=text
        return element
    package=ET.Element('{'+ns+'}Package',{'IgnorableNamespaces':'uap uap5 rescap'})
    add(package,'Identity',{'Name':config['identityName'],'Publisher':config['publisher'],'Version':version()+'.0','ProcessorArchitecture':arch})
    props=add(package,'Properties'); add(props,'DisplayName',text=config['displayName']);add(props,'PublisherDisplayName',text=config['publisherDisplayName']);add(props,'Logo',text='StoreAssets\\Logo50.png')
    deps=add(package,'Dependencies');add(deps,'TargetDeviceFamily',{'Name':'Windows.Desktop','MinVersion':'10.0.22000.0','MaxVersionTested':'10.0.26100.0'})
    resources=add(package,'Resources');add(resources,'Resource',{'Language':'pt-BR'})
    apps=add(package,'Applications');app=add(apps,'Application',{'Id':'StickyNotes','Executable':'WindowsStickyNotes.exe','EntryPoint':'Windows.FullTrustApplication'})
    ET.SubElement(app,'{'+uap+'}VisualElements',{'DisplayName':config['displayName'],'Description':'Notas adesivas coloridas com texto, imagens e organização por áreas.','Square150x150Logo':'StoreAssets\\Logo150.png','Square44x44Logo':'StoreAssets\\Logo44.png','BackgroundColor':'transparent'})
    extensions=add(app,'Extensions');extension=ET.SubElement(extensions,'{'+uap5+'}Extension',{'Category':'windows.startupTask','Executable':'WindowsStickyNotes.exe','EntryPoint':'Windows.FullTrustApplication'})
    ET.SubElement(extension,'{'+uap5+'}StartupTask',{'TaskId':'StickyNotesStartup','Enabled':'true','DisplayName':config['displayName']})
    capabilities=add(package,'Capabilities');ET.SubElement(capabilities,'{'+rescap+'}Capability',{'Name':'runFullTrust'})
    ET.ElementTree(package).write(destination,encoding='utf-8',xml_declaration=True)

def msix(test=False):
    config={'identityName':'Showmetech.StickyNotes.PackagingTest','publisher':'CN=PackagingTest','publisherDisplayName':'Packaging Test','displayName':'TEST ONLY Sticky Notes','productId':'TEST-ONLY'} if test else identity()
    makeappx=sdk_tool('makeappx.exe'); destination=STAGE/'packaging-test' if test else ARTIFACTS
    destination.mkdir(parents=True,exist_ok=True)
    if test: (ARTIFACTS/'packaging-verification.json').unlink(missing_ok=True)
    bundle=destination/'packages';bundle.mkdir(exist_ok=True)
    for arch in ('x64','arm64'):
        source=STAGE/arch
        if not (source/'WindowsStickyNotes.exe').exists(): raise RuntimeError('Run build first')
        if test:
            target=destination/arch
            shutil.copytree(source,target,dirs_exist_ok=True)
            source=target
        reject_private_files(source)
        run('dotnet','run','--project',ROOT/'tools'/'StoreAssets','-c','Release','--',ROOT/'src'/'Assets'/'Notes.png',source/'StoreAssets')
        manifest(config,arch,source/'AppxManifest.xml')
        run(makeappx,'pack','/d',source,'/p',bundle/f'StickyNotes-{arch}.msix','/o')
    package=destination/f'Windows-Sticky-Notes-{version()}.msixbundle'
    run(makeappx,'bundle','/d',bundle,'/p',package,'/o')
    with zipfile.ZipFile(package.with_suffix('.msixupload'),'w',zipfile.ZIP_DEFLATED) as upload:
        upload.write(package,package.name)
    if test:
        report={'passed':True,'identity':'TEST ONLY - not a Store identity','architectures':['x64','arm64'],'makeappxSemanticValidation':True,'signed':False,'installed':False,'storeCertified':False}
        with zipfile.ZipFile(package.with_suffix('.msixupload')) as upload:
            if upload.namelist()!=[package.name] or upload.testzip() is not None: raise RuntimeError('Invalid upload container')
        (ARTIFACTS/'packaging-verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    else: write_hashes()

def write_hashes():
    files=sorted(p for p in ARTIFACTS.iterdir() if p.suffix in ('.zip','.msixbundle','.msix','.msixupload'))
    (ARTIFACTS/'SHA256.txt').write_text(''.join(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+p.name+'\n' for p in files),encoding='utf-8')

def prepare_release():
    """Only one path to a release: commit -> remote CI -> Store submission -> local update."""
    if subprocess.check_output(['git','status','--porcelain'],cwd=ROOT,text=True).strip(): raise RuntimeError('Commit and review all changes first')
    run('gh','auth','status')
    tag='v'+version()
    run('git','push','origin','HEAD:main')
    existing=subprocess.check_output(['git','tag','--list',tag],cwd=ROOT,text=True).strip()
    if not existing: run('git','tag','-a',tag,'-m','Release '+tag)
    if subprocess.check_output(['git','rev-parse',tag+'^{}'],cwd=ROOT,text=True).strip()!=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(): raise RuntimeError('Version tag belongs to another commit')
    run('git','push','origin',tag)
    print('Release requested. Do not install new desktop binaries until the release workflow and Store submission have succeeded. Store certification is a separate Microsoft decision.')

def deploy(tag):
    if not re.fullmatch(r'v\d+\.\d+\.\d+',tag or ''): raise RuntimeError('A version tag is required')
    dest=STAGE/'deploy'/tag;dest.mkdir(parents=True,exist_ok=True)
    run('gh','release','download',tag,'--repo','showmetech/windows-sticky-notes','--pattern','store-receipt.json','--dir',dest)
    receipt=json.loads((dest/'store-receipt.json').read_text(encoding='utf-8-sig'))
    if receipt.get('status')!='published' or receipt.get('version')!=tag[1:] or not receipt.get('sourceCommit') or not receipt.get('storeUrl'):
        raise RuntimeError('Store publication has not been verified. Desktop update is blocked.')
    arch='arm64' if os.environ.get('PROCESSOR_ARCHITECTURE','').upper()=='ARM64' else 'x64'
    asset=f'Windows-Sticky-Notes-{tag[1:]}-{arch}.zip'
    run('gh','release','download',tag,'--repo','showmetech/windows-sticky-notes','--pattern',asset,'--pattern','SHA256.txt','--dir',dest)
    sums={line.split()[1]:line.split()[0] for line in (dest/'SHA256.txt').read_text().splitlines()}
    if hashlib.sha256((dest/asset).read_bytes()).hexdigest()!=sums.get(asset): raise RuntimeError('Release checksum mismatch')
    with zipfile.ZipFile(dest/asset) as z:
        for info in z.infolist():
            target=(dest/'expanded'/info.filename).resolve()
            if not target.is_relative_to((dest/'expanded').resolve()): raise RuntimeError('Unsafe archive path')
        z.extractall(dest/'expanded')
    build_info=json.loads((dest/'expanded'/'app'/'build-info.json').read_text(encoding='utf-8-sig'))
    if build_info.get('sourceCommit')!=receipt['sourceCommit'] or build_info.get('version')!=receipt['version']: raise RuntimeError('Store receipt and release binary do not match')
    run('powershell.exe','-NoProfile','-File',dest/'expanded'/'Install.ps1')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('command',choices=('build','msix','msix-test','publish','deploy'));parser.add_argument('--tag');args=parser.parse_args()
    try: {'build':build,'msix':msix,'msix-test':lambda:msix(True),'publish':prepare_release,'deploy':lambda:deploy(args.tag)}[args.command]()
    except Exception as error: print(str(error),file=sys.stderr);sys.exit(1)
