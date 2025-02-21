namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Manager : Employee
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected Manager() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Manager(int registrationNumber, string firstname, string lastname, Address address, string type, string carType)
            : base(registrationNumber, firstname, lastname, address, type)
        {
            CarType = carType;
        }

        public int Id { get; set; }
        public string CarType { get; set; }

    }
}