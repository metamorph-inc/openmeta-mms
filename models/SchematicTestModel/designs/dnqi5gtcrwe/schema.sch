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
        <part device="" value="LTST-C191KRKT" name="LED3$1" library="Andres_Project" deviceset="LED" />
        <part device="" value="LTST-C191KRKT" name="LED4$1" library="Andres_Project" deviceset="LED" />
        <part device="" value="LTST-C191KRKT" name="LED1$1" library="Andres_Project" deviceset="LED" />
        <part device="" value="LTST-C191KRKT" name="LED2$1" library="Andres_Project" deviceset="LED" />
      </parts>
      <sheets>
        <sheet>
          <description />
          <plain />
          <instances>
            <instance y="38.10" part="LED3" gate="G$1" x="63.50" />
            <instance y="45.72" part="LED4" gate="G$1" x="81.28" />
            <instance y="20.32" part="LED1" gate="G$1" x="25.40" />
            <instance y="60.96" part="LED2" gate="G$1" x="43.18" />
            <instance y="45.72" part="LED3$1" gate="G$1" x="147.32" />
            <instance y="53.34" part="LED4$1" gate="G$1" x="165.10" />
            <instance y="27.94" part="LED1$1" gate="G$1" x="109.22" />
            <instance y="68.58" part="LED2$1" gate="G$1" x="127.00" />
          </instances>
          <busses />
          <nets>
            <net name="N$0">
              <segment>
                <wire x1="63.50" y1="45.72" x2="60.96" y2="45.72" width="0.3" layer="91" />
                <label x="60.96" y="45.72" size="1.27" layer="95" />
                <pinref part="LED3" gate="G$1" pin="CATHODE" />
              </segment>
              <segment>
                <wire x1="81.28" y1="53.34" x2="78.74" y2="53.34" width="0.3" layer="91" />
                <label x="78.74" y="53.34" size="1.27" layer="95" />
                <pinref part="LED4" gate="G$1" pin="CATHODE" />
              </segment>
              <segment>
                <wire x1="43.18" y1="68.58" x2="40.64" y2="68.58" width="0.3" layer="91" />
                <label x="40.64" y="68.58" size="1.27" layer="95" />
                <pinref part="LED2" gate="G$1" pin="CATHODE" />
              </segment>
            </net>
            <net name="N$1">
              <segment>
                <wire x1="63.50" y1="35.56" x2="60.96" y2="35.56" width="0.3" layer="91" />
                <label x="60.96" y="35.56" size="1.27" layer="95" />
                <pinref part="LED3" gate="G$1" pin="ANODE" />
              </segment>
              <segment>
                <wire x1="81.28" y1="43.18" x2="78.74" y2="43.18" width="0.3" layer="91" />
                <label x="78.74" y="43.18" size="1.27" layer="95" />
                <pinref part="LED4" gate="G$1" pin="ANODE" />
              </segment>
              <segment>
                <wire x1="25.40" y1="17.78" x2="22.86" y2="17.78" width="0.3" layer="91" />
                <label x="22.86" y="17.78" size="1.27" layer="95" />
                <pinref part="LED1" gate="G$1" pin="ANODE" />
              </segment>
            </net>
            <net name="N$2">
              <segment>
                <wire x1="147.32" y1="53.34" x2="144.78" y2="53.34" width="0.3" layer="91" />
                <label x="144.78" y="53.34" size="1.27" layer="95" />
                <pinref part="LED3$1" gate="G$1" pin="CATHODE" />
              </segment>
              <segment>
                <wire x1="127.00" y1="76.20" x2="124.46" y2="76.20" width="0.3" layer="91" />
                <label x="124.46" y="76.20" size="1.27" layer="95" />
                <pinref part="LED2$1" gate="G$1" pin="CATHODE" />
              </segment>
              <segment>
                <wire x1="165.10" y1="60.96" x2="162.56" y2="60.96" width="0.3" layer="91" />
                <label x="162.56" y="60.96" size="1.27" layer="95" />
                <pinref part="LED4$1" gate="G$1" pin="CATHODE" />
              </segment>
            </net>
            <net name="N$3">
              <segment>
                <wire x1="147.32" y1="43.18" x2="144.78" y2="43.18" width="0.3" layer="91" />
                <label x="144.78" y="43.18" size="1.27" layer="95" />
                <pinref part="LED3$1" gate="G$1" pin="ANODE" />
              </segment>
              <segment>
                <wire x1="165.10" y1="50.80" x2="162.56" y2="50.80" width="0.3" layer="91" />
                <label x="162.56" y="50.80" size="1.27" layer="95" />
                <pinref part="LED4$1" gate="G$1" pin="ANODE" />
              </segment>
              <segment>
                <wire x1="109.22" y1="25.40" x2="106.68" y2="25.40" width="0.3" layer="91" />
                <label x="106.68" y="25.40" size="1.27" layer="95" />
                <pinref part="LED1$1" gate="G$1" pin="ANODE" />
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
