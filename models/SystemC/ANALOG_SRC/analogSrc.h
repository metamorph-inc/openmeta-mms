/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple ANALOG SRC	                			     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////

#ifndef ANALOGSRC_H
#define ANALOGSRC_H


#include <systemc.h>
#define MAX_SIG_LEN (32768 * 16)
#define MAX_SIG_NUM 8

SC_MODULE(analogSrc) {

	typedef sc_uint<10>	analog_type; 
	// ports
	//sc_in<sc_logic> pwm;
	sc_out<analog_type>	analog0;
	sc_out<analog_type>	analog1;
	sc_out<analog_type>	analog2;
	sc_out<analog_type>	analog3;
	sc_out<analog_type>	analog4;
	sc_out<analog_type>	analog5;
	sc_out<analog_type>	analog6;
	sc_out<analog_type>	analog7;

	SC_CTOR(analogSrc);


//private:
	double		full_scale;
	int         init_file;
	sc_time		pos_time;
	sc_time		neg_time;
	float *time_scale;
	float *data_trace[MAX_SIG_NUM];

	void analog_update();
//			sensitive << clk.pos();

};

#endif // ANALOGSRC_H