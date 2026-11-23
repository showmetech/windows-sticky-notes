"""Protect the public name while retaining the installed Store identity."""
import importlib.util, json, pathlib, tempfile, unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET

spec=importlib.util.spec_from_file_location('release',pathlib.Path(__file__).with_name('release.py'))
release=importlib.util.module_from_spec(spec);spec.loader.exec_module(release)

class BrandingTests(unittest.TestCase):
    def test_previous_configuration_cannot_restore_old_brand(self):
        official={'identityName':'Showmetech.ShowmetechStickyNotes',
                  'publisher':'CN=FD7524DA-7205-4A8D-BD87-CB2B06495E9A',
                  'publisherDisplayName':'Showmetech','productId':'9PN00KMTMDS1',
                  'displayName':'Showmetech Sticky Notes'}
        with tempfile.TemporaryDirectory() as folder:
            root=pathlib.Path(folder);(root/'store').mkdir()
            (root/'store'/'identity.json').write_text(json.dumps(official))
            with patch.object(release,'ROOT',root):
                configured=release.identity()
            self.assertEqual(configured['displayName'],'SMT Sticky Notes')
            self.assertEqual(configured['identityName'],official['identityName'])
            self.assertEqual(configured['publisher'],official['publisher'])
            for architecture in ('x64','arm64'):
                path=root/(architecture+'.xml');release.manifest(configured,architecture,path)
                manifest=ET.parse(path).getroot()
                self.assertEqual(manifest.find('{*}Identity').get('Name'),official['identityName'])
                self.assertEqual(manifest.find('{*}Properties/{*}DisplayName').text,'SMT Sticky Notes')
                app=manifest.find('{*}Applications/{*}Application')
                self.assertEqual(app.get('Id'),'StickyNotes')
                self.assertEqual(app.find('{*}VisualElements').get('DisplayName'),'SMT Sticky Notes')
                task=app.find('{*}Extensions/{*}Extension/{*}StartupTask')
                self.assertEqual(task.get('TaskId'),'StickyNotesStartup')
                self.assertEqual(task.get('DisplayName'),'SMT Sticky Notes')

if __name__=='__main__': unittest.main()
