using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenderPayGap.Core.Classes
{
    [Serializable]
    public struct SelectedItem
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}
