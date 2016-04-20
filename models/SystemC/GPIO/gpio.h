/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple GPIO Model											 ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef GPIO_H
#define GPIO_H

#include <systemc.h>

SC_MODULE(gpio) {

	// ports
	sc_in<bool>   clk;
	sc_in<bool>   rst;

	sc_inout<sc_lv<8>>	pin;

	sc_in<sc_uint<8>>   data_out;
	sc_in<bool>			data_out_wr;
	sc_in<sc_uint<8>>   oe;
	sc_in<bool>			oe_wr;
	sc_out<sc_uint<8>>	data_in;


	SC_CTOR(gpio) : data_reg(0), oe_reg(0) {
		SC_METHOD(input_sync);
		sensitive << clk.pos() << rst;
		SC_METHOD(output_update);
		sensitive << clk.pos() << rst;
		
	}

//private:
	sc_signal<sc_uint<8>>	data_reg;
	sc_signal<sc_uint<8>>	oe_reg;
	sc_signal<sc_uint<8>>	sync1_reg, sync2_reg;	// double synchronizer

	void input_sync();
	void output_update();
};

#endif // GPIO_H