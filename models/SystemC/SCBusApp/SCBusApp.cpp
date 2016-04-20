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
#include "SCBusApp.h"

namespace SCBusAppFirmware {
using namespace ArduinoAPI;

#include "../../Arduino/SCBusApp/SCBusApp.ino"

} // namespace firmware

SCBusApp::SCBusApp(sc_module_name mname) : 
	sc_module(mname)
{
	i_arduino = new ArduinoSCBus("SCBusApp", SCBusAppFirmware::setup, SCBusAppFirmware::loop );

    i_arduino->d0(d0);
	i_arduino->d1(d1);
	i_arduino->d2(d2);
	i_arduino->d3(d3);
	i_arduino->d4(d4);
	i_arduino->d5(d5);
	i_arduino->d6(d6);
	i_arduino->d7(d7);
	i_arduino->d8(d8);
	i_arduino->d9(pwm0);
	i_arduino->d10(pwm1);
	i_arduino->d11(pwm2);
	i_arduino->d12(d12);
	i_arduino->d13(d13);
	i_arduino->a0(analog0);
	i_arduino->a1(analog1);
	i_arduino->a2(a2);
	i_arduino->a3(a3);
	i_arduino->a4(a4);
	i_arduino->a5(a5);

	i_arduino->app_state(app_state);
}

SCBusApp::~SCBusApp()
{
	if (i_arduino) {
		delete i_arduino;
	}
}
