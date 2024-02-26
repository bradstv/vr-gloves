char* encodeData(int* flexion, bool triggerButton, bool grab, bool pinch)
{
    static char stringToEncode[75];
    int trigger = (flexion[1] > ANALOG_MAX/2) ? (flexion[1] - ANALOG_MAX/2) * 2:0;
    #if USING_SPLAY
    sprintf(stringToEncode, "A%dB%dC%dD%dE%dP%d%s%s%s(AB)%d(BB)%d(CB)%d(DB)%d(EB)%d\n", 
    flexion[0], flexion[1], flexion[2], flexion[3], flexion[4], 
    trigger, triggerButton?"I":"", grab?"L":"", pinch?"M":"",
    flexion[5], flexion[6], flexion[7], flexion[8], flexion[9]);
    #else
    sprintf(stringToEncode, "A%dB%dC%dD%dE%dP%d%s%s%s\n", 
    flexion[0], flexion[1], flexion[2], flexion[3], flexion[4], 
    trigger, triggerButton?"I":"", grab?"L":"", pinch?"M":"");
    #endif
    return stringToEncode;
}

void decodeData(char* stringToDecode, int* parsedThermal, int* parsedServo, int* parsedHaptic)
{
    //Check if a Z command was received
    if (strchr(stringToDecode, 'Z') != NULL) 
    {
        bool toReturn = false;
        if (strstr(stringToDecode, "ClearData") != NULL) 
        {
            clearFlags();
            toReturn = true;
        }
        if (strstr(stringToDecode, "SaveInter") != NULL) 
        {
            saveIntermediate();
            toReturn = true;
        }
        if (strstr(stringToDecode, "SaveTravel") != NULL) 
        {
            saveTravel();
            toReturn = true;
        }

        if (toReturn)
        {
            return;
        }
    }

    parsedServo[0] = getArgument(stringToDecode, 'A'); //thumb
    parsedServo[1] = getArgument(stringToDecode, 'B'); //index
    parsedServo[2] = getArgument(stringToDecode, 'C'); //middle
    parsedServo[3] = getArgument(stringToDecode, 'D'); //ring
    parsedServo[4] = getArgument(stringToDecode, 'E'); //pinky

    parsedThermal[0] = getArgument(stringToDecode, 'I');

    parsedHaptic[0] = getArgument(stringToDecode, 'J'); //thumb
    parsedHaptic[1] = getArgument(stringToDecode, 'K'); //index
    parsedHaptic[2] = getArgument(stringToDecode, 'L'); //middle
    parsedHaptic[3] = getArgument(stringToDecode, 'M'); //ring
    parsedHaptic[4] = getArgument(stringToDecode, 'N'); //pinky
}

int getArgument(char* stringToDecode, char command)
{
    char* start = strchr(stringToDecode, command);
    if (start == NULL)
    {
        return -1;
    }
    else
    {
        return atoi(start + 1);
    } 
}

void mapServoLimits(int* parsedServo, float* scaledLimits)
{
    for(int i = 0; i < 5; i++)
    {
        #if FLIP_FORCE_FEEDBACK
        scaledLimits[i] = parsedServo[i] / 1000.0f * 180.0f;
        #else
        scaledLimits[i] = 180.0f - parsedServo[i] / 1000.0f * 180.0f;
        #endif
    }
}

void mapThermalValue(int* parsedThermal, int* thermalValues)
{
    int value = abs(parsedThermal[0]);
    thermalValues[0] = map(value, 0, 1000, 0, 255);

    if(parsedThermal[0] < 0)
    {
        thermalValues[0] = thermalValues[0] * -1; //convert back to negative if negative
    }
}