using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityServiceApp
{
    public static class AppData
    {
        private static UtilityServiceDBEntities _db;

        public static UtilityServiceDBEntities db
        {
            get
            {
                if (_db == null) _db = new UtilityServiceDBEntities();
                return _db;
            }
        }
    }
}
