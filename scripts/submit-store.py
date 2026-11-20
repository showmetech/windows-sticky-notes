"""Submit one hash-verified official package; never log credentials or SAS URLs."""
import argparse, copy, hashlib, io, json, os, pathlib, re, time, urllib.error, urllib.parse, urllib.request, uuid, zipfile

PRODUCT = '9PN00KMTMDS1'
API = 'https://manage.devcenter.microsoft.com/v1.0/my/applications/'

def verify_package(package):
    package = pathlib.Path(package)
    if not re.fullmatch(r'Windows-Sticky-Notes-\d+\.\d+\.\d+\.msixupload', package.name):
        raise ValueError('Unexpected package filename')
    lines = (package.parent/'SHA256.txt').read_text(encoding='utf-8-sig').splitlines()
    matches = [line[:64].lower() for line in lines if re.fullmatch(r'[a-fA-F0-9]{64}  '+re.escape(package.name), line)]
    if len(matches) != 1 or hashlib.sha256(package.read_bytes()).hexdigest() != matches[0]:
        raise ValueError('Published package checksum mismatch')
    return package

def submit(package, product, env=os.environ, open_url=urllib.request.urlopen, sleep=time.sleep, emit=print):
    if product != PRODUCT:
        raise ValueError('Only the Sticky Notes product is allowed')
    package = verify_package(package)
    tenant = str(uuid.UUID(env['TENANT_ID']))
    client = str(uuid.UUID(env['CLIENT_ID']))
    secret = env['CLIENT_SECRET']
    if not secret:
        raise ValueError('Missing client registration')
    phase = 'authentication'
    submission_id = None
    def request(url, method='GET', data=None, token=None, blob=False):
        headers = {'Content-Type': 'application/json'}
        if token:
            headers['Authorization'] = 'Bearer '+token
        if blob:
            headers = {'Content-Type':'application/zip','x-ms-blob-type':'BlockBlob'}
        req = urllib.request.Request(url, data=data, headers=headers, method=method)
        with open_url(req, timeout=600 if blob else 60) as response:
            payload = response.read()
        return json.loads(payload) if payload and not blob else {}
    try:
        body = urllib.parse.urlencode({'grant_type':'client_credentials','client_id':client,
            'client_secret':secret,'resource':'https://manage.devcenter.microsoft.com'}).encode()
        auth = urllib.request.Request('https://login.microsoftonline.com/'+tenant+'/oauth2/token',
            data=body, headers={'Content-Type':'application/x-www-form-urlencoded'})
        with open_url(auth, timeout=60) as response:
            token = json.load(response)['access_token']
        phase = 'read-product'
        app_url = API+product
        app = request(app_url, token=token)
        if app.get('id') != product or not app.get('lastPublishedApplicationSubmission'):
            raise ValueError('A verified first publication is required')
        if app.get('pendingApplicationSubmission'):
            raise ValueError('Existing submission requires review; nothing changed')
        phase = 'create-submission'
        draft = request(app_url+'/submissions', 'POST', b'', token)
        submission_id = str(draft['id'])
        if not submission_id.isdecimal():
            raise ValueError('Invalid submission identifier')
        emit(json.dumps({'productId':product,'submissionId':submission_id,'phase':'created'}))
        sas = draft['fileUploadUrl']
        upload_uri = urllib.parse.urlparse(sas)
        if upload_uri.scheme != 'https' or not (upload_uri.hostname or '').endswith('.blob.core.windows.net'):
            raise ValueError('Unexpected upload destination; no upload performed')
        update = copy.deepcopy(draft)
        for name in ('id','status','statusDetails','fileUploadUrl','friendlyName'):
            update.pop(name, None)
        for old in update.get('applicationPackages', []):
            old['fileStatus'] = 'PendingDelete'
        update.setdefault('applicationPackages', []).append({'fileName':package.name,
            'fileStatus':'PendingUpload','minimumDirectXVersion':'None','minimumSystemRam':'None'})
        phase = 'update-submission'
        submission_url = app_url+'/submissions/'+submission_id
        request(submission_url, 'PUT', json.dumps(update).encode(), token)
        archive = io.BytesIO()
        with zipfile.ZipFile(archive, 'w', compression=zipfile.ZIP_STORED) as bundle:
            bundle.write(package, package.name)
        phase = 'upload-package'
        request(sas, 'PUT', archive.getvalue(), blob=True)
        phase = 'commit-submission'
        request(submission_url+'/commit', 'POST', b'', token)
        phase = 'verify-commit'
        for attempt in range(30):
            status = request(submission_url+'/status', token=token).get('status')
            if status in ('PreProcessing','Certification','PendingPublication','Publishing','Published','Release'):
                result = {'productId':product,'submissionId':submission_id,'status':status,
                    'submissionAccepted':True,'publicationVerified':status=='Published'}
                emit(json.dumps(result))
                return result
            if status not in ('CommitStarted','PendingCommit','None'):
                raise ValueError('Submission did not pass commit validation')
            sleep(20)
        raise TimeoutError('Commit remains pending; review this submission before retrying')
    except urllib.error.HTTPError as error:
        raise RuntimeError(f'Store phase {phase} failed (HTTP {error.code}); submissionId={submission_id or "none"}. No response body or credential was logged.') from None

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--package', required=True)
    parser.add_argument('--product', required=True)
    args = parser.parse_args()
    try:
        result = submit(args.package, args.product)
    except Exception as error:
        # Generic errors deliberately omit exception values, which could contain a SAS URL.
        message = str(error) if isinstance(error, RuntimeError) and str(error).startswith('Store phase ') else 'Store submission stopped safely. Review the last logged phase and submission ID before retrying.'
        raise SystemExit(message)
    if os.environ.get('GITHUB_STEP_SUMMARY'):
        with open(os.environ['GITHUB_STEP_SUMMARY'], 'a') as summary:
            summary.write('Store submission accepted: `'+json.dumps(result)+'`. Certification and public availability require separate verification.\n')

if __name__ == '__main__':
    main()
