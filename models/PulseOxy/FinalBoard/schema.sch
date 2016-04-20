<?xml version="1.0"?>
<eagle xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" version="6.5.0" xmlns="eagle">
  <compatibility />
  <drawing>
    <settings>
      <setting alwaysvectorfont="no" />
      <setting />
    </settings>
    <grid distance="0.1" unitdist="inch" unit="inch" altdistance="0.01" altunitdist="inch" altunit="inch" />
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
        <library name="resistor">
          <description />
          <packages>
            <package name="C0805">
              <description>&lt;b&gt;CAPACITOR&lt;/b&gt;&lt;p&gt;
chip</description>
              <wire x1="-1.973" y1="0.983" x2="1.973" y2="0.983" width="0.0508" layer="39" />
              <wire x1="1.973" y1="-0.983" x2="-1.973" y2="-0.983" width="0.0508" layer="39" />
              <wire x1="-1.973" y1="-0.983" x2="-1.973" y2="0.983" width="0.0508" layer="39" />
              <wire x1="-0.381" y1="0.66" x2="0.381" y2="0.66" width="0.1016" layer="51" />
              <wire x1="-0.356" y1="-0.66" x2="0.381" y2="-0.66" width="0.1016" layer="51" />
              <wire x1="1.973" y1="0.983" x2="1.973" y2="-0.983" width="0.0508" layer="39" />
              <smd name="1" x="-0.85" y="0" dx="1.3" dy="1.5" layer="1" />
              <smd name="2" x="0.85" y="0" dx="1.3" dy="1.5" layer="1" />
              <text x="-0.889" y="1.016" size="1.27" layer="25">&gt;NAME</text>
              <text x="-0.889" y="-2.286" size="1.27" layer="27">&gt;VALUE</text>
              <rectangle x1="-1.0922" y1="-0.7239" x2="-0.3421" y2="0.7262" layer="51" />
              <rectangle x1="0.3556" y1="-0.7239" x2="1.1057" y2="0.7262" layer="51" />
              <rectangle x1="-0.1001" y1="-0.4001" x2="0.1001" y2="0.4001" layer="35" />
            </package>
            <package name="C0603">
              <description>&lt;b&gt;CAPACITOR&lt;/b&gt;&lt;p&gt;
chip</description>
              <wire x1="-1.473" y1="0.983" x2="1.473" y2="0.983" width="0.0508" layer="39" />
              <wire x1="1.473" y1="0.983" x2="1.473" y2="-0.983" width="0.0508" layer="39" />
              <wire x1="1.473" y1="-0.983" x2="-1.473" y2="-0.983" width="0.0508" layer="39" />
              <wire x1="-1.473" y1="-0.983" x2="-1.473" y2="0.983" width="0.0508" layer="39" />
              <wire x1="-0.356" y1="0.432" x2="0.356" y2="0.432" width="0.1016" layer="51" />
              <wire x1="-0.356" y1="-0.419" x2="0.356" y2="-0.419" width="0.1016" layer="51" />
              <smd name="1" x="-0.85" y="0" dx="1.1" dy="1" layer="1" />
              <smd name="2" x="0.85" y="0" dx="1.1" dy="1" layer="1" />
              <text x="-0.889" y="0.762" size="1.27" layer="25">&gt;NAME</text>
              <text x="-0.889" y="-2.032" size="1.27" layer="27">&gt;VALUE</text>
              <rectangle x1="-0.8382" y1="-0.4699" x2="-0.3381" y2="0.4801" layer="51" />
              <rectangle x1="0.3302" y1="-0.4699" x2="0.8303" y2="0.4801" layer="51" />
              <rectangle x1="-0.1999" y1="-0.3" x2="0.1999" y2="0.3" layer="35" />
            </package>
          </packages>
          <symbols>
            <symbol name="C-US">
              <description />
              <wire x1="-2.54" y1="0" x2="2.54" y2="0" width="0.254" layer="94" />
              <wire x1="0" y1="-1.016" x2="0" y2="-2.54" width="0.1524" layer="94" />
              <wire x1="0" y1="-1" x2="2.4892" y2="-1.8542" width="0.254" layer="94" curve="-37.878202" cap="flat" />
              <wire x1="-2.4668" y1="-1.8504" x2="0" y2="-1.0161" width="0.254" layer="94" curve="-37.373024" cap="flat" />
              <text x="1.016" y="0.635" size="1.778" layer="95">&gt;NAME</text>
              <text x="1.016" y="-4.191" size="1.778" layer="96">&gt;VALUE</text>
              <pin name="1" x="0" y="2.54" visible="off" length="short" direction="pas" swaplevel="1" rot="R270" />
              <pin name="2" x="0" y="-5.08" visible="off" length="short" direction="pas" swaplevel="1" rot="R90" />
            </symbol>
          </symbols>
          <devicesets>
            <deviceset uservalue="yes" prefix="C" name="C-US">
              <description />
              <gates>
                <gate name="G$1" symbol="C-US" x="0" y="0" />
              </gates>
              <devices>
                <device package="C0805" name="C0805">
                  <connects>
                    <connect gate="G$1" pin="1" pad="1" />
                    <connect gate="G$1" pin="2" pad="2" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
                <device package="C0603" name="C0603">
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
        <library name="linear">
          <description />
          <packages>
            <package name="SO08">
              <description>&lt;b&gt;Small Outline Package 8&lt;/b&gt;&lt;br&gt;
NS Package M08A</description>
              <wire x1="2.4" y1="1.9" x2="2.4" y2="-1.4" width="0.2032" layer="51" />
              <wire x1="2.4" y1="-1.4" x2="2.4" y2="-1.9" width="0.2032" layer="51" />
              <wire x1="2.4" y1="-1.9" x2="-2.4" y2="-1.9" width="0.2032" layer="51" />
              <wire x1="-2.4" y1="-1.9" x2="-2.4" y2="-1.4" width="0.2032" layer="51" />
              <wire x1="-2.4" y1="-1.4" x2="-2.4" y2="1.9" width="0.2032" layer="51" />
              <wire x1="-2.4" y1="1.9" x2="2.4" y2="1.9" width="0.2032" layer="51" />
              <wire x1="2.4" y1="-1.4" x2="-2.4" y2="-1.4" width="0.2032" layer="51" />
              <smd name="2" x="-0.635" y="-2.6" dx="0.6" dy="2.2" layer="1" />
              <smd name="7" x="-0.635" y="2.6" dx="0.6" dy="2.2" layer="1" />
              <smd name="1" x="-1.905" y="-2.6" dx="0.6" dy="2.2" layer="1" />
              <smd name="3" x="0.635" y="-2.6" dx="0.6" dy="2.2" layer="1" />
              <smd name="4" x="1.905" y="-2.6" dx="0.6" dy="2.2" layer="1" />
              <smd name="8" x="-1.905" y="2.6" dx="0.6" dy="2.2" layer="1" />
              <smd name="6" x="0.635" y="2.6" dx="0.6" dy="2.2" layer="1" />
              <smd name="5" x="1.905" y="2.6" dx="0.6" dy="2.2" layer="1" />
              <text x="-2.667" y="-1.905" size="1.27" layer="25" rot="R90">&gt;NAME</text>
              <text x="3.937" y="-1.905" size="1.27" layer="27" rot="R90">&gt;VALUE</text>
              <rectangle x1="-2.15" y1="-3.1" x2="-1.66" y2="-2" layer="51" />
              <rectangle x1="-0.88" y1="-3.1" x2="-0.39" y2="-2" layer="51" />
              <rectangle x1="0.39" y1="-3.1" x2="0.88" y2="-2" layer="51" />
              <rectangle x1="1.66" y1="-3.1" x2="2.15" y2="-2" layer="51" />
              <rectangle x1="1.66" y1="2" x2="2.15" y2="3.1" layer="51" />
              <rectangle x1="0.39" y1="2" x2="0.88" y2="3.1" layer="51" />
              <rectangle x1="-0.88" y1="2" x2="-0.39" y2="3.1" layer="51" />
              <rectangle x1="-2.15" y1="2" x2="-1.66" y2="3.1" layer="51" />
            </package>
          </packages>
          <symbols>
            <symbol name="PWR+-">
              <description />
              <text x="1.27" y="3.175" size="0.8128" layer="93" rot="R90">V+</text>
              <text x="1.27" y="-4.445" size="0.8128" layer="93" rot="R90">V-</text>
              <pin name="V+" x="0" y="7.62" visible="pad" length="middle" direction="pwr" rot="R270" />
              <pin name="V-" x="0" y="-7.62" visible="pad" length="middle" direction="pwr" rot="R90" />
            </symbol>
            <symbol name="OPAMP">
              <description />
              <wire x1="-5.08" y1="5.08" x2="-5.08" y2="-5.08" width="0.4064" layer="94" />
              <wire x1="-5.08" y1="-5.08" x2="5.08" y2="0" width="0.4064" layer="94" />
              <wire x1="5.08" y1="0" x2="-5.08" y2="5.08" width="0.4064" layer="94" />
              <wire x1="-3.81" y1="3.175" x2="-3.81" y2="1.905" width="0.1524" layer="94" />
              <wire x1="-4.445" y1="2.54" x2="-3.175" y2="2.54" width="0.1524" layer="94" />
              <wire x1="-4.445" y1="-2.54" x2="-3.175" y2="-2.54" width="0.1524" layer="94" />
              <text x="2.54" y="3.175" size="1.778" layer="95">&gt;NAME</text>
              <text x="2.54" y="-5.08" size="1.778" layer="96">&gt;VALUE</text>
              <pin name="-IN" x="-7.62" y="-2.54" visible="pad" length="short" direction="in" />
              <pin name="+IN" x="-7.62" y="2.54" visible="pad" length="short" direction="in" />
              <pin name="OUT" x="7.62" y="0" visible="pad" length="short" direction="out" rot="R180" />
            </symbol>
          </symbols>
          <devicesets>
            <deviceset prefix="IC" name="*1458">
              <description />
              <gates>
                <gate name="A" symbol="OPAMP" x="5.08" y="0" swaplevel="1" />
                <gate name="P" symbol="PWR+-" x="5.08" y="0" addlevel="request" />
                <gate name="B" symbol="OPAMP" x="27.94" y="0" swaplevel="1" />
              </gates>
              <devices>
                <device package="SO08" name="D">
                  <connects>
                    <connect gate="A" pin="+IN" pad="3" />
                    <connect gate="A" pin="-IN" pad="2" />
                    <connect gate="A" pin="OUT" pad="1" />
                    <connect gate="B" pin="+IN" pad="5" />
                    <connect gate="B" pin="-IN" pad="6" />
                    <connect gate="B" pin="OUT" pad="7" />
                    <connect gate="P" pin="V+" pad="8" />
                    <connect gate="P" pin="V-" pad="4" />
                  </connects>
                  <technologies>
                    <technology name="LM" />
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
              <description>&lt;b&gt;RESISTOR&lt;/b&gt;</description>
              <wire x1="-0.432" y1="-0.356" x2="0.432" y2="-0.356" width="0.1524" layer="51" />
              <wire x1="0.432" y1="0.356" x2="-0.432" y2="0.356" width="0.1524" layer="51" />
              <wire x1="-1.473" y1="0.983" x2="1.473" y2="0.983" width="0.0508" layer="39" />
              <wire x1="1.473" y1="0.983" x2="1.473" y2="-0.983" width="0.0508" layer="39" />
              <wire x1="1.473" y1="-0.983" x2="-1.473" y2="-0.983" width="0.0508" layer="39" />
              <wire x1="-1.473" y1="-0.983" x2="-1.473" y2="0.983" width="0.0508" layer="39" />
              <smd name="1" x="-0.85" y="0" dx="1" dy="1.1" layer="1" />
              <smd name="2" x="0.85" y="0" dx="1" dy="1.1" layer="1" />
              <text x="-0.635" y="0.635" size="1.27" layer="25">&gt;NAME</text>
              <text x="-0.635" y="-1.905" size="1.27" layer="27">&gt;VALUE</text>
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
        <library name="jumper">
          <description />
          <packages>
            <package name="JP3Q">
              <description>&lt;b&gt;JUMPER&lt;/b&gt;</description>
              <wire x1="-3.81" y1="-2.159" x2="-3.81" y2="2.159" width="0.1524" layer="21" />
              <wire x1="-1.651" y1="2.54" x2="-1.27" y2="2.159" width="0.1524" layer="21" />
              <wire x1="-1.27" y1="2.159" x2="-0.889" y2="2.54" width="0.1524" layer="21" />
              <wire x1="-0.889" y1="2.54" x2="0.889" y2="2.54" width="0.1524" layer="21" />
              <wire x1="1.27" y1="2.159" x2="0.889" y2="2.54" width="0.1524" layer="21" />
              <wire x1="1.27" y1="2.159" x2="1.651" y2="2.54" width="0.1524" layer="21" />
              <wire x1="1.651" y1="2.54" x2="3.429" y2="2.54" width="0.1524" layer="21" />
              <wire x1="3.81" y1="2.159" x2="3.429" y2="2.54" width="0.1524" layer="21" />
              <wire x1="3.81" y1="2.159" x2="3.81" y2="-2.159" width="0.1524" layer="21" />
              <wire x1="3.429" y1="-2.54" x2="3.81" y2="-2.159" width="0.1524" layer="21" />
              <wire x1="3.429" y1="-2.54" x2="1.651" y2="-2.54" width="0.1524" layer="21" />
              <wire x1="1.27" y1="-2.159" x2="1.651" y2="-2.54" width="0.1524" layer="21" />
              <wire x1="1.27" y1="-2.159" x2="0.889" y2="-2.54" width="0.1524" layer="21" />
              <wire x1="0.889" y1="-2.54" x2="-0.889" y2="-2.54" width="0.1524" layer="21" />
              <wire x1="-1.27" y1="-2.159" x2="-0.889" y2="-2.54" width="0.1524" layer="21" />
              <wire x1="-1.27" y1="-2.159" x2="-1.651" y2="-2.54" width="0.1524" layer="21" />
              <wire x1="-3.81" y1="2.159" x2="-3.429" y2="2.54" width="0.1524" layer="21" />
              <wire x1="-3.429" y1="2.54" x2="-1.651" y2="2.54" width="0.1524" layer="21" />
              <wire x1="-3.81" y1="-2.159" x2="-3.429" y2="-2.54" width="0.1524" layer="21" />
              <wire x1="-3.429" y1="-2.54" x2="-1.651" y2="-2.54" width="0.1524" layer="21" />
              <pad name="1" x="-2.54" y="-1.27" drill="0.9144" shape="octagon" />
              <pad name="2" x="-2.54" y="1.27" drill="0.9144" shape="octagon" />
              <pad name="3" x="0" y="-1.27" drill="0.9144" shape="octagon" />
              <pad name="4" x="0" y="1.27" drill="0.9144" shape="octagon" />
              <pad name="5" x="2.54" y="-1.27" drill="0.9144" shape="octagon" />
              <pad name="6" x="2.54" y="1.27" drill="0.9144" shape="octagon" />
              <text x="-3.048" y="-4.191" size="1.27" layer="21" ratio="10">1</text>
              <text x="-0.508" y="-4.191" size="1.27" layer="21" ratio="10">2</text>
              <text x="2.032" y="-4.191" size="1.27" layer="21" ratio="10">3</text>
              <text x="-3.429" y="2.921" size="1.27" layer="25" ratio="10">&gt;NAME</text>
              <text x="-3.429" y="-5.842" size="1.27" layer="27" ratio="10">&gt;VALUE</text>
              <rectangle x1="-2.8448" y1="0.9652" x2="-2.2352" y2="1.5748" layer="51" />
              <rectangle x1="-0.3048" y1="0.9652" x2="0.3048" y2="1.5748" layer="51" />
              <rectangle x1="2.2352" y1="0.9652" x2="2.8448" y2="1.5748" layer="51" />
              <rectangle x1="-2.8448" y1="-1.5748" x2="-2.2352" y2="-0.9652" layer="51" />
              <rectangle x1="-0.3048" y1="-1.5748" x2="0.3048" y2="-0.9652" layer="51" />
              <rectangle x1="2.2352" y1="-1.5748" x2="2.8448" y2="-0.9652" layer="51" />
            </package>
          </packages>
          <symbols>
            <symbol name="J3">
              <description />
              <wire x1="-2.54" y1="2.54" x2="-2.54" y2="3.81" width="0.4064" layer="94" />
              <wire x1="-2.54" y1="3.81" x2="-2.54" y2="5.08" width="0.1524" layer="94" />
              <wire x1="-2.54" y1="-2.54" x2="-2.54" y2="-3.81" width="0.4064" layer="94" />
              <wire x1="-2.54" y1="-3.81" x2="-2.54" y2="-5.08" width="0.1524" layer="94" />
              <wire x1="0" y1="2.54" x2="0" y2="3.81" width="0.4064" layer="94" />
              <wire x1="0" y1="3.81" x2="0" y2="5.08" width="0.1524" layer="94" />
              <wire x1="0" y1="-2.54" x2="0" y2="-3.81" width="0.4064" layer="94" />
              <wire x1="0" y1="-3.81" x2="0" y2="-5.08" width="0.1524" layer="94" />
              <wire x1="2.54" y1="2.54" x2="2.54" y2="3.81" width="0.4064" layer="94" />
              <wire x1="2.54" y1="3.81" x2="2.54" y2="5.08" width="0.1524" layer="94" />
              <wire x1="2.54" y1="-2.54" x2="2.54" y2="-3.81" width="0.4064" layer="94" />
              <wire x1="2.54" y1="-3.81" x2="2.54" y2="-5.08" width="0.1524" layer="94" />
              <wire x1="-4.445" y1="5.08" x2="4.445" y2="5.08" width="0.4064" layer="94" />
              <wire x1="4.445" y1="5.08" x2="4.445" y2="-5.08" width="0.4064" layer="94" />
              <wire x1="4.445" y1="-5.08" x2="-4.445" y2="-5.08" width="0.4064" layer="94" />
              <wire x1="-4.445" y1="-5.08" x2="-4.445" y2="5.08" width="0.4064" layer="94" />
              <text x="-5.08" y="-5.08" size="1.778" layer="95" rot="R90">&gt;NAME</text>
              <text x="6.985" y="-5.08" size="1.778" layer="96" rot="R90">&gt;VALUE</text>
              <pin name="1" x="-2.54" y="-7.62" visible="pad" length="short" direction="pas" rot="R90" />
              <pin name="2" x="-2.54" y="7.62" visible="pad" length="short" direction="pas" rot="R270" />
              <pin name="3" x="0" y="-7.62" visible="pad" length="short" direction="pas" rot="R90" />
              <pin name="4" x="0" y="7.62" visible="pad" length="short" direction="pas" rot="R270" />
              <pin name="5" x="2.54" y="-7.62" visible="pad" length="short" direction="pas" rot="R90" />
              <pin name="6" x="2.54" y="7.62" visible="pad" length="short" direction="pas" rot="R270" />
            </symbol>
          </symbols>
          <devicesets>
            <deviceset uservalue="yes" prefix="JP" name="JP3Q">
              <description />
              <gates>
                <gate name="B" symbol="J3" x="0" y="0" />
              </gates>
              <devices>
                <device package="JP3Q">
                  <connects>
                    <connect gate="B" pin="1" pad="1" />
                    <connect gate="B" pin="2" pad="2" />
                    <connect gate="B" pin="3" pad="3" />
                    <connect gate="B" pin="4" pad="4" />
                    <connect gate="B" pin="5" pad="5" />
                    <connect gate="B" pin="6" pad="6" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
          </devicesets>
        </library>
        <library name="avr-4">
          <description />
          <packages>
            <package name="TQFP32-08">
              <description>&lt;B&gt;Thin Plasic Quad Flat Package&lt;/B&gt; Grid 0.8 mm</description>
              <wire x1="3.505" y1="3.505" x2="3.505" y2="-3.505" width="0.1524" layer="21" />
              <wire x1="3.505" y1="-3.505" x2="-3.505" y2="-3.505" width="0.1524" layer="21" />
              <wire x1="-3.505" y1="-3.505" x2="-3.505" y2="3.15" width="0.1524" layer="21" />
              <wire x1="-3.15" y1="3.505" x2="3.505" y2="3.505" width="0.1524" layer="21" />
              <wire x1="-3.15" y1="3.505" x2="-3.505" y2="3.15" width="0.1524" layer="21" />
              <circle x="-2.7432" y="2.7432" radius="0.3592" width="0.1524" layer="21" />
              <smd name="1" x="-4.2926" y="2.8" dx="1.27" dy="0.5588" layer="1" />
              <smd name="2" x="-4.2926" y="2" dx="1.27" dy="0.5588" layer="1" />
              <smd name="3" x="-4.2926" y="1.2" dx="1.27" dy="0.5588" layer="1" />
              <smd name="4" x="-4.2926" y="0.4" dx="1.27" dy="0.5588" layer="1" />
              <smd name="5" x="-4.2926" y="-0.4" dx="1.27" dy="0.5588" layer="1" />
              <smd name="6" x="-4.2926" y="-1.2" dx="1.27" dy="0.5588" layer="1" />
              <smd name="7" x="-4.2926" y="-2" dx="1.27" dy="0.5588" layer="1" />
              <smd name="8" x="-4.2926" y="-2.8" dx="1.27" dy="0.5588" layer="1" />
              <smd name="9" x="-2.8" y="-4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="10" x="-2" y="-4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="11" x="-1.2" y="-4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="12" x="-0.4" y="-4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="13" x="0.4" y="-4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="14" x="1.2" y="-4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="15" x="2" y="-4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="16" x="2.8" y="-4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="17" x="4.2926" y="-2.8" dx="1.27" dy="0.5588" layer="1" />
              <smd name="18" x="4.2926" y="-2" dx="1.27" dy="0.5588" layer="1" />
              <smd name="19" x="4.2926" y="-1.2" dx="1.27" dy="0.5588" layer="1" />
              <smd name="20" x="4.2926" y="-0.4" dx="1.27" dy="0.5588" layer="1" />
              <smd name="21" x="4.2926" y="0.4" dx="1.27" dy="0.5588" layer="1" />
              <smd name="22" x="4.2926" y="1.2" dx="1.27" dy="0.5588" layer="1" />
              <smd name="23" x="4.2926" y="2" dx="1.27" dy="0.5588" layer="1" />
              <smd name="24" x="4.2926" y="2.8" dx="1.27" dy="0.5588" layer="1" />
              <smd name="25" x="2.8" y="4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="26" x="2" y="4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="27" x="1.2" y="4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="28" x="0.4" y="4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="29" x="-0.4" y="4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="30" x="-1.2" y="4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="31" x="-2" y="4.2926" dx="0.5588" dy="1.27" layer="1" />
              <smd name="32" x="-2.8" y="4.2926" dx="0.5588" dy="1.27" layer="1" />
              <text x="-2.7686" y="5.08" size="0.8128" layer="25">&gt;NAME</text>
              <text x="-3.0226" y="-1.27" size="0.8128" layer="27">&gt;VALUE</text>
              <rectangle x1="-4.5466" y1="2.5714" x2="-3.556" y2="3.0286" layer="51" />
              <rectangle x1="-4.5466" y1="1.7714" x2="-3.556" y2="2.2286" layer="51" />
              <rectangle x1="-4.5466" y1="0.9714" x2="-3.556" y2="1.4286" layer="51" />
              <rectangle x1="-4.5466" y1="0.1714" x2="-3.556" y2="0.6286" layer="51" />
              <rectangle x1="-4.5466" y1="-0.6286" x2="-3.556" y2="-0.1714" layer="51" />
              <rectangle x1="-4.5466" y1="-1.4286" x2="-3.556" y2="-0.9714" layer="51" />
              <rectangle x1="-4.5466" y1="-2.2286" x2="-3.556" y2="-1.7714" layer="51" />
              <rectangle x1="-4.5466" y1="-3.0286" x2="-3.556" y2="-2.5714" layer="51" />
              <rectangle x1="-3.0286" y1="-4.5466" x2="-2.5714" y2="-3.556" layer="51" />
              <rectangle x1="-2.2286" y1="-4.5466" x2="-1.7714" y2="-3.556" layer="51" />
              <rectangle x1="-1.4286" y1="-4.5466" x2="-0.9714" y2="-3.556" layer="51" />
              <rectangle x1="-0.6286" y1="-4.5466" x2="-0.1714" y2="-3.556" layer="51" />
              <rectangle x1="0.1714" y1="-4.5466" x2="0.6286" y2="-3.556" layer="51" />
              <rectangle x1="0.9714" y1="-4.5466" x2="1.4286" y2="-3.556" layer="51" />
              <rectangle x1="1.7714" y1="-4.5466" x2="2.2286" y2="-3.556" layer="51" />
              <rectangle x1="2.5714" y1="-4.5466" x2="3.0286" y2="-3.556" layer="51" />
              <rectangle x1="3.556" y1="-3.0286" x2="4.5466" y2="-2.5714" layer="51" />
              <rectangle x1="3.556" y1="-2.2286" x2="4.5466" y2="-1.7714" layer="51" />
              <rectangle x1="3.556" y1="-1.4286" x2="4.5466" y2="-0.9714" layer="51" />
              <rectangle x1="3.556" y1="-0.6286" x2="4.5466" y2="-0.1714" layer="51" />
              <rectangle x1="3.556" y1="0.1714" x2="4.5466" y2="0.6286" layer="51" />
              <rectangle x1="3.556" y1="0.9714" x2="4.5466" y2="1.4286" layer="51" />
              <rectangle x1="3.556" y1="1.7714" x2="4.5466" y2="2.2286" layer="51" />
              <rectangle x1="3.556" y1="2.5714" x2="4.5466" y2="3.0286" layer="51" />
              <rectangle x1="2.5714" y1="3.556" x2="3.0286" y2="4.5466" layer="51" />
              <rectangle x1="1.7714" y1="3.556" x2="2.2286" y2="4.5466" layer="51" />
              <rectangle x1="0.9714" y1="3.556" x2="1.4286" y2="4.5466" layer="51" />
              <rectangle x1="0.1714" y1="3.556" x2="0.6286" y2="4.5466" layer="51" />
              <rectangle x1="-0.6286" y1="3.556" x2="-0.1714" y2="4.5466" layer="51" />
              <rectangle x1="-1.4286" y1="3.556" x2="-0.9714" y2="4.5466" layer="51" />
              <rectangle x1="-2.2286" y1="3.556" x2="-1.7714" y2="4.5466" layer="51" />
              <rectangle x1="-3.0286" y1="3.556" x2="-2.5714" y2="4.5466" layer="51" />
            </package>
          </packages>
          <symbols>
            <symbol name="ATMEGA48/88/168-TQFP/QFN32">
              <description />
              <wire x1="-25.4" y1="30.48" x2="25.4" y2="30.48" width="0.254" layer="94" />
              <wire x1="25.4" y1="30.48" x2="25.4" y2="-33.02" width="0.254" layer="94" />
              <wire x1="25.4" y1="-33.02" x2="-25.4" y2="-33.02" width="0.254" layer="94" />
              <wire x1="-25.4" y1="-33.02" x2="-25.4" y2="30.48" width="0.254" layer="94" />
              <text x="-25.4" y="33.02" size="1.778" layer="95" rot="MR180">&gt;NAME</text>
              <text x="-25.4" y="-35.56" size="1.778" layer="96">&gt;VALUE</text>
              <pin name="PB5(SCK/PCINT5)" x="30.48" y="-30.48" length="middle" rot="R180" />
              <pin name="PB7(XTAL2/TOSC2/PCINT7)" x="-30.48" y="7.62" length="middle" />
              <pin name="PB6(XTAL1/TOSC1/PCINT6)" x="-30.48" y="12.7" length="middle" />
              <pin name="GND@1" x="-30.48" y="-27.94" length="middle" direction="pwr" />
              <pin name="VCC@1" x="-30.48" y="0" length="middle" direction="pwr" />
              <pin name="AGND" x="-30.48" y="-22.86" length="middle" direction="pwr" />
              <pin name="AREF" x="-30.48" y="-10.16" length="middle" direction="pas" />
              <pin name="AVCC" x="-30.48" y="-7.62" length="middle" direction="pas" />
              <pin name="PB4(MISO/PCINT4)" x="30.48" y="-27.94" length="middle" rot="R180" />
              <pin name="PB3(MOSI/OC2A/PCINT3)" x="30.48" y="-25.4" length="middle" rot="R180" />
              <pin name="PB2(SS/OC1B/PCINT2)" x="30.48" y="-22.86" length="middle" rot="R180" />
              <pin name="PB1(OC1A/PCINT1)" x="30.48" y="-20.32" length="middle" rot="R180" />
              <pin name="PB0(ICP1/CLKO/PCINT0)" x="30.48" y="-17.78" length="middle" rot="R180" />
              <pin name="PD7(AIN1/PCINT23)" x="30.48" y="-12.7" length="middle" rot="R180" />
              <pin name="PD6(AIN0/OC0A/PCINT22)" x="30.48" y="-10.16" length="middle" rot="R180" />
              <pin name="PD5(T1/OC0B/PCINT21)" x="30.48" y="-7.62" length="middle" rot="R180" />
              <pin name="PD4(T0/XCK/PCINT20)" x="30.48" y="-5.08" length="middle" rot="R180" />
              <pin name="PD3(INT1/OC2B/PCINT19)" x="30.48" y="-2.54" length="middle" rot="R180" />
              <pin name="PD2(INT0/PCINT18)" x="30.48" y="0" length="middle" rot="R180" />
              <pin name="PD1(TXD/PCINT17)" x="30.48" y="2.54" length="middle" rot="R180" />
              <pin name="PD0(RXD/PCINT16)" x="30.48" y="5.08" length="middle" rot="R180" />
              <pin name="PC5(ADC5/SCL/PCINT13)" x="30.48" y="15.24" length="middle" rot="R180" />
              <pin name="PC4(ADC4/SDA/PCINT12)" x="30.48" y="17.78" length="middle" rot="R180" />
              <pin name="PC3(ADC3/PCINT11)" x="30.48" y="20.32" length="middle" rot="R180" />
              <pin name="PC2(ADC2/PCINT10)" x="30.48" y="22.86" length="middle" rot="R180" />
              <pin name="PC1(ADC1/PCINT9)" x="30.48" y="25.4" length="middle" rot="R180" />
              <pin name="PC0(ADC0/PCINT8)" x="30.48" y="27.94" length="middle" rot="R180" />
              <pin name="PC6(/RESET/PCINT14)" x="-30.48" y="27.94" length="middle" />
              <pin name="GND@2" x="-30.48" y="-30.48" length="middle" direction="pwr" />
              <pin name="VCC@2" x="-30.48" y="-2.54" length="middle" direction="pwr" />
              <pin name="ADC7" x="30.48" y="10.16" length="middle" direction="pas" rot="R180" />
              <pin name="ADC6" x="30.48" y="12.7" length="middle" direction="pas" rot="R180" />
            </symbol>
          </symbols>
          <devicesets>
            <deviceset prefix="IC" name="MEGA48/88/168">
              <description />
              <gates>
                <gate name="1" symbol="ATMEGA48/88/168-TQFP/QFN32" x="0" y="0" />
              </gates>
              <devices>
                <device package="TQFP32-08" name="-AU">
                  <connects>
                    <connect gate="1" pin="ADC6" pad="19" />
                    <connect gate="1" pin="ADC7" pad="22" />
                    <connect gate="1" pin="AGND" pad="21" />
                    <connect gate="1" pin="AREF" pad="20" />
                    <connect gate="1" pin="AVCC" pad="18" />
                    <connect gate="1" pin="GND@1" pad="3" />
                    <connect gate="1" pin="GND@2" pad="5" />
                    <connect gate="1" pin="PB0(ICP1/CLKO/PCINT0)" pad="12" />
                    <connect gate="1" pin="PB1(OC1A/PCINT1)" pad="13" />
                    <connect gate="1" pin="PB2(SS/OC1B/PCINT2)" pad="14" />
                    <connect gate="1" pin="PB3(MOSI/OC2A/PCINT3)" pad="15" />
                    <connect gate="1" pin="PB4(MISO/PCINT4)" pad="16" />
                    <connect gate="1" pin="PB5(SCK/PCINT5)" pad="17" />
                    <connect gate="1" pin="PB6(XTAL1/TOSC1/PCINT6)" pad="7" />
                    <connect gate="1" pin="PB7(XTAL2/TOSC2/PCINT7)" pad="8" />
                    <connect gate="1" pin="PC0(ADC0/PCINT8)" pad="23" />
                    <connect gate="1" pin="PC1(ADC1/PCINT9)" pad="24" />
                    <connect gate="1" pin="PC2(ADC2/PCINT10)" pad="25" />
                    <connect gate="1" pin="PC3(ADC3/PCINT11)" pad="26" />
                    <connect gate="1" pin="PC4(ADC4/SDA/PCINT12)" pad="27" />
                    <connect gate="1" pin="PC5(ADC5/SCL/PCINT13)" pad="28" />
                    <connect gate="1" pin="PC6(/RESET/PCINT14)" pad="29" />
                    <connect gate="1" pin="PD0(RXD/PCINT16)" pad="30" />
                    <connect gate="1" pin="PD1(TXD/PCINT17)" pad="31" />
                    <connect gate="1" pin="PD2(INT0/PCINT18)" pad="32" />
                    <connect gate="1" pin="PD3(INT1/OC2B/PCINT19)" pad="1" />
                    <connect gate="1" pin="PD4(T0/XCK/PCINT20)" pad="2" />
                    <connect gate="1" pin="PD5(T1/OC0B/PCINT21)" pad="9" />
                    <connect gate="1" pin="PD6(AIN0/OC0A/PCINT22)" pad="10" />
                    <connect gate="1" pin="PD7(AIN1/PCINT23)" pad="11" />
                    <connect gate="1" pin="VCC@1" pad="4" />
                    <connect gate="1" pin="VCC@2" pad="6" />
                  </connects>
                  <technologies>
                    <technology name="" />
                  </technologies>
                </device>
              </devices>
            </deviceset>
          </devicesets>
        </library>
        <library name="con-cypressindustries">
          <description />
          <packages>
            <package name="32005-101">
              <description>&lt;b&gt;MINI USB 4P R/A SMT&lt;/b&gt; Two Salient Point&lt;p&gt;
Source: http://www.cypressindustries.com/pdf/32005-101.pdf</description>
              <wire x1="-3.5464" y1="3.6429" x2="-1.8857" y2="3.6429" width="0.1016" layer="21" />
              <wire x1="-1.8857" y1="3.6429" x2="-1.8857" y2="3.1125" width="0.1016" layer="21" />
              <wire x1="-1.8857" y1="-3.2125" x2="-1.8857" y2="-3.6428" width="0.1016" layer="21" />
              <wire x1="-1.8857" y1="-3.6428" x2="-3.5464" y2="-3.6428" width="0.1016" layer="21" />
              <wire x1="-3.5464" y1="-3.6428" x2="-3.5464" y2="3.6429" width="0.1016" layer="21" />
              <wire x1="-1.8321" y1="3.1072" x2="-0.4794" y2="3.1072" width="0.1016" layer="51" />
              <wire x1="-0.4794" y1="3.1072" x2="-0.4794" y2="4.4465" width="0.1016" layer="51" />
              <wire x1="-0.4794" y1="4.4465" x2="2.2661" y2="4.4465" width="0.1016" layer="51" />
              <wire x1="2.2661" y1="4.4465" x2="2.2661" y2="3.1072" width="0.1016" layer="51" />
              <wire x1="2.4269" y1="3.1072" x2="2.4269" y2="-3.2072" width="0.1016" layer="51" />
              <wire x1="2.4269" y1="-3.2072" x2="2.2661" y2="-3.2072" width="0.1016" layer="51" />
              <wire x1="2.2661" y1="-3.2072" x2="2.2661" y2="-4.4465" width="0.1016" layer="51" />
              <wire x1="2.2661" y1="-4.4465" x2="-0.466" y2="-4.4465" width="0.1016" layer="51" />
              <wire x1="-0.466" y1="-4.4465" x2="-0.466" y2="-3.2143" width="0.1016" layer="51" />
              <wire x1="-1.8321" y1="-3.2143" x2="-0.466" y2="-3.2143" width="0.1016" layer="51" />
              <wire x1="1.4626" y1="-3.234" x2="1.4626" y2="-3.9108" width="0.1016" layer="51" />
              <wire x1="1.4626" y1="-3.9108" x2="0.2304" y2="-3.9108" width="0.1016" layer="51" />
              <wire x1="0.2304" y1="-3.9108" x2="0.2304" y2="-3.234" width="0.1016" layer="51" />
              <wire x1="1.4626" y1="3.9108" x2="0.2304" y2="3.9108" width="0.1016" layer="51" />
              <wire x1="0.2304" y1="3.9108" x2="0.2304" y2="3.134" width="0.1016" layer="51" />
              <wire x1="1.4626" y1="3.1339" x2="1.4626" y2="3.9108" width="0.1016" layer="51" />
              <wire x1="-0.4794" y1="3.1072" x2="2.2661" y2="3.1072" width="0.1016" layer="51" />
              <wire x1="2.2661" y1="3.1072" x2="2.4269" y2="3.1072" width="0.1016" layer="51" />
              <wire x1="-0.466" y1="-3.2143" x2="2.429" y2="-3.2143" width="0.1016" layer="51" />
              <smd name="M1" x="0.85" y="3.875" dx="2.25" dy="3.8" layer="1" rot="R270" />
              <smd name="M2" x="0.85" y="-3.875" dx="2.25" dy="3.8" layer="1" rot="R270" />
              <smd name="1" x="3.15" y="1.2" dx="0.55" dy="2.5" layer="1" rot="R270" />
              <smd name="2" x="3.15" y="0.4" dx="0.55" dy="2.5" layer="1" rot="R270" />
              <smd name="3" x="3.15" y="-0.4" dx="0.55" dy="2.5" layer="1" rot="R270" />
              <smd name="4" x="3.15" y="-1.2" dx="0.55" dy="2.5" layer="1" rot="R270" />
              <text x="-2" y="5.5" size="1.27" layer="25">&gt;NAME</text>
              <text x="-2" y="-6.5" size="1.27" layer="27">&gt;VALUE</text>
              <rectangle x1="3.0125" y1="0.4125" x2="3.4125" y2="1.9875" layer="51" rot="R270" />
              <rectangle x1="3.025" y1="-0.375" x2="3.425" y2="1.175" layer="51" rot="R270" />
              <rectangle x1="3.025" y1="-1.175" x2="3.425" y2="0.375" layer="51" rot="R270" />
              <rectangle x1="3.0375" y1="-1.9625" x2="3.4375" y2="-0.4375" layer="51" rot="R270" />
              <hole x="0" y="1.5" drill="1" />
              <hole x="0" y="-1.5" drill="1" />
            </package>
          </packages>
          <symbols>
            <symbol name="MINI-USB-4P-">
              <description />
              <wire x1="-2.54" y1="6.35" x2="-2.54" y2="-3.81" width="0.254" layer="94" />
              <wire x1="-2.54" y1="-3.81" x2="-1.27" y2="-5.08" width="0.254" layer="94" curve="90" />
              <wire x1="-1.27" y1="-5.08" x2="5.08" y2="-5.08" width="0.254" layer="94" />
              <wire x1="5.08" y1="-5.08" x2="6.35" y2="-3.81" width="0.254" layer="94" curve="90" />
              <wire x1="6.35" y1="-3.81" x2="6.35" y2="6.35" width="0.254" layer="94" />
              <wire x1="-2.54" y1="6.35" x2="-1.27" y2="7.62" width="0.254" layer="94" curve="-90" />
              <wire x1="-1.27" y1="7.62" x2="5.08" y2="7.62" width="0.254" layer="94" />
              <wire x1="5.08" y1="7.62" x2="6.35" y2="6.35" width="0.254" layer="94" curve="-90" />
              <wire x1="0" y1="5.08" x2="0" y2="-2.54" width="0.254" layer="94" />
              <wire x1="0" y1="-2.54" x2="1.27" y2="-3.81" width="0.254" layer="94" />
              <wire x1="1.27" y1="-3.81" x2="2.54" y2="-3.81" width="0.254" layer="94" />
              <wire x1="3.81" y1="-2.54" x2="3.81" y2="5.08" width="0.254" layer="94" />
              <wire x1="2.54" y1="6.35" x2="1.27" y2="6.35" width="0.254" layer="94" />
              <wire x1="1.27" y1="6.35" x2="0" y2="5.08" width="0.254" layer="94" />
              <wire x1="2.54" y1="6.35" x2="3.81" y2="5.08" width="0.254" layer="94" />
              <wire x1="2.54" y1="-3.81" x2="3.81" y2="-2.54" width="0.254" layer="94" />
              <text x="-2.54" y="10.16" size="1.778" layer="95" font="vector">&gt;NAME</text>
              <text x="10.16" y="-5.08" size="1.778" layer="96" font="vector" rot="R90">&gt;VALUE</text>
              <pin name="1" x="-5.08" y="5.08" visible="pin" direction="pas" />
              <pin name="2" x="-5.08" y="2.54" visible="pin" direction="pas" />
              <pin name="3" x="-5.08" y="0" visible="pin" direction="pas" />
              <pin name="4" x="-5.08" y="-2.54" visible="pin" direction="pas" />
            </symbol>
          </symbols>
          <devicesets>
            <deviceset prefix="X" name="MINI-USB_4P-">
              <description />
              <gates>
                <gate name="G$1" symbol="MINI-USB-4P-" x="0" y="0" />
              </gates>
              <devices>
                <device package="32005-101" name="32005-101">
                  <connects>
                    <connect gate="G$1" pin="1" pad="1" />
                    <connect gate="G$1" pin="2" pad="2" />
                    <connect gate="G$1" pin="3" pad="3" />
                    <connect gate="G$1" pin="4" pad="4" />
                  </connects>
                  <technologies>
                    <technology name="">
                      <attribute name="MF" value="" />
                      <attribute name="MPN" value="" />
                      <attribute name="OC_FARNELL" value="unknown" />
                      <attribute name="OC_NEWARK" value="unknown" />
                    </technology>
                  </technologies>
                </device>
              </devices>
            </deviceset>
          </devicesets>
        </library>
        <library name="ftdichip">
          <description />
          <packages>
            <package name="SSOP28">
              <description>&lt;b&gt;Shrink Small Outline Package&lt;/b&gt; SSOP-28&lt;p&gt;
http://www.ftdichip.com/Documents/DataSheets/DS_FT232R_v104.pdf</description>
              <wire x1="-5.1" y1="-2.6" x2="5.1" y2="-2.6" width="0.2032" layer="21" />
              <wire x1="5.1" y1="-2.6" x2="5.1" y2="2.6" width="0.2032" layer="21" />
              <wire x1="5.1" y1="2.6" x2="-5.1" y2="2.6" width="0.2032" layer="21" />
              <wire x1="-5.1" y1="2.6" x2="-5.1" y2="-2.6" width="0.2032" layer="21" />
              <circle x="-4.2" y="-1.625" radius="0.4422" width="0" layer="21" />
              <smd name="1" x="-4.225" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="2" x="-3.575" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="3" x="-2.925" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="4" x="-2.275" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="5" x="-1.625" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="6" x="-0.975" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="7" x="-0.325" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="8" x="0.325" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="9" x="0.975" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="10" x="1.625" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="11" x="2.275" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="12" x="2.925" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="13" x="3.575" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="14" x="4.225" y="-3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="15" x="4.225" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="16" x="3.575" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="17" x="2.925" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="18" x="2.275" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="19" x="1.625" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="20" x="0.975" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="21" x="0.325" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="22" x="-0.325" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="23" x="-0.975" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="24" x="-1.625" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="25" x="-2.275" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="26" x="-2.925" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="27" x="-3.575" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <smd name="28" x="-4.225" y="3.625" dx="0.4" dy="1.5" layer="1" />
              <text x="-5.476" y="-2.6299" size="1.27" layer="25" rot="R90">&gt;NAME</text>
              <text x="-3.8999" y="-0.68" size="1.27" layer="27">&gt;VALUE</text>
              <rectangle x1="-4.4028" y1="-3.937" x2="-4.0472" y2="-2.6416" layer="51" />
              <rectangle x1="-3.7529" y1="-3.937" x2="-3.3973" y2="-2.6416" layer="51" />
              <rectangle x1="-3.1029" y1="-3.937" x2="-2.7473" y2="-2.6416" layer="51" />
              <rectangle x1="-2.4529" y1="-3.937" x2="-2.0973" y2="-2.6416" layer="51" />
              <rectangle x1="-1.8029" y1="-3.937" x2="-1.4473" y2="-2.6416" layer="51" />
              <rectangle x1="-1.1529" y1="-3.937" x2="-0.7973" y2="-2.6416" layer="51" />
              <rectangle x1="-0.5029" y1="-3.937" x2="-0.1473" y2="-2.6416" layer="51" />
              <rectangle x1="0.1473" y1="-3.937" x2="0.5029" y2="-2.6416" layer="51" />
              <rectangle x1="0.7973" y1="-3.937" x2="1.1529" y2="-2.6416" layer="51" />
              <rectangle x1="1.4473" y1="-3.937" x2="1.8029" y2="-2.6416" layer="51" />
              <rectangle x1="2.0973" y1="-3.937" x2="2.4529" y2="-2.6416" layer="51" />
              <rectangle x1="2.7473" y1="-3.937" x2="3.1029" y2="-2.6416" layer="51" />
              <rectangle x1="3.3973" y1="-3.937" x2="3.7529" y2="-2.6416" layer="51" />
              <rectangle x1="4.0472" y1="-3.937" x2="4.4028" y2="-2.6416" layer="51" />
              <rectangle x1="4.0472" y1="2.6416" x2="4.4028" y2="3.937" layer="51" />
              <rectangle x1="3.3973" y1="2.6416" x2="3.7529" y2="3.937" layer="51" />
              <rectangle x1="2.7473" y1="2.6416" x2="3.1029" y2="3.937" layer="51" />
              <rectangle x1="2.0973" y1="2.6416" x2="2.4529" y2="3.937" layer="51" />
              <rectangle x1="1.4473" y1="2.6416" x2="1.8029" y2="3.937" layer="51" />
              <rectangle x1="0.7973" y1="2.6416" x2="1.1529" y2="3.937" layer="51" />
              <rectangle x1="0.1473" y1="2.6416" x2="0.5029" y2="3.937" layer="51" />
              <rectangle x1="-0.5029" y1="2.6416" x2="-0.1473" y2="3.937" layer="51" />
              <rectangle x1="-1.1529" y1="2.6416" x2="-0.7973" y2="3.937" layer="51" />
              <rectangle x1="-1.8029" y1="2.6416" x2="-1.4473" y2="3.937" layer="51" />
              <rectangle x1="-2.4529" y1="2.6416" x2="-2.0973" y2="3.937" layer="51" />
              <rectangle x1="-3.1029" y1="2.6416" x2="-2.7473" y2="3.937" layer="51" />
              <rectangle x1="-3.7529" y1="2.6416" x2="-3.3973" y2="3.937" layer="51" />
              <rectangle x1="-4.4028" y1="2.6416" x2="-4.0472" y2="3.937" layer="51" />
            </package>
          </packages>
          <symbols>
            <symbol name="FT232R">
              <description />
              <wire x1="-10.16" y1="25.4" x2="12.7" y2="25.4" width="0.254" layer="94" />
              <wire x1="12.7" y1="25.4" x2="12.7" y2="-27.94" width="0.254" layer="94" />
              <wire x1="12.7" y1="-27.94" x2="-10.16" y2="-27.94" width="0.254" layer="94" />
              <wire x1="-10.16" y1="-27.94" x2="-10.16" y2="25.4" width="0.254" layer="94" />
              <text x="-10.16" y="26.67" size="1.778" layer="95">&gt;NAME</text>
              <text x="-10.16" y="-30.48" size="1.778" layer="96">&gt;VALUE</text>
              <pin name="VCC" x="-12.7" y="22.86" length="short" direction="pwr" />
              <pin name="3V3OUT" x="-12.7" y="-5.08" length="short" direction="out" />
              <pin name="USBDP" x="-12.7" y="-10.16" length="short" />
              <pin name="USBDM" x="-12.7" y="-12.7" length="short" />
              <pin name="OSCO" x="-12.7" y="7.62" length="short" direction="out" />
              <pin name="OSCI" x="-12.7" y="10.16" length="short" direction="in" />
              <pin name="GND" x="15.24" y="-20.32" length="short" direction="pwr" rot="R180" />
              <pin name="TXD" x="15.24" y="22.86" length="short" direction="out" rot="R180" />
              <pin name="RXD" x="15.24" y="20.32" length="short" direction="in" rot="R180" />
              <pin name="!RTS" x="15.24" y="17.78" length="short" direction="out" rot="R180" />
              <pin name="!CTS" x="15.24" y="15.24" length="short" direction="in" rot="R180" />
              <pin name="!DTR" x="15.24" y="12.7" length="short" direction="out" rot="R180" />
              <pin name="!DSR" x="15.24" y="10.16" length="short" direction="in" rot="R180" />
              <pin name="!DCD" x="15.24" y="7.62" length="short" direction="in" rot="R180" />
              <pin name="!RI" x="15.24" y="5.08" length="short" direction="in" rot="R180" />
              <pin name="CBUS0" x="15.24" y="0" length="short" rot="R180" />
              <pin name="CBUS1" x="15.24" y="-2.54" length="short" rot="R180" />
              <pin name="CBUS2" x="15.24" y="-5.08" length="short" rot="R180" />
              <pin name="CBUS3" x="15.24" y="-7.62" length="short" rot="R180" />
              <pin name="CBUS4" x="15.24" y="-10.16" length="short" rot="R180" />
              <pin name="VCCIO" x="-12.7" y="20.32" length="short" direction="pwr" />
              <pin name="!RESET" x="-12.7" y="15.24" length="short" direction="in" />
              <pin name="GND@A" x="-12.7" y="-17.78" length="short" direction="pwr" />
              <pin name="GND@1" x="15.24" y="-22.86" length="short" direction="pwr" rot="R180" />
              <pin name="TEST" x="15.24" y="-15.24" length="short" direction="in" rot="R180" />
              <pin name="GND@2" x="15.24" y="-25.4" length="short" direction="pwr" rot="R180" />
            </symbol>
          </symbols>
          <devicesets>
            <deviceset prefix="IC" name="FT232R">
              <description />
              <gates>
                <gate name="1" symbol="FT232R" x="0" y="0" />
              </gates>
              <devices>
                <device package="SSOP28" name="L">
                  <connects>
                    <connect gate="1" pin="!CTS" pad="11" />
                    <connect gate="1" pin="!DCD" pad="10" />
                    <connect gate="1" pin="!DSR" pad="9" />
                    <connect gate="1" pin="!DTR" pad="2" />
                    <connect gate="1" pin="!RESET" pad="19" />
                    <connect gate="1" pin="!RI" pad="6" />
                    <connect gate="1" pin="!RTS" pad="3" />
                    <connect gate="1" pin="3V3OUT" pad="17" />
                    <connect gate="1" pin="CBUS0" pad="23" />
                    <connect gate="1" pin="CBUS1" pad="22" />
                    <connect gate="1" pin="CBUS2" pad="13" />
                    <connect gate="1" pin="CBUS3" pad="14" />
                    <connect gate="1" pin="CBUS4" pad="12" />
                    <connect gate="1" pin="GND" pad="7" />
                    <connect gate="1" pin="GND@1" pad="18" />
                    <connect gate="1" pin="GND@2" pad="21" />
                    <connect gate="1" pin="GND@A" pad="25" />
                    <connect gate="1" pin="OSCI" pad="27" />
                    <connect gate="1" pin="OSCO" pad="28" />
                    <connect gate="1" pin="RXD" pad="5" />
                    <connect gate="1" pin="TEST" pad="26" />
                    <connect gate="1" pin="TXD" pad="1" />
                    <connect gate="1" pin="USBDM" pad="16" />
                    <connect gate="1" pin="USBDP" pad="15" />
                    <connect gate="1" pin="VCC" pad="20" />
                    <connect gate="1" pin="VCCIO" pad="4" />
                  </connects>
                  <technologies>
                    <technology name="">
                      <attribute name="MF" value="" />
                      <attribute name="MPN" value="FT232RL" />
                      <attribute name="OC_FARNELL" value="1146032" />
                      <attribute name="OC_NEWARK" value="91K9918" />
                    </technology>
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
        <part device="C0805" name="PulseOxy.AnalogSection.AnalogIn.C2" library="resistor" deviceset="C-US" />
        <part device="C0805" name="PulseOxy.AnalogSection.AnalogIn.C4" library="resistor" deviceset="C-US" />
        <part device="C0805" name="PulseOxy.AnalogSection.AnalogIn.Cbp1" library="resistor" deviceset="C-US" />
        <part device="C0805" name="PulseOxy.AnalogSection.AnalogIn.Cf1" library="resistor" deviceset="C-US" />
        <part device="C0805" name="PulseOxy.AnalogSection.AnalogIn.Cf2" library="resistor" deviceset="C-US" />
        <part device="D" technology="LM" name="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" library="linear" deviceset="*1458" />
        <part device="C0805" name="PulseOxy.AnalogSection.AnalogIn.CRef" library="resistor" deviceset="C-US" />
        <part device="C0603" name="PulseOxy.AnalogSection.AnalogIn.R11" library="resistor" deviceset="C-US" />
        <part device="C0603" name="PulseOxy.AnalogSection.AnalogIn.Rf1" library="resistor" deviceset="C-US" />
        <part device="C0603" name="PulseOxy.AnalogSection.AnalogIn.RF2" library="resistor" deviceset="C-US" />
        <part device="C0603" name="PulseOxy.AnalogSection.AnalogIn.RI2" library="resistor" deviceset="C-US" />
        <part device="C0603" name="PulseOxy.AnalogSection.AnalogIn.Rref1" library="resistor" deviceset="C-US" />
        <part device="C0603" name="PulseOxy.AnalogSection.AnalogIn.RRef2" library="resistor" deviceset="C-US" />
        <part device="C0805" name="PulseOxy.AnalogSection.LEDDrive.Cfilt1" library="resistor" deviceset="C-US" />
        <part device="C0805" name="PulseOxy.AnalogSection.LEDDrive.Cfilt2" library="resistor" deviceset="C-US" />
        <part device="R0603" name="PulseOxy.AnalogSection.LEDDrive.Rdrive1" library="rcl" deviceset="R-US_" />
        <part device="R0603" name="PulseOxy.AnalogSection.LEDDrive.Rdrive2" library="rcl" deviceset="R-US_" />
        <part device="R0603" name="PulseOxy.AnalogSection.LEDDrive.Rlim1" library="rcl" deviceset="R-US_" />
        <part device="R0603" name="PulseOxy.AnalogSection.LEDDrive.Rlim2" library="rcl" deviceset="R-US_" />
        <part device="SOT143" name="PulseOxy.AnalogSection.LEDDrive.T1" library="semicon-smd-ipc" deviceset="NPN-TRANSISTIOR-2COL" />
        <part device="SOT143" name="PulseOxy.AnalogSection.LEDDrive.T6" library="semicon-smd-ipc" deviceset="NPN-TRANSISTIOR-2COL" />
        <part device="" name="PulseOxy.AnalogSection.SensorHeader" library="jumper" deviceset="JP3Q" />
        <part device="C0805" name="PulseOxy.Bypass.BP1" library="resistor" deviceset="C-US" />
        <part device="C0805" name="PulseOxy.Bypass.BP2" library="resistor" deviceset="C-US" />
        <part device="C0805" name="PulseOxy.Bypass.BP3" library="resistor" deviceset="C-US" />
        <part device="-AU" name="PulseOxy.AtMEGA_U1" library="avr-4" deviceset="MEGA48/88/168" />
        <part device="32005-101" name="PulseOxy.Comp_32005_101" library="con-cypressindustries" deviceset="MINI-USB_4P-" />
        <part device="L" name="PulseOxy.FTDI_U2" library="ftdichip" deviceset="FT232R" />
        <part device="" name="PulseOxy.ProgrPort" library="jumper" deviceset="JP3Q" />
      </parts>
      <sheets>
        <sheet>
          <description />
          <plain />
          <instances>
            <instance y="541.545" part="PulseOxy.AnalogSection.AnalogIn.C2" gate="G$1" x="413.996" />
            <instance y="434.107" part="PulseOxy.AnalogSection.AnalogIn.C4" gate="G$1" x="332.096" />
            <instance y="396.961" part="PulseOxy.AnalogSection.AnalogIn.Cbp1" gate="G$1" x="311.796" />
            <instance y="467.253" part="PulseOxy.AnalogSection.AnalogIn.Cf1" gate="G$1" x="404.606" />
            <instance y="504.399" part="PulseOxy.AnalogSection.AnalogIn.Cf2" gate="G$1" x="404.606" />
            <instance y="450.68" part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="A" x="384.731" />
            <instance y="450.68" part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="P" x="384.731" />
            <instance y="450.68" part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="B" x="407.591" />
            <instance y="385.061" part="PulseOxy.AnalogSection.AnalogIn.CRef" gate="G$1" x="311.796" />
            <instance y="529.163" part="PulseOxy.AnalogSection.AnalogIn.R11" gate="G$1" x="404.606" />
            <instance y="479.635" part="PulseOxy.AnalogSection.AnalogIn.Rf1" gate="G$1" x="404.606" />
            <instance y="492.017" part="PulseOxy.AnalogSection.AnalogIn.RF2" gate="G$1" x="404.606" />
            <instance y="516.781" part="PulseOxy.AnalogSection.AnalogIn.RI2" gate="G$1" x="404.606" />
            <instance y="421.725" part="PulseOxy.AnalogSection.AnalogIn.Rref1" gate="G$1" x="330.696" />
            <instance y="409.343" part="PulseOxy.AnalogSection.AnalogIn.RRef2" gate="G$1" x="330.696" />
            <instance y="288.357" part="PulseOxy.AnalogSection.LEDDrive.Cfilt1" gate="G$1" x="205.332" />
            <instance y="315.915" part="PulseOxy.AnalogSection.LEDDrive.Cfilt2" gate="G$1" x="270.432" />
            <instance y="268.29" part="PulseOxy.AnalogSection.LEDDrive.Rdrive1" gate="G$1" x="182.432" />
            <instance y="296.548" part="PulseOxy.AnalogSection.LEDDrive.Rdrive2" gate="G$1" x="250.332" />
            <instance y="296.548" part="PulseOxy.AnalogSection.LEDDrive.Rlim1" gate="G$1" x="210.92" />
            <instance y="324.106" part="PulseOxy.AnalogSection.LEDDrive.Rlim2" gate="G$1" x="276.02" />
            <instance y="274.578" part="PulseOxy.AnalogSection.LEDDrive.T1" gate="G$1" x="198.538" />
            <instance y="302.136" part="PulseOxy.AnalogSection.LEDDrive.T6" gate="G$1" x="263.638" />
            <instance y="340.488" part="PulseOxy.AnalogSection.SensorHeader" gate="B" x="292.402" />
            <instance y="63.691" part="PulseOxy.Bypass.BP1" gate="G$1" x="35.7" />
            <instance y="94.973" part="PulseOxy.Bypass.BP2" gate="G$1" x="36.4" />
            <instance y="82.591" part="PulseOxy.Bypass.BP3" gate="G$1" x="35.7" />
            <instance y="160.059" part="PulseOxy.AtMEGA_U1" gate="1" x="87.51601" />
            <instance y="633.013" part="PulseOxy.Comp_32005_101" gate="G$1" x="474.73" />
            <instance y="598.2791" part="PulseOxy.FTDI_U2" gate="1" x="455.363" />
            <instance y="115.546" part="PulseOxy.ProgrPort" gate="B" x="47.194" />
          </instances>
          <busses />
          <nets>
            <net name="N$0">
              <segment>
                <wire x1="413.996" y1="536.465" x2="413.996" y2="534.965" width="0.3" layer="91" />
                <label x="413.996" y="534.965" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.C2" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="205.332" y1="283.277" x2="205.332" y2="281.777" width="0.3" layer="91" />
                <label x="205.332" y="281.777" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Cfilt1" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="270.432" y1="310.835" x2="270.432" y2="309.335" width="0.3" layer="91" />
                <label x="270.432" y="309.335" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Cfilt2" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="201.078" y1="279.658" x2="201.078" y2="281.158" width="0.3" layer="91" />
                <label x="201.078" y="281.158" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.T1" gate="G$1" pin="C" />
              </segment>
              <segment>
                <wire x1="266.178" y1="307.216" x2="266.178" y2="308.716" width="0.3" layer="91" />
                <label x="266.178" y="308.716" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.T6" gate="G$1" pin="C" />
              </segment>
              <segment>
                <wire x1="311.796" y1="391.881" x2="311.796" y2="390.381" width="0.3" layer="91" />
                <label x="311.796" y="390.381" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Cbp1" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="330.696" y1="424.265" x2="330.696" y2="425.765" width="0.3" layer="91" />
                <label x="330.696" y="425.765" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Rref1" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="384.731" y1="443.06" x2="384.731" y2="444.56" width="0.3" layer="91" />
                <label x="384.731" y="444.56" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="P" pin="V-" />
              </segment>
              <segment>
                <wire x1="311.796" y1="379.981" x2="311.796" y2="378.481" width="0.3" layer="91" />
                <label x="311.796" y="378.481" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.CRef" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="294.942" y1="348.108" x2="294.942" y2="349.608" width="0.3" layer="91" />
                <label x="294.942" y="349.608" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.SensorHeader" gate="B" pin="6" />
              </segment>
            </net>
            <net name="N$1">
              <segment>
                <wire x1="413.996" y1="544.085" x2="413.996" y2="545.585" width="0.3" layer="91" />
                <label x="413.996" y="545.585" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.C2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="399.971" y1="453.22" x2="401.471" y2="453.22" width="0.3" layer="91" />
                <label x="401.471" y="453.22" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="B" pin="+IN" />
              </segment>
              <segment>
                <wire x1="404.606" y1="524.0831" x2="404.606" y2="522.5831" width="0.3" layer="91" />
                <label x="404.606" y="522.5831" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.R11" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$2">
              <segment>
                <wire x1="332.096" y1="429.027" x2="332.096" y2="427.527" width="0.3" layer="91" />
                <label x="332.096" y="427.527" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.C4" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="377.111" y1="448.14" x2="378.611" y2="448.14" width="0.3" layer="91" />
                <label x="378.611" y="448.14" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="A" pin="-IN" />
              </segment>
              <segment>
                <wire x1="404.606" y1="469.793" x2="404.606" y2="471.293" width="0.3" layer="91" />
                <label x="404.606" y="471.293" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Cf1" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="294.942" y1="332.868" x2="294.942" y2="334.368" width="0.3" layer="91" />
                <label x="294.942" y="334.368" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.SensorHeader" gate="B" pin="5" />
              </segment>
              <segment>
                <wire x1="404.606" y1="482.175" x2="404.606" y2="483.675" width="0.3" layer="91" />
                <label x="404.606" y="483.675" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Rf1" gate="G$1" pin="1" />
              </segment>
            </net>
            <net name="N$3">
              <segment>
                <wire x1="332.096" y1="436.647" x2="332.096" y2="438.147" width="0.3" layer="91" />
                <label x="332.096" y="438.147" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.C4" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="377.111" y1="453.22" x2="378.611" y2="453.22" width="0.3" layer="91" />
                <label x="378.611" y="453.22" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="A" pin="+IN" />
              </segment>
              <segment>
                <wire x1="330.696" y1="411.883" x2="330.696" y2="413.383" width="0.3" layer="91" />
                <label x="330.696" y="413.383" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.RRef2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="311.796" y1="387.601" x2="311.796" y2="389.101" width="0.3" layer="91" />
                <label x="311.796" y="389.101" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.CRef" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="330.696" y1="416.645" x2="330.696" y2="415.145" width="0.3" layer="91" />
                <label x="330.696" y="415.145" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Rref1" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="292.402" y1="348.108" x2="292.402" y2="349.608" width="0.3" layer="91" />
                <label x="292.402" y="349.608" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.SensorHeader" gate="B" pin="4" />
              </segment>
            </net>
            <net name="N$4">
              <segment>
                <wire x1="311.796" y1="399.501" x2="311.796" y2="401.001" width="0.3" layer="91" />
                <label x="311.796" y="401.001" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Cbp1" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="289.862" y1="332.868" x2="289.862" y2="334.368" width="0.3" layer="91" />
                <label x="289.862" y="334.368" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.SensorHeader" gate="B" pin="1" />
              </segment>
              <segment>
                <wire x1="330.696" y1="404.263" x2="330.696" y2="402.763" width="0.3" layer="91" />
                <label x="330.696" y="402.763" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.RRef2" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="384.731" y1="458.3" x2="384.731" y2="459.8" width="0.3" layer="91" />
                <label x="384.731" y="459.8" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="P" pin="V+" />
              </segment>
            </net>
            <net name="N$5">
              <segment>
                <wire x1="404.606" y1="462.173" x2="404.606" y2="460.673" width="0.3" layer="91" />
                <label x="404.606" y="460.673" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Cf1" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="117.996" y1="170.219" x2="119.496" y2="170.219" width="0.3" layer="91" />
                <label x="119.496" y="170.219" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="ADC7" />
              </segment>
              <segment>
                <wire x1="404.606" y1="519.321" x2="404.606" y2="520.821" width="0.3" layer="91" />
                <label x="404.606" y="520.821" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.RI2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="392.351" y1="450.68" x2="393.851" y2="450.68" width="0.3" layer="91" />
                <label x="393.851" y="450.68" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="A" pin="OUT" />
              </segment>
              <segment>
                <wire x1="404.606" y1="474.555" x2="404.606" y2="473.055" width="0.3" layer="91" />
                <label x="404.606" y="473.055" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Rf1" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$6">
              <segment>
                <wire x1="404.606" y1="499.319" x2="404.606" y2="497.819" width="0.3" layer="91" />
                <label x="404.606" y="497.819" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Cf2" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="117.996" y1="172.759" x2="119.496" y2="172.759" width="0.3" layer="91" />
                <label x="119.496" y="172.759" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="ADC6" />
              </segment>
              <segment>
                <wire x1="404.606" y1="486.937" x2="404.606" y2="485.437" width="0.3" layer="91" />
                <label x="404.606" y="485.437" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.RF2" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="415.211" y1="450.68" x2="416.711" y2="450.68" width="0.3" layer="91" />
                <label x="416.711" y="450.68" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="B" pin="OUT" />
              </segment>
            </net>
            <net name="N$7">
              <segment>
                <wire x1="404.606" y1="506.939" x2="404.606" y2="508.439" width="0.3" layer="91" />
                <label x="404.606" y="508.439" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Cf2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="399.971" y1="448.14" x2="401.471" y2="448.14" width="0.3" layer="91" />
                <label x="401.471" y="448.14" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.Comp_D_1458OpAmp" gate="B" pin="-IN" />
              </segment>
              <segment>
                <wire x1="404.606" y1="494.557" x2="404.606" y2="496.057" width="0.3" layer="91" />
                <label x="404.606" y="496.057" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.RF2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="404.606" y1="511.701" x2="404.606" y2="510.201" width="0.3" layer="91" />
                <label x="404.606" y="510.201" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.RI2" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$8">
              <segment>
                <wire x1="404.606" y1="531.7031" x2="404.606" y2="533.2031" width="0.3" layer="91" />
                <label x="404.606" y="533.2031" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.AnalogIn.R11" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="117.996" y1="134.659" x2="119.496" y2="134.659" width="0.3" layer="91" />
                <label x="119.496" y="134.659" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="PB3(MOSI/OC2A/PCINT3)" />
              </segment>
              <segment>
                <wire x1="47.194" y1="123.166" x2="47.194" y2="124.666" width="0.3" layer="91" />
                <label x="47.194" y="124.666" size="1.27" layer="95" />
                <pinref part="PulseOxy.ProgrPort" gate="B" pin="4" />
              </segment>
            </net>
            <net name="N$9">
              <segment>
                <wire x1="205.332" y1="290.897" x2="205.332" y2="292.397" width="0.3" layer="91" />
                <label x="205.332" y="292.397" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Cfilt1" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="195.998" y1="274.578" x2="197.498" y2="274.578" width="0.3" layer="91" />
                <label x="197.498" y="274.578" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.T1" gate="G$1" pin="B" />
              </segment>
              <segment>
                <wire x1="187.512" y1="268.29" x2="189.012" y2="268.29" width="0.3" layer="91" />
                <label x="189.012" y="268.29" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Rdrive1" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$10">
              <segment>
                <wire x1="270.432" y1="318.455" x2="270.432" y2="319.955" width="0.3" layer="91" />
                <label x="270.432" y="319.955" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Cfilt2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="261.098" y1="302.136" x2="262.598" y2="302.136" width="0.3" layer="91" />
                <label x="262.598" y="302.136" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.T6" gate="G$1" pin="B" />
              </segment>
              <segment>
                <wire x1="255.412" y1="296.548" x2="256.912" y2="296.548" width="0.3" layer="91" />
                <label x="256.912" y="296.548" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Rdrive2" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$11">
              <segment>
                <wire x1="177.352" y1="268.29" x2="178.852" y2="268.29" width="0.3" layer="91" />
                <label x="178.852" y="268.29" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Rdrive1" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="117.996" y1="139.739" x2="119.496" y2="139.739" width="0.3" layer="91" />
                <label x="119.496" y="139.739" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="PB1(OC1A/PCINT1)" />
              </segment>
            </net>
            <net name="N$12">
              <segment>
                <wire x1="245.252" y1="296.548" x2="246.752" y2="296.548" width="0.3" layer="91" />
                <label x="246.752" y="296.548" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Rdrive2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="117.996" y1="137.199" x2="119.496" y2="137.199" width="0.3" layer="91" />
                <label x="119.496" y="137.199" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="PB2(SS/OC1B/PCINT2)" />
              </segment>
            </net>
            <net name="N$13">
              <segment>
                <wire x1="216" y1="296.548" x2="217.5" y2="296.548" width="0.3" layer="91" />
                <label x="217.5" y="296.548" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Rlim1" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="289.862" y1="348.108" x2="289.862" y2="349.608" width="0.3" layer="91" />
                <label x="289.862" y="349.608" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.SensorHeader" gate="B" pin="2" />
              </segment>
            </net>
            <net name="N$14">
              <segment>
                <wire x1="205.84" y1="296.548" x2="207.34" y2="296.548" width="0.3" layer="91" />
                <label x="207.34" y="296.548" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Rlim1" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="201.078" y1="269.498" x2="201.078" y2="270.998" width="0.3" layer="91" />
                <label x="201.078" y="270.998" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.T1" gate="G$1" pin="E" />
              </segment>
            </net>
            <net name="N$15">
              <segment>
                <wire x1="270.94" y1="324.106" x2="272.44" y2="324.106" width="0.3" layer="91" />
                <label x="272.44" y="324.106" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Rlim2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="266.178" y1="297.056" x2="266.178" y2="298.556" width="0.3" layer="91" />
                <label x="266.178" y="298.556" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.T6" gate="G$1" pin="E" />
              </segment>
            </net>
            <net name="N$16">
              <segment>
                <wire x1="281.1" y1="324.106" x2="282.6" y2="324.106" width="0.3" layer="91" />
                <label x="282.6" y="324.106" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.LEDDrive.Rlim2" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="292.402" y1="332.868" x2="292.402" y2="334.368" width="0.3" layer="91" />
                <label x="292.402" y="334.368" size="1.27" layer="95" />
                <pinref part="PulseOxy.AnalogSection.SensorHeader" gate="B" pin="3" />
              </segment>
            </net>
            <net name="N$17">
              <segment>
                <wire x1="35.7" y1="66.231" x2="35.7" y2="67.731" width="0.3" layer="91" />
                <label x="35.7" y="67.731" size="1.27" layer="95" />
                <pinref part="PulseOxy.Bypass.BP1" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="44.654" y1="123.166" x2="44.654" y2="124.666" width="0.3" layer="91" />
                <label x="44.654" y="124.666" size="1.27" layer="95" />
                <pinref part="PulseOxy.ProgrPort" gate="B" pin="2" />
              </segment>
              <segment>
                <wire x1="57.036" y1="152.439" x2="58.536" y2="152.439" width="0.3" layer="91" />
                <label x="58.536" y="152.439" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="AVCC" />
              </segment>
              <segment>
                <wire x1="469.65" y1="638.093" x2="468.15" y2="638.093" width="0.3" layer="91" />
                <label x="468.15" y="638.093" size="1.27" layer="95" />
                <pinref part="PulseOxy.Comp_32005_101" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="442.663" y1="621.139" x2="444.163" y2="621.139" width="0.3" layer="91" />
                <label x="444.163" y="621.139" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="VCC" />
              </segment>
              <segment>
                <wire x1="442.663" y1="613.519" x2="444.163" y2="613.519" width="0.3" layer="91" />
                <label x="444.163" y="613.519" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="!RESET" />
              </segment>
              <segment>
                <wire x1="442.663" y1="618.599" x2="444.163" y2="618.599" width="0.3" layer="91" />
                <label x="444.163" y="618.599" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="VCCIO" />
              </segment>
              <segment>
                <wire x1="57.036" y1="157.519" x2="58.536" y2="157.519" width="0.3" layer="91" />
                <label x="58.536" y="157.519" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="VCC@2" />
              </segment>
              <segment>
                <wire x1="57.036" y1="160.059" x2="58.536" y2="160.059" width="0.3" layer="91" />
                <label x="58.536" y="160.059" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="VCC@1" />
              </segment>
              <segment>
                <wire x1="36.4" y1="97.513" x2="36.4" y2="99.013" width="0.3" layer="91" />
                <label x="36.4" y="99.013" size="1.27" layer="95" />
                <pinref part="PulseOxy.Bypass.BP2" gate="G$1" pin="1" />
              </segment>
              <segment>
                <wire x1="35.7" y1="85.131" x2="35.7" y2="86.631" width="0.3" layer="91" />
                <label x="35.7" y="86.631" size="1.27" layer="95" />
                <pinref part="PulseOxy.Bypass.BP3" gate="G$1" pin="1" />
              </segment>
            </net>
            <net name="N$18">
              <segment>
                <wire x1="35.7" y1="58.611" x2="35.7" y2="57.111" width="0.3" layer="91" />
                <label x="35.7" y="57.111" size="1.27" layer="95" />
                <pinref part="PulseOxy.Bypass.BP1" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="49.734" y1="123.166" x2="49.734" y2="124.666" width="0.3" layer="91" />
                <label x="49.734" y="124.666" size="1.27" layer="95" />
                <pinref part="PulseOxy.ProgrPort" gate="B" pin="6" />
              </segment>
              <segment>
                <wire x1="57.036" y1="137.199" x2="58.536" y2="137.199" width="0.3" layer="91" />
                <label x="58.536" y="137.199" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="AGND" />
              </segment>
              <segment>
                <wire x1="469.65" y1="630.473" x2="468.15" y2="630.473" width="0.3" layer="91" />
                <label x="468.15" y="630.473" size="1.27" layer="95" />
                <pinref part="PulseOxy.Comp_32005_101" gate="G$1" pin="4" />
              </segment>
              <segment>
                <wire x1="470.603" y1="575.419" x2="472.103" y2="575.419" width="0.3" layer="91" />
                <label x="472.103" y="575.419" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="GND@1" />
              </segment>
              <segment>
                <wire x1="442.663" y1="580.499" x2="444.163" y2="580.499" width="0.3" layer="91" />
                <label x="444.163" y="580.499" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="GND@A" />
              </segment>
              <segment>
                <wire x1="470.603" y1="577.959" x2="472.103" y2="577.959" width="0.3" layer="91" />
                <label x="472.103" y="577.959" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="GND" />
              </segment>
              <segment>
                <wire x1="470.603" y1="605.899" x2="472.103" y2="605.899" width="0.3" layer="91" />
                <label x="472.103" y="605.899" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="!DCD" />
              </segment>
              <segment>
                <wire x1="470.603" y1="610.979" x2="472.103" y2="610.979" width="0.3" layer="91" />
                <label x="472.103" y="610.979" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="!DTR" />
              </segment>
              <segment>
                <wire x1="470.603" y1="608.439" x2="472.103" y2="608.439" width="0.3" layer="91" />
                <label x="472.103" y="608.439" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="!DSR" />
              </segment>
              <segment>
                <wire x1="470.603" y1="603.359" x2="472.103" y2="603.359" width="0.3" layer="91" />
                <label x="472.103" y="603.359" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="!RI" />
              </segment>
              <segment>
                <wire x1="470.603" y1="583.039" x2="472.103" y2="583.039" width="0.3" layer="91" />
                <label x="472.103" y="583.039" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="TEST" />
              </segment>
              <segment>
                <wire x1="470.603" y1="613.519" x2="472.103" y2="613.519" width="0.3" layer="91" />
                <label x="472.103" y="613.519" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="!CTS" />
              </segment>
              <segment>
                <wire x1="470.603" y1="572.879" x2="472.103" y2="572.879" width="0.3" layer="91" />
                <label x="472.103" y="572.879" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="GND@2" />
              </segment>
              <segment>
                <wire x1="57.036" y1="129.579" x2="58.536" y2="129.579" width="0.3" layer="91" />
                <label x="58.536" y="129.579" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="GND@2" />
              </segment>
              <segment>
                <wire x1="57.036" y1="132.119" x2="58.536" y2="132.119" width="0.3" layer="91" />
                <label x="58.536" y="132.119" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="GND@1" />
              </segment>
              <segment>
                <wire x1="36.4" y1="89.893" x2="36.4" y2="88.393" width="0.3" layer="91" />
                <label x="36.4" y="88.393" size="1.27" layer="95" />
                <pinref part="PulseOxy.Bypass.BP2" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="35.7" y1="77.51099" x2="35.7" y2="76.01099" width="0.3" layer="91" />
                <label x="35.7" y="76.01099" size="1.27" layer="95" />
                <pinref part="PulseOxy.Bypass.BP3" gate="G$1" pin="2" />
              </segment>
            </net>
            <net name="N$19">
              <segment>
                <wire x1="117.996" y1="165.139" x2="119.496" y2="165.139" width="0.3" layer="91" />
                <label x="119.496" y="165.139" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="PD0(RXD/PCINT16)" />
              </segment>
              <segment>
                <wire x1="470.603" y1="621.139" x2="472.103" y2="621.139" width="0.3" layer="91" />
                <label x="472.103" y="621.139" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="TXD" />
              </segment>
            </net>
            <net name="N$20">
              <segment>
                <wire x1="117.996" y1="162.599" x2="119.496" y2="162.599" width="0.3" layer="91" />
                <label x="119.496" y="162.599" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="PD1(TXD/PCINT17)" />
              </segment>
              <segment>
                <wire x1="470.603" y1="618.599" x2="472.103" y2="618.599" width="0.3" layer="91" />
                <label x="472.103" y="618.599" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="RXD" />
              </segment>
            </net>
            <net name="N$21">
              <segment>
                <wire x1="57.036" y1="187.999" x2="58.536" y2="187.999" width="0.3" layer="91" />
                <label x="58.536" y="187.999" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="PC6(/RESET/PCINT14)" />
              </segment>
              <segment>
                <wire x1="49.734" y1="107.926" x2="49.734" y2="109.426" width="0.3" layer="91" />
                <label x="49.734" y="109.426" size="1.27" layer="95" />
                <pinref part="PulseOxy.ProgrPort" gate="B" pin="5" />
              </segment>
            </net>
            <net name="N$22">
              <segment>
                <wire x1="117.996" y1="132.119" x2="119.496" y2="132.119" width="0.3" layer="91" />
                <label x="119.496" y="132.119" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="PB4(MISO/PCINT4)" />
              </segment>
              <segment>
                <wire x1="44.654" y1="107.926" x2="44.654" y2="109.426" width="0.3" layer="91" />
                <label x="44.654" y="109.426" size="1.27" layer="95" />
                <pinref part="PulseOxy.ProgrPort" gate="B" pin="1" />
              </segment>
            </net>
            <net name="N$23">
              <segment>
                <wire x1="117.996" y1="129.579" x2="119.496" y2="129.579" width="0.3" layer="91" />
                <label x="119.496" y="129.579" size="1.27" layer="95" />
                <pinref part="PulseOxy.AtMEGA_U1" gate="1" pin="PB5(SCK/PCINT5)" />
              </segment>
              <segment>
                <wire x1="47.194" y1="107.926" x2="47.194" y2="109.426" width="0.3" layer="91" />
                <label x="47.194" y="109.426" size="1.27" layer="95" />
                <pinref part="PulseOxy.ProgrPort" gate="B" pin="3" />
              </segment>
            </net>
            <net name="N$24">
              <segment>
                <wire x1="469.65" y1="633.013" x2="468.15" y2="633.013" width="0.3" layer="91" />
                <label x="468.15" y="633.013" size="1.27" layer="95" />
                <pinref part="PulseOxy.Comp_32005_101" gate="G$1" pin="3" />
              </segment>
              <segment>
                <wire x1="442.663" y1="588.119" x2="444.163" y2="588.119" width="0.3" layer="91" />
                <label x="444.163" y="588.119" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="USBDP" />
              </segment>
            </net>
            <net name="N$25">
              <segment>
                <wire x1="469.65" y1="635.553" x2="468.15" y2="635.553" width="0.3" layer="91" />
                <label x="468.15" y="635.553" size="1.27" layer="95" />
                <pinref part="PulseOxy.Comp_32005_101" gate="G$1" pin="2" />
              </segment>
              <segment>
                <wire x1="442.663" y1="585.579" x2="444.163" y2="585.579" width="0.3" layer="91" />
                <label x="444.163" y="585.579" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="USBDM" />
              </segment>
            </net>
            <net name="N$26">
              <segment>
                <wire x1="442.663" y1="605.899" x2="444.163" y2="605.899" width="0.3" layer="91" />
                <label x="444.163" y="605.899" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="OSCO" />
              </segment>
              <segment>
                <wire x1="442.663" y1="608.439" x2="444.163" y2="608.439" width="0.3" layer="91" />
                <label x="444.163" y="608.439" size="1.27" layer="95" />
                <pinref part="PulseOxy.FTDI_U2" gate="1" pin="OSCI" />
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
