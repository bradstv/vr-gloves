using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Calculator : MonoBehaviour
{
    public TextMeshPro screenOutput;
    private float num = 0f;
    private bool showingAnswer = false;
    private string currentString = string.Empty;
    private string operation = string.Empty;

    void Start()
    {
        updateScreen("0");
    }

    public void onButtonPressed(string button)
    {
        if (button == "AC" || button == "C")
        {
            updateScreen("0");
            num = 0f;
            operation = string.Empty;
            showingAnswer = false;
            return;
        }

        float parsedNum;
        if (float.TryParse(button, out parsedNum))
        {
            if (screenOutput.text == "0" || screenOutput.text == "ERROR" || showingAnswer)
                currentString = parsedNum.ToString();
            else
                currentString += parsedNum.ToString();

            updateScreen(currentString);
        }

        float value;
        if (!float.TryParse(currentString, out value))
        {
            updateScreen("ERROR");
            return;
        }

        if (button == ".")
        {
            if (screenOutput.text.Contains("."))
                return;

            if (screenOutput.text != "0" || screenOutput.text != "ERROR")
                addToScreen(button);
        }

        if (isValidOperation(button))
        {
            num = float.Parse(currentString);
            operation = button;
            currentString = String.Empty;
        }

        if (button == "%")
        {
            float answer = value * 0.01f;
            updateScreen(answer.ToString());
        }

        if (button == "Sqrt")
        {
            float answer = Mathf.Sqrt(value);
            updateScreen(answer.ToString());
        }

        if (button == "=")
        {
            equalsOperation();
            showingAnswer = true;
        }
        else
        {
            showingAnswer = false;
        }
    }

    private void equalsOperation()
    {
        float value;
        float answer = 0f;

        if(!float.TryParse(currentString, out value) || !isValidOperation(operation))
        {
            updateScreen("ERROR");
            return;
        }

        if(operation == "+")
        {
            answer = num + value;
            updateScreen(answer.ToString());
        }
        
        if(operation == "-")
        {
            answer = num - value;
            updateScreen(answer.ToString());
        }
        
        if(operation == "*")
        {
            answer = num * value;
            updateScreen(answer.ToString());
        }
        
        if (operation == "/")
        {
            answer = num / value;
            updateScreen(answer.ToString());
        }
    }
    private void updateScreen(string value)
    {
        screenOutput.text = value;
        currentString = value;
    }

    private void addToScreen(string value)
    {
        screenOutput.text += value;
        currentString = screenOutput.text;
    }

    private bool isValidOperation(string operation)
    {
        switch (operation)
        {
            case "+":
                return true;
            case "-":
                return true;
            case "*":
                return true;
            case "/":
                return true;
            default:
                return false;
        }
    }
}
