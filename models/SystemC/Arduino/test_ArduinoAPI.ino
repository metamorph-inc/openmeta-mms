/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple Arduino Test Bench                                  ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Sandor Szilvsai, MetaMorph, Inc.                   ////
////          sszilvasi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

static void setup()
{
    cout << "setup() invoked" << endl;
    return;
}

static void loop()
{
    unsigned long t1, t2, te;

	//cout << "loop() invoked" << endl;
	wait(10, SC_US);

    if (test_en)
    {
        switch (test_id)
        {
        case 0:
            cout << "loop(): Starting test case #" << test_id << endl;
            pinMode(0, OUTPUT);
            digitalWrite(0, HIGH);
            break;
        case 1:
            cout << "loop(): Starting test case #" << test_id << endl;
            digitalWrite(0, LOW);
            break;
        case 2:
            cout << "loop(): Starting test case #" << test_id << endl;
            pinMode(3, OUTPUT);
            digitalWrite(3, HIGH);
            break;
        case 3:
            cout << "loop(): Starting test case #" << test_id << endl;
            digitalWrite(3, LOW);
            break;
        case 4:
            cout << "loop(): Starting test case #" << test_id << endl;
            // Release pin 1
            pinMode(0, OUTPUT);
            pinMode(1, INPUT);
            break;
        case 5:
            cout << "loop(): Starting test case #" << test_id << endl;
            // Loop back logical value ot pin 0
            digitalWrite(0, digitalRead(1));
            break;

        // analogRead()
        case 10:
            cout << "loop(): Starting test case #" << test_id << endl;
            for (int i = 0; i < 6; i++)
            {
                pinMode(i, OUTPUT);
                digitalWrite(i, analogRead(i) == analog_magic_v[i] ?  HIGH : LOW);
            }
            break;

        // analogWrite()
        case 20:
            cout << "loop(): Starting test case #" << test_id << endl;
            analogWrite(9,64);
            break;

        // delay()
        case 50:
            cout << "loop(): Starting test case #" << test_id << endl;
            pinMode(0, OUTPUT);
            digitalWrite(0, LOW);
            digitalWrite(0, HIGH);
            delay(1); // ms
            digitalWrite(0, LOW);
            break;

        // delayMicroseconds()
        case 51:
            cout << "loop(): Starting test case #" << test_id << endl;
            pinMode(0, OUTPUT);
            digitalWrite(0, LOW);
            digitalWrite(0, HIGH);
            delayMicroseconds(50); // us
            digitalWrite(0, LOW);
            break;

        // millis()
        case 52:
            cout << "loop(): Starting test case #" << test_id << endl;
            te = 3;
            pinMode(0, OUTPUT);
            pinMode(1, OUTPUT);
            digitalWrite(0, LOW);
            digitalWrite(1, LOW);

            t1 = millis();
            delay(te);
            t2 = millis();

            digitalWrite(1, (t2 - t1 == te) ? HIGH : LOW);
            digitalWrite(0, HIGH);
            break;

        // micros()
        case 53:
            cout << "loop(): Starting test case #" << test_id << endl;
            te = 37;
            pinMode(0, OUTPUT);
            pinMode(1, OUTPUT);
            digitalWrite(0, LOW);
            digitalWrite(1, LOW);

            t1 = micros();
            delayMicroseconds(te);
            t2 = micros();

            digitalWrite(1, (t2 - t1 == te) ? HIGH : LOW);
            digitalWrite(0, HIGH);
            break;

		case 60:
			cout << "loop(): Starting test case #" << test_id << endl;
			randomSeed( 1 );
#define MAX_BUCKETS (10)
#define MAX_PRN (1000)
			int bucketsArray[ MAX_BUCKETS ];
			for( int i = 0; i < MAX_BUCKETS; i++ )
			{
				bucketsArray[ i ] = 0;
			}
			for( int i = 0; i < MAX_PRN; i++ )
			{
				long prn = random( 0, MAX_BUCKETS );
				if( (prn < 0) || (prn >= MAX_BUCKETS) )
				{
					cout << "WARNING! prn = " << prn << " is out of bounds in test case " << test_id << endl;
				}
				else
				{
					bucketsArray[ prn ] += 1;
				}
			}
			for( int i = 0; i < MAX_BUCKETS; i++ )
			{
				cout << "     bucketsArray[ " << i << " ] = " << bucketsArray[ i ] << endl;
			}

			break;

        default:
            cout << "INVALID TEST CASE ID" << endl;
        }
        test_en = 0;
    }
	return;
}