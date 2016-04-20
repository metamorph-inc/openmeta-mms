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

#ifndef ARDUINOSCBUS_H
#define ARDUINOSCBUS_H

#include <systemc.h>
#include <Arduino/Arduino.h>
#include <SCBus/SCBus.h>

class ArduinoSCBus : public Arduino
{
public:

    //static ArduinoSCBus* current;
    SC_HAS_PROCESS(ArduinoSCBus);
    ArduinoSCBus(sc_module_name _name, ArduinoAPI::setupfn _setup, ArduinoAPI::loopfn _loop);
    ~ArduinoSCBus();

private:

    SCBus*	i_comm;
};

#endif // ARDUINOSCBUS_H