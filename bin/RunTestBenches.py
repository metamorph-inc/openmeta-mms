# coding: utf-8
import os
import os.path
import unittest
import nose.core
from nose.loader import TestLoader
from nose.plugins.manager import BuiltinPluginManager
from nose.config import Config, all_config_files
import subprocess
import json
from win32com.client import DispatchEx
Dispatch = DispatchEx
import _winreg as winreg
with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"Software\META") as software_meta:
    meta_path, _ = winreg.QueryValueEx(software_meta, "META_PATH")


def log(s):
    print s


class TestBenchTest(unittest.TestCase):
    def __init__(self, *args):
        super(TestBenchTest, self).__init__(*args)
        self.longMessage = True

    # this class will get methods added to it
    def _testTestBench(self, testBench, master, config):
        result = master.RunInTransactionWithConfigLight(config)[0]
        if not result.Success:
            self.fail(result.Exception)

        subprocess.check_call((os.path.join(meta_path, r'bin\Python27\Scripts\python.exe'), '-m', 'testbenchexecutor', 'testbench_manifest.json'),
            cwd=result.OutputDirectory)
        print 'Output directory is {}'.format(result.OutputDirectory)

        with open(os.path.join(result.OutputDirectory, 'testbench_manifest.json')) as manifest_file:
            manifest = json.load(manifest_file)

        self.assertEqual(manifest['Status'], 'OK')
        # metrics = {metric['GMEID']: metric['Value'] for metric in manifest['Metrics']}
        project = testBench.Project
        project.BeginTransactionInNewTerr()
        try:
            for metric in manifest['Metrics']:
                metric_fco = project.GetFCOByID(metric['GMEID'])
                for connPoint in metric_fco.PartOfConns:
                    if connPoint.Owner.MetaBase.Name != 'MetricConstraintBinding':
                        continue
                    other = connPoint.Owner.Dst if connPoint.ConnRole == 'dst' else connPoint.Owner.Dst
                    target_type = other.GetStrAttrByNameDisp('TargetType')
                    target_value = other.GetFloatAttrByNameDisp('TargetValue')
                    if target_type == 'Must Exceed':
                        test = self.assertGreater
                    elif target_type == 'Must Not Exceed':
                        test = self.assertLessEqual
                    else:
                        test = self.assertAlmostEqual
                    test(float(metric['Value']), target_value, 'Metric {} failed'.format(metric['Name']))

        finally:
            project.AbortTransaction()

if __name__ == '__main__':
    def run():
        import argparse

        parser = argparse.ArgumentParser(description='Run TestBenches.')
        parser.add_argument('model_file')
        parser.add_argument('nose_options', nargs=argparse.REMAINDER)
        command_line_args = parser.parse_args()

        project = Dispatch("Mga.MgaProject")
        mga_file = command_line_args.model_file
        if mga_file.endswith('.xme'):
            project = Dispatch("Mga.MgaProject")
            parser = Dispatch("Mga.MgaParser")
            resolver = Dispatch("Mga.MgaResolver")
            resolver.IsInteractive = False
            parser.Resolver = resolver
            mga_file = os.path.splitext(command_line_args.model_file)[0] + ".mga"
            project.Create("MGA=" + os.path.abspath(mga_file), "CyPhyML")
            parser.ParseProject(project, command_line_args.model_file)
        else:
            # n.b. without abspath, things break (e.g. CyPhy2CAD)
            project.Open("MGA=" + os.path.abspath(command_line_args.model_file))

        mi_configs = []
        project.BeginTransactionInNewTerr()
        try:
            filter = project.CreateFilter()
            filter.Kind = "TestBench"
            testBenches = project.AllFCOs(filter)
            for tb in testBenches:
                if tb.IsLibObject:
                    continue
                suts = [sut for sut in tb.ChildFCOs if sut.MetaRole.Name == 'TopLevelSystemUnderTest']
                if len(suts) == 0:
                    raise ValueError('Error: TestBench "{}" has no TopLevelSystemUnderTest'.format(tb.Name))
                if len(suts) > 1:
                    raise ValueError('Error: TestBench "{}" has more than one TopLevelSystemUnderTest'.format(tb.Name))
                sut = suts[0]
                if sut.Referred.MetaBase.Name == 'ComponentAssembly':
                    config_ids = [sut.Referred.ID]
                else:
                    configurations = [config for config in sut.Referred.ChildFCOs if config.MetaBase.Name == 'Configurations']
                    if not configurations:
                        raise ValueError('Error: design has no Configurations models')
                    configurations = configurations[0]
                    cwcs = [cwc for cwc in configurations.ChildFCOs if cwc.MetaBase.Name == 'CWC' and cwc.Name]
                    if not cwcs:
                        raise ValueError('Error: could not find CWCs for "{}"'.format(tb.Name))
                    config_ids = [cwc.ID for cwc in cwcs]

                config_light = Dispatch("CyPhyMasterInterpreter.ConfigurationSelectionLight")

                # GME id, or guid, or abs path or path to Test bench or SoT or PET
                config_light.ContextId = tb.ID

                config_light.SetSelectedConfigurationIds(config_ids)

                # config_light.KeepTemporaryModels = True
                config_light.PostToJobManager = False
                mi_configs.append(config_light)

            master = Dispatch("CyPhyMasterInterpreter.CyPhyMasterInterpreterAPI")
            master.Initialize(project)

            for testBench, config in zip(testBenches, mi_configs):
                def add_test(testBench, config):
                    def testTestBench(self):
                        self._testTestBench(testBench, master, config)
                    testTestBench.__name__ = str('test' + testBench.Name + "_" + testBench.ID)
                    setattr(TestBenchTest, testTestBench.__name__, testTestBench)
                add_test(testBench, config)
        finally:
            project.CommitTransaction()

        config = Config(files=all_config_files(), plugins=BuiltinPluginManager())
        config.configure(argv=['nose'] + command_line_args.nose_options)
        loader = TestLoader(config=config)

        tests = [loader.loadTestsFromTestClass(TestBenchTest)]

        try:
            nose.core.TestProgram(suite=tests, argv=['nose'] + command_line_args.nose_options, exit=True, testLoader=loader, config=config)
        finally:
            # project.Save(project.ProjectConnStr + "_debug.mga", True)
            project.Close(True)

    run()
