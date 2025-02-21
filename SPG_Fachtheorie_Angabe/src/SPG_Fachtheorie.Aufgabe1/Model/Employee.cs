using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public abstract class Employee
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected Employee() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Employee(int registrationNumber, string firstname, string lastname, Address address, string type)
        {
            RegistrationNumber = registrationNumber;
            Firstname = firstname;
            Lastname = lastname;
            Address = address;
            Type = type;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RegistrationNumber { get; set; }
        [MaxLength(255)]
        public string Firstname { get; set; }
        [MaxLength(255)]
        public string Lastname { get; set; }
        public Address? Address { get; set; }
        public string Type { get; set; } = null!;
    }
}   