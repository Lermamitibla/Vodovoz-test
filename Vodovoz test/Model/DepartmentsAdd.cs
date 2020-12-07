using System.Collections.Generic;
using System.Text;

namespace Vodovoz_test.Model
{
    public partial class Departments
    {
        public Departments(string depName)
        {
            this.depName = depName;
            this.Employees = new HashSet<Employees>();
        }

        public override string ToString()
        {
            return new StringBuilder(depID).Append(" ").Append(depName).ToString();
        }
    }
}
