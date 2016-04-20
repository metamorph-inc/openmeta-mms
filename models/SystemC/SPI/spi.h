/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple SPI Model											 ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef SPI_H
#define SPI_H

#include <systemc.h>

SC_MODULE(spi) {

	// ports
	sc_in<bool>   clk;
	sc_in<bool>   rst;

	/*
	sc_out<bool>	sclk;
	sc_out<bool>	mosi;
	sc_in<bool>		miso;


	sc_in<sc_uint<8>>   data_out;
	sc_in<bool>			data_out_wr;
	sc_out<sc_uint<8>>	data_in;
	sc_out<bool>	data_in_rdy;

	*/
	SC_CTOR(spi) {
		
	}

//private:
};

#endif // SPI_H