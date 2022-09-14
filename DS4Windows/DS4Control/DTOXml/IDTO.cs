using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Control.DTOXml
{
    internal interface IDTO<T>
    {
        public void MapTo(T destination);
        public void MapFrom(T source);
    }
}
