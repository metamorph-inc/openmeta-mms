/////////////////////////////////////////////////////////////////////
////                                                             ////
////  SystemC Power on Reset Logic								 ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////

#ifndef POR_H
#define POR_H

#include <systemc.h>

SC_MODULE(por) {
	sc_out<bool>	rst;

	SC_CTOR(por) {
		SC_THREAD(main);
		rst.initialize(false);
	}

	void main() {
		wait(100, SC_US);
		rst.write(true);
	}
};

#endif // POR_H