using System.Collections.Generic;
using UnityEngine;

public static class EventLibrary
{
    private static List<RandomEvent> events = new List<RandomEvent>()
    {
        new RandomEvent
        {
            id = "overload_oven",
            description = "One oven is overheating!",
            option1Title = "Shut it down",
            option1Description = "Production slows, but no damage.",
            option1Function = () => WorldTip.showNow("Oven cooled. Slowed down a bit.", true, "top", 3f),
            option2Title = "Push it harder",
            option2Description = "Increases output, but risk damage.",
            option2Function = () => WorldTip.showNow("Oven's going crazy fast!", true, "top", 3f)
        },
        new RandomEvent
        {
            id = "hire_friend",
            description = "A friend wants to join your pizza team!",
            option1Title = "Hire them",
            option1Description = "Adds a new employee.",
            option1Function = () => {
                var emp = EmployeeLibrary.CreateEmployee("friend_" + Random.Range(1000, 9999));
                PizzaSimulator.instance.CreateEmployeeWindow(emp);
                WorldTip.showNow("Friend hired!", true, "top", 3f);
            },
            option2Title = "Maybe later",
            option2Description = "No change.",
            option2Function = () => WorldTip.showNow("Friend told to wait.", true, "top", 3f)
        }
    };

    public static RandomEvent GetRandomEvent()
    {
        return events[UnityEngine.Random.Range(0, events.Count)];
    }
}
