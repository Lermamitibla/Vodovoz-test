using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vodovoz_test.SupportingClasses
{
    class DepWithManagerName
    {
        public int depID { get; set; }
        public string depName { get; set; }
        public Nullable<int> managerID { get; set; }
        public string managerfullname { get; set; }


        public override string ToString()
        {
            return depID.ToString() + " " + depName + " " + managerfullname;
        }
    }
}
