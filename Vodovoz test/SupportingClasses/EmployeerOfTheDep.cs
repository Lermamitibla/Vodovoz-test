using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz_test.Model;

namespace Vodovoz_test.SupportingClasses
{
    class EmployeerOfTheDep
    {
        public EmployeerOfTheDep(int empid, string firstname, string lastname, string patronymic, Gender gender, DateTime dateOfBirth, int depID, string deptName, int? managerId)
        {
            this.empid = empid;
            this.firstname = firstname;
            this.lastname = lastname;
            this.patronymic = patronymic;
            this.gender = gender;
            this.dateOfBirth = dateOfBirth;
            this.depID = depID;
            this.deptname = deptName;
            this.depManagerID = managerId;
            AddFullName();
            CheckIfManager();

            //отчество может быть нулл
        }

        public EmployeerOfTheDep() { }


        public int empid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string patronymic { get; set; }
        public Gender gender { get; set; }
        public System.DateTime dateOfBirth { get; set; }
        public int depID { get; set; }
        public string deptname { get; set; }
        public Nullable<int> depManagerID { get; set; }
        public string fullName { get; private set; }
        public bool isManager { get; private set; }
        private void AddFullName()
        {
            var strb = new StringBuilder();
            strb.Append(lastname);
            strb.Append(" ");
            strb.Append(firstname);
            strb.Append(" ");
            strb.Append(patronymic);
            fullName = strb.ToString();
        }
        private void CheckIfManager()
        {
            if (empid == depManagerID) isManager = true;
            isManager = false;
        }

    }
}
