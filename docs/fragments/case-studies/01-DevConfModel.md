___
## LED Matrix Model

* Overview
    * Uses Ara Standard Interface
    * Includes a microcontroller to control the display and communicate with the Android app
    	* Features a design space of microcontrollers
    * Good test platform for measuring current draw: varies strongly with behavior

* **Still TODO:**
    * Some components are wrong (wrong EAGLE model type)
    * Microcontroller component connections -- are the correct & complete?
    * Get thermal working on whole thing
    * Have ProtoModule separated from LEDModule example
    * A USB (as opposed to Ara Interface) version so people could actually prototype their design
    	* _"How to turn Arduino thinking into Ara thinking. Here are the things you switch out."_
* Components
* Component Assemblies
    * ProtoModule
        * **Path:** `/C_Modules_PointDesigns/ProtoModule`
        * This design implements the Ara Standard Interface. A microcontroller has been included 
    * DevConModuleDesign
        * **Path:** `/C_Modules_PointDesigns/DevConModule`
        * Could be a good "step-by-step" exercise to build this up.
* Design Space
* Test Benches
    * This model features a number of interesting Test Benches for testing a design.
    * A_Placement_Layout
    * AA1_ChipFit_TestBench
        * **Path:** `/E_NewTestBenches/AA1_ChipFit_TestBench`
        * This test bench checks for your module to fix within a 2x1 space. Especially interesting with a design space.
        * Only trust a "No" answer. It can give false positives.
    * AA6_PlaceAndRoute_TestBench
        * **Path:** `/E_NewTestBenches/AA6_PlaceAndRoute_TestBench`
        * This test bench will build an EAGLE schematic of your design. It will also generate an EAGLE board, place your components based on constraints supplied in your design, and route your nets. EAGLE's routing engine is used for the net routing.
        * This process can take up to 5 minutes per design.
    * AA7_CADPCB
        * **Path:** `/E_NewTestBenches/AA7_CADPCB`
        * This test bench will do an auto-placement of a PCB, and then build a 3D CAD model of your module.
        * _*NOTE:* This capability currently requires a licenses copy of PTC's CREO tool._
* Interesting Stuff