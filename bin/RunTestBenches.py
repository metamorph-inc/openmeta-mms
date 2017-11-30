# coding: utf-8
r"""
Run MasterInterpreter on TestBenches, run the JobManager command, and compare Metrics to MetricConstraints.

e.g.
META\bin\Python27\Scripts\python META\bin\RunTestBenches.py --max_configs 2 "Master Combined.mga" -- -s --with-xunit
"""
import os
import os.path
import operator
import unittest
import nose.core
import subprocess
import json
import itertools
import collections

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
    def _testTestBench(self, context, master, config):
        result = master.RunInTransactionWithConfigLight(config)[0]
        if not result.Success:
            self.fail(result.Exception)
        print 'Output directory is {}'.format(result.OutputDirectory)

        project = context.Project
        project.BeginTransactionInNewTerr()
        try:
            kind = context.Meta.Name
        finally:
            project.AbortTransaction()

        if kind == 'ParametricExploration':
            import run_mdao
            import openmdao.api
            originalDir = os.getcwd()
            test = self

            class ConstraintCheckingRecorder(openmdao.api.BaseRecorder):
                def record_metadata(self, group):
                    pass

                def record_derivatives(self, derivs, metadata):
                    pass

                def record_iteration(self, *args, **kwargs):
                    test._checkParametricExplorationMetrics(context, self.root)

                def close(self):
                    pass

                def startup(self, root):
                    super(ConstraintCheckingRecorder, self).startup(root)
                    self.root = root

            os.chdir(result.OutputDirectory)
            try:
                mdao_top = run_mdao.run('mdao_config.json', additional_recorders=[ConstraintCheckingRecorder()])
            finally:
                os.chdir(originalDir)
            self._checkParametricExplorationMetrics(context, mdao_top.top)
        else:
            try:
                subprocess.check_call((os.path.join(meta_path, r'bin\Python27\Scripts\python.exe'), '-m', 'testbenchexecutor', '--detailed-errors', 'testbench_manifest.json'),
                    cwd=result.OutputDirectory)
            except:
                failed_txt = os.path.join(result.OutputDirectory, '_FAILED.txt')
                if os.path.isfile(failed_txt):
                    print(open(failed_txt, 'r').read())
                raise

            with open(os.path.join(result.OutputDirectory, 'testbench_manifest.json')) as manifest_file:
                manifest = json.load(manifest_file)

            self.assertEqual(manifest['Status'], 'OK')
            self._checkTestBenchMetrics(context, manifest, result.OutputDirectory)
        # metrics = {metric['GMEID']: metric['Value'] for metric in manifest['Metrics']}

    def _checkParametricExplorationMetrics(self, pet, mdao_group):
        project = pet.Project
        project.BeginTransactionInNewTerr()
        root = pet

        def getPathNames(pet):
            ret = []
            while pet.ID != root.ID:
                ret.append(pet.Name)
                pet = pet.ParentModel
            ret.reverse()
            return ret
        try:
            def getMetricValue(metric_fco, pet, ref):
                componentName = metric_fco.ParentModel.Name
                if ref is not None:
                    componentName = ref.Name
                group = mdao_group
                for name in getPathNames(pet):
                    group = getattr(group, name)._problem.root
                return getattr(group, componentName).unknowns[metric_fco.Name]

            queue = collections.deque()
            queue.append(pet)
            while queue:
                pet = queue.pop()
                for childPET in (p for p in pet.ChildFCOs if p.Meta.Name == 'ParametricExploration'):
                    queue.append(childPET)
                for constraintBinding in (me for me in pet.ChildFCOs if me.MetaBase.Name == 'MetricConstraintBinding'):
                    if constraintBinding.Src.Meta.Name == 'Metric':
                        metric_fco, constraint = constraintBinding.Src, constraintBinding.Dst
                        metric_refs = constraintBinding.SrcReferences
                    else:
                        constraint, metric_fco = constraintBinding.Src, constraintBinding.Dst
                        metric_refs = constraintBinding.DstReferences
                    testBenchRef = metric_refs.Item(1) if len(metric_refs) else None
                    value = getMetricValue(metric_fco, pet, testBenchRef)
                    parentName = testBenchRef.Name if testBenchRef else metric_fco.ParentModel.Name
                    self._testMetricConstraint(value, constraintBinding, metric_name='.'.join(itertools.chain(getPathNames(pet), [parentName, metric_fco.Name])))
                for testBenchRef in (tb for tb in pet.ChildFCOs if tb.Meta.Name == 'TestBenchRef'):
                    for constraintBinding in (me for me in testBenchRef.Referred.ChildFCOs if me.MetaBase.Name == 'MetricConstraintBinding'):
                        if constraintBinding.Src.Meta.Name == 'Metric':
                            metric_fco, constraint = constraintBinding.Src, constraintBinding.Dst
                        else:
                            constraint, metric_fco = constraintBinding.Src, constraintBinding.Dst
                        value = getMetricValue(metric_fco, pet, testBenchRef)
                        self._testMetricConstraint(value, constraintBinding, metric_name='.'.join(itertools.chain(getPathNames(pet), [testBenchRef.Name, metric_fco.Name])))

        finally:
            project.AbortTransaction()

    def _checkTestBenchMetrics(self, testBench, manifest, outputDir):
        project = testBench.Project
        project.BeginTransactionInNewTerr()
        try:
            manifestMetrics = {m['Name']: m['Value'] for m in manifest['Metrics']}

            def getMetricValue(metric_fco):
                return manifestMetrics[metric_fco.Name]

            for constraintBinding in (me for me in testBench.ChildFCOs if me.MetaBase.Name == 'MetricConstraintBinding'):
                if constraintBinding.Src.Meta.Name == 'Metric':
                    metric_fco, constraint = constraintBinding.Src, constraintBinding.Dst
                else:
                    constraint, metric_fco = constraintBinding.Src, constraintBinding.Dst
                value = getMetricValue(metric_fco)
                self._testMetricConstraint(value, constraintBinding)
        finally:
            project.AbortTransaction()

    def _testMetricConstraint(self, value, constraintBinding, metric_name=None):
        if constraintBinding.Src.Meta.Name == 'Metric':
            metric_fco, constraint = constraintBinding.Src, constraintBinding.Dst
        else:
            constraint, metric_fco = constraintBinding.Src, constraintBinding.Dst
        target_type = constraint.GetStrAttrByNameDisp('TargetType')
        target_value = constraint.GetFloatAttrByNameDisp('TargetValue')
        try:
            value_float = float(value)
        except ValueError:
            self.fail('Metric {} has value "{}" that is not a number'.format(metric_fco.Name, value))
        if target_type == 'Must Exceed':
            test = self.assertGreater
        elif target_type == 'Must Not Exceed':
            test = self.assertLessEqual
        else:
            test = self.assertAlmostEqual
        test(value_float, target_value, msg='Metric {} failed'.format(metric_name or metric_fco.Name))


def crawlForKinds(root, folderKinds, modelKinds):
    queue = collections.deque()
    queue.append(root)
    while queue:
        folder = queue.pop()
        for child_folder in (f for f in folder.ChildFolders if f.MetaBase.Name in folderKinds):
            queue.append(child_folder)
        for child in folder.ChildFCOs:
            if child.Meta.Name in modelKinds:
                yield child


if __name__ == '__main__':
    def run():
        import argparse

        parser = argparse.ArgumentParser(description='Run TestBenches.')
        parser.add_argument('--max_configs', type=int)
        parser.add_argument('--run_desert', action='store_true')
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
            if command_line_args.run_desert:
                desert = Dispatch("MGA.Interpreter.DesignSpaceHelper")
                desert.Initialize(project)
                filter = project.CreateFilter()
                filter.Kind = "DesignContainer"
                # FIXME wont work right for TBs that point to non-root design sace
                designContainers = [tb for tb in project.AllFCOs(filter) if not tb.IsLibObject and tb.ParentFolder is not None]
                for designContainer in designContainers:
                    desert.InvokeEx(project, designContainer, Dispatch("MGA.MgaFCOs"), 128)

            master = Dispatch("CyPhyMasterInterpreter.CyPhyMasterInterpreterAPI")
            master.Initialize(project)

            modelKinds = set(("TestBench", "CADTestBench", "KinematicTestBench", "BlastTestBench", "BallisticTestBench", "CarTestBench", "CFDTestBench", "ParametricExploration"))
            # masterContexts = [tb for tb in itertools.chain(*(project.AllFCOs(filter) for filter in filters)) if not tb.IsLibObject]
            masterContexts = list(crawlForKinds(project.RootFolder, ("ParametricExplorationFolder", "Testing"), modelKinds))
            print repr([t.Name for t in masterContexts])
            for masterContext in masterContexts:
                configs = None
                if masterContext.Meta.Name == "ParametricExploration":
                    tbs = [tb for tb in masterContext.ChildFCOs if tb.MetaBase.Name == 'TestBenchRef' and tb.Referred is not None]
                    if not tbs:
                        configs = [masterContext]
                    else:
                        testBench = tbs[0].Referred
                else:
                    testBench = masterContext

                if not configs:
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
                            raise ValueError('Error: design has no Configurations models. Try using the --run_desert option')
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
                    mi_config.ContextId = masterContext.ID

                    mi_config.SetSelectedConfigurationIds([config.ID])

                    # mi_config.KeepTemporaryModels = True
                    mi_config.PostToJobManager = False

                    def add_test(masterContext, mi_config, config):
                        def testTestBench(self):
                            self._testTestBench(masterContext, master, mi_config)
                        # testTestBench.__name__ = str('test_' + masterContext.Name + "_" + masterContext.ID + "__" + config.Name)
                        testTestBench.__name__ = str('test_' + masterContext.Name + "__" + config.Name)
                        setattr(TestBenchTest, testTestBench.__name__, testTestBench)
                    add_test(masterContext, mi_config, config)
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
