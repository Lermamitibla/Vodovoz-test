using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vodovoz_test.Model
{
    public partial class Employees
    {
        public Employees( string firstName, string lastName, string patronymic, Gender gender, DateTime birthDate, int depId)
        {
            this.Departments1 = new HashSet<Departments>();
            this.firstname = firstName;
            this.lastname = lastName;
            this.patronymic = patronymic;
            this.gender = gender;
            this.dateOfBirth = birthDate;
            this.depID = depId;

        }

        public override string ToString()
        {
            return new StringBuilder(empid).Append(" ").Append(firstname).Append(" ").Append(lastname).ToString();
        }
    }
}
