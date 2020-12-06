using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Vodovoz_test.SupportingClasses
{
    class EnumBindingSourceExtension : MarkupExtension
    {
        public Type enumType { get; private set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {

            return Enum.GetValues(enumType);
        }

        public EnumBindingSourceExtension(Type EnumType)
        {
            if (EnumType is null || !EnumType.IsEnum) throw new ArgumentException("EnumType is null or not Enum");

            enumType = EnumType;
        }
    }
}
