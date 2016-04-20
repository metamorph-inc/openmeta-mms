/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simplified Arduino / Atmel megaAVR microcontroller model	 ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////  Author: Sandor Szilvasi, MetaMorph, Inc.                   ////
////          sszilvasi@metamorphsoftware.com                    ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#include "ArduinoUART.h"

//ArduinoUART* ArduinoUART::current = NULL; // Arduino only?

ArduinoUART::ArduinoUART(sc_module_name _name, ArduinoAPI::setupfn _setup,
    ArduinoAPI::loopfn _loop) : Arduino(_name, _setup, _loop)
{
    i_comm = new uart("UART");

    i_comm->txd(txd);
	i_comm->rxd(rxd);

    i_comm->rst(rst);
    i_comm->clk(clk);

    i_comm->tx_data_wr(tx_data_wr);
    i_comm->tx_data(tx_data);
    i_comm->tx_empty(tx_empty);

    i_comm->rx_data_rd(rx_data_rd);
    i_comm->rx_data(rx_data);
    i_comm->rx_avail(rx_avail);

    //app_state.initialize(APP_STATE_PRESETUP); // Should this be called in Arduino?
}

ArduinoUART::~ArduinoUART()
{
    if (i_comm) {
        delete i_comm;
    }
}
