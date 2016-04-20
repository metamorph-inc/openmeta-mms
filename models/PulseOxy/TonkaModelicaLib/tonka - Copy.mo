within ;
package tonka "basic set of models for"
  annotation (uses(Modelica(version="3.2")));
  extends Modelica;
  model HeatGen "Starting point for component heat"
    parameter Modelica.SIunits.HeatCapacity Cap
      "Heat capacity of element (= cp*m)";
      parameter Modelica.SIunits.ThermalConductance Gcond
      "Constant thermal conductance of material";
    Modelica.Thermal.HeatTransfer.Components.HeatCapacitor heatCapacitor(C=Cap)
      annotation (Placement(transformation(extent={{-40,20},{-20,40}})));
    Modelica.Thermal.HeatTransfer.Components.ThermalConductor thermalConductor(G=Gcond)
      annotation (Placement(transformation(extent={{8,24},{28,44}})));
    Modelica.Thermal.HeatTransfer.Sources.PrescribedHeatFlow prescribedHeatFlow
      annotation (Placement(transformation(extent={{-60,-16},{-40,4}})));
    Modelica.Thermal.HeatTransfer.Interfaces.HeatPort_a port_a
      annotation (Placement(transformation(extent={{54,24},{74,44}})));
    Modelica.Blocks.Interfaces.RealInput heatGen
      annotation (Placement(transformation(extent={{-100,-16},{-78,6}})));
  equation
    connect(prescribedHeatFlow.port, heatCapacitor.port) annotation (Line(
        points={{-40,-6},{-30,-6},{-30,20}},
        color={191,0,0},
        smooth=Smooth.None));
    connect(prescribedHeatFlow.port, thermalConductor.port_a) annotation (Line(
        points={{-40,-6},{2,-6},{2,34},{8,34}},
        color={191,0,0},
        smooth=Smooth.None));
    connect(thermalConductor.port_b, port_a) annotation (Line(
        points={{28,34},{64,34}},
        color={191,0,0},
        smooth=Smooth.None));
    connect(prescribedHeatFlow.Q_flow, heatGen) annotation (Line(
        points={{-60,-6},{-65.5,-6},{-65.5,-5},{-89,-5}},
        color={0,0,127},
        smooth=Smooth.None));
    connect(port_a, port_a) annotation (Line(
        points={{64,34},{64,34}},
        color={191,0,0},
        smooth=Smooth.None));
    annotation (Diagram(coordinateSystem(preserveAspectRatio=false, extent={{-100,
              -100},{100,100}}), graphics));
  end HeatGen;

  model HeatSink "Example Heat sink"
    parameter Modelica.SIunits.ThermalConductance G_toBoard
      "Chip to Board, Constant thermal conductance of material,";
    parameter Modelica.SIunits.ThermalConductance G_toEnv
      "Board to Environment, Constant thermal conductance of material";
    parameter Modelica.SIunits.HeatCapacity Cap
      "Board Heat capacity of element (= cp*m)";
    Modelica.Thermal.HeatTransfer.Components.HeatCapacitor heatCapacitor(C=Cap)
      annotation (Placement(transformation(extent={{-16,4},{4,24}})));
    Modelica.Thermal.HeatTransfer.Interfaces.HeatPort_a port_a
      annotation (Placement(transformation(extent={{-74,-2},{-54,18}})));
    Modelica.Thermal.HeatTransfer.Components.ThermalConductor thermalConductor(G=G_toEnv)
      annotation (Placement(transformation(extent={{-6,-62},{14,-42}})));
    Modelica.Thermal.HeatTransfer.Components.ThermalConductor thermalConductor1(G=
         G_toBoard) annotation (Placement(transformation(extent={{-38,-28},{-18,-8}})));
    Modelica.Thermal.HeatTransfer.Sources.FixedTemperature fixedTemperature(T=293.15)
      annotation (Placement(transformation(extent={{-40,-62},{-20,-42}})));
  equation
    connect(port_a, thermalConductor1.port_a) annotation (Line(
        points={{-64,8},{-66,8},{-66,-18},{-38,-18}},
        color={191,0,0},
        smooth=Smooth.None));
    connect(heatCapacitor.port, thermalConductor1.port_b) annotation (Line(
        points={{-6,4},{-6,-18},{-18,-18}},
        color={191,0,0},
        smooth=Smooth.None));
    connect(fixedTemperature.port, thermalConductor.port_a) annotation (Line(
        points={{-20,-52},{-6,-52}},
        color={191,0,0},
        smooth=Smooth.None));
    connect(thermalConductor.port_b, thermalConductor1.port_b) annotation (Line(
        points={{14,-52},{26,-52},{26,-18},{-18,-18}},
        color={191,0,0},
        smooth=Smooth.None));
    annotation (Diagram(coordinateSystem(preserveAspectRatio=false, extent={{-100,
              -100},{100,100}}), graphics));
  end HeatSink;

  model simpleThermalCircuit

    HeatGen heatGen(Cap=10, Gcond=3)
      annotation (Placement(transformation(extent={{-50,24},{-30,44}})));
    HeatSink heatSink(
      G_toBoard=2,
      G_toEnv=6,
      Cap=30)
      annotation (Placement(transformation(extent={{6,26},{26,46}})));
    Modelica.Blocks.Sources.Pulse pulse(
      amplitude=100,
      period=6,
      offset=100)
      annotation (Placement(transformation(extent={{-92,24},{-72,44}})));
  equation
    connect(heatGen.port_a, heatSink.port_a) annotation (Line(
        points={{-33.6,37.4},{-13.8,37.4},{-13.8,36.8},{9.6,36.8}},
        color={191,0,0},
        smooth=Smooth.None));
    connect(pulse.y, heatGen.heatGen) annotation (Line(
        points={{-71,34},{-60,34},{-60,33.5},{-48.9,33.5}},
        color={0,0,127},
        smooth=Smooth.None));
    annotation (Diagram(coordinateSystem(preserveAspectRatio=false, extent={{-100,
              -100},{100,100}}), graphics));
  end simpleThermalCircuit;
end tonka;
