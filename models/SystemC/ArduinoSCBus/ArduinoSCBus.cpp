/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simplified Arduino / Atmel megaAVR microcontroller model	 ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////
#include "ArduinoSCBus.h"
#pragma comment(lib,"ws2_32.lib")

/*
ArduinoSCBus* ArduinoSCBus::current = NULL;
#define TICK do {wait(clk.posedge_event());} while (false)
*/

ArduinoSCBus::ArduinoSCBus(sc_module_name _name, ArduinoAPI::setupfn _setup,
    ArduinoAPI::loopfn _loop) : Arduino(_name, _setup, _loop)
{
	i_comm = new SCBus("SCBus");

	i_comm->rst(rst);
	i_comm->clk(clk);

	i_comm->tx_data_wr(tx_data_wr);
	i_comm->tx_data(tx_data);
	i_comm->tx_empty(tx_empty);

	i_comm->rx_data_rd(rx_data_rd);
	i_comm->rx_data(rx_data);
	i_comm->rx_avail(rx_avail);

    // app_state.initialize(APP_STATE_PRESETUP);
}

ArduinoSCBus::~ArduinoSCBus()
{
	if (i_comm) {
		delete i_comm;
	}
}
