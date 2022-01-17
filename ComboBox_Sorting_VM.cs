using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Проекты_8_9_Классы
{
    class ComboBox_Sorting_VM
    {
        public List<string> SortingCollection { get; set; }

        public ComboBox_Sorting_VM()
        {
            SortingCollection = new List<string>()
            {
                "По алфавиту",
                "Против алфавитного порядка",
                "По возрастанию кол-ва баллов",
                "По убыванию кол-ва баллов"
            };
        }
    }
}
