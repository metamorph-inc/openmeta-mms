/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Pulse Oxymeter Application (uC modul + software)           ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////

#ifndef PULSEOXYAPP_H
#define PULSEOXYAPP_H

#include <systemc.h>
#include <ArduinoUART/ArduinoUART.h>

SC_MODULE(PulseOxyApp) {

	typedef sc_uint<10>	analog_type; 

	// Ports
	sc_out<bool>  txd;
	sc_in<bool>	  rxd;

	sc_out<sc_logic>  ir_led_pwm;
	sc_out<sc_logic>  red_led_pwm;
	sc_out<sc_logic>  dc_pwm;

	sc_in<analog_type>	raw_analog;
	sc_in<analog_type>	filt_analog;

	sc_out<Arduino::app_state_type>	app_state;

	SC_CTOR(PulseOxyApp);
	~PulseOxyApp();

private:
	ArduinoUART*	i_arduino;

    // Signals to bind unused Arduino ports
    sc_signal<sc_logic> d0, d1, d2, d3, d4, d5, d6, d7, d8, d12, d13;
    sc_signal<analog_type> a2, a3, a4, a5;
};

#endif // PULSEOXYAPP_H