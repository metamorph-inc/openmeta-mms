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
        <library name="partsFromSpreadsheet">
          <description />
          <packages>
            <package name="0603-LED">
              <description>&lt;B&gt;LED 0603&lt;/B&gt;&lt;P&gt;
Blue LED 0603, designed for the Osram LB Q39G-L2N2-35-1.&lt;br&gt;Digikey part 475-2816-1-ND.</description>
              <circle x="-0.8" y="-1.1" radius="0.1" width="0.127" layer="21" />
              <wire x1="-1.6" y1="0.8" x2="1.6" y2="0.8" width="0.127" layer="21" />
              <wire x1="-1.6" y1="-0.8" x2="1.6" y2="-0.8" width="0.127" layer="21" />
              <wire x1="-1.6" y1="0.8" x2="-1.6" y2="-0.8" width="0.127" layer="21" />
              <wire x1="1.6" y1="0.8" x2="1.6" y2="-0.8" width="0.127" layer="21" />
              <smd name="P$1" x="-0.8" y="0" dx="0.8" dy="0.8" layer="1" />
              <smd name="P$2" x="0.8" y="0" dx="0.8" dy="0.8" layer="1" />
              <polygon layer="21" width="0.127">
                <vertex x="-1.6" y="-0.8" />
                <vertex x="-1.6" y="-0.4" />
                <vertex x="-1.2" y="-0.8" />
              </polygon>
            </package>
          </packages>
          <symbols>
            <symbol name="LED_WITH_PIN1_CATHODE">
              <description>&lt;B&gt;LED with pin 1 cathode&lt;/B&gt;&lt;P&gt;
Designed for the Osram LB Q39G-L2N2-35-1
blue led, in an 0603 package.&lt;P&gt;
Digikey 475-2816-1-ND</description>
              <wire x1="-1.27" y1="2.54" x2="1.27" y2="2.54" width="0.254" layer="94" />
              <wire x1="1.778" y1="5.08" x2="2.54" y2="5.842" width="0.254" layer="94" />
              <wire x1="2.54" y1="5.842" x2="2.54" y2="5.334" width="0.254" layer="94" />
              <wire x1="2.54" y1="5.334" x2="3.048" y2="5.842" width="0.254" layer="94" />
              <wire x1="1.524" y1="3.81" x2="2.286" y2="4.572" width="0.254" layer="94" />
              <wire x1="2.286" y1="4.572" x2="2.286" y2="4.064" width="0.254" layer="94" />
              <wire x1="2.286" y1="4.064" x2="2.794" y2="4.572" width="0.254" layer="94" />
              <pin name="P$1" x="0" y="0" visible="off" length="short" direction="pas" rot="R90" />
              <pin name="P$2" x="0" y="7.62" visible="off" length="short" direction="pas" rot="R270" />
              <text x="-2.032" y="1.778" size="1.778" layer="95" rot="MR180">1</text>
              <text x="-2.032" y="7.62" size="1.778" layer="95" rot="MR180">2</text>
              <text x="-5.08" y="0" size="1.778" layer="95" rot="R90">&gt;NAME</text>
              <text x="-2.54" y="0" size="1.778" layer="95" rot="R90">&gt;VALUE</text>
              <polygon layer="94" width="0.254">
                <vertex x="0" y="2.54" />
                <vertex x="-1.27" y="5.08" />
                <vertex x="1.27" y="5.08" />
              </polygon>
              <polygon layer="94" width="0.254">
                <vertex x="3.556" y="6.35" />
                <vertex x="3.048" y="6.096" />
                <vertex x="3.302" y="5.842" />
              </polygon>
              <polygon layer="94" width="0.254">
                <vertex x="3.302" y="5.08" />
                <vertex x="2.794" y="4.826" />
                <vertex x="3.048" y="4.572" />
              </polygon>
            </symbol>
          </symbols>
          <devicesets>
            <deviceset uservalue="yes" prefix="DS" name="LED-BLUE-0603">
              <description />
              <gates>
                <gate name="G$1" symbol="LED_WITH_PIN1_CATHODE" x="0" y="0" />
              </gates>
              <devices>
                <device package="0603-LED">
                  <connects>
                    <connect gate="G$1" pin="P$1" pad="P$1" />
                    <connect gate="G$1" pin="P$2" pad="P$2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
          </devicesets>
        </library>
        <library name="rcl">
          <description />
          <packages>
            <package name="R0603">
              <description>&lt;b&gt;RESISTOR&lt;/b&gt;&lt;p&gt;
chip</description>
              <wire x1="-0.432" y1="-0.356" x2="0.432" y2="-0.356" width="0.1524" layer="51" />
              <wire x1="0.432" y1="0.356" x2="-0.432" y2="0.356" width="0.1524" layer="51" />
              <wire x1="-1.473" y1="0.983" x2="1.473" y2="0.983" width="0.0508" layer="39" />
              <wire x1="1.473" y1="0.983" x2="1.473" y2="-0.983" width="0.0508" layer="39" />
              <wire x1="1.473" y1="-0.983" x2="-1.473" y2="-0.983" width="0.0508" layer="39" />
              <wire x1="-1.473" y1="-0.983" x2="-1.473" y2="0.983" width="0.0508" layer="39" />
              <smd name="1" x="-0.85" y="0" dx="1" dy="1.1" layer="1" />
              <smd name="2" x="0.85" y="0" dx="1" dy="1.1" layer="1" />
              <text x="-0.889" y="0.889" size="1.27" layer="25">&gt;NAME</text>
              <text x="-0.889" y="-2.032" size="1.27" layer="27">&gt;VALUE</text>
              <rectangle x1="0.4318" y1="-0.4318" x2="0.8382" y2="0.4318" layer="51" />
              <rectangle x1="-0.8382" y1="-0.4318" x2="-0.4318" y2="0.4318" layer="51" />
              <rectangle x1="-0.1999" y1="-0.4001" x2="0.1999" y2="0.4001" layer="35" />
            </package>
          </packages>
          <symbols>
            <symbol name="R-US">
              <description />
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
            <deviceset uservalue="yes" prefix="R" name="R-US_">
              <description />
              <gates>
                <gate name="G$1" symbol="R-US" x="0" y="0" />
              </gates>
              <devices>
                <device package="R0603" name="R0603">
                  <connects>
                    <connect gate="G$1" pin="1" pad="1" />
                    <connect gate="G$1" pin="2" pad="2" />
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
        <part device="" value="" name="LD11" library="partsFromSpreadsheet" deviceset="LED-BLUE-0603" />
        <part device="R0603" name="RdvC" library="rcl" deviceset="R-US_" />
        <part device="R0603" name="RlimC" library="rcl" deviceset="R-US_" />
        <part device="SOT143" name="TC" library="semicon-smd-ipc" deviceset="NPN-TRANSISTIOR-2COL" />
        <part device="R0603" name="RdrvR" library="rcl" deviceset="R-US_" />
        <part device="SOT143" name="TR" library="semicon-smd-ipc" deviceset="NPN-TRANSISTIOR-2COL" />
      </parts>
      <sheets>
        <sheet>
          <description />
          <plain />
          <instances>
            <instance y="33.02" part="LD11" gate="G$1" x="66.04" />
            <instance y="17.78" part="RdvC" gate="G$1" x="17.78" />
            <instance y="35.56" part="RlimC" gate="G$1" x="38.10" />
            <instance y="22.86" part="TC" gate="G$1" x="33.02" />
            <instance y="27.94" part="RdrvR" gate="G$1" x="91.44" />
            <instance y="30.48" part="TR" gate="G$1" x="106.68" />
          </instances>
          <busses />
          <nets>
            <net name="N$0">
              <segment>
                <wire x1="66.04" y1="40.64" x2="66.04" y2="43.18" width="0.3" layer="91" />
                <label x="66.04" y="43.18" size="1.27" layer="95" />
                <pinref part="LD11" gate="G$1" pin="P$2" />
              </segment>
              <segment>
                <wire x1="109.22" y1="35.56" x2="109.22" y2="38.10" width="0.3" layer="91" />
                <label x="109.22" y="38.10" size="1.27" layer="95" />
                <pinref part="TR" gate="G$1" pin="C" />
              </segment>
            </net>
            <net name="N$1">
              <segment>
                <wire x1="66.04" y1="33.02" x2="66.04" y2="30.48" width="0.3" layer="91" />
                <label x="66.04" y="30.48" size="1.27" layer="95" />
                <pinref part="LD11" gate="G$1" pin="P$1" />
              </segment>
              <segment>
                <wire x1="35.56" y1="17.78" x2="35.56" y2="15.24" width="0.3" layer="91" />
                <label x="35.56" y="15.24" size="1.27" layer="95" />
                <pinref part="TC" gate="G$1" pin="E" />
              </segment>
            </net>
            <net name="N$2">
              <segment>
                <wire x1="22.86" y1="17.78" x2="25.40" y2="17.78" width="0.3" layer="91" />
                <label x="25.40" y="17.78" size="1.27" layer="95" />
                <pinref part="RdvC" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="30.48" y1="22.86" x2="27.94" y2="22.86" width="0.3" layer="91" />
                <label x="27.94" y="22.86" size="1.27" layer="95" />
                <pinref part="TC" gate="G$1" pin="B" />
              </segment>
            </net>
            <net name="N$3">
              <segment>
                <wire x1="33.02" y1="35.56" x2="30.48" y2="35.56" width="0.3" layer="91" />
                <label x="30.48" y="35.56" size="1.27" layer="95" />
                <pinref part="RlimC" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="35.56" y1="27.94" x2="35.56" y2="30.48" width="0.3" layer="91" />
                <label x="35.56" y="30.48" size="1.27" layer="95" />
                <pinref part="TC" gate="G$1" pin="C" />
              </segment>
            </net>
            <net name="N$4">
              <segment>
                <wire x1="96.52" y1="27.94" x2="99.06" y2="27.94" width="0.3" layer="91" />
                <label x="99.06" y="27.94" size="1.27" layer="95" />
                <pinref part="RdrvR" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="104.14" y1="30.48" x2="101.60" y2="30.48" width="0.3" layer="91" />
                <label x="101.60" y="30.48" size="1.27" layer="95" />
                <pinref part="TR" gate="G$1" pin="B" />
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
