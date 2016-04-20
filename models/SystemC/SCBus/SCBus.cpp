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
#include "SCBus.h"
#pragma comment(lib,"ws2_32.lib")

static void print_socket_error(const char* prefix) {
	LPTSTR  errMsg;
	FormatMessage( FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
			NULL,
			WSAGetLastError(),
			0,
			(LPTSTR)&errMsg,
			0,
			NULL);
	cerr << prefix << errMsg << endl;
	LocalFree(errMsg);
}

SCBus::SCBus(sc_module_name mname) :
	sc_module(mname),
		udp_socket(INVALID_SOCKET), local2wall_adjust_us(0), ext_synced(false)
{
	LARGE_INTEGER perfCountFreq;
	if (!QueryPerformanceFrequency(&perfCountFreq)) {
		cerr << "SCBus: This platform does not support high resolution timers" << endl;
		exit(GetLastError());
	}
	perfCountPerUS = double(perfCountFreq.QuadPart) / 1e6;
	cout << "SCBus: DBG Performance Counter per uS: " << perfCountPerUS << endl;

	SC_METHOD(transmit_data);
	dont_initialize();
	sensitive << clk.pos() << rst;

	SC_METHOD(receive_data);
	dont_initialize();
	sensitive << clk.pos() << rst;

	WIN32_FIND_DATA FindFileData;
	HANDLE hFind;

	hFind = FindFirstFile("*.sca", &FindFileData);
	if (hFind == INVALID_HANDLE_VALUE) {
		cout << "SCBus: No valid input batch file found (" << GetLastError() << ")" << endl;
		cout << "SCBus: Starting to listen on UDP" << endl;
		SC_THREAD(comm_thread);
	}
	else
	{
		cout << "SCBus: Using batch file '" << FindFileData.cFileName << "'" << endl;
        setup_batch(FindFileData.cFileName);
		FindClose(hFind);
		SC_THREAD(batch_thread);
	}
}

void SCBus::setup_socket()
{
	if(WSAStartup(MAKEWORD(2,2),&WsaDat)!=0) {
		print_socket_error("SCBus: WinSock Initialization failed: ");
		WSACleanup();
		exit(WSAGetLastError());
	}

	udp_socket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if(udp_socket == INVALID_SOCKET) {
		print_socket_error("SCBus: Server socket creation failed: ");
		WSACleanup();
		exit(WSAGetLastError());
	}

	u_long iMode = 1;
	if (ioctlsocket(udp_socket, FIONBIO, &iMode)) {
		print_socket_error("SCBus: Cannot create non-blocking socket: ");
		WSACleanup();
		exit(WSAGetLastError());
	}

	SOCKADDR_IN server_inf;
	memset((void *)&server_inf, 0, sizeof(server_inf));

	server_inf.sin_family = AF_INET;
	server_inf.sin_addr.s_addr = INADDR_ANY;
	server_inf.sin_port = htons(SCBUS_PORT);

	if(bind(udp_socket, (SOCKADDR*)(&server_inf), sizeof(server_inf)) == SOCKET_ERROR) {
		print_socket_error("SCBus: Unable to bind server socket: ");
		WSACleanup();
		exit(WSAGetLastError());
	}

	cout << "SCBus listening on port " << SCBUS_PORT << endl;
}

int SCBus::setup_batch(const char* file_name)
{
    cout << "SCBus: setup_batch() invoked" << endl;
    cout << "Parsing file '" << file_name << "'" << endl;

    std::string line; // FIXME: check why std:: is needed here
    ifstream infile(file_name);

    if (!infile.is_open()) {
        cout << "SCBus: Unable to open file '" << file_name << "'" << endl;
    }

    SCBusPacket packet;
    unsigned int dctr, data, lctr;
    long long ts, ts_last;
    ts_last = 0;
    lctr = 0;

    while (std::getline(infile, line)) {
        std::istringstream lss(line);
        std::string token;
        lctr++;

        getline(lss,token,' ');
        ts = std::stoll(token.c_str());
        if (ts_last >= ts && ts_last != 0) {
            cout << "WARNING: SCBus batch file time stamp is out of sequence in line "
                << lctr << ":" << endl;
            cout << "'" << line << "' (ignoring line)" << endl;
            continue;
        }

        memset(&packet, 0, sizeof(SCBusPacket));
        packet.header.timestamp = ts;

        dctr = 0;
        while (std::getline(lss,token,' ')) {
            if (dctr == SCBUS_PACKET_MAX_DATA_LENGTH-1) {
                cout << "WARNING: SCBus batch file data length is over the limit (" <<
                    (int)SCBUS_PACKET_MAX_DATA_LENGTH << ") in line " << lctr
                    << " (truncating line)" << endl;
                break;
            }
            data = std::stoul(token.c_str(),nullptr,16);
            if (data > 255) {
                cout << "WARNING: SCBus batch file data out of range in line "
                    << lctr << ":" << endl;
                cout << "'" << line << "' (truncating value)" << endl;
                data &= 0xFFul;
            }
            packet.data[dctr++] = (char)data;
        }

        if (dctr == 0) {
            cout << "WARNING: SCBus batch file with empty data field in line " << lctr << ":" << endl;
            cout << "'" << line << "' (ignoring line)" << endl;
            continue;
        }

        packet.header.length = dctr;
        batch_fifo.push(packet);
        ts_last = ts;
    }

    cout << "Parsing done" << endl;
    infile.close();
    return 0;
}

double SCBus::get_wall_time_us()
{
	LARGE_INTEGER  perfCounter;

	if (!QueryPerformanceCounter(&perfCounter)) {
		cerr << "SCBus: Unable to read performance counter" << endl;
		exit(GetLastError());
	}

	return local2wall_adjust_us +  (double(perfCounter.QuadPart) / perfCountPerUS);
}

void SCBus::set_wall_time_us(double t)
{
	LARGE_INTEGER  perfCounter;

	if (!QueryPerformanceCounter(&perfCounter)) {
		cerr << "SCBus: Unable to read performance counter" << endl;
		exit(GetLastError());
	}

	double local_time_us = double(perfCounter.QuadPart) / perfCountPerUS;
	local2wall_adjust_us = t - local_time_us;
}

// This is the main part, taking care of time synchronization
// between Android and SystemC
// Basic principles:
//	- Android will dictate the global/master time
//	- Messages from Android are sporadic: async data messages and
//    keepalive (~1s) messages if no data is to be sent
//	- the SystemC simulation is allowed to advance 1 quantum (currently, 1ms)
//	  past the last known sync point
//	- this advance happens in two phases: first the DE simulation is let to run,
//    then we are spinning for the physical time to catch up locally
//  - we won't check for incoming messages before the the quatum finishes.
//
// NOTE: This section is somewhat outdated, see the comments in the algorithm below
// We keep track of the system (wall) time in wall_time_us (virtual)
// We adjust (set) this value to the timestamp of the last incoming message
// or, if no incoming message, just increase it by the quantum value
// Before setting wall_time_us to its new value, we calculate the difference
// between the current and the new value.
// We use this difference for advancing the DE simulation. NOTE, we are still going to
// spin for a proper 1ms wall time at the end of the quantum (using the local wall clock
// - this is the only case where we use this clock)
// Special case: for the very first incoming message we will just set wall_time_us and will
// use a constant quantum step
// We receive and send data at the end of each quantum
//
// To rephrase: we always go ahead for a fixed quantum interval (1ms) in each round
// but might advance the DE simulator for shorter or longer logical time to align it
// with the system (wall) time

void SCBus::comm_thread()
{
	setup_socket();

	set_wall_time_us(0);	// Until we hear anything from Android
	sc_time_us = 0;
	double target_time_us = SCBUS_QUANTUM_US;	// How far we go in the next quantum

	struct sockaddr from;
	int fromlen;
	SCBusPacket	packet;

	while (true) {
		// Quantum step for SC
		double delta_sc_time_us = target_time_us - sc_time_us;
		if (delta_sc_time_us > 0) {
			// Let the simulator proceed for the next quantum
			wait(delta_sc_time_us, SC_US);
			sc_time_us = target_time_us;
		}

		// Catch-up with wall clock
		double slack = target_time_us - get_wall_time_us();

		//cout << "slack=" << slack << "us" << endl;
		if (slack < 0) {
			cerr << "SCBus: SystemC is too slow [slack=" << slack << "us]" << endl;
		}
		else {
			// TODO: we might want to move this after processing the packets (?)
			// Spinning to wait for wall-time to catch up
			while (get_wall_time_us() < target_time_us) {}
		}

		// Process incoming messages
		int res;
		double latest = -1;

		do {
			fromlen = sizeof(from);
			res = recvfrom(udp_socket, (char*)&(packet), sizeof(SCBusPacket), 0, &from, &fromlen);
			if (res > 0) {
				// Sanity check
				if (memcmp(MAGIC_A2SC, (const void*)&(packet.header.magic),
					sizeof(((SCBusPacketHeader *)0)->magic))) {
					cerr << "SCBus: invalid header, dropping packet" << endl;
				}
				else {
					// search for max timestamp
					latest = max(double(packet.header.timestamp), latest);
					// get data
					for (int i = 0; i < packet.header.length; i++) {
						rx_fifo.push(packet.data[i]);
					}
					// update rx_avail will be taken care of during the next clock cycle (by receive_data())
				}
			}
			else if (WSAGetLastError() != WSAEWOULDBLOCK) {
				print_socket_error("SCBus: packet receive error: ");
			}
		} while (res > 0);

		// time adjustments
		// if we received any packet
		//	if this is the first ever, set wall_time_us and sc_time_us to this timestamp
		//  otherwise, set wall_time_us only to the new timestamp (if diff is beyond the sync resolution)
		// else (no packets received): do not touch xxx_time_us
		// always set the new target to the new wall_time_us+SCBUS_QUANTUM_US
		// TODO: we might change the last step to adapt better
		//   (e.g. use the min(wall_time_us, sc_time_us) + SCBUS_QUANTUM_US
		if (latest > 0) {
			if (!ext_synced) {
				cout << "SCBus: Received external timesync, set local time to: " << latest << " us" << endl;
				set_wall_time_us(latest);
				sc_time_us = latest;
				ext_synced = true;
			}
			else {
				double local = get_wall_time_us();
				if (fabs(local-latest) > SCBUS_QUANTUM_US) {
					cout << "SCBus: Adjusting timesync to: " << latest <<
						" us (dT=" << (local-latest) << " us)" << endl;
					set_wall_time_us(latest);
				}
			}
		}
		target_time_us = get_wall_time_us() + SCBUS_QUANTUM_US;


		// Send outgoing messages
		if (!tx_fifo.empty()) {
			int i = 0;
			while (!tx_fifo.empty()) {
				packet.data[i++] = tx_fifo.front();
				tx_fifo.pop();
				if (i > SCBUS_PACKET_MAX_DATA_LENGTH) {
					// We will continue from here next time
					break;
				}
			}
			cout << endl;
			packet.header.length = i;
			packet.header.cid = 0;
			packet.header.timestamp = get_wall_time_us();
			memcpy(&(packet.header.magic), MAGIC_SC2A, sizeof(((SCBusPacketHeader *)0)->magic));

			if (sendto(udp_socket, (char*)&packet,
				sizeof(SCBusPacketHeader)+packet.header.length, 0, &from, fromlen) == SOCKET_ERROR) {
				print_socket_error("SCBus: packet send error: ");
			}
		}
	}
}

void SCBus::batch_thread()
{
	cout << "SCBus: batch_thread() started @ " << sc_time_stamp() << endl;

    SCBusPacket packet;
    set_wall_time_us(0);
    double target_time_us = 0;
    double last_time_us = 0;

    while (true)
    {
        //cout << "SCBus: batch_thread() loop executed @ " << sc_time_stamp() << endl;

        if (batch_fifo.empty()) {
            cout << "SCBus: batch_thread() input FIFO empty @ " << sc_time_stamp() << endl;
            wait(1, SC_MS);
            sc_stop();
            return;
        }

        packet = batch_fifo.front();
        batch_fifo.pop();

        target_time_us = packet.header.timestamp;

        // SystemC timing
        wait(sc_time(target_time_us, SC_US) - sc_time_stamp());
        cout << "SCBus: batch_thread() SystemC wait ended @ " << sc_time_stamp() << " ["
            << get_wall_time_us()/1e6 << " s]" << endl;

        // Wall clock timing
        while (get_wall_time_us() < target_time_us) {}
        last_time_us = get_wall_time_us();
        cout << "SCBus: batch_thread() wall clock wait ended @ " << sc_time_stamp() << " ["
            << get_wall_time_us()/1e6 << " s]" << endl;

        for (int i = 0; i < packet.header.length; i++) {
            rx_fifo.push(packet.data[i]);
        }
    }
}

SCBus::~SCBus()
{
	closesocket(udp_socket);
	WSACleanup();
	cout << "SCBus server socket closed" << endl;
}

// SystemC side incoming (TX)
void SCBus::transmit_data()
{
	if (!rst.read()) {
		while (!tx_fifo.empty()) {
			tx_fifo.pop();
		}
	}
	else {
		if (tx_data_wr.read()) {
			tx_fifo.push(tx_data.read());
		}
	}

	tx_empty.write(tx_fifo.empty());
}

// SystemC side outgoing (RX)
void SCBus::receive_data()
{
	if (!rst.read()) {
	}
	else {
		if (rx_data_rd.read()) {
			if (!rx_fifo.empty()) {
				rx_fifo.pop();
			}
		}
	}

	if (!rx_fifo.empty()) {
		rx_data.write(rx_fifo.front());
		rx_avail.write(true);
	}
	else {
		rx_avail.write(false);
	}
}

