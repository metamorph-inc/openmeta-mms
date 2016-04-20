/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Android Application for SCBus Testing (uC modul + software)////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////

#ifndef SCBUSAPP_H
#define SCBUSAPP_H

#include <systemc.h>

#include <ArduinoSCBus/ArduinoSCBus.h>

SC_MODULE(SCBusApp) {

	typedef sc_uint<10>	analog_type; 

	// Ports
	sc_out<sc_logic>  pwm0;
	sc_out<sc_logic>  pwm1;
	sc_out<sc_logic>  pwm2;

	sc_in<analog_type>	analog0;
	sc_in<analog_type>	analog1;

	sc_out<ArduinoSCBus::app_state_type>	app_state;

	SC_CTOR(SCBusApp);
	~SCBusApp();

private:
	ArduinoSCBus*	i_arduino;

    // Signals to bind unused Arduino ports
    sc_signal<sc_logic> d0, d1, d2, d3, d4, d5, d6, d7, d8, d12, d13;
    sc_signal<analog_type> a2, a3, a4, a5;

};

#endif // SCBUSAPP_H