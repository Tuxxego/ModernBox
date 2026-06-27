public static class EmployeeLibrary
{
    public static Employee CreateEmployee(string id)
    {
        return new Employee
        {
            id = id,
            name = char.ToUpper(id[0]) + id.Substring(1),
            pizzasPerSecond = UnityEngine.Random.Range(1f, 3f)
        };
    }
}