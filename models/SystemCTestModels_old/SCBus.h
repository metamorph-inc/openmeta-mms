/////////////////////////////////////////////////////////////////////
////                                                             ////
////  SCBus for communicating with QEMU/Android                  ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef SCBUS_H
#define SCBUS_H

#include <queue>
#include <systemc.h>

#define SCBUS_QUANTUM_US		(10e3)
#define SCBUS_PORT	7674
#define SCBUS_PACKET_MAX_DATA_LENGTH	512

#define MAGIC_A2SC "A2SC"
#define MAGIC_SC2A "SC2A"

#pragma pack(1)
typedef struct tagSCBusPacketHeader {
	char	magic[4];
	long long timestamp;
	int cid;
	int length;
} SCBusPacketHeader;


typedef struct tagSCBusPacket {
	SCBusPacketHeader header;
	char	data[SCBUS_PACKET_MAX_DATA_LENGTH];
} SCBusPacket;

typedef struct tagSCBusPacket SCBusPacket;

SC_MODULE(SCBus) {

	// ports
	sc_in<bool>   clk;
	sc_in<bool>   rst;

	sc_in<bool>   tx_data_wr;
	sc_in<sc_uint<8>> tx_data;
	sc_out<bool>  tx_empty;

	sc_in<bool>   rx_data_rd;
	sc_out<sc_uint<8>> rx_data;
	sc_out<bool>  rx_avail;

	SC_CTOR(SCBus);
	~SCBus();

//private:
	void transmit_data();
	void receive_data();
	void comm_thread();
	void batch_thread();

	void setup_socket();
    int setup_batch(const char* file_name);
	double get_wall_time_us();
	void set_wall_time_us(double t);

	std::queue<sc_uint<8>> tx_fifo;
	std::queue<sc_uint<8>> rx_fifo;

	WSADATA WsaDat;
	SOCKET udp_socket;
	double perfCountPerUS;

    std::queue<SCBusPacket> batch_fifo;

	double sc_time_us;
	double local2wall_adjust_us;
	bool   ext_synced;
};

#endif // SCBUS_H