# coding: utf-8
r'''
Run MasterInterpreter on TestBenches, run the JobManager command, and compare Metrics to MetricConstraints.

e.g.
META\bin\Python27\Scripts\python META\bin\RunTestBenches.py --max_configs 2 "Master Combined.mga" -- -s --with-xunit
'''
import os
import os.path
import operator
import unittest
import nose.core
import subprocess
import json

from nose.loader import TestLoader
from nose.plugins.manager import BuiltinPluginManager
from nose.config import Config, all_config_files
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
        testBench_metrics = {m.Name: m for m in (me for me in testBench.ChildFCOs if me.MetaBase.Name == 'Metric')}
        try:
            for metric in manifest['Metrics']:
                # can't use project.GetFCOByID(metric['GMEID']) because it may be from a copy
                metric_fco = testBench_metrics[metric['Name']]
                for connPoint in metric_fco.PartOfConns:
                    if connPoint.Owner.MetaBase.Name != 'MetricConstraintBinding':
                        continue
                    other = connPoint.Owner.Src if connPoint.ConnRole == 'dst' else connPoint.Owner.Dst
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
        parser.add_argument('--max_configs', type=int)
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
            project.OpenEx("MGA=" + os.path.abspath(command_line_args.model_file), "CyPhyML", None)

        project.BeginTransactionInNewTerr()
        try:
            master = Dispatch("CyPhyMasterInterpreter.CyPhyMasterInterpreterAPI")
            master.Initialize(project)

            filter = project.CreateFilter()
            filter.Kind = "TestBench"
            testBenches = [tb for tb in project.AllFCOs(filter) if not tb.IsLibObject]
            for testBench in testBenches:
                suts = [sut for sut in testBench.ChildFCOs if sut.MetaRole.Name == 'TopLevelSystemUnderTest']
                if len(suts) == 0:
                    raise ValueError('Error: TestBench "{}" has no TopLevelSystemUnderTest'.format(testBench.Name))
                if len(suts) > 1:
                    raise ValueError('Error: TestBench "{}" has more than one TopLevelSystemUnderTest'.format(testBench.Name))
                sut = suts[0]
                if sut.Referred.MetaBase.Name == 'ComponentAssembly':
                    configs = [sut.Referred]
                else:
                    configurations = [config for config in sut.Referred.ChildFCOs if config.MetaBase.Name == 'Configurations']
                    if not configurations:
                        raise ValueError('Error: design has no Configurations models')
                    configurations = configurations[0]
                    cwcs = [cwc for cwc in configurations.ChildFCOs if cwc.MetaBase.Name == 'CWC' and cwc.Name]
                    if not cwcs:
                        raise ValueError('Error: could not find CWCs for "{}"'.format(testBench.Name))
                    configs = list(cwcs)
                    # FIXME cfg2 > cfg10
                    configs.sort(key=operator.attrgetter('Name'))
                    configs = configs[slice(0, command_line_args.max_configs)]

                for config in configs:
                    mi_config = Dispatch("CyPhyMasterInterpreter.ConfigurationSelectionLight")

                    # GME id, or guid, or abs path or path to Test bench or SoT or PET
                    mi_config.ContextId = testBench.ID

                    mi_config.SetSelectedConfigurationIds([config.ID])

                    # mi_config.KeepTemporaryModels = True
                    mi_config.PostToJobManager = False

                    def add_test(testBench, mi_config, config):
                        def testTestBench(self):
                            self._testTestBench(testBench, master, mi_config)
                        # testTestBench.__name__ = str('test_' + testBench.Name + "_" + testBench.ID + "__" + config.Name)
                        testTestBench.__name__ = str('test_' + testBench.Name + "__" + config.Name)
                        setattr(TestBenchTest, testTestBench.__name__, testTestBench)
                    add_test(testBench, mi_config, config)
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
