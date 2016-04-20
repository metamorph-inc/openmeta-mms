/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Data Player for SPICE-Derived Files          			     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Ted Bapty, MetaMorph, Inc.				         ////
////          ted.bapty@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////

#include "analogSrc.h"
#include <malloc.h>
#define ANALOG_CONV_SCALE 512

int tempCtr = 0;
void analogSrc::analog_update()
{
	int current_time_index = 0;
	int max_time_index;
	int ret;
	float sc_currentTime;
	sc_out<analog_type>	*analogs[8];
	analogs[0] = &analog0;
	analogs[1] = &analog1;
	analogs[2] = &analog2;
	analogs[3] = &analog3;
	analogs[4] = &analog4;
	analogs[5] = &analog5;
	analogs[6] = &analog6;
	analogs[7] = &analog7;
	
	float periodic_offset;
	float minTime = 9999990.0;
	float maxTime = -1.0;
	float currTime;
	FILE *fd=0;
	sc_time now = sc_time_stamp();
	double period = 0, duty = 0;
	time_scale = (float *)malloc(MAX_SIG_LEN*sizeof(float));
	for(int i = 0; i < MAX_SIG_NUM; i++)
	{
		data_trace[i] = (float *)malloc(MAX_SIG_LEN*sizeof(float));
	}
	fd = fopen("systemCTable.csv","r");
	while(!feof(fd) && (current_time_index < MAX_SIG_LEN))
	{
		float val;
	   ret = fscanf(fd,"%f,",&val);
	   if(minTime > val)
		   minTime = val;
	   if(maxTime < val)
		   maxTime = val;
	   if(current_time_index < MAX_SIG_LEN)
			time_scale[current_time_index] = val;
	   for(int i = 0; i < MAX_SIG_NUM-1; i++)
	   {
		   ret = fscanf(fd,"%f,",&val);
		   if(current_time_index < MAX_SIG_LEN)
			   data_trace[i][current_time_index] = val;
	   }
	   fscanf(fd,"%f\n",&val);
	   if(current_time_index < MAX_SIG_LEN)
  			data_trace[MAX_SIG_NUM-1][current_time_index] = val;
	   current_time_index++;
	}
	max_time_index = current_time_index;
	current_time_index = 0;
    while(true)
	{
	  //analog.write(tempCtr);
	  tempCtr++;
	  currTime = (float)sc_time_stamp().to_seconds();
	  while((current_time_index < max_time_index) && (time_scale[current_time_index] < currTime))
	  {
		  current_time_index++;
	  }
	  for(int ii = 0; ii < 8; ii++)
	  {
		  analogs[ii]->write(data_trace[ii][current_time_index]*ANALOG_CONV_SCALE);
	  }
	  //analog.write(data_trace[0][current_time_index]*ANALOG_CONV_SCALE);
	  cout << "Send " ;
	  for(int ii = 0; ii < 8; ii++)
	  {
		  cout << data_trace[ii][current_time_index]*ANALOG_CONV_SCALE << ", ";
	  }
      cout << " at: " << sc_time_stamp() << endl;
	  wait(10, SC_US);
	}

		//duty = neg_time.to_double() - pos_time.to_double();
/*
	switch (pwm.read().value())
    {
    case sc_dt::Log_1:
		period = now.to_double() - pos_time.to_double();
		duty = neg_time.to_double() - pos_time.to_double();
		pos_time = now;
        break;
    case sc_dt::Log_0:
		period = now.to_double() - neg_time.to_double();
		duty = now.to_double() - pos_time.to_double();
		neg_time = now;
        break;
    default:
        cout << "WARNING: PWMDAC analog_update() invalid input value" << endl;
    }
	if (duty > 0) {
		analog.write(duty / period * full_scale);
	}
*/
}


analogSrc::analogSrc(sc_module_name mname) : sc_module(mname),
	pos_time(), neg_time() 
{
	analog_type	tmp;
	sc_clock clk1("clk1", 100,SC_US);

	SC_THREAD(analog_update);
	    sensitive << clk1.posedge_event();
}