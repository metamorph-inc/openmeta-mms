import unittest
import shutil
import json
import errno
import os.path

from __init__ import TestBenchExecutor
import dateutil.parser


__author__ = 'adam'
_this_dir = os.path.dirname(os.path.abspath(__file__))


class TBManifestTestBase(unittest.TestCase):
    def _create_clean_manifest_copy(self):
        shutil.copyfile(self.__org_manifest_path__, self.__test_manifest_path__)

    def _load_manifest(self, path):
        f = open(path)
        dict_manifest = json.load(f)
        f.close()
        return dict_manifest


    def _load_manifest_as_dict(self, path):
        """
        Given a manifest, load it, and return its contents as a dictionary.

        @param path: The path to the manifest
        @type path: str
        @return: A dictionary of the manifest contents
        @rtype: dict
        """

        return self._load_manifest(path)

    def _load_steps_from_manifest(self, path):
        """
        Given a manifest, load all the steps, and return them as a list of dictionaries

        @param path: The path to the manifest
        @type path: str
        @return: A list of steps from the manifest file
        @rtype: list
        """
        dict_manifest = self._load_manifest_as_dict(path)

        return dict_manifest["Steps"]

    def _rm_log_dir(self):
        try:
            shutil.rmtree(os.path.join(_this_dir, 'log'))
        except OSError as e:
            if e.errno != errno.ENOENT:
                raise

    def setUp(self):
        self._create_clean_manifest_copy()
        self.executor = TestBenchExecutor(self.__test_manifest_path__)
        self._rm_log_dir()

    def tearDown(self):
        pass
        # self._rm_log_dir()


class TestLoadTBManifest(TBManifestTestBase):
    __org_manifest_path__ = os.path.join(_this_dir, "test_tb_manifest.json")
    __test_manifest_path__ = os.path.join(_this_dir, "test_tb_manifest_tmp.json")

    def test_executor_loaded(self):
        """
        Pretty simple; just make sure that the executor initialized.
        """
        self.assertNotEqual(self.executor, None)
        pass

    def test_executor_run_all(self):
        """
        Runs all of the steps in the manifest.
        Before running, check that all steps appear unexecuted.
        Afterwards, check for status, timestamps, and log messages.
        """
        dict_unexecuted = {
            "Status": "UNEXECUTED",
            "ExecutionCompletionTimestamp": None,
            "ExecutionStartTimestamp": None,
            "LogFile": None
        }
        for s in self._load_steps_from_manifest(self.__test_manifest_path__):
            self.assertDictContainsSubset(dict_unexecuted, s,
                                          msg="Test manifest's steps appear to be partially or fully executed. "
                                              "An unexecuted manifest should have been created for the test.")

        self.assertEqual(0, self.executor.run_all())

        manifest_dict = self._load_manifest_as_dict(self.__test_manifest_path__)
        self.assertEqual(str(manifest_dict["Status"]), "OK")

        for s in self._load_steps_from_manifest(self.__test_manifest_path__):
            self.assertEqual(s["Status"], "OK")
            self.assertNotEqual(s["ExecutionCompletionTimestamp"], None)
            self.assertNotEqual(s["ExecutionStartTimestamp"], None)
            completion_timestamp = dateutil.parser.parse(s["ExecutionCompletionTimestamp"])
            start_timestamp = dateutil.parser.parse(s["ExecutionStartTimestamp"])
            self.assertGreater(completion_timestamp, start_timestamp)

            self.assertNotEqual(s["LogFile"], None)
            logpath = os.path.isfile(os.path.join(os.path.dirname(self.__test_manifest_path__), s["LogFile"]))
            self.assertTrue(logpath, s["LogFile"] + " doesn't exist")
        manifest = self._load_manifest(self.__test_manifest_path__)
        self.assertEqual("OK", manifest["Status"])


class TestTBManifestFailedStep(TBManifestTestBase):
    __org_manifest_path__ = os.path.join(_this_dir, "test_tb_manifest_fail.json")
    __test_manifest_path__ = os.path.join(_this_dir, "test_tb_manifest_fail_tmp.json")

    def test_executor_run_all(self):
        """
        Runs all of the steps in the manifest. One will fail.
        """
        dict_unexecuted = {
            "Status": "UNEXECUTED",
            "ExecutionCompletionTimestamp": None,
            "ExecutionStartTimestamp": None,
            "LogFile": None
        }
        for s in self._load_steps_from_manifest(self.__test_manifest_path__):
            self.assertDictContainsSubset(dict_unexecuted, s,
                                          msg="Test manifest's steps appear to be partially or fully executed. "
                                              "An unexecuted manifest should have been created for the test.")

        self.assertGreater(0, self.executor.run_all())

        manifest_dict = self._load_manifest_as_dict(self.__test_manifest_path__)
        self.assertEqual(str(manifest_dict["Status"]), "FAILED")

        steps = self._load_steps_from_manifest(self.__test_manifest_path__)
        self.assertEqual(steps[0]["Status"], "FAILED")
        self.assertEqual(steps[1]["Status"], "UNEXECUTED")
        self.assertNotEqual(steps[0]["ExecutionCompletionTimestamp"], None)
        self.assertNotEqual(steps[0]["ExecutionStartTimestamp"], None)
        self.assertEqual(steps[1]["ExecutionCompletionTimestamp"], None)
        self.assertEqual(steps[1]["ExecutionStartTimestamp"], None)

        self.assertNotEqual(steps[0]["LogFile"], None)
        logpath = os.path.isfile(os.path.join(os.path.dirname(self.__test_manifest_path__), steps[0]["LogFile"]))
        self.assertTrue(logpath, steps[0]["LogFile"] + " doesn't exist")
        manifest = self._load_manifest(self.__test_manifest_path__)
        self.assertEqual("FAILED", manifest["Status"])


if __name__ == '__main__':
    unittest.main()
