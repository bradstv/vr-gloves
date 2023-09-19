void parseData(char* stringToDecode, int* parsedThermo, int* parsedServo, bool* parsedBuzzer)
{
    int index = 0;
    byte thermoIndex = 0;
    byte servoIndex = 0;
    byte buzzerIndex = 0;
    char* ptr = strtok(stringToDecode, ";");  // takes a list of delimiters
    while(ptr != NULL)
    {
        if(index == 0) //thermo
        {
            parsedThermo[thermoIndex] = atoi(ptr);
            thermoIndex++;
        }
        else if(index >= 1 && index <= 5) //servo fingers
        {
            parsedServo[servoIndex] = atoi(ptr);
            servoIndex++;
        }
        else if(index >= 6 && index <= 10) //buzzer fingers
        {
            parsedBuzzer[buzzerIndex] = atoi(ptr) > 0 ? true : false;
            buzzerIndex++;
        }
        index++;
        ptr = strtok(NULL, ";");  // takes a list of delimiters
    }
}

void mapServoLimits(int* parsedServo, float* scaledLimits)
{
    for(int i = 0; i < sizeof(parsedServo); i++)
    {
        scaledLimits[i] = parsedServo[i] / 1000.0f * 180.0f;
    }
}

void mapThermoValue(int* parsedThermo, int* thermoValues)
{
    int value = abs(parsedThermo[0]);
    thermoValues[0] = map(value, 0, 1000, 0, 255);

    if(parsedThermo[0] < 0)
    {
        thermoValues[0] = thermoValues[0] * -1; //convert back to negative if negative
    }
}