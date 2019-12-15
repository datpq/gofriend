using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace goFriend.Controls
{
    public class DphPin : Pin
    {
        public string IconUrl { get; set; }
        public bool Draggable { get; set; }
    }

    public class DphMap : Map
    {
        public List<DphPin> AllPins { get; set; }
    }
}
