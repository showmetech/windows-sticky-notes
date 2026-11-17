import importlib.util, pathlib, unittest
spec=importlib.util.spec_from_file_location('distribution',pathlib.Path(__file__).with_name('check-distribution.py'))
audit=importlib.util.module_from_spec(spec);spec.loader.exec_module(audit)
class PrivacyGuardTests(unittest.TestCase):
 def test_user_credentials(self):
  synthetic='1/'+'/'+'Ab9_'*16
  for encoding in ('utf-8','utf-16le'):
   self.assertTrue(audit.inspect('app/example.exe',('"'+synthetic+'"').encode(encoding)))
 def test_binary_resource_is_not_a_user_token(self):
  resource=('1111/'+'/'+'1'*56+'/'+'A'*30).encode('utf-16le')
  self.assertEqual([],audit.inspect('app/example.exe',resource))
 def test_private_files(self):
  for name in ('app/.sync/google-session.dat','app/notebook.json','app/documents/example.note','app/logs/errors.log','app/key.pfx','app/debug.pdb'):
   self.assertTrue(audit.inspect(name,b'synthetic'))
 def test_public_client_identity_and_docs(self):
  self.assertEqual([],audit.inspect('app/Assets/google-oauth.json',b'{"client_id":"example.apps.googleusercontent.com"}'))
  self.assertEqual([],audit.inspect('PRIVACY.md',b'Contact: support@example.invalid'))
 def test_unsafe_archive_paths(self):
  self.assertTrue(audit.inspect('../outside.txt',b''))
if __name__=='__main__':unittest.main()
