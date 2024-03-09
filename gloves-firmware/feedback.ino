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

void setupThermal()
{
    pinMode(PIN_THERMAL_IN1, OUTPUT);
    pinMode(PIN_THERMAL_IN2, OUTPUT);
    
    ledcSetup(THERMAL_PWM_CHANNEL, 1000, 8);
    ledcAttachPin(PIN_THERMAL_ENA, THERMAL_PWM_CHANNEL);
}

void setupHaptics()
{
    pinMode(PIN_PINKY_HAPTIC, OUTPUT);
    pinMode(PIN_RING_HAPTIC, OUTPUT);
    pinMode(PIN_MIDDLE_HAPTIC, OUTPUT);
    pinMode(PIN_INDEX_HAPTIC, OUTPUT);
    pinMode(PIN_THUMB_HAPTIC, OUTPUT);
}

void writeServos(int* parsedServo)
{
    float scaledLimits[5];
    mapServoLimits(parsedServo, scaledLimits);
    
    if(parsedServo[THUMB_IND] >= 0) thumbServo.write(scaledLimits[THUMB_IND]);
    if(parsedServo[INDEX_IND] >= 0) indexServo.write(scaledLimits[INDEX_IND]);
    if(parsedServo[MIDDLE_IND] >= 0) middleServo.write(scaledLimits[MIDDLE_IND]);
    if(parsedServo[RING_IND] >= 0) ringServo.write(scaledLimits[RING_IND]);
    if(parsedServo[PINKY_IND] >= 0) pinkyServo.write(scaledLimits[PINKY_IND]);
}

void writeThermal(int* parsedThermal)
{
    int thermalValues[1];
    mapThermalValue(parsedThermal, thermalValues);

    if(thermalValues[0] == 0) //turn off
    {
        digitalWrite(PIN_THERMAL_IN1, LOW);
        digitalWrite(PIN_THERMAL_IN2, LOW); 
    }
    else if(thermalValues[0] < 0) //start cooling
    {
        digitalWrite(PIN_THERMAL_IN1, HIGH);
        digitalWrite(PIN_THERMAL_IN2, LOW);
        ledcWrite(THERMAL_PWM_CHANNEL, abs(thermalValues[0])); 
    }
    else if(thermalValues[0] > 0) //start heating
    {
        digitalWrite(PIN_THERMAL_IN1, LOW);
        digitalWrite(PIN_THERMAL_IN2, HIGH);
        ledcWrite(THERMAL_PWM_CHANNEL, thermalValues[0]);
    }
}

void writeHaptics(int* parsedHaptic)
{
    unsigned long currentTime = millis();
    if(parsedHaptic[THUMB_IND] >= 0)
    {   
        hapticTime[THUMB_IND] = parsedHaptic[THUMB_IND] + currentTime;
        digitalWrite(PIN_THUMB_HAPTIC, HIGH);
    }

    if(parsedHaptic[INDEX_IND] >= 0)
    {   
        hapticTime[INDEX_IND] = parsedHaptic[INDEX_IND] + currentTime;
        digitalWrite(PIN_INDEX_HAPTIC, HIGH);
    }

    if(parsedHaptic[MIDDLE_IND] >= 0)
    {   
        hapticTime[MIDDLE_IND] = parsedHaptic[MIDDLE_IND] + currentTime;
        digitalWrite(PIN_MIDDLE_HAPTIC, HIGH);
    }

    if(parsedHaptic[RING_IND] >= 0)
    {   
        hapticTime[RING_IND] = parsedHaptic[RING_IND] + currentTime;
        digitalWrite(PIN_RING_HAPTIC, HIGH);
    }

    if(parsedHaptic[PINKY_IND] >= 0)
    {   
        hapticTime[PINKY_IND] = parsedHaptic[PINKY_IND] + currentTime;
        digitalWrite(PIN_PINKY_HAPTIC, HIGH);
    }
}

void checkHaptics()
{
    unsigned long currentTime = millis();
    if(hapticTime[THUMB_IND] > 0 && currentTime > hapticTime[THUMB_IND])
    {
        hapticTime[THUMB_IND] = 0;
        digitalWrite(PIN_THUMB_HAPTIC, LOW);
    }

    if(hapticTime[INDEX_IND] > 0 && currentTime > hapticTime[INDEX_IND])
    {
        hapticTime[INDEX_IND] = 0;
        digitalWrite(PIN_INDEX_HAPTIC, LOW);
    }

    if(hapticTime[MIDDLE_IND] > 0 && currentTime > hapticTime[MIDDLE_IND])
    {
        hapticTime[MIDDLE_IND] = 0;
        digitalWrite(PIN_MIDDLE_HAPTIC, LOW);
    }

    if(hapticTime[RING_IND] > 0 && currentTime > hapticTime[RING_IND])
    {
        hapticTime[RING_IND] = 0;
        digitalWrite(PIN_RING_HAPTIC, LOW);
    }

    if(hapticTime[PINKY_IND] > 0 && currentTime > hapticTime[PINKY_IND])
    {
        hapticTime[PINKY_IND] = 0;
        digitalWrite(PIN_PINKY_HAPTIC, LOW);
    }
}


