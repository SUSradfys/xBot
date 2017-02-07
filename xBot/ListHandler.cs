using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvilDICOM.Network.DIMSE.IOD;

namespace xBot
{
    class ListHandler
    {
        public static List<CFindImageIOD> Unique(List<CFindImageIOD> source)
        {
            List<CFindImageIOD> uniques = new List<CFindImageIOD>();
            uniques = source
                .GroupBy(u => u.SOPInstanceUID)
                .Select(g => g.First())
                .ToList();

            return uniques;
        }
    }
}
