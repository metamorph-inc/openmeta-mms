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
        <library name="Andres_Project">
          <description />
          <packages>
            <package name="LED">
              <description />
              <smd name="ANODE" x="-0.75" y="0" dx="0.8" dy="0.8" layer="1" />
              <smd name="CATHODE" x="0.75" y="0" dx="0.8" dy="0.8" layer="1" />
              <wire x1="-1.5" y1="1" x2="1.5" y2="1" width="0.127" layer="21" />
              <wire x1="1.5" y1="1" x2="1.5" y2="-1" width="0.127" layer="21" />
              <wire x1="1.5" y1="-1" x2="-1.5" y2="-1" width="0.127" layer="21" />
              <wire x1="-1.5" y1="-1" x2="-1.5" y2="1" width="0.127" layer="21" />
              <circle x="1.25" y="-0.75" radius="0.06" width="0.15" layer="21" />
              <text x="-2" y="1.25" size="0.75" layer="25">&gt;NAME</text>
            </package>
          </packages>
          <symbols>
            <symbol name="LED">
              <description />
              <wire x1="0" y1="7.62" x2="0" y2="5.08" width="0.254" layer="94" />
              <wire x1="0" y1="5.08" x2="0" y2="-2.54" width="0.254" layer="94" />
              <wire x1="-2.54" y1="5.08" x2="0" y2="5.08" width="0.254" layer="94" />
              <wire x1="0" y1="5.08" x2="2.54" y2="5.08" width="0.254" layer="94" />
              <wire x1="-2.54" y1="0" x2="0" y2="5.08" width="0.254" layer="94" />
              <wire x1="0" y1="5.08" x2="2.54" y2="0" width="0.254" layer="94" />
              <wire x1="2.54" y1="0" x2="-2.54" y2="0" width="0.254" layer="94" />
              <wire x1="5.08" y1="5.08" x2="7.62" y2="7.62" width="0.254" layer="94" />
              <wire x1="7.62" y1="5.08" x2="7.62" y2="7.62" width="0.254" layer="94" />
              <wire x1="7.62" y1="7.62" x2="5.08" y2="7.62" width="0.254" layer="94" />
              <pin name="ANODE" x="0" y="-2.54" visible="pin" length="point" direction="pas" function="dot" />
              <pin name="CATHODE" x="0" y="7.62" length="point" direction="pas" function="dot" />
              <text x="-5.08" y="10.16" size="1.27" layer="95">&gt;NAME</text>
              <text x="-5.08" y="-5.08" size="1.27" layer="96">&gt;VALUE</text>
            </symbol>
          </symbols>
          <devicesets>
            <deviceset name="LED">
              <description />
              <gates>
                <gate name="G$1" symbol="LED" x="0" y="2.54" />
              </gates>
              <devices>
                <device package="LED">
                  <connects>
                    <connect gate="G$1" pin="ANODE" pad="ANODE" />
                    <connect gate="G$1" pin="CATHODE" pad="CATHODE" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
          </devicesets>
        </library>
        <library name="semicon-smd-ipc">
          <description />
          <packages>
            <package name="SOT143">
              <description>&lt;b&gt;SOT-143&lt;/b&gt;</description>
              <wire x1="-1.448" y1="0.635" x2="1.448" y2="0.635" width="0.1" layer="51" />
              <wire x1="-1.448" y1="-0.635" x2="1.448" y2="-0.635" width="0.1" layer="51" />
              <wire x1="-1.448" y1="-0.635" x2="-1.448" y2="0.635" width="0.1" layer="51" />
              <wire x1="1.448" y1="-0.635" x2="1.448" y2="0.635" width="0.1" layer="51" />
              <smd name="4" x="-0.95" y="1.1" dx="1" dy="1.44" layer="1" />
              <smd name="3" x="0.95" y="1.1" dx="1" dy="1.44" layer="1" />
              <smd name="2" x="0.95" y="-1.1" dx="1" dy="1.44" layer="1" />
              <smd name="1" x="-0.75" y="-1.1" dx="1.2" dy="1.44" layer="1" />
              <text x="-1.905" y="1.905" size="1.27" layer="25">&gt;NAME</text>
              <text x="-1.905" y="-3.175" size="1.27" layer="27">&gt;VALUE</text>
              <rectangle x1="0.7366" y1="-1.3208" x2="1.1938" y2="-0.635" layer="51" />
              <rectangle x1="0.7112" y1="0.635" x2="1.1684" y2="1.3208" layer="51" />
              <rectangle x1="-1.143" y1="0.635" x2="-0.6858" y2="1.3208" layer="51" />
              <rectangle x1="-1.1938" y1="-1.3208" x2="-0.3048" y2="-0.635" layer="51" />
            </package>
          </packages>
          <symbols>
            <symbol name="NPN2">
              <description />
              <wire x1="2.54" y1="2.54" x2="0.508" y2="1.524" width="0.1524" layer="94" />
              <wire x1="1.5781" y1="-1.4239" x2="2.54" y2="-2.54" width="0.1524" layer="94" />
              <wire x1="2.54" y1="-2.54" x2="1.0701" y2="-2.4399" width="0.1524" layer="94" />
              <wire x1="1.0701" y1="-2.4399" x2="1.5781" y2="-1.4239" width="0.1524" layer="94" />
              <wire x1="1.3401" y1="-1.9401" x2="0.108" y2="-1.3241" width="0.1524" layer="94" />
              <text x="-10.16" y="2.54" size="1.778" layer="95">&gt;NAME</text>
              <text x="-10.16" y="5.08" size="1.778" layer="96">&gt;VALUE</text>
              <rectangle x1="-0.254" y1="-2.54" x2="0.508" y2="2.54" layer="94" />
              <pin name="B" x="-2.54" y="0" visible="off" length="short" direction="pas" />
              <pin name="E" x="2.54" y="-5.08" visible="off" length="short" direction="pas" rot="R90" />
              <pin name="C" x="2.54" y="5.08" visible="off" length="short" direction="pas" rot="R270" />
              <pin name="C/" x="2.54" y="2.54" visible="off" length="short" direction="pas" rot="R90" />
            </symbol>
          </symbols>
          <devicesets>
            <deviceset uservalue="yes" prefix="Q" name="NPN-TRANSISTIOR-2COL">
              <description />
              <gates>
                <gate name="G$1" symbol="NPN2" x="0" y="0" />
              </gates>
              <devices>
                <device package="SOT143" name="SOT143">
                  <connects>
                    <connect gate="G$1" pin="B" pad="1" />
                    <connect gate="G$1" pin="C" pad="2" />
                    <connect gate="G$1" pin="C/" pad="4" />
                    <connect gate="G$1" pin="E" pad="3" />
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
        <part device="" value="LTST-C191KRKT" name="LED3" library="Andres_Project" deviceset="LED" />
        <part device="" value="LTST-C191KRKT" name="LED4" library="Andres_Project" deviceset="LED" />
        <part device="" value="LTST-C191KRKT" name="LED1" library="Andres_Project" deviceset="LED" />
        <part device="" value="LTST-C191KRKT" name="LED2" library="Andres_Project" deviceset="LED" />
        <part device="SOT143" value="" name="Comp_SOT143" library="semicon-smd-ipc" deviceset="NPN-TRANSISTIOR-2COL" />
      </parts>
      <sheets>
        <sheet>
          <description />
          <plain />
          <instances>
            <instance y="40.64" part="LED3" gate="G$1" x="66.04" />
            <instance y="48.26" part="LED4" gate="G$1" x="83.82" />
            <instance y="22.86" part="LED1" gate="G$1" x="27.94" />
            <instance y="63.50" part="LED2" gate="G$1" x="45.72" />
            <instance y="15.24" part="Comp_SOT143" gate="G$1" x="99.06" />
          </instances>
          <busses />
          <nets>
            <net name="N$0">
              <segment>
                <wire x1="66.04" y1="38.10" x2="63.50" y2="38.10" width="0.3" layer="91" />
                <label x="63.50" y="38.10" size="1.27" layer="95" />
                <pinref part="LED3" gate="G$1" pin="ANODE" />
              </segment>
              <segment>
                <wire x1="83.82" y1="45.72" x2="81.28" y2="45.72" width="0.3" layer="91" />
                <label x="81.28" y="45.72" size="1.27" layer="95" />
                <pinref part="LED4" gate="G$1" pin="ANODE" />
              </segment>
              <segment>
                <wire x1="27.94" y1="20.32" x2="25.40" y2="20.32" width="0.3" layer="91" />
                <label x="25.40" y="20.32" size="1.27" layer="95" />
                <pinref part="LED1" gate="G$1" pin="ANODE" />
              </segment>
            </net>
            <net name="N$1">
              <segment>
                <wire x1="66.04" y1="48.26" x2="63.50" y2="48.26" width="0.3" layer="91" />
                <label x="63.50" y="48.26" size="1.27" layer="95" />
                <pinref part="LED3" gate="G$1" pin="CATHODE" />
              </segment>
              <segment>
                <wire x1="83.82" y1="55.88" x2="81.28" y2="55.88" width="0.3" layer="91" />
                <label x="81.28" y="55.88" size="1.27" layer="95" />
                <pinref part="LED4" gate="G$1" pin="CATHODE" />
              </segment>
              <segment>
                <wire x1="45.72" y1="71.12" x2="43.18" y2="71.12" width="0.3" layer="91" />
                <label x="43.18" y="71.12" size="1.27" layer="95" />
                <pinref part="LED2" gate="G$1" pin="CATHODE" />
              </segment>
            </net>
            <net name="N$2">
              <segment>
                <wire x1="45.72" y1="60.96" x2="43.18" y2="60.96" width="0.3" layer="91" />
                <label x="43.18" y="60.96" size="1.27" layer="95" />
                <pinref part="LED2" gate="G$1" pin="ANODE" />
              </segment>
              <segment>
                <wire x1="96.52" y1="15.24" x2="93.98" y2="15.24" width="0.3" layer="91" />
                <label x="93.98" y="15.24" size="1.27" layer="95" />
                <pinref part="Comp_SOT143" gate="G$1" pin="B" />
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
