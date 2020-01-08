#define DEBUG

#include "secrets.h"
#include <TheThingsNetwork.h>
#include <AltSoftSerial.h>
#include <LowPower.h>
#include <I2CSoilMoistureSensor.h>
#include <Wire.h>

AltSoftSerial loraSerial;
I2CSoilMoistureSensor sensor1(0x20);
I2CSoilMoistureSensor sensor2(0x21);
I2CSoilMoistureSensor sensor3(0x22);

// Set your AppEUI and AppKey in secrets.h, found on TTN console device details
const char *appEui = SECRET_APPEUI;
const char *appKey = SECRET_APPKEY;
#define SLEEP_PERIOD 10000
#define BAUD_RATE_LORA 19200
#define BAUD_RATE_DEBUG 115200
#define RN2483_RESET_PIN 4
#define debugSerial Serial

// Replace REPLACE_ME with TTN_FP_EU868 or TTN_FP_US915
#define freqPlan TTN_FP_EU868

TheThingsNetwork ttn(loraSerial, debugSerial, freqPlan);

void setup()
{
  #ifdef DEBUG
  debugSerial.println(F("Setup"));
  #endif

  unsigned char pinNumber;
  for (pinNumber = 0; pinNumber < 19; pinNumber++)
  {
    pinMode(pinNumber, INPUT_PULLUP);
  }

  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);
  pinMode(RN2483_RESET_PIN, OUTPUT);
  
  // Reset RN2483/RN2903 for a clean power up
  digitalWrite(RN2483_RESET_PIN, LOW);
  delay(500);
  digitalWrite(RN2483_RESET_PIN, HIGH);
  
  loraSerial.begin(BAUD_RATE_LORA);
  debugSerial.begin(BAUD_RATE_DEBUG);
  
  Wire.begin();
  sensor1.sleep();
  sensor2.sleep();
  sensor3.sleep();

  // Reset is required to autobaud RN2483 into 19200 bps from the
  // default 57600 bps (autobaud process is called within reset())
  ttn.reset();
  ttn.showStatus();
  #ifdef DEBUG
  debugSerial.println("-- JOIN");
  #endif
  ttn.join(appEui, appKey);
}

void loop()
{
  unsigned char payload[10];
  float batteryVoltage;
  int adcReading;
  int voltage;

  debugSerial.println(F("-- LOOP"));
  
  // Discard first inaccurate reading
  adcReading = analogRead(A6);
  adcReading = 0;
  // Perform averaging
  for (char batCounter = 10; batCounter > 0; batCounter--)
  {
    adcReading += analogRead(A6);
  }
  adcReading = adcReading/10;
  // Convert to volts
  batteryVoltage = adcReading * (3.3 / 1024.0);
  
  #ifdef DEBUG
  debugSerial.println(F("Sensor1 begin"));
  #endif
  sensor1.begin(true);
  uint16_t capacitance1 = sensor1.getCapacitance();
  int16_t tempc1 = sensor1.getTemperature();
  sensor1.sleep();
  #ifdef DEBUG
  debugSerial.println(F("Sensor1 sleep"));
  #endif

  #ifdef DEBUG
  debugSerial.println(F("Sensor2 begin"));
  #endif
  sensor2.begin(true);
  uint16_t capacitance2 = sensor2.getCapacitance();
  sensor2.sleep();
  #ifdef DEBUG
  debugSerial.println(F("Sensor2 sleep"));
  #endif

  #ifdef DEBUG
  debugSerial.println(F("Sensor3 begin"));
  #endif
  sensor3.begin(true);
  uint16_t capacitance3 = sensor3.getCapacitance();
  sensor3.sleep();
  #ifdef DEBUG
  debugSerial.println(F("Sensor3 sleep"));
  #endif

  //uint16_t voltage = GetVoltage();
  // Pack float into int with 2 decimal point resolution
  voltage = batteryVoltage * 1000;
  payload[0] = voltage >> 8;
  payload[1] = voltage;
  
  payload[2] = capacitance1 >> 8;
  payload[3] = capacitance1;
  payload[4] = tempc1 >> 8;
  payload[5] = tempc1;
  
  payload[6] = capacitance2 >> 8;
  payload[7] = capacitance2;
  
  payload[8] = capacitance3 >> 8;
  payload[9] = capacitance3;
  
  #ifdef DEBUG
  debugSerial.print(F("Capacitance1: "));
  debugSerial.println(capacitance1);
  debugSerial.print(F("Temperature1: "));
  debugSerial.println(tempc1);
  debugSerial.print(F("Capacitance2: "));
  debugSerial.println(capacitance2);
  debugSerial.print(F("Capacitance3: "));
  debugSerial.println(capacitance3);
  debugSerial.print(F("Voltage: "));
  debugSerial.println((int)voltage);
  #endif
  
  // Send & sleep
  ttn.sendBytes(payload, sizeof(payload));
  ttn.sleep(SLEEP_PERIOD);
  
  // Ensure all debugging message are sent before sleep
  debugSerial.flush();
  // Put IO pins (D8 & D9) used for software serial into low power 
  loraSerial.end();
  // Use RN2483/RN2903 as MCU wake-up source after RN2483/RN2903 sleep period 
  // expires 
  attachInterrupt(digitalPinToInterrupt(2), awake, LOW);
  // Put MCU into sleep mode and to be woken up by RN2483/RN2903
  LowPower.powerDown(SLEEP_FOREVER, ADC_OFF, BOD_OFF);
  loraSerial.begin(BAUD_RATE_LORA);
}

void awake()
{
  detachInterrupt(digitalPinToInterrupt(2));
}