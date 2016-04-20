/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple GPIO Model										     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
#include "gpio.h"


void gpio::input_sync()
{
	if (!rst.read()) {
		sync1_reg.write(0);
		sync2_reg.write(0);
	}
	else {
		sc_lv<8> tmp = pin.read();
		for (int i = 0; i < tmp.length(); i++) {
			if (tmp.get_bit(i) == sc_dt::Log_Z) {
				tmp.set_bit(i, sc_dt::Log_1);	// Pullup logic
			}
		}
		sync1_reg.write(tmp);
		sync2_reg.write(sync1_reg.read());
	}
	data_in.write(sync2_reg.read());
}

void gpio::output_update()
{
	if (!rst.read()) {
	}
	else if (clk.posedge()) {
		if (data_out_wr.read()) {
			data_reg.write(data_out.read());
		}
		if (oe_wr.read()) {
			oe_reg.write(oe.read());
		}
	}

	sc_lv<8> tmp = data_reg.read();
	for (int i = 0; i < tmp.length(); i++) {
		if (!oe_reg.read().bit(i)) {
			tmp.set_bit(i, sc_dt::Log_Z);
		}
	}
	pin.write(tmp);
}