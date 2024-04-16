bool grabGesture(int *flexion)
{
    return (flexion[PINKY_IND] + flexion[RING_IND] + flexion[MIDDLE_IND] + flexion[INDEX_IND]) / 4 <= GRAB_MIN ? 0:1;
}

bool pinchGesture(int *flexion)
{
    return (flexion[INDEX_IND] <= GRAB_MIN) || (flexion[THUMB_IND] <= GRAB_MIN) ? 0:1;
}

bool triggerGesture(int *flexion)
{
    return flexion[INDEX_IND] <= (GRAB_MIN) ? 0:1;
}
