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
              <smd name="ANODE" x="0.75" y="0" dx="0.8" dy="0.8" layer="1" rot="R180" />
              <smd name="CATHODE" x="-0.75" y="0" dx="0.8" dy="0.8" layer="1" rot="R180" />
              <wire x1="1.5" y1="-0.7" x2="-1.5" y2="-0.7" width="0.1542" layer="21" />
              <wire x1="-1.5" y1="-0.7" x2="-1.5" y2="0.7" width="0.1542" layer="21" />
              <wire x1="-1.5" y1="0.7" x2="1.5" y2="0.7" width="0.1542" layer="21" />
              <wire x1="1.5" y1="0.7" x2="1.5" y2="-0.7" width="0.1542" layer="21" />
              <circle x="-1.25" y="-1.05" radius="0.1542" width="0" layer="21" />
              <text x="-2" y="1.25" size="1.016" layer="25" font="vector" ratio="15">&gt;NAME</text>
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
              <text x="-5.08" y="10.16" size="1.778" layer="95">&gt;NAME</text>
              <text x="-5.08" y="-5.08" size="1.778" layer="96">&gt;VALUE</text>
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
          </devicesets>
        </library>
      </libraries>
      <attributes />
      <variantdefs />
      <classes>
        <class number="0" name="default" />
      </classes>
      <parts>
        <part device="" value="" name="D1" library="Andres_Project" deviceset="LED" />
        <part device="" value="RC1608J181CS" name="R1" library="resistor" deviceset="RESISTOR_0603" />
      </parts>
      <sheets>
        <sheet>
          <description />
          <plain />
          <instances>
            <instance y="15.24" part="D1" gate="G$1" x="40.64" />
            <instance y="7.62" part="R1" gate="G$1" x="22.86" />
          </instances>
          <busses />
          <nets>
            <net name="N$0">
              <segment>
                <wire x1="40.64" y1="12.70" x2="38.10" y2="12.70" width="0.3" layer="91" />
                <label x="38.10" y="12.70" size="1.27" layer="95" />
                <pinref part="D1" gate="G$1" pin="ANODE" />
              </segment>
              <segment>
                <wire x1="27.94" y1="7.62" x2="30.48" y2="7.62" width="0.3" layer="91" />
                <label x="30.48" y="7.62" size="1.27" layer="95" />
                <pinref part="R1" gate="G$1" pin="2" />
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
