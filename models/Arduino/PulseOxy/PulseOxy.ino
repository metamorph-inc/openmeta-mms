/*
  Pulse Oxymeter firmware example
  
  Copyright (c) 2013 MetaMorph, Inc.
  All Rights Reserved.
  
  Author: Peter Volgyesi <pvolgyesi@metamorphsoftware.com>
 
 */
 
const int rawAnalogPin = 0;
const int filtAnalogPin = 1;
const int IRLedPWMPin = 9;
const int redLedPWMPin = 10;
const int DCPWMPin = 11;

const int REQ_IR = 0x74;
const int REQ_RED = 0x76;

const int FS = 10;  // sampling freq in Hz
const int T_MEASURE = 2;  // measurement period in seconds

// derived values, do not edit
const int TS = (1000 / FS);
const int N_MEASURE = (T_MEASURE * FS);
const int ANALOG_MAX = 1023;
const int ANALOG_MIN = 0;
const int PWM_MAX = 255;


int doMeasurement(int ledPWMPin) {
  int aMin, aMax;

  // Turn on selected LED (only)
  analogWrite(IRLedPWMPin, 0);
  analogWrite(redLedPWMPin, 0);
  analogWrite(ledPWMPin, PWM_MAX);
    
  // Turn off DC bias and measure offset
  analogWrite(DCPWMPin, 0);
  delayMicroseconds(TS);	// should be PWM settling time

  aMin = ANALOG_MAX; aMax = ANALOG_MIN;
  for(int i = 0; i < N_MEASURE; i++) {
    int a = analogRead(rawAnalogPin);
    aMin = min(a, aMin);
    aMax = max(a, aMax);
    delayMicroseconds(TS);
  }
  // Calibrate and set DC bias
  int dcOffset = (aMax + aMin - ANALOG_MAX) / 2;

  if (dcOffset > 0) {
    analogWrite(DCPWMPin, (PWM_MAX * dcOffset / ANALOG_MAX));
  }
  
  // Measure amplitude
  aMin = ANALOG_MAX; aMax = ANALOG_MIN;
  for(int i = 0; i < N_MEASURE; i++) {
    int a = analogRead(filtAnalogPin);
    aMin = min(a, aMin);
    aMax = max(a, aMax);
    delayMicroseconds(TS);
  }
  
  // Turn off LEDs and bias compensation
  analogWrite(IRLedPWMPin, 0);
  analogWrite(redLedPWMPin, 0);
  analogWrite(DCPWMPin, 0);
  
  return (aMax - aMin);
}

void setup() {
  Serial.begin(9600);
}

void loop() {
  if (Serial.available() > 0) {
    int req = Serial.read();
    int result;
    
    switch(req) {
      case REQ_IR:
        result = doMeasurement(IRLedPWMPin);
        break;
      case REQ_RED:
        result = doMeasurement(redLedPWMPin);
        break;
      default:
        result = -1; 
    }
    
    Serial.write(result);
  }
}
