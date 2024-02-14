using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class Enums
    {
        #region enums
        public enum TimeAdjust
        {
            PlusOne,
            None,
            SubtractOne,
            Unassigned
        }
        public enum FileSource
        {
            NTExport,
            TSApp,
            TSWebsite
        }

        #endregion enums

    }
}
