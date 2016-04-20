/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simplified Arduino / Atmel megaAVR microcontroller model	 ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Sandor Szilvasi, MetaMorph, Inc.                   ////
////          sszilvasi@metamorphsoftware.com                    ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef ARDUINOUART_H
#define ARDUINOUART_H

#include <systemc.h>
#include <Arduino/Arduino.h>
#include <UART/uart.h>

class ArduinoUART : public Arduino
{
public:
    sc_out<bool>  txd;
    sc_in<bool>	  rxd;

    //static ArduinoUART* current;
    SC_HAS_PROCESS(ArduinoUART);
    ArduinoUART(sc_module_name _name, ArduinoAPI::setupfn _setup, ArduinoAPI::loopfn _loop);
    ~ArduinoUART();

private:

    uart* i_comm;
};

#endif // ARDUINOUART_H