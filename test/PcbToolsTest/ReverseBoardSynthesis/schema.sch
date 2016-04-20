<?xml version="1.0"?>
<eagle xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" version="6.5.0" xmlns="eagle">
  <compatibility />
  <drawing>
    <settings>
      <setting alwaysvectorfont="no" />
      <setting />
    </settings>
    <grid distance="0.01" unitdist="inch" unit="inch" display="yes" altdistance="0.01" altunitdist="inch" altunit="inch" />
    <layers>
      <layer number="1" name="Top" color="4" fill="1" visible="no" active="no" />
      <layer number="16" name="Bottom" color="1" fill="1" visible="no" active="no" />
      <layer number="17" name="Pads" color="2" fill="1" visible="no" active="no" />
      <layer number="18" name="Vias" color="2" fill="1" visible="no" active="no" />
      <layer number="19" name="Unrouted" color="6" fill="1" visible="no" active="no" />
      <layer number="20" name="Dimension" color="15" fill="1" visible="no" active="no" />
      <layer number="21" name="tPlace" color="7" fill="1" visible="no" active="no" />
      <layer number="22" name="bPlace" color="7" fill="1" visible="no" active="no" />
      <layer number="23" name="tOrigins" color="15" fill="1" visible="no" active="no" />
      <layer number="24" name="bOrigins" color="15" fill="1" visible="no" active="no" />
      <layer number="25" name="tNames" color="7" fill="1" visible="no" active="no" />
      <layer number="26" name="bNames" color="7" fill="1" visible="no" active="no" />
      <layer number="27" name="tValues" color="7" fill="1" visible="no" active="no" />
      <layer number="28" name="bValues" color="7" fill="1" visible="no" active="no" />
      <layer number="29" name="tStop" color="7" fill="3" visible="no" active="no" />
      <layer number="30" name="bStop" color="7" fill="6" visible="no" active="no" />
      <layer number="31" name="tCream" color="7" fill="4" visible="no" active="no" />
      <layer number="32" name="bCream" color="7" fill="5" visible="no" active="no" />
      <layer number="33" name="tFinish" color="6" fill="3" visible="no" active="no" />
      <layer number="34" name="bFinish" color="6" fill="6" visible="no" active="no" />
      <layer number="35" name="tGlue" color="7" fill="4" visible="no" active="no" />
      <layer number="36" name="bGlue" color="7" fill="5" visible="no" active="no" />
      <layer number="37" name="tTest" color="7" fill="1" visible="no" active="no" />
      <layer number="38" name="bTest" color="7" fill="1" visible="no" active="no" />
      <layer number="39" name="tKeepout" color="4" fill="11" visible="no" active="no" />
      <layer number="40" name="bKeepout" color="1" fill="11" visible="no" active="no" />
      <layer number="41" name="tRestrict" color="4" fill="10" visible="no" active="no" />
      <layer number="42" name="bRestrict" color="1" fill="10" visible="no" active="no" />
      <layer number="43" name="vRestrict" color="2" fill="10" visible="no" active="no" />
      <layer number="44" name="Drills" color="7" fill="1" visible="no" active="no" />
      <layer number="45" name="Holes" color="7" fill="1" visible="no" active="no" />
      <layer number="46" name="Milling" color="3" fill="1" visible="no" active="no" />
      <layer number="47" name="Measures" color="7" fill="1" visible="no" active="no" />
      <layer number="48" name="Document" color="7" fill="1" visible="no" active="no" />
      <layer number="49" name="Reference" color="7" fill="1" visible="no" active="no" />
      <layer number="51" name="tDocu" color="7" fill="1" visible="no" active="no" />
      <layer number="52" name="bDocu" color="7" fill="1" visible="no" active="no" />
      <layer number="91" name="Nets" color="2" fill="1" />
      <layer number="92" name="Busses" color="1" fill="1" />
      <layer number="93" name="Pins" color="2" fill="1" visible="no" />
      <layer number="94" name="Symbols" color="4" fill="1" />
      <layer number="95" name="Names" color="7" fill="1" />
      <layer number="96" name="Values" color="7" fill="1" />
      <layer number="97" name="Info" color="7" fill="1" />
      <layer number="98" name="Guide" color="6" fill="1" />
    </layers>
    <schematic xrefpart="/%S.%C%R" xreflabel="%F%N/%S.%C%R">
      <description />
      <libraries>
        <library name="mlcc">
          <description />
          <packages>
            <package name="C_0603">
              <description>&lt;B&gt; 0603&lt;/B&gt; (1608 Metric) MLCC Capacitor &lt;P&gt;</description>
              <wire x1="-0.8" y1="0.4" x2="0.8" y2="0.4" width="0.0762" layer="51" />
              <wire x1="0.8" y1="0.4" x2="0.8" y2="-0.4" width="0.0762" layer="51" />
              <wire x1="0.8" y1="-0.4" x2="-0.8" y2="-0.4" width="0.0762" layer="51" />
              <wire x1="-0.8" y1="-0.4" x2="-0.8" y2="0.4" width="0.0762" layer="51" />
              <wire x1="-0.1016" y1="-0.5334" x2="0.1016" y2="-0.5334" width="0.1524" layer="21" />
              <wire x1="-0.1016" y1="0.5334" x2="0.1016" y2="0.5334" width="0.1524" layer="21" />
              <smd name="1" x="-0.9" y="0" dx="1.15" dy="1.1" layer="1" />
              <smd name="2" x="0.9" y="0" dx="1.15" dy="1.1" layer="1" />
              <text x="-1.6" y="0.8" size="1.016" layer="25" font="vector" ratio="15">&gt;NAME</text>
            </package>
            <package name="C_0402">
              <description>&lt;B&gt; 0402&lt;/B&gt; (1005 Metric) MLCC Capacitor &lt;P&gt;</description>
              <wire x1="-0.5" y1="0.25" x2="0.5" y2="0.25" width="0.0762" layer="51" />
              <wire x1="0.5" y1="0.25" x2="0.5" y2="-0.25" width="0.0762" layer="51" />
              <wire x1="-0.5" y1="-0.25" x2="0.5" y2="-0.25" width="0.0762" layer="51" />
              <wire x1="-0.5" y1="-0.25" x2="-0.5" y2="0.25" width="0.0762" layer="51" />
              <smd name="1" x="-0.5" y="0" dx="0.72" dy="0.72" layer="1" />
              <smd name="2" x="0.5" y="0" dx="0.72" dy="0.72" layer="1" />
              <text x="-1" y="0.6" size="0.508" layer="51" font="vector" ratio="15">&gt;NAME</text>
            </package>
          </packages>
          <symbols>
            <symbol name="CAP_NP">
              <description>&lt;B&gt;Capacitor&lt;/B&gt; -- non-polarized</description>
              <wire x1="-1.905" y1="-3.175" x2="0" y2="-3.175" width="0.6096" layer="94" />
              <wire x1="0" y1="-3.175" x2="1.905" y2="-3.175" width="0.6096" layer="94" />
              <wire x1="-1.905" y1="-4.445" x2="0" y2="-4.445" width="0.6096" layer="94" />
              <wire x1="0" y1="-4.445" x2="1.905" y2="-4.445" width="0.6096" layer="94" />
              <wire x1="0" y1="-2.54" x2="0" y2="-3.175" width="0.254" layer="94" />
              <wire x1="0" y1="-5.08" x2="0" y2="-4.445" width="0.254" layer="94" />
              <pin name="P$1" x="0" y="0" visible="off" length="short" direction="pas" rot="R270" />
              <pin name="P$2" x="0" y="-7.62" visible="off" length="short" direction="pas" rot="R90" />
              <text x="-2.54" y="-7.62" size="1.778" layer="96" rot="R90">&gt;VALUE</text>
              <text x="-5.08" y="-7.62" size="1.778" layer="95" rot="R90">&gt;NAME</text>
              <text x="0.508" y="-2.286" size="1.778" layer="95">1</text>
            </symbol>
          </symbols>
          <devicesets>
            <deviceset prefix="C" name="C_0603">
              <description />
              <gates>
                <gate name="G$1" symbol="CAP_NP" x="0" y="0" />
              </gates>
              <devices>
                <device package="C_0603">
                  <connects>
                    <connect gate="G$1" pin="P$1" pad="1" />
                    <connect gate="G$1" pin="P$2" pad="2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
            <deviceset prefix="C" name="C_0402">
              <description />
              <gates>
                <gate name="G$1" symbol="CAP_NP" x="0" y="0" />
              </gates>
              <devices>
                <device package="C_0402">
                  <connects>
                    <connect gate="G$1" pin="P$1" pad="1" />
                    <connect gate="G$1" pin="P$2" pad="2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
          </devicesets>
        </library>
        <library name="Toxic Gas Detector">
          <description />
          <packages>
            <package name="SOT23-3">
              <description />
              <smd name="3" x="0.925" y="0" dx="0.5" dy="0.75" layer="1" rot="R90" />
              <text x="-1.9255" y="1.848" size="1.016" layer="25" font="vector" ratio="15">&gt;NAME</text>
              <circle x="-2.35" y="1.6" radius="0.1524" width="0" layer="21" />
              <smd name="1" x="-0.925" y="0.95" dx="0.5" dy="0.75" layer="1" rot="R90" />
              <smd name="2" x="-0.925" y="-0.95" dx="0.5" dy="0.75" layer="1" rot="R90" />
              <wire x1="-0.3" y1="1.5" x2="0.81" y2="1.5" width="0.1524" layer="21" />
              <wire x1="0.81" y1="1.5" x2="0.81" y2="0.8" width="0.1524" layer="21" />
              <wire x1="-0.3" y1="-1.5" x2="0.806" y2="-1.5" width="0.1524" layer="21" />
              <wire x1="0.806" y1="-1.5" x2="0.806" y2="-0.8" width="0.1524" layer="21" />
              <wire x1="-0.75" y1="0.33" x2="-0.75" y2="-0.33" width="0.1524" layer="21" />
            </package>
            <package name="CO-AX">
              <description />
              <circle x="0" y="0" radius="10.15" width="0.127" layer="21" />
              <circle x="0" y="0" radius="9.05" width="0.127" layer="51" />
              <circle x="0" y="0" radius="8.05" width="0.127" layer="51" />
              <circle x="0" y="0" radius="5.05" width="0.127" layer="21" />
              <text x="-3.41" y="10.36" size="1.27" layer="25">&gt;NAME</text>
              <pad name="WORKER" x="0" y="6.75" drill="0.9" />
              <pad name="COUNTER" x="-6.75" y="0" drill="0.9" />
              <pad name="REFERENCE" x="6.75" y="0" drill="0.9" />
            </package>
            <package name="MSOP-8">
              <description />
              <smd name="P$1" x="-2.025" y="0.975" dx="0.5" dy="1.25" layer="1" rot="R270" stop="no" />
              <rectangle x1="-2.325" y1="0.3" x2="-1.725" y2="1.65" layer="29" rot="R270" />
              <smd name="P$2" x="-2.025" y="0.325" dx="0.5" dy="1.25" layer="1" rot="R270" stop="no" />
              <rectangle x1="-2.325" y1="-0.35" x2="-1.725" y2="1" layer="29" rot="R270" />
              <smd name="P$3" x="-2.025" y="-0.325" dx="0.5" dy="1.25" layer="1" rot="R270" stop="no" />
              <rectangle x1="-2.325" y1="-1" x2="-1.725" y2="0.35" layer="29" rot="R270" />
              <smd name="P$4" x="-2.025" y="-0.975" dx="0.5" dy="1.25" layer="1" rot="R270" stop="no" />
              <rectangle x1="-2.325" y1="-1.65" x2="-1.725" y2="-0.3" layer="29" rot="R270" />
              <smd name="P$5" x="2.025" y="-0.975" dx="0.5" dy="1.25" layer="1" rot="R90" stop="no" />
              <rectangle x1="1.725" y1="-1.65" x2="2.325" y2="-0.3" layer="29" rot="R90" />
              <smd name="P$6" x="2.025" y="-0.325" dx="0.5" dy="1.25" layer="1" rot="R90" stop="no" />
              <rectangle x1="1.725" y1="-1" x2="2.325" y2="0.35" layer="29" rot="R90" />
              <smd name="P$7" x="2.025" y="0.325" dx="0.5" dy="1.25" layer="1" rot="R90" stop="no" />
              <rectangle x1="1.725" y1="-0.35" x2="2.325" y2="1" layer="29" rot="R90" />
              <smd name="P$8" x="2.025" y="0.975" dx="0.5" dy="1.25" layer="1" rot="R90" stop="no" />
              <rectangle x1="1.725" y1="0.3" x2="2.325" y2="1.65" layer="29" rot="R90" />
              <wire x1="1.6" y1="1.6" x2="-1.6" y2="1.6" width="0.127" layer="21" />
              <wire x1="1.6" y1="-1.6" x2="-1.6" y2="-1.6" width="0.127" layer="21" />
              <circle x="-2.9" y="1.6" radius="0.14141875" width="0.127" layer="21" />
              <text x="-3.3" y="1.9" size="1.27" layer="25">&gt;NAME</text>
            </package>
            <package name="MSOP-10">
              <description />
              <smd name="P$1" x="-2.075" y="1" dx="0.4" dy="1.25" layer="1" rot="R270" stop="no" />
              <rectangle x1="-2.305" y1="0.345" x2="-1.845" y2="1.655" layer="29" rot="R270" />
              <smd name="P$2" x="-2.075" y="0.5" dx="0.4" dy="1.25" layer="1" rot="R270" stop="no" />
              <rectangle x1="-2.305" y1="-0.155" x2="-1.845" y2="1.155" layer="29" rot="R270" />
              <smd name="P$3" x="-2.075" y="0" dx="0.4" dy="1.25" layer="1" rot="R270" stop="no" />
              <rectangle x1="-2.305" y1="-0.655" x2="-1.845" y2="0.655" layer="29" rot="R270" />
              <smd name="P$4" x="-2.075" y="-0.5" dx="0.4" dy="1.25" layer="1" rot="R270" stop="no" />
              <rectangle x1="-2.305" y1="-1.155" x2="-1.845" y2="0.155" layer="29" rot="R270" />
              <smd name="P$5" x="-2.075" y="-1" dx="0.4" dy="1.25" layer="1" rot="R270" stop="no" />
              <rectangle x1="-2.305" y1="-1.655" x2="-1.845" y2="-0.345" layer="29" rot="R270" />
              <smd name="P$6" x="2.075" y="-1" dx="0.4" dy="1.25" layer="1" rot="R90" stop="no" />
              <rectangle x1="1.845" y1="-1.655" x2="2.305" y2="-0.345" layer="29" rot="R90" />
              <smd name="P$7" x="2.075" y="-0.5" dx="0.4" dy="1.25" layer="1" rot="R90" stop="no" />
              <rectangle x1="1.845" y1="-1.155" x2="2.305" y2="0.155" layer="29" rot="R90" />
              <smd name="P$8" x="2.075" y="0" dx="0.4" dy="1.25" layer="1" rot="R90" stop="no" />
              <rectangle x1="1.845" y1="-0.655" x2="2.305" y2="0.655" layer="29" rot="R90" />
              <smd name="P$9" x="2.075" y="0.5" dx="0.4" dy="1.25" layer="1" rot="R90" stop="no" />
              <rectangle x1="1.845" y1="-0.155" x2="2.305" y2="1.155" layer="29" rot="R90" />
              <smd name="P$10" x="2.075" y="1" dx="0.4" dy="1.25" layer="1" rot="R90" stop="no" />
              <rectangle x1="1.845" y1="0.345" x2="2.305" y2="1.655" layer="29" rot="R90" />
              <wire x1="1.55" y1="1.55" x2="-1.55" y2="1.55" width="0.127" layer="21" />
              <wire x1="1.55" y1="-1.55" x2="-1.55" y2="-1.55" width="0.127" layer="21" />
              <circle x="-2.9" y="1.6" radius="0.1" width="0.127" layer="21" />
              <text x="-3.2" y="1.8" size="1.27" layer="25">&gt;NAME</text>
            </package>
            <package name="SOT-23">
              <description />
              <smd name="P$1" x="-1.175" y="0.95" dx="0.6" dy="0.85" layer="1" rot="R270" />
              <smd name="P$2" x="-1.175" y="0" dx="0.6" dy="0.85" layer="1" rot="R270" />
              <smd name="P$3" x="-1.175" y="-0.95" dx="0.6" dy="0.85" layer="1" rot="R270" />
              <smd name="P$4" x="1.175" y="-0.95" dx="0.6" dy="0.85" layer="1" rot="R90" />
              <smd name="P$5" x="1.175" y="0" dx="0.6" dy="0.85" layer="1" rot="R90" />
              <smd name="P$6" x="1.175" y="0.95" dx="0.6" dy="0.85" layer="1" rot="R90" />
              <wire x1="0.5" y1="1.5" x2="-0.5" y2="1.5" width="0.127" layer="21" />
              <wire x1="0.5" y1="-1.5" x2="-0.6" y2="-1.5" width="0.127" layer="21" />
              <circle x="-2.2" y="1.3" radius="0.14141875" width="0.127" layer="21" />
              <text x="-3.6" y="1.7" size="1.27" layer="25">&gt;NAME</text>
            </package>
            <package name="SC-70">
              <description />
              <smd name="P$1" x="-1.05" y="0.65" dx="0.4" dy="0.75" layer="1" rot="R270" />
              <smd name="P$2" x="-1.05" y="0" dx="0.4" dy="0.75" layer="1" rot="R270" />
              <smd name="P$3" x="-1.05" y="-0.65" dx="0.4" dy="0.75" layer="1" rot="R270" />
              <smd name="P$4" x="1.05" y="-0.65" dx="0.4" dy="0.75" layer="1" rot="R270" />
              <smd name="P$5" x="1.05" y="0.65" dx="0.4" dy="0.75" layer="1" rot="R270" />
              <text x="-3" y="1.4" size="1.27" layer="25">&gt;NAME</text>
              <circle x="-1.8" y="1.1" radius="0.1" width="0.05" layer="21" />
              <wire x1="1.2" y1="0.13" x2="1.2" y2="-0.13" width="0.05" layer="21" />
              <wire x1="1.1" y1="1.1" x2="-1.1" y2="1.1" width="0.05" layer="21" />
              <wire x1="1.1" y1="-1.1" x2="-1.1" y2="-1.1" width="0.05" layer="21" />
            </package>
          </packages>
          <symbols>
            <symbol name="MMBFJ270">
              <description />
              <wire x1="-3.6576" y1="2.413" x2="-3.6576" y2="-2.54" width="0.254" layer="94" />
              <wire x1="0" y1="1.905" x2="-2.0066" y2="1.905" width="0.1524" layer="94" />
              <wire x1="0" y1="0" x2="0" y2="-1.905" width="0.1524" layer="94" />
              <wire x1="-2.032" y1="-1.905" x2="0" y2="-1.905" width="0.1524" layer="94" />
              <wire x1="0" y1="2.54" x2="0" y2="1.905" width="0.1524" layer="94" />
              <wire x1="0" y1="1.905" x2="2.54" y2="1.905" width="0.1524" layer="94" />
              <wire x1="2.54" y1="-1.905" x2="0" y2="-1.905" width="0.1524" layer="94" />
              <wire x1="0" y1="-1.905" x2="0" y2="-2.54" width="0.1524" layer="94" />
              <wire x1="2.54" y1="-1.905" x2="2.54" y2="-0.762" width="0.1524" layer="94" />
              <wire x1="2.54" y1="-0.762" x2="2.54" y2="1.905" width="0.1524" layer="94" />
              <wire x1="2.54" y1="-0.762" x2="3.175" y2="0.635" width="0.1524" layer="94" />
              <wire x1="3.175" y1="0.635" x2="1.905" y2="0.635" width="0.1524" layer="94" />
              <wire x1="1.905" y1="0.635" x2="2.54" y2="-0.762" width="0.1524" layer="94" />
              <wire x1="3.175" y1="-0.762" x2="2.54" y2="-0.762" width="0.1524" layer="94" />
              <wire x1="2.54" y1="-0.762" x2="1.905" y2="-0.762" width="0.1524" layer="94" />
              <wire x1="1.905" y1="-0.762" x2="1.651" y2="-1.016" width="0.1524" layer="94" />
              <wire x1="3.175" y1="-0.762" x2="3.429" y2="-0.508" width="0.1524" layer="94" />
              <wire x1="0" y1="0" x2="-1.27" y2="0.508" width="0.1524" layer="94" />
              <wire x1="-1.27" y1="0.508" x2="-1.27" y2="-0.508" width="0.1524" layer="94" />
              <wire x1="-1.27" y1="-0.508" x2="0" y2="0" width="0.1524" layer="94" />
              <wire x1="-1.143" y1="0" x2="-2.032" y2="0" width="0.1524" layer="94" />
              <wire x1="-1.143" y1="-0.254" x2="-0.254" y2="0" width="0.3048" layer="94" />
              <wire x1="-0.254" y1="0" x2="-1.143" y2="0.254" width="0.3048" layer="94" />
              <wire x1="-1.143" y1="0.254" x2="-1.143" y2="0" width="0.3048" layer="94" />
              <wire x1="-1.143" y1="0" x2="-0.889" y2="0" width="0.3048" layer="94" />
              <wire x1="-3.81" y1="0" x2="-5.08" y2="0" width="0.1524" layer="94" />
              <circle x="0" y="-1.905" radius="0.127" width="0.4064" layer="94" />
              <circle x="0" y="1.905" radius="0.127" width="0.4064" layer="94" />
              <text x="5.08" y="2.54" size="1.778" layer="95">&gt;NAME</text>
              <text x="5.08" y="0" size="1.778" layer="96">&gt;VALUE</text>
              <text x="-1.27" y="2.54" size="1.778" layer="93">D</text>
              <text x="-1.27" y="-3.556" size="1.778" layer="93">S</text>
              <text x="-5.08" y="-1.27" size="1.778" layer="93">G</text>
              <rectangle x1="-2.794" y1="-2.54" x2="-2.032" y2="-1.27" layer="94" />
              <rectangle x1="-2.794" y1="1.27" x2="-2.032" y2="2.54" layer="94" />
              <rectangle x1="-2.794" y1="-0.889" x2="-2.032" y2="0.889" layer="94" />
              <pin name="G" x="-7.62" y="0" visible="off" length="short" direction="pas" />
              <pin name="D" x="0" y="5.08" visible="off" length="short" direction="pas" rot="R270" />
              <pin name="S" x="0" y="-5.08" visible="off" length="short" direction="pas" rot="R90" />
            </symbol>
            <symbol name="CO-AX">
              <description />
              <circle x="0" y="0" radius="7.184203125" width="0.254" layer="94" />
              <pin name="WE" x="0" y="12.7" length="middle" rot="R270" />
              <pin name="CE" x="12.7" y="0" length="middle" rot="R180" />
              <pin name="RE" x="-12.7" y="0" length="middle" />
            </symbol>
            <symbol name="ADA4528-2ARMZ">
              <description />
              <pin name="-IN" x="-15.24" y="2.54" length="middle" />
              <pin name="NIC1" x="-15.24" y="7.62" length="middle" />
              <pin name="+IN" x="-15.24" y="-2.54" length="middle" />
              <pin name="V-" x="-15.24" y="-7.62" length="middle" />
              <pin name="V+" x="15.24" y="2.54" length="middle" rot="R180" />
              <pin name="NIC3" x="15.24" y="7.62" length="middle" rot="R180" />
              <pin name="OUT" x="15.24" y="-2.54" length="middle" rot="R180" />
              <pin name="NIC2" x="15.24" y="-7.62" length="middle" rot="R180" />
              <wire x1="-10.16" y1="10.16" x2="-10.16" y2="-10.16" width="0.254" layer="94" />
              <wire x1="-10.16" y1="-10.16" x2="10.16" y2="-10.16" width="0.254" layer="94" />
              <wire x1="10.16" y1="-10.16" x2="10.16" y2="10.16" width="0.254" layer="94" />
              <wire x1="-10.16" y1="10.16" x2="10.16" y2="10.16" width="0.254" layer="94" />
              <text x="-10.16" y="15.24" size="1.27" layer="95">&gt;NAME</text>
              <text x="-10.16" y="12.7" size="1.27" layer="96">&gt;VALUE</text>
            </symbol>
            <symbol name="AD5270BRMZ">
              <description />
              <pin name="W" x="-17.78" y="0" length="middle" />
              <pin name="A" x="-17.78" y="5.08" length="middle" />
              <pin name="VDD" x="-17.78" y="10.16" length="middle" />
              <pin name="VSS" x="-17.78" y="-5.08" length="middle" />
              <pin name="EXT_CAP" x="-17.78" y="-10.16" length="middle" />
              <pin name="DIN" x="17.78" y="0" length="middle" rot="R180" />
              <pin name="SCLK" x="17.78" y="5.08" length="middle" rot="R180" />
              <pin name="SYNC" x="17.78" y="10.16" length="middle" rot="R180" />
              <pin name="SDO" x="17.78" y="-5.08" length="middle" rot="R180" />
              <pin name="GND" x="17.78" y="-10.16" length="middle" rot="R180" />
              <wire x1="-12.7" y1="12.7" x2="-12.7" y2="-12.7" width="0.254" layer="94" />
              <wire x1="-12.7" y1="-12.7" x2="12.7" y2="-12.7" width="0.254" layer="94" />
              <wire x1="12.7" y1="-12.7" x2="12.7" y2="12.7" width="0.254" layer="94" />
              <wire x1="12.7" y1="12.7" x2="-12.7" y2="12.7" width="0.254" layer="94" />
              <text x="-12.7" y="17.78" size="1.27" layer="95">&gt;NAME</text>
              <text x="-12.7" y="15.24" size="1.27" layer="96">&gt;VALUE</text>
            </symbol>
            <symbol name="ARD3412">
              <description />
              <pin name="GND_SENSE" x="-22.86" y="0" length="middle" />
              <pin name="GND_FORCE" x="-22.86" y="5.08" length="middle" />
              <pin name="ENABLE" x="-22.86" y="-5.08" length="middle" />
              <pin name="VOUT_SENSE" x="22.86" y="0" length="middle" rot="R180" />
              <pin name="VOUT_FORCE" x="22.86" y="5.08" length="middle" rot="R180" />
              <pin name="VIN" x="22.86" y="-5.08" length="middle" rot="R180" />
              <wire x1="-17.78" y1="7.62" x2="-17.78" y2="-7.62" width="0.254" layer="94" />
              <wire x1="-17.78" y1="-7.62" x2="17.78" y2="-7.62" width="0.254" layer="94" />
              <wire x1="17.78" y1="-7.62" x2="17.78" y2="7.62" width="0.254" layer="94" />
              <wire x1="17.78" y1="7.62" x2="-17.78" y2="7.62" width="0.254" layer="94" />
              <text x="-17.78" y="12.7" size="1.27" layer="95">&gt;NAME</text>
              <text x="-17.78" y="10.16" size="1.27" layer="96">&gt;VALUE</text>
            </symbol>
            <symbol name="AD8500AKSZ-R2">
              <description />
              <pin name="V-" x="-15.24" y="0" length="middle" />
              <pin name="OUT_A" x="-15.24" y="5.08" length="middle" />
              <pin name="+IN" x="-15.24" y="-5.08" length="middle" />
              <pin name="V+" x="15.24" y="5.08" length="middle" rot="R180" />
              <pin name="-IN" x="15.24" y="-5.08" length="middle" rot="R180" />
              <wire x1="-10.16" y1="7.62" x2="-10.16" y2="-7.62" width="0.254" layer="94" />
              <wire x1="-10.16" y1="-7.62" x2="10.16" y2="-7.62" width="0.254" layer="94" />
              <wire x1="10.16" y1="-7.62" x2="10.16" y2="7.62" width="0.254" layer="94" />
              <wire x1="10.16" y1="7.62" x2="-10.16" y2="7.62" width="0.254" layer="94" />
              <text x="-10.16" y="12.7" size="1.27" layer="95">&gt;NAME</text>
              <text x="-10.16" y="10.16" size="1.27" layer="96">&gt;VALUE</text>
            </symbol>
            <symbol name="AD7790BRMZ">
              <description />
              <pin name="AIN(+)" x="-17.78" y="0" length="middle" />
              <pin name="CS" x="-17.78" y="5.08" length="middle" />
              <pin name="SCLK" x="-17.78" y="10.16" length="middle" />
              <pin name="AIN(-)" x="-17.78" y="-5.08" length="middle" />
              <pin name="REF(+)" x="-17.78" y="-10.16" length="middle" />
              <pin name="VDD" x="17.78" y="0" length="middle" rot="R180" />
              <pin name="DOUT/RDY" x="17.78" y="5.08" length="middle" rot="R180" />
              <pin name="DIN" x="17.78" y="10.16" length="middle" rot="R180" />
              <pin name="GND" x="17.78" y="-5.08" length="middle" rot="R180" />
              <pin name="REF(-)" x="17.78" y="-10.16" length="middle" rot="R180" />
              <wire x1="-12.7" y1="12.7" x2="-12.7" y2="-12.7" width="0.254" layer="94" />
              <wire x1="-12.7" y1="-12.7" x2="12.7" y2="-12.7" width="0.254" layer="94" />
              <wire x1="12.7" y1="-12.7" x2="12.7" y2="12.7" width="0.254" layer="94" />
              <wire x1="12.7" y1="12.7" x2="-12.7" y2="12.7" width="0.254" layer="94" />
              <text x="-12.7" y="17.78" size="1.27" layer="95">&gt;NAME</text>
              <text x="-12.7" y="15.24" size="1.27" layer="96">&gt;VALUE</text>
            </symbol>
          </symbols>
          <devicesets>
            <deviceset name="MMBFJ270">
              <description />
              <gates>
                <gate name="G$1" symbol="MMBFJ270" x="0" y="0" />
              </gates>
              <devices>
                <device package="SOT23-3">
                  <connects>
                    <connect gate="G$1" pin="D" pad="1" />
                    <connect gate="G$1" pin="G" pad="3" />
                    <connect gate="G$1" pin="S" pad="2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
            <deviceset name="CO-AX">
              <description />
              <gates>
                <gate name="G$1" symbol="CO-AX" x="0" y="0" />
              </gates>
              <devices>
                <device package="CO-AX">
                  <connects>
                    <connect gate="G$1" pin="CE" pad="COUNTER" />
                    <connect gate="G$1" pin="RE" pad="REFERENCE" />
                    <connect gate="G$1" pin="WE" pad="WORKER" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
            <deviceset name="ADA4528-2ARMZ">
              <description />
              <gates>
                <gate name="G$1" symbol="ADA4528-2ARMZ" x="0" y="0" />
              </gates>
              <devices>
                <device package="MSOP-8">
                  <connects>
                    <connect gate="G$1" pin="+IN" pad="P$3" />
                    <connect gate="G$1" pin="-IN" pad="P$2" />
                    <connect gate="G$1" pin="NIC1" pad="P$1" />
                    <connect gate="G$1" pin="NIC2" pad="P$5" />
                    <connect gate="G$1" pin="NIC3" pad="P$8" />
                    <connect gate="G$1" pin="OUT" pad="P$6" />
                    <connect gate="G$1" pin="V+" pad="P$7" />
                    <connect gate="G$1" pin="V-" pad="P$4" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
            <deviceset name="AD5270BRMZ-20">
              <description />
              <gates>
                <gate name="G$1" symbol="AD5270BRMZ" x="0" y="0" />
              </gates>
              <devices>
                <device package="MSOP-10">
                  <connects>
                    <connect gate="G$1" pin="A" pad="P$2" />
                    <connect gate="G$1" pin="DIN" pad="P$8" />
                    <connect gate="G$1" pin="EXT_CAP" pad="P$5" />
                    <connect gate="G$1" pin="GND" pad="P$6" />
                    <connect gate="G$1" pin="SCLK" pad="P$9" />
                    <connect gate="G$1" pin="SDO" pad="P$7" />
                    <connect gate="G$1" pin="SYNC" pad="P$10" />
                    <connect gate="G$1" pin="VDD" pad="P$1" />
                    <connect gate="G$1" pin="VSS" pad="P$4" />
                    <connect gate="G$1" pin="W" pad="P$3" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
            <deviceset name="AD3412ARJZ">
              <description />
              <gates>
                <gate name="G$1" symbol="ARD3412" x="0" y="0" />
              </gates>
              <devices>
                <device package="SOT-23">
                  <connects>
                    <connect gate="G$1" pin="ENABLE" pad="P$3" />
                    <connect gate="G$1" pin="GND_FORCE" pad="P$1" />
                    <connect gate="G$1" pin="GND_SENSE" pad="P$2" />
                    <connect gate="G$1" pin="VIN" pad="P$4" />
                    <connect gate="G$1" pin="VOUT_FORCE" pad="P$6" />
                    <connect gate="G$1" pin="VOUT_SENSE" pad="P$5" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
            <deviceset name="AD8500AKSZ-R2">
              <description />
              <gates>
                <gate name="G$1" symbol="AD8500AKSZ-R2" x="0" y="0" />
              </gates>
              <devices>
                <device package="SC-70">
                  <connects>
                    <connect gate="G$1" pin="+IN" pad="P$3" />
                    <connect gate="G$1" pin="-IN" pad="P$4" />
                    <connect gate="G$1" pin="OUT_A" pad="P$1" />
                    <connect gate="G$1" pin="V+" pad="P$5" />
                    <connect gate="G$1" pin="V-" pad="P$2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
            <deviceset name="AD7790BRMZ">
              <description />
              <gates>
                <gate name="G$1" symbol="AD7790BRMZ" x="0" y="0" />
              </gates>
              <devices>
                <device package="MSOP-10">
                  <connects>
                    <connect gate="G$1" pin="AIN(+)" pad="P$3" />
                    <connect gate="G$1" pin="AIN(-)" pad="P$4" />
                    <connect gate="G$1" pin="CS" pad="P$2" />
                    <connect gate="G$1" pin="DIN" pad="P$10" />
                    <connect gate="G$1" pin="DOUT/RDY" pad="P$9" />
                    <connect gate="G$1" pin="GND" pad="P$7" />
                    <connect gate="G$1" pin="REF(+)" pad="P$5" />
                    <connect gate="G$1" pin="REF(-)" pad="P$6" />
                    <connect gate="G$1" pin="SCLK" pad="P$1" />
                    <connect gate="G$1" pin="VDD" pad="P$8" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
          </devicesets>
        </library>
        <library name="resistor">
          <description />
          <packages>
            <package name="R_0603">
              <description>&lt;B&gt;
0603
&lt;/B&gt; SMT inch-code chip resistor package&lt;P&gt;
Derived from dimensions and tolerances
in the &lt;B&gt; &lt;A HREF="http://www.samsungsem.com/global/support/library/product-catalog/__icsFiles/afieldfile/2015/01/12/CHIP_RESISTOR_150112_1.pdf"&gt;Samsung Thick Film Chip Resistor Catalog&lt;/A&gt;&lt;/B&gt;
dated December 2014,
for 
general-purpose chip resistor
reflow soldering.</description>
              <wire x1="-0.1" y1="0.3" x2="0.1" y2="0.3" width="0.1524" layer="21" />
              <wire x1="-0.1" y1="-0.3" x2="0.1" y2="-0.3" width="0.1524" layer="21" />
              <smd name="P$1" x="-0.8" y="0" dx="0.8" dy="0.8" layer="1" />
              <smd name="P$2" x="0.8" y="0" dx="0.8" dy="0.8" layer="1" />
              <text x="-1.3" y="0.8" size="1.016" layer="25" font="vector" ratio="15">&gt;NAME</text>
              <wire x1="-0.8" y1="0.4" x2="0.8" y2="0.4" width="0.0762" layer="51" />
              <wire x1="0.8" y1="0.4" x2="0.8" y2="-0.4" width="0.0762" layer="51" />
              <wire x1="0.8" y1="-0.4" x2="-0.8" y2="-0.4" width="0.0762" layer="51" />
              <wire x1="-0.8" y1="-0.4" x2="-0.8" y2="0.4" width="0.0762" layer="51" />
            </package>
            <package name="R_0402">
              <description>&lt;B&gt;
0402
&lt;/B&gt; SMT inch-code chip resistor package&lt;P&gt;
Derived from dimensions and tolerances
in the &lt;B&gt; &lt;A HREF="http://www.samsungsem.com/global/support/library/product-catalog/__icsFiles/afieldfile/2015/01/12/CHIP_RESISTOR_150112_1.pdf"&gt;Samsung Thick Film Chip Resistor Catalog&lt;/A&gt;&lt;/B&gt;
dated December 2014,
for 
general-purpose chip resistor
reflow soldering.</description>
              <smd name="P$1" x="-0.55" y="0" dx="0.6" dy="0.5" layer="1" />
              <smd name="P$2" x="0.55" y="0" dx="0.6" dy="0.5" layer="1" />
              <text x="-0.8" y="0.4" size="0.508" layer="51" font="vector" ratio="15">&gt;NAME</text>
              <wire x1="-0.5" y1="0.25" x2="0.5" y2="0.25" width="0.0762" layer="51" />
              <wire x1="0.5" y1="0.25" x2="0.5" y2="-0.25" width="0.0762" layer="51" />
              <wire x1="0.5" y1="-0.25" x2="-0.5" y2="-0.25" width="0.0762" layer="51" />
              <wire x1="-0.5" y1="-0.25" x2="-0.5" y2="0.25" width="0.0762" layer="51" />
            </package>
          </packages>
          <symbols>
            <symbol name="R">
              <description>&lt;B&gt;Resistor&lt;/B&gt;</description>
              <wire x1="-2.54" y1="0" x2="-2.159" y2="1.016" width="0.2032" layer="94" />
              <wire x1="-2.159" y1="1.016" x2="-1.524" y2="-1.016" width="0.2032" layer="94" />
              <wire x1="-1.524" y1="-1.016" x2="-0.889" y2="1.016" width="0.2032" layer="94" />
              <wire x1="-0.889" y1="1.016" x2="-0.254" y2="-1.016" width="0.2032" layer="94" />
              <wire x1="-0.254" y1="-1.016" x2="0.381" y2="1.016" width="0.2032" layer="94" />
              <wire x1="0.381" y1="1.016" x2="1.016" y2="-1.016" width="0.2032" layer="94" />
              <wire x1="1.016" y1="-1.016" x2="1.651" y2="1.016" width="0.2032" layer="94" />
              <wire x1="1.651" y1="1.016" x2="2.286" y2="-1.016" width="0.2032" layer="94" />
              <wire x1="2.286" y1="-1.016" x2="2.54" y2="0" width="0.2032" layer="94" />
              <pin name="1" x="-5.08" y="0" visible="off" length="short" direction="pas" swaplevel="1" />
              <pin name="2" x="5.08" y="0" visible="off" length="short" direction="pas" swaplevel="1" rot="R180" />
              <text x="-3.81" y="1.4986" size="1.778" layer="95">&gt;NAME</text>
              <text x="-3.81" y="-3.302" size="1.778" layer="96">&gt;VALUE</text>
            </symbol>
          </symbols>
          <devicesets>
            <deviceset prefix="R" name="RESISTOR_0603">
              <description />
              <gates>
                <gate name="G$1" symbol="R" x="0" y="0" />
              </gates>
              <devices>
                <device package="R_0603">
                  <connects>
                    <connect gate="G$1" pin="1" pad="P$1" />
                    <connect gate="G$1" pin="2" pad="P$2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
            <deviceset prefix="R" name="RESISTOR_0402">
              <description />
              <gates>
                <gate name="G$1" symbol="R" x="0" y="0" />
              </gates>
              <devices>
                <device package="R_0402">
                  <connects>
                    <connect gate="G$1" pin="1" pad="P$1" />
                    <connect gate="G$1" pin="2" pad="P$2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
          </devicesets>
        </library>
      </libraries>
      <attributes />
      <variantdefs />
      <classes>
        <class number="0" name="default" />
      </classes>
      <parts>
        <part device="" value="TMK107B7105KA-T" name="C1" library="mlcc" deviceset="C_0603" />
        <part device="" value="GCM188R71E104KA57D" name="C10" library="mlcc" deviceset="C_0603" />
        <part device="" value="CL05B562KB5NNNC" name="C14" library="mlcc" deviceset="C_0402" />
        <part device="" value="CL10B203KB8NNNC" name="C2" library="mlcc" deviceset="C_0603" />
        <part device="" value="CL10B203KB8NNNC" name="C3" library="mlcc" deviceset="C_0603" />
        <part device="" value="CL10B203KB8NNNC" name="C4" library="mlcc" deviceset="C_0603" />
        <part device="" value="TMK107B7105KA-T" name="C6" library="mlcc" deviceset="C_0603" />
        <part device="" value="GCM188R71E104KA57D" name="C8" library="mlcc" deviceset="C_0603" />
        <part device="" value="C1005X5R0J106M050BC" name="C9" library="mlcc" deviceset="C_0402" />
        <part device="" value="MMBFJ270" name="Q1" library="Toxic Gas Detector" deviceset="MMBFJ270" />
        <part device="" value="CR0603-JW-105ELF" name="R1" library="resistor" deviceset="RESISTOR_0603" />
        <part device="" value="CRCW06033K30JNEA" name="R10" library="resistor" deviceset="RESISTOR_0603" />
        <part device="" value="ERJ-2GEJ151X" name="R12" library="resistor" deviceset="RESISTOR_0402" />
        <part device="" value="ERJ-2GEJ330X" name="R2" library="resistor" deviceset="RESISTOR_0402" />
        <part device="" value="RT0603DRD0712K4L" name="R3" library="resistor" deviceset="RESISTOR_0603" />
        <part device="" value="RT0603DRD0712K4L" name="R4" library="resistor" deviceset="RESISTOR_0603" />
        <part device="" value="RT0603DRD0712K4L" name="R5" library="resistor" deviceset="RESISTOR_0603" />
        <part device="" value="RT0603DRD0712K4L" name="R6" library="resistor" deviceset="RESISTOR_0603" />
        <part device="" value="ESR01MZPJ104" name="R8" library="resistor" deviceset="RESISTOR_0402" />
        <part device="" value="CRCW06033K30JNEA" name="R9" library="resistor" deviceset="RESISTOR_0603" />
        <part device="" value="CO-AX" name="U1" library="Toxic Gas Detector" deviceset="CO-AX" />
        <part device="" value="ADA4528-2ARMZ" name="U2-A" library="Toxic Gas Detector" deviceset="ADA4528-2ARMZ" />
        <part device="" value="ADA4528-2ARMZ" name="U2-B" library="Toxic Gas Detector" deviceset="ADA4528-2ARMZ" />
        <part device="" value="AD5270BRMZ-20" name="U3" library="Toxic Gas Detector" deviceset="AD5270BRMZ-20" />
        <part device="" value="AD3412ARJZ" name="U4" library="Toxic Gas Detector" deviceset="AD3412ARJZ" />
        <part device="" value="AD8500AKSZ-R2" name="U5" library="Toxic Gas Detector" deviceset="AD8500AKSZ-R2" />
        <part device="" value="AD7790BRMZ" name="U8" library="Toxic Gas Detector" deviceset="AD7790BRMZ" />
      </parts>
      <sheets>
        <sheet>
          <description />
          <plain />
          <instances>
            <instance y="73.66" part="C1" gate="G$1" x="30.48" />
            <instance y="50.80" part="C10" gate="G$1" x="22.86" />
            <instance y="226.06" part="C14" gate="G$1" x="218.44" />
            <instance y="114.30" part="C2" gate="G$1" x="93.98" />
            <instance y="200.66" part="C3" gate="G$1" x="180.34" />
            <instance y="233.68" part="C4" gate="G$1" x="226.06" />
            <instance y="55.88" part="C6" gate="G$1" x="30.48" />
            <instance y="96.52" part="C8" gate="G$1" x="55.88" />
            <instance y="200.66" part="C9" gate="G$1" x="172.72" />
            <instance y="251.46" part="Q1" gate="G$1" x="256.54" />
            <instance y="213.36" part="R1" gate="G$1" x="223.52" />
            <instance y="220.98" part="R10" gate="G$1" x="231.14" />
            <instance y="264.16" part="R12" gate="G$1" x="256.54" />
            <instance y="38.10" part="R2" gate="G$1" x="17.78" />
            <instance y="91.44" part="R3" gate="G$1" x="43.18" />
            <instance y="154.94" part="R4" gate="G$1" x="142.24" />
            <instance y="187.96" part="R5" gate="G$1" x="160.02" />
            <instance y="246.38" part="R6" gate="G$1" x="238.76" />
            <instance y="187.96" part="R8" gate="G$1" x="177.80" />
            <instance y="264.16" part="R9" gate="G$1" x="274.32" />
            <instance y="137.16" part="U1" gate="G$1" x="149.86" />
            <instance y="223.52" part="U2-A" gate="G$1" x="195.58" />
            <instance y="119.38" part="U2-B" gate="G$1" x="71.12" />
            <instance y="139.70" part="U3" gate="G$1" x="111.76" />
            <instance y="76.20" part="U4" gate="G$1" x="60.96" />
            <instance y="279.40" part="U5" gate="G$1" x="284.48" />
            <instance y="167.64" part="U8" gate="G$1" x="172.72" />
          </instances>
          <busses />
          <nets>
            <net name="N$0">
              <segment>
                <wire x1="30.48" y1="73.66" x2="30.48" y2="76.20" width="0.3" layer="91" />
                <label x="30.48" y="76.20" size="1.27" layer="95" />
                <pinref part="C1" gate="G$1" pin="P$1" />
              </segment>
              <segment>
                <wire x1="93.98" y1="134.62" x2="91.44" y2="134.62" width="0.3" layer="91" />
                <label x="91.44" y="134.62" size="1.27" layer="95" />
                <pinref part="U3" gate="G$1" pin="VSS" />
              </segment>
              <segment>
                <wire x1="269.24" y1="279.40" x2="266.70" y2="279.40" width="0.3" layer="91" />
                <label x="266.70" y="279.40" size="1.27" layer="95" />
                <pinref part="U5" gate="G$1" pin="V-" />
              </segment>
              <segment>
                <wire x1="190.50" y1="162.56" x2="193.04" y2="162.56" width="0.3" layer="91" />
                <label x="193.04" y="162.56" size="1.27" layer="95" />
                <pinref part="U8" gate="G$1" pin="GND" />
              </segment>
              <segment>
                <wire x1="190.50" y1="157.48" x2="193.04" y2="157.48" width="0.3" layer="91" />
                <label x="193.04" y="157.48" size="1.27" layer="95" />
                <pinref part="U8" gate="G$1" pin="REF(-)" />
              </segment>
              <segment>
                <wire x1="218.44" y1="226.06" x2="218.44" y2="228.60" width="0.3" layer="91" />
                <label x="218.44" y="228.60" size="1.27" layer="95" />
                <pinref part="C14" gate="G$1" pin="P$1" />
              </segment>
              <segment>
                <wire x1="172.72" y1="200.66" x2="172.72" y2="203.20" width="0.3" layer="91" />
                <label x="172.72" y="203.20" size="1.27" layer="95" />
                <pinref part="C9" gate="G$1" pin="P$1" />
              </segment>
              <segment>
                <wire x1="55.88" y1="111.76" x2="53.34" y2="111.76" width="0.3" layer="91" />
                <label x="53.34" y="111.76" size="1.27" layer="95" />
                <pinref part="U2-B" gate="G$1" pin="V-" />
              </segment>
              <segment>
                <wire x1="22.86" y1="43.18" x2="22.86" y2="40.64" width="0.3" layer="91" />
                <label x="22.86" y="40.64" size="1.27" layer="95" />
                <pinref part="C10" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="38.10" y1="81.28" x2="35.56" y2="81.28" width="0.3" layer="91" />
                <label x="35.56" y="81.28" size="1.27" layer="95" />
                <pinref part="U4" gate="G$1" pin="GND_FORCE" />
              </segment>
              <segment>
                <wire x1="38.10" y1="76.20" x2="35.56" y2="76.20" width="0.3" layer="91" />
                <label x="35.56" y="76.20" size="1.27" layer="95" />
                <pinref part="U4" gate="G$1" pin="GND_SENSE" />
              </segment>
              <segment>
                <wire x1="55.88" y1="96.52" x2="55.88" y2="99.06" width="0.3" layer="91" />
                <label x="55.88" y="99.06" size="1.27" layer="95" />
                <pinref part="C8" gate="G$1" pin="P$1" />
              </segment>
              <segment>
                <wire x1="180.34" y1="215.90" x2="177.80" y2="215.90" width="0.3" layer="91" />
                <label x="177.80" y="215.90" size="1.27" layer="95" />
                <pinref part="U2-A" gate="G$1" pin="V-" />
              </segment>
              <segment>
                <wire x1="129.54" y1="129.54" x2="132.08" y2="129.54" width="0.3" layer="91" />
                <label x="132.08" y="129.54" size="1.27" layer="95" />
                <pinref part="U3" gate="G$1" pin="GND" />
              </segment>
              <segment>
                <wire x1="30.48" y1="55.88" x2="30.48" y2="58.42" width="0.3" layer="91" />
                <label x="30.48" y="58.42" size="1.27" layer="95" />
                <pinref part="C6" gate="G$1" pin="P$1" />
              </segment>
            </net>
            <net name="N$1">
              <segment>
                <wire x1="30.48" y1="66.04" x2="30.48" y2="63.50" width="0.3" layer="91" />
                <label x="30.48" y="63.50" size="1.27" layer="95" />
                <pinref part="C1" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="93.98" y1="129.54" x2="91.44" y2="129.54" width="0.3" layer="91" />
                <label x="91.44" y="129.54" size="1.27" layer="95" />
                <pinref part="U3" gate="G$1" pin="EXT_CAP" />
              </segment>
            </net>
            <net name="N$2">
              <segment>
                <wire x1="22.86" y1="50.80" x2="22.86" y2="53.34" width="0.3" layer="91" />
                <label x="22.86" y="53.34" size="1.27" layer="95" />
                <pinref part="C10" gate="G$1" pin="P$1" />
              </segment>
              <segment>
                <wire x1="38.10" y1="71.12" x2="35.56" y2="71.12" width="0.3" layer="91" />
                <label x="35.56" y="71.12" size="1.27" layer="95" />
                <pinref part="U4" gate="G$1" pin="ENABLE" />
              </segment>
              <segment>
                <wire x1="83.82" y1="71.12" x2="86.36" y2="71.12" width="0.3" layer="91" />
                <label x="86.36" y="71.12" size="1.27" layer="95" />
                <pinref part="U4" gate="G$1" pin="VIN" />
              </segment>
              <segment>
                <wire x1="86.36" y1="121.92" x2="88.90" y2="121.92" width="0.3" layer="91" />
                <label x="88.90" y="121.92" size="1.27" layer="95" />
                <pinref part="U2-B" gate="G$1" pin="V+" />
              </segment>
              <segment>
                <wire x1="299.72" y1="284.48" x2="302.26" y2="284.48" width="0.3" layer="91" />
                <label x="302.26" y="284.48" size="1.27" layer="95" />
                <pinref part="U5" gate="G$1" pin="V+" />
              </segment>
              <segment>
                <wire x1="190.50" y1="167.64" x2="193.04" y2="167.64" width="0.3" layer="91" />
                <label x="193.04" y="167.64" size="1.27" layer="95" />
                <pinref part="U8" gate="G$1" pin="VDD" />
              </segment>
              <segment>
                <wire x1="218.44" y1="213.36" x2="215.90" y2="213.36" width="0.3" layer="91" />
                <label x="215.90" y="213.36" size="1.27" layer="95" />
                <pinref part="R1" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="210.82" y1="226.06" x2="213.36" y2="226.06" width="0.3" layer="91" />
                <label x="213.36" y="226.06" size="1.27" layer="95" />
                <pinref part="U2-A" gate="G$1" pin="V+" />
              </segment>
              <segment>
                <wire x1="30.48" y1="48.26" x2="30.48" y2="45.72" width="0.3" layer="91" />
                <label x="30.48" y="45.72" size="1.27" layer="95" />
                <pinref part="C6" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="93.98" y1="149.86" x2="91.44" y2="149.86" width="0.3" layer="91" />
                <label x="91.44" y="149.86" size="1.27" layer="95" />
                <pinref part="U3" gate="G$1" pin="VDD" />
              </segment>
            </net>
            <net name="N$3">
              <segment>
                <wire x1="218.44" y1="218.44" x2="218.44" y2="215.90" width="0.3" layer="91" />
                <label x="218.44" y="215.90" size="1.27" layer="95" />
                <pinref part="C14" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="251.46" y1="264.16" x2="248.92" y2="264.16" width="0.3" layer="91" />
                <label x="248.92" y="264.16" size="1.27" layer="95" />
                <pinref part="R12" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="154.94" y1="167.64" x2="152.40" y2="167.64" width="0.3" layer="91" />
                <label x="152.40" y="167.64" size="1.27" layer="95" />
                <pinref part="U8" gate="G$1" pin="AIN(+)" />
              </segment>
            </net>
            <net name="N$4">
              <segment>
                <wire x1="93.98" y1="106.68" x2="93.98" y2="104.14" width="0.3" layer="91" />
                <label x="93.98" y="104.14" size="1.27" layer="95" />
                <pinref part="C2" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="86.36" y1="116.84" x2="88.90" y2="116.84" width="0.3" layer="91" />
                <label x="88.90" y="116.84" size="1.27" layer="95" />
                <pinref part="U2-B" gate="G$1" pin="OUT" />
              </segment>
              <segment>
                <wire x1="172.72" y1="187.96" x2="170.18" y2="187.96" width="0.3" layer="91" />
                <label x="170.18" y="187.96" size="1.27" layer="95" />
                <pinref part="R8" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="93.98" y1="144.78" x2="91.44" y2="144.78" width="0.3" layer="91" />
                <label x="91.44" y="144.78" size="1.27" layer="95" />
                <pinref part="U3" gate="G$1" pin="A" />
              </segment>
            </net>
            <net name="N$5">
              <segment>
                <wire x1="93.98" y1="114.30" x2="93.98" y2="116.84" width="0.3" layer="91" />
                <label x="93.98" y="116.84" size="1.27" layer="95" />
                <pinref part="C2" gate="G$1" pin="P$1" />
              </segment>
              <segment>
                <wire x1="93.98" y1="139.70" x2="91.44" y2="139.70" width="0.3" layer="91" />
                <label x="91.44" y="139.70" size="1.27" layer="95" />
                <pinref part="U3" gate="G$1" pin="W" />
              </segment>
              <segment>
                <wire x1="55.88" y1="121.92" x2="53.34" y2="121.92" width="0.3" layer="91" />
                <label x="53.34" y="121.92" size="1.27" layer="95" />
                <pinref part="U2-B" gate="G$1" pin="-IN" />
              </segment>
              <segment>
                <wire x1="22.86" y1="38.10" x2="25.40" y2="38.10" width="0.3" layer="91" />
                <label x="25.40" y="38.10" size="1.27" layer="95" />
                <pinref part="R2" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$6">
              <segment>
                <wire x1="180.34" y1="193.04" x2="180.34" y2="190.50" width="0.3" layer="91" />
                <label x="180.34" y="190.50" size="1.27" layer="95" />
                <pinref part="C3" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="210.82" y1="220.98" x2="213.36" y2="220.98" width="0.3" layer="91" />
                <label x="213.36" y="220.98" size="1.27" layer="95" />
                <pinref part="U2-A" gate="G$1" pin="OUT" />
              </segment>
              <segment>
                <wire x1="162.56" y1="137.16" x2="165.10" y2="137.16" width="0.3" layer="91" />
                <label x="165.10" y="137.16" size="1.27" layer="95" />
                <pinref part="U1" gate="G$1" pin="CE" />
              </segment>
              <segment>
                <wire x1="226.06" y1="233.68" x2="226.06" y2="236.22" width="0.3" layer="91" />
                <label x="226.06" y="236.22" size="1.27" layer="95" />
                <pinref part="C4" gate="G$1" pin="P$1" />
              </segment>
            </net>
            <net name="N$7">
              <segment>
                <wire x1="180.34" y1="200.66" x2="180.34" y2="203.20" width="0.3" layer="91" />
                <label x="180.34" y="203.20" size="1.27" layer="95" />
                <pinref part="C3" gate="G$1" pin="P$1" />
              </segment>
              <segment>
                <wire x1="154.94" y1="187.96" x2="152.40" y2="187.96" width="0.3" layer="91" />
                <label x="152.40" y="187.96" size="1.27" layer="95" />
                <pinref part="R5" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="180.34" y1="226.06" x2="177.80" y2="226.06" width="0.3" layer="91" />
                <label x="177.80" y="226.06" size="1.27" layer="95" />
                <pinref part="U2-A" gate="G$1" pin="-IN" />
              </segment>
            </net>
            <net name="N$8">
              <segment>
                <wire x1="226.06" y1="226.06" x2="226.06" y2="223.52" width="0.3" layer="91" />
                <label x="226.06" y="223.52" size="1.27" layer="95" />
                <pinref part="C4" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="165.10" y1="187.96" x2="167.64" y2="187.96" width="0.3" layer="91" />
                <label x="167.64" y="187.96" size="1.27" layer="95" />
                <pinref part="R5" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="233.68" y1="246.38" x2="231.14" y2="246.38" width="0.3" layer="91" />
                <label x="231.14" y="246.38" size="1.27" layer="95" />
                <pinref part="R6" gate="G$1" pin="1" />
              </segment>
            </net>
            <net name="N$9">
              <segment>
                <wire x1="55.88" y1="88.90" x2="55.88" y2="86.36" width="0.3" layer="91" />
                <label x="55.88" y="86.36" size="1.27" layer="95" />
                <pinref part="C8" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="83.82" y1="81.28" x2="86.36" y2="81.28" width="0.3" layer="91" />
                <label x="86.36" y="81.28" size="1.27" layer="95" />
                <pinref part="U4" gate="G$1" pin="VOUT_FORCE" />
              </segment>
              <segment>
                <wire x1="83.82" y1="76.20" x2="86.36" y2="76.20" width="0.3" layer="91" />
                <label x="86.36" y="76.20" size="1.27" layer="95" />
                <pinref part="U4" gate="G$1" pin="VOUT_SENSE" />
              </segment>
              <segment>
                <wire x1="154.94" y1="157.48" x2="152.40" y2="157.48" width="0.3" layer="91" />
                <label x="152.40" y="157.48" size="1.27" layer="95" />
                <pinref part="U8" gate="G$1" pin="REF(+)" />
              </segment>
              <segment>
                <wire x1="154.94" y1="162.56" x2="152.40" y2="162.56" width="0.3" layer="91" />
                <label x="152.40" y="162.56" size="1.27" layer="95" />
                <pinref part="U8" gate="G$1" pin="AIN(-)" />
              </segment>
              <segment>
                <wire x1="38.10" y1="91.44" x2="35.56" y2="91.44" width="0.3" layer="91" />
                <label x="35.56" y="91.44" size="1.27" layer="95" />
                <pinref part="R3" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="137.16" y1="154.94" x2="134.62" y2="154.94" width="0.3" layer="91" />
                <label x="134.62" y="154.94" size="1.27" layer="95" />
                <pinref part="R4" gate="G$1" pin="1" />
              </segment>
            </net>
            <net name="N$10">
              <segment>
                <wire x1="172.72" y1="193.04" x2="172.72" y2="190.50" width="0.3" layer="91" />
                <label x="172.72" y="190.50" size="1.27" layer="95" />
                <pinref part="C9" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="182.88" y1="187.96" x2="185.42" y2="187.96" width="0.3" layer="91" />
                <label x="185.42" y="187.96" size="1.27" layer="95" />
                <pinref part="R8" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="226.06" y1="220.98" x2="223.52" y2="220.98" width="0.3" layer="91" />
                <label x="223.52" y="220.98" size="1.27" layer="95" />
                <pinref part="R10" gate="G$1" pin="1" />
              </segment>
            </net>
            <net name="N$11">
              <segment>
                <wire x1="256.54" y1="256.54" x2="256.54" y2="259.08" width="0.3" layer="91" />
                <label x="256.54" y="259.08" size="1.27" layer="95" />
                <pinref part="Q1" gate="G$1" pin="D" />
              </segment>
              <segment>
                <wire x1="243.84" y1="246.38" x2="246.38" y2="246.38" width="0.3" layer="91" />
                <label x="246.38" y="246.38" size="1.27" layer="95" />
                <pinref part="R6" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="137.16" y1="137.16" x2="134.62" y2="137.16" width="0.3" layer="91" />
                <label x="134.62" y="137.16" size="1.27" layer="95" />
                <pinref part="U1" gate="G$1" pin="RE" />
              </segment>
            </net>
            <net name="N$12">
              <segment>
                <wire x1="256.54" y1="246.38" x2="256.54" y2="243.84" width="0.3" layer="91" />
                <label x="256.54" y="243.84" size="1.27" layer="95" />
                <pinref part="Q1" gate="G$1" pin="S" />
              </segment>
              <segment>
                <wire x1="149.86" y1="149.86" x2="149.86" y2="152.40" width="0.3" layer="91" />
                <label x="149.86" y="152.40" size="1.27" layer="95" />
                <pinref part="U1" gate="G$1" pin="WE" />
              </segment>
              <segment>
                <wire x1="12.70" y1="38.10" x2="10.16" y2="38.10" width="0.3" layer="91" />
                <label x="10.16" y="38.10" size="1.27" layer="95" />
                <pinref part="R2" gate="G$1" pin="1" />
              </segment>
            </net>
            <net name="N$13">
              <segment>
                <wire x1="248.92" y1="251.46" x2="246.38" y2="251.46" width="0.3" layer="91" />
                <label x="246.38" y="251.46" size="1.27" layer="95" />
                <pinref part="Q1" gate="G$1" pin="G" />
              </segment>
              <segment>
                <wire x1="228.60" y1="213.36" x2="231.14" y2="213.36" width="0.3" layer="91" />
                <label x="231.14" y="213.36" size="1.27" layer="95" />
                <pinref part="R1" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$14">
              <segment>
                <wire x1="236.22" y1="220.98" x2="238.76" y2="220.98" width="0.3" layer="91" />
                <label x="238.76" y="220.98" size="1.27" layer="95" />
                <pinref part="R10" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="269.24" y1="274.32" x2="266.70" y2="274.32" width="0.3" layer="91" />
                <label x="266.70" y="274.32" size="1.27" layer="95" />
                <pinref part="U5" gate="G$1" pin="+IN" />
              </segment>
            </net>
            <net name="N$15">
              <segment>
                <wire x1="261.62" y1="264.16" x2="264.16" y2="264.16" width="0.3" layer="91" />
                <label x="264.16" y="264.16" size="1.27" layer="95" />
                <pinref part="R12" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="269.24" y1="284.48" x2="266.70" y2="284.48" width="0.3" layer="91" />
                <label x="266.70" y="284.48" size="1.27" layer="95" />
                <pinref part="U5" gate="G$1" pin="OUT_A" />
              </segment>
              <segment>
                <wire x1="279.40" y1="264.16" x2="281.94" y2="264.16" width="0.3" layer="91" />
                <label x="281.94" y="264.16" size="1.27" layer="95" />
                <pinref part="R9" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$16">
              <segment>
                <wire x1="48.26" y1="91.44" x2="50.80" y2="91.44" width="0.3" layer="91" />
                <label x="50.80" y="91.44" size="1.27" layer="95" />
                <pinref part="R3" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="55.88" y1="116.84" x2="53.34" y2="116.84" width="0.3" layer="91" />
                <label x="53.34" y="116.84" size="1.27" layer="95" />
                <pinref part="U2-B" gate="G$1" pin="+IN" />
              </segment>
            </net>
            <net name="N$17">
              <segment>
                <wire x1="147.32" y1="154.94" x2="149.86" y2="154.94" width="0.3" layer="91" />
                <label x="149.86" y="154.94" size="1.27" layer="95" />
                <pinref part="R4" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="180.34" y1="220.98" x2="177.80" y2="220.98" width="0.3" layer="91" />
                <label x="177.80" y="220.98" size="1.27" layer="95" />
                <pinref part="U2-A" gate="G$1" pin="+IN" />
              </segment>
            </net>
            <net name="N$18">
              <segment>
                <wire x1="269.24" y1="264.16" x2="266.70" y2="264.16" width="0.3" layer="91" />
                <label x="266.70" y="264.16" size="1.27" layer="95" />
                <pinref part="R9" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="299.72" y1="274.32" x2="302.26" y2="274.32" width="0.3" layer="91" />
                <label x="302.26" y="274.32" size="1.27" layer="95" />
                <pinref part="U5" gate="G$1" pin="-IN" />
              </segment>
            </net>
            <net name="N$19">
              <segment>
                <wire x1="129.54" y1="144.78" x2="132.08" y2="144.78" width="0.3" layer="91" />
                <label x="132.08" y="144.78" size="1.27" layer="95" />
                <pinref part="U3" gate="G$1" pin="SCLK" />
              </segment>
              <segment>
                <wire x1="154.94" y1="177.80" x2="152.40" y2="177.80" width="0.3" layer="91" />
                <label x="152.40" y="177.80" size="1.27" layer="95" />
                <pinref part="U8" gate="G$1" pin="SCLK" />
              </segment>
            </net>
            <net name="N$20">
              <segment>
                <wire x1="129.54" y1="139.70" x2="132.08" y2="139.70" width="0.3" layer="91" />
                <label x="132.08" y="139.70" size="1.27" layer="95" />
                <pinref part="U3" gate="G$1" pin="DIN" />
              </segment>
              <segment>
                <wire x1="190.50" y1="177.80" x2="193.04" y2="177.80" width="0.3" layer="91" />
                <label x="193.04" y="177.80" size="1.27" layer="95" />
                <pinref part="U8" gate="G$1" pin="DIN" />
              </segment>
            </net>
            <net name="N$21">
              <segment>
                <wire x1="129.54" y1="134.62" x2="132.08" y2="134.62" width="0.3" layer="91" />
                <label x="132.08" y="134.62" size="1.27" layer="95" />
                <pinref part="U3" gate="G$1" pin="SDO" />
              </segment>
              <segment>
                <wire x1="190.50" y1="172.72" x2="193.04" y2="172.72" width="0.3" layer="91" />
                <label x="193.04" y="172.72" size="1.27" layer="95" />
                <pinref part="U8" gate="G$1" pin="DOUT/RDY" />
              </segment>
            </net>
          </nets>
        </sheet>
      </sheets>
      <errors />
    </schematic>
  </drawing>
  <compatibility />
</eagle>
