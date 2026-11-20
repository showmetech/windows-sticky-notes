import copy, hashlib, importlib.util, io, json, pathlib, tempfile, unittest, urllib.error, zipfile
spec=importlib.util.spec_from_file_location('submit_store',pathlib.Path(__file__).with_name('submit-store.py'))
store=importlib.util.module_from_spec(spec);spec.loader.exec_module(store)
ENV={'TENANT_ID':'00000000-0000-0000-0000-000000000001','CLIENT_ID':'00000000-0000-0000-0000-000000000002','CLIENT_SECRET':'synthetic-fixture'}

class SubmissionTests(unittest.TestCase):
 def setUp(self):
  self.temp=tempfile.TemporaryDirectory();self.addCleanup(self.temp.cleanup)
  self.package=pathlib.Path(self.temp.name)/'Windows-Sticky-Notes-1.5.4.msixupload'
  self.package.write_bytes(b'public-release-fixture')
  self.checksum=self.package.parent/'SHA256.txt'
  self.checksum.write_text(hashlib.sha256(self.package.read_bytes()).hexdigest()+'  '+self.package.name+'\n')
  self.logs=[];self.requests=[]
  self.app={'id':store.PRODUCT,'lastPublishedApplicationSubmission':{'id':'11'},'pendingApplicationSubmission':None}
  self.draft={'id':'12','fileUploadUrl':'https://fixture.blob.core.windows.net/package?sig=synthetic-sas',
   'status':'PendingCommit','pricing':{'priceId':'Free'},'listings':{'pt-br':{'description':'Preserve listing'}},
   'applicationPackages':[{'fileName':'old.msixbundle','fileStatus':'Uploaded'}]}
  self.status='PreProcessing'
 def open(self,req,timeout):
  self.requests.append(req)
  url=req.full_url; method=req.get_method()
  if 'login.microsoftonline.com' in url:return io.BytesIO(b'{"access_token":"synthetic-token"}')
  if url==store.API+store.PRODUCT:return io.BytesIO(json.dumps(self.app).encode())
  if url.endswith('/submissions') and method=='POST':return io.BytesIO(json.dumps(self.draft).encode())
  if url.endswith('/status'):return io.BytesIO(json.dumps({'status':self.status}).encode())
  return io.BytesIO(b'')
 def run_submit(self):return store.submit(self.package,store.PRODUCT,ENV,self.open,lambda _:None,self.logs.append)
 def test_preserves_listing_and_price_and_uploads_only_release(self):
  result=self.run_submit();self.assertTrue(result['submissionAccepted']);self.assertFalse(result['publicationVerified'])
  update=json.loads(next(r.data for r in self.requests if r.full_url.endswith('/submissions/12') and r.get_method()=='PUT'))
  self.assertEqual(update['pricing'],self.draft['pricing']);self.assertEqual(update['listings'],self.draft['listings'])
  self.assertEqual([p['fileStatus'] for p in update['applicationPackages']],['PendingDelete','PendingUpload'])
  upload=next(r for r in self.requests if 'blob.core.windows.net' in r.full_url)
  self.assertIsNone(upload.get_header('Authorization'))
  with zipfile.ZipFile(io.BytesIO(upload.data)) as archive:
   self.assertEqual(archive.namelist(),[self.package.name]);self.assertEqual(archive.read(self.package.name),self.package.read_bytes())
  self.assertNotIn('synthetic',str(self.logs));self.assertNotIn('sig=',str(self.logs))
 def test_foreign_product_blocked_before_network(self):
  with self.assertRaises(ValueError):store.submit(self.package,'OTHERPRODUCT',ENV,self.open)
  self.assertEqual(self.requests,[])
 def test_tampered_release_blocked_before_network(self):
  self.package.write_bytes(b'tampered')
  with self.assertRaises(ValueError):self.run_submit()
  self.assertEqual(self.requests,[])
 def test_existing_submission_never_changed(self):
  self.app['pendingApplicationSubmission']={'id':'human-draft'}
  with self.assertRaises(ValueError):self.run_submit()
  self.assertEqual(len(self.requests),2)
 def test_unexpected_upload_host_never_receives_file(self):
  self.draft['fileUploadUrl']='https://unexpected.invalid/upload'
  with self.assertRaises(ValueError):self.run_submit()
  self.assertFalse(any(r.get_method()=='PUT' for r in self.requests))
 def test_failed_commit_does_not_claim_acceptance(self):
  self.status='CommitFailed'
  with self.assertRaises(ValueError):self.run_submit()
  self.assertFalse(any('submissionAccepted' in line for line in self.logs))
 def test_http_error_does_not_echo_credentials_or_response(self):
  def failed(req,timeout):raise urllib.error.HTTPError(req.full_url,403,'synthetic-sensitive-response',{},None)
  with self.assertRaises(RuntimeError) as raised:store.submit(self.package,store.PRODUCT,ENV,failed,emit=self.logs.append)
  self.assertIn('authentication',str(raised.exception));self.assertNotIn('synthetic',str(raised.exception))

if __name__=='__main__':unittest.main()
