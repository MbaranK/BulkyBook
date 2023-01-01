using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class StripeSettings
    {
        //appsettings.jsonda kullandığımız key değerlerinin aynı isimleriyle propertyleri oluşturmak zorundayız.
        public string StringKey { get; set; }
        public string PublishableKey { get; set; }
    }
}
