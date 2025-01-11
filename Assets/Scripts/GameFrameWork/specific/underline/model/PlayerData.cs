using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData 
{
    public string id;
    public GENDER gender = GENDER.MALE;
    public ROLE role = ROLE.PALADIN;
    public RACE race = RACE.BLOOODELF;
}
public enum ROLE
{
    WARRIOR,
    PALADIN,
    HUNTER,
    ROGUE,
    PRISST,
    DWATHKNIGHT,
    SHAMAM,
    MAGE,
    WARLOCK,
    DRVIO
}
public enum RACE
{
    HUMAN,
    DWARF,
    GNOME,
    NIGHTELF,
    ORC,
    TROLL,
    FORSAKEN,
    TAUREN,
    BLOOODELF
}
public enum GENDER
{
    MALE,
    FEMALE
}
