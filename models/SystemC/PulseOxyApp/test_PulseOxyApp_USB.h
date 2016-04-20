/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Pulse Oxymeter Application (uC modul + software) testbench ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef TEST_PULSEOXYAPP_USB_H
#define TEST_PULSEOXYAPP_USB_H

#include <systemc.h>
#include <PulseOxyApp/PulseOxyApp.h>
#include <FT232R/usb_defines.h>
#include <FT232R/usb_phy.h>
#include <FT232R/usb_ep0.h>
#include <POR/por.h>

SC_MODULE(test_PulseOxyApp_USB) {

	// DUT ports
	sc_out<bool>	tx_dp, tx_dn, tx_oe;
	sc_in<bool>		rx_dp, rx_dn, rx_d;

	// Signals
	sc_clock		clk;
	sc_signal<bool>	rst;
	sc_signal<bool>	vcc;

	sc_signal<bool>			phy_usb_rst_nc, phy_txoe_nc;
	sc_signal<sc_uint<2> >	phy_line_nc;
	sc_signal<bool>			phy_rx_valid, phy_rx_active, phy_rx_error;
	sc_signal<bool>			phy_tx_valid, phy_tx_ready;
	sc_signal<sc_uint<8> >	phy_rx_data, phy_tx_data;

	sc_signal<bool> setup_pid;

	// Local Vars
	int error_cnt;
	
	sc_uint<8> txmem[2049];
	sc_uint<8> test_buffer[16385];
	sc_uint<8> host_buffer[16385];
	sc_uint<16> host_buffer_last;

	usb_phy		i_phy;
	por			i_por;

	
/////////////////////////////////////////////////////////////////////
////                                                             ////
////              Test Bench Library                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

void show_errors(void) {
	cout << "+----------------------+" << endl;
	cout << "| TOTAL ERRORS: " << error_cnt << endl;
	cout << "+----------------------+" << endl << endl;
}



sc_uint<5> crc5(sc_uint<5> crc_in, sc_uint<11> din) {
	sc_uint<5> crc_out;

	crc_out[0] = din[10] ^ din[9] ^ din[6] ^ din[5] ^ din[3] ^
			din[0] ^ crc_in[0] ^ crc_in[3] ^ crc_in[4];
	crc_out[1] = din[10] ^ din[7] ^ din[6] ^ din[4] ^ din[1] ^
			crc_in[0] ^ crc_in[1] ^ crc_in[4];
	crc_out[2] = din[10] ^ din[9] ^ din[8] ^ din[7] ^ din[6] ^
			din[3] ^ din[2] ^ din[0] ^ crc_in[0] ^
			crc_in[1] ^ crc_in[2] ^ crc_in[3] ^ crc_in[4];
	crc_out[3] = din[10] ^ din[9] ^ din[8] ^ din[7] ^ din[4] ^
			din[3] ^ din[1] ^ crc_in[1] ^ crc_in[2] ^
			crc_in[3] ^ crc_in[4];
	crc_out[4] = din[10] ^ din[9] ^ din[8] ^ din[5] ^ din[4] ^
			din[2] ^ crc_in[2] ^ crc_in[3] ^ crc_in[4];

	return crc_out;
}

sc_uint<16> crc16(sc_uint<16> crc_in, sc_uint<8> din) {
	sc_uint<16> crc_out;

	crc_out[0] = din[7] ^ din[6] ^ din[5] ^ din[4] ^ din[3] ^ din[2] ^
			din[1] ^ din[0] ^ crc_in[8] ^ crc_in[9] ^ crc_in[10] ^
			crc_in[11] ^ crc_in[12] ^ crc_in[13] ^ crc_in[14] ^
			crc_in[15];
	crc_out[1] = din[7] ^ din[6] ^ din[5] ^ din[4] ^ din[3] ^ din[2] ^
			din[1] ^ crc_in[9] ^ crc_in[10] ^ crc_in[11] ^ crc_in[12] ^
			crc_in[13] ^ crc_in[14] ^ crc_in[15];
	crc_out[2] = din[1] ^ din[0] ^ crc_in[8] ^ crc_in[9];
	crc_out[3] = din[2] ^ din[1] ^ crc_in[9] ^ crc_in[10];
	crc_out[4] = din[3] ^ din[2] ^ crc_in[10] ^ crc_in[11];
	crc_out[5] = din[4] ^ din[3] ^ crc_in[11] ^ crc_in[12];
	crc_out[6] = din[5] ^ din[4] ^ crc_in[12] ^ crc_in[13];
	crc_out[7] = din[6] ^ din[5] ^ crc_in[13] ^ crc_in[14];
	crc_out[8] = din[7] ^ din[6] ^ crc_in[0] ^ crc_in[14] ^ crc_in[15];
	crc_out[9] = din[7] ^ crc_in[1] ^ crc_in[15];
	crc_out[10] = crc_in[2];
	crc_out[11] = crc_in[3];
	crc_out[12] = crc_in[4];
	crc_out[13] = crc_in[5];
	crc_out[14] = crc_in[6];
	crc_out[15] = din[7] ^ din[6] ^ din[5] ^ din[4] ^ din[3] ^
			din[2] ^ din[1] ^ din[0] ^ crc_in[7] ^ crc_in[8] ^
			crc_in[9] ^ crc_in[10] ^ crc_in[11] ^ crc_in[12] ^
			crc_in[13] ^ crc_in[14] ^ crc_in[15];

	return crc_out;
}

void utmi_send_pack(int size) {
	int n;

	wait(clk.posedge_event());
	phy_tx_valid.write(true);
	for (n = 0; n < size; n++) {
		phy_tx_data.write(txmem[n]);
		wait(clk.posedge_event());
		while (!phy_tx_ready.read())
			wait(clk.posedge_event());
	}
	phy_tx_valid.write(false);
	wait(clk.posedge_event());
}

void utmi_recv_pack(int *size) {
	*size = 0;
	while (!phy_rx_active.read())
		wait(clk.posedge_event());
	while (phy_rx_active.read()) {
		while (!phy_rx_valid.read() && phy_rx_active.read())
			wait(clk.posedge_event());
		if (phy_rx_valid.read() && phy_rx_active.read()) {
			txmem[*size] = phy_rx_data.read();
			(*size)++;
		}
		wait(clk.posedge_event());
	}
}

void recv_packet(sc_uint<4> *pid, int *size) {
	int n;
	sc_uint<16> crc16r;
	sc_uint<8> x, y;

	crc16r = 0xffff;
	utmi_recv_pack(size);

	if (*size != 1) {
		for (n = 1; n < *size - 2; n++) {
			y = txmem[n];
			x = (	(sc_uint<1>)y[0],
					(sc_uint<1>)y[1],
					(sc_uint<1>)y[2],
					(sc_uint<1>)y[3],
						(sc_uint<1>)y[4],
					(sc_uint<1>)y[5],
					(sc_uint<1>)y[6],
					(sc_uint<1>)y[7]);
			crc16r = crc16(crc16r, x);
		}

		crc16r = (	(sc_uint<1>)!crc16r[8],
					(sc_uint<1>)!crc16r[9],
					(sc_uint<1>)!crc16r[10],
					(sc_uint<1>)!crc16r[11],
					(sc_uint<1>)!crc16r[12],
					(sc_uint<1>)!crc16r[13],
					(sc_uint<1>)!crc16r[14],
					(sc_uint<1>)!crc16r[15],
					(sc_uint<1>)!crc16r[0],
					(sc_uint<1>)!crc16r[1],
					(sc_uint<1>)!crc16r[2],
					(sc_uint<1>)!crc16r[3],
					(sc_uint<1>)!crc16r[4],
					(sc_uint<1>)!crc16r[5],
					(sc_uint<1>)!crc16r[6],
					(sc_uint<1>)!crc16r[7]);

		if (crc16r != (sc_uint<16>)(txmem[n], txmem[n + 1]))
			cout << "ERROR: CRC Mismatch: Expected: " << crc16r << ", Got: " <<
					txmem[n] << txmem[n + 1] << " (" << sc_time_stamp() << ")" << endl << endl;

		for (n = 0; n < *size - 3; n++)
			host_buffer[host_buffer_last + n] = txmem[n + 1];
		host_buffer_last = host_buffer_last + n;
	} else {
		*size = 3;
	}

	x = txmem[0];

	if ((sc_uint<4>)x.range(7, 4) != (sc_uint<4>)~x.range(3, 0))
		cout << "ERROR: Pid Checksum mismatch: Top: " << (sc_uint<4>)x.range(7, 4) <<
				" Bottom: " << (sc_uint<4>)x.range(3, 0) << " (" << sc_time_stamp() << ")" << endl << endl;

	*pid = (sc_uint<4>)x.range(3, 0);
	*size = *size - 3;
}

void send_token(sc_uint<7> fa, sc_uint<4> ep, sc_uint<4> pid) {
	sc_uint<16> tmp_data;
	sc_uint<11> x, y;
	int len;

	tmp_data = ((sc_uint<7>)fa, (sc_uint<4>)ep, (sc_uint<5>)0);
	if (pid == USBF_T_PID_ACK)
		len = 1;
	else
		len = 3;

	y = ((sc_uint<7>)fa, (sc_uint<4>)ep);
	x = (	(sc_uint<1>)y[4],
			(sc_uint<1>)y[5],
			(sc_uint<1>)y[6],
			(sc_uint<1>)y[7],
			(sc_uint<1>)y[8],
			(sc_uint<1>)y[9],
			(sc_uint<1>)y[10],
			(sc_uint<1>)y[0],
			(sc_uint<1>)y[1],
			(sc_uint<1>)y[2],
			(sc_uint<1>)y[3]);

	y = ((sc_uint<6>)0, (sc_uint<5>)crc5(0x1f, x));
	tmp_data = ((sc_uint<11>)x, (sc_uint<5>)~y.range(4, 0));
	txmem[0] = ((sc_uint<4>)~pid, (sc_uint<4>)pid);
	txmem[1] = (	(sc_uint<1>)tmp_data[8],
					(sc_uint<1>)tmp_data[9],
					(sc_uint<1>)tmp_data[10],
					(sc_uint<1>)tmp_data[11],
					(sc_uint<1>)tmp_data[12],
					(sc_uint<1>)tmp_data[13],
					(sc_uint<1>)tmp_data[14],
					(sc_uint<1>)tmp_data[15]);
	txmem[2] = (	(sc_uint<1>)tmp_data[0],
					(sc_uint<1>)tmp_data[1],
					(sc_uint<1>)tmp_data[2],
					(sc_uint<1>)tmp_data[3],
					(sc_uint<1>)tmp_data[4],
					(sc_uint<1>)tmp_data[5],
					(sc_uint<1>)tmp_data[6],
					(sc_uint<1>)tmp_data[7]);

	utmi_send_pack(len);
}

void send_sof(sc_uint<11> frmn) {
	sc_uint<16> tmp_data;
	sc_uint<11> x;

	x = (	(sc_uint<1>)frmn[0],
			(sc_uint<1>)frmn[1],
			(sc_uint<1>)frmn[2],
			(sc_uint<1>)frmn[3],
			(sc_uint<1>)frmn[4],
			(sc_uint<1>)frmn[5],
			(sc_uint<1>)frmn[6],
			(sc_uint<1>)frmn[7],
			(sc_uint<1>)frmn[8],
			(sc_uint<1>)frmn[9],
			(sc_uint<1>)frmn[10]);

	tmp_data = ((sc_uint<11>)x, (sc_uint<5>)~crc5(0x1f, x));
	txmem[0] = ((sc_uint<4>)~USBF_T_PID_SOF, (sc_uint<4>)USBF_T_PID_SOF);
//	txmem[1] = (	(sc_uint<1>)tmp_data[8],
//					(sc_uint<1>)tmp_data[9],
//					(sc_uint<1>)tmp_data[10],
//					(sc_uint<1>)tmp_data[11],
//					(sc_uint<1>)tmp_data[12],
//					(sc_uint<1>)tmp_data[13],
//					(sc_uint<1>)tmp_data[14],
//					(sc_uint<1>)tmp_data[15]);
//	txmem[2] = (	(sc_uint<1>)tmp_data[0],
//					(sc_uint<1>)tmp_data[1],
//					(sc_uint<1>)tmp_data[2],
//					(sc_uint<1>)tmp_data[3],
//					(sc_uint<1>)tmp_data[4],
//					(sc_uint<1>)tmp_data[5],
//					(sc_uint<1>)tmp_data[6],
//					(sc_uint<1>)tmp_data[7]);
	txmem[1] = (sc_uint<8>)frmn.range(7, 0);
	txmem[2] = (	(sc_uint<1>)tmp_data[0],
					(sc_uint<1>)tmp_data[1],
					(sc_uint<1>)tmp_data[2],
					(sc_uint<1>)tmp_data[3],
					(sc_uint<1>)tmp_data[4],
					(sc_uint<3>)frmn.range(10, 8));

	utmi_send_pack(3);
}

void send_data(sc_uint<4> pid, int len, int mode) {
	int n;
	sc_uint<16> crc16r;
	sc_uint<8> x, y;

	txmem[0] = ((sc_uint<4>)~pid, (sc_uint<4>)pid);
	crc16r = 0xffff;
	for (n = 0; n < len; n++) {
		if (mode == 1)
			y = host_buffer[host_buffer_last + n];
		else
			y = n;
		x = (	(sc_uint<1>)y[0],
				(sc_uint<1>)y[1],
				(sc_uint<1>)y[2],
				(sc_uint<1>)y[3],
				(sc_uint<1>)y[4],
				(sc_uint<1>)y[5],
				(sc_uint<1>)y[6],
				(sc_uint<1>)y[7]);
		txmem[n + 1] = y;
		crc16r = crc16(crc16r, x);
	}

	host_buffer_last = host_buffer_last + n;
	y = (sc_uint<8>)crc16r.range(15, 8);
	txmem[n + 1] = (	(sc_uint<1>)!y[0],
						(sc_uint<1>)!y[1],
						(sc_uint<1>)!y[2],
						(sc_uint<1>)!y[3],
						(sc_uint<1>)!y[4],
						(sc_uint<1>)!y[5],
						(sc_uint<1>)!y[6],
						(sc_uint<1>)!y[7]);
	y = (sc_uint<8>)crc16r.range(7, 0);
	txmem[n + 2] = (	(sc_uint<1>)!y[0],
						(sc_uint<1>)!y[1],
						(sc_uint<1>)!y[2],
						(sc_uint<1>)!y[3],
						(sc_uint<1>)!y[4],
						(sc_uint<1>)!y[5],
						(sc_uint<1>)!y[6],
						(sc_uint<1>)!y[7]);

	utmi_send_pack(len + 3);
}



/////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////
////                                                             ////
////              Test Case Collection                           ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

void send_setup(	sc_uint<7> fa,
					sc_uint<8> req_type,
					sc_uint<8> request,
					sc_uint<16> wValue,
					sc_uint<16> wIndex,
					sc_uint<16> wLength) {
	int len;

	host_buffer[0] = req_type;
	host_buffer[1] = request;
	host_buffer[3] = (sc_uint<8>)wValue.range(15, 8);
	host_buffer[2] = (sc_uint<8>)wValue.range(7, 0);
	host_buffer[5] = (sc_uint<8>)wIndex.range(15, 8);
	host_buffer[4] = (sc_uint<8>)wIndex.range(7, 0);
	host_buffer[7] = (sc_uint<8>)wLength.range(15, 8);
	host_buffer[6] = (sc_uint<8>)wLength.range(7, 0);

	host_buffer_last = 0;

	send_token(fa, 0, USBF_T_PID_SETUP);

	wait(clk.posedge_event());

	send_data(USBF_T_PID_DATA0, 8, 1);

	utmi_recv_pack(&len);

	if (txmem[0] != 0xd2) {
		cout << "ERROR: SETUP: ACK mismatch. Expected: 0xD2, Got: " << txmem[0] <<
				" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	if (len != 1) {
		cout << "ERROR: SETUP: ACK mismatch. Expected: 1, Got: " << len <<
				" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	wait(clk.posedge_event());
	setup_pid.write(true);
	wait(clk.posedge_event());
}

void data_in(sc_uint<7> fa, int pl_size) {
	int rlen;
	sc_uint<4> pid, expect_pid;

	host_buffer_last = 0;
	for (int i = 0; i < 5; i++) wait(clk.posedge_event());
	send_token(fa, 0, USBF_T_PID_IN);

	recv_packet(&pid, &rlen);
	if (setup_pid.read())
		expect_pid = 0xb;	// DATA 1
	else
		expect_pid = 0x3;	// DATA 0

	if (pid != expect_pid) {
		cout << "ERROR: Data IN PID mismatch. Expected: " << expect_pid << ", Got: " << pid <<
				" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	setup_pid.write(!setup_pid.read());
	if (rlen != pl_size) {
		cout << "ERROR: Data IN Size mismatch. Expected: " << pl_size << ", Got: " << rlen <<
				" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	for (int i = 0; i < rlen; i++) {
		cout << "RCV Data[" << i << "]: 0x";
		printf("%02x", (unsigned int)host_buffer[i]);
		cout << endl;
	}
	cout << endl;

	for (int i = 0; i < 5; i++) wait(clk.posedge_event());
	send_token(fa, 0, USBF_T_PID_ACK);

	for (int i = 0; i < 5; i++)	wait(clk.posedge_event());
}

void data_out(sc_uint<7> fa, int pl_size) {
	int len;

	send_token(fa, 0, USBF_T_PID_OUT);

	wait(clk.posedge_event());

	if (!setup_pid.read())
		send_data(USBF_T_PID_DATA0, pl_size, 1);
	else
		send_data(USBF_T_PID_DATA1, pl_size, 1);

	setup_pid.write(!setup_pid.read());

	utmi_recv_pack(&len);

	if (txmem[0] != 0xd2) {
		cout << "ERROR: Ack mismatch. Expected: 0xd2, Got: " << txmem[0] <<
				" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	if (len != 1) {
		cout << "ERROR: SETUP: Length mismatch. Expected: 1, Got: " << len <<
				" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	for (int i = 0; i < 5; i++)	wait(clk.posedge_event());
}

// Enumeration Test -> Endpoint 0
void setup0(void) {
	// TODO: print decoded descriptors
	cout << endl;

	cout << "The Default Time Unit is: " << sc_get_default_time_unit().to_string() << endl << endl;

	cout << "**************************************************" << endl;
	cout << "*** CONTROL EP TEST 0                          ***" << endl;
	cout << "**************************************************" << endl << endl;

	cout << "Setting Address ..." << endl << endl;

	send_setup(0x00, 0x00, SET_ADDRESS, 0x0012, 0x0000, 0);
	data_in(0x00,0);

	cout << "Getting Device Descriptor ..." << endl << endl;

	send_setup(0x12, 0x80, GET_DESCRIPTOR, 0x0100, 0x0000, 18);
	data_in(0x12, 18);
	data_out(0x12, 0);

	cout << "Getting Configuration Descriptor (First Phase) ..." << endl << endl;

	send_setup(0x12, 0x80, GET_DESCRIPTOR, 0x0200, 0x0000, 9);
	data_in(0x12, 9);
	data_out(0x12, 0);

	unsigned short conf_length = host_buffer[2] + (host_buffer[3] << 8);
	cout << "Getting Configuration Descriptor (Second Phase) len=" << conf_length << " ..." << endl << endl;

	send_setup(0x12, 0x80, GET_DESCRIPTOR, 0x0200, 0x0000, conf_length);
	data_in(0x12, conf_length);
	data_out(0x12, 0);

	cout << "Getting LANG IDs Descriptor ..." << endl << endl;

	send_setup(0x12, 0x80, GET_DESCRIPTOR, 0x0300, 0x0000, 4);
	data_in(0x12, 4);
	data_out(0x12, 0);

	cout << "Getting String Descriptor 1 ..." << endl << endl;

	send_setup(0x12, 0x80, GET_DESCRIPTOR, 0x0301, 0x0409, 10);
	data_in(0x12, 10);
	data_out(0x12, 0);

	cout << "Getting String Descriptor 2 ..." << endl << endl;

	send_setup(0x12, 0x80, GET_DESCRIPTOR, 0x0302, 0x0409, 32); 
	data_in(0x12, 32);
	data_out(0x12, 0);

	cout << "Getting String Descriptor 3 ..." << endl << endl;

	send_setup(0x12, 0x80, GET_DESCRIPTOR, 0x0303, 0x0409, 20);
	data_in(0x12, 20);
	data_out(0x12, 0);

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}


void receive_ir_result(void) {
	const sc_uint<7> my_fa = 0x12;
	const sc_uint<4> PID0 = 0x3, PID1 = 0xb;
	sc_uint<4> pid, expect_pid = PID0;

	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** RECEIVE PULSEOXY RESULT                    ***" << endl;
	cout << "**************************************************" << endl << endl;


	// send_sof(0x000); ???

	host_buffer_last = 0;
	int rx_len;

	// Send Data
	wait(clk.posedge_event());
	send_sof(0x000);
	wait(clk.posedge_event());
	send_token(my_fa, 1, USBF_T_PID_IN);
	wait(clk.posedge_event());

	recv_packet(&pid, &rx_len);

	if (pid != expect_pid) {
		cout << "ERROR: PID mismatch. Expected: " << expect_pid << ", Got: " << pid <<
				" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}
	expect_pid = (expect_pid == PID0) ? PID1 : PID0;

	for (int i = 0; i < 4; i++) wait(clk.posedge_event());

	send_token(my_fa, 3, USBF_T_PID_ACK);

	for (int i = 0; i < 60; i++) wait(clk.posedge_event());
	
	if (rx_len > 0) {
		cout << "Result of IR measurement: " << host_buffer[0] << endl;
	}
	else {
		cout << "No response received for IR measurement request." << endl;
		error_cnt++;
	}

	cout << endl;

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

void send_ir_req(void) {
	const sc_uint<7> my_fa = 0x12;
	bool pid = false;

	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** SEND PULSEOXY REQ                          ***" << endl;
	cout << "**************************************************" << endl << endl;

	host_buffer[0] = 0x74;
	host_buffer_last = 0;


	wait(clk.posedge_event());
	send_sof(0x000);
	wait(clk.posedge_event());

	send_token(my_fa, 2, USBF_T_PID_OUT);
	wait(clk.posedge_event());

	if (!pid)
		send_data(USBF_T_PID_DATA0, 1, 1);
	else
		send_data(USBF_T_PID_DATA1, 1, 1);
	pid = !pid;

	int rx_len;
	utmi_recv_pack(&rx_len);

	if (txmem[0] != 0xd2) {
		cout << "ERROR: ACK mismatch. Expected: 0xd2, Got: " << txmem[0] <<
				" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	if (rx_len != 1) {
		cout << "ERROR: Size mismatch. Expected: 1, Got: " << rx_len <<
				" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	for (int i = 0; i < 60; i++) wait(clk.posedge_event());

	cout << endl;

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}


/////////////////////////////////////////////////////////////////////

	
	void init(void) {
		error_cnt = 0;
		phy_tx_valid.write(false);

		while (!rst.read()) wait(rst.value_changed_event());
		for (int i = 0; i < 10; i++) wait(clk.posedge_event());

		setup0();
		send_ir_req();
		wait(10, SC_MS);
		receive_ir_result();

		for (int i = 0; i < 500; i++) wait(clk.posedge_event());
		sc_stop();
	}

	SC_CTOR(test_PulseOxyApp_USB) : i_phy("USB_HOST_PHY"), i_por("POR"), clk("clk", 20.84, SC_NS) {
		i_por.rst(rst);

		vcc.write(true);
		
		i_phy.clk(clk);
		i_phy.rst(rst);
		i_phy.phy_tx_mode(vcc);
		i_phy.usb_rst(phy_usb_rst_nc);
		i_phy.txdp(tx_dp);
		i_phy.txdn(tx_dn);
		i_phy.txoe(phy_txoe_nc);
		i_phy.rxd(rx_dp);
		i_phy.rxdp(rx_dp);
		i_phy.rxdn(rx_dn);
		i_phy.DataOut_i(phy_tx_data);
		i_phy.TxValid_i(phy_tx_valid);
		i_phy.TxReady_o(phy_tx_ready);
		i_phy.DataIn_o(phy_rx_data);
		i_phy.RxValid_o(phy_rx_valid);
		i_phy.RxActive_o(phy_rx_active);
		i_phy.RxError_o(phy_rx_error);
		i_phy.LineState_o(phy_line_nc);

		SC_THREAD(init);
			sensitive << clk.posedge_event();
	}
};


#endif //TEST_PULSEOXYAPP_H