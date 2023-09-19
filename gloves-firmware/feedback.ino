#include "ESP32Servo.h"

Servo pinkyServo;
Servo ringServo;
Servo middleServo;
Servo indexServo;
Servo thumbServo;

void setupServos()
{
    pinkyServo.attach(PIN_PINKY_SERVO);
    ringServo.attach(PIN_RING_SERVO);
    middleServo.attach(PIN_MIDDLE_SERVO);
    indexServo.attach(PIN_INDEX_SERVO);
    thumbServo.attach(PIN_THUMB_SERVO);
}

void setupThermo()
{
    pinMode(PIN_THERMO_IN1, OUTPUT);
    pinMode(PIN_THERMO_IN2, OUTPUT);
    
    ledcSetup(THERMO_PWM_CHANNEL, 1000, 8);
    ledcAttachPin(PIN_THERMO_ENA, THERMO_PWM_CHANNEL);
}

void setupBuzzers()
{
    pinMode(PIN_PINKY_BUZZER, OUTPUT);
    pinMode(PIN_RING_BUZZER, OUTPUT);
    pinMode(PIN_MIDDLE_BUZZER, OUTPUT);
    pinMode(PIN_INDEX_BUZZER, OUTPUT);
    pinMode(PIN_THUMB_BUZZER, OUTPUT);
}

void writeServos(int* parsedServo)
{
    float scaledLimits[5];
    mapServoLimits(parsedServo, scaledLimits);
    indexServo.write(scaledLimits[0]);
    middleServo.write(scaledLimits[1]);
    pinkyServo.write(scaledLimits[2]);
    ringServo.write(scaledLimits[3]);
    thumbServo.write(scaledLimits[4]);
}

void writeThermo(int* parsedThermo)
{
    int thermoValues[1];
    mapThermoValue(parsedThermo, thermoValues);

    if(thermoValues[0] == 0) //turn off
    {
        digitalWrite(PIN_THERMO_IN1, LOW);
        digitalWrite(PIN_THERMO_IN2, LOW); 
    }
    else if(thermoValues[0] < 0) //start cooling
    {
        digitalWrite(PIN_THERMO_IN1, HIGH);
        digitalWrite(PIN_THERMO_IN2, LOW);
        ledcWrite(THERMO_PWM_CHANNEL, abs(thermoValues[0])); 
    }
    else if(thermoValues[0] > 0) //start heating
    {
        digitalWrite(PIN_THERMO_IN1, LOW);
        digitalWrite(PIN_THERMO_IN2, HIGH);
        ledcWrite(THERMO_PWM_CHANNEL, thermoValues[0]);
    }
}

void writeBuzzers(bool* parsedBuzzer)
{
    digitalWrite(PIN_INDEX_BUZZER, parsedBuzzer[0]);
    digitalWrite(PIN_MIDDLE_BUZZER, parsedBuzzer[1]);
    digitalWrite(PIN_PINKY_BUZZER, parsedBuzzer[2]);
    digitalWrite(PIN_RING_BUZZER, parsedBuzzer[3]);
    digitalWrite(PIN_THUMB_BUZZER, parsedBuzzer[4]);
}


