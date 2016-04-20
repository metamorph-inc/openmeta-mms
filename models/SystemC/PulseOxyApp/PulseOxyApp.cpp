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
#include "PulseOxyApp.h"

namespace PulseOxy {
using namespace ArduinoAPI;

#include "../../Arduino/PulseOxy/PulseOxy.ino"

} // namespace firmware

PulseOxyApp::PulseOxyApp(sc_module_name mname) : 
	sc_module(mname)
{
	i_arduino = new ArduinoUART("PulseOxy", PulseOxy::setup, PulseOxy::loop);
	
	i_arduino->txd(txd);
	i_arduino->rxd(rxd);
	i_arduino->d0(d0);
	i_arduino->d1(d1);
	i_arduino->d2(d2);
	i_arduino->d3(d3);
	i_arduino->d4(d4);
	i_arduino->d5(d5);
	i_arduino->d6(d6);
	i_arduino->d7(d7);
	i_arduino->d8(d8);
	i_arduino->d9(ir_led_pwm);
	i_arduino->d10(red_led_pwm);
	i_arduino->d11(dc_pwm);
	i_arduino->d12(d12);
	i_arduino->d13(d13);
	i_arduino->a0(raw_analog);
	i_arduino->a1(filt_analog);
	i_arduino->a2(a2);
	i_arduino->a3(a3);
	i_arduino->a4(a4);
	i_arduino->a5(a5);
	i_arduino->app_state(app_state);
}

PulseOxyApp::~PulseOxyApp()
{
	if (i_arduino) {
		delete i_arduino;
	}
}
