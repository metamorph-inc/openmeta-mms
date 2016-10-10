using GME.CSharp;
using GME.MGA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyC = ISIS.GME.Dsml.CyPhyML.Classes;

namespace ModelsTest
{
    public class ValueFlowTestFixture : IDisposable
    {
        public ValueFlowTestFixture()
        {
            String connectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(path_XME, out connectionString);

            Boolean ro_mode;
            Project = new MgaProject();
            Project.Open(connectionString, out ro_mode);
            Project.EnableAutoAddOns(true);

            MgaFilter filter = Project.CreateFilter();
            filter.Kind = "Component";
            filter.Name = "ValueFlow";

            var mgaGateway = new MgaGateway(Project);
            mgaGateway.PerformInTransaction(delegate
            {
                ValueFlow = Project.AllFCOs(filter)
                                   .Cast<MgaFCO>()
                                   .Select(x => CyPhyC.Component.Cast(x))
                                   .First();

                RunFormulaEvaluator(ValueFlow.Impl as MgaFCO);
            }, 
            transactiontype_enum.TRANSACTION_GENERAL,
            abort: false);
        }

        public void Dispose()
        {
            Project.Save();
            Project.Close();
        }

        public void RunFormulaEvaluator(MgaFCO currentobj, bool expanded = true)
        {
            // create formula evaluator type
            // FIXME: calling the elaborator is faster than calling the formula evaluator
            Type typeFormulaEval = Type.GetTypeFromProgID("MGA.Interpreter.CyPhyFormulaEvaluator");
            IMgaComponentEx formulaEval = Activator.CreateInstance(typeFormulaEval) as IMgaComponentEx;

            // empty selected object set
            Type typeMgaFCOs = Type.GetTypeFromProgID("Mga.MgaFCOs");
            MgaFCOs selectedObjs = Activator.CreateInstance(typeMgaFCOs) as MgaFCOs;

            // initialize formula evauator
            formulaEval.Initialize(Project);

            // automation means no UI element shall be shown by the interpreter
            formulaEval.ComponentParameter["automation"] = "true";

            // do not write to the console
            formulaEval.ComponentParameter["console_messages"] = "off";

            // do not expand nor collapse the model
            formulaEval.ComponentParameter["expanded"] = expanded ? "true" : "false";

            // do not generate the post processing python scripts
            // FIXME: Why should we generate them ???
            formulaEval.ComponentParameter["do_not_generate_post_processing"] = "true";

            // call the formula evaluator and update all parameters starting from the current object
            formulaEval.InvokeEx(Project, currentobj, selectedObjs, 16);
        }

        #region Paths
        public static readonly String path_Test = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                                               "..\\..\\..\\..",
                                                               "models",
                                                               "ValueFlowTest");

        private String path_XME = Path.Combine(path_Test,
                                              "ValueFlowTest.xme");
        #endregion

        public MgaProject Project { get; private set; }

        public CyPhy.Component ValueFlow { get; private set; }

    }

    public class ValueFlowTest : IUseFixture<ValueFlowTestFixture>
    {
        #region Fixture
        private ValueFlowTestFixture fixture;
        public void SetFixture(ValueFlowTestFixture data)
        {
            fixture = data;
        }
        #endregion

        private void ModelOperation(GME.CSharp.MgaGateway.voidDelegate d)
        {
            var mgaGateway = new MgaGateway(fixture.Project);
            mgaGateway.PerformInTransaction(d);
        }

        private CyPhy.Component compValueFlow
        {
            get
            {
                return fixture.ValueFlow;
            }
        }

        [Fact]
        public void SystemC_FromCustomFormula()
        {
            String nameDomainModel = "SystemCModel";
            String nameParameter = "FromCustomFormula";
            String valueExpected = "5";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void SystemC_FromParameter()
        {
            String nameDomainModel = "SystemCModel";
            String nameParameter = "FromParam";
            String valueExpected = "2";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void SystemC_FromProperty()
        {
            String nameDomainModel = "SystemCModel";
            String nameParameter = "FromProp";
            String valueExpected = "3";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void SystemC_FromSimpleFormula()
        {
            String nameDomainModel = "SystemCModel";
            String nameParameter = "FromSimpleFormula";
            String valueExpected = "5";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void SystemC_StringVal()
        {
            String nameDomainModel = "SystemCModel";
            String nameParameter = "StringVal";
            String valueExpected = "SOMEVALUE";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void EDA_FromCustomFormula()
        {
            String nameDomainModel = "EDAModel";
            String nameParameter = "FromCustomFormula";
            String valueExpected = "5";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void EDA_FromParameter()
        {
            String nameDomainModel = "EDAModel";
            String nameParameter = "FromParam";
            String valueExpected = "2";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void EDA_FromProperty()
        {
            String nameDomainModel = "EDAModel";
            String nameParameter = "FromProp";
            String valueExpected = "3";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void EDA_FromSimpleFormula()
        {
            String nameDomainModel = "EDAModel";
            String nameParameter = "FromSimpleFormula";
            String valueExpected = "5";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void EDA_StringVal()
        {
            String nameDomainModel = "EDAModel";
            String nameParameter = "StringVal";
            String valueExpected = "SOMEVALUE";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void SPICE_FromCustomFormula()
        {
            String nameDomainModel = "SPICEModel";
            String nameParameter = "FromCustomFormula";
            String valueExpected = "5";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void SPICE_FromParameter()
        {
            String nameDomainModel = "SPICEModel";
            String nameParameter = "FromParam";
            String valueExpected = "2";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void SPICE_FromProperty()
        {
            String nameDomainModel = "SPICEModel";
            String nameParameter = "FromProp";
            String valueExpected = "3";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void SPICE_FromSimpleFormula()
        {
            String nameDomainModel = "SPICEModel";
            String nameParameter = "FromSimpleFormula";
            String valueExpected = "5";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        [Fact]
        public void SPICE_StringVal()
        {
            String nameDomainModel = "SPICEModel";
            String nameParameter = "StringVal";
            String valueExpected = "SOMEVALUE";

            CheckParamValue(nameDomainModel, nameParameter, valueExpected);
        }

        private void CheckParamValue(String nameDomainModel, String nameParameter, String valueExpected)
        {
            ModelOperation(delegate
            {
                var domainModel = compValueFlow.AllChildren
                                               .First(c => c.Name == nameDomainModel)
                                               .Impl as MgaModel;
                Assert.NotNull(domainModel);

                MgaFCO param = null;
                foreach (MgaFCO obj in domainModel.ChildFCOs)
                {
                    if (obj.Name == nameParameter)
                    {
                        param = obj;
                        break;
                    }
                }
                Assert.NotNull(param);

                String attrValue = null;
                foreach (MgaAttribute attr in param.Attributes)
                {
                    if (attr.Meta.Name == "Value")
                    {
                        attrValue = attr.StringValue;
                        break;
                    }
                }
                Assert.NotNull(attrValue);

                Assert.Equal(valueExpected, attrValue);
            });
        }
    }
}
