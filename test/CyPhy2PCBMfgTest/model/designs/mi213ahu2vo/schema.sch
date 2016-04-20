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
        <library name="chipCapacitors">
          <description />
          <packages>
            <package name="C0402">
              <description>&lt;B&gt; 0402&lt;/B&gt; (1005 Metric) MLCC Capacitor &lt;P&gt;</description>
              <smd name="1" x="-0.5" y="0" dx="0.5" dy="0.55" layer="1" />
              <smd name="2" x="0.5" y="0" dx="0.5" dy="0.55" layer="1" />
              <wire x1="-0.44" y1="0.18" x2="0.44" y2="0.18" width="0.127" layer="49" />
              <wire x1="0.44" y1="0.18" x2="0.44" y2="-0.18" width="0.127" layer="49" />
              <wire x1="0.44" y1="-0.18" x2="-0.44" y2="-0.18" width="0.127" layer="49" />
              <wire x1="-0.44" y1="-0.18" x2="-0.44" y2="0.18" width="0.127" layer="49" />
              <text x="-1.26" y="0.42" size="1.27" layer="25" font="vector" ratio="15">&gt;NAME</text>
              <wire x1="-0.0635" y1="0.254" x2="0.0635" y2="0.254" width="0.127" layer="21" />
              <wire x1="-0.0635" y1="-0.254" x2="0.0635" y2="-0.254" width="0.127" layer="21" />
            </package>
          </packages>
          <symbols>
            <symbol name="CAP_NP">
              <description>&lt;B&gt;Capacitor&lt;/B&gt; -- non-polarized</description>
              <pin name="P$1" x="0" y="0" visible="off" length="short" direction="pas" rot="R270" />
              <pin name="P$2" x="0" y="-7.62" visible="off" length="short" direction="pas" rot="R90" />
              <wire x1="-1.905" y1="-3.175" x2="0" y2="-3.175" width="0.6096" layer="94" />
              <wire x1="0" y1="-3.175" x2="1.905" y2="-3.175" width="0.6096" layer="94" />
              <wire x1="-1.905" y1="-4.445" x2="0" y2="-4.445" width="0.6096" layer="94" />
              <wire x1="0" y1="-4.445" x2="1.905" y2="-4.445" width="0.6096" layer="94" />
              <wire x1="0" y1="-2.54" x2="0" y2="-3.175" width="0.254" layer="94" />
              <wire x1="0" y1="-5.08" x2="0" y2="-4.445" width="0.254" layer="94" />
              <text x="-2.54" y="-7.62" size="1.778" layer="96" rot="R90">&gt;VALUE</text>
              <text x="-5.08" y="-7.62" size="1.778" layer="95" rot="R90">&gt;NAME</text>
              <text x="0.508" y="-2.286" size="1.27" layer="95">1</text>
            </symbol>
          </symbols>
          <devicesets>
            <deviceset prefix="C" name="C0402">
              <description />
              <gates>
                <gate name="A" symbol="CAP_NP" x="0" y="0" />
              </gates>
              <devices>
                <device package="C0402">
                  <connects>
                    <connect gate="A" pin="P$1" pad="1" />
                    <connect gate="A" pin="P$2" pad="2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
          </devicesets>
        </library>
        <library name="chipResistors">
          <description />
          <packages>
            <package name="R_2010">
              <description>&lt;B&gt;
2010
&lt;/B&gt; SMT inch-code chip resistor package</description>
              <smd name="P$1" x="-2.3" y="0" dx="1.2" dy="2.7" layer="1" />
              <smd name="P$2" x="2.3" y="0" dx="1.2" dy="2.7" layer="1" />
              <wire x1="-2.5" y1="1.6" x2="2.5" y2="1.6" width="0.127" layer="21" />
              <wire x1="-2.5" y1="-1.6" x2="2.5" y2="-1.6" width="0.127" layer="21" />
              <text x="-2.9" y="1.8" size="1.27" layer="25" font="vector" ratio="15">&gt;NAME</text>
            </package>
            <package name="R_0201">
              <description>&lt;B&gt;
0201
&lt;/B&gt; SMT inch-code chip resistor package&lt;P&gt;
Derived from dimensions and tolerances
from page 8 of the &lt;B&gt; &lt;A HREF="http://www.samsungsem.com/servlet/FileDownload?filename=productcatalog/pdf/Chip_Resistor.pdf"&gt;Samsung Thick Film Chip Resistor Catalog&lt;/A&gt;&lt;/B&gt;,
for
RC0603
 general-purpose chip resistors.</description>
              <smd name="P$1" x="-0.24" y="0" dx="0.28" dy="0.33" layer="1" />
              <smd name="P$2" x="0.24" y="0" dx="0.28" dy="0.33" layer="1" />
              <wire x1="-0.3" y1="0.35" x2="0.3" y2="0.35" width="0.127" layer="21" />
              <wire x1="-0.3" y1="-0.35" x2="0.3" y2="-0.35" width="0.127" layer="21" />
              <text x="-1.3" y="0.5" size="1.27" layer="25" font="vector" ratio="15">&gt;NAME</text>
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
              <text x="-3.81" y="1.4986" size="1.778" layer="95">&gt;NAME</text>
              <text x="-3.81" y="-3.302" size="1.778" layer="96">&gt;VALUE</text>
              <pin name="2" x="5.08" y="0" visible="off" length="short" direction="pas" swaplevel="1" rot="R180" />
              <pin name="1" x="-5.08" y="0" visible="off" length="short" direction="pas" swaplevel="1" />
            </symbol>
          </symbols>
          <devicesets>
            <deviceset prefix="R" name="RESISTOR_2010">
              <description />
              <gates>
                <gate name="A" symbol="R" x="0" y="0" />
              </gates>
              <devices>
                <device package="R_2010">
                  <connects>
                    <connect gate="A" pin="1" pad="P$1" />
                    <connect gate="A" pin="2" pad="P$2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
            <deviceset prefix="R" name="RESISTOR_0201">
              <description />
              <gates>
                <gate name="G$1" symbol="R" x="0" y="0" />
              </gates>
              <devices>
                <device package="R_0201">
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
        <library name="juneSpreadsheetParts">
          <description />
          <packages>
            <package name="SOT23">
              <description>&lt;B&gt;SOT-23&lt;/B&gt; package&lt;br&gt;
Designed for Diodes Inc. NPN transistors</description>
              <smd name="B" x="-0.95" y="-1" dx="0.8" dy="0.9" layer="1" />
              <smd name="E" x="0.95" y="-1" dx="0.8" dy="0.9" layer="1" />
              <smd name="C" x="0" y="1" dx="0.8" dy="0.9" layer="1" />
              <wire x1="-0.7" y1="0.8" x2="-1.6" y2="0.8" width="0.127" layer="21" />
              <wire x1="-1.6" y1="0.8" x2="-1.6" y2="-0.8" width="0.127" layer="21" />
              <wire x1="0.7" y1="0.8" x2="1.6" y2="0.8" width="0.127" layer="21" />
              <wire x1="1.6" y1="0.8" x2="1.6" y2="-0.8" width="0.127" layer="21" />
              <wire x1="-0.3" y1="-0.8" x2="0.3" y2="-0.8" width="0.127" layer="21" />
              <text x="-1.8" y="1.8" size="0.8128" layer="21" font="vector" ratio="15">&gt;NAME</text>
            </package>
          </packages>
          <symbols>
            <symbol name="TRANS_NPN">
              <description>&lt;B&gt;NPN Transistor&lt;/b&gt;</description>
              <pin name="C" x="0" y="0" visible="off" length="short" direction="pas" rot="R270" />
              <pin name="B" x="-7.62" y="-5.08" visible="off" length="short" direction="pas" />
              <pin name="E" x="0" y="-10.16" visible="off" length="short" direction="pas" rot="R90" />
              <circle x="-1.016" y="-5.08" radius="3.5921" width="0.254" layer="94" />
              <wire x1="-2.54" y1="-3.302" x2="-2.54" y2="-3.81" width="0.254" layer="94" />
              <wire x1="-2.54" y1="-3.81" x2="-2.54" y2="-5.08" width="0.254" layer="94" />
              <wire x1="-2.54" y1="-5.08" x2="-2.54" y2="-6.35" width="0.254" layer="94" />
              <wire x1="-2.54" y1="-6.35" x2="-2.54" y2="-7.112" width="0.254" layer="94" />
              <wire x1="0" y1="-7.62" x2="-2.54" y2="-6.35" width="0.254" layer="94" />
              <wire x1="0" y1="-2.54" x2="-2.54" y2="-3.81" width="0.254" layer="94" />
              <wire x1="-5.08" y1="-5.08" x2="-2.54" y2="-5.08" width="0.254" layer="94" />
              <polygon layer="94" width="0.254">
                <vertex x="0" y="-7.62" />
                <vertex x="-0.635" y="-6.985" />
                <vertex x="-0.889" y="-7.493" />
              </polygon>
              <text x="-7.62" y="5.08" size="1.9304" layer="95" ratio="20">&gt;NAME</text>
              <text x="-7.62" y="2.54" size="1.9304" layer="96" ratio="20">&gt;VALUE</text>
            </symbol>
          </symbols>
          <devicesets>
            <deviceset prefix="Q" name="TRANSISTOR_NPN_SOT23">
              <description />
              <gates>
                <gate name="G$1" symbol="TRANS_NPN" x="2.54" y="5.08" />
              </gates>
              <devices>
                <device package="SOT23">
                  <connects>
                    <connect gate="G$1" pin="B" pad="B" />
                    <connect gate="G$1" pin="C" pad="C" />
                    <connect gate="G$1" pin="E" pad="E" />
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
        <part device="" name="C2" library="chipCapacitors" deviceset="C0402" />
        <part device="" name="R_" library="chipResistors" deviceset="RESISTOR_2010" />
        <part device="" name="R2" library="chipResistors" deviceset="RESISTOR_0201" />
        <part device="" name="C1" library="chipCapacitors" deviceset="C0402" />
        <part device="" name="Q1" library="juneSpreadsheetParts" deviceset="TRANSISTOR_NPN_SOT23" />
        <part device="" name="Q2" library="juneSpreadsheetParts" deviceset="TRANSISTOR_NPN_SOT23" />
        <part device="" name="R" library="chipResistors" deviceset="RESISTOR_2010" />
        <part device="" name="R1" library="chipResistors" deviceset="RESISTOR_0201" />
      </parts>
      <sheets>
        <sheet>
          <description />
          <plain />
          <instances>
            <instance y="45.72" part="C2" gate="A" x="106.68" />
            <instance y="30.48" part="R_" gate="A" x="121.92" />
            <instance y="30.48" part="R2" gate="G$1" x="104.14" />
            <instance y="30.48" part="C1" gate="A" x="15.24" />
            <instance y="50.80" part="Q1" gate="G$1" x="30.48" />
            <instance y="50.80" part="Q2" gate="G$1" x="50.80" />
            <instance y="15.24" part="R" gate="A" x="12.70" />
            <instance y="15.24" part="R1" gate="G$1" x="30.48" />
          </instances>
          <busses />
          <nets>
            <net name="N$0">
              <segment>
                <wire x1="106.68" y1="45.72" x2="106.68" y2="48.26" width="0.3" layer="91" />
                <label x="106.68" y="48.26" size="1.27" layer="95" />
                <pinref part="C2" gate="A" pin="P$1" />
              </segment>
              <segment>
                <wire x1="109.22" y1="30.48" x2="111.76" y2="30.48" width="0.3" layer="91" />
                <label x="111.76" y="30.48" size="1.27" layer="95" />
                <pinref part="R2" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="22.86" y1="45.72" x2="20.32" y2="45.72" width="0.3" layer="91" />
                <label x="20.32" y="45.72" size="1.27" layer="95" />
                <pinref part="Q1" gate="G$1" pin="B" />
              </segment>
            </net>
            <net name="N$1">
              <segment>
                <wire x1="106.68" y1="38.10" x2="106.68" y2="35.56" width="0.3" layer="91" />
                <label x="106.68" y="35.56" size="1.27" layer="95" />
                <pinref part="C2" gate="A" pin="P$2" />
              </segment>
              <segment>
                <wire x1="50.80" y1="50.80" x2="50.80" y2="53.34" width="0.3" layer="91" />
                <label x="50.80" y="53.34" size="1.27" layer="95" />
                <pinref part="Q2" gate="G$1" pin="C" />
              </segment>
              <segment>
                <wire x1="127.00" y1="30.48" x2="129.54" y2="30.48" width="0.3" layer="91" />
                <label x="129.54" y="30.48" size="1.27" layer="95" />
                <pinref part="R_" gate="A" pin="2" />
              </segment>
            </net>
            <net name="N$2">
              <segment>
                <wire x1="116.84" y1="30.48" x2="114.30" y2="30.48" width="0.3" layer="91" />
                <label x="114.30" y="30.48" size="1.27" layer="95" />
                <pinref part="R_" gate="A" pin="1" />
              </segment>
              <segment>
                <wire x1="99.06" y1="30.48" x2="96.52" y2="30.48" width="0.3" layer="91" />
                <label x="96.52" y="30.48" size="1.27" layer="95" />
                <pinref part="R2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="25.40" y1="15.24" x2="22.86" y2="15.24" width="0.3" layer="91" />
                <label x="22.86" y="15.24" size="1.27" layer="95" />
                <pinref part="R1" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="7.62" y1="15.24" x2="5.08" y2="15.24" width="0.3" layer="91" />
                <label x="5.08" y="15.24" size="1.27" layer="95" />
                <pinref part="R" gate="A" pin="1" />
              </segment>
            </net>
            <net name="N$3">
              <segment>
                <wire x1="15.24" y1="22.86" x2="15.24" y2="20.32" width="0.3" layer="91" />
                <label x="15.24" y="20.32" size="1.27" layer="95" />
                <pinref part="C1" gate="A" pin="P$2" />
              </segment>
              <segment>
                <wire x1="43.18" y1="45.72" x2="40.64" y2="45.72" width="0.3" layer="91" />
                <label x="40.64" y="45.72" size="1.27" layer="95" />
                <pinref part="Q2" gate="G$1" pin="B" />
              </segment>
              <segment>
                <wire x1="35.56" y1="15.24" x2="38.10" y2="15.24" width="0.3" layer="91" />
                <label x="38.10" y="15.24" size="1.27" layer="95" />
                <pinref part="R1" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$4">
              <segment>
                <wire x1="15.24" y1="30.48" x2="15.24" y2="33.02" width="0.3" layer="91" />
                <label x="15.24" y="33.02" size="1.27" layer="95" />
                <pinref part="C1" gate="A" pin="P$1" />
              </segment>
              <segment>
                <wire x1="30.48" y1="50.80" x2="30.48" y2="53.34" width="0.3" layer="91" />
                <label x="30.48" y="53.34" size="1.27" layer="95" />
                <pinref part="Q1" gate="G$1" pin="C" />
              </segment>
              <segment>
                <wire x1="17.78" y1="15.24" x2="20.32" y2="15.24" width="0.3" layer="91" />
                <label x="20.32" y="15.24" size="1.27" layer="95" />
                <pinref part="R" gate="A" pin="2" />
              </segment>
            </net>
            <net name="N$5">
              <segment>
                <wire x1="30.48" y1="40.64" x2="30.48" y2="38.10" width="0.3" layer="91" />
                <label x="30.48" y="38.10" size="1.27" layer="95" />
                <pinref part="Q1" gate="G$1" pin="E" />
              </segment>
              <segment>
                <wire x1="50.80" y1="40.64" x2="50.80" y2="38.10" width="0.3" layer="91" />
                <label x="50.80" y="38.10" size="1.27" layer="95" />
                <pinref part="Q2" gate="G$1" pin="E" />
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
