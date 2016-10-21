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
            foreach (CFindImageIOD iod in source)
            {
                //if (!uniques.Contains(iod)) uniques.Add(iod);
                if (!uniques.Select(u => u.SOPInstanceUID).ToList().Contains(iod.SOPInstanceUID)) uniques.Add(iod);
            }
            return uniques;
        }
    }
}
